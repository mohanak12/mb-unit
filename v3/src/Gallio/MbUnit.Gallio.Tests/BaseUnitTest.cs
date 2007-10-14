// Copyright 2007 MbUnit Project - http://www.mbunit.com/
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

extern alias MbUnit2;
using MbUnit2::MbUnit.Framework;

using System;
using System.Collections.Generic;
using System.Text;
using Rhino.Mocks;

namespace MbUnit.Tests
{
    /// <summary>
    /// Base unit test.
    /// All unit tests that require certain common facilities like Mock Objects
    /// inherit from this class.
    /// </summary>
    public abstract class BaseUnitTest
    {
        private MockRepository mocks;

        /// <summary>
        /// Gets the mock object repository.
        /// </summary>
        public MockRepository Mocks
        {
            get
            {
                if (mocks == null)
                    mocks = new MockRepository();

                return mocks;
            }
        }

        [SetUp]
        [global::MbUnit.Framework.SetUp]
        public virtual void SetUp()
        {
        }

        [TearDown]
        [global::MbUnit.Framework.TearDown]
        public virtual void TearDown()
        {
            if (mocks != null)
            {
                try
                {
                    mocks.ReplayAll();
                    mocks.VerifyAll();
                }
                finally
                {
                    mocks = null;
                }
            }
        }
    }
}