using System;
using System.Text;
using System.Collections.Generic;

using MonoTorrent;
using MonoTorrent.Client;
using System.Threading.Tasks;
using MonoTorrent.BEncoding;

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
		public DownloaderConfig Config { get; private set; }
		public ClientEngine Engine { get; private set; }

		public Torrent TorrentFile { get; private set; }
		public TorrentManager Manager { get; private set; }
		public string TorrentPath { get; private set; }

		private bool Ready { get; set; }

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

		// Just works with CLI
		public async Task StartDownloadAsync(string torrent_file)
		{
			if (!Ready)
			{
				throw new Exception("Torrent download is not ready");
			}
			
			if (Config.Verbose)
			{
				Manager.PeerConnected += (o, e) => Console.WriteLine($"Connection succeeded: {e.Peer.Uri}");
				Manager.ConnectionAttemptFailed += (o, e) => Console.WriteLine($"Connection failed: {e.Peer.ConnectionUri} - {e.Reason} - {e.Peer}");
				Manager.PeersFound += Manager_PeersFound;
			}

			await Manager.StartAsync();

			StringBuilder sb = new(1024);
			while (Engine.IsRunning)
			{
				sb.Remove(0, sb.Length);
				sb.AppendFormat($"Transfer Rate: { Engine.TotalDownloadSpeed / 1024.0 }kb/s | { Engine.TotalUploadSpeed / 1024.0 }kb/s");

				Console.Clear();
				Console.WriteLine(sb);
			}
		}

		public async Task SetupDownload(string torrent)
		{
			TorrentPath = torrent;
			TorrentFile = Torrent.Load(TorrentPath);
			Manager = await Engine.AddAsync(torrent, Config.DownloadPath);

			Ready = true;
		}

		static void Manager_PeersFound(object sender, PeersAddedEventArgs e)
		{
			Console.WriteLine($"Found {e.NewPeers} new peers and {e.ExistingPeers} existing peers");
		}

		static void Main(string[] args) { }
	}
}
