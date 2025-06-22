using Best.HTTP.Shared;
using Best.STOMP.Frames;

using System;
using System.Threading.Tasks;

namespace Best.STOMP
{
    /// <summary>
    /// Represents a transaction for message operations within a STOMP session.
    /// Transactions allow multiple operations to be grouped together and committed or aborted as a single unit.
    /// </summary>
    public sealed class Transaction
    {
        /// <summary>
        /// Gets the unique identifier of the transaction.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the STOMP client session associated with this transaction.
        /// </summary>
        public Client Session { get; private set; }

        internal Transaction(int id, Client session)
        {
            this.Id = id;
            this.Session = session;
        }

        /// <summary>
        /// Begins the transaction and optionally executes a callback upon acknowledgment.
        /// </summary>
        /// <param name="ackCallback">An optional callback to be invoked upon acknowledgment of the transaction start.</param>
        public void Begin(Action<Client, Transaction> ackCallback)
        {
            long receiptId = 0;
            if (ackCallback != null)
            {
                receiptId = this.Session.ReceiptManager.SubsribeTo((session, frame) =>
                {
                    try
                    {
                        ackCallback.Invoke(session, this);
                    }
                    catch(Exception ex)
                    {
                        HTTPManager.Logger.Exception(nameof(Transaction), nameof(Begin), ex, this.Session.Context);
                    }
                });
            }

            var frameData = ClientFrameHelper.CreateBeginFrame(this, receiptId, null, this.Session.Context);
            this.Session.SendFrame(frameData);
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns>A task representing the asynchronous transaction start operation.</returns>
        public Task<Transaction> BeginAsync()
        {
            var tcs = new TaskCompletionSource<Transaction>();
            this.Begin((client, transaction) => tcs.TrySetResult(transaction));
            return tcs.Task;
        }

        /// <summary>
        /// Commits the transaction, applying all operations performed within the transaction scope, and optionally executes a callback upon acknowledgment.
        /// </summary>
        /// <param name="ackCallback">An optional callback to be invoked upon acknowledgment of the transaction commit.</param>
        public void Commit(Action<Client, Transaction> ackCallback)
        {
            long receiptId = 0;
            if (ackCallback != null)
            {
                receiptId = this.Session.ReceiptManager.SubsribeTo((session, frame) =>
                {
                    try
                    {
                        ackCallback.Invoke(session, this);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception(nameof(Transaction), nameof(Commit), ex, this.Session.Context);
                    }
                });
            }

            var frameData = ClientFrameHelper.CreateCommitFrame(this, receiptId, null, this.Session.Context);
            this.Session.SendFrame(frameData);
        }

        /// <summary>
        /// Commits the transaction, applying all operations performed within the transaction scope, and optionally executes a callback upon acknowledgment.
        /// </summary>
        /// <returns>A task representing the asynchronous transaction commit operation.</returns>
        public Task<Transaction> CommitAsync()
        {
            var tcs = new TaskCompletionSource<Transaction>();
            this.Commit((client, transaction) => tcs.TrySetResult(transaction));
            return tcs.Task;
        }

        /// <summary>
        /// Aborts the transaction, discarding all operations performed within the transaction scope, and optionally executes a callback upon acknowledgment.
        /// </summary>
        /// <param name="ackCallback">An optional callback to be invoked upon acknowledgment of the transaction abort.</param>
        public void Abort(Action<Client, Transaction> ackCallback)
        {
            long receiptId = 0;
            if (ackCallback != null)
            {
                receiptId = this.Session.ReceiptManager.SubsribeTo((session, frame) =>
                {
                    try
                    {
                        ackCallback.Invoke(session, this);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception(nameof(Transaction), nameof(Abort), ex, this.Session.Context);
                    }
                });
            }

            var frameData = ClientFrameHelper.CreateAbortFrame(this, receiptId, null, this.Session.Context);
            this.Session.SendFrame(frameData);
        }

        /// <summary>
        /// Aborts the transaction, discarding all operations performed within the transaction scope, and optionally executes a callback upon acknowledgment.
        /// </summary>
        /// <returns>A task representing the asynchronous transaction abort operation.</returns>
        public Task<Transaction> AbortAsync()
        {
            var tcs = new TaskCompletionSource<Transaction>();
            this.Abort((client, transaction) => tcs.TrySetResult(transaction));
            return tcs.Task;
        }

        public override string ToString()
            => $"[Transaction {this.Id}]";
    }
}
