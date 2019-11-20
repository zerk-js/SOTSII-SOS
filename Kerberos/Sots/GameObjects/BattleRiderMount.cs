// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.BattleRiderMount
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_BATTLERIDERMOUNT)]
	internal class BattleRiderMount : MountObject
	{
		private int _designID;
		private int _squadIndex;
		private bool _isWeapon;
		private Section _section;
		private Module _module;
		private LogicalBank _bank;
		private string _icon;

		public int DesignID
		{
			get
			{
				return this._designID;
			}
			set
			{
				this._designID = value;
			}
		}

		public int SquadIndex
		{
			get
			{
				return this._squadIndex;
			}
			set
			{
				this._squadIndex = value;
			}
		}

		public bool IsWeapon
		{
			get
			{
				return this._isWeapon;
			}
			set
			{
				this._isWeapon = value;
			}
		}

		public Section AssignedSection
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

		public Module AssignedModule
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

		public LogicalBank WeaponBank
		{
			get
			{
				return this._bank;
			}
			set
			{
				this._bank = value;
			}
		}

		public string BankIcon
		{
			get
			{
				return this._icon;
			}
			set
			{
				this._icon = value;
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			this._bank = (LogicalBank)null;
			this._module = (Module)null;
			this._section = (Section)null;
		}

		public static bool CanBattleRiderConnect(
		  WeaponEnums.TurretClasses mountType,
		  BattleRiderTypes brct,
		  ShipClass sc)
		{
			switch (mountType)
			{
				case WeaponEnums.TurretClasses.Biomissile:
					return brct == BattleRiderTypes.biomissile;
				case WeaponEnums.TurretClasses.Drone:
					return brct == BattleRiderTypes.drone;
				case WeaponEnums.TurretClasses.AssaultShuttle:
					return brct == BattleRiderTypes.assaultshuttle;
				case WeaponEnums.TurretClasses.DestroyerRider:
					if (brct.IsBattleRiderType())
						return sc == ShipClass.BattleRider;
					return false;
				case WeaponEnums.TurretClasses.CruiserRider:
					if (brct.IsControllableBattleRider())
						return sc == ShipClass.Cruiser;
					return false;
				case WeaponEnums.TurretClasses.DreadnoughtRider:
					if (brct.IsControllableBattleRider())
						return sc == ShipClass.Dreadnought;
					return false;
				case WeaponEnums.TurretClasses.BoardingPod:
					return brct == BattleRiderTypes.boardingpod;
				case WeaponEnums.TurretClasses.EscapePod:
					return brct == BattleRiderTypes.escapepod;
				default:
					return false;
			}
		}
	}
}
