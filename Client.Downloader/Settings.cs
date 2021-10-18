using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonoTorrent;
using MonoTorrent.Client;

namespace Client.Downloader
{
	[Flags]
	public enum SettingsOpts
	{
		AllowPortForwarding = 1,
		AutoSaveLoadDhtCache = 2,
		AutoSaveLoadFastResume = 4,
		AutoSaveLoadMagnetLinkMetadata = 8,
	}

	public class Settings
	{
		public readonly bool AllowPortForwarding;
		public readonly bool AutoSaveLoadDhtCache;
		public readonly bool AutoSaveLoadFastResume;
		public readonly bool AutoSaveLoadMagnetLinkMetadata;

		public int Port 
		{ 
			get => Port;
			set
			{
				if (value > 0 && value < 65536)
				{
					Port = value;
				}
			}
		}

		public int DhtPort 
		{
			get => DhtPort;
			set
			{
				if (value > 0 && value < 65536)
				{
					DhtPort = value;
				}
			}
		}

		public readonly EngineSettingsBuilder engineSettings;

		public Settings()
		{
			AllowPortForwarding = true;
			AutoSaveLoadDhtCache = true;
			AutoSaveLoadFastResume = true;
			AutoSaveLoadMagnetLinkMetadata = true;

			engineSettings = new EngineSettingsBuilder
			{
				AllowPortForwarding = AllowPortForwarding,
				AutoSaveLoadDhtCache = AutoSaveLoadDhtCache,
				AutoSaveLoadFastResume = AutoSaveLoadFastResume,
				AutoSaveLoadMagnetLinkMetadata = AutoSaveLoadMagnetLinkMetadata,
			};
		}

		public Settings(SettingsOpts opts)
		{
			if ((opts & SettingsOpts.AllowPortForwarding) == SettingsOpts.AllowPortForwarding)
			{
				AllowPortForwarding = true;
			}

			if ((opts & SettingsOpts.AutoSaveLoadDhtCache) == SettingsOpts.AutoSaveLoadDhtCache)
			{
				AutoSaveLoadDhtCache = true;
			}

			if ((opts & SettingsOpts.AutoSaveLoadFastResume) == SettingsOpts.AutoSaveLoadFastResume)
			{
				AutoSaveLoadFastResume = true;
			}

			if ((opts & SettingsOpts.AutoSaveLoadMagnetLinkMetadata) == SettingsOpts.AutoSaveLoadMagnetLinkMetadata)
			{
				AutoSaveLoadMagnetLinkMetadata = true;
			}

			engineSettings = new EngineSettingsBuilder
			{
				AllowPortForwarding = AllowPortForwarding,
				AutoSaveLoadDhtCache = AutoSaveLoadDhtCache,
				AutoSaveLoadFastResume = AutoSaveLoadFastResume,
				AutoSaveLoadMagnetLinkMetadata = AutoSaveLoadMagnetLinkMetadata,
			};
		}

		public void ShowSettings()
		{
			Console.WriteLine($"AllowPortForwarding: { AllowPortForwarding }");
			Console.WriteLine($"AutoSaveLoadDhtCache: { AutoSaveLoadDhtCache }");
			Console.WriteLine($"AutoSaveLoadFastResume: { AutoSaveLoadFastResume }");
			Console.WriteLine($"AutoSaveLoadMagnetLinkMetadata: { AutoSaveLoadMagnetLinkMetadata }");
		}
	}
}
