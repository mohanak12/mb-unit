using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Gallio.Common;
using Gallio.Common.Policies;
using Gallio.Common.Remoting;

namespace Gallio.Common.Messaging
{
    /// <summary>
    /// Wraps a <see cref="IMessageSink"/> and queues messages so that messages are
    /// published asynchronously.
    /// </summary>
    [Serializable]
    public class QueuedMessageSink : IMessageSink, IDisposable
    {
        private readonly IMessageSink messageSink;
        private readonly Queue<Message> queue;
        private readonly Action asyncPublishLoop;

        private volatile IAsyncResult asyncResult;

        /// <summary>
        /// Creates a queued message sink.
        /// </summary>
        /// <param name="messageSink">The message sink to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="messageSink"/> is null.</exception>
        public QueuedMessageSink(IMessageSink messageSink)
        {
            if (messageSink == null)
                throw new ArgumentNullException("messageSink");

            this.messageSink = messageSink;

            asyncPublishLoop = AsyncPublishLoop;
            queue = new Queue<Message>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                Flush();
            }
            catch (Exception ex)
            {
                UnhandledExceptionPolicy.Report("An unhandled exception occurred while flushing a queued message sink.", ex);
            }
        }

        /// <inheritdoc />
        public void Publish(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            lock (queue)
            {
                queue.Enqueue(message);

                if (asyncResult == null)
                {
                    asyncResult = asyncPublishLoop.BeginInvoke(null, null);
                }
            }
        }

        /// <summary>
        /// Flushes the queue.
        /// </summary>
        public void Flush()
        {
            IAsyncResult oldAsyncResult;
            lock (queue)
            {
                oldAsyncResult = asyncResult;
            }

            if (oldAsyncResult != null && ! oldAsyncResult.IsCompleted)
                oldAsyncResult.AsyncWaitHandle.WaitOne();
        }

        private void AsyncPublishLoop()
        {
            for (; ; )
            {
                Message message;
                lock (queue)
                {
                    if (queue.Count == 0)
                    {
                        asyncResult = null;
                        return;
                    }

                    message = queue.Dequeue();
                }

                try
                {
                    messageSink.Publish(message);
                }
                catch (Exception ex)
                {
                    UnhandledExceptionPolicy.Report(
                        "An unhandled exception occurred while asynchronously publishing a queued message.", ex);
                }
            }
        }
    }
}
