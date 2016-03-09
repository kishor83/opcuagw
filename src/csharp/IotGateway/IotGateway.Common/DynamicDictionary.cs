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
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotGateway.Common
{
	/// <summary>
	/// Adapted from MSDN samples; if a field is not found, null will be returned
	/// </summary>
	public class DynamicDictionary : DynamicObject
	{
		Dictionary<string, object> _innerDictionary = new Dictionary<string, object>();
		public int Count
		{
			get { return _innerDictionary.Count; }
		}
		/// <summary>
		/// Always returns true to avoid RuntimeBinderException
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			//string name = binder.Name.ToLower();
			_innerDictionary.TryGetValue(binder.Name, out result);
			//ignore the return value as we don't want RuntimeBinderException if an item is not found
			return true;
		}
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_innerDictionary[binder.Name] = value;
			return true;
		}

		public Dictionary<string, object> InnerDictionary
		{ get { return _innerDictionary; } }
	}
}
