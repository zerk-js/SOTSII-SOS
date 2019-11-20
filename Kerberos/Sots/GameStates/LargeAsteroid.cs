// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.LargeAsteroid
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_LARGEASTEROID)]
	internal class LargeAsteroid : GameObject, IActive, IDisposable
	{
		private bool _active;
		private Matrix _worldTransform;
		private int _id;

		public LargeAsteroid(App g, Vector3 position)
		{
			g.AddExistingObject((IGameObject)this, (object)position);
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

		public Matrix WorldTransform
		{
			get
			{
				return this._worldTransform;
			}
			set
			{
				this._worldTransform = value;
			}
		}

		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}

		public void Dispose()
		{
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
