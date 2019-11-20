// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.Player
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.PlayerFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_PLAYER)]
	internal class Player : GameObject, IDisposable
	{
		private static Vector3 _overridePrimaryPlayerColor = Vector3.One;
		private static Vector3 _overrideSecondaryPlayerColor = Vector3.One;
		public static List<Vector3> DefaultPrimaryPlayerColors = new List<Vector3>()
	{
	  new Vector3(1f, 0.0f, 0.0f),
	  new Vector3(1f, 1f, 0.0f),
	  new Vector3(0.0f, 0.0f, 1f),
	  new Vector3(0.7098f, 0.0f, 1f),
	  new Vector3(1f, 0.4902f, 0.0f),
	  new Vector3(0.0f, 1f, 0.0f),
	  new Vector3(0.0f, 1f, 1f),
	  new Vector3(0.79607f, 0.56078f, 0.40784f),
	  new Vector3(0.10196f, 0.52941f, 0.45098f),
	  new Vector3(2.2222f, 0.1765f, 0.5098f)
	};
		public static readonly List<Vector3> DefaultMPPrimaryPlayerColors = new List<Vector3>()
	{
	  new Vector3(1f, 0.0f, 0.0f),
	  new Vector3(1f, 1f, 0.0f),
	  new Vector3(0.0f, 0.0f, 1f),
	  new Vector3(0.7098f, 0.0f, 1f),
	  new Vector3(1f, 0.4902f, 0.0f),
	  new Vector3(0.0f, 1f, 0.0f),
	  new Vector3(0.0f, 1f, 1f),
	  new Vector3(0.79607f, 0.56078f, 0.40784f),
	  new Vector3(0.10196f, 0.52941f, 0.45098f),
	  new Vector3(2.2222f, 0.1765f, 0.5098f)
	};
		public static readonly Vector3[] DefaultPrimaryTeamColors = new Vector3[9]
		{
	  new Vector3(1f, 0.0f, 0.0f),
	  new Vector3(1f, 1f, 0.0f),
	  new Vector3(2.2222f, 0.1765f, 0.5098f),
	  new Vector3(0.7098f, 0.0f, 1f),
	  new Vector3(1f, 0.4902f, 0.0f),
	  new Vector3(0.0f, 1f, 0.0f),
	  new Vector3(0.0f, 1f, 1f),
	  new Vector3(0.79607f, 0.56078f, 0.40784f),
	  new Vector3(0.10196f, 0.52941f, 0.45098f)
		};
		public const double DefaultSuitabilityTolerance = 6.0;
		public const double MinIdealSuitability = 6.0;
		public const double MaxIdealSuitability = 14.0;
		private static bool _overridePlayerColors;
		private PlayerInfo _pi;
		private GameSession game;
		private CivilianRatios m_DesiredCivilianRatios;
		public int _techPointsAtStartOfTurn;
		private Faction _faction;
		private StrategicAI m_AI;
		private App app;

		public static bool CanBuildMiningStations(GameDatabase db, int playerId)
		{
			return db.PlayerHasTech(playerId, "IND_Mega-Strip_Mining");
		}

		public AIStance? Stance
		{
			get
			{
				if (this.GetAI() != null)
					return this.GetAI().LastStance;
				return new AIStance?();
			}
		}

		public AITechStyles TechStyles
		{
			get
			{
				if (this.GetAI() != null)
					return this.GetAI().TechStyles;
				return (AITechStyles)null;
			}
		}

		public static void OverridePlayerColors(Vector3 primary, Vector3 secondary)
		{
			Player._overridePlayerColors = true;
			Player._overridePrimaryPlayerColor = primary;
			Player._overrideSecondaryPlayerColor = secondary;
		}

		public static void RestorePlayerColors()
		{
			Player._overridePlayerColors = false;
		}

		public static Vector3[] GetShuffledPlayerColors(Random rng)
		{
			return Player.DefaultPrimaryPlayerColors.Shuffle<Vector3>(rng).ToArray<Vector3>();
		}

		public void SetEmpireColor(int index)
		{
			this.PostSetProp("Color1", (object)new Vector4(Player.DefaultPrimaryPlayerColors[index % Player.DefaultPrimaryPlayerColors.Count], 1f));
		}

		public void SetPlayerColor(Vector3 value)
		{
			this.PostSetProp("Color2", (object)new Vector4(value, 1f));
		}

		public void SetBadgeTexture(string texturePath)
		{
			this.PostSetProp("Badge", texturePath);
		}

		public string GetName()
		{
			return this._pi.Name;
		}

		public int SubfactionIndex
		{
			get
			{
				return this._pi.SubfactionIndex;
			}
		}

		public Player(App app, GameSession game, PlayerInfo pi, Player.ClientTypes clientType)
		{
			this.game = game;
			this.app = app;
			this._pi = pi;
			Vector3 vector3_1 = pi.PrimaryColor;
			Vector3 vector3_2 = pi.SecondaryColor;
			if (Player._overridePlayerColors)
			{
				vector3_1 = Player._overridePrimaryPlayerColor;
				vector3_2 = Player._overrideSecondaryPlayerColor;
			}
			app.AddExistingObject((IGameObject)this, (object)this.ID, (object)vector3_1, (object)vector3_2, (object)pi.BadgeAssetPath);
			if (game != null)
			{
				FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(pi.FactionID);
				this.SetFaction(app.AssetDatabase.Factions.First<Faction>((Func<Faction, bool>)(x => x.Name == factionInfo.Name)));
			}
			this.SetAI(clientType == Player.ClientTypes.AI);
		}

		public bool IsAI()
		{
			return this.m_AI != null;
		}

		public void SetAI(bool enabled)
		{
			if (enabled && this.game != null)
			{
				if (this._pi.ID != 0)
				{
					this.game.GameDatabase.InsertOrIgnoreAI(this._pi.ID, AIStance.EXPANDING);
					this.m_AI = new StrategicAI(this.game, this);
				}
				this.PostSetProp("SetUseAI", (object)true, (object)(int)this._pi.AIDifficulty);
			}
			else
			{
				this.m_AI = (StrategicAI)null;
				this.PostSetProp("SetUseAI", (object)false, (object)(int)this._pi.AIDifficulty);
			}
		}

		public void ReplaceWithAI()
		{
			this.SetAI(true);
			if (this.m_AI == null)
				return;
			this.m_AI.SetDropInActivationTurn(this.game.GameDatabase.GetTurnCount() + 2);
		}

		public StrategicAI GetAI()
		{
			return this.m_AI;
		}

		public bool InstantDefeatMorrigiRelics()
		{
			if (this._faction != null)
				return this._faction.Name == "morrigi";
			return false;
		}

		public CivilianRatios GetDesiredCivilianRatios()
		{
			return this.m_DesiredCivilianRatios;
		}

		public void SetDesiredCivilianRatios(CivilianRatios ratios)
		{
			this.m_DesiredCivilianRatios = ratios;
		}

		public int ID
		{
			get
			{
				return this._pi.ID;
			}
		}

		public bool IsStandardPlayer
		{
			get
			{
				return this._pi.isStandardPlayer;
			}
		}

		public GameSession Game
		{
			get
			{
				return this.game;
			}
		}

		public PlayerInfo PlayerInfo
		{
			get
			{
				return this._pi;
			}
		}

		public Faction Faction
		{
			get
			{
				return this._faction;
			}
		}

		public void SetFaction(Faction value)
		{
			if (this._faction == value)
				return;
			if (this._faction != null)
				this._faction.ReleaseFactionReference(this.App);
			this._faction = value;
			if (this._faction != null)
				this._faction.AddFactionReference(this.App);
			FactionObject factionObject = this._faction != null ? this._faction.FactionObj : (FactionObject)null;
			this.PostSetProp("Faction", factionObject != null ? factionObject.ObjectID : 0);
		}

		public static double GetSuitabilityTolerance()
		{
			return 6.0;
		}

		public void Dispose()
		{
			if (this._faction != null)
				this._faction.ReleaseFactionReference(this.App);
			this._faction = (Faction)null;
			this.App.ReleaseObject((IGameObject)this);
		}

		public static int GetWeaponLevelFromTechs(LogicalWeapon weapon, List<PlayerTechInfo> techs)
		{
			if (weapon == null || (!((IEnumerable<WeaponEnums.WeaponTraits>)weapon.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(x => x == WeaponEnums.WeaponTraits.Upgradable)) || techs.Count == 0))
				return 1;
			switch (weapon.PayloadType)
			{
				case WeaponEnums.PayloadTypes.Missile:
				case WeaponEnums.PayloadTypes.BattleRider:
					PlayerTechInfo playerTechInfo1 = techs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "WAR_Anti-Matter_Warheads"));
					PlayerTechInfo playerTechInfo2 = techs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "WAR_Reflex_Warheads"));
					if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched)
						return 3;
					if (playerTechInfo1 != null && playerTechInfo1.State == TechStates.Researched)
						return 2;
					break;
			}
			return 1;
		}

		public static int GetPsiResistanceFromTech(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			PlayerTechInfo playerTechInfo = techs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "CYB_PsiShield"));
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				return ab.GetTechBonus<int>("CYB_PsiShield", "psiPower");
			return 0;
		}

		public static bool HasNodeDriveTech(List<PlayerTechInfo> techs)
		{
			return techs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.State != TechStates.Researched)
				   return false;
			   if (!(x.TechFileID == "DRV_Node") && !(x.TechFileID == "DRV_Node_Focusing"))
				   return x.TechFileID == "DRV_Node_Pathing";
			   return true;
		   }));
		}

		public static bool HasWarpPulseTech(List<PlayerTechInfo> techs)
		{
			return techs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.State == TechStates.Researched)
				   return x.TechFileID == "DRV_Warp_Pulse";
			   return false;
		   }));
		}

		public static float GetSubversionRange(
		  AssetDatabase ab,
		  List<PlayerTechInfo> techs,
		  bool isLoa)
		{
			PlayerTechInfo playerTechInfo = techs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "PSI_Subversion"));
			if (isLoa || playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				return ab.GetTechBonus<float>("PSI_Subversion", "range");
			return 0.0f;
		}

		public static float GetPowerBonus(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			float num = 0.0f;
			if (techs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.State == TechStates.Researched)
				   return x.TechFileID == "CYB_InFldManip";
			   return false;
		   })))
				num += ab.GetTechBonus<float>("CYB_InFldManip", "power");
			return num;
		}

		public static float GetKineticDampeningValue(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			float num = 1f;
			if (techs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.State == TechStates.Researched)
				   return x.TechFileID == "NRG_Internal_Kinetic_Dampers";
			   return false;
		   })))
				num += ab.GetTechBonus<float>("NRG_Internal_Kinetic_Dampers", "force");
			return num;
		}

		public static float GetPDAccuracyBonus(AssetDatabase ab, List<PlayerTechInfo> techs)
		{
			float num = 0.0f;
			if (techs.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.State == TechStates.Researched)
				   return x.TechFileID == "PSI_MechaEmpathy";
			   return false;
		   })))
				num += ab.GetTechBonus<float>("PSI_MechaEmpathy", "pdaccuracy");
			return num;
		}

		public static WeaponTechModifiers ObtainWeaponTechModifiers(
		  AssetDatabase ab,
		  WeaponEnums.TurretClasses tc,
		  LogicalWeapon weapon,
		  IEnumerable<PlayerTechInfo> playerTechs)
		{
			WeaponTechModifiers weaponTechModifiers = new WeaponTechModifiers();
			weaponTechModifiers.DamageModifier = 0.0f;
			weaponTechModifiers.SpeedModifier = 0.0f;
			weaponTechModifiers.AccelModifier = 0.0f;
			weaponTechModifiers.MassModifier = 0.0f;
			weaponTechModifiers.ROFModifier = 1f;
			weaponTechModifiers.RangeModifier = 0.0f;
			weaponTechModifiers.SmartNanites = false;
			if (weapon == null || playerTechs == null || playerTechs.Count<PlayerTechInfo>() == 0)
				return weaponTechModifiers;
			if (tc != WeaponEnums.TurretClasses.Torpedo && weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt)
			{
				PlayerTechInfo playerTechInfo1 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "BAL_Neutronium_Rounds"));
				PlayerTechInfo playerTechInfo2 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "BAL_Acceleration_Amplification"));
				if (playerTechInfo1 != null && playerTechInfo1.State == TechStates.Researched)
				{
					weaponTechModifiers.DamageModifier += ab.GetTechBonus<float>(playerTechInfo1.TechFileID, "damage");
					weaponTechModifiers.MassModifier += ab.GetTechBonus<float>(playerTechInfo1.TechFileID, "mass");
				}
				if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched)
				{
					weaponTechModifiers.DamageModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "damage");
					weaponTechModifiers.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "speed");
				}
			}
			if (weapon.PayloadType == WeaponEnums.PayloadTypes.Missile)
			{
				PlayerTechInfo playerTechInfo1 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "WAR_MicroFusion_Drives"));
				PlayerTechInfo playerTechInfo2 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "NRG_Ionic_Thruster"));
				if (playerTechInfo1 != null && playerTechInfo1.State == TechStates.Researched)
				{
					weaponTechModifiers.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo1.TechFileID, "speed");
					weaponTechModifiers.RangeModifier += ab.GetTechBonus<float>(playerTechInfo1.TechFileID, "range");
				}
				if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched)
				{
					weaponTechModifiers.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "speed");
					weaponTechModifiers.AccelModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "accel");
				}
			}
			if (weapon.PayloadType == WeaponEnums.PayloadTypes.Beam)
			{
				PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "NRG_Quantum_Capacitors"));
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
					weaponTechModifiers.ROFModifier += ab.GetTechBonus<float>(playerTechInfo.TechFileID, "rateoffire");
			}
			if (((IEnumerable<WeaponEnums.WeaponTraits>)weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic))
			{
				PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "BAL_VRF_Systems"));
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
					weaponTechModifiers.ROFModifier += ab.GetTechBonus<float>(playerTechInfo.TechFileID, "rateoffire");
			}
			if (((IEnumerable<WeaponEnums.WeaponTraits>)weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Nanite))
			{
				PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "IND_Smart_Nanites"));
				weaponTechModifiers.SmartNanites = playerTechInfo != null && playerTechInfo.State == TechStates.Researched;
			}
			return weaponTechModifiers;
		}

		public static ShipSpeedModifiers GetShipSpeedModifiers(
		  AssetDatabase ab,
		  Player player,
		  RealShipClasses shipClass,
		  IEnumerable<PlayerTechInfo> playerTechs,
		  bool isDeepSpace)
		{
			ShipSpeedModifiers shipSpeedModifiers = new ShipSpeedModifiers();
			shipSpeedModifiers.SpeedModifier = 1f;
			shipSpeedModifiers.RotSpeedModifier = 1f;
			shipSpeedModifiers.LinearAccelModifier = 1f;
			shipSpeedModifiers.RotAccelModifier = 1f;
			if (player != null)
			{
				PlayerTechInfo playerTechInfo1 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "NRG_Ionic_Thruster"));
				if (playerTechInfo1 != null && playerTechInfo1.State == TechStates.Researched)
				{
					float techBonus1 = ab.GetTechBonus<float>(playerTechInfo1.TechFileID, "shiprot");
					float techBonus2 = ab.GetTechBonus<float>(playerTechInfo1.TechFileID, "shipaccel");
					shipSpeedModifiers.RotSpeedModifier += techBonus1;
					shipSpeedModifiers.LinearAccelModifier += techBonus2;
					shipSpeedModifiers.RotAccelModifier += techBonus2;
				}
				PlayerTechInfo playerTechInfo2 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "NRG_Small_Scale_Fusion"));
				if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched && (shipClass == RealShipClasses.AssaultShuttle || shipClass == RealShipClasses.Drone || shipClass == RealShipClasses.Biomissile))
					shipSpeedModifiers.SpeedModifier += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "speed");
				if (isDeepSpace && player.Faction.Name == "liir_zuul")
				{
					shipSpeedModifiers.SpeedModifier += 0.2f;
					shipSpeedModifiers.RotSpeedModifier += 0.2f;
					shipSpeedModifiers.LinearAccelModifier += 0.2f;
					shipSpeedModifiers.RotAccelModifier += 0.2f;
				}
			}
			return shipSpeedModifiers;
		}

		public enum ClientTypes
		{
			User,
			AI,
		}
	}
}
