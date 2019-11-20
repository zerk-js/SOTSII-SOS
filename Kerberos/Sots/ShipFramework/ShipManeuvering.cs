// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ShipManeuvering
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIPMANEUVERING)]
	internal class ShipManeuvering : GameObject
	{
		private Vector3 _position;
		private Vector3 _rotation;
		private Vector3 _destination;
		private Vector3 _velocity;
		private Vector3 _retreatDestination;
		private RetreatData _retreatData;
		private ShipSpeedState _shipSpeedState;
		private TargetFacingAngle _targetFacingAngle;
		private float _maxShipSpeed;

		public ShipManeuvering()
		{
			this._shipSpeedState = ShipSpeedState.Normal;
			this._retreatData = new RetreatData();
		}

		public ShipSpeedState SpeedState
		{
			get
			{
				return this._shipSpeedState;
			}
			set
			{
				if (value == this._shipSpeedState)
					return;
				this._shipSpeedState = value;
				this.PostSetProp("SetSpeedState", (int)value);
			}
		}

		public TargetFacingAngle TargetFacingAngle
		{
			get
			{
				return this._targetFacingAngle;
			}
			set
			{
				if (value == this._targetFacingAngle)
					return;
				this._targetFacingAngle = value;
				this.PostSetProp(nameof(TargetFacingAngle), (int)value);
			}
		}

		public Vector3 Position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = value;
			}
		}

		public Vector3 Rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				this._rotation = value;
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return this._velocity;
			}
			set
			{
				this._velocity = value;
			}
		}

		public Vector3 Destination
		{
			get
			{
				return this._destination;
			}
			set
			{
				this._destination = value;
			}
		}

		public float MaxShipSpeed
		{
			get
			{
				return this._maxShipSpeed;
			}
			set
			{
				this._maxShipSpeed = value;
			}
		}

		public RetreatData RetreatData
		{
			get
			{
				return this._retreatData;
			}
			set
			{
				this._retreatData = value;
				this.RetreatDestination = value.DefaultDestination;
			}
		}

		public Vector3 RetreatDestination
		{
			get
			{
				return this._retreatDestination;
			}
			set
			{
				this._retreatDestination = value;
				this.PostSetProp("RetreatDest", value);
			}
		}

		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			switch (messageId)
			{
				case InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP:
					string str = message.ReadString();
					if (str == "Position")
					{
						this._position = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
						return true;
					}
					if (str == "ShipSpeedScale")
					{
						this._shipSpeedState = (ShipSpeedState)message.ReadInteger();
						return true;
					}
					if (str == "TargetFacingAngle")
					{
						this._targetFacingAngle = (TargetFacingAngle)message.ReadInteger();
						return true;
					}
					break;
				case InteropMessageID.IMID_SCRIPT_MANEUVER_INFO:
					this._position = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._rotation = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._velocity = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					this._destination = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
					return true;
				default:
					App.Log.Warn("Unhandled message (id=" + (object)messageId + ").", "combat");
					break;
			}
			return false;
		}
	}
}
