// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.LensFlare
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_LENSFLARE)]
	internal class LensFlare : GameObject, IPosition, IActive, IAttachable
	{
		private Vector3 _color = new Vector3(1f, 1f, 1f);
		private Vector2 _range = new Vector2(10f, 100f);
		private Vector2 _radius = new Vector2(0.2f, 0.2f);
		private Vector3 _pos;
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
				if (value == this._pos)
					return;
				this._pos = value;
				this.PostSetPosition(this._pos);
			}
		}

		public Vector3 Color
		{
			get
			{
				return this._color;
			}
			set
			{
				if (value == this._color)
					return;
				this._color = value;
				this.PostSetProp(nameof(Color), this._color);
			}
		}

		public Vector2 Range
		{
			get
			{
				return this._range;
			}
			set
			{
				if (value == this._range)
					return;
				this._range = value;
				this.PostSetProp(nameof(Range), this._range);
			}
		}

		public Vector2 Radius
		{
			get
			{
				return this._radius;
			}
			set
			{
				if (value == this._radius)
					return;
				this._radius = value;
				this.PostSetProp(nameof(Radius), this._radius);
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
