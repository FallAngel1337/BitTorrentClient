using System;
using CommandLine;
using System.Threading.Tasks;
using MonoTorrent;
using MonoTorrent.Client;

namespace Client
{
	public class Options
	{
		[Option('v', "verbose", Required = false, HelpText = "Enable verbose mode")]
		public bool Verbose { get; set; } = false;

		[Option('l', "load", Required = true, HelpText = "The torrent file/link/hash")]
		public string Torrent { get; set; } = null;

		[Option('d', "path", Required = false, HelpText = "Download path")]
		public string Download { get; set; } = Environment.CurrentDirectory;
	}

	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello!");
		}
	}
}
