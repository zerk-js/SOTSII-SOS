// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.PlanetTarget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.GameStates;

namespace Kerberos.Sots.Combat
{
	internal class PlanetTarget
	{
		private StellarBody _planet;
		private bool _hasBeenVisited;

		public PlanetTarget(StellarBody planet)
		{
			this._planet = planet;
			this._hasBeenVisited = false;
		}

		public StellarBody Planet
		{
			get
			{
				return this._planet;
			}
		}

		public bool HasBeenVisted
		{
			get
			{
				return this._hasBeenVisited;
			}
			set
			{
				this._hasBeenVisited = value;
			}
		}
	}
}
