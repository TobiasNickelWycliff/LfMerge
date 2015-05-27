﻿// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using NUnit.Framework;

namespace LfMergeLift.Tests
{
	[TestFixture]
	public class ProcessingStateTests
	{
		private TestEnvironment _env;

		[SetUp]
		public void Setup()
		{
			_env = new TestEnvironment();
		}

		[TearDown]
		public void TearDown()
		{
			_env.Dispose();
		}

		[Test]
		public void LfMergeStateDirectory_Correct()
		{
			Assert.That(_env.LangForgeDirFinder.StatePath,
				Is.EqualTo(Path.Combine(_env.LanguageForgeFolder, "state")));
		}

		[Test]
		public void ProjectStateFile_Correct()
		{
			Assert.That(_env.LangForgeDirFinder.LfMergeStateFile("ProjA"),
				Is.EqualTo(Path.Combine(_env.LanguageForgeFolder, Path.Combine("state", "ProjA.state"))));
		}

		[Test]
		public void Serialization_Roundtrip()
		{
			var expectedState = new ProcessingState("ProjA") {
				SRState = ProcessingState.SendReceiveStates.QUEUED,
				LastStateChangeTicks = DateTime.Now.Ticks,
				PercentComplete = 50,
				ElapsedTimeSeconds = 10,
				TimeRemainingSeconds = 20,
				TotalSteps = 5,
				CurrentStep = 1,
				RetryCounter = 2,
				UncommittedEditCounter = 0
			};
			ProcessingState.Serialize(expectedState);
			var state = ProcessingState.Deserialize("ProjA");

			Assert.That(state, Is.EqualTo(expectedState));
		}

		[Test]
		public void Deserialization_NonexistingStateFile_ReturnsDefault()
		{
			var state = ProcessingState.Deserialize("ProjB");
			Assert.That(state.ProjectCode, Is.EqualTo("ProjB"));
			Assert.That(state.SRState, Is.EqualTo(ProcessingState.SendReceiveStates.QUEUED));
		}

		[Test]
		public void Deserialization_FromFile()
		{
			var expectedState = new ProcessingState("ProjC") {
				SRState = ProcessingState.SendReceiveStates.QUEUED,
				LastStateChangeTicks = 635683277459459160,
				PercentComplete = 30,
				ElapsedTimeSeconds = 40,
				TimeRemainingSeconds = 50,
				TotalSteps = 3,
				CurrentStep = 2,
				RetryCounter = 1,
				UncommittedEditCounter = 0
			};
			const string json = "{\"SRState\":0,\"LastStateChangeTicks\":635683277459459160," +
				"\"PercentComplete\":30,\"ElapsedTimeSeconds\":40,\"TimeRemainingSeconds\":50," +
				"\"TotalSteps\":3,\"CurrentStep\":2,\"RetryCounter\":1,\"UncommittedEditCounter\":0," +
				"\"ErrorMessage\":null,\"ErrorCode\":0,\"ProjectCode\":\"ProjC\"}";

			LfDirectoriesAndFiles.Current.CreateStateFolder();
			var filename = LfDirectoriesAndFiles.Current.LfMergeStateFile("ProjC");
			File.WriteAllText(filename, json);

			var state = ProcessingState.Deserialize("ProjC");
			Assert.That(state, Is.EqualTo(expectedState));
		}
	}
}

