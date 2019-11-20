// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DemandSystemSelectDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DemandSystemSelectDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string ItemList = "lstItems";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private const string StarmapObjectHost = "ohStarmap";
		private int _otherPlayer;
		private DemandType _type;
		private DemandInfo _demand;
		private GameObjectSet _crits;
		private Sky _sky;
		private StarMap _starmap;

		public DemandSystemSelectDialog(App game, DemandType type, int otherPlayer, string template = "dialogRequestSystemSelect")
		  : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._demand = new DemandInfo();
			this._demand.InitiatingPlayer = game.LocalPlayer.ID;
			this._demand.ReceivingPlayer = this._otherPlayer;
			this._demand.State = AgreementState.Unrequested;
			this._demand.Type = type;
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
			this._app.UI.SetText("lblHeader", string.Format(App.Localize(DemandTypeDialog.DemandTypeLocMap[this._type]), (object)this._app.AssetDatabase.GetDiplomaticDemandPointCost(this._type)));
			this._app.UI.SetEnabled("btnFinishRequest", true);
			List<StarSystemInfo> StarSystems = this._app.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			switch (this._type)
			{
				case DemandType.SystemInfoDemand:
					List<StarSystemInfo> list1 = StarSystems.ToList<StarSystemInfo>();
					foreach (StarSystemInfo starSystemInfo in list1)
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
					if (list1.Count > 0)
					{
						this._app.UI.SetSelection("lstItems", list1.First<StarSystemInfo>().ID);
						break;
					}
					this._app.UI.SetEnabled("btnFinishRequest", false);
					break;
				case DemandType.WorldDemand:
					List<int> colonizedSystemIds = this._app.GameDatabase.GetPlayerColonySystemIDs(this._otherPlayer).ToList<int>();
					List<StarSystemInfo> list2 = StarSystems.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
				   {
					   if (colonizedSystemIds.Contains(x.ID))
						   return StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, x, (Dictionary<int, List<ShipInfo>>)null);
					   return false;
				   })).ToList<StarSystemInfo>();
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
				case DemandType.ProvinceDemand:
					List<ProvinceInfo> list3 = this._app.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>().Where<ProvinceInfo>((Func<ProvinceInfo, bool>)(x =>
				   {
					   if (x.PlayerID == this._otherPlayer)
						   return StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, StarSystems.First<StarSystemInfo>((Func<StarSystemInfo, bool>)(y => y.ID == x.CapitalSystemID)), (Dictionary<int, List<ShipInfo>>)null);
					   return false;
				   })).ToList<ProvinceInfo>();
					foreach (ProvinceInfo provinceInfo in list3)
					{
						this._app.UI.AddItem("lstItems", string.Empty, provinceInfo.ID, string.Empty);
						string itemGlobalId = this._app.UI.GetItemGlobalID("lstItems", string.Empty, provinceInfo.ID, string.Empty);
						this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblHeader"), provinceInfo.Name);
						this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblValue"), "");
						this._starmap.PostSetProp("ProvincePoolEffect", (object)true, (object)this._starmap.Systems.Reverse[provinceInfo.CapitalSystemID]);
					}
					if (list3.Count > 0)
					{
						this._app.UI.SetSelection("lstItems", list3.First<ProvinceInfo>().ID);
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
					this._app.GameDatabase.SpendDiplomacyPoints(this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID), this._app.GameDatabase.GetPlayerFactionID(this._otherPlayer), this._app.AssetDatabase.GetDiplomaticDemandPointCost(this._type));
					this._app.GameDatabase.InsertDemand(this._demand);
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					if (!(panelName == "btnCancel"))
						return;
					this._demand = (DemandInfo)null;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else
			{
				if (!(msgType == "list_sel_changed") || !(panelName == "lstItems"))
					return;
				if (this._starmap.Systems.Reverse.ContainsKey((int)this._demand.DemandValue))
					this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)false, (object)this._starmap.Systems.Reverse[(int)this._demand.DemandValue]);
				this._demand.DemandValue = float.Parse(msgParams[0]);
				if (!this._starmap.Systems.Reverse.ContainsKey((int)this._demand.DemandValue))
					return;
				this._starmap.SetFocus((IGameObject)this._starmap.Systems.Reverse[(int)this._demand.DemandValue]);
				this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)true, (object)this._starmap.Systems.Reverse[(int)this._demand.DemandValue]);
				this._app.Game.StarMapSelectedObject = (object)this._starmap.Systems.Reverse[(int)this._demand.DemandValue];
			}
		}

		public override string[] CloseDialog()
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			List<string> stringList = new List<string>();
			if (this._demand == null)
				stringList.Add("true");
			else
				stringList.Add("false");
			return stringList.ToArray();
		}
	}
}
