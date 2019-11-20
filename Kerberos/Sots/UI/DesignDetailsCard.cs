// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DesignDetailsCard
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DesignDetailsCard : PanelBinding
	{
		private static string UIDesignName = "shipDesignName";
		private static string UICrewPanel = "UICrew";
		private static string UICrewValue = "CrewValueLabel";
		private static string UISupplyPanel = "UISupply";
		private static string UISupplyValue = "SupplyValueLabel";
		private static string UIPowerPanel = "UIPower";
		private static string UIPowerValue = "PowerValueLabel";
		private static string UIUpkeepLabel = "UpkeepLabel";
		private static string UIEnduranceLabel = "EnduranceLabel";
		private static string UIConstructionCostLabelValue = "ConstructionCostValueLabel";
		private static string UIShipCostLabelValue = "ShipCostValueLabel";
		private static string UIShipCommandLabel = "CommandSectionLabel";
		private static string UIShipMissionLabel = "MissionSectionLabel";
		private static string UIShipEngineLabel = "EngineSectionLabel";
		private static string ListWeaponIcons = "lstWeaponIcons";
		private App App;
		private DesignInfo Design;
		private ShipInfo Ship;

		public DesignDetailsCard(App game, int designid, int? ShipID, UICommChannel ui, string id)
		  : base(ui, id)
		{
			this.App = game;
			this.Design = this.App.GameDatabase.GetDesignInfo(designid);
			if (ShipID.HasValue)
				this.Ship = this.App.GameDatabase.GetShipInfo(ShipID.Value, false);
			else
				this.Ship = (ShipInfo)null;
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
		}

		public void Initialize()
		{
			this.FillOutCard();
		}

		public void FillOutCard()
		{
			Faction faction = this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerInfo(this.Design.PlayerID).FactionID);
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIDesignName), this.Design.Name);
			this.App.UI.SetVisible(this.App.UI.Path(this.ID, DesignDetailsCard.UICrewPanel), (faction.Name != "loa" ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this.ID, "constructionBg"), (faction.Name == "loa" ? 1 : 0) != 0);
			this.App.UI.SetVisible(this.App.UI.Path(this.ID, "standardBg"), (faction.Name != "loa" ? 1 : 0) != 0);
			int crewAvailable = this.Design.CrewAvailable;
			int num1 = crewAvailable;
			int supplyAvailable = this.Design.SupplyAvailable;
			int num2 = supplyAvailable;
			int powerAvailable = this.Design.PowerAvailable;
			int num3 = powerAvailable;
			float scale = 1f;
			if (this.Ship != null)
			{
				num1 = 0;
				num2 = 0;
				foreach (SectionInstanceInfo sectionInstanceInfo in this.App.GameDatabase.GetShipSectionInstances(this.Ship.ID).ToList<SectionInstanceInfo>())
				{
					num1 += sectionInstanceInfo.Crew;
					num2 += sectionInstanceInfo.Supply;
				}
				FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this.Ship.FleetID);
				if (fleetInfo != null && this.App.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.Elite))
					scale = this.App.AssetDatabase.EliteUpkeepCostScale;
			}
			double shipUpkeepCost = GameSession.CalculateShipUpkeepCost(this.App.AssetDatabase, this.Design, scale, false);
			int endurance = this.Design.GetEndurance(this.App.Game);
			int playerProductionCost = this.Design.GetPlayerProductionCost(this.App.GameDatabase, this.Design.PlayerID, !this.Design.isPrototyped, new float?());
			int savingsCost = this.Design.SavingsCost;
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIShipCommandLabel), "");
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIShipMissionLabel), "");
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIShipEngineLabel), "");
			List<int> intList = new List<int>();
			foreach (DesignSectionInfo designSection in this.Design.DesignSections)
			{
				if (designSection.ShipSectionAsset.Type == ShipSectionType.Command)
					this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIShipCommandLabel), App.Localize(designSection.ShipSectionAsset.Title));
				else if (designSection.ShipSectionAsset.Type == ShipSectionType.Mission)
					this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIShipMissionLabel), App.Localize(designSection.ShipSectionAsset.Title));
				else if (designSection.ShipSectionAsset.Type == ShipSectionType.Engine)
					this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIShipEngineLabel), App.Localize(designSection.ShipSectionAsset.Title));
				foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
				{
					if (weaponBank.WeaponID.HasValue && !intList.Contains(weaponBank.WeaponID.Value))
						intList.Add(weaponBank.WeaponID.Value);
				}
			}
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UICrewValue), num1.ToString() + "/" + crewAvailable.ToString());
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UISupplyValue), num2.ToString() + "/" + supplyAvailable.ToString());
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIPowerValue), num3.ToString() + "/" + powerAvailable.ToString());
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIUpkeepLabel), "Upkeep " + shipUpkeepCost.ToString());
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIEnduranceLabel), "Endurance " + endurance.ToString() + "T");
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIConstructionCostLabelValue), playerProductionCost.ToString("N0"));
			this.App.UI.SetText(this.App.UI.Path(this.ID, DesignDetailsCard.UIShipCostLabelValue), savingsCost.ToString("N0"));
			this.App.UI.ClearItems(DesignDetailsCard.ListWeaponIcons);
			foreach (int num4 in intList)
			{
				string asset = this.App.GameDatabase.GetWeaponAsset(num4);
				LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == asset));
				if (logicalWeapon != null)
				{
					this.App.UI.AddItem(DesignDetailsCard.ListWeaponIcons, string.Empty, num4, "");
					this.App.UI.SetPropertyString(this.App.UI.Path(this.App.UI.GetItemGlobalID(DesignDetailsCard.ListWeaponIcons, string.Empty, num4, ""), "imgWeaponIcon"), "sprite", logicalWeapon.IconSpriteName);
				}
			}
		}

		public void SyncDesign(int DesignID, int? Shipid)
		{
			this.Design = this.App.GameDatabase.GetDesignInfo(DesignID);
			this.Ship = !Shipid.HasValue ? (ShipInfo)null : this.App.GameDatabase.GetShipInfo(Shipid.Value, false);
			this.FillOutCard();
		}
	}
}
