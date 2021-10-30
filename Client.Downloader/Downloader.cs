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

		public DownloaderConfig Config { get; private set; }
		private ClientEngine Engine { get; set; }

		private Torrent TorrentFile { get; set; }
		private TorrentManager Manager { get; set; }

		public string TorrentPath { get; private set; }

		public int Num { get; private set; }
		public string TotalSize { get; private set; }
		public long TotalBytesSize { get; private set; }

		static readonly string[] SizeSuffixes =
				  { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		public async Task InitDownloader(string torrent, DownloaderConfig? config = null)
		{
			TorrentPath = torrent;
			Parser = new(TorrentPath);
			Parser.Parse();

			Config = config ?? new(Environment.CurrentDirectory, false, new Settings());
			Engine = new(Config.Settings.engineSettings.ToSettings());

			TorrentFile = await Torrent.LoadAsync(TorrentPath);
			Manager = await Engine.AddAsync(TorrentFile, Config.DownloadPath);

			TotalBytesSize = Manager.Torrent.Size;
			TotalSize = SizeSuffix(TotalBytesSize);
			Num = Manager.Torrent.Files.Count;
		}

		public async Task StartDownloadAsync(IProgress<int> download_progress)
		{
			if (Config.Verbose)
			{
				Manager.PeerConnected += (o, e) => Console.WriteLine($"Connection succeeded: {e.Peer.Uri}");
				Manager.ConnectionAttemptFailed += (o, e) => Console.WriteLine($"Connection failed: {e.Peer.ConnectionUri} - {e.Reason} - {e.Peer}");
				Manager.PeersFound += Manager_PeersFound;
			}

			await Manager.StartAsync();

			while (Engine.IsRunning)
			{
				if (Manager.Monitor.DataBytesDownloaded >= Manager.Torrent.Size)
				{
					return;
				}

				download_progress.Report((int)Manager.Monitor.DataBytesDownloaded);
				System.Threading.Thread.Sleep(100);
			}
		}

		static void Manager_PeersFound(object sender, PeersAddedEventArgs e)
		{
			Console.WriteLine($"Found {e.NewPeers} new peers and {e.ExistingPeers} existing peers");
		}

		// https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
		public static string SizeSuffix(long value, int decimalPlaces = 1)
		{
			if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }

			int i = 0;
			decimal dValue = (decimal)value;
			while (Math.Round(dValue, decimalPlaces) >= 1000)
			{
				dValue /= 1024;
				i++;
			}

			return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
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

			try
			{
				foreach (BDictionary file in (BList)((BDictionary)parsed["info"])["files"])
				{
					foreach (var i in (BList)file["path"])
					{
						Files[i.ToString()] = Convert.ToInt64(file["length"].ToString());
					}
				}
			}
			catch (NullReferenceException)
			{
				Files[((BDictionary)parsed["info"])["name"].ToString()] = Convert.ToInt64(((BDictionary)parsed["info"])["length"].ToString());
			}
		}

		public void ShowParsed()
		{
			Console.WriteLine("\nTorrent Information:");
			Console.WriteLine($"Torrent: { TorrentFile }");
			Console.WriteLine($"Comment: { Comment }\nAuthor: { Author }\nDate: { Date }");
			Console.WriteLine($"Hash: 0x{ HashString }");
			Console.WriteLine($"File(s):");
			foreach (var file in Files)
			{
				Console.WriteLine($"{ file.Key }  { TorrentDownloader.SizeSuffix(file.Value) }");
			}
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
