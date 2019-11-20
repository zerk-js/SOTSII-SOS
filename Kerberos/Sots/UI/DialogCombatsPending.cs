// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogCombatsPending
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DialogCombatsPending : Dialog
	{
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private const string ContentList = "contentList";
		private const string SimWidget = "simWidget";
		private const string ResolvingText = "reolvetext";
		private const string Combatslabel = "combatsremaininglbl";
		private const string AvatarsInCombat = "combatPlayers";
		private const string AvatarsInother = "remainingPlayers";
		private List<PendingCombat> _listedcombats;

		public DialogCombatsPending(App app)
		  : base(app, "dialogCombatsPending")
		{
			this._listedcombats = new List<PendingCombat>();
		}

		private void UpdateCombatList()
		{
			if (!this._app.Game.GetPendingCombats().Any<PendingCombat>((Func<PendingCombat, bool>)(x => this._listedcombats.Any<PendingCombat>((Func<PendingCombat, bool>)(j => j.ConflictID == x.ConflictID)))))
			{
				this._listedcombats.Clear();
				foreach (SystemWidget systemWidget in this._systemWidgets)
					systemWidget.Terminate();
				this._systemWidgets.Clear();
				this._app.UI.ClearItems(this._app.UI.Path(this.ID, "contentList"));
			}
			foreach (PendingCombat pendingCombat in this._app.Game.GetPendingCombats())
			{
				PendingCombat cmb = pendingCombat;
				bool flag1 = this._app.Game.CombatData.GetCombat(this._app.GameDatabase, cmb.ConflictID, cmb.SystemID, this._app.GameDatabase.GetTurnCount()) != null;
				string itemGlobalId1;
				if (this._listedcombats.Any<PendingCombat>((Func<PendingCombat, bool>)(x => x.ConflictID == cmb.ConflictID)))
				{
					if (this._listedcombats.FirstOrDefault<PendingCombat>((Func<PendingCombat, bool>)(x => x.ConflictID == cmb.ConflictID)).complete != flag1)
					{
						this._listedcombats.FirstOrDefault<PendingCombat>((Func<PendingCombat, bool>)(x => x.ConflictID == cmb.ConflictID)).complete = flag1;
						itemGlobalId1 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "contentList"), "", cmb.ConflictID, "");
						this._listedcombats.Remove(this._listedcombats.FirstOrDefault<PendingCombat>((Func<PendingCombat, bool>)(x => x.ConflictID == cmb.ConflictID)));
					}
					else
						continue;
				}
				else
				{
					this._app.UI.AddItem(this._app.UI.Path(this.ID, "contentList"), "", cmb.ConflictID, "");
					itemGlobalId1 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "contentList"), "", cmb.ConflictID, "");
					this._app.GameDatabase.GetStarSystemInfo(cmb.SystemID);
					this._listedcombats.Add(cmb);
					this._app.UI.ClearItems(this._app.UI.Path(itemGlobalId1, "combatPlayers"));
					bool flag2 = StarMap.IsInRange(this._app.GameDatabase, this._app.LocalPlayer.ID, cmb.SystemID);
					foreach (int num in cmb.PlayersInCombat)
					{
						this._app.UI.AddItem(this._app.UI.Path(itemGlobalId1, "combatPlayers"), "", num, "");
						string itemGlobalId2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "combatPlayers"), "", num, "");
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "smallAvatar"), "sprite", Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(num).AvatarAssetPath));
						if (flag2 && Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(num).AvatarAssetPath) != string.Empty)
						{
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "smallAvatar"), true);
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "UnknownText"), false);
						}
						else
						{
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "smallAvatar"), false);
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "UnknownText"), true);
						}
						CombatState.SetPlayerCardOutlineColor(this._app, this._app.UI.Path(itemGlobalId2, "bgPlayerColor"), flag2 ? this._app.GameDatabase.GetPlayerInfo(num).PrimaryColor : new Vector3(0.0f, 0.0f, 0.0f));
					}
					this._app.UI.ClearItems(this._app.UI.Path(itemGlobalId1, "remainingPlayers"));
					foreach (int num in cmb.NPCPlayersInCombat)
					{
						this._app.UI.AddItem(this._app.UI.Path(itemGlobalId1, "remainingPlayers"), "", num, "");
						string itemGlobalId2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "remainingPlayers"), "", num, "");
						this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId2, "smallAvatar"), "sprite", Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(num).AvatarAssetPath));
						if (flag2 && Path.GetFileNameWithoutExtension(this._app.GameDatabase.GetPlayerInfo(num).AvatarAssetPath) != string.Empty)
						{
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "smallAvatar"), true);
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "UnknownText"), false);
						}
						else
						{
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "smallAvatar"), false);
							this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId2, "UnknownText"), true);
						}
						CombatState.SetPlayerCardOutlineColor(this._app, this._app.UI.Path(itemGlobalId2, "bgPlayerColor"), flag2 ? this._app.GameDatabase.GetPlayerInfo(num).PrimaryColor : new Vector3(0.0f, 0.0f, 0.0f));
					}
					this.SyncSystemOwnershipEffect(this._app.UI.Path(itemGlobalId1, "systemTitleCard"), cmb.SystemID, !flag2);
				}
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "pendingBG"), (!flag1 ? 1 : 0) != 0);
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "completeBG"), (flag1 ? 1 : 0) != 0);
				bool isMultiplayer = this._app.GameSetup.IsMultiplayer;
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "simWidget"), false);
				string text = "PENDING";
				if (this._app.CurrentState == this._app.GetGameState<CommonCombatState>() || this._app.CurrentState == this._app.GetGameState<SimCombatState>())
				{
					CommonCombatState currentState = (CommonCombatState)this._app.CurrentState;
					if (currentState != null && currentState.GetCombatID() == cmb.ConflictID)
					{
						if (currentState.PlayersInCombat.Any<Player>((Func<Player, bool>)(x => x.ID == this._app.LocalPlayer.ID)))
							text = "RESOLVING";
						this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId1, "simWidget"), true);
					}
				}
				if (isMultiplayer)
					text = "RESOLVING";
				if (flag1)
					text = "RESOLVED";
				this._app.UI.SetText(this._app.UI.Path(itemGlobalId1, "reolvetext"), text);
				this._app.UI.SetText(this._app.UI.Path(this.ID, "combatsremaininglbl"), this._listedcombats.Where<PendingCombat>((Func<PendingCombat, bool>)(x => !x.complete)).Count<PendingCombat>().ToString() + (this._listedcombats.Where<PendingCombat>((Func<PendingCombat, bool>)(x => !x.complete)).Count<PendingCombat>() != 1 ? " Combats" : " Combat") + " Pending");
			}
		}

		private void SyncSystemOwnershipEffect(string itemID, int systemid, bool cloaksystem)
		{
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(systemid);
			if (starSystemInfo == (StarSystemInfo)null || starSystemInfo.IsDeepSpace)
			{
				this._app.UI.SetVisible(this._app.UI.GetGlobalID(this._app.UI.Path(itemID, "systemDeepspace")), true);
				this._app.UI.SetText(this._app.UI.Path(itemID, "title"), cloaksystem ? "Unknown" : starSystemInfo.Name);
			}
			else
			{
				this._systemWidgets.Add(new SystemWidget(this._app, itemID));
				this._systemWidgets.Last<SystemWidget>().Sync(systemid);
				if (cloaksystem)
					this._app.UI.SetText(this._app.UI.Path(itemID, "title"), "Unknown");
				HomeworldInfo homeworldInfo = this._app.GameDatabase.GetHomeworlds().ToList<HomeworldInfo>().FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.SystemID == systemid));
				int? systemOwningPlayer = this._app.GameDatabase.GetSystemOwningPlayer(systemid);
				PlayerInfo Owner = this._app.GameDatabase.GetPlayerInfo(systemOwningPlayer.HasValue ? systemOwningPlayer.Value : 0);
				if (homeworldInfo != null && homeworldInfo.SystemID != 0 && !cloaksystem)
				{
					string globalId = this._app.UI.GetGlobalID(this._app.UI.Path(itemID, "systemHome"));
					this._app.UI.SetVisible(globalId, true);
					this._app.UI.SetPropertyColor(globalId, "color", this._app.GameDatabase.GetPlayerInfo(homeworldInfo.PlayerID).PrimaryColor * (float)byte.MaxValue);
				}
				else if (!cloaksystem && Owner != null && this._app.GameDatabase.GetProvinceInfos().Where<ProvinceInfo>((Func<ProvinceInfo, bool>)(x =>
			   {
				   if (x.CapitalSystemID != systemid || x.PlayerID != Owner.ID)
					   return false;
				   int capitalSystemId = x.CapitalSystemID;
				   int? homeworld = Owner.Homeworld;
				   if (capitalSystemId == homeworld.GetValueOrDefault())
					   return !homeworld.HasValue;
				   return true;
			   })).Any<ProvinceInfo>())
				{
					string globalId = this._app.UI.GetGlobalID(this._app.UI.Path(itemID, "systemCapital"));
					this._app.UI.SetVisible(globalId, true);
					this._app.UI.SetPropertyColor(globalId, "color", Owner.PrimaryColor * (float)byte.MaxValue);
				}
				else
				{
					string globalId = this._app.UI.GetGlobalID(this._app.UI.Path(itemID, "systemOwnership"));
					this._app.UI.SetVisible(globalId, true);
					if (Owner == null || cloaksystem)
						return;
					this._app.UI.SetPropertyColor(globalId, "color", Owner.PrimaryColor * (float)byte.MaxValue);
				}
			}
		}

		public override void Initialize()
		{
			this._app.UI.SetListCleanClear(this._app.UI.Path(this.ID, "contentList"), true);
		}

		protected override void OnUpdate()
		{
			this.UpdateCombatList();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			int num = msgType == "button_clicked" ? 1 : 0;
		}

		public override string[] CloseDialog()
		{
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			return (string[])null;
		}
	}
}
