// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.AdmiralInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class AdmiralInfo : IIDProvider
	{
		public int PlayerID;
		public int? HomeworldID;
		public string Name;
		public string Race;
		public float Age;
		public string Gender;
		public int ReactionBonus;
		public int EvasionBonus;
		public int Loyalty;
		public int BattlesFought;
		public int BattlesWon;
		public int MissionsAssigned;
		public int MissionsAccomplished;
		public int TurnCreated;
		public bool Engram;

		public static string GetTraitDescription(AdmiralInfo.TraitType t, int level)
		{
			return string.Format(App.Localize(string.Format("@ADMIRALTRAITS_{0}_DESC", (object)t.ToString().ToUpper())), (object)level, (object)(10 * level));
		}

		public static bool IsGoodTrait(AdmiralInfo.TraitType t)
		{
			switch (t)
			{
				case AdmiralInfo.TraitType.Thrifty:
				case AdmiralInfo.TraitType.Pathfinder:
				case AdmiralInfo.TraitType.Slippery:
				case AdmiralInfo.TraitType.TrueBeliever:
				case AdmiralInfo.TraitType.GoodShepherd:
				case AdmiralInfo.TraitType.GreenThumb:
				case AdmiralInfo.TraitType.DrillSergeant:
				case AdmiralInfo.TraitType.Vigilant:
				case AdmiralInfo.TraitType.Architect:
				case AdmiralInfo.TraitType.Inquisitor:
				case AdmiralInfo.TraitType.Evangelist:
				case AdmiralInfo.TraitType.HeadHunter:
				case AdmiralInfo.TraitType.TrueGrit:
				case AdmiralInfo.TraitType.Hunter:
				case AdmiralInfo.TraitType.Defender:
				case AdmiralInfo.TraitType.Attacker:
				case AdmiralInfo.TraitType.ArtilleryExpert:
				case AdmiralInfo.TraitType.Psion:
				case AdmiralInfo.TraitType.MediaHero:
				case AdmiralInfo.TraitType.Sherman:
				case AdmiralInfo.TraitType.Elite:
					return true;
				default:
					return false;
			}
		}

		public static bool AreTraitsMutuallyExclusive(
		  AdmiralInfo.TraitType tA,
		  AdmiralInfo.TraitType tB)
		{
			switch (tA)
			{
				case AdmiralInfo.TraitType.Pathfinder:
					return tB == AdmiralInfo.TraitType.Livingstone;
				case AdmiralInfo.TraitType.Livingstone:
					return tB == AdmiralInfo.TraitType.Pathfinder;
				case AdmiralInfo.TraitType.GoodShepherd:
					return tB == AdmiralInfo.TraitType.BadShepherd;
				case AdmiralInfo.TraitType.BadShepherd:
					return tB == AdmiralInfo.TraitType.GoodShepherd;
				case AdmiralInfo.TraitType.GreenThumb:
					return tB == AdmiralInfo.TraitType.BlackThumb;
				case AdmiralInfo.TraitType.BlackThumb:
					return tB == AdmiralInfo.TraitType.GreenThumb;
				case AdmiralInfo.TraitType.Psion:
					return tB == AdmiralInfo.TraitType.Skeptic;
				case AdmiralInfo.TraitType.Skeptic:
					return tB == AdmiralInfo.TraitType.Psion;
				default:
					return false;
			}
		}

		public static bool CanRaceHaveTrait(AdmiralInfo.TraitType t, string race)
		{
			switch (t)
			{
				case AdmiralInfo.TraitType.Inquisitor:
					if (!(race == "hordezuul"))
						return race == "presterzuul";
					return true;
				case AdmiralInfo.TraitType.Evangelist:
					if (!(race == "liir"))
						return race == "presterzuul";
					return true;
				case AdmiralInfo.TraitType.HeadHunter:
					return race == "morrigi";
				case AdmiralInfo.TraitType.TrueGrit:
					return race == "human";
				case AdmiralInfo.TraitType.Psion:
				case AdmiralInfo.TraitType.Skeptic:
				case AdmiralInfo.TraitType.Technophobe:
					return race != "loa";
				default:
					return true;
			}
		}

		public int ID { get; set; }

		public string GetAdmiralSoundCueContext(AssetDatabase assetdb)
		{
			switch (this.Race)
			{
				case "human":
				case "tarkas":
				case "morrigi":
					switch (this.Gender)
					{
						case "male":
							return "A_";
						case "female":
							return "B_";
					}
					break;
				case "hiver":
				case "hordezuul":
				case "presterzuul":
				case "liir":
					return this.ID % 2 == 0 ? "A_" : "B_";
			}
			return "";
		}

		public override string ToString()
		{
			return string.Format("ID={0},Name={1},Race={2}", (object)this.ID, (object)this.Name, (object)this.Race.ToString());
		}

		public enum TraitType
		{
			Thrifty,
			Wastrel,
			Pathfinder,
			Slippery,
			Livingstone,
			Conscript,
			TrueBeliever,
			GoodShepherd,
			BadShepherd,
			GreenThumb,
			BlackThumb,
			DrillSergeant,
			Vigilant,
			Architect,
			Inquisitor,
			Evangelist,
			HeadHunter,
			TrueGrit,
			Hunter,
			Defender,
			Attacker,
			ArtilleryExpert,
			Psion,
			Skeptic,
			MediaHero,
			GloryHound,
			Sherman,
			Technophobe,
			Elite,
		}
	}
}
