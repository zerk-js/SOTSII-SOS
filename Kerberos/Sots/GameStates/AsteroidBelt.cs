// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.AsteroidBelt
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_ASTEROIDBELT)]
	internal class AsteroidBelt : GameObject, IActive, IDisposable
	{
		private bool _active;

		public AsteroidBelt(
		  App g,
		  int randomSeed,
		  Vector3 center,
		  float innerRadius,
		  float outterRadius,
		  float minHeight,
		  float maxHeight,
		  int numAsteroids)
		{
			g.AddExistingObject((IGameObject)this, (object)randomSeed, (object)center, (object)innerRadius, (object)outterRadius, (object)minHeight, (object)maxHeight, (object)numAsteroids);
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

		public void Dispose()
		{
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
