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
using System.Collections.Concurrent;
using TypeC;
using IotGateway.Config;

namespace IotGateway.Tests
{
    [TestClass]
    public class TagStateManagerTests
    {
        private List<TagConfigRecord> _tagRecords;
        [TestInitialize]
        public void Initialize()
        {
            _tagRecords = new List<TagConfigRecord>();
            _tagRecords.Add(new TagConfigRecord { TagGroupName = "Group1", TagName = "Boiler1.Temperature", SendingInterval = 5, TagNamespaceIndex = 2 });
            _tagRecords.Add(new TagConfigRecord { TagGroupName = "Group1", TagName = "AirConditioner1.Temperature", SendingInterval = 5, TagNamespaceIndex = 2 });
            _tagRecords.Add(new TagConfigRecord { TagGroupName = "Group2", TagName = "Boiler2.Temperature", SendingInterval = 2, TagNamespaceIndex = 2 });
            _tagRecords.Add(new TagConfigRecord { TagGroupName = "Group2", TagName = "AirConditioner2.Temperature", SendingInterval = 2, TagNamespaceIndex = 2 });

        }
        [TestMethod]
        public void TestMethod1()
        {
            Dictionary<string, object> fields = new Dictionary<string, object>() { { "Quality", "1" } };
            Tag tag = new Tag("Boiler1", fields);
            tag.SetTimestamp(DateTime.UtcNow);
            tag.SetDataPoint("Boiler1.Temperature", 75.3D, fields);
            tag.SetDataPoint("Rack1.OpenStatus", true, fields);
            tag.ResetChangedStatus();

            tag.SetTimestamp(DateTime.UtcNow);
            tag.SetDataPoint("Boiler1.Temperature", 78.3D, fields);
            tag.SetDataPoint("Rack1.OpenStatus", false, fields);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(tag);
            Tag tag2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Tag>(json);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            TagConfigDatabase db = TagConfigDatabase.Instance;
            db.Load(_tagRecords);
            TagStateManager tsm = new TagStateManager(db);
            Assert.AreEqual("AirConditioner1.Temperature", tsm.GetTag("AirConditioner1.Temperature").Name);
        }

        [TestMethod]
        public void UpdateTagTest()
        {
            TagConfigDatabase db = TagConfigDatabase.Instance;
            db.Load(_tagRecords);
            TagStateManager tsm = new TagStateManager(db);
            Dictionary<string, object> fields = new Dictionary<string, object> { { "Quality", "1" } };
            tsm.UpdateTag<double>("AirConditioner1.Temperature", DateTime.UtcNow, 77.5D, fields);
            tsm.UpdateTag<double>("AirConditioner1.Temperature", DateTime.UtcNow, 79.4D, fields);

            var tag = tsm.GetTag( "AirConditioner1.Temperature");
            DataPoint<double> dp = (DataPoint<double>)tag.DataPoints["AirConditioner1.Temperature"];
            Assert.AreEqual(79.4D, dp.Val);
        }
    }
}
