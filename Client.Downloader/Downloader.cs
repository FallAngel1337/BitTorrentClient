using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;

using MonoTorrent;
using MonoTorrent.Client;

using BencodeNET.Parsing;
using BencodeNET.Objects;

using System.IO;

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
		public TorrentParser Parser;

		private readonly DownloaderConfig Config;
		private readonly ClientEngine Engine;

		private Torrent TorrentFile { get; set; }
		private TorrentManager Manager { get; set; }

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

		public async Task StartDownloadAsync(IProgress<int> download_progress)
		{
			if (!Ready)
			{
				throw new Exception("Torrent setup isn't ready yet");
			}
			
			if (Config.Verbose)
			{
				Manager.PeerConnected += (o, e) => Console.WriteLine($"Connection succeeded: {e.Peer.Uri}");
				Manager.ConnectionAttemptFailed += (o, e) => Console.WriteLine($"Connection failed: {e.Peer.ConnectionUri} - {e.Reason} - {e.Peer}");
				Manager.PeersFound += Manager_PeersFound;
			}

			await Manager.StartAsync();

			while (Engine.IsRunning)
			{
				foreach (TorrentManager manager in Engine.Torrents)
				{
					download_progress.Report((int)manager.Monitor.DataBytesDownloaded);
					System.Threading.Thread.Sleep(100);
				}
			}
		}

		public async Task SetupDownload(string torrent)
		{
			TorrentPath = torrent;
			
			Parser = new(TorrentPath);
			Parser.Parse();

			Parser.ShowParsed();
			Console.WriteLine($"\nDestination folder: { Config.DownloadPath }");

			Console.Write("\nDo you want to proceed with the download?[Y/N] ");
			string input = Console.ReadLine().Trim().ToUpper();
			if (!input.StartsWith("Y"))
			{
				Console.WriteLine("Stopping Download ...");
				Ready = false;
				return;
			}

			TorrentFile = await Torrent.LoadAsync(TorrentPath);
			Manager = await Engine.AddAsync(TorrentPath, Config.DownloadPath);

			Ready = true;
		}

		static void Manager_PeersFound(object sender, PeersAddedEventArgs e)
		{
			Console.WriteLine($"Found {e.NewPeers} new peers and {e.ExistingPeers} existing peers");
		}

		static void Main(string[] args) { }
	}

	public class TorrentParser
	{
		private static readonly BencodeParser Parser = new();

		public readonly string TorrentFile;

		public string FileName { get; private set; }
		public long FileSize { get; private set; }

		private DateTimeOffset dateTimeOffset { get; set; }
		public DateTime Date { get; private set; }
		
		public string Comment { get; private set; }
		public string Author { get; private set; }
		
		private byte[] Hash { get; set; }
		public string HashString { get; private set; }

		public TorrentParser(string file) => TorrentFile = file;

		public void Parse()
		{
			BDictionary parsed = Parser.Parse<BDictionary>(File.ReadAllBytes(TorrentFile));

			Comment = parsed["comment"].ToString();
			Author = parsed["created by"].ToString();
			dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(parsed["creation date"].ToString()));
			Date = dateTimeOffset.DateTime;

			// File information
			FileName = ((BDictionary)parsed["info"])["name"].ToString();
			FileSize = Convert.ToInt64(((BDictionary)parsed["info"])["length"].ToString());
			Hash = SHA1.Create().ComputeHash(((BDictionary)parsed["info"]).EncodeAsBytes());
			HashString = ByteArrayToString(Hash);
		}

		public void ShowParsed()
		{
			Console.WriteLine("\nTorrent Information:");
			Console.WriteLine($"File: { TorrentFile }");
			Console.WriteLine($"Comment: { Comment }\nAuthor: { Author }\nDate: { Date }");
			Console.WriteLine($"FileName: { FileName }");
			Console.WriteLine($"FileSize: { FileSize }");
			Console.WriteLine($"Hash: 0x{ HashString }");
		}

			private void GetHashes(byte[] bytes)
		{
			Hash = SHA1.Create().ComputeHash(bytes);
			HashString = ByteArrayToString(Hash);
		}

		private static string ByteArrayToString(byte[] arrInput)
		{
			StringBuilder sOutput = new StringBuilder(arrInput.Length);
			for (int i = 0; i < arrInput.Length; i++)
			{
				sOutput.Append(arrInput[i].ToString("x2"));
			}
			return sOutput.ToString();
		}

	}
}
