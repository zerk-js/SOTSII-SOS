// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.IntelMissionDesc_XxxSystem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Strategy
{
	internal abstract class IntelMissionDesc_XxxSystem : IntelMissionDesc
	{
		public override sealed void OnProcessTurnEvent(GameSession game, TurnEvent e)
		{
			base.OnProcessTurnEvent(game, e);
			if (e.SystemID == 0)
				return;
			game.GameDatabase.UpdatePlayerViewWithStarSystem(game.LocalPlayer.ID, e.SystemID);
			int turnCount = game.GameDatabase.GetTurnCount();
			game.GameDatabase.InsertExploreRecord(e.SystemID, game.LocalPlayer.ID, turnCount, true, true);
		}
	}
}
