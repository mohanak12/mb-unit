﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.Model;
using MbUnit.Framework;

namespace Gallio.Tests.Model
{
    [TestsOn(typeof(BaseTest))]
    public class BaseTestTest
    {
        [Test]
        public void AssignsUniqueLocalIdsToChildren()
        {
            var test = new BaseTest("name", null);

            Assert.AreEqual("localId", test.GetUniqueLocalIdForChild("localId"),
                "If no conflict, assigns local id hint to child.");
            Assert.AreEqual("localId2", test.GetUniqueLocalIdForChild("localId"),
                "If conflict, assigned suffixed local id hint to child.");
            Assert.AreEqual("localId3", test.GetUniqueLocalIdForChild("localId"),
                "If conflict, assigned suffixed local id hint to child.");
            Assert.AreEqual("localId22", test.GetUniqueLocalIdForChild("localId2"),
                "If conflict, assigned suffixed local id hint to child.");

            Assert.AreEqual("differentId", test.GetUniqueLocalIdForChild("differentId"),
                "If no conflict, assigns local id hint to child.");
        }

        [Test]
        public void UsesNameAsLocalIdIfNoHintAndNoConflicts()
        {
            var test = new BaseTest("name", null);
            Assert.AreEqual("name", test.LocalId, "Uses name when no parent available.");

            var testWithParent = new BaseTest("name", null);
            test.AddChild(testWithParent);
            Assert.AreEqual("name", testWithParent.LocalId, "Uses name when parent available but no conflicts.");
        }

        [Test]
        public void UsesLocalIdHintAsLocalIdIfNoConflicts()
        {
            var test = new BaseTest("name", null);
            test.LocalIdHint = "hint";
            Assert.AreEqual("hint", test.LocalId, "Uses local id hint when no parent available.");

            var testWithParent = new BaseTest("name", null);
            test.AddChild(testWithParent);
            testWithParent.LocalIdHint = "hint";
            Assert.AreEqual("hint", testWithParent.LocalId, "Uses local id hint when parent available but no conflicts.");
        }

        [Test]
        public void UsesSuffixedLocalIdWhenConflictsOccur()
        {
            var parent = new BaseTest("name", null);

            var test1 = new BaseTest("test", null);
            parent.AddChild(test1);
            Assert.AreEqual("test", test1.LocalId, "Uses name because there is no conflict.");

            var test2 = new BaseTest("test", null);
            parent.AddChild(test2);
            Assert.AreEqual("test2", test2.LocalId, "Uses suffixed name because there is a conflict in the name and no hint.");

            var test3 = new BaseTest("name", null);
            test3.LocalIdHint = "test";
            parent.AddChild(test3);
            Assert.AreEqual("test3", test3.LocalId, "Uses suffixed name because there is a conflict in the hint.");
        }
    }
}
