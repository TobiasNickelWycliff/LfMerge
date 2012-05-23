using System;
using System.IO;

namespace lfmergelift
{
	class LangForgeDirectories
	{
		private String _languageForgeServerFolder;
		//These are the folder names for the location of the repositories for the Language Forge projects
		internal const String WebWorkFolder = "WebWork";
		internal const String MergeWorkFolder = "MergeWork";
		//This is the subfolder of the MergeWork foler where the .lift.update files are found.
		internal const String LiftUpdatesFolder = "LiftUpdates";
		internal const String MergeWorkProjectsFolder = "Projects";

		public LangForgeDirectories(String languageForgeServerFolder)
		{
			if (!languageForgeServerFolder.Contains(Path.DirectorySeparatorChar.ToString()))
			{
				throw new ArgumentException("languageForgeServerFolder must be a full path, not just a file name. Path was " + languageForgeServerFolder);
			}
			_languageForgeServerFolder = languageForgeServerFolder;

		}

		/// <summary>
		/// This folder contains a subfolder for each language project's repository and LIFT that the the Language Forge
		/// clients are reading from.
		/// e.g .../LangForge/WebWork/ProjA     .../LangForge/WebWork/ProjB
		/// </summary>
		public string WebWorkPath
		{
			get { return Path.Combine(_languageForgeServerFolder, WebWorkFolder); }
		}

		/// <summary>
		/// This is the location where the updates from language forge clients are done
		/// e.g.  .../LangForge/MergeWork/
		/// </summary>
		public string MergeWorkPath
		{
			get { return Path.Combine(_languageForgeServerFolder, MergeWorkFolder); }
		}

		/// <summary>
		/// This folder contains one folder for each language project the server managing for Language Forge
		/// clients.
		/// e.g. .../LangForge/MergeWork/Projects/ProjA     .../LangForge/MergeWork/Projects/ProjB
		/// </summary>
		public string MergeWorkProjects
		{
			get { return Path.Combine(MergeWorkPath, MergeWorkProjectsFolder); }
		}

		/// <summary>
		/// This folder contains the .lift.update files that are produced by the Language Forge clients and need to be applied
		/// to the appropriate projects under the MergeWorkProjects folder.
		/// e.g.  .../LangForge/MergeWork/LiftUpdates
		/// </summary>
		public string LiftUpdatesPath
		{
			get { return Path.Combine(MergeWorkPath, LiftUpdatesFolder); }
		}

		public void CreateWebWorkFolder()
		{
			Directory.CreateDirectory(WebWorkPath);
		}

		public String CreateWebWorkProjectFolder(String projectName)
		{
			Directory.CreateDirectory(GetProjWebWorkPath(projectName));
			return GetProjWebWorkPath(projectName);
		}


		public void CreateMergeWorkFolder()
		{
			Directory.CreateDirectory(MergeWorkPath);
		}

		public void CreateLiftUpdatesFolder()
		{
			Directory.CreateDirectory(LiftUpdatesPath);
		}

		public void CreateMergeWorkProjectsFolder()
		{
			Directory.CreateDirectory(MergeWorkProjects);
		}

		public String CreateMergeWorkProjectFolder(String projectName)
		{
			Directory.CreateDirectory(GetProjMergeWorkPath(projectName));
			return GetProjMergeWorkPath(projectName);
		}

		public String GetProjWebWorkPath(String projectName)
		{
			return Path.Combine(WebWorkPath, projectName);
		}

		public String GetProjMergeWorkPath(String projectName)
		{
			return Path.Combine(MergeWorkProjects, projectName);
		}
	}
}