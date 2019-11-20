// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DesignInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal class DesignInfo : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public int Armour;
		public float Structure;
		public int NumTurrets;
		public float Mass;
		public float Acceleration;
		public float TopSpeed;
		public int SavingsCost;
		public int ProductionCost;
		public ShipClass Class;
		public int CrewAvailable;
		public int PowerAvailable;
		public int SupplyAvailable;
		public int CrewRequired;
		public int PowerRequired;
		public int SupplyRequired;
		public int NumBuilt;
		public int DesignDate;
		public bool isPrototyped;
		public bool isAttributesDiscovered;
		public ShipRole Role;
		public WeaponRole WeaponRole;
		public DesignSectionInfo[] DesignSections;
		public StationType StationType;
		public int StationLevel;
		public float TacSensorRange;
		public float StratSensorRange;
		public string PriorityWeaponName;
		public int NumDestroyed;
		public int RetrofitBaseID;

		public int ID { get; set; }

		public void HackValidateRole()
		{
			if (this.Class != ShipClass.Station || this.Role == ShipRole.PLATFORM)
				return;
			this.Role = ShipRole.UNDEFINED;
		}

		public bool IsAccelerator()
		{
			foreach (DesignSectionInfo designSection in this.DesignSections)
			{
				if (designSection.ShipSectionAsset.IsAccelerator)
					return true;
			}
			return false;
		}

		public bool IsLoaCube()
		{
			foreach (DesignSectionInfo designSection in this.DesignSections)
			{
				if (designSection.ShipSectionAsset.IsLoaCube)
					return true;
			}
			return false;
		}

		public bool IsSuperTransport()
		{
			return ((IEnumerable<DesignSectionInfo>)this.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.IsSuperTransport));
		}

		public bool IsPoliceShip()
		{
			return ((IEnumerable<DesignSectionInfo>)this.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x =>
		  {
			  if (x.ShipSectionAsset.Type == ShipSectionType.Mission)
				  return x.ShipSectionAsset.isPolice;
			  return false;
		  }));
		}

		public bool IsSDB()
		{
			return ((IEnumerable<DesignSectionInfo>)this.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.RealClass == RealShipClasses.SystemDefenseBoat));
		}

		public bool IsMinelayer()
		{
			return ((IEnumerable<DesignSectionInfo>)this.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.isMineLayer));
		}

		public bool IsSuulka()
		{
			return ((IEnumerable<DesignSectionInfo>)this.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.IsSuulka));
		}

		public bool CanHaveAttributes()
		{
			if (((IEnumerable<DesignSectionInfo>)this.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.isPropaganda)) || this.IsPlatform() || this.IsPoliceShip())
				return false;
			if (this.GetRealShipClass().HasValue)
			{
				RealShipClasses? realShipClass = this.GetRealShipClass();
				if ((realShipClass.GetValueOrDefault() != RealShipClasses.Drone ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
					return false;
			}
			if (this.GetRealShipClass().HasValue)
			{
				RealShipClasses? realShipClass = this.GetRealShipClass();
				if ((realShipClass.GetValueOrDefault() != RealShipClasses.BattleRider ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
					return false;
			}
			return true;
		}

		public PlatformTypes? GetPlatformType()
		{
			return this.GetMissionSectionAsset()?.GetPlatformType();
		}

		public int GetPlayerProductionCost(
		  GameDatabase db,
		  int player,
		  bool isPrototype,
		  float? overrideProductionCost = null)
		{
			float num1 = overrideProductionCost.HasValue ? overrideProductionCost.Value : (float)this.ProductionCost;
			float num2 = 1f;
			float num3 = 1f;
			switch (this.Class)
			{
				case ShipClass.Cruiser:
					num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierCR, player);
					if (isPrototype)
					{
						num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierCR, player);
						break;
					}
					break;
				case ShipClass.Dreadnought:
					num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierDN, player);
					if (isPrototype)
					{
						num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierDN, player);
						break;
					}
					break;
				case ShipClass.Leviathan:
					num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierLV, player);
					if (isPrototype)
					{
						num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierLV, player);
						break;
					}
					break;
				case ShipClass.Station:
					RealShipClasses? realShipClass = this.GetRealShipClass();
					if ((realShipClass.GetValueOrDefault() != RealShipClasses.Platform ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
					{
						num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierSN, player);
						if (isPrototype)
						{
							num3 = db.GetStratModifierFloatToApply(StratModifiers.PrototypeConstructionCostModifierPF, player);
							break;
						}
						break;
					}
					num2 = db.GetStratModifierFloatToApply(StratModifiers.ConstructionCostModifierSN, player);
					break;
			}
			return (int)((double)num1 * (double)num2 * (double)num3);
		}

		public int CommandPointCost
		{
			get
			{
				int num = 0;
				switch (this.Class)
				{
					case ShipClass.Cruiser:
						num = 6;
						break;
					case ShipClass.Dreadnought:
						num = 18;
						break;
					case ShipClass.Leviathan:
						num = 54;
						break;
				}
				return num;
			}
		}

		public DesignInfo()
		{
		}

		public DesignInfo(int playerID, string name, params string[] sections)
		  : this(playerID, name, (IEnumerable<string>)sections)
		{
		}

		public DesignInfo(int playerID, string name, IEnumerable<string> sections)
		{
			this.PlayerID = playerID;
			this.Name = name;
			this.DesignSections = sections.Select<string, DesignSectionInfo>((Func<string, DesignSectionInfo>)(x => new DesignSectionInfo()
			{
				FilePath = x,
				DesignInfo = this
			})).ToArray<DesignSectionInfo>();
		}

		public int GetEndurance(GameSession game)
		{
			float stratModifier = game.GameDatabase.GetStratModifier<float>(StratModifiers.ShipSupplyModifier, this.PlayerID);
			float supplyAvailable = (float)this.SupplyAvailable;
			float supplyRequired = (float)this.SupplyRequired;
			float crewRequired = (float)this.CrewRequired;
			return Math.Max((int)(((double)supplyAvailable * (double)stratModifier - (double)supplyRequired) / ((double)Math.Max(crewRequired, 2f) / 2.0)), 1);
		}

		public int GetCommandPoints()
		{
			ShipSectionAsset missionSectionAsset = this.GetMissionSectionAsset();
			if (missionSectionAsset == null)
				return 0;
			return missionSectionAsset.CommandPoints;
		}

		public ShipSectionAsset GetMissionSectionAsset()
		{
			return ((IEnumerable<DesignSectionInfo>)this.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(y => y.Type == ShipSectionType.Mission));
		}

		public RealShipClasses? GetRealShipClass()
		{
			return this.GetMissionSectionAsset()?.RealClass;
		}

		public ShipSectionAsset GetCommandSectionAsset(AssetDatabase assetdb)
		{
			foreach (DesignSectionInfo designSection in this.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = assetdb.GetShipSectionAsset(designSection.FilePath);
				if (shipSectionAsset.Type == ShipSectionType.Command)
					return shipSectionAsset;
			}
			return (ShipSectionAsset)null;
		}

		public bool IsPlatform()
		{
			RealShipClasses? realShipClass = this.GetRealShipClass();
			if (realShipClass.GetValueOrDefault() == RealShipClasses.Platform)
				return realShipClass.HasValue;
			return false;
		}

		public override string ToString()
		{
			return string.Format("ID={0},Name={1},Role={2},WeaponRole={3}", (object)this.ID, (object)this.Name, (object)this.Role, (object)this.WeaponRole);
		}
	}
}
