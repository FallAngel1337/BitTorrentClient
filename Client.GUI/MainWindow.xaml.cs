using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

using Client.Downloader;

namespace Client.GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<TorrentParser> torrentParsers = new();

		public MainWindow()
		{
			InitializeComponent();
		}

		private async void OpenFileOption_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog();
			var result = openFileDlg.ShowDialog();
			informationDisplay.ItemsSource = torrentParsers;

			if (result.ToString() != string.Empty)
			{
				try
				{
					var parsed = new TorrentParser(openFileDlg.FileName);
					parsed.Parse();

					torrentParsers.Add(parsed);
					informationDisplay.Items.Refresh();

					pbStatus.Maximum = parsed.FileSize;

					var progress = new Progress<int>(total => pbStatus.Value = total);

					await Task.Run(() => StartDownloadAsync(openFileDlg.FileName, progress));
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show($"Got an ERROR while downloading! { ex }");
				}
			}
		}

		private async Task StartDownloadAsync(string torrent, IProgress<int> progress)
		{

			Settings settings = new(SettingsOpts.AllowPortForwarding | SettingsOpts.AutoSaveLoadDhtCache |
						         SettingsOpts.AutoSaveLoadFastResume | SettingsOpts.AutoSaveLoadMagnetLinkMetadata);

			DownloaderConfig config = new(Environment.CurrentDirectory, false, settings);
			TorrentDownloader downloader = new(torrent, config);

			try
			{
				await downloader.StartDownloadAsync(progress);
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show($"ERROR :: { ex.Message } | { ex }");
			}

			System.Windows.MessageBox.Show("Download Done!");
			pbStatus.Value = 0;
		}

		private void OpenMagnetLinkOption_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.MessageBox.Show("Opening magnetlink!");
		}

		private void ExitOption_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void TorrentCreatorOption_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.MessageBox.Show("Torrent creator!");
		}

		private void TorrentParserOption_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.MessageBox.Show("Torrent parser!");
		}

		private void InformationButton_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.MessageBox.Show("Showing some information");
		}
		
		private void TrackersButton_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.MessageBox.Show("Listing the trackers");
		}
	}
}
