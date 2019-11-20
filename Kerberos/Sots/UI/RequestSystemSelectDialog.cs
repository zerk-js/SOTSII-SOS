// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RequestSystemSelectDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class RequestSystemSelectDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string ItemList = "lstItems";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private const string StarmapObjectHost = "ohStarmap";
		private int _otherPlayer;
		private RequestType _type;
		private RequestInfo _request;
		private GameObjectSet _crits;
		private Sky _sky;
		private StarMap _starmap;

		public RequestSystemSelectDialog(App game, RequestType type, int otherPlayer, string template = "dialogRequestSystemSelect")
		  : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._request = new RequestInfo();
			this._request.InitiatingPlayer = game.LocalPlayer.ID;
			this._request.ReceivingPlayer = this._otherPlayer;
			this._request.State = AgreementState.Unrequested;
			this._request.Type = type;
			this._crits = new GameObjectSet(game);
			this._sky = new Sky(game, SkyUsage.StarMap, 0);
			this._crits.Add((IGameObject)this._sky);
			this._starmap = new StarMap(game, game.Game, this._sky);
			this._crits.Add((IGameObject)this._starmap);
			this._starmap.SetCamera(game.Game.StarMapCamera);
		}

		public override void Initialize()
		{
			this._crits.Activate();
			this._starmap.Sync(this._crits);
			this._app.UI.Send((object)"SetGameObject", (object)this._app.UI.Path(this.ID, "ohStarmap"), (object)this._starmap.ObjectID);
			DiplomacyUI.SyncDiplomacyPopup(this._app, this.ID, this._otherPlayer);
			this.SyncSystemSelect();
		}

		private void SyncSystemSelect()
		{
			this._app.UI.SetText("lblHeader", string.Format(App.Localize(RequestTypeDialog.RequestTypeLocMap[this._type]), (object)this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type)));
			this._app.UI.SetEnabled("btnFinishRequest", true);
			List<StarSystemInfo> list1 = this._app.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<int> colonizedSystemIds;
			switch (this._type)
			{
				case RequestType.SystemInfoRequest:
					List<StarSystemInfo> list2 = list1.ToList<StarSystemInfo>();
					foreach (StarSystemInfo starSystemInfo in list2)
					{
						if (starSystemInfo.IsVisible)
						{
							this._app.UI.AddItem("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							string itemGlobalId = this._app.UI.GetItemGlobalID("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblHeader"), starSystemInfo.Name);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblValue"), "");
							this._starmap.PostSetProp("ProvincePoolEffect", (object)true, (object)this._starmap.Systems.Reverse[starSystemInfo.ID]);
						}
					}
					if (list2.Count > 0)
					{
						this._app.UI.SetSelection("lstItems", list2.First<StarSystemInfo>().ID);
						break;
					}
					this._app.UI.SetEnabled("btnFinishRequest", false);
					break;
				case RequestType.MilitaryAssistanceRequest:
					List<StarSystemInfo> list3 = list1.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => this._app.GameDatabase.IsSurveyed(this._app.LocalPlayer.ID, x.ID))).ToList<StarSystemInfo>();
					foreach (StarSystemInfo starSystemInfo in list3)
					{
						if (starSystemInfo.IsVisible)
						{
							this._app.UI.AddItem("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							string itemGlobalId = this._app.UI.GetItemGlobalID("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblHeader"), starSystemInfo.Name);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblValue"), "");
							this._starmap.PostSetProp("ProvincePoolEffect", (object)true, (object)this._starmap.Systems.Reverse[starSystemInfo.ID]);
						}
					}
					if (list3.Count > 0)
					{
						this._app.UI.SetSelection("lstItems", list3.First<StarSystemInfo>().ID);
						break;
					}
					this._app.UI.SetEnabled("btnFinishRequest", false);
					break;
				case RequestType.GatePermissionRequest:
					colonizedSystemIds = this._app.GameDatabase.GetPlayerColonySystemIDs(this._otherPlayer).ToList<int>();
					List<StarSystemInfo> list4 = list1.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   if (colonizedSystemIds.Contains(x.ID))
						   return StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, x, (Dictionary<int, List<ShipInfo>>)null);
					   return false;
				   })).ToList<StarSystemInfo>();
					foreach (StarSystemInfo starSystemInfo in list4)
					{
						if (starSystemInfo.IsVisible)
						{
							this._app.UI.AddItem("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							string itemGlobalId = this._app.UI.GetItemGlobalID("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblHeader"), starSystemInfo.Name);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblValue"), "");
							this._starmap.PostSetProp("ProvincePoolEffect", (object)true, (object)this._starmap.Systems.Reverse[starSystemInfo.ID]);
						}
					}
					if (list4.Count > 0)
					{
						this._app.UI.SetSelection("lstItems", list4.First<StarSystemInfo>().ID);
						break;
					}
					this._app.UI.SetEnabled("btnFinishRequest", false);
					break;
				case RequestType.EstablishEnclaveRequest:
					colonizedSystemIds = this._app.GameDatabase.GetPlayerColonySystemIDs(this._otherPlayer).ToList<int>();
					List<StarSystemInfo> list5 = list1.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   if (colonizedSystemIds.Contains(x.ID))
						   return StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, x, (Dictionary<int, List<ShipInfo>>)null);
					   return false;
				   })).ToList<StarSystemInfo>();
					foreach (StarSystemInfo starSystemInfo in list5)
					{
						if (starSystemInfo.IsVisible)
						{
							this._app.UI.AddItem("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							string itemGlobalId = this._app.UI.GetItemGlobalID("lstItems", string.Empty, starSystemInfo.ID, string.Empty);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblHeader"), starSystemInfo.Name);
							this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblValue"), "");
							this._starmap.PostSetProp("ProvincePoolEffect", (object)true, (object)this._starmap.Systems.Reverse[starSystemInfo.ID]);
						}
					}
					if (list5.Count > 0)
					{
						this._app.UI.SetSelection("lstItems", list5.First<StarSystemInfo>().ID);
						break;
					}
					this._app.UI.SetEnabled("btnFinishRequest", false);
					break;
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnFinishRequest")
				{
					this._app.GameDatabase.SpendDiplomacyPoints(this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID), this._app.GameDatabase.GetPlayerFactionID(this._otherPlayer), this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type));
					this._app.GameDatabase.InsertRequest(this._request);
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					if (!(panelName == "btnCancel"))
						return;
					this._request = (RequestInfo)null;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else
			{
				if (!(msgType == "list_sel_changed"))
					return;
				if (this._starmap.Systems.Reverse.ContainsKey((int)this._request.RequestValue))
					this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)false, (object)this._starmap.Systems.Reverse[(int)this._request.RequestValue]);
				this._request.RequestValue = float.Parse(msgParams[0]);
				if (!this._starmap.Systems.Reverse.ContainsKey((int)this._request.RequestValue))
					return;
				this._starmap.SetFocus((IGameObject)this._starmap.Systems.Reverse[(int)this._request.RequestValue]);
				this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)true, (object)this._starmap.Systems.Reverse[(int)this._request.RequestValue]);
				this._app.Game.StarMapSelectedObject = (object)this._starmap.Systems.Reverse[(int)this._request.RequestValue];
			}
		}

		public override string[] CloseDialog()
		{
			List<string> stringList = new List<string>();
			if (this._request == null)
				stringList.Add("true");
			else
				stringList.Add("false");
			this._crits.Dispose();
			this._app.GetGameState<StarMapState>().RefreshCameraControl();
			return stringList.ToArray();
		}
	}
}
