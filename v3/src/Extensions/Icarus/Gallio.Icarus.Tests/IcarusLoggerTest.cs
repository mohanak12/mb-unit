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

using Castle.Core.Logging;

using Gallio.Icarus.Interfaces;

using MbUnit.Framework;

namespace Gallio.Icarus.Tests
{
    [TestFixture]
    public class IcarusLoggerTest : MockTest
    {
        [Test]
        public void CreateChildLogger_Test()
        {
            IProjectAdapterView projectAdapterView = mocks.CreateMock<IProjectAdapterView>();
            Exception e = new Exception("test");
            //projectAdapterView.WriteToLog(LoggerLevel.Debug, "test", "test", e);
            mocks.ReplayAll();
            IcarusLogger icarusLogger = new IcarusLogger(projectAdapterView);
            IcarusLogger childLogger = (IcarusLogger)icarusLogger.CreateChildLogger("test");
            //childLogger.Debug("test", e);
        }
    }
}