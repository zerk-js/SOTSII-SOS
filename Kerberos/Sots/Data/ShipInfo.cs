// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal class ShipInfo : IIDProvider
	{
		public int FleetID;
		public int DesignID;
		public int ParentID;
		public string ShipName;
		public int SerialNumber;
		public int ComissionDate;
		public ShipParams Params;
		public int RiderIndex;
		public int PsionicPower;
		public double SlavesObtained;
		public int LoaCubes;
		public Vector3? ShipFleetPosition;
		public Matrix? ShipSystemPosition;
		public int? AIFleetID;
		public DesignInfo DesignInfo;

		public int ID { get; set; }

		public static int GetMaxPsionicPower(
		  App app,
		  DesignInfo di,
		  List<AdmiralInfo.TraitType> admiralTraits)
		{
			if (((IEnumerable<DesignSectionInfo>)di.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.IsSuulka)))
				return (int)((IEnumerable<DesignSectionInfo>)di.DesignSections).Select<DesignSectionInfo, float>((Func<DesignSectionInfo, float>)(x => x.ShipSectionAsset.PsionicPowerLevel)).Sum();
			float num = 1f;
			if (admiralTraits.Contains(AdmiralInfo.TraitType.Psion))
				num += 0.2f;
			else if (admiralTraits.Contains(AdmiralInfo.TraitType.Skeptic))
				num -= 0.2f;
			return (int)Math.Floor((double)((IEnumerable<DesignSectionInfo>)di.DesignSections).Select<DesignSectionInfo, int>((Func<DesignSectionInfo, int>)(x => x.ShipSectionAsset.Crew)).Sum() * (double)app.AssetDatabase.GetFaction(app.GameDatabase.GetPlayerFactionID(di.PlayerID)).PsionicPowerPerCrew * (double)num);
		}

		public bool IsPoliceShip()
		{
			return this.DesignInfo.IsPoliceShip();
		}

		public bool IsSDB()
		{
			return this.DesignInfo.IsSDB();
		}

		public bool IsPlatform()
		{
			return this.DesignInfo.IsPlatform();
		}

		public bool IsMinelayer()
		{
			return this.DesignInfo.IsMinelayer();
		}

		public bool IsPlaced()
		{
			return this.ShipSystemPosition.HasValue;
		}

		public override string ToString()
		{
			return string.Format("ID={0},ShipName={1},SerialNumber={2}", (object)this.ID, (object)this.ShipName, (object)this.SerialNumber);
		}
	}
}
