// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.StaticSpotLight
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_STATICSPOTLIGHT)]
	internal class StaticSpotLight : GameObject, IPosition, IActive, IAttachable
	{
		private float _radius = 100f;
		private float _specularPower = 50f;
		private Vector3 _intensity = new Vector3(100f, 100f, 100f);
		private float _conePower = 10f;
		private float _coneScale = 10f;
		private float _coneFov = 90f;
		private Vector3 _pos;
		private Vector3 _rot;
		private bool _active;
		private IGameObject _parent;

		public IGameObject Parent
		{
			get
			{
				return this._parent;
			}
			set
			{
				if (this._parent == value)
					return;
				this._parent = value;
				this.PostSetParent(value);
			}
		}

		public Vector3 Position
		{
			get
			{
				return this._pos;
			}
			set
			{
				if ((double)value.X == (double)this._pos.X && (double)value.Y == (double)this._pos.Y && (double)value.Z == (double)this._pos.Z)
					return;
				this._pos = value;
				this.PostSetPosition(this._pos);
			}
		}

		public Vector3 Rotation
		{
			get
			{
				return this._rot;
			}
			set
			{
				if ((double)value.X == (double)this._rot.X && (double)value.Y == (double)this._rot.Y && (double)value.Z == (double)this._rot.Z)
					return;
				this._rot = value;
				this.PostSetRotation(this._rot);
			}
		}

		public Vector3 Intensity
		{
			get
			{
				return this._intensity;
			}
			set
			{
				if ((double)value.X == (double)this._intensity.X && (double)value.Y == (double)this._intensity.Y && (double)value.Z == (double)this._intensity.Z)
					return;
				this._intensity = value;
				this.PostSetProp(nameof(Intensity), this._intensity);
			}
		}

		public float SpecularPower
		{
			get
			{
				return this._specularPower;
			}
			set
			{
				if ((double)value == (double)this._specularPower)
					return;
				this._specularPower = value;
				this.PostSetProp(nameof(SpecularPower), this._specularPower);
			}
		}

		public float Radius
		{
			get
			{
				return this._radius;
			}
			set
			{
				if ((double)value == (double)this._radius)
					return;
				this._radius = value;
				this.PostSetProp(nameof(Radius), this._radius);
			}
		}

		public float ConePower
		{
			get
			{
				return this._conePower;
			}
			set
			{
				if ((double)value == (double)this._conePower)
					return;
				this._conePower = value;
				this.PostSetProp(nameof(ConePower), this._conePower);
			}
		}

		public float ConeScale
		{
			get
			{
				return this._coneScale;
			}
			set
			{
				if ((double)value == (double)this._coneScale)
					return;
				this._coneScale = value;
				this.PostSetProp(nameof(ConeScale), this._coneScale);
			}
		}

		public float ConeFov
		{
			get
			{
				return this._coneFov;
			}
			set
			{
				if ((double)value == (double)this._coneFov)
					return;
				this._coneFov = value;
				this.PostSetProp(nameof(ConeFov), this._coneFov);
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
				if (value == this._active)
					return;
				this._active = value;
				this.PostSetActive(this._active);
			}
		}
	}
}
