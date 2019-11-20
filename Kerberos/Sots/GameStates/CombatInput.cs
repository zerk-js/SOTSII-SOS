// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.CombatInput
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_COMBATINPUT)]
	internal class CombatInput : GameObject, IActive
	{
		private bool _enableTimeScale = true;
		private bool _active;
		private int _combatGridID;
		private int _combatSensorID;
		private int _combatID;
		private int _selectedID;
		private int _cameraID;
		private int _localPlayerId;

		protected override GameObjectStatus OnCheckStatus()
		{
			return base.OnCheckStatus();
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
					return;
				this._active = value;
				this.PostSetActive(this._active);
			}
		}

		public int CombatGridID
		{
			get
			{
				return this._combatGridID;
			}
			set
			{
				if (value == this._combatGridID)
					return;
				this._combatGridID = value;
				this.PostSetProp("CombatGrid", this._combatGridID);
			}
		}

		public int CombatSensorID
		{
			get
			{
				return this._combatSensorID;
			}
			set
			{
				if (value == this._combatSensorID)
					return;
				this._combatSensorID = value;
				this.PostSetProp("CombatSensor", this._combatSensorID);
			}
		}

		public bool EnableTimeScale
		{
			get
			{
				return this._enableTimeScale;
			}
			set
			{
				if (value == this._enableTimeScale)
					return;
				this._enableTimeScale = value;
				this.PostSetProp(nameof(EnableTimeScale), this._enableTimeScale);
			}
		}

		public int CombatID
		{
			get
			{
				return this._combatID;
			}
			set
			{
				if (value == this._combatID)
					return;
				this._combatID = value;
				this.PostSetProp("Combat", this._combatID);
			}
		}

		public int SelectedID
		{
			get
			{
				return this._selectedID;
			}
			set
			{
				if (value == this._selectedID)
					return;
				this._selectedID = value;
				this.PostSetProp("Selected", this._selectedID);
			}
		}

		public int CameraID
		{
			get
			{
				return this._cameraID;
			}
			set
			{
				if (value == this._cameraID)
					return;
				this._cameraID = value;
				this.PostSetProp("Camera", this._cameraID);
			}
		}

		public int PlayerId
		{
			get
			{
				return this._localPlayerId;
			}
			set
			{
				if (value == this._localPlayerId)
					return;
				this._localPlayerId = value;
				this.PostSetProp("LocalPlayer", this._localPlayerId);
			}
		}
	}
}
