using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Common.Markup;
using Gallio.Common.Validation;
using Gallio.Model.Schema;
using Gallio.Common.Messaging;

namespace Gallio.Model.Messages.Execution
{
    /// <summary>
    /// Notifies that text has been written to a test step log stream.
    /// </summary>
    [Serializable]
    public class TestStepLogStreamWriteMessage : Message
    {
        /// <summary>
        /// Gets or sets the id of the test step, not null.
        /// </summary>
        public string StepId { get; set; }
        
        /// <summary>
        /// Gets or sets the stream name, not null.
        /// </summary>
        public string StreamName { get; set; }

        /// <summary>
        /// Gets or sets the text, not null.
        /// </summary>
        public string Text { get; set; }

        /// <inheritdoc />
        public override void Validate()
        {
            ValidationUtils.ValidateNotNull("stepId", StepId);
            ValidationUtils.ValidateNotNull("streamName", StreamName);
            ValidationUtils.ValidateNotNull("text", Text);
        }
    }
}