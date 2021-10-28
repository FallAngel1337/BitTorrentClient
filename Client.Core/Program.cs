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
			TorrentDownloader downloader = new(config);

			await downloader.SetupDownload(opts.Torrent);

			var progressbar = new ProgressBar((int)downloader.Parser.FileSize);
			var progress = new Progress<int>((percent) => progressbar.Refresh(percent, "#"));

			try
			{
				var watch = System.Diagnostics.Stopwatch.StartNew();
				
				await downloader.StartDownloadAsync(progress);
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
