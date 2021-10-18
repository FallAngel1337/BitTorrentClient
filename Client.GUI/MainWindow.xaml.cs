using System;
using System.Collections.Generic;
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

namespace Client.GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void OpenFileOption_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Opening file!");
		}

		private void OpenMagnetLinkOption_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Opening magnetlink!");
		}

		private void ExitOption_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void TorrentCreatorOption_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Torrent creator!");
		}

		private void TorrentParserOption_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Torrent parser!");
		}

		private void InformationButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Showing some information");
		}
		
		private void TrackersButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Listing the trackers");
		}
	}
}
