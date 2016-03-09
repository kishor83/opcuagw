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
using Newtonsoft.Json;

namespace IotGateway.Core
{
    public class Msg:IMessage
    {
        private string _idFieldName; 
        private Dictionary<string, string> _fieldDict;
        public Msg()
        {
            _fieldDict = new Dictionary<string, string>();
        }
        public string GetId()
        {
            if (_idFieldName == null)
                return null;

            if (!_fieldDict.ContainsKey(_idFieldName))
                return null;
            
            return _fieldDict[_idFieldName];
        }

        public string GetValue(string fieldName)
        {
            if (_fieldDict.ContainsKey(fieldName))
            {
                return _fieldDict[fieldName];
            }
            return null; 
        }

        public void Add(string fieldName, string fieldValue)
        {
            AddOrReplace(fieldName, fieldValue);
        }
        public void Add(string fieldName, string fieldValue,bool isId)
        {
            _idFieldName = fieldName;
            AddOrReplace(fieldName, fieldValue);
        }
        private void AddOrReplace(string fieldName, string fieldValue)
        {
            if (_fieldDict.ContainsKey(fieldName))
            {
                _fieldDict[fieldName] = fieldValue;
            }
            else
            {
                _fieldDict.Add(fieldName, fieldValue);
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static string Serialize(Msg msg)
        {
            return JsonConvert.SerializeObject(msg);
        }
        public static Msg Deserialize(string serializedObject)
        {
            return JsonConvert.DeserializeObject<Msg>(serializedObject);
        }
        public string KeyName
        {
            get { return _idFieldName; }
            set {_idFieldName = value; }
        }
        public Dictionary<string, string> FieldList
        {
            get {return _fieldDict; }
            set {_fieldDict = value; }
        }
    }
}
