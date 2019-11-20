// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.LimitationTreatyBrokenDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class LimitationTreatyBrokenDialog : Dialog
	{
		private Dictionary<DiplomacyState, string> _diplomacyTypeLocMap = new Dictionary<DiplomacyState, string>()
	{
	  {
		DiplomacyState.ALLIED,
		"@UI_DIPLOMACY_STATE_ALLIED"
	  },
	  {
		DiplomacyState.CEASE_FIRE,
		"@UI_DIPLOMACY_STATE_CEASE_FIRE"
	  },
	  {
		DiplomacyState.NEUTRAL,
		"@UI_DIPLOMACY_STATE_NEUTRAL"
	  },
	  {
		DiplomacyState.NON_AGGRESSION,
		"@UI_DIPLOMACY_STATE_NON_AGGRESSION"
	  },
	  {
		DiplomacyState.PEACE,
		"@UI_DIPLOMACY_STATE_PEACE"
	  },
	  {
		DiplomacyState.WAR,
		"@UI_DIPLOMACY_STATE_WAR"
	  }
	};
		private Dictionary<TreatyType, string> _treatyPanelMap = new Dictionary<TreatyType, string>()
	{
	  {
		TreatyType.Armistice,
		"pnlArmisticeTreaty"
	  },
	  {
		TreatyType.Limitation,
		"pnlLimitationTreaty"
	  },
	  {
		TreatyType.Trade,
		"pnlTradeTreaty"
	  },
	  {
		TreatyType.Incorporate,
		"pnlIncorporateTreaty"
	  },
	  {
		TreatyType.Protectorate,
		"pnlProtectorateTreaty"
	  }
	};
		private Dictionary<ShipClass, string> _shipClassLocStringMap = new Dictionary<ShipClass, string>()
	{
	  {
		ShipClass.BattleRider,
		"@SHIPCLASSES_BATTLE_RIDER"
	  },
	  {
		ShipClass.Cruiser,
		"@SHIPCLASSES_CRUISER"
	  },
	  {
		ShipClass.Dreadnought,
		"@SHIPCLASSES_DREADNOUGHT"
	  },
	  {
		ShipClass.Leviathan,
		"@SHIPCLASSES_LEVIATHAN"
	  }
	};
		private Dictionary<StationType, string> _stationTypeLocStringMap = new Dictionary<StationType, string>()
	{
	  {
		StationType.CIVILIAN,
		"@STATION_TYPE_CIVILIAN"
	  },
	  {
		StationType.DEFENCE,
		"@STATION_TYPE_DEFENCE"
	  },
	  {
		StationType.DIPLOMATIC,
		"@STATION_TYPE_DIPLOMATIC"
	  },
	  {
		StationType.GATE,
		"@STATION_TYPE_GATE"
	  },
	  {
		StationType.MINING,
		"@STATION_TYPE_MINING"
	  },
	  {
		StationType.NAVAL,
		"@STATION_TYPE_NAVAL"
	  },
	  {
		StationType.SCIENCE,
		"@STATION_TYPE_SCIENCE"
	  }
	};
		private const string IncentiveList = "lstIncentives";
		private const string ConsequencesList = "lstLimitationTreatyConsequences";
		private const string AcceptButton = "btnAccept";
		private const string DeclineButton = "btnDecline";
		private bool _isVictim;
		private TreatyInfo _treatyInfo;

		public LimitationTreatyBrokenDialog(App game, int treatyId, bool isVictim)
		  : base(game, "dialogTreatyBroken_Limitation")
		{
			this._treatyInfo = game.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>().FirstOrDefault<TreatyInfo>((Func<TreatyInfo, bool>)(x => x.ID == treatyId));
			this._isVictim = isVictim;
			if (this._treatyInfo != null)
				return;
			game.UI.CloseDialog((Dialog)this, true);
		}

		public override void Initialize()
		{
			foreach (KeyValuePair<TreatyType, string> treatyPanel in this._treatyPanelMap)
				this._app.UI.SetVisible(treatyPanel.Value, false);
			this._app.UI.SetVisible(this._treatyPanelMap[TreatyType.Limitation], true);
			PlayerInfo playerInfo1 = this._app.GameDatabase.GetPlayerInfo(this._treatyInfo.InitiatingPlayerId);
			this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblIncentives"), "text", App.Localize("@UI_TREATY_REQUEST_INCENTIVES"));
			PlayerInfo playerInfo2 = this._app.GameDatabase.GetPlayerInfo(this._treatyInfo.ReceivingPlayerId);
			new DiplomacyCard(this._app, playerInfo1.ID != this._app.LocalPlayer.ID ? playerInfo1.ID : playerInfo2.ID, this.UI, "playerdiplocard1").Initialize();
			this._app.UI.ClearItems("lstIncentives");
			foreach (TreatyIncentiveInfo incentive in this._treatyInfo.Incentives)
				this._app.UI.AddItem("lstIncentives", string.Empty, incentive.ID, string.Format("{0} {1}", (object)App.Localize(TreatyEditDialog.IncentiveTypeLocMap[incentive.Type]), (object)incentive.IncentiveValue));
			switch (this._treatyInfo.Type)
			{
				case TreatyType.Armistice:
					this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblArmisticeTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_ARMISTICE_DESC"), (object)App.Localize(this._diplomacyTypeLocMap[((ArmisticeTreatyInfo)this._treatyInfo).SuggestedDiplomacyState]), (object)playerInfo1.Name));
					break;
				case TreatyType.Trade:
					this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblTradeTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_TRADE_DESC"), (object)this._treatyInfo.Duration, (object)playerInfo1.Name));
					break;
				case TreatyType.Limitation:
					LimitationTreatyInfo lti = (LimitationTreatyInfo)this._treatyInfo;
					this._app.UI.ClearItems("lstLimitationTreatyConsequences");
					foreach (TreatyConsequenceInfo consequence in lti.Consequences)
						this._app.UI.AddItem("lstLimitationTreatyConsequences", string.Empty, consequence.ID, string.Format("{0} {1}", (object)App.Localize(TreatyEditDialog.ConsequenceTypeLocMap[consequence.Type]), (object)consequence.ConsequenceValue));
					string str;
					switch (lti.LimitationType)
					{
						case LimitationTreatyType.FleetSize:
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_FLEETSIZE"), (object)lti.Duration, (object)lti.LimitationAmount, (object)playerInfo2.Name));
							break;
						case LimitationTreatyType.ShipClass:
							str = App.Localize(this._shipClassLocStringMap[(ShipClass)int.Parse(lti.LimitationGroup)]);
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_SHIPCLASS"), (object)lti.Duration, (object)lti.LimitationGroup, (object)lti.LimitationAmount, (object)playerInfo2.Name));
							break;
						case LimitationTreatyType.Weapon:
							str = this._app.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.UniqueWeaponID == int.Parse(lti.LimitationGroup))).Name;
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_WEAPON"), (object)lti.Duration, (object)lti.LimitationGroup, (object)playerInfo2.Name));
							break;
						case LimitationTreatyType.ResearchTree:
							string name1 = this._app.AssetDatabase.MasterTechTree.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == lti.LimitationGroup)).Name;
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_RESEARCHTREE"), (object)lti.Duration, (object)name1, (object)playerInfo2.Name));
							break;
						case LimitationTreatyType.ResearchTech:
							string name2 = this._app.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(x => x.Id == lti.LimitationGroup)).Name;
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_RESEARCHTECH"), (object)lti.Duration, (object)name2, (object)playerInfo2.Name));
							break;
						case LimitationTreatyType.EmpireSize:
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_EMPIRESIZE"), (object)lti.Duration, (object)lti.LimitationAmount, (object)playerInfo2.Name));
							break;
						case LimitationTreatyType.ForgeGemWorlds:
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_FORGEGEMWORLDS"), (object)lti.Duration, (object)lti.LimitationAmount, (object)playerInfo2.Name));
							break;
						case LimitationTreatyType.StationType:
							str = App.Localize(this._stationTypeLocStringMap[(StationType)int.Parse(lti.LimitationGroup)]);
							this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyDesc"), "text", string.Format(App.Localize("@UI_TREATY_REQUEST_LIMITATION_BROKEN_STATIONTYPE"), (object)lti.Duration, (object)lti.LimitationGroup, (object)lti.LimitationAmount, (object)playerInfo2.Name));
							break;
					}
					break;
			}
			if (!this._isVictim)
				this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyConsequences"), "text", string.Format(App.Localize("@UI_TREATY_BROKEN_VICTIM"), (object)playerInfo2.Name));
			else
				this._app.UI.SetPropertyString(this.UI.Path(this.ID, "lblLimitationTreatyConsequences"), "text", string.Format(App.Localize("@UI_TREATY_BROKEN_OFFENDER"), (object)playerInfo1.Name));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "btnAccept"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
