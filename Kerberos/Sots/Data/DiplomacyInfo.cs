// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DiplomacyInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.Data
{
	internal class DiplomacyInfo : IIDProvider
	{
		public static int DefaultDeplomacyRelations = 1000;
		public static int MinDeplomacyRelations = 0;
		public static int MaxDeplomacyRelations = 2000;
		public int PlayerID;
		public int TowardsPlayerID;
		public DiplomacyState State;
		public bool isEncountered;
		private int _relations;

		public int ID { get; set; }

		public int Relations
		{
			get
			{
				return this._relations;
			}
			set
			{
				this._relations = value;
				this._relations.Clamp(DiplomacyInfo.MinDeplomacyRelations, DiplomacyInfo.MaxDeplomacyRelations);
			}
		}

		public DiplomaticMood GetDiplomaticMood()
		{
			if (this.Relations <= 200)
				return DiplomaticMood.Hatred;
			if (this.Relations <= 600)
				return DiplomaticMood.Hostility;
			if (this.Relations <= 900)
				return DiplomaticMood.Distrust;
			if (this.Relations <= 1100)
				return DiplomaticMood.Indifference;
			if (this.Relations <= 1400)
				return DiplomaticMood.Trust;
			return this.Relations <= 1800 ? DiplomaticMood.Friendship : DiplomaticMood.Love;
		}

		public static string GetDiplomaticMoodSprite(DiplomaticMood mood)
		{
			switch (mood)
			{
				case DiplomaticMood.Hatred:
					return "Hate";
				case DiplomaticMood.Love:
					return "Love";
				default:
					return null;
			}
		}

		public string GetDiplomaticMoodSprite()
		{
			return DiplomacyInfo.GetDiplomaticMoodSprite(this.GetDiplomaticMood());
		}
	}
}
