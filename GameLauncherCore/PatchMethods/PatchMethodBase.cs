﻿namespace GameLauncherCore
{
	internal abstract class PatchMethodBase : IOperationProgress, IDownloadListener
	{
		protected readonly PatchManager comms;

		private const int FILE_OPERATIONS_PROGRESS_CONTRIBUTION = 50;
		private const int DOWNLOAD_SIZE_PROGRESS_CONTRIBUTION = 50;

		private float fileOperationsMultiplier;
		private double downloadSizeMultiplier;

		private int fileOperationsContribution;
		private int downloadSizeContribution;

		private int completedFileOperations;
		private long downloadedBytes;

		public int Percentage
		{
			get
			{
				int fileOpPercentage, downloadPercentage;
				if( fileOperationsMultiplier > 0f )
				{
					fileOpPercentage = (int) ( completedFileOperations * fileOperationsMultiplier + 0.5f );
					if( fileOpPercentage > fileOperationsContribution )
						fileOpPercentage = fileOperationsContribution;
				}
				else
					fileOpPercentage = fileOperationsContribution;

				if( downloadSizeMultiplier > 0 )
				{
					downloadPercentage = (int) ( downloadedBytes * downloadSizeMultiplier + 0.5 );
					if( downloadPercentage > downloadSizeContribution )
						downloadPercentage = downloadSizeContribution;
				}
				else
					downloadPercentage = downloadSizeContribution;

				return fileOpPercentage + downloadPercentage;
			}
		}

		public string ProgressInfo
		{
			get
			{
				if( this is RepairPatchApplier )
					return Localization.Get( LocalizationID.ApplyingRepairPatch );
				else if( this is IncrementalPatchApplier )
					return Localization.Get( LocalizationID.ApplyingIncrementalPatch );
				else
					return Localization.Get( LocalizationID.ApplyingInstallerPatch );
			}
		}

        public string ProgressNoFileInfo {
            get {
                if (this is RepairPatchApplier)
                    return Localization.Get (LocalizationID.ApplyingRepairPatch);
                else if (this is IncrementalPatchApplier)
                    return Localization.Get (LocalizationID.ApplyingIncrementalPatch);
                else
                    return Localization.Get (LocalizationID.ApplyingInstallerPatch);
            }
        }

        protected PatchMethodBase( PatchManager comms )
		{
			this.comms = comms;
		}

		public PatchResult Run()
		{
			PatchResult result = Execute();

			if( comms.LogProgress )
			{
				if( fileOperationsMultiplier > 0f )
					completedFileOperations = (int) ( 150f / fileOperationsMultiplier );
				else
					completedFileOperations = 0;

				if( downloadSizeMultiplier > 0 )
					downloadedBytes = (long) ( 150 / downloadSizeMultiplier );
				else
					downloadedBytes = 0;

				comms.SetOverallProgress( this );
			}

			return result;
		}

		protected abstract PatchResult Execute();

		protected void InitializeProgress( int numberOfFileOperations, long expectedDownloadSize )
		{
			if( !comms.LogProgress )
				return;

			completedFileOperations = 0;
			downloadedBytes = 0L;

			if( numberOfFileOperations == 0 )
			{
				fileOperationsContribution = 0;
				downloadSizeContribution = 100;
			}
			else if( expectedDownloadSize == 0 )
			{
				fileOperationsContribution = 100;
				downloadSizeContribution = 0;
			}
			else
			{
				fileOperationsContribution = FILE_OPERATIONS_PROGRESS_CONTRIBUTION;
				downloadSizeContribution = DOWNLOAD_SIZE_PROGRESS_CONTRIBUTION;
			}

			if( numberOfFileOperations > 0 )
				fileOperationsMultiplier = (float) fileOperationsContribution / numberOfFileOperations;
			else
				fileOperationsMultiplier = 0f;

			if( expectedDownloadSize > 0 )
				downloadSizeMultiplier = (double) downloadSizeContribution / expectedDownloadSize;
			else
				downloadSizeMultiplier = 0;

			comms.DownloadManager.SetListener( this );
			comms.SetOverallProgress( this );
		}

		protected void ReportProgress( int filesProcessed, long bytesDownloaded )
		{
			completedFileOperations += filesProcessed;
			downloadedBytes += bytesDownloaded;

			comms.SetOverallProgress( this );
		}

		public void DownloadedBytes( long bytes )
		{
			ReportProgress( 0, bytes );
		}
	}
}