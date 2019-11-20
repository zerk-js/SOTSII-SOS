// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.Shield
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIELD)]
	internal class Shield : GameObject, IActive
	{
		private bool _isShipAssigned;
		private bool _active;

		public Shield(
		  App game,
		  Ship ship,
		  LogicalShield logShield,
		  Section sectWithShield,
		  List<PlayerTechInfo> playerTechs,
		  bool isShipAssigned = true)
		{
			ShieldData shieldData = ship.ShipClass == ShipClass.Dreadnought ? logShield.DNShieldData : logShield.CRShieldData;
			game.AddExistingObject((IGameObject)this, new List<object>()
	  {
		(object) (int) logShield.Type,
		(object) Shield.GetTotalStructure(game.AssetDatabase, shieldData.Structure, playerTechs),
		(object) shieldData.RechargeTime,
		(object) (float) (logShield.Type == LogicalShield.ShieldType.PSI_SHIELD ? 0.0 : (double) Shield.GetShieldRegenPerSec(playerTechs)),
		(object) ship.ObjectID,
		(object) sectWithShield.ObjectID,
		(object) Path.Combine("props\\models\\Shields", Path.GetFileNameWithoutExtension(shieldData.ModelFileName) + "_convex.obj"),
		(object) Path.Combine("props\\models\\Shields", shieldData.ModelFileName),
		(object) Path.Combine(shieldData.ImpactEffectName),
		(object) 1000000f,
		(object) true,
		(object) isShipAssigned
	  }.ToArray());
			this._isShipAssigned = isShipAssigned;
		}

		public bool IsShipAssigned
		{
			get
			{
				return this._isShipAssigned;
			}
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				this._active = value;
				this.PostSetActive(this._active);
			}
		}

		public static float GetTotalStructure(
		  AssetDatabase ab,
		  float baseStruct,
		  List<PlayerTechInfo> playerTechs)
		{
			PlayerTechInfo playerTechInfo1 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "NRG_Quantum_Capacitors"));
			PlayerTechInfo playerTechInfo2 = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "NRG_Shield_Magnifier"));
			float num = 1f;
			if (playerTechInfo1 != null && playerTechInfo1.State == TechStates.Researched)
				num += ab.GetTechBonus<float>(playerTechInfo1.TechFileID, "shieldstructure");
			if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched)
				num += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "shieldstructure");
			return baseStruct * num;
		}

		public static float GetShieldRegenPerSec(List<PlayerTechInfo> playerTechs)
		{
			PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "SLD_Shields_Rechargers"));
			return playerTechInfo != null && playerTechInfo.State == TechStates.Researched ? 50f : 0.0f;
		}
	}
}
