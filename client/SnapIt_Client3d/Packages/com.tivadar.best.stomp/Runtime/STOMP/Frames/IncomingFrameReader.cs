using Best.HTTP.Shared;
using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.HTTP.Shared.Streams;

using System;
using System.Collections.Generic;

namespace Best.STOMP.Frames
{
    internal enum ClientFrameTypes
    {
        STOMP, // CONNECT
        Send,
        Subscribe,
        Unsubscribe,
        ACK,
        NACK,
        Begin,
        Commit,
        Abort,
        Disconnect
    }

    internal enum PeekableReadState
    {
        StatusLine,
        Headers,
        WaitForContentSent, // when received a 100-continue
        PrepareForContent,
        ContentSetup,
        Content,
        Finished
    }

    internal sealed class IncomingFrameFactory
    {
        Client _client;
        IncomingFrame _currentFrame = null;

        public enum PeekableReadState
        {
            Command,
            Headers,
            ContentUnknownLength,
            ContentKnownLength,
            ReadLeadingNULL,
        }
        private PeekableReadState _readState;

        private PeekableIncomingSegmentStream _peekable;

        public IncomingFrameFactory(Client client, PeekableIncomingSegmentStream peekable)
        {
            this._client = client;
            this._peekable = peekable;
            this._readState = PeekableReadState.Command;
        }

        // return true if it could advance. If it can complete a frame, frame will be set to a non-null value.
        public bool TryAdvance(out IncomingFrame frame)
        {
            frame = null;

            switch (this._readState)
            {
                case PeekableReadState.Command:
                    if (!IsNewLinePresent())
                        return false;

                    var typeStr = ReadUntil(0x0A, 0x0A); // \n

                    if (HTTPManager.Logger.IsDiagnostic)
                        HTTPManager.Logger.Verbose(nameof(IncomingFrameFactory), $"Command read: \"{typeStr}\"", this._client.Context);

                    if (Enum.TryParse<ServerFrameTypes>(typeStr, true, out var type))
                    {
                        _currentFrame = new IncomingFrame();
                        _currentFrame.Type = type;
                    }
                    else
                    {
                        // and empty '\n' is a ping from the server: https://stomp.github.io/stomp-specification-1.2.html#Heart-beating
                        frame = new IncomingFrame() { Type = ServerFrameTypes.Ping };
                        return true;
                    }

                    this._readState = PeekableReadState.Headers;

                    break;

                case PeekableReadState.Headers:
                    if (!IsNewLinePresent())
                        return false;
                    do
                    {
                        string headerName = ReadUntil((byte)':', 0x0A);

                        if (headerName == string.Empty)
                        {
                            this._readState = this._currentFrame.ContentLength == -1 ? PeekableReadState.ContentUnknownLength : PeekableReadState.ContentKnownLength;
                            return true;
                        }

                        string value = ReadUntil(0x0A, 0x0A);

                        if (HTTPManager.Logger.IsDiagnostic)
                            HTTPManager.Logger.Verbose(nameof(IncomingFrameFactory), $"Header read: \"{headerName}\":\"{value}\"", this._client.Context);

                        if (this._currentFrame.Headers == null)
                            this._currentFrame.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        if (!this._currentFrame.Headers.ContainsKey(headerName))
                            this._currentFrame.Headers.Add(headerName, value);

                        if (headerName.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                            this._currentFrame.ContentLength = int.Parse(value);

                        if (headerName.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                            this._currentFrame.ContentType = value;

                        if (headerName.Equals("receipt", StringComparison.OrdinalIgnoreCase))
                            this._currentFrame.Receipt = value;

                    } while (IsNewLinePresent());
                    break;

                // Read until the first NULL octet
                case PeekableReadState.ContentUnknownLength:
                    {
                        int pos = FindNULL();
                        // -1: found end of stream before finding NULL
                        //  0: NULL is the first byte - no content
                        // >0: content with NULL terminator

                        // Not enough data
                        if (pos == -1)
                            return false;

                        // No content
                        if (pos > 0)
                        {
                            var buffer = BufferPool.Get(pos, true, this._client.Context);
                            this._peekable.Read(buffer, 0, pos);

                            this._currentFrame.Body = buffer.AsBuffer(pos);
                            return true;
                        }

                        frame = _currentFrame;
                        _currentFrame = null;

                        this._readState = PeekableReadState.ReadLeadingNULL;
                        break;
                    }

                // Read BodyLength octets + NULL octet
                case PeekableReadState.ContentKnownLength:
                    {
                        if (this._peekable.Length < this._currentFrame.ContentLength)
                            return false;

                        if (this._currentFrame.ContentLength != 0)
                        {
                            var buffer = BufferPool.Get(this._currentFrame.ContentLength, true, this._client.Context);
                            this._peekable.Read(buffer, 0, this._currentFrame.ContentLength);

                            this._currentFrame.Body = buffer.AsBuffer(this._currentFrame.ContentLength);
                        }

                        frame = this._currentFrame;
                        this._currentFrame = null;
                        this._readState = PeekableReadState.ReadLeadingNULL;
                        break;
                    }

                case PeekableReadState.ReadLeadingNULL:
                    if (this._peekable.Length < 1)
                        return false;

                    // read leading NULL
                    this._peekable.ReadByte();
                    this._readState = PeekableReadState.Command;
                    break;
            }

            return true;
        }

        int FindNULL()
        {
            this._peekable.BeginPeek();

            int count = 0;
            int nextByte = this._peekable.PeekByte();
            while (nextByte > 0) {
                count++;
                nextByte = this._peekable.PeekByte();
            }

            return nextByte == 0 ? count : -1;
        }

        string ReadUntil(byte blocker1, byte blocker2)
        {
            byte[] readBuf = BufferPool.Get(1024, true);
            try
            {
                int bufpos = 0;

                int ch = this._peekable.ReadByte();
                while (ch != blocker1 && ch != blocker2 && ch != -1)
                {
                    //make buffer larger if too short
                    if (readBuf.Length <= bufpos)
                        BufferPool.Resize(ref readBuf, readBuf.Length * 2, true, true);

                    // https://stomp.github.io/stomp-specification-1.2.html#Value_Encoding
                    //  \r (octet 92 and 114) translates to carriage return (octet 13)
                    //  \n (octet 92 and 110) translates to line feed (octet 10)
                    //  \c (octet 92 and 99) translates to : (octet 58)
                    //  \\ (octet 92 and 92) translates to \ (octet 92)

                    if (ch == 92)
                    {
                        this._peekable.BeginPeek();
                        int nextCh = this._peekable.PeekByte();

                        switch(nextCh)
                        {
                            case 114: // \r
                                ch = 13;
                                this._peekable.ReadByte();
                                break;

                            case 110:
                                ch = 10;
                                this._peekable.ReadByte();
                                break;

                            case 99:
                                ch = 58;
                                this._peekable.ReadByte(); 
                                break;

                            case 92:
                                ch = 92;
                                this._peekable.ReadByte();
                                break;
                        }
                    }

                    readBuf[bufpos++] = (byte)ch;
                    ch = this._peekable.ReadByte();
                }

                while (bufpos > 0 && readBuf[bufpos - 1] == '\r')
                    bufpos--;

                return System.Text.Encoding.UTF8.GetString(readBuf, 0, bufpos);
            }
            finally
            {
                BufferPool.Release(readBuf);
            }
        }

        bool IsNewLinePresent()
        {
            this._peekable.BeginPeek();

            int nextByte = this._peekable.PeekByte();
            while (nextByte >= 0 && nextByte != 0x0A)
                nextByte = this._peekable.PeekByte();

            return nextByte == 0x0A;
        }
    }
}
