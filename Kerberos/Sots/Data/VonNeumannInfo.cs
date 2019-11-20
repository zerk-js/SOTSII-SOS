// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.VonNeumannInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class VonNeumannInfo
	{
		public List<VonNeumannTargetInfo> TargetInfos = new List<VonNeumannTargetInfo>();
		public int Id;
		public int? FleetId;
		public int SystemId;
		public int OrbitalId;
		public int Resources;
		public int ResourcesCollectedLastTurn;
		public int ConstructionProgress;
		public int? ProjectDesignId;
		public int LastCollectionSystem;
		public int LastTargetSystem;
		public int LastCollectionTurn;
		public int LastTargetTurn;
	}
}
