using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Logging;

using CommandLine;
using Error = CommandLine.Error;

using Konsole;

using Client.Downloader;
using System.Collections.Concurrent;

namespace Client.Core
{
	public class Options
	{
		[Option('v', "verbose", Required = false, HelpText = "Enable verbose mode")]
		public bool Verbose { get; set; } = false;

		[Option('l', "load", Required = true, HelpText = "The torrent file/link/hash")]
		public string Torrent { get; set; }

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
			Settings settings = new(SettingsOpts.AllowPortForwarding | SettingsOpts.AutoSaveLoadDhtCache |
				                    SettingsOpts.AutoSaveLoadFastResume | SettingsOpts.AutoSaveLoadMagnetLinkMetadata);

			DownloaderConfig  config = new(opts.Download, opts.Verbose, settings);
			TorrentDownloader downloader = new();

			await downloader.InitDownloader(opts.Torrent, config);

			downloader.Parser.ShowParsed();
			Console.WriteLine($"\nDestination folder: { downloader.Config.DownloadPath }");
			Console.WriteLine($"Total Space Required: { downloader.TotalSize }");
			Console.Write("\nDo you want to proceed with the download?[Y/N] ");
			string input = Console.ReadLine().Trim().ToUpper();
			if (!input.StartsWith("Y"))
			{
				Console.WriteLine("Stopping Download ...");
				return;
			}

			var tasks = new List<Task>();
			var bars = new ConcurrentBag<ProgressBar>();

			try
			{
				var watch = System.Diagnostics.Stopwatch.StartNew();
				var progressbar = new ProgressBar((int)downloader.TotalBytesSize);
				var progress = new Progress<int>((percent) => progressbar.Refresh(percent, "#"));

				await downloader.StartDownloadAsync(progress);

				Task.WaitAll(tasks.ToArray());
				watch.Stop();
				
				var elapsed = DateTimeOffset.FromUnixTimeMilliseconds(watch.ElapsedMilliseconds).DateTime;
				Console.WriteLine($"Total Download Time: { elapsed.ToString("HH:mm:ss") }");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Download CANCELLED :: { ex.Message }");
			}

		}

		static void HandleNotParsed(IEnumerable<Error> error)
		{
			Console.WriteLine("Could not parse the arguments!");
		}
	}
}
