using Best.HTTP.Shared;

using System;
using System.Collections.Generic;
using System.Threading;

namespace Best.STOMP
{
    internal sealed class ReceiptManager
    {
        private Client _session;
        private long _nextReceiptId;

        private List<KeyValuePair<long, Action<Client, IncomingFrame>>> _ackCallbacks;

        public ReceiptManager(Client session)
            => this._session = session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiptId">Receipt Id associated with the ackCallback</param>
        /// <param name="ackCallback"></param>
        /// <returns></returns>
        public long SubsribeTo(Action<Client, IncomingFrame> ackCallback)
        {
            long receiptId = Interlocked.Increment(ref this._nextReceiptId);
            this._ackCallbacks = this._ackCallbacks ?? new List<KeyValuePair<long, Action<Client, IncomingFrame>>>();
            this._ackCallbacks.Add(new KeyValuePair<long, Action<Client, IncomingFrame>>(receiptId, ackCallback));

            return receiptId;
        }

        public void OnReceipt(long receiptId, IncomingFrame frame)
        {
            var idx = this._ackCallbacks.FindIndex(kvp => kvp.Key == receiptId);

            if (idx > -1)
            {
                try
                {
                    this._ackCallbacks[idx].Value?.Invoke(this._session, frame);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception(nameof(ReceiptManager), $"callback", ex, this._session.Context);
                }

                this._ackCallbacks.RemoveAt(idx);
            }
        }
    }
}
