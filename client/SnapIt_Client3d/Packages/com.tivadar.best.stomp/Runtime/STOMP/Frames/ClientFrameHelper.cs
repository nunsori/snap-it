using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.Logger;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.HTTP.Shared.PlatformSupport.Text;

using System;
using System.Collections.Generic;
using System.Text;

namespace Best.STOMP.Frames
{
    internal static class ClientFrameHelper
    {
        // https://stomp.github.io/stomp-specification-1.2.html#Connecting
        public static BufferSegment CreateSTOMPFrame(ConnectParameters parameters, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"STOMP\naccept-version:1.2\nhost:{parameters.VirtualHost ?? parameters.Host}\nheart-beat:{(int)parameters.PreferredOutgoingHeartbeats.TotalMilliseconds},{(int)parameters.PreferredIncomingHeartbeats.TotalMilliseconds}\n");
            AddAdditionalHeaders(sb, parameters.Headers, false);
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#DISCONNECT
        public static BufferSegment CreateDisconnectFrame(long receiptId, Dictionary<string, string> additionalHeaders, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"DISCONNECT\nreceipt:{receiptId}\n");
            AddAdditionalHeaders(sb, additionalHeaders, true);
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#SUBSCRIBE
        public static BufferSegment CreateSubscribeFrame(long receiptId, Subscription subscription, Dictionary<string, string> headers, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"SUBSCRIBE\nid:{subscription.Id}\ndestination:{subscription.Destination}\n");
            switch(subscription.AcknowledgmentMode)
            {
                // This is the default, we don't have to send it
                case AcknowledgmentModes.Auto: break;
                case AcknowledgmentModes.Client: sb.Append("ack:client\n"); break;
                case AcknowledgmentModes.ClientIndividual: sb.Append("ack:client-individual\n"); break;
            }

            if (receiptId > 0)
                sb.Append($"receipt:{receiptId}\n");

            AddAdditionalHeaders(sb, headers, true);
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#UNSUBSCRIBE
        public static BufferSegment CreateUnsubscribeFrame(long receiptId, Subscription subscription, Dictionary<string, string> headers, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"UNSUBSCRIBE\nid:{subscription.Id}\n");

            if (receiptId > 0)
                sb.Append($"receipt:{receiptId}\n");

            AddAdditionalHeaders(sb, headers, true);
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);

        }

        // https://stomp.github.io/stomp-specification-1.2.html#SEND
        public static BufferSegment CreateSendFrame(long receiptId, string destination, string contentType, Transaction transaction, Dictionary<string, string> headers, BufferSegment content, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"SEND\ndestination:{destination}\ncontent-length:{content.Count}\n");
            if (contentType != null)
                sb.Append($"content-type:{contentType}\n");
            if (receiptId > 0)
                sb.Append($"receipt:{receiptId}\n");
            if (transaction != null)
                sb.Append($"transaction:{transaction.Id}\n");
            AddAdditionalHeaders(sb, headers, true);
            sb.Append('\n');

            return ToBuffer(sb, content, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#ACK
        public static BufferSegment CreateACKFrame(string messageId, Transaction transaction, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"ACK\nid:{messageId}\n");
            if (transaction != null)
                sb.Append($"transaction:{transaction.Id}\n");
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#NACK
        public static BufferSegment CreateNACKFrame(string messageId, Transaction transaction, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"NACK\nid:{messageId}\n");
            if (transaction != null)
                sb.Append($"transaction:{transaction.Id}\n");
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#BEGIN
        public static BufferSegment CreateBeginFrame(Transaction transaction, long receiptId, Dictionary<string, string> headers, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"BEGIN\ntransaction:{transaction.Id}\n");
            if (receiptId > 0)
                sb.Append($"receipt:{receiptId}\n");
            AddAdditionalHeaders(sb, headers, true);
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#COMMIT
        public static BufferSegment CreateCommitFrame(Transaction transaction, long receiptId, Dictionary<string, string> headers, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"COMMIT\ntransaction:{transaction.Id}\n");
            if (receiptId > 0)
                sb.Append($"receipt:{receiptId}\n");
            AddAdditionalHeaders(sb, headers, true);
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        // https://stomp.github.io/stomp-specification-1.2.html#ABORT
        public static BufferSegment CreateAbortFrame(Transaction transaction, long receiptId, Dictionary<string, string> headers, LoggingContext context)
        {
            var sb = StringBuilderPool.Get(2);
            sb.Append($"ABORT\ntransaction:{transaction.Id}\n");
            if (receiptId > 0)
                sb.Append($"receipt:{receiptId}\n");
            AddAdditionalHeaders(sb, headers, true);
            sb.Append('\n');

            return ToBuffer(sb, BufferSegment.Empty, context);
        }

        private static void AddAdditionalHeaders(StringBuilder sb, Dictionary<string, string> headers, bool escapeHeader)
        {
            if (headers != null)
                foreach (var header in headers)
                    sb.Append($"{EscapeString(header.Key, escapeHeader)}:{EscapeString(header.Value, escapeHeader)}\n");
        }

        // https://stomp.github.io/stomp-specification-1.2.html#Value_Encoding
        private static string EscapeString(string str, bool escape)
        {
            if (!escape)
                return str;

            /*
             *  \r (octet 92 and 114) translates to carriage return (octet 13)
                \n (octet 92 and 110) translates to line feed (octet 10)
                \c (octet 92 and 99) translates to : (octet 58)
                \\ (octet 92 and 92) translates to \ (octet 92)
             * */
            return str.Replace("\\", "\\\\", StringComparison.OrdinalIgnoreCase)
                .Replace(":", "\\c", StringComparison.OrdinalIgnoreCase)
                .Replace("\n", "\\n", StringComparison.OrdinalIgnoreCase)
                .Replace("\r", "\\r", StringComparison.OrdinalIgnoreCase);
        }

        private static BufferSegment ToBuffer(StringBuilder sb, BufferSegment content, LoggingContext context)
        {
            var frameStr = StringBuilderPool.ReleaseAndGrab(sb);

            // convert to byte[], however, have to allocate +1 bytes for the leading NULL
            var count = Encoding.UTF8.GetByteCount(frameStr);
            var buffer = BufferPool.Get(count + content.Count + 1, true, context);
            Encoding.UTF8.GetBytes(frameStr, 0, frameStr.Length, buffer, 0);

            if (content.Data != null)
                Array.Copy(content.Data, content.Offset, buffer, count, content.Count);

            // set the leading NULL
            buffer[count + content.Count] = 0x00;

            return buffer.AsBuffer(count + content.Count + 1);
        }
    }
}
