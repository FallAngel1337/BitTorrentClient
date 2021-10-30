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

		public readonly DownloaderConfig Config;
		private readonly ClientEngine Engine;

		private Torrent TorrentFile { get; set; }
		private TorrentManager Manager { get; set; }

		public readonly string TorrentPath;

		public TorrentDownloader(string torrent)
		{
			TorrentPath = torrent;
			Parser = new(TorrentPath);
			Parser.Parse();

			Config = new(Environment.CurrentDirectory, false, new Settings());
			Engine = new(Config.Settings.engineSettings.ToSettings());
		}

		public TorrentDownloader(string torrent, DownloaderConfig config)
		{
			TorrentPath = torrent;
			Parser = new(TorrentPath);
			Parser.Parse();

			Config = config;
			Engine = new(Config.Settings.engineSettings.ToSettings());
		}

		public async Task StartDownloadAsync(IProgress<int> download_progress)
		{
			TorrentFile = await Torrent.LoadAsync(TorrentPath);
			Manager = await Engine.AddAsync(TorrentFile, Config.DownloadPath);

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
					// Console.WriteLine($"==> { manager.Monitor.DataBytesDownloaded } / { Parser.FileSize } >> { Parser.FileSize - manager.Monitor.DataBytesDownloaded }");
					// Console.Clear();

					if (manager.Monitor.DataBytesDownloaded >= Parser.FileSize)
					{
						return;
					}

					System.Threading.Thread.Sleep(100);
					download_progress.Report((int)manager.Monitor.DataBytesDownloaded);

				}
			}
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

		// Torrent information
		public string Comment { get; private set; }
		public string Author { get; private set; }

		private DateTimeOffset dateTimeOffset { get; set; }
		public DateTime Date { get; private set; }

		private byte[] Hash { get; set; }
		public string HashString { get; private set; }

		public readonly Dictionary<string, long> Files = new();

		public TorrentParser(string file)
		{
			TorrentFile = file;
		}

		public void Parse()
		{
			BDictionary parsed = Parser.Parse<BDictionary>(File.ReadAllBytes(TorrentFile));

			Comment = parsed["comment"]?.ToString();
			Author = parsed["created by"]?.ToString();
			dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(parsed["creation date"]?.ToString()));
			Date = dateTimeOffset.DateTime;
			Hash = SHA1.Create().ComputeHash(((BDictionary)parsed["info"]).EncodeAsBytes());
			HashString = ByteArrayToString(Hash);
				
			foreach (BDictionary file in (BList)((BDictionary)parsed["info"])["files"])
			{
				foreach (var i in (BList)file["path"])
				{
					Files[i.ToString()] = Convert.ToInt64(file["length"].ToString());
				}
			}
		}

		public void ShowParsed()
		{
			Console.WriteLine("\nTorrent Information:");
			Console.WriteLine($"Torrent: { TorrentFile }");
			Console.WriteLine($"Comment: { Comment }\nAuthor: { Author }\nDate: { Date }");
			Console.WriteLine($"Hash: 0x{ HashString }");
			Console.WriteLine($"File(s) | Size ");
			foreach (var file in Files)
			{
				Console.WriteLine($"{ file.Key } | { file.Value }");
			}
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
