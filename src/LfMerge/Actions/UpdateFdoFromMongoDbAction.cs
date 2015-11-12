﻿// Copyright (c) 2015 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using LfMerge.LanguageForge.Config;
using LfMerge.LanguageForge.Model;
using SIL.FieldWorks.FDO;

namespace LfMerge.Actions
{
	public class UpdateFdoFromMongoDbAction: Action
	{
		protected override ProcessingState.SendReceiveStates StateForCurrentAction
		{
			get { return ProcessingState.SendReceiveStates.QUEUED; }
		}

		protected override void DoRun(ILfProject project)
		{
			ILfProjectConfig config = GetConfigForTesting(project);
			if (config == null)
				return;
			IEnumerable<LfLexEntry> lexicon = GetLexiconForTesting(project, config);
			foreach (LfLexEntry entry in lexicon)
			{
				Console.Write("{0}: ", entry.Guid);
				if (entry.Lexeme != null && entry.Lexeme.Values != null)
					Console.WriteLine(String.Join(", ", entry.Lexeme.Values.Select(x => (x.Value == null) ? "" : x.Value)));
				foreach(LfSense sense in entry.Senses)
				{
					if (sense.PartOfSpeech != null)
					{
						if (sense.PartOfSpeech.Value != null)
							Console.Write(" - " + sense.PartOfSpeech.Value);
					}
					Console.WriteLine();
				}
			}

			if (project.FieldWorksProject == null)
			{
				Console.WriteLine("Failed to find the corresponding FieldWorks project!");
				return;
			}
			FdoCache cache = project.FieldWorksProject.Cache;
			if (cache == null)
			{
				Console.WriteLine("Failed to find the FDO cache!");
				var fwProject = project.FieldWorksProject;
				Console.WriteLine(fwProject.IsDisposed);
				return;
			}

			IFdoServiceLocator servLoc = cache.ServiceLocator;
			Console.WriteLine("Got the service locator");
			ILexSenseRepository senseRepo = servLoc.GetInstance<ILexSenseRepository>();
			Console.WriteLine("Got the sense repository");

			lexicon = GetLexiconForTesting(project, config);
			foreach (LfLexEntry entry in lexicon)
			{
				string entryName = String.Join(", ", entry.Lexeme.Values.Select(x => (x.Value == null) ? "" : x.Value));
				Console.WriteLine("Checking entry {0} in lexicon", entryName);
				if (entry.Senses == null) continue;
				foreach(LfSense sense in entry.Senses)
				{
					if (sense.Definition == null)
					{
						Console.WriteLine("No definition found in LF entry");
						continue;
					}
					string senseName = String.Join(", ", sense.Definition.Values.Select(x => (x.Value == null) ? "" : x.Value));
					Console.WriteLine("Checking sense {0} in entry {1} in lexicon", senseName, entryName);
					Guid liftId;
					ILexSense fwSense = null;
					if (Guid.TryParse(sense.LiftId, out liftId))
						fwSense = senseRepo.GetObject(liftId);
					// TODO: Handle GUID parsing failures
					if (fwSense == null)
					{
						Console.WriteLine("Didn't find sense {0} in FDO", sense.LiftId);
						continue;
					}
					IMultiString definition = fwSense.Definition;
					if (definition == null)
					{
						Console.WriteLine("Didn't find definition for {0} in FDO", sense.LiftId);
						continue;
					}

					Console.WriteLine("Definition: {0}", definition.BestAnalysisVernacularAlternative.Text);
					foreach (int wsid in definition.AvailableWritingSystemIds)
					{
						string wsid_str = servLoc.WritingSystemManager.GetStrFromWs(wsid);
						var text = definition.get_String(wsid);
						Console.WriteLine("{0}: {1}", wsid_str, text.Text);
					}
				}
			}
		}

		private IEnumerable<LfLexEntry> GetLexiconForTesting(ILfProject project, ILfProjectConfig config)
		{
			var db = MongoConnection.Default.GetProjectDatabase(project);
			var collection = db.GetCollection<LfLexEntry>("lexicon");
			IAsyncCursor<LfLexEntry> result2 = collection.Find<LfLexEntry>(_ => true).ToCursorAsync().Result;
			return result2.AsEnumerable();
		}

		private ILfProjectConfig GetConfigForTesting(ILfProject project)
		{
			MongoProjectRecord projectRecord = MongoProjectRecord.Create(project);
			if (projectRecord == null)
			{
				Console.WriteLine("No project named {0}", project.LfProjectCode);
				Console.WriteLine("If we are unit testing, this may not be an error");
				return null;
			}
			ILfProjectConfig config = projectRecord.Config;
			Console.WriteLine(config.GetType()); // Should be LfMerge.LanguageForge.Config.LfProjectConfig
			Console.WriteLine(config.Entry.Type);
			Console.WriteLine(String.Join(", ", config.Entry.FieldOrder));
			Console.WriteLine(config.Entry.Fields["lexeme"].Type);
			Console.WriteLine(config.Entry.Fields["lexeme"].GetType());
			return config;
		}

		protected override ActionNames NextActionName
		{
			get { return ActionNames.None; }
		}
	}
}

