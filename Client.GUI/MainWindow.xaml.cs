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
		private readonly List<DownloadInfo> DownloadInfos = new();

		public MainWindow()
		{
			InitializeComponent();
		}

		private async void OpenFileOption_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog();
			var result = openFileDlg.ShowDialog();
			var progress = new Progress<DownloadInfo[]>((a) => informationDisplay.ItemsSource = a);

			if (result.ToString() != string.Empty)
			{
				try
				{
					await Task.Factory.StartNew(() => StartGUIDownloadAsync(openFileDlg.FileName, progress), TaskCreationOptions.LongRunning);
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show($"Got an ERROR while downloading! { ex }");
				}
			}

		}

		private async Task StartGUIDownloadAsync(string torrent, IProgress<DownloadInfo[]> progress)
		{

			TorrentDownloader downloader = new();
			await downloader.SetupDownload(torrent);

			await downloader.Manager.StartAsync();

			DownloadInfos.Add(new DownloadInfo
			{
				FileName = downloader.TorrentFile.Name,
				Size = downloader.TorrentFile.Size,
				Progress = 0, //  stuck in 0% for testing
				DownloadSpeed = 0,
				UploadSpeed = 0
			});

			// TODO: IMPROVE THIS ASAP (but it's working =])
			int i = 0;
			while (downloader.Engine.IsRunning)
			{
				DownloadInfos[i % DownloadInfos.Count].DownloadSpeed = downloader.Engine.TotalDownloadSpeed / 1024;
				DownloadInfos[i++ % DownloadInfos.Count].UploadSpeed = downloader.Engine.TotalUploadSpeed / 1024;
				
				Task.Delay(1000).Wait();
				progress.Report(DownloadInfos.ToArray());
			}
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
