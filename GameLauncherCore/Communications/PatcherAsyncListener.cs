using System.Threading;

namespace GameLauncherCore
{
	public class PatcherAsyncListener : GameLauncherPatcher.IListener
	{
		private const int DEFAULT_REFRESH_INTERVAL = 100; // In milliseconds

		public delegate void LogReceiveDelegate( string log );
		public delegate void ProgressChangeDelegate( IOperationProgress progress );
		public delegate void PatchStageChangeDelegate( PatchStage stage );
		public delegate void PatchMethodChangeDelegate( PatchMethod method );
		public delegate void VersionInfoFetchDelegate( VersionInfo versionInfo );
		public delegate void VersionFetchDelegate( string currentVersion, string newVersion );
		public delegate void NoParameterDelegate();

		public event LogReceiveDelegate OnLogReceived;
		public event ProgressChangeDelegate OnProgressChanged;
		public event ProgressChangeDelegate OnOverallProgressChanged;
		public event PatchStageChangeDelegate OnPatchStageChanged;
		public event PatchMethodChangeDelegate OnPatchMethodChanged;
		public event VersionInfoFetchDelegate OnVersionInfoFetched;
		public event VersionFetchDelegate OnVersionFetched;
		public event NoParameterDelegate OnStart, OnFinish;

		bool GameLauncherPatcher.IListener.ReceiveLogs { get { return true; } }
		bool GameLauncherPatcher.IListener.ReceiveProgress { get { return true; } }

		public int RefreshInterval { get; set; }

		private bool pendingStart, pendingFinish;
		private string pendingLog;
		private IOperationProgress pendingProgress, pendingOverallProgress;
		private PatchStage? pendingStage;
		private PatchMethod? pendingMethod;
		private string pendingCurrentVersion, pendingNewVersion;

		private bool isInitialized;
		private bool isPatcherRunning;

		private readonly object threadLock;

		public PatcherAsyncListener()
		{
			RefreshInterval = DEFAULT_REFRESH_INTERVAL;

			pendingStart = false;
			pendingFinish = false;
			pendingLog = null;
			pendingProgress = null;
			pendingOverallProgress = null;
			pendingStage = null;
			pendingMethod = null;
			pendingCurrentVersion = null;
			pendingNewVersion = null;

			isInitialized = false;
			isPatcherRunning = false;

			threadLock = new object();
		}

		void GameLauncherPatcher.IListener.Started()
		{
			lock( threadLock )
			{
				isPatcherRunning = true;
				pendingStart = true;
				pendingFinish = false;
			}

			if( !isInitialized )
			{
				isInitialized = true;
				Initialize();
			}
		}

		void GameLauncherPatcher.IListener.LogReceived( string log )
		{
			pendingLog = log;
		}

		void GameLauncherPatcher.IListener.ProgressChanged( IOperationProgress progress )
		{
			pendingProgress = progress;
		}

		void GameLauncherPatcher.IListener.OverallProgressChanged( IOperationProgress progress )
		{
			pendingOverallProgress = progress;
		}

		void GameLauncherPatcher.IListener.PatchStageChanged( PatchStage stage )
		{
			pendingStage = stage;
		}

		void GameLauncherPatcher.IListener.PatchMethodChanged( PatchMethod method )
		{
			pendingMethod = method;
		}

		void GameLauncherPatcher.IListener.VersionInfoFetched( VersionInfo versionInfo )
		{
			// This is the only callback that blocks GameLauncherCore's thread because 
			// changes made to the VersionInfo should be committed before GameLauncherCore
			// starts patching (e.g. adding an ignored path to the VersionInfo)
			if( OnVersionInfoFetched != null )
				OnVersionInfoFetched( versionInfo );
		}

		void GameLauncherPatcher.IListener.VersionFetched( string currentVersion, string newVersion )
		{
			pendingCurrentVersion = currentVersion;
			pendingNewVersion = newVersion;
		}

		void GameLauncherPatcher.IListener.Finished()
		{
			lock( threadLock )
			{
				isPatcherRunning = false;
				pendingFinish = true;
			}
		}

		protected virtual void Initialize()
		{
			PatchUtils.CreateBackgroundThread( new ThreadStart( ThreadRefresherFunction ) ).Start();
		}

		private void ThreadRefresherFunction()
		{
			while( Refresh() )
				Thread.Sleep( RefreshInterval );
		}

		protected bool Refresh()
		{
			try
			{
				RefreshInternal();
			}
			catch { }

			bool shouldContinue;
			lock( threadLock )
			{
				shouldContinue = isPatcherRunning;
				isInitialized = isPatcherRunning;
			}

			// Refresh once more, just in case
			if( !shouldContinue )
			{
				try
				{
					RefreshInternal();
				}
				catch { }
			}

			return shouldContinue;
		}

		private void RefreshInternal()
		{
			if( pendingStart )
			{
				if( OnStart != null )
					OnStart();

				pendingStart = false;
			}

			if( pendingLog != null )
			{
				if( OnLogReceived != null )
					OnLogReceived( pendingLog );

				pendingLog = null;
			}

			if( pendingProgress != null )
			{
				if( OnProgressChanged != null )
					OnProgressChanged( pendingProgress );

				pendingProgress = null;
			}

			if( pendingOverallProgress != null )
			{
				if( OnOverallProgressChanged != null )
					OnOverallProgressChanged( pendingOverallProgress );

				pendingOverallProgress = null;
			}

			if( pendingStage != null )
			{
				if( OnPatchStageChanged != null )
					OnPatchStageChanged( pendingStage.Value );

				pendingStage = null;
			}

			if( pendingMethod != null )
			{
				if( OnPatchMethodChanged != null )
					OnPatchMethodChanged( pendingMethod.Value );

				pendingMethod = null;
			}

			if( pendingCurrentVersion != null && pendingNewVersion != null )
			{
				if( OnVersionFetched != null )
					OnVersionFetched( pendingCurrentVersion, pendingNewVersion );

				pendingCurrentVersion = null;
				pendingNewVersion = null;
			}

			if( pendingFinish )
			{
				if( OnFinish != null )
					OnFinish();

				pendingFinish = false;
			}
		}
	}
}