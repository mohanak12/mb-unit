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
using Gallio.Hosting;
using Gallio.Hosting.Channels;
using Gallio.Tests.Integration;
using MbUnit.Framework;

namespace Gallio.Tests.Hosting.Channels
{
    [TestFixture]
    [TestsOn(typeof(BinaryTcpClientChannel))]
    [TestsOn(typeof(BinaryTcpServerChannel))]
    [DependsOn(typeof(BaseClientChannelTest))]
    [DependsOn(typeof(BaseServerChannelTest))]
    public class BinaryTcpChannelTest
    {
        private const int PortNumber = 33333;
        private const string ServiceName = "Test";

        [Test, ExpectedArgumentNullException]
        public void BinaryTcpClientChannelConstructorThrowsIfHostNameIsNull()
        {
            new BinaryTcpClientChannel(null, 1);
        }

        [Test, ExpectedArgumentNullException]
        public void BinaryTcpServerChannelConstructorThrowsIfHostNameIsNull()
        {
            new BinaryTcpServerChannel(null, 1);
        }

        [Test]
        public void RegisteredServiceCanBeAccessedWithGetService()
        {
            using (IHost host = new IsolatedAppDomainHostFactory().CreateHost(new HostSetup(), new LogStreamLogger()))
            {
                HostAssemblyResolverHook.Install(host);

                host.DoCallback(RemoteCallback);

                using (BinaryTcpClientChannel clientChannel = new BinaryTcpClientChannel("localhost", PortNumber))
                {
                    TestService serviceProxy =
                        (TestService)clientChannel.GetService(typeof(TestService), ServiceName);
                    Assert.AreEqual(42, serviceProxy.Add(23, 19));
                }
            }
        }

        public static void RemoteCallback()
        {
            BinaryTcpServerChannel serverChannel = new BinaryTcpServerChannel("localhost", PortNumber);
            TestService serviceProvider = new TestService();
            serverChannel.RegisterService(ServiceName, serviceProvider);
        }

        public class TestService : MarshalByRefObject
        {
            public int Add(int x, int y)
            {
                return x + y;
            }
        }
    }
}
