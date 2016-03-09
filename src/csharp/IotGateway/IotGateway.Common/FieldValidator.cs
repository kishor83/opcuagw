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

namespace IotGateway.Common
{
	public class FieldValidator
	{
		/// <summary>
		/// A collection of Name-Value pairs
		/// </summary>
		/// <param name="fieldCollection"></param>
		/// <param name="fieldNames"></param>
		public static void CheckMissingFields(dynamic fieldCollection, string[] fieldNames)
		{
			List<string> errors = new List<string>();
			foreach (var fieldName in fieldNames)
			{
				object fieldValue = null;
				try
				{
					fieldValue = fieldCollection.InnerDictionary[fieldName];
				}
				catch(KeyNotFoundException)
				{
					fieldValue = null; 
				}
				if (fieldValue == null)
				{
					errors.Add(string.Format("{0} is missing", fieldName));
				}
			}

			if (errors.Count > 0)
			{
				ValidationException ex = new ValidationException("Invalid settings");
				ex.Errors = errors;
				throw ex;
			}
		}

        public static void CheckMissingFields<TKey, TVal>(Dictionary<TKey, TVal> fieldCollection, TKey[] fieldNames)
        {
            List<string> errors = new List<string>();
            foreach (var fieldName in fieldNames)
            {
                object fieldValue = null;
                try
                {
                    fieldValue = fieldCollection[fieldName];
                }
                catch (KeyNotFoundException)
                {
                    fieldValue = null;
                }
                if (fieldValue == null)
                {
                    errors.Add(string.Format("{0} is missing", fieldName));
                }
            }

            if (errors.Count > 0)
            {
                ValidationException ex = new ValidationException("Invalid settings");
                ex.Errors = errors;
                throw ex;
            }
        }
    }
}
