using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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


		public static void Decode(byte[] bytes)
		{
			MemoryStream memStream = new(bytes);

			int b;
			while ((b = memStream.ReadByte()) != -1)
			{
				switch (b)
				{
					case DictionaryStart:
						Console.WriteLine("| DICTIONARY |");
						break;
					case NumberStart:
						Console.WriteLine("| NUMBER |");
						break;
					case ListStart:
						Console.WriteLine("| LIST |");
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
						string data = DecodeString(memStream, b - '0');
						Console.WriteLine(data);
						break;

					default:
						//throw new Exception("Invalid file format");
						break;
				}
			}
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
	}
}