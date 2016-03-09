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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotGateway.Config;

namespace IotGateway.Core
{
    public class TagStateManager
    {
        private ConcurrentDictionary<string, TagGroup> _tagGroups = new ConcurrentDictionary<string, TagGroup>();
        public TagStateManager(TagConfigDatabase tagConfigDatabase)
        {
            LoadTags(tagConfigDatabase);
        }
        public Tag GetTag(string tagName)
        {
            string tagGroupName = TagConfigDatabase.Instance.GetTagGroupName(tagName);
            return _tagGroups[tagGroupName].Tags[tagName];
        }
        private void LoadTags(TagConfigDatabase tagConfigDatabase)
        {
            var tagGroups = tagConfigDatabase.GetDisctinctTagGroupNames();
            foreach(var groupName in tagGroups)
            {
                TagGroup currentTagGroup;
                if (!_tagGroups.TryGetValue(groupName, out currentTagGroup))
                {
                    currentTagGroup = new TagGroup(groupName);
                    _tagGroups.TryAdd(groupName, currentTagGroup);
                }

                var tagRecords = tagConfigDatabase.GetTags(groupName);
                foreach(var tr in tagRecords)
                {
                    Tag tag = new Tag(tr.TagName);
                    currentTagGroup.Config.SendIntervalSeconds = tr.SendingInterval;
                    currentTagGroup.Tags.TryAdd(tag.Name, tag);
                }
            }
        }
        public void UpdateTag<TVal>(string tagName, string dataPointName, DateTime timestamp, TVal val, Dictionary<string, object> fields) where TVal :IComparable
        {
            Tag t = GetTag(tagName);
            if (fields == null)
            {
                t.SetTimestamp(timestamp);
                t.UpdateDataPoint(dataPointName, val);
            }
        }
        public ICollection<TagGroup> TagGroups
        {
            get
            {
                return _tagGroups.Values;
            }
        }
        public void UpdateTag<TVal>(string tagName, DateTime timestamp, TVal val, Dictionary<string, object> fields) where TVal : IComparable
        {
            Tag t = GetTag(tagName);
            string dataPointName = tagName;
            t.SetTimestamp(timestamp);
            t.UpdateDataPoint(dataPointName, val);
            if (fields != null)
            {
                t.Other = fields; 
            }
        }
    }
}
