using System;
using System.Text;
using System.Collections.Generic;

using MonoTorrent;
using MonoTorrent.Client;
using System.Threading.Tasks;

namespace Client.Downloader
{
	public struct DownloaderConfig
	{
		public readonly string DownloadPath;
		public readonly bool Verbose;
		public readonly Settings Settings;

		public DownloaderConfig(string path, bool verbose, Settings settings)
		{
			DownloadPath = path;
			Verbose = verbose;
			Settings = settings;
		}
	}

	public class TorrentDownloader
	{
		private readonly ClientEngine Engine;
		public readonly DownloaderConfig Config;

		public TorrentDownloader()
		{
			Config = new(Environment.CurrentDirectory, false, new Settings());
			Engine = new(Config.Settings.engineSettings.ToSettings());
		}

		public TorrentDownloader(DownloaderConfig config)
		{
			Config = config;
			Engine = new(Config.Settings.engineSettings.ToSettings());
		}

		public async Task StartDownloadAsync(string torrent_file)
		{
			var torrent = await Torrent.LoadAsync(torrent_file);
			var manager = await Engine.AddAsync(torrent, Config.DownloadPath);
			Console.WriteLine("InfoHash =>" + torrent.InfoHash.ToString());

			if (Config.Verbose)
			{
				manager.PeerConnected += (o, e) => Console.WriteLine($"Connection succeeded: {e.Peer.Uri}");
				manager.ConnectionAttemptFailed += (o, e) => Console.WriteLine($"Connection failed: {e.Peer.ConnectionUri} - {e.Reason} - {e.Peer}");
				manager.PeersFound += Manager_PeersFound;
			}

			await manager.StartAsync();

			StringBuilder sb = new(1024);
			while (Engine.IsRunning)
			{
				sb.Remove(0, sb.Length);
				sb.AppendFormat($"Transfer Rate: { Engine.TotalDownloadSpeed / 1024.0 }kb/s | { Engine.TotalUploadSpeed / 1024.0 }kb/s");
			}
		}


		static void Manager_PeersFound(object sender, PeersAddedEventArgs e)
		{
			Console.WriteLine($"Found {e.NewPeers} new peers and {e.ExistingPeers} existing peers");
		}

		static void Main(string[] args) { }
	}
}
