/*
Copyright(c) Microsoft.All rights reserved.

The MIT License(MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IotGateway.Core;
using System.Collections.Generic;
using System.Diagnostics;
using IotGateway.Config;

namespace IotGateway.Tests
{
    [TestClass]
    public class TagConfigDatabaseTests
    {
        [TestMethod]
        public void QueryTest1()
        {
            List<TagConfigRecord> tagRecords = new List<TagConfigRecord>();
            tagRecords.Add(new TagConfigRecord { TagGroupName = "Group1", TagName = "Boiler1.Temperature", SendingInterval = 5, TagNamespaceIndex = 2 });
            tagRecords.Add(new TagConfigRecord { TagGroupName = "Group1", TagName = "AirConditioner1.Temperature", SendingInterval = 5, TagNamespaceIndex = 2 });
            tagRecords.Add(new TagConfigRecord { TagGroupName = "Group2", TagName = "Boiler2.Temperature", SendingInterval = 2, TagNamespaceIndex = 2 });
            tagRecords.Add(new TagConfigRecord { TagGroupName = "Group2", TagName = "AirConditioner2.Temperature", SendingInterval = 2, TagNamespaceIndex = 2 });

            TagConfigDatabase db = TagConfigDatabase.Instance;
            db.Load(tagRecords);

            var groups =  db.GetDisctinctTagGroupNames();
            int count = 0;
            foreach (string str in groups)
            {
                count++;
            }
            Assert.AreEqual(2, count);

            var t1 = db.GetTagGroupName("Boiler1.Temperature");
            Assert.AreEqual("Group1", t1);

            var tagName = db.GetTagGroupName("Boiler1.Temperature1");
            Assert.IsNull(tagName);
        }
    }
}
