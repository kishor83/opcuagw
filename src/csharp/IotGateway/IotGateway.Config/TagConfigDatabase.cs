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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotGateway.Config
{
    public class TagConfigDatabase
    {
        private List<TagConfigRecord> _tagRecords;
        private static TagConfigDatabase _instance = new TagConfigDatabase();
        private TagConfigDatabase() {}
        public IEnumerable<string> GetDisctinctTagGroupNames()
        {
            if (_tagRecords == null) return null;
            var tagRecords = _tagRecords.Select(rec => rec.TagGroupName);
            if (tagRecords.Count() == 0)
            {
                return null; 
            }
            else
            {
                return tagRecords.Distinct();
            }
        }
        public string GetTagGroupName(string tagName)
        {
            if (_tagRecords == null) return null;
            var tagRecords = _tagRecords.Where(rec => (rec.TagName == tagName));
            if (tagRecords.Count() == 0)
            {
                return null;
            }
            else
            {
                return tagRecords.First().TagGroupName;
            }
        }
        public int GetSendingInterval(string tagGroupName)
        {
            if (_tagRecords == null) return -1;

            var tagRecords = _tagRecords.Where(rec => (rec.TagGroupName == tagGroupName));
            if (tagRecords.Count() == 0)
            {
                return -1;
            }
            else
            {
                return tagRecords.First().SendingInterval;
            }
        }
        public int GetNamespaceIndex(string tagName)
        {
            if (_tagRecords == null) return -1;
            var tagRecords = _tagRecords.Where(rec => (rec.TagName == tagName));
            if (tagRecords.Count() == 0)
            {
                return -1;
            }
            else
            {
                return tagRecords.First().TagNamespaceIndex;
            }
        }
        public IEnumerable<TagConfigRecord> GetTags(string tagGroupName)
        {
            if (_tagRecords == null) return null;

            return _tagRecords.Where(rec => (rec.TagGroupName == tagGroupName));
        }

        private List<TagConfigRecord> TagRecords
        {
            get { return _tagRecords; }
        }

        public void  Load(string json)
        {
            this._tagRecords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TagConfigRecord>>(json);
        }
        public void Load(List<TagConfigRecord> tagRecords)
        {
            this._tagRecords = tagRecords;
        }

        public static TagConfigDatabase Instance
        {
            get { return _instance; }
        }
    }
}
