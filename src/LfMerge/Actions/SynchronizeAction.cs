﻿// Copyright (c) 2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using Autofac;
using LfMerge.Settings;
using Chorus.sync;
using Chorus.VcsDrivers;
using Palaso.Progress;
using Palaso.Network;

namespace LfMerge.Actions
{
	public class SynchronizeAction: Action
	{
		public SynchronizeAction(LfMergeSettingsIni settings, LfMerge.Logging.ILogger logger) : base(settings, logger) {}

		protected override ProcessingState.SendReceiveStates StateForCurrentAction
		{
			get { return ProcessingState.SendReceiveStates.RECEIVING; }
		}

		protected override void DoRun(ILfProject project)
		{
			using (var scope = MainClass.Container.BeginLifetimeScope())
			{
				GetAction(ActionNames.TransferMongoToFdo).Run(project);

				Logger.Notice("Syncing");
				var projectFolderPath = Path.Combine(Settings.WebWorkDirectory, project.LfProjectCode);
				var config = new ProjectFolderConfiguration(projectFolderPath);
				var synchroniser = Synchronizer.FromProjectConfiguration(config, Progress);
				var options = new SyncOptions();
				var repoPath = RepositoryAddress.Create("Language Depot", getSyncUri(project));
				options.RepositorySourcesToTry.Add(repoPath);
				var syncResult = synchroniser.SyncNow(options);
				if (!syncResult.Succeeded)
				{
					Logger.Error("Sync failed - {0}", syncResult.ErrorEncountered);
					return;
				}
				if (syncResult.DidGetChangesFromOthers)
					Logger.Notice("Received changes from others");
				else
					Logger.Notice("No changes from others");

				GetAction(ActionNames.TransferFdoToMongo).Run(project);
			}
		}

		protected override ActionNames NextActionName
		{
			get { return ActionNames.None; }
		}

		private string getSyncUri(ILfProject project)
		{
			string uri = project.LanguageDepotProjectUri;
			string serverPath = uri.StartsWith("http://") ? uri.Replace("http://", "") : uri;
			return "http://" +
				HttpUtilityFromMono.UrlEncode(project.LanguageDepotProject.Username) + ":" +
				HttpUtilityFromMono.UrlEncode(project.LanguageDepotProject.Password) + "@" + serverPath + "/" +
				HttpUtilityFromMono.UrlEncode(project.LanguageDepotProject.Identifier);
		}

	}
}
