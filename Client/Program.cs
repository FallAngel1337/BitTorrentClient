using System;
using System.IO;
using BitTorrent;

namespace Client
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				BEncoding.Decode(File.ReadAllBytes(args[0]));
			}
		}
	}
}
