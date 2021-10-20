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

			if (result.ToString() != string.Empty)
			{
				try
				{
					await StartGUIDownloadAsync(openFileDlg.FileName);
					informationDisplay.ItemsSource = DownloadInfos.ToArray();
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show($"Got an ERROR while downloading! { ex }");
				}
			}

		}

		private async Task StartGUIDownloadAsync(string torrent_file)
		{
			TorrentDownloader torrentDownloader = new();
			await torrentDownloader.SetupDownload(torrent_file);

			await torrentDownloader.Manager.StartAsync();
			DownloadInfos.Add(new DownloadInfo
			{
				FileName = torrentDownloader.TorrentFile.Name,
				Size = torrentDownloader.TorrentFile.Size,
				Progress = 0, //  stuck in 0% for testing
				DownloadSpeed = torrentDownloader.Engine.TotalDownloadSpeed / 1024,
				UploadSpeed = torrentDownloader.Engine.TotalUploadSpeed / 1024
			});

			System.Windows.MessageBox.Show("WAIT!");

			//while (torrentDownloader.Engine.IsRunning)
			//{
			//	// wait till it's over
			//}
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
