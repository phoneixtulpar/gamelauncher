﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameLauncherCore
{
	[XmlRoot( "ProjectInfo" )]
	public class ProjectInfo
	{
		internal const int LATEST_VERSION = 2;

		[XmlAttribute( AttributeName = "Surum" )]
		public int Version;

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				if( !PatchUtils.IsProjectNameValid( value ) )
					throw new FormatException( Localization.Get( LocalizationID.E_XContainsInvalidCharacters, "'Name'" ) );

				m_name = value;
			}
		}

		public bool CreateRepairPatch;
		public bool CreateInstallerPatch;
		public bool CreateIncrementalPatch;
		[XmlElement( ElementName = "CreateIncrementalPatchesFromEachPreviousVersionToNewVersion" )]
		public bool CreateAllIncrementalPatches;

		public bool DontCreatePatchFilesForUnchangedFiles;

		public int BinaryDiffQuality;

		public CompressionFormat CompressionFormatRepairPatch;
		public CompressionFormat CompressionFormatInstallerPatch;
		public CompressionFormat CompressionFormatIncrementalPatch;

		public string BaseDownloadURL;
		public string MaintenanceCheckURL;
		public List<string> IgnoredPaths;
		public bool IsSelfPatchingApp;

		public ProjectInfo()
		{
			Version = LATEST_VERSION;
			Name = "NewProject";

			CreateRepairPatch = true;
			CreateInstallerPatch = true;
			CreateIncrementalPatch = true;
			CreateAllIncrementalPatches = false;

			DontCreatePatchFilesForUnchangedFiles = false;
			BinaryDiffQuality = 3;

			CompressionFormatRepairPatch = CompressionFormat.NONE;
			CompressionFormatInstallerPatch = CompressionFormat.NONE;
			CompressionFormatIncrementalPatch = CompressionFormat.NONE;

			BaseDownloadURL = "";
			MaintenanceCheckURL = "";
			IgnoredPaths = new List<string>();
			IsSelfPatchingApp = true;
		}
	}
}