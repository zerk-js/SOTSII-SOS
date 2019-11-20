// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.StaticModel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_STATICMODEL)]
	internal class StaticModel : GameObject, IPosition, IOrientatable, IScalable, IActive, IAttachable
	{
		private float _scale = 1f;
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

		public float Scale
		{
			get
			{
				return this._scale;
			}
			set
			{
				if ((double)value == (double)this._scale)
					return;
				this._scale = value;
				this.PostSetScale(this._scale);
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
