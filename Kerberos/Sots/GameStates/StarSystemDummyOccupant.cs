// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarSystemDummyOccupant
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARSYSTEMDUMMYOCCUPANT)]
	internal class StarSystemDummyOccupant : GameObject, IDisposable, IActive
	{
		private bool _active;

		public StarSystemDummyOccupant(App game, string modelName, StationType type)
		{
			game.AddExistingObject((IGameObject)this, (object)modelName, (object)type.ToFlags());
		}

		public void Dispose()
		{
			this.App.ReleaseObject((IGameObject)this);
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active == value)
					return;
				this.PostSetActive(value);
				this._active = value;
			}
		}
	}
}
