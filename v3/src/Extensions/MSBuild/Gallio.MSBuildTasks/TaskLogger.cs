// Copyright 2005-2008 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan De Halleux, Jamie Cansdale
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Castle.Core.Logging;
using ILogger=Castle.Core.Logging.ILogger;

namespace Gallio.MSBuildTasks
{
    /// <exclude />
    /// <summary>
    /// Logs messages to a <see cref="TaskLoggingHelper" /> instance.
    /// </summary>
    internal class TaskLogger : LevelFilteredLogger
    {
        private readonly TaskLoggingHelper taskLoggingHelper;
        
        public TaskLogger(TaskLoggingHelper taskLoggingHelper)
        {
            if (taskLoggingHelper == null)
                throw new ArgumentNullException("taskLoggingHelper");
            this.taskLoggingHelper = taskLoggingHelper;
            Level = LoggerLevel.Debug;
        }

        protected override void Log(LoggerLevel level, string name, string message, Exception exception)
        {
            if (exception != null)
                message += "\n" + exception;

            switch (level)
            {
                case LoggerLevel.Fatal:
                case LoggerLevel.Error:
                    taskLoggingHelper.LogError(message);
                    break;

                case LoggerLevel.Warn:
                    taskLoggingHelper.LogWarning(message);
                    break;

                case LoggerLevel.Info:
                    taskLoggingHelper.LogMessage(MessageImportance.High, message);
                    break;

                case LoggerLevel.Debug:
                    taskLoggingHelper.LogMessage(MessageImportance.Low, message);
                    break;
            }
        }

        public override ILogger CreateChildLogger(string name)
        {
            return this;
        }
    }
}
