﻿// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace LfMerge
{
	public enum Actions
	{
		None,
		UpdateFdoFromMongoDb,
		Commit,
		Receive,
		Merge,
		Send,
		UpdateMongoDbFromFdo
	}
}

