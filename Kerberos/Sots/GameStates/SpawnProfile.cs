// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.SpawnProfile
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	internal class SpawnProfile
	{
		public int _fleetID;
		public int _playerID;
		public int _activeCommandShipID;
		public float _radius;
		public Vector3 _startingPosition;
		public Vector3 _spawnPosition;
		public Vector3 _spawnFacing;
		public Vector3 _retreatPosition;
		public List<int> _activeShips;
		public List<int> _reserveShips;

		public SpawnProfile()
		{
			this._activeShips = new List<int>();
			this._reserveShips = new List<int>();
			this._radius = 2000f;
		}

		public bool SpawnProfileOverlaps(SpawnProfile sp)
		{
			if ((double)this._spawnPosition.Y != (double)sp._spawnPosition.Y)
				return false;
			float num = this._radius + sp._radius;
			return (double)(this._spawnPosition - sp._spawnPosition).LengthSquared < (double)num * (double)num;
		}
	}
}
