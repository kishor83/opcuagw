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

namespace IotGateway.Core
{
    public class DataPoint<TVal> : IDataPoint<TVal>, IChangeable where TVal : IComparable
    {
        private string _name;
        private TVal _val;
        private Dictionary<string, object> _fields;
        private bool _changed;

        public DataPoint(string name)
        {
            this._name = name;
            this._fields = null;
            this._val = default(TVal);
            this._changed = false;
        }
        public DataPoint(string name, Dictionary<string, object> fields)
        {
            this._name = name;
            this._fields = fields;
            this._val = default(TVal);
            this._changed = false;
        }

        public string Name
        {
            get { return _name; }

        }
        public TVal Val
        {
            get { return _val; }

            set
            {
                if (object.Equals(_val, default(TVal)) || (_val.CompareTo(value) != 0))
                {
                    _val = value;
                    _changed = true;
                }
                else
                if (_val.CompareTo(value) == 0)
                {
                    return;
                }
            }
        }


        public Dictionary<string, object> Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }

        public int CompareTo(object obj)
        {
            return _name.CompareTo(obj);
        }

        public bool GetChanged()
        {
            return _changed;
        }

        public void SetChanged(bool val)
        {
            this._changed = true;
        }
    }
}
