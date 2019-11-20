// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.BattleRiderSquad
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_BATTLERIDERSQUAD)]
	internal class BattleRiderSquad : GameObject, IDisposable
	{
		private int _parentID;
		private Section _section;
		private Module _module;
		private List<BattleRiderMount> _mounts;
		private int _numRiders;

		public BattleRiderSquad()
		{
			this._mounts = new List<BattleRiderMount>();
		}

		public int ParentID
		{
			get
			{
				return this._parentID;
			}
			set
			{
				this._parentID = value;
			}
		}

		public Section AttachedSection
		{
			get
			{
				return this._section;
			}
			set
			{
				this._section = value;
			}
		}

		public Module AttachedModule
		{
			get
			{
				return this._module;
			}
			set
			{
				this._module = value;
			}
		}

		public List<BattleRiderMount> Mounts
		{
			get
			{
				return this._mounts;
			}
			set
			{
				this._mounts = value;
			}
		}

		public int NumRiders
		{
			get
			{
				return this._numRiders;
			}
			set
			{
				this._numRiders = value;
			}
		}

		public void Dispose()
		{
			if (this._mounts != null)
				this._mounts.Clear();
			this._module = (Module)null;
			this._section = (Section)null;
		}

		public static int GetMinRiderSlotsPerSquad(
		  WeaponEnums.TurretClasses mountType,
		  ShipClass carrierClass)
		{
			int num = 0;
			switch (carrierClass)
			{
				case ShipClass.Cruiser:
				case ShipClass.Dreadnought:
					num = 3;
					break;
				case ShipClass.Leviathan:
					num = mountType != WeaponEnums.TurretClasses.DestroyerRider ? 3 : 6;
					break;
			}
			return num;
		}

		public static int GetNumRidersPerSquad(
		  WeaponEnums.TurretClasses mountType,
		  ShipClass carrierClass,
		  int totalMounts)
		{
			int val1;
			switch (carrierClass)
			{
				case ShipClass.Cruiser:
				case ShipClass.Dreadnought:
					val1 = 3;
					break;
				case ShipClass.Leviathan:
					val1 = mountType != WeaponEnums.TurretClasses.DestroyerRider ? 3 : 6;
					break;
				default:
					val1 = totalMounts;
					break;
			}
			if (WeaponEnums.IsWeaponBattleRider(mountType))
				val1 = totalMounts;
			return Math.Min(val1, totalMounts);
		}
	}
}
