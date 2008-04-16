using System;
using Gallio.Runner.Reports;

namespace Gallio.Runner.Events
{
    /// <summary>
    /// Arguments for an event raised to indicate that a test package has finished loading.
    /// </summary>
    public sealed class LoadFinishedEventArgs : OperationFinishedEventArgs
    {
        private readonly Report report;

        /// <summary>
        /// Initializes the event arguments.
        /// </summary>
        /// <param name="success">True if the test package was loaded successfully</param>
        /// <param name="report">The report, including test package data on success</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="report"/> is null</exception>
        public LoadFinishedEventArgs(bool success, Report report)
            : base(success)
        {
            if (report == null)
                throw new ArgumentNullException("report");

            this.report = report;
        }

        /// <summary>
        /// Gets the report, including test package data on success.
        /// </summary>
        public Report Report
        {
            get { return report; }
        }
    }
}