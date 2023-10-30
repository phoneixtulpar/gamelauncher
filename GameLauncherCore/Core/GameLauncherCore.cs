using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace GameLauncherCore
{
	public enum PatchOperation { CheckingForUpdates, Patching, SelfPatching, ApplyingSelfPatch }
	public enum PatchMethod { None, RepairPatch, IncrementalPatch, InstallerPatch }
	public enum PatchResult { Failed, Success, AlreadyUpToDate }
	public enum PatchStage
	{
		CheckingUpdates, CheckingFileIntegrity, DeletingObsoleteFiles,
		DownloadingFiles, ExtractingFilesFromArchive, VerifyingFilesOnServer,
		CalculatingFilesToUpdate, UpdatingFiles
	}

	public enum MaintenanceCheckResult { NoMaintenance, Maintenance_AbortApp, Maintenance_CanLaunchApp }

	public enum PatchFailReason
	{
		None, Cancelled, Unknown, FatalException,
		InsufficientSpace, RequiresAdminPriviledges, MultipleRunningInstances,
		NoSuitablePatchMethodFound, FilesAreNotUpToDateAfterPatch,
		UnderMaintenance_AbortApp, UnderMaintenance_CanLaunchApp,
		DownloadError, CorruptDownloadError,
		FileDoesNotExistOnServer, FileIsNotValidOnServer,
		XmlDeserializeError, InvalidVersionCode,
		CantVerifyVersionInfo, CantVerifyPatchInfo,
		SelfPatcherNotFound
	}

	public delegate IDownloadHandler DownloadHandlerFactory();
	public delegate long FreeDiskSpaceCalculator( string drive );
	public delegate bool XMLVerifier( ref string xmlContents );

	public class GameLauncherPatcher
	{
		public interface IListener
		{
			bool ReceiveLogs { get; }
			bool ReceiveProgress { get; }

			void Started();
			void LogReceived( string log );
			void ProgressChanged( IOperationProgress progress );
			void OverallProgressChanged( IOperationProgress progress );
			void PatchStageChanged( PatchStage stage );
			void PatchMethodChanged( PatchMethod method );
			void VersionInfoFetched( VersionInfo versionInfo );
			void VersionFetched( string currentVersion, string newVersion );
			void Finished();
		}

		private struct PatchMethodHolder
		{
			public readonly PatchMethod method;
			public readonly long size;

			public PatchMethodHolder( PatchMethod method, long size )
			{
				this.method = method;
				this.size = size;
			}
		}

		private readonly string[] ROOT_PATH_PLACEHOLDERS = new string[] { "{ROOT_PATH}", "{APPLICATION_DIRECTORY}" };

		private readonly PatchManager manager;
		private readonly string versionInfoURL;

		private VersionCode currentVersion;

		private bool canRepairPatch;
		private bool canIncrementalPatch;
		private bool canInstallerPatch;

		private bool checkForMultipleRunningInstances;

		private DownloadHandlerFactory downloadHandlerFactory;
		private FreeDiskSpaceCalculator freeDiskSpaceCalculator;
		private XMLVerifier versionInfoVerifier;
		private XMLVerifier patchInfoVerifier;

		private readonly List<IncrementalPatch> incrementalPatches;
		private readonly List<IncrementalPatchInfo> incrementalPatchesInfo;
		private readonly HashSet<string> filesInVersion;

		public bool IsRunning { get; private set; }
		public PatchOperation Operation { get; private set; }
		public PatchMethod PatchMethod { get; private set; }
		public PatchResult Result { get; private set; }

		public string NewVersion { get { return manager.VersionInfo != null ? manager.VersionInfo.Version : null; } }

		public PatchStage PatchStage
		{
			get { return manager.Stage; }
			private set { manager.Stage = value; }
		}

		public PatchFailReason FailReason
		{
			get
			{
				if( IsRunning || Result != PatchResult.Failed )
					return PatchFailReason.None;

				if( manager.Cancel )
					return PatchFailReason.Cancelled;

				return manager.FailReason;
			}
			private set { manager.FailReason = value; }
		}

		public string FailDetails
		{
			get
			{
				if( IsRunning || Result != PatchResult.Failed )
					return null;

				if( manager.Cancel )
					return Localization.Get( LocalizationID.Cancelled );

				return manager.FailDetails;
			}
			private set { manager.FailDetails = value; }
		}

		/// <exception cref = "ArgumentException">An argument is empty</exception>
		public GameLauncherPatcher( string rootPath, string versionInfoURL )
		{
			rootPath = rootPath.Trim();
			versionInfoURL = versionInfoURL.Trim();

			if( string.IsNullOrEmpty( rootPath ) )
				throw new ArgumentException( Localization.Get( LocalizationID.E_XCanNotBeEmpty, "'rootPath'" ) );

			if( string.IsNullOrEmpty( versionInfoURL ) )
				throw new ArgumentException( Localization.Get( LocalizationID.E_XCanNotBeEmpty, "'versionInfoURL'" ) );

			Localization.Get( LocalizationID.Done ); // Force the localization system to be initialized with the current culture/language

			for( int i = 0; i < ROOT_PATH_PLACEHOLDERS.Length; i++ )
			{
				if( rootPath.IndexOf( ROOT_PATH_PLACEHOLDERS[i] ) >= 0 )
					rootPath.Replace( ROOT_PATH_PLACEHOLDERS[i], Path.GetDirectoryName( PatchUtils.GetCurrentExecutablePath() ) );
			}

			manager = new PatchManager( this, PatchUtils.GetPathWithTrailingSeparatorChar( rootPath ) );
			this.versionInfoURL = versionInfoURL;

			canRepairPatch = true;
			canIncrementalPatch = true;
			canInstallerPatch = true;

			checkForMultipleRunningInstances = true;

			incrementalPatches = new List<IncrementalPatch>();
			incrementalPatchesInfo = new List<IncrementalPatchInfo>();
			filesInVersion = new HashSet<string>();

			UseCustomDownloadHandler( null );
			UseCustomFreeSpaceCalculator( null );

			IsRunning = false;
			Operation = PatchOperation.CheckingForUpdates;
			PatchMethod = PatchMethod.None;
			Result = PatchResult.Failed;
		}

		public GameLauncherPatcher SetListener( IListener listener )
		{
			manager.Listener = listener;
			if( IsRunning )
				manager.ListenerCallStarted();

			return this;
		}

		public GameLauncherPatcher UseRepairPatch( bool canRepairPatch )
		{
			this.canRepairPatch = canRepairPatch;
			return this;
		}

		public GameLauncherPatcher UseIncrementalPatch( bool canIncrementalPatch )
		{
			this.canIncrementalPatch = canIncrementalPatch;
			return this;
		}

		public GameLauncherPatcher UseInstallerPatch( bool canInstallerPatch )
		{
			this.canInstallerPatch = canInstallerPatch;
			return this;
		}

		public GameLauncherPatcher CheckForMultipleRunningInstances( bool checkForMultipleRunningInstances )
		{
			this.checkForMultipleRunningInstances = checkForMultipleRunningInstances;
			return this;
		}

		public GameLauncherPatcher VerifyFilesOnServer( bool verifyFiles )
		{
			manager.VerifyFiles = verifyFiles;
			return this;
		}

		public GameLauncherPatcher UseCustomDownloadHandler( DownloadHandlerFactory factoryFunction )
		{
			if( !IsRunning )
			{
				if( factoryFunction == null )
					factoryFunction = () => new CookieAwareWebClient(); // Default WebClient based download handler

				downloadHandlerFactory = factoryFunction;
				manager.DownloadManager.SetDownloadHandler( factoryFunction() );
			}

			return this;
		}

		public GameLauncherPatcher UseCustomFreeSpaceCalculator( FreeDiskSpaceCalculator freeSpaceCalculatorFunction )
		{
			if( !IsRunning )
			{
				if( freeSpaceCalculatorFunction == null )
					freeSpaceCalculatorFunction = ( drive ) => new DriveInfo( drive ).AvailableFreeSpace;

				freeDiskSpaceCalculator = freeSpaceCalculatorFunction;
			}

			return this;
		}

		public GameLauncherPatcher UseVersionInfoVerifier( XMLVerifier verifierFunction )
		{
			versionInfoVerifier = verifierFunction;
			return this;
		}

		public GameLauncherPatcher UsePatchInfoVerifier( XMLVerifier verifierFunction )
		{
			patchInfoVerifier = verifierFunction;
			return this;
		}

		public GameLauncherPatcher LogProgress( bool value )
		{
			manager.LogProgress = value;
			return this;
		}

		public GameLauncherPatcher LogToFile( bool value )
		{
			manager.FileLogging = value;
			return this;
		}

		public GameLauncherPatcher SilentMode( bool silent )
		{
			manager.SilentMode = silent;
			return this;
		}

		public void Cancel()
		{
			if( IsRunning )
				manager.Cancel = true;
		}

		public string FetchLog()
		{
			return manager.FetchLog();
		}

		public IOperationProgress FetchProgress()
		{
			return manager.FetchProgress();
		}

		public IOperationProgress FetchOverallProgress()
		{
			return manager.FetchOverallProgress();
		}

		public bool CheckForUpdates( bool checkVersionOnly = true )
		{
			if( IsRunning )
				return false;

			IsRunning = true;
			Operation = PatchOperation.CheckingForUpdates;
			PatchMethod = PatchMethod.None;
			manager.Cancel = false;

			PatchUtils.CreateBackgroundThread( new ParameterizedThreadStart( ThreadCheckForUpdatesFunction ) ).Start( checkVersionOnly );
			return true;
		}

		public bool Run( bool selfPatching )
		{
			if( IsRunning )
				return false;

			IsRunning = true;
			Operation = selfPatching ? PatchOperation.SelfPatching : PatchOperation.Patching;
			PatchMethod = PatchMethod.None;
			manager.Cancel = false;

			PatchUtils.CreateBackgroundThread( new ThreadStart( ThreadPatchFunction ) ).Start();
			return true;
		}

		// For self-patching applications only - should be called after Run(true) returns PatchResult.Success
		// Starts specified self patcher executable with required parameters
		public bool ApplySelfPatch( string selfPatcherExecutable, string postSelfPatchExecutable = null )
		{
			if( IsRunning )
				return false;

			IsRunning = true;
			Operation = PatchOperation.ApplyingSelfPatch;

			manager.InitializeFileLogger();
			manager.ListenerCallStarted();

			try
			{
				selfPatcherExecutable = selfPatcherExecutable.Trim();
				if( postSelfPatchExecutable != null )
					postSelfPatchExecutable = postSelfPatchExecutable.Trim();

				if( !File.Exists( selfPatcherExecutable ) )
				{
					Result = PatchResult.Failed;
					FailReason = PatchFailReason.SelfPatcherNotFound;
					FailDetails = Localization.Get( LocalizationID.E_SelfPatcherDoesNotExist );

					manager.Log( manager.FailDetails );
					return false;
				}

				string instructionsPath = manager.CachePath + PatchParameters.SELF_PATCH_INSTRUCTIONS_FILENAME;
				string completedInstructionsPath = manager.CachePath + PatchParameters.SELF_PATCH_COMPLETED_INSTRUCTIONS_FILENAME;
				if( !File.Exists( instructionsPath ) )
				{
					Result = PatchResult.Failed;
					FailReason = PatchFailReason.Unknown;
					FailDetails = "";

					manager.LogToFile( Localization.Get( LocalizationID.E_XDoesNotExist, instructionsPath ) );
					return false;
				}

				FileInfo selfPatcher = new FileInfo( selfPatcherExecutable );

				string args = "\"" + instructionsPath + "\" \"" + completedInstructionsPath + "\"";
				if( !string.IsNullOrEmpty( postSelfPatchExecutable ) && File.Exists( postSelfPatchExecutable ) )
					args += " \"" + postSelfPatchExecutable + "\"";

				ProcessStartInfo startInfo = new ProcessStartInfo( selfPatcher.FullName )
				{
					Arguments = args,
					WorkingDirectory = selfPatcher.DirectoryName
				};

				Process.Start( startInfo );
				Result = PatchResult.Success;
			}
			catch( Exception e )
			{
				Result = PatchResult.Failed;
				FailReason = PatchFailReason.FatalException;
				FailDetails = e.ToString();

				manager.LogToFile( e );
				return false;
			}
			finally
			{
				manager.DisposeFileLogger();

				IsRunning = false;
				manager.ListenerCallFinished();
			}

			Process.GetCurrentProcess().Kill();
			return true;
		}

   //     public long GetDownloadSizeFromVersionInfoURL (string VERSIONINFO_URL)
   //     {
			//if (string.IsNullOrEmpty (VERSIONINFO_URL)) {
			//	Console.WriteLine ($"The provided VERSIONINFO_URL: {VERSIONINFO_URL} is null or empty");
			//	return 0;
			//}

   //         List<VersionItem> versionItems = FetchVersionInfoFromURL (VERSIONINFO_URL).Files;
   //         long estimatedDownloadSize = 0L;

   //         for (int i = 0; i < versionItems.Count; i++)
   //             estimatedDownloadSize += versionItems[i].CompressedFileSize;

   //         return estimatedDownloadSize;
   //     }

        public long GetDownloadSize (VersionInfo versionInfo = null)
        {
			if (versionInfo == null)
			{
				versionInfo = manager.VersionInfo;
			}

			if (versionInfo == null)
			{
				return 0;
			}

			List<VersionItem> versionItems = versionInfo.Files;
            long estimatedDownloadSize = 0L;

            for (int i = 0; i < versionItems.Count; i++)
                estimatedDownloadSize += versionItems[i].CompressedFileSize;

            return estimatedDownloadSize;
        }

        private void ThreadCheckForUpdatesFunction( object checkVersionOnlyParameter )
		{
			manager.InitializeFileLogger();
			manager.ListenerCallStarted();

			try
			{
				bool checkVersionOnly = (bool) checkVersionOnlyParameter;
				Result = CheckForUpdatesInternal( checkVersionOnly );
			}
			catch( Exception e )
			{
				Result = PatchResult.Failed;
				FailReason = PatchFailReason.FatalException;
				FailDetails = e.ToString();
			}

			if( Result == PatchResult.AlreadyUpToDate )
				manager.Log( Localization.Get( LocalizationID.AppIsUpToDate ) );
			else if( Result == PatchResult.Success )
				manager.Log( Localization.Get( LocalizationID.UpdateAvailable ) );
			else
				manager.Log( manager.FailDetails );

			manager.DisposeFileLogger();

			IsRunning = false;
			manager.ListenerCallFinished();
		}

		private void ThreadPatchFunction()
		{
			manager.InitializeFileLogger();
			manager.ListenerCallStarted();

			try
			{
				Result = Patch();
			}
			catch( Exception e )
			{
				Result = PatchResult.Failed;
				FailReason = PatchFailReason.FatalException;
				FailDetails = e.ToString();
			}

			if( Result == PatchResult.AlreadyUpToDate )
				manager.Log( Localization.Get( LocalizationID.AppIsUpToDate ) );
			else if( Result == PatchResult.Success )
				manager.Log( Operation == PatchOperation.Patching ? Localization.Get( LocalizationID.AppIsUpToDate ) : Localization.Get( LocalizationID.ReadyToSelfPatch ) );
			else
				manager.Log( manager.FailDetails );

			manager.DisposeFileLogger();

			IsRunning = false;
			manager.ListenerCallFinished();
		}

		private PatchResult CheckForUpdatesInternal( bool checkVersionOnly )
		{
			PatchStage = PatchStage.CheckingUpdates;

			manager.Log( Localization.Get( LocalizationID.CheckingForUpdates ) );

            if ( !FetchVersionInfo() )
				return PatchResult.Failed;

			if( manager.IsUnderMaintenance() )
				return PatchResult.Failed;

			if( !checkVersionOnly )
			{
				if( CheckLocalFilesUpToDate( true, false ) )
					return PatchResult.AlreadyUpToDate;

				return PatchResult.Success;
			}
			else
			{
				if (currentVersion == manager.VersionInfo.Version) {
					return PatchResult.AlreadyUpToDate;
				}
				else {
					return PatchResult.Success;
				}
			}
		}

		private PatchResult Patch()
		{
			PatchStage = PatchStage.CheckingUpdates;

			Stopwatch timer = Stopwatch.StartNew();

			manager.Log( Localization.Get( LocalizationID.RetrievingVersionInfo ) );

			if( !FetchVersionInfo() )
				return PatchResult.Failed;

			if( manager.IsUnderMaintenance() )
				return PatchResult.Failed;

			if( !currentVersion.IsValid )
				currentVersion = new VersionCode( 0 );

			VersionCode rootVersion = currentVersion;
			if( manager.SelfPatching )
			{
				VersionCode patchedVersion = PatchUtils.GetVersion( manager.DecompressedFilesPath, manager.VersionInfo.Name );
				if( patchedVersion > currentVersion )
					currentVersion = patchedVersion;
			}

			PatchStage = PatchStage.CheckingFileIntegrity;

			if( CheckLocalFilesUpToDate( true, false ) )
				return PatchResult.AlreadyUpToDate;

			if( !PatchUtils.CheckWriteAccessToFolder( manager.RootPath ) )
			{
				FailReason = PatchFailReason.RequiresAdminPriviledges;
				FailDetails = Localization.Get( LocalizationID.E_AccessToXIsForbiddenRunInAdminMode, manager.RootPath );

				return PatchResult.Failed;
			}

			if( !PatchUtils.CheckWriteAccessToFolder( manager.CachePath ) )
			{
				FailReason = PatchFailReason.RequiresAdminPriviledges;
				FailDetails = Localization.Get( LocalizationID.E_AccessToXIsForbiddenRunInAdminMode, manager.CachePath );

				return PatchResult.Failed;
			}

			if( checkForMultipleRunningInstances )
			{
				string currentExecutablePath = PatchUtils.GetCurrentExecutablePath();
				if( PatchUtils.GetNumberOfRunningProcesses( currentExecutablePath ) > 1 )
				{
					FailReason = PatchFailReason.MultipleRunningInstances;
					FailDetails = Localization.Get( LocalizationID.E_AnotherInstanceOfXIsRunning, Path.GetFileName( currentExecutablePath ) );

					return PatchResult.Failed;
				}
			}

			if( manager.Cancel )
				return PatchResult.Failed;

			// Add a date holder file to the cache to save the last access time reliably
			DateTime dateTimeNow = DateTime.UtcNow;
			File.WriteAllText( manager.CachePath + PatchParameters.CACHE_DATE_HOLDER_FILENAME, dateTimeNow.ToString( "O" ) );

			// Check if there are any leftover files from other GameLauncherCore integrated apps in cache
			DirectoryInfo[] patcherCaches = new DirectoryInfo( manager.CachePath ).Parent.GetDirectories();
			for( int i = 0; i < patcherCaches.Length; i++ )
			{
				DirectoryInfo cacheDir = patcherCaches[i];
				if( cacheDir.Name.Equals( manager.VersionInfo.Name, StringComparison.OrdinalIgnoreCase ) )
					continue;

				FileInfo dateHolder = new FileInfo( PatchUtils.GetPathWithTrailingSeparatorChar( cacheDir.FullName ) + PatchParameters.CACHE_DATE_HOLDER_FILENAME );
				if( dateHolder.Exists && dateHolder.Length > 0L )
				{
					DateTime lastAccessTime;
					if( DateTime.TryParseExact( File.ReadAllText( dateHolder.FullName ), "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out lastAccessTime ) )
					{
						if( ( dateTimeNow - lastAccessTime ).TotalDays <= PatchParameters.CACHE_DATE_EXPIRE_DAYS )
							continue;
					}
				}

				// This cache directory doesn't have a date holder file or is older than CACHE_DATE_EXPIRE_DAYS, delete it
				cacheDir.Delete( true );
			}

			bool canRepairPatch = this.canRepairPatch;
			bool canIncrementalPatch = this.canIncrementalPatch;
			bool canInstallerPatch = this.canInstallerPatch;

			List<PatchMethodHolder> preferredPatchMethods = new List<PatchMethodHolder>( 3 );
			List<VersionItem> versionInfoFiles = manager.VersionInfo.Files;

			if( canRepairPatch )
			{
				for( int i = 0; i < versionInfoFiles.Count; i++ )
				{
					VersionItem item = versionInfoFiles[i];
					if( item.CompressedFileSize == 0L && string.IsNullOrEmpty( item.CompressedMd5Hash ) )
					{
						canRepairPatch = false;
						break;
					}
				}

				if( canRepairPatch )
				{
					long repairPatchSize = 0L;
					for( int i = 0; i < versionInfoFiles.Count; i++ )
					{
						VersionItem item = versionInfoFiles[i];
						FileInfo localFile = new FileInfo( manager.RootPath + item.Path );
						if( localFile.Exists && localFile.MatchesSignature( item.FileSize, item.Md5Hash ) )
							continue;

						FileInfo downloadedFile = new FileInfo( manager.DownloadsPath + item.Path );
						if( downloadedFile.Exists && downloadedFile.MatchesSignature( item.CompressedFileSize, item.CompressedMd5Hash ) )
							continue;

						if( manager.SelfPatching )
						{
							FileInfo decompressedFile = new FileInfo( manager.DecompressedFilesPath + item.Path );
							if( decompressedFile.Exists && decompressedFile.MatchesSignature( item.FileSize, item.Md5Hash ) )
								continue;
						}

						repairPatchSize += item.CompressedFileSize;
					}

					preferredPatchMethods.Add( new PatchMethodHolder( PatchMethod.RepairPatch, repairPatchSize ) );
				}
			}

			if( canIncrementalPatch )
			{
				// Find incremental patches to apply
				VersionCode thisVersion = rootVersion;
				List<IncrementalPatch> versionInfoPatches = manager.VersionInfo.IncrementalPatches;
				for( int i = 0; i < versionInfoPatches.Count; i++ )
				{
					if( thisVersion == manager.VersionInfo.Version )
						break;

					IncrementalPatch patch = versionInfoPatches[i];
					if( thisVersion == patch.FromVersion )
					{
						thisVersion = patch.ToVersion;
						incrementalPatches.Add( patch );
					}
				}

				if( thisVersion != manager.VersionInfo.Version )
					incrementalPatches.Clear();

				if( incrementalPatches.Count == 0 )
					canIncrementalPatch = false;
				else
				{
					long incrementalPatchSize = 0L;
					for( int i = 0; i < incrementalPatches.Count; i++ )
					{
						IncrementalPatch incrementalPatch = incrementalPatches[i];
						if( currentVersion > incrementalPatch.FromVersion )
							continue;

						FileInfo patchFile = new FileInfo( manager.GetDownloadPathForPatch( incrementalPatch.PatchVersion() ) );
						if( patchFile.Exists && patchFile.MatchesSignature( incrementalPatch.PatchSize, incrementalPatch.PatchMd5Hash ) )
							continue;

						incrementalPatchSize += incrementalPatch.PatchSize;
					}

					preferredPatchMethods.Add( new PatchMethodHolder( PatchMethod.IncrementalPatch, incrementalPatchSize ) );
				}
			}

			if( canInstallerPatch )
			{
				InstallerPatch installerPatch = manager.VersionInfo.InstallerPatch;
				if( installerPatch.PatchSize == 0L && string.IsNullOrEmpty( installerPatch.PatchMd5Hash ) )
					canInstallerPatch = false;
				else
					preferredPatchMethods.Add( new PatchMethodHolder( PatchMethod.InstallerPatch, installerPatch.PatchSize ) );
			}

			preferredPatchMethods.Sort( ( p1, p2 ) => p1.size.CompareTo( p2.size ) );

			if( preferredPatchMethods.Count == 0 )
			{
				FailReason = PatchFailReason.NoSuitablePatchMethodFound;
				FailDetails = Localization.Get( LocalizationID.E_NoSuitablePatchMethodFound );

				return PatchResult.Failed;
			}

			// Check if there is enough free disk space
			long requiredFreeSpaceInCache = preferredPatchMethods[0].size, requiredFreeSpaceInRoot = 0L;
			for( int i = 0; i < versionInfoFiles.Count; i++ )
			{
				VersionItem item = versionInfoFiles[i];
				FileInfo localFile = new FileInfo( manager.RootPath + item.Path );
				if( !localFile.Exists )
				{
					requiredFreeSpaceInCache += item.FileSize;
					requiredFreeSpaceInRoot += item.FileSize;
				}
				else if( !localFile.MatchesSignature( item.FileSize, item.Md5Hash ) )
				{
					requiredFreeSpaceInCache += item.FileSize;

					long deltaSize = item.FileSize - localFile.Length;
					if( deltaSize > 0L )
						requiredFreeSpaceInRoot += deltaSize;
				}
			}

			requiredFreeSpaceInCache += requiredFreeSpaceInCache / 3; // Require additional 33% free space (might be needed by compressed files and/or incremental patches)
			requiredFreeSpaceInCache += 1024 * 1024 * 1024L; // Require additional 1 GB of free space, just in case

			string rootDrive = new DirectoryInfo( manager.RootPath ).Root.FullName;
			string cacheDrive = new DirectoryInfo( manager.CachePath ).Root.FullName;
			if( rootDrive.Equals( cacheDrive, StringComparison.OrdinalIgnoreCase ) )
			{
				if( !CheckFreeSpace( rootDrive, requiredFreeSpaceInCache + requiredFreeSpaceInRoot ) )
					return PatchResult.Failed;
			}
			else
			{
				if( !CheckFreeSpace( rootDrive, requiredFreeSpaceInRoot ) )
					return PatchResult.Failed;

				if( !CheckFreeSpace( cacheDrive, requiredFreeSpaceInCache ) )
					return PatchResult.Failed;
			}

			for( int i = 0; i < preferredPatchMethods.Count; i++ )
				manager.LogToFile( Localization.Get( LocalizationID.PatchMethodXSizeY, preferredPatchMethods[i].method, preferredPatchMethods[i].size.ToMegabytes() + "MB" ) );

			// Start patching
			for( int i = 0; i < preferredPatchMethods.Count; i++ )
			{
				PatchMethod patchMethod = preferredPatchMethods[i].method;

				bool success;
				if( patchMethod == PatchMethod.RepairPatch )
				{
					PatchMethod = PatchMethod.RepairPatch;
					manager.ListenerCallPatchMethodChanged( PatchMethod );

					success = PatchUsingRepairPatch();
				}
				else if( patchMethod == PatchMethod.IncrementalPatch )
				{
					PatchMethod = PatchMethod.IncrementalPatch;
					manager.ListenerCallPatchMethodChanged( PatchMethod );

					success = PatchUsingIncrementalPatches();
				}
				else
				{
					PatchMethod = PatchMethod.InstallerPatch;
					manager.ListenerCallPatchMethodChanged( PatchMethod );

					success = PatchUsingInstallerPatch();
				}

				if( manager.Cancel )
					return PatchResult.Failed;

				if( success )
					break;
				else
				{
					manager.LogToFile( string.Concat( manager.FailReason, ": ", manager.FailDetails ) );

					if( i == preferredPatchMethods.Count - 1 )
						return PatchResult.Failed;
				}
			}


            // Check file integrity
            // Only if NOT is using incremental patching
            //if (PatchMethod != PatchMethod.IncrementalPatch) {
				PatchStage = PatchStage.CheckingFileIntegrity;

				if (!CheckLocalFilesUpToDate (false, manager.SelfPatching))
				{
					manager.Log (Localization.Get (LocalizationID.SomeFilesAreStillNotUpToDate));

					if (canRepairPatch)
					{
						if (!PatchUsingRepairPatch ())
							return PatchResult.Failed;
					}
					else
					{
						FailReason = PatchFailReason.FilesAreNotUpToDateAfterPatch;
						FailDetails = Localization.Get (LocalizationID.E_FilesAreNotUpToDateAfterPatch);

						return PatchResult.Failed;
					}
				}
			//}

			manager.UpdateVersion( manager.VersionInfo.Version );

			PatchStage = PatchStage.DeletingObsoleteFiles;
			manager.Log( Localization.Get( LocalizationID.CalculatingObsoleteFiles ) );

			List<string> obsoleteFiles = FindFilesToDelete( manager.RootPath );
			if( !manager.SelfPatching )
			{
				if( obsoleteFiles.Count > 0 )
				{
					manager.Log( Localization.Get( LocalizationID.DeletingXObsoleteFiles, obsoleteFiles.Count ) );
					for( int i = 0; i < obsoleteFiles.Count; i++ )
					{
						manager.Log( Localization.Get( LocalizationID.DeletingX, obsoleteFiles[i] ) );
						File.Delete( manager.RootPath + obsoleteFiles[i] );
					}
				}
				else
					manager.Log( Localization.Get( LocalizationID.NoObsoleteFiles ) );

				PatchUtils.DeleteDirectory( manager.CachePath );
			}
			else
			{
				// Delete obsolete self patching files
				List<string> obsoleteSelfPatchingFiles = FindFilesToDelete( manager.DecompressedFilesPath );
				if( obsoleteSelfPatchingFiles.Count > 0 )
				{
					manager.Log( Localization.Get( LocalizationID.DeletingXObsoleteFiles, obsoleteSelfPatchingFiles.Count ) );
					for( int i = 0; i < obsoleteSelfPatchingFiles.Count; i++ )
					{
						manager.Log( Localization.Get( LocalizationID.DeletingX, obsoleteSelfPatchingFiles[i] ) );
						File.Delete( manager.DecompressedFilesPath + obsoleteSelfPatchingFiles[i] );
					}
				}
				else
					manager.Log( Localization.Get( LocalizationID.NoObsoleteFiles ) );

				// Self patcher executable, if exists, can't self patch itself, so patch it manually here
				// This assumes that self patcher and any related files are located at SELF_PATCHER_DIRECTORY
				string selfPatcherFiles = manager.DecompressedFilesPath + PatchParameters.SELF_PATCHER_DIRECTORY;
				if( Directory.Exists( selfPatcherFiles ) )
					PatchUtils.MoveDirectory( selfPatcherFiles, manager.RootPath + PatchParameters.SELF_PATCHER_DIRECTORY );

				string separator = PatchParameters.SELF_PATCH_OP_SEPARATOR;
				StringBuilder sb = new StringBuilder( 500 );

				// Append current version to the beginning of the file
				sb.Append( rootVersion );

				// 1. Rename files
				if( incrementalPatchesInfo.Count > 0 )
				{
					sb.Append( separator ).Append( PatchParameters.SELF_PATCH_MOVE_OP );
					for( int i = 0; i < incrementalPatchesInfo.Count; i++ )
					{
						IncrementalPatchInfo incrementalPatch = incrementalPatchesInfo[i];
						for( int j = 0; j < incrementalPatch.RenamedFiles.Count; j++ )
						{
							PatchRenamedItem renamedItem = incrementalPatch.RenamedFiles[j];
							sb.Append( separator ).Append( manager.RootPath + renamedItem.BeforePath ).Append( separator ).Append( manager.RootPath + renamedItem.AfterPath );
						}
					}
				}

				// 2. Update files
				sb.Append( separator ).Append( PatchParameters.SELF_PATCH_MOVE_OP );

				DirectoryInfo updatedFilesDir = new DirectoryInfo( manager.DecompressedFilesPath );
				DirectoryInfo[] updatedSubDirectories = updatedFilesDir.GetDirectories();
				for( int i = 0; i < updatedSubDirectories.Length; i++ )
					sb.Append( separator ).Append( manager.DecompressedFilesPath ).Append( updatedSubDirectories[i].Name ).Append( Path.DirectorySeparatorChar ).Append( separator ).Append( manager.RootPath ).Append( updatedSubDirectories[i].Name ).Append( Path.DirectorySeparatorChar );

				string versionHolderFilename = manager.VersionInfo.Name + PatchParameters.VERSION_HOLDER_FILENAME_POSTFIX;
				FileInfo[] updatedFiles = updatedFilesDir.GetFiles();
				for( int i = 0; i < updatedFiles.Length; i++ )
				{
					if( updatedFiles[i].Name != versionHolderFilename )
						sb.Append( separator ).Append( manager.DecompressedFilesPath ).Append( updatedFiles[i].Name ).Append( separator ).Append( manager.RootPath ).Append( updatedFiles[i].Name );
				}

				// Update the version holder only after everything else is updated properly
				sb.Append( separator ).Append( manager.DecompressedFilesPath ).Append( versionHolderFilename ).Append( separator ).Append( manager.RootPath ).Append( versionHolderFilename );

				// 3. Delete obsolete files
				if( obsoleteFiles.Count > 0 )
				{
					string selfPatcherDirectory = PatchParameters.SELF_PATCHER_DIRECTORY + Path.DirectorySeparatorChar;
					sb.Append( separator ).Append( PatchParameters.SELF_PATCH_DELETE_OP );

					manager.Log( Localization.Get( LocalizationID.DeletingXObsoleteFiles, obsoleteFiles.Count ) );
					for( int i = 0; i < obsoleteFiles.Count; i++ )
					{
						// Delete the obsolete files inside SELF_PATCHER_DIRECTORY manually
						string absolutePath = manager.RootPath + obsoleteFiles[i];
						if( obsoleteFiles[i].StartsWith( selfPatcherDirectory, StringComparison.OrdinalIgnoreCase ) )
						{
							manager.Log( Localization.Get( LocalizationID.DeletingX, obsoleteFiles[i] ) );

							if( File.Exists( absolutePath ) )
								File.Delete( absolutePath );
							else if( Directory.Exists( absolutePath ) )
								PatchUtils.DeleteDirectory( absolutePath );
						}
						else
						{
							// '-->' indicates that the file will be deleted by the self patcher executable
							manager.LogToFile( Localization.Get( LocalizationID.DeletingX, "--> " + obsoleteFiles[i] ) );
							sb.Append( separator ).Append( absolutePath );
						}
					}
				}
				else
					manager.Log( Localization.Get( LocalizationID.NoObsoleteFiles ) );

				sb.Append( separator ).Append( manager.CachePath );

				File.Delete( manager.CachePath + PatchParameters.SELF_PATCH_COMPLETED_INSTRUCTIONS_FILENAME );
				File.WriteAllText( manager.CachePath + PatchParameters.SELF_PATCH_INSTRUCTIONS_FILENAME, sb.Append( separator ).ToString() );
			}

			manager.Log( Localization.Get( LocalizationID.PatchCompletedInXSeconds, timer.ElapsedSeconds() ) );
			return PatchResult.Success;
		}

		public VersionInfo FetchVersionInfoFromURL (string url)
		{
			VersionInfo versionInfo = null;
            string versionInfoXML = manager.DownloadManager.DownloadTextFromURL (url);
            if (string.IsNullOrEmpty (versionInfoXML))
            {
                return null;
            }

            if (versionInfoVerifier != null && !versionInfoVerifier (ref versionInfoXML))
            {
                return null;
            }

            try
            {
                versionInfo = PatchUtils.DeserializeXMLToVersionInfo (versionInfoXML);
            }
            catch (Exception e)
            {
                return null;
            }

			return versionInfo;
        }

        private bool FetchVersionInfo()
		{
			string versionInfoXML = manager.DownloadManager.DownloadTextFromURL( versionInfoURL );
			if( string.IsNullOrEmpty( versionInfoXML ) )
			{
				FailReason = PatchFailReason.DownloadError;
				FailDetails = Localization.Get( LocalizationID.E_VersionInfoCouldNotBeDownloaded );
				manager.Log (Localization.Get( LocalizationID.E_VersionInfoCouldNotBeDownloaded ) + $"at URL: {versionInfoURL}");
				return false;
			}

			if( versionInfoVerifier != null && !versionInfoVerifier( ref versionInfoXML ) )
			{
				FailReason = PatchFailReason.CantVerifyVersionInfo;
				FailDetails = Localization.Get( LocalizationID.E_VersionInfoCouldNotBeVerified );
				manager.Log (Localization.Get (LocalizationID.E_VersionInfoCouldNotBeVerified));
				return false;
			}

			try
			{
				manager.VersionInfo = PatchUtils.DeserializeXMLToVersionInfo( versionInfoXML );
				manager.Log (Localization.Get (LocalizationID.GotVersionInfoXML));
				//manager.Log ($"VersionInfoXML: {versionInfoXML}");
			}
			catch( Exception e )
			{
				manager.LogToFile( e );

				FailReason = PatchFailReason.XmlDeserializeError;
				FailDetails = Localization.Get( LocalizationID.E_VersionInfoInvalid );

				return false;
			}

			if( manager.Cancel )
				return false;

			if( !manager.VersionInfo.Version.IsValid )
			{
				FailReason = PatchFailReason.InvalidVersionCode;
				FailDetails = Localization.Get( LocalizationID.E_VersionInfoInvalid );

				return false;
			}

			incrementalPatches.Clear();
			incrementalPatchesInfo.Clear();
			filesInVersion.Clear();

			manager.ListenerCallVersionInfoFetched( manager.VersionInfo );

			List<VersionItem> versionInfoFiles = manager.VersionInfo.Files;
			for( int i = 0; i < versionInfoFiles.Count; i++ )
				filesInVersion.Add( versionInfoFiles[i].Path );

			currentVersion = PatchUtils.GetVersion( manager.RootPath, manager.VersionInfo.Name );
			manager.ListenerCallVersionFetched( currentVersion, manager.VersionInfo.Version );

            manager.Log ($"...Current App version: {currentVersion}");
            manager.Log ($"...Server version: {manager.VersionInfo.Version}");

            return true;
		}

		private bool PatchUsingRepairPatch()
		{
			if( manager.Cancel )
				return false;

			manager.LogToFile( Localization.Get( LocalizationID.ApplyingRepairPatch ) );

			if( new RepairPatchApplier( manager ).Run() == PatchResult.Failed )
				return false;

			return true;
		}

		private bool PatchUsingIncrementalPatches()
		{
			if( manager.Cancel )
				return false;

			manager.LogToFile( Localization.Get( LocalizationID.ApplyingIncrementalPatch ) );

			if( incrementalPatches.Count == 0 )
				return false;

			if( manager.VerifyFiles )
			{
				PatchStage = PatchStage.VerifyingFilesOnServer;

				for( int i = 0; i < incrementalPatches.Count; i++ )
				{
					if( manager.Cancel )
						return false;

					IncrementalPatch incrementalPatch = incrementalPatches[i];
					long fileSize;
					if( !manager.DownloadManager.FileExistsAtUrl( manager.VersionInfo.GetInfoURLFor( incrementalPatch ), out fileSize ) )
					{
						FailReason = PatchFailReason.FileDoesNotExistOnServer;
						FailDetails = Localization.Get( LocalizationID.E_FileXDoesNotExistOnServer, incrementalPatch.PatchVersion() + PatchParameters.INCREMENTAL_PATCH_INFO_EXTENSION );

						return false;
					}

					if( incrementalPatch.Files > 0 )
					{
						if( !manager.DownloadManager.FileExistsAtUrl( manager.VersionInfo.GetDownloadURLFor( incrementalPatch ), out fileSize ) )
						{
							FailReason = PatchFailReason.FileDoesNotExistOnServer;
							FailDetails = Localization.Get( LocalizationID.E_FileXDoesNotExistOnServer, incrementalPatch.PatchVersion() + PatchParameters.INCREMENTAL_PATCH_FILE_EXTENSION );

							return false;
						}
						else if( fileSize > 0L && fileSize != incrementalPatch.PatchSize )
						{
							FailReason = PatchFailReason.FileIsNotValidOnServer;
							FailDetails = Localization.Get( LocalizationID.E_FileXIsNotValidOnServer, incrementalPatch.PatchVersion() + PatchParameters.INCREMENTAL_PATCH_FILE_EXTENSION );

							return false;
						}
					}
				}
			}

			for( int i = 0; i < incrementalPatches.Count; i++ )
			{
				if( manager.Cancel )
					return false;

				IncrementalPatch incrementalPatch = incrementalPatches[i];
				string patchInfoXML = manager.DownloadManager.DownloadTextFromURL( manager.VersionInfo.GetInfoURLFor( incrementalPatch ) );
				if( patchInfoXML == null )
				{
					FailReason = PatchFailReason.DownloadError;
					FailDetails = Localization.Get( LocalizationID.E_CouldNotDownloadPatchInfoX, incrementalPatch.PatchVersionBrief() );

					return false;
				}

				if( patchInfoVerifier != null && !patchInfoVerifier( ref patchInfoXML ) )
				{
					FailReason = PatchFailReason.CantVerifyPatchInfo;
					FailDetails = Localization.Get( LocalizationID.E_PatchInfoCouldNotBeVerified );

					return false;
				}

				IncrementalPatchInfo patchInfo;
				try
				{
					patchInfo = PatchUtils.DeserializeXMLToIncrementalPatchInfo( patchInfoXML );
				}
				catch( Exception e )
				{
					manager.LogToFile( e );

					FailReason = PatchFailReason.XmlDeserializeError;
					FailDetails = Localization.Get( LocalizationID.E_InvalidPatchInfoX, incrementalPatch.PatchVersionBrief() );

					return false;
				}

				patchInfo.FromVersion = incrementalPatch.FromVersion;
				patchInfo.ToVersion = incrementalPatch.ToVersion;
				patchInfo.DownloadURL = manager.VersionInfo.GetDownloadURLFor( incrementalPatch );
				patchInfo.CompressedFileSize = incrementalPatch.PatchSize;
				patchInfo.CompressedMd5Hash = incrementalPatch.PatchMd5Hash;
				patchInfo.CompressionFormat = incrementalPatch.CompressionFormat;

				incrementalPatchesInfo.Add( patchInfo );

				if( currentVersion > incrementalPatch.FromVersion )
					continue;

				if( new IncrementalPatchApplier( manager, patchInfo ).Run() == PatchResult.Failed )
					return false;
			}

			return true;
		}

		private bool PatchUsingInstallerPatch()
		{
			if( manager.Cancel )
				return false;

			manager.LogToFile( Localization.Get( LocalizationID.ApplyingInstallerPatch ) );

			if( manager.VerifyFiles )
			{
				PatchStage = PatchStage.VerifyingFilesOnServer;

				InstallerPatch installerPatch = manager.VersionInfo.InstallerPatch;
				long fileSize;
				if( !manager.DownloadManager.FileExistsAtUrl( manager.VersionInfo.GetDownloadURLFor( installerPatch ), out fileSize ) )
				{
					FailReason = PatchFailReason.FileDoesNotExistOnServer;
					FailDetails = Localization.Get( LocalizationID.E_FileXDoesNotExistOnServer, PatchParameters.INSTALLER_PATCH_FILENAME );

					return false;
				}
				else if( fileSize > 0L && fileSize != installerPatch.PatchSize )
				{
					FailReason = PatchFailReason.FileIsNotValidOnServer;
					FailDetails = Localization.Get( LocalizationID.E_FileXIsNotValidOnServer, PatchParameters.INSTALLER_PATCH_FILENAME );

					return false;
				}
			}

			if( new InstallerPatchApplier( manager ).Run() == PatchResult.Failed )
				return false;

			return true;
		}

        private bool CheckFreeSpace( string drive, long requiredFreeSpace )
		{
			if( freeDiskSpaceCalculator( drive ) < requiredFreeSpace )
			{
				FailReason = PatchFailReason.InsufficientSpace;
				FailDetails = Localization.Get( LocalizationID.E_InsufficientSpaceXNeededInY, requiredFreeSpace.ToMegabytes() + "MB", drive );

				return false;
			}

			return true;
		}

		private bool CheckLocalFilesUpToDate( bool checkObsoleteFiles, bool searchSelfPatchFiles )
		{
			manager.Log( Localization.Get( LocalizationID.CheckingIfFilesAreUpToDate ) );

			List<VersionItem> versionInfoFiles = manager.VersionInfo.Files;
			//foreach (VersionItem versionItem in versionInfoFiles) {
   //             manager.Log ($"File: {versionItem.}");
   //         }

            for ( int i = 0; i < versionInfoFiles.Count; i++ )
			{
				VersionItem item = versionInfoFiles[i];
				FileInfo localFile = new FileInfo( manager.RootPath + item.Path );

                manager.Log ($"Server version file: {item.Path} FileSize: {item.FileSize} MD5: {item.Md5Hash}");

				if (localFile.Exists) {
					manager.Log ($"Checking local file: {localFile.FullName} FileSize: {localFile.Length} MD5: {localFile.Md5Hash ()}");
				} else {
					manager.Log ($"Local file not exist");
                }
		
				if( !localFile.Exists || !localFile.MatchesSignature( item.FileSize, item.Md5Hash ) )
				{
					manager.Log ($"Different file found: {localFile.FullName}");
					if( searchSelfPatchFiles )
					{
						FileInfo decompressedFile = new FileInfo( manager.DecompressedFilesPath + item.Path );
						if( decompressedFile.Exists && decompressedFile.MatchesSignature( item.FileSize, item.Md5Hash ) )
							continue;
					}

					return false;
				}
			}

			// Check if there are any obsolete files
			return !checkObsoleteFiles || FindFilesToDelete( manager.RootPath ).Count == 0;
		}

		private List<string> FindFilesToDelete( string rootPath )
		{
			List<string> filesToDelete = new List<string>();
			FindFilesToDelete( rootPath, filesToDelete );
			return filesToDelete;
		}

		private void FindFilesToDelete( string rootPath, List<string> filesToDelete, string relativePath = "" )
		{
			DirectoryInfo directory = new DirectoryInfo( rootPath + relativePath );

			FileInfo[] files = directory.GetFiles();
			for( int i = 0; i < files.Length; i++ )
			{
				string fileRelativePath = relativePath + files[i].Name;
				if( !filesInVersion.Contains( fileRelativePath ) && !manager.VersionInfo.IgnoredPathsRegex.PathMatchesPattern( fileRelativePath ) )
					filesToDelete.Add( fileRelativePath );
			}

			DirectoryInfo[] subDirectories = directory.GetDirectories();
			for( int i = 0; i < subDirectories.Length; i++ )
			{
				string directoryRelativePath = relativePath + subDirectories[i].Name + Path.DirectorySeparatorChar;
				if( !manager.VersionInfo.IgnoredPathsRegex.PathMatchesPattern( directoryRelativePath ) )
					FindFilesToDelete( rootPath, filesToDelete, directoryRelativePath );
			}
		}
	}
}