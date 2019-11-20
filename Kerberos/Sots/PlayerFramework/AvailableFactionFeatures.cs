// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.AvailableFactionFeatures
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.PlayerFramework
{
	internal class AvailableFactionFeatures
	{
		public GrabBag<string> Avatars { get; private set; }

		public GrabBag<string> Badges { get; private set; }

		public AvailableFactionFeatures(Random random, Faction faction)
		{
			this.Avatars = new GrabBag<string>(random, ((IEnumerable<string>)faction.AvatarTexturePaths).Select<string, string>((Func<string, string>)(x => Path.GetFileNameWithoutExtension(x))));
			this.Badges = new GrabBag<string>(random, ((IEnumerable<string>)faction.BadgeTexturePaths).Select<string, string>((Func<string, string>)(x => Path.GetFileNameWithoutExtension(x))));
		}
	}
}
