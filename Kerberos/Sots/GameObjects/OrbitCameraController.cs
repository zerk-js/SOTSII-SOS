// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.OrbitCameraController
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_ORBITCAMERACONTROLLER)]
	internal class OrbitCameraController : GameObject, IActive, IDisposable
	{
		private bool _zoomEnabled = true;
		private Vector3 _targetPosition = Vector3.Zero;
		private bool _yawEnabled = true;
		private float _minPitch = -90f;
		private float _maxPitch = 90f;
		private bool _active;
		private int _targetID;
		private float _maxDistance;
		private float _minDistance;
		private float _desiredDistance;
		private float _desiredYaw;
		private float _minYaw;
		private float _maxYaw;
		private float _desiredPitch;
		private bool _pitchEnabled;

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

		public bool ZoomEnabled
		{
			get
			{
				return this._zoomEnabled;
			}
			set
			{
				if (this._zoomEnabled == value)
					return;
				this._zoomEnabled = value;
				this.PostSetProp(nameof(ZoomEnabled), value);
			}
		}

		public int TargetID
		{
			get
			{
				return this._targetID;
			}
			set
			{
				if (value == this._targetID)
					return;
				this._targetID = value;
				this.PostSetProp(nameof(TargetID), this._targetID);
			}
		}

		public Vector3 TargetPosition
		{
			get
			{
				return this._targetPosition;
			}
			set
			{
				if (value == this._targetPosition)
					return;
				this._targetPosition = value;
				this.PostSetProp("TargetPos", this._targetPosition);
			}
		}

		public float MaxDistance
		{
			get
			{
				return this._maxDistance;
			}
			set
			{
				if ((double)value == (double)this._maxDistance)
					return;
				this._maxDistance = value;
				this.PostSetProp(nameof(MaxDistance), this._maxDistance);
			}
		}

		public float MinDistance
		{
			get
			{
				return this._minDistance;
			}
			set
			{
				if ((double)value == (double)this._minDistance)
					return;
				this._minDistance = value;
				this.PostSetProp(nameof(MinDistance), this._minDistance);
			}
		}

		public float DesiredDistance
		{
			get
			{
				return this._desiredDistance;
			}
			set
			{
				this._desiredDistance = value;
				this.PostSetProp(nameof(DesiredDistance), this._desiredDistance);
			}
		}

		public float DesiredYaw
		{
			get
			{
				return this._desiredYaw;
			}
			set
			{
				if ((double)value == (double)this._desiredYaw)
					return;
				this._desiredYaw = value;
				this.PostSetProp(nameof(DesiredYaw), this._desiredYaw);
			}
		}

		public float MinYaw
		{
			get
			{
				return this._minYaw;
			}
			set
			{
				if ((double)value == (double)this._minYaw)
					return;
				this._minYaw = value;
				this.PostSetProp(nameof(MinYaw), this._minYaw);
			}
		}

		public float MaxYaw
		{
			get
			{
				return this._maxYaw;
			}
			set
			{
				if ((double)value == (double)this._maxYaw)
					return;
				this._maxYaw = value;
				this.PostSetProp(nameof(MaxYaw), this._maxYaw);
			}
		}

		public bool YawEnabled
		{
			get
			{
				return this._yawEnabled;
			}
			set
			{
				if (value == this._yawEnabled)
					return;
				this._yawEnabled = value;
				this.PostSetProp(nameof(YawEnabled), this._yawEnabled);
			}
		}

		public float DesiredPitch
		{
			get
			{
				return this._desiredPitch;
			}
			set
			{
				if ((double)value == (double)this._desiredPitch)
					return;
				this._desiredPitch = value;
				this.PostSetProp(nameof(DesiredPitch), this._desiredPitch);
			}
		}

		public bool PitchEnabled
		{
			get
			{
				return this._pitchEnabled;
			}
			set
			{
				if (value == this._pitchEnabled)
					return;
				this._pitchEnabled = value;
				this.PostSetProp(nameof(PitchEnabled), this._pitchEnabled);
			}
		}

		public void SnapToDesiredPosition()
		{
			this.PostSetProp("Snap");
		}

		public float MinPitch
		{
			get
			{
				return this._minPitch;
			}
			set
			{
				if ((double)value == (double)this._minPitch)
					return;
				this._minPitch = value;
				this.PostSetProp(nameof(MinPitch), this._minPitch);
			}
		}

		public float MaxPitch
		{
			get
			{
				return this._maxPitch;
			}
			set
			{
				if ((double)value == (double)this._maxPitch)
					return;
				this._maxPitch = value;
				this.PostSetProp(nameof(MaxPitch), this._maxPitch);
			}
		}

		public OrbitCameraController()
		{
			this._minDistance = 0.1f;
			this._maxDistance = 5000f;
			this._desiredDistance = 60f;
			this._pitchEnabled = true;
		}

		public OrbitCameraController(App game)
		  : this()
		{
			game.AddExistingObject((IGameObject)this);
		}

		public void SetAttractMode(bool value)
		{
			this.PostSetProp("AttractMode", value);
		}

		public void Dispose()
		{
			if (this.App == null)
				return;
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
