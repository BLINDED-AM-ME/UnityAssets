
//    MIT License
//    
//    Copyright (c) 2017 Dustin Whirle
//    
//    My Youtube stuff: https://www.youtube.com/playlist?list=PL-sp8pM7xzbVls1NovXqwgfBQiwhTA_Ya
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace BLINDED_AM_ME._ApplicationComponents{

	[RequireComponent(typeof(ApplicationComponent_Manager))]
	public class SaveData_ApplicationComponent : ApplicationComponent_Manager.ApplicationComponent {

		public string secretPassword  = "no_such_luck";
		public string fileDestination = "Application.persistentDataPath";
		public string fileName        = "Saved_Data.xml";
		public bool   isEncrypted     = true;


		private Dictionary<string, object> _allData;

		#if UNITY_EDITOR

		void Reset(){

			SaveData_ApplicationComponent[] comps = GetComponents<SaveData_ApplicationComponent>();

			for(int i=0; i<comps.Length; i++)
				if(comps[i] == this)
					fileName = "Saved_Data"+i+".xml";

		}

		#endif


		///<summary>Called in the Load up scene by LoadUpScene_Controller</summary>
		public override void Initialize (CallBack callback)
		{
			base.Initialize (callback);

			_allData = new Dictionary<string, object>();
			_allData.Add("emptyObj", "0");

			if(fileDestination.Equals("Application.persistentDataPath"))
				fileDestination = Application.persistentDataPath;

			Load_Data();

			callback();

		}

		void OnApplicationQuit(){

			Save_Data();
		}


		/// <summary>
		/// Call this at the beginning of the App's opening
		/// </summary>
		public void Load_Data(){

			string finalOutcome = "";
			string line = "";

			if(System.IO.File.Exists(fileDestination+"/"+fileName)){

				//Pass the file path and file name to the StreamReader constructor
				using (StreamReader sr = new StreamReader(fileDestination+"/"+fileName)){

					//Read the first line of text
					line = sr.ReadLine();

					//Continue to read until you reach end of file
					while (line != null) 
					{
						finalOutcome += line;
						//Read the next line
						line = sr.ReadLine();
					}

					//close the file
					sr.Close();

					if(!isEncrypted){

						_allData = XML_Deserialize(finalOutcome);

					}else{

						byte[] theKeyBytes  = Encoding.UTF8.GetBytes(secretPassword);
						byte[] theDataBytes = Convert.FromBase64String(finalOutcome);

						// decipher
						int tempInt = 0;
						for(int i=0; i<theDataBytes.Length; i++){

							tempInt = (int) theDataBytes[i];
							tempInt -= (int) theKeyBytes[i % theKeyBytes.Length];
							if(tempInt < 0)// aka negative
								tempInt += 256;
							theDataBytes[i] = (byte) tempInt;
						}

						string theDecodedString = Encoding.UTF8.GetString(theDataBytes);

						_allData = XML_Deserialize(theDecodedString);

					}

				}

			}else{

				_allData = new Dictionary<string, object>();
				_allData.Add("emptyObj", "0");
			}

		}


		/// <summary>
		/// Saves the data to destination.
		/// </summary>
		/// <param name="destination">Destination.</param>
		/// <param name="fileName">File name.</param>
		public void Save_Data(){


			if(!isEncrypted){

				string theString = "";

				theString = XML_Serialize(_allData);

				// check if destination exists
				if(!System.IO.Directory.Exists(fileDestination))
					System.IO.Directory.CreateDirectory(fileDestination);


				//Pass the filepath and filename to the StreamWriter Constructor
				using(StreamWriter sw = new StreamWriter(fileDestination + "/" + fileName)){
					sw.Write(theString);
					//Close the file
					sw.Close();
				}	

			}else{

				byte[] theKeyBytes  = Encoding.UTF8.GetBytes(secretPassword);
				byte[] theDataBytes;

				theDataBytes = Encoding.UTF8.GetBytes(XML_Serialize(_allData));


				int tempInt = 0;  // byte can equal 0 - 255
				for(int i=0; i<theDataBytes.Length; i++){

					tempInt = (int) theDataBytes[i];
					tempInt += (int) theKeyBytes[i % theKeyBytes.Length];
					tempInt = tempInt  % 256;
					theDataBytes[i] = (byte) tempInt;
				}

				string theEncodedString = Convert.ToBase64String(theDataBytes, Base64FormattingOptions.InsertLineBreaks);

				// check if destination exists
				if(!System.IO.Directory.Exists(fileDestination))
					System.IO.Directory.CreateDirectory(fileDestination);


				//Pass the filepath and filename to the StreamWriter Constructor
				using(StreamWriter sw = new StreamWriter(fileDestination + "/" + fileName)){
					sw.Write(theEncodedString);
					//Close the file
					sw.Close();
				}	

			}
		}

		public void Set_Value(string key, string value){

			if(_allData.ContainsKey(key)){
				_allData[key] = value;
			}else{
				_allData.Add(key, value);
			}
		}

		public string Get_Value(string key, string defualtValue){

			if(_allData.ContainsKey(key)){
				return _allData[key].ToString();
			}else{
				return defualtValue;
			}
		}


		// XML


		public static string XML_Serialize(Dictionary<string, object> dict) 
		{ 

			// it won't do Dictionary so make it a List

			MemoryStream memory = new MemoryStream(); 
			XmlTextWriter writer = new XmlTextWriter(memory, Encoding.UTF8); 
			XmlSerializer serializer = new XmlSerializer(typeof(List<string>)); 

			List<string> list = new List<string>();
			// convert dictionary to list manually
			foreach(var item in dict)
			{
				list.Add(item.Key);
				list.Add(item.Value.ToString());
			}

			serializer.Serialize(writer, list); 
			memory = (MemoryStream)writer.BaseStream; 

			return Encoding.UTF8.GetString(memory.ToArray()); 

		} 

		public static Dictionary<string, object> XML_Deserialize(string dataString) 
		{ 

			MemoryStream memory = new MemoryStream(Encoding.UTF8.GetBytes(dataString));
			XmlSerializer serializer = new XmlSerializer(typeof(List<string>)); 

			List<string> list = (List<string>) serializer.Deserialize(memory);
			Dictionary<string, object> dict = new Dictionary<string, object>();

			for(int i=0; i<list.Count; i+=2){

				dict.Add(list[i], list[i+1]);
			}

			return dict;
		} 

	}
	
}