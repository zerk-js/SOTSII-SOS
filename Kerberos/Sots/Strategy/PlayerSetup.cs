// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.PlayerSetup
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class PlayerSetup
	{
		private string _avatar;

		public string Name { get; set; }

		public string Faction { get; set; }

		public int SubfactionIndex { get; set; }

		public AIDifficulty AIDifficulty { get; set; }

		public Vector3 ShipColor { get; set; }

		public bool AI { get; set; }

		public bool Fixed { get; set; }

		public bool Locked { get; set; }

		public bool localPlayer { get; set; }

		public int InitialColonies { get; set; }

		public int InitialTechs { get; set; }

		public int InitialTreasury { get; set; }

		public int Team { get; set; }

		public int databaseId
		{
			get
			{
				return this.slot + 1;
			}
		}

		public int slot { get; set; }

		public int? EmpireColor { get; set; }

		public string Avatar
		{
			get
			{
				return this._avatar;
			}
			set
			{
				this._avatar = value;
			}
		}

		public string Badge { get; set; }

		public string EmpireName { get; set; }

		public bool Ready { get; set; }

		public NPlayerStatus Status { get; set; }

		public string GetBadgeTextureAssetPath(AssetDatabase assetdb)
		{
			if (string.IsNullOrEmpty(this.Badge) || string.IsNullOrEmpty(this.Faction))
				return string.Empty;
			Kerberos.Sots.PlayerFramework.Faction faction = assetdb.GetFaction(this.Faction);
			if (faction == null)
				return string.Empty;
			string path = ((IEnumerable<string>)faction.BadgeTexturePaths).FirstOrDefault<string>((Func<string, bool>)(x => Path.GetFileNameWithoutExtension(x).ToLowerInvariant() == this.Badge.ToLowerInvariant()));
			if (path == null)
				return string.Empty;
			return Path.Combine("factions", faction.Name, "badges", Path.GetFileNameWithoutExtension(path) + ".tga");
		}

		public string GetAvatarTextureAssetPath(AssetDatabase assetdb)
		{
			if (string.IsNullOrEmpty(this.Avatar) || string.IsNullOrEmpty(this.Faction))
				return string.Empty;
			Kerberos.Sots.PlayerFramework.Faction faction = assetdb.GetFaction(this.Faction);
			if (faction == null)
				return string.Empty;
			string path = ((IEnumerable<string>)faction.AvatarTexturePaths).FirstOrDefault<string>((Func<string, bool>)(x => Path.GetFileNameWithoutExtension(x).ToLowerInvariant() == this.Avatar.ToLowerInvariant()));
			if (path == null)
				return string.Empty;
			return Path.Combine("factions", faction.Name, "avatars", Path.GetFileNameWithoutExtension(path) + ".tga");
		}

		public PlayerSetup()
		{
			this.Name = string.Empty;
			this.EmpireName = string.Empty;
			this.ShipColor = Vector3.One;
			this.Avatar = null;
			this.Badge = null;
			this.AI = false;
			this.AIDifficulty = AIDifficulty.Normal;
			this.InitialTreasury = 500000;
			this.InitialColonies = 1;
			this.InitialTechs = 0;
			this.localPlayer = false;
			this.Ready = false;
			this.Locked = false;
			this.slot = 0;
			this.Team = 0;
		}

		public void FinalizeSetup(App game, AvailablePlayerFeatures availableFeatures)
		{
			if (string.IsNullOrEmpty(this.Faction))
				this.Faction = App.GetSafeRandom().Choose<Kerberos.Sots.PlayerFramework.Faction>(availableFeatures.Factions.Keys).Name;
			Kerberos.Sots.PlayerFramework.Faction faction = game.AssetDatabase.GetFaction(this.Faction);
			if (string.IsNullOrEmpty(this.Name))
				this.Name = "Player";
			if (string.IsNullOrEmpty(this.EmpireName))
				this.EmpireName = AssetDatabase.CommonStrings.Localize(faction.EmpireNames.GetNextStringID());
			if (string.IsNullOrEmpty(this.Avatar))
				this.Avatar = Path.GetFileNameWithoutExtension(availableFeatures.Factions[faction].Avatars.TakeRandom());
			if (string.IsNullOrEmpty(this.Badge))
				this.Badge = Path.GetFileNameWithoutExtension(availableFeatures.Factions[faction].Badges.TakeRandom());
			if (this.EmpireColor.HasValue)
				return;
			this.EmpireColor = new int?(availableFeatures.EmpireColors.TakeRandom());
		}
	}
}
