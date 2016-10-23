using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace sockets_blogpost
{
	[DataContract]
	class Balloon 
	{
		[DataMember]
		public float x;
		[DataMember]
		public float y;
		[DataMember]
		public float z;
		[DataMember]
		public string color;

		public override string ToString ()
		{
			return string.Format ("[Balloon] {0} {1} {2} {3}", x, y, z, color);
		}
	}

	[DataContract]
	class Command
	{
		[DataMember]
		public string command;

		public override string ToString ()
		{
			return string.Format ("[Command] {0}", command);
		}
	}

	class Program
	{
		static void Main()
		{
			Console.WriteLine ("Main");

			TcpListener myListner = new
				TcpListener(new IPAddress(0x00000000), 9090);
			myListner.Start();

			while(true)
			{
				Console.WriteLine ("Waiting for connection...");
				Socket mySocket = myListner.AcceptSocket();
				Console.WriteLine ("Connection accepted");
				Stream myStream = new NetworkStream(mySocket);
				StreamReader reader = new StreamReader(myStream);

				bool quit = false;

				while (!quit) 
				{
					string text = reader.ReadLine();
					if (text != null) 
					{
						Console.WriteLine ("Got message " + text);

						if (text.Contains ("command"))
						{
							Console.WriteLine ("Deserializing command");
							DataContractJsonSerializer s = new DataContractJsonSerializer (typeof(Command));
							byte [] bytes = Encoding.Unicode.GetBytes (text);
							Command c = (Command) s.ReadObject (new MemoryStream (bytes));
							Console.WriteLine ("Deserialized command " + c);
							if (c.command == "END") 
							{
								quit = true;
							}
						}
						else 
						{
							Console.WriteLine ("Deserializing Balloon");
							DataContractJsonSerializer s = new DataContractJsonSerializer (typeof(Balloon));
							byte [] bytes = Encoding.Unicode.GetBytes (text);
							Balloon c = (Balloon) s.ReadObject (new MemoryStream (bytes));
							Console.WriteLine ("Deserialized Balloon " + c);
						}
						if (text != null && text.ToLower() == "quit")
						{
							Console.WriteLine ("Quitting");
							quit = true;
						}
					}
				}	

				myStream.Close();
				mySocket.Close();
				reader.Close ();
			}
		}

		public T deserializeJSON<T>(string json)
		{
			var instance = typeof(T);

			using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
			{
				DataContractJsonSerializer deserializer = new DataContractJsonSerializer(instance.GetType());
				return (T)deserializer.ReadObject(ms);
			}
		}
	}
}
