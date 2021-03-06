﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.CombatSensor
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_COMBATSENSOR)]
	internal class CombatSensor : GameObject, IActive
	{
		private float _minDistFromCamera = 50000f;
		private bool _active;

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

		public float MinDistance
		{
			get
			{
				return this._minDistFromCamera;
			}
			set
			{
				if ((double)value == (double)this._minDistFromCamera)
					return;
				this._minDistFromCamera = value;
			}
		}
	}
}
