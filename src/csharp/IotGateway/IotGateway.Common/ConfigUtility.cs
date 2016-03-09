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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IotGateway.Common
{
	public class ConfigUtility
	{
		/// <summary>
		/// reads json file and returns it as string
		/// </summary>
		/// <param name="fileName">Name of the file</param>
		/// <returns></returns>
		public static string ReadFile(string fileName)
		{
			string codebase = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
			string workingDir = new Uri(Path.GetDirectoryName(codebase)).LocalPath;
			return File.ReadAllText(workingDir + @"/" + fileName);
		}

        public static string GetCurrentWorkingDirectory()
        {
            string codebase = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            string workingDir = new Uri(Path.GetDirectoryName(codebase)).LocalPath;
            return workingDir;
        }
		public static void WriteFile(string fileName, string text)
		{
			string codebase = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
			string workingDir = new Uri(Path.GetDirectoryName(codebase)).LocalPath;
			File.WriteAllText(workingDir + @"/" + fileName, text);
		}

		public static T ReadConfig<T>(string fileName)
		{
			string config = ReadFile(fileName);
			return JsonConvert.DeserializeObject<T>(config);	
		}
		public static void SaveConfig<T>(string fileName, T config)
		{
			WriteFile(fileName, JsonConvert.SerializeObject(config));
		}
	}
}
