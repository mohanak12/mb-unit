using System;

namespace Gallio.Model.Execution
{
    /// <summary>
    /// A log writer that sends messages to a <see cref="ITestListener" />.
    /// </summary>
    public class ObservableTestLogWriter : BaseTestLogWriter
    {
        private ITestListener listener;
        private readonly string stepId;

        /// <summary>
        /// Creates a log writer.
        /// </summary>
        /// <param name="listener">The event listener</param>
        /// <param name="stepId">The step id</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="listener"/> or
        /// <paramref name="stepId"/> is null</exception>
        public ObservableTestLogWriter(ITestListener listener, string stepId)
        {
            if (listener == null)
                throw new ArgumentNullException(@"listener");
            if (stepId == null)
                throw new ArgumentNullException(@"stepId");

            this.listener = listener;
            this.stepId = stepId;
        }

        /// <inheritdoc />
        protected override void CloseImpl()
        {
            listener = null;
        } 

        /// <inheritdoc />
        protected override void AttachTextImpl(string attachmentName, string contentType, string text)
        {
            listener.NotifyLogEvent(LogEventArgs.CreateAttachTextEvent(stepId, attachmentName, contentType, text));
        }

        /// <inheritdoc />
        protected override void AttachBytesImpl(string attachmentName, string contentType, byte[] bytes)
        {
            listener.NotifyLogEvent(LogEventArgs.CreateAttachBytesEvent(stepId, attachmentName, contentType, bytes));
        }

        /// <inheritdoc />
        protected override void WriteImpl(string streamName, string text)
        {
            listener.NotifyLogEvent(LogEventArgs.CreateWriteEvent(stepId, streamName, text));
        }

        /// <inheritdoc />
        protected override void EmbedImpl(string streamName, string attachmentName)
        {
            listener.NotifyLogEvent(LogEventArgs.CreateEmbedEvent(stepId, streamName, attachmentName));
        }

        /// <inheritdoc />
        protected override void BeginSectionImpl(string streamName, string sectionName)
        {
            listener.NotifyLogEvent(LogEventArgs.CreateBeginSectionEvent(stepId, streamName, sectionName));
        }

        /// <inheritdoc />
        protected override void EndSectionImpl(string streamName)
        {
            listener.NotifyLogEvent(LogEventArgs.CreateEndSectionEvent(stepId, streamName));
        }
    }
}
