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

namespace IotGateway.Core
{
    public class Tag
    {
        private string _name;
        private long _timestamp;
        private bool _changed;
        private ConcurrentDictionary<string, object> _dataPoints;
        private Dictionary<string, object> _fields;

        public Tag(string name, Dictionary<string, object> fields)
        {
            this._name = name;
            this._fields = fields;
            this._timestamp = default(long);
            this._changed = default(bool);
            _dataPoints = new ConcurrentDictionary<string, object>();
        }
        public Tag(string name)
        {
            this._name = name;
            this._fields = null;
            this._timestamp = default(long);
            this._changed = default(bool);
            _dataPoints = new ConcurrentDictionary<string, object>();
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        //unix time
        public long Timestamp
        {
            get { return this._timestamp; }
            set { this._timestamp = value; }
        }
        //we will not use property for this to avoid serialization
        public bool IsChanged()
        {
            return this._changed;
        }

        public ConcurrentDictionary<string, object> DataPoints
        {
            get { return _dataPoints; }
            set
            {
                this._dataPoints = value;
            }
        }

        //we will not use property for this to avoid serialization
        public void SetTimestamp(DateTime timestamp)
        {
            this._timestamp = UnixEpoch.ToUnixTime(timestamp);
        }
        //we will not use property for this to avoid serialization
        public DateTime GetTimestamp()
        {
            return UnixEpoch.FromUnixTime(this._timestamp);
        }
        public void UpdateDataPoint<TVal>(string name, TVal val) where TVal : IComparable
        {
            var dp = GetDataPoint<TVal>(name);
            if (dp == null)
            {
                dp = new DataPoint<TVal>(name);
                dp.Val = val;
                _dataPoints.TryAdd(name, dp);
            }
            else
            {
                dp.Val = val;
            }
            this._changed = dp.GetChanged();
        }
        public void UpdateDataPoint<TVal>(string name, TVal val, Dictionary<string, object> fields) where TVal : IComparable
        {
            var dp = GetDataPoint<TVal>(name);
            if (dp == null)
            {
                dp = new DataPoint<TVal>(name);
                dp.Val = val;
                dp.Fields = fields; 
                _dataPoints.TryAdd(name, dp);
            }
            else
            {
                dp.Val = val;
                dp.Fields = fields;
            }
            this._changed = dp.GetChanged();
        }
        public void UpdateDataPoint<TVal1, TVal2>(string name1, TVal1 val1, string name2, TVal2 val2 ) 
            where TVal1 : IComparable
            where TVal2 : IComparable
        {
            UpdateDataPoint<TVal1>(name1, val1, null);
            UpdateDataPoint<TVal2>(name2, val2, null);
        }
        public void ResetChangedStatus()
        {
            this._changed = false;
            foreach (var dp in this._dataPoints.Keys)
            {
                ((IChangeable)this._dataPoints[dp]).SetChanged(false);
            }
        }
        public void SetDataPoint<TVal>(string name, TVal val) where TVal : IComparable
        {
            SetDataPoint<TVal>(name, val, null);
        }
        public void SetDataPoint<TVal>(string name, TVal val, Dictionary<string, object> fields) where TVal : IComparable
        {
            if (_dataPoints == null)
            {
                _dataPoints = new ConcurrentDictionary<string, object>();
            }

            _dataPoints.AddOrUpdate(name, (key) =>            
                {
                    DataPoint<TVal> dp = new DataPoint<TVal>(key);
                    dp.Val = val;
                    dp.Fields = fields;
                    this._changed = dp.GetChanged();
                    return dp;
                },                         
                (key, oldVal) => 
                {
                    DataPoint<TVal> dp = (DataPoint<TVal>)oldVal;
                    dp.Val = val;
                    dp.Fields = fields;
                    this._changed = dp.GetChanged();
                    return oldVal;
                });
        }
        private DataPoint<TVal> GetDataPoint<TVal>(string name) where TVal : IComparable
        {
            if (this._dataPoints == null) return null;
            object dpTemp;
            if (_dataPoints.TryGetValue(name, out dpTemp))
            {

                DataPoint<TVal> dp = (DataPoint<TVal>)(object)dpTemp;
                return dp;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, object> Other
        {
            get { return _fields; }
            set { _fields = value; }
        }
    }
}
