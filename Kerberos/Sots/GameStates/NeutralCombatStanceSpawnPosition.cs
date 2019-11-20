// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.NeutralCombatStanceSpawnPosition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameStates
{
	internal class NeutralCombatStanceSpawnPosition
	{
		public int FleetID;
		public Vector3 Position;
		public Vector3 Facing;

		public void SetInfo(int fleetID, Vector3 pos, Vector3 facing)
		{
			this.FleetID = fleetID;
			this.Position = pos;
			this.Facing = facing;
		}
	}
}
