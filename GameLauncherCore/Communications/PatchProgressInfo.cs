namespace GameLauncherCore
{
	public interface IOperationProgress
	{
		int Percentage { get; }
		string ProgressInfo { get; }
		string ProgressNoFileInfo { get; }
	}

	public class FilePatchProgress : IOperationProgress, Octodiff.Diagnostics.IProgressReporter
	{
		private readonly PatchManager comms;

		public int Percentage { get; private set; }
		public string ProgressInfo { get; private set; }
		public string ProgressNoFileInfo { get; private set; }

		internal FilePatchProgress( PatchManager comms, string filename )
		{
			this.comms = comms;

			Percentage = 0;
			ProgressInfo = Localization.Get( LocalizationID.UpdatingX, filename );

			comms.SetProgress( this );
		}

		public void ReportProgress( string operation, long currentPosition, long total )
		{
			Percentage = (int) ( (double) currentPosition / total * 100.0 + 0.5 );
			comms.SetProgress( this );
		}
	}

	public class DownloadProgress : IOperationProgress
	{
		private readonly PatchManager comms;
		private readonly IDownloadHandler downloadHandler;

		public string Filename { get { return downloadHandler.DownloadedFilename; } }
		public long Speed { get; private set; }

		public long FileDownloadedSize { get; private set; }
		public long FileTotalSize { get { return downloadHandler.DownloadedFileSize; } }

		public string SpeedPretty
		{
			get
			{
				if( Speed < 1048576L ) // 1MB
					return Speed.ToKilobytes() + "KB/s";
				else
					return Speed.ToMegabytes() + "MB/s";
			}
		}

		public int Percentage { get { return FileTotalSize > 0L ? (int) ( ( (double) FileDownloadedSize / FileTotalSize ) * 100 ) : 0; } }
		public string ProgressInfo { get { return Localization.Get( LocalizationID.DownloadingXProgressInfo, Filename, FileDownloadedSize.ToMegabytes(), FileTotalSize.ToMegabytes(), SpeedPretty ); } }

		public string ProgressNoFileInfo { get { 
				return string.Format("{0}MB/{1}MB ({2})", FileDownloadedSize.ToMegabytes(), FileTotalSize.ToMegabytes(), SpeedPretty ); 
		} }
		internal DownloadProgress( PatchManager comms, IDownloadHandler downloadHandler )
		{
			this.comms = comms;
			this.downloadHandler = downloadHandler;

			comms.SetProgress( this );
		}

		internal void UpdateValues( long speed, long fileDownloadedSize )
		{
			Speed = speed;
			FileDownloadedSize = fileDownloadedSize;

			comms.SetProgress( this );
		}
	}
}