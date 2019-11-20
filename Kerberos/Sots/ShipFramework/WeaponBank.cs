// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.WeaponBank
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;

namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_WEAPONBANK)]
	internal class WeaponBank : GameObject
	{
		private bool m_ToggleStateOn;

		public IGameObject Owner { get; private set; }

		public LogicalWeapon Weapon { get; private set; }

		public Module Module { get; private set; }

		public LogicalBank LogicalBank { get; private set; }

		public int WeaponLevel { get; private set; }

		public int DesignID { get; private set; }

		public int TargetFilter { get; private set; }

		public int FireMode { get; private set; }

		public WeaponEnums.TurretClasses TurretClass { get; private set; }

		public WeaponEnums.WeaponSizes WeaponSize { get; private set; }

		public bool ToggleState
		{
			get
			{
				return this.m_ToggleStateOn;
			}
			set
			{
				if (this.m_ToggleStateOn == value)
					return;
				this.PostSetProp("SetToggleState", value);
				this.m_ToggleStateOn = value;
			}
		}

		public WeaponBank(
		  App game,
		  IGameObject owner,
		  LogicalBank bank,
		  Module module,
		  LogicalWeapon weapon,
		  int weaponLevel,
		  int designID,
		  int targetFilter,
		  int fireMode,
		  WeaponEnums.WeaponSizes weaponSize,
		  WeaponEnums.TurretClasses turretClass)
		{
			this.Owner = owner;
			this.Weapon = weapon;
			this.LogicalBank = bank;
			this.Module = module;
			this.WeaponLevel = weaponLevel;
			this.DesignID = designID;
			this.TargetFilter = targetFilter;
			this.FireMode = fireMode;
			this.TurretClass = turretClass;
			this.WeaponSize = weaponSize;
			this.m_ToggleStateOn = false;
		}

		public virtual void AddExistingObject(App game)
		{
			game.AddExistingObject((IGameObject)this, (object)this.Weapon.GameObject.ObjectID, (object)this.Owner.ObjectID, (object)this.WeaponLevel, (object)this.TargetFilter, (object)this.FireMode, (object)(int)this.WeaponSize);
		}

		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (base.OnEngineMessage(messageId, message))
				return true;
			if (messageId != InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP || !(message.ReadString() == "WeaponToggleStateChanged"))
				return false;
			this.m_ToggleStateOn = message.ReadBool();
			return true;
		}
	}
}
