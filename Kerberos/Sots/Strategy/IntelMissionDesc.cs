// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.IntelMissionDesc
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal abstract class IntelMissionDesc
	{
		public IntelMission ID { get; protected set; }

		public string Name { get; protected set; }

		public IEnumerable<TurnEventType> TurnEventTypes { get; protected set; }

		public abstract void OnCommit(
		  GameSession game,
		  int playerId,
		  int targetPlayerId,
		  int? missionid = null);

		public virtual void OnProcessTurnEvent(GameSession game, TurnEvent e)
		{
		}
	}
}
