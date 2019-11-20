// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.CivilianRatios
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.PlayerFramework
{
	internal class CivilianRatios
	{
		public readonly Dictionary<Faction, float> FactionPopulationWeightTargets = new Dictionary<Faction, float>();
		public float CivilianPopulationWeightTarget = 1f;
	}
}
