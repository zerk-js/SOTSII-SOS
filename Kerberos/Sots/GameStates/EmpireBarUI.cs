// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.EmpireBarUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System.Collections.Generic;
using System.IO;

namespace Kerberos.Sots.GameStates
{
	internal static class EmpireBarUI
	{
		public const string UISavings = "gameEmpireSavings";
		public const string UIPsiLevel = "lblPsiValue";
		public const string UIResearchSlider = "gameEmpireResearchSlider";
		public const string UITitleFrame = "gameScreenFrame";
		public const string UIPsiPanel = "pnlPsiLevel";

		public static void SyncTitleFrame(App game)
		{
			Vector3 primaryColor = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID).PrimaryColor;
			game.UI.SetPropertyColorNormalized("gameScreenFrame", "empire_color", primaryColor);
		}

		public static void SyncTitleBar(App game, string panelId, BudgetPiechart piechart)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			Vector3 primaryColor = playerInfo.PrimaryColor;
			string withoutExtension1 = Path.GetFileNameWithoutExtension(playerInfo.BadgeAssetPath);
			string withoutExtension2 = Path.GetFileNameWithoutExtension(playerInfo.AvatarAssetPath);
			game.UI.SetPropertyString(panelId, "avatar", withoutExtension2);
			game.UI.SetPropertyString(panelId, "badge", withoutExtension1);
			game.UI.SetPropertyString(panelId, "name", playerInfo.Name.ToUpper());
			game.UI.SetPropertyColorNormalized(panelId, "empire_color", primaryColor);
			string propertyValue = string.Format("{0} {1}", (object)App.Localize("@UI_GENERAL_TURN"), (object)game.GameDatabase.GetTurnCount());
			game.UI.SetPropertyString("turn_count", "text", propertyValue);
			long savings = (long)playerInfo.Savings;
			long psionicPotential = (long)playerInfo.PsionicPotential;
			game.UI.SetPropertyString("gameEmpireSavings", "text", savings.ToString("N0"));
			game.UI.SetVisible("pnlPsiLevel", game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "PSI_Clairvoyance") | game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "PSI_Empathy") | game.GameDatabase.PlayerHasTech(game.LocalPlayer.ID, "PSI_Telekinesis") && game.LocalPlayer.Faction.Name != "loa");
			game.UI.SetPropertyString("lblPsiValue", "text", psionicPotential.ToString("N0"));
			EmpireHistoryData historyForPlayer = game.GameDatabase.GetLastEmpireHistoryForPlayer(game.LocalPlayer.ID);
			Vector3 vector3_1 = new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
			Vector3 vector3_2 = new Vector3(0.0f, (float)byte.MaxValue, 0.0f);
			Vector3 vector3_3 = new Vector3((float)byte.MaxValue, 0.0f, 0.0f);
			if (historyForPlayer != null)
			{
				if (psionicPotential > (long)historyForPlayer.psi_potential)
					game.UI.SetPropertyColor("lblPsiValue", "color", vector3_2);
				else if (psionicPotential < (long)historyForPlayer.psi_potential)
					game.UI.SetPropertyColor("lblPsiValue", "color", vector3_3);
				else
					game.UI.SetPropertyColor("lblPsiValue", "color", vector3_1);
			}
			EmpireBarUI.SyncResearchSlider(game, "gameEmpireResearchSlider", playerInfo, piechart);
		}

		public static void SyncResearchSlider(App game, string sliderId, BudgetPiechart piechart)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			EmpireBarUI.SyncResearchSlider(game, sliderId, playerInfo, piechart);
		}

		private static void SyncResearchSlider(
		  App game,
		  string sliderId,
		  PlayerInfo playerInfo,
		  BudgetPiechart piechart)
		{
			int num = (int)((double)playerInfo.RateGovernmentResearch * 100.0);
			game.UI.SetSliderValue(sliderId, 100 - num);
			if (piechart == null)
				return;
			Budget budget = Budget.GenerateBudget(game.Game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic);
			piechart.SetSlices(budget);
		}
	}
}
