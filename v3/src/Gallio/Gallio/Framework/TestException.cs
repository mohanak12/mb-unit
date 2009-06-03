// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Runtime.Serialization;
using Gallio.Model;

namespace Gallio.Framework
{
    /// <summary>
    /// <para>
    /// This exception type is an abstract base class for exceptions that are used to
    /// explicitly signal the outcome of a test.
    /// </para>
    /// <para>
    /// The test framework uses the value of the <see cref="Outcome" /> property to set the test
    /// result instead of applying the standard behavior for unexpected exceptions.
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class TestException : Exception
    {
        private const string HasNonDefaultMessageKey = "HasNonDefaultMessage";
        private bool hasNonDefaultMessageKey;

        /// <summary>
        /// Creates an exception.
        /// </summary>
        protected TestException()
        {
        }

        /// <summary>
        /// Creates an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        protected TestException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Creates an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        protected TestException(string message, Exception innerException)
            : base(message, innerException)
        {
            hasNonDefaultMessageKey = !string.IsNullOrEmpty(message);
        }

        /// <summary>
        /// Creates an exception from serialization info.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected TestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            hasNonDefaultMessageKey = info.GetBoolean(HasNonDefaultMessageKey);
        }

        /// <summary>
        /// Returns true if the test exception has a non-default message
        /// (the message parameter was not null in the constructor arguments).
        /// </summary>
        public bool HasNonDefaultMessage
        {
            get { return hasNonDefaultMessageKey; }
            protected set { hasNonDefaultMessageKey = value; }
        }

        /// <summary>
        /// Gets the outcome of the test.
        /// </summary>
        public abstract TestOutcome Outcome { get; }

        /// <summary>
        /// Returns true if the outcome and message (if any) should be used but the exception
        /// stack trace should not be logged.
        /// </summary>
        public virtual bool ExcludeStackTrace
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(HasNonDefaultMessageKey, hasNonDefaultMessageKey);
        }
    }
}