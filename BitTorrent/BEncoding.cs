using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace BitTorrent
{
	// http://www.bittorrent.org/beps/bep_0003.html
	public static class BEncoding
	{
		private const byte ListStart = (byte)'l';
		private const byte NumberStart = (byte)'i';
		private const byte DictionaryStart = (byte)'d';
		private const byte Split = (byte)':';
		private const byte End = (byte)'e';

		public static object DecodeFile(byte[] bytes)
		{
			MemoryStream memStream = new(bytes);
			return Decode(memStream);
		}

		private static object Decode(MemoryStream memStream)
		{
			object data = null;

			int b;
			while ((b = memStream.ReadByte()) != -1)
			{
				switch (b)
				{
					case DictionaryStart:
						Console.WriteLine("| DICTIONARY |");
						data = DecodeDictionary(memStream);
						foreach (var item in (Dictionary<string, object>)data)
						{
							Console.WriteLine($"	DATA: { item }");
						}
						break;
					case NumberStart:
						Console.WriteLine("| NUMBER |");
						data = DecodeNumber(memStream);
						Console.WriteLine($"DATA: { data }");

						break;
					case ListStart:
						Console.WriteLine("| LIST |");
						data = DecodeList(memStream);
						foreach (var item in (List<object>)data)
						{
							Console.WriteLine($"	DATA: { item }");
						}
						break;

					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						Console.WriteLine("| STRING |");
						data = DecodeString(memStream, b - '0');
						Console.WriteLine($"DATA: { data }");
						break;

					default:
						//throw new Exception("Invalid file format");
						break;

				}
			}

			return data;
		}
		
		private static Dictionary<string, object> DecodeDictionary(MemoryStream memStream)
		{
			Dictionary<string, object> dict = new();
			List<string> keys = new();

			int b;
			while ((b = memStream.ReadByte()) != -1 && b != End)
			{
				string key = DecodeString(memStream, b - '0');
				object val = Decode(memStream);

				keys.Add(key);
				dict.Add(key, val);
			}

			var sortedKeys = keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
			if (!keys.SequenceEqual(sortedKeys))
				throw new Exception("error loading dictionary: keys not sorted");

			return dict;
		}

		private static List<object> DecodeList(MemoryStream memStream)
		{
			List<object> list = new();

			int b;
			while ((b = memStream.ReadByte()) != -1 && b != End)
			{
				list.Add(Decode(memStream));
			}

			return list;
		}

		private static string DecodeString(MemoryStream memStream, int length)
		{
			int b;
			while ((b = memStream.ReadByte()) != -1 && b != Split)
			{
				if (b <= '9' && b >= '0')
				{
					length = length * 10 + (b - '0');
				}
				else
				{
					// throw new Exception($"Invalid string with length : { length }");
					return null;
				}
			}

			byte[] bytes = new byte[length];
			memStream.Read(bytes, 0, length);

			return Encoding.UTF8.GetString(bytes);
		}

		private static long DecodeNumber(MemoryStream memStream)
		{
			long num = 0;

			int b;
			while ((b = memStream.ReadByte()) != -1 && b != End)
			{
				if (b <= '9' && b >= '0')
				{
					num = num * 10 + (b - '0');
					// Console.WriteLine($"NUM: { num }");
				}
				else
				{
					return -1;
				}
			}

			return num;
		}
	}
}