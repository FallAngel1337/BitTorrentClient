using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Downloader
{
	public class DownloadInfo
	{
		public string FileName { get; set; }
		public long Size { get; set; }
		public double Progress { get; set; }
		public double DownloadSpeed { get; set; }
		public double UploadSpeed { get; set; }
	}
}
