using System;
using System.Threading.Tasks;

using MonoTorrent;
using MonoTorrent.Client;

using CommandLine;
using Error = CommandLine.Error;
using System.Collections.Generic;
using System.Text;

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
		static async Task Main(string[] args)
		{
			var parser = await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(RunOptions);
			parser.WithNotParsed(HandleNotParsed);
		}

		static async Task RunOptions(Options opts)
		{
			ClientEngine engine = new();
			var torrent = await Torrent.LoadAsync(opts.Torrent);
			var manager = await engine.AddAsync(torrent, opts.Download);
			manager.PeersFound += Manager_PeersFound;
			Console.WriteLine("InfoHash =>" + torrent.InfoHash.ToString());

			if (opts.Verbose)
			{
				manager.PeerConnected += (o, e) => Console.WriteLine($"Connection succeeded: {e.Peer.Uri}");
				manager.ConnectionAttemptFailed += (o, e) => Console.WriteLine($"Connection failed: {e.Peer.ConnectionUri} - {e.Reason} - {e.Peer}");
			}

			await manager.StartAsync();

			StringBuilder sb = new(1024);
			while (engine.IsRunning)
			{
				sb.Remove(0, sb.Length);
				sb.AppendFormat($"Transfer Rate: { engine.TotalDownloadSpeed / 1024.0 }kb/s | { engine.TotalUploadSpeed / 1024.0 }kb/s");
			}

		}

		static void HandleNotParsed(IEnumerable<Error> error)
		{
			Console.WriteLine("Could not parse the arguments!");
		}

		static void Manager_PeersFound(object sender, PeersAddedEventArgs e)
		{
			Console.WriteLine($"Found {e.NewPeers} new peers and {e.ExistingPeers} existing peers");
		}
	}
}
