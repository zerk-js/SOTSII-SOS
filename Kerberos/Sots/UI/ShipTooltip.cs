// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ShipTooltip
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class ShipTooltip
	{
		private static Rectangle ArmorPanelShape = new Rectangle()
		{
			X = 390f,
			Y = 22f,
			W = 54f,
			H = 54f
		};
		private string _rootPanel = "";
		private const string LabelDesignName = "lblDesignName";
		private const string LabelShipName = "lblShipName";
		private const string LabelFirstSection = "lblFirstSection";
		private const string LabelSecondSection = "lblSecondSection";
		private const string LabelThirdSection = "lblThirdSection";
		private const string LabelAttributeName = "lblAttributeName";
		private const string LabelUpkeepName = "lblUpkeep";
		private const string LabelShipAge = "lblShipAge";
		private const string LabelSupply = "lblSupply";
		private const string LabelPower = "lblPower";
		private const string LabelCrew = "lblCrew";
		private const string LabelEndurance = "lblEndurance";
		private const string ListWeaponIcons = "lstWeaponIcons";
		private const string ObjectHostShip = "ohShip";
		private int _shipID;
		private int _designID;
		public App _game;
		public App App;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private ShipBuilder _builder;
		private ShipHoloView _shipHoloView;
		private bool _ready;
		private bool _activated;

		public ShipTooltip(App app)
		{
			this._game = app;
			this.App = app;
			this._rootPanel = this.App.UI.CreatePanelFromTemplate("ShipinfoTooltip", null);
		}

		public void Initialize()
		{
			this._builder = new ShipBuilder(this.App);
			this._crits = new GameObjectSet(this.App);
			this._camera = new OrbitCameraController(this.App);
			this._camera.DesiredDistance = 10000f;
			this._crits.Add((IGameObject)this._camera);
			this._shipHoloView = new ShipHoloView(this.App, this._camera);
			this._shipID = 0;
		}

		public void SyncShipTooltip(int shipid)
		{
			if (shipid == this._shipID)
				return;
			this.SyncDesignTooltip(this._rootPanel, shipid);
			this._shipID = shipid;
			this._designID = -1;
		}

		public void SyncShipTooltipByDesignID(int designid)
		{
			if (this._designID == designid)
				return;
			this.SyncDesignTooltipByDesign(this._rootPanel, designid);
			this._designID = designid;
			this._shipID = -1;
		}

		public int GetShipID()
		{
			return this._shipID;
		}

		public bool isvalid()
		{
			if (this._builder != null && this._shipHoloView != null)
				return this._crits != null;
			return false;
		}

		public string GetPanelID()
		{
			return this._rootPanel;
		}

		private void SyncDesignTooltip(string panelId, int shipId)
		{
			ShipInfo shipInfo = this._game.GameDatabase.GetShipInfo(shipId, true);
			List<SectionEnumerations.DesignAttribute> list1 = this._game.GameDatabase.GetDesignAttributesForDesign(shipInfo.DesignInfo.ID).ToList<SectionEnumerations.DesignAttribute>();
			FleetInfo fleetInfo = this._game.GameDatabase.GetFleetInfo(shipInfo.FleetID);
			List<int> intList = new List<int>();
			foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
			{
				foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
				{
					if (weaponBank.WeaponID.HasValue && !intList.Contains(weaponBank.WeaponID.Value))
						intList.Add(weaponBank.WeaponID.Value);
				}
			}
			string str = "";
			switch (shipInfo.DesignInfo.Class)
			{
				case ShipClass.Cruiser:
					str = App.Localize("@SHIPCLASSES_ABBR_CR");
					break;
				case ShipClass.Dreadnought:
					str = App.Localize("@SHIPCLASSES_ABBR_DN");
					break;
				case ShipClass.Leviathan:
					str = App.Localize("@SHIPCLASSES_ABBR_LV");
					break;
			}
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblDesignName"), shipInfo.ShipName);
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblShipName"), str + " - " + shipInfo.DesignInfo.Name);
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblFirstSection"), "");
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblSecondSection"), "");
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblThirdSection"), "");
			foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
			{
				switch (designSection.ShipSectionAsset.Type)
				{
					case ShipSectionType.Command:
						this.App.UI.SetText(this.App.UI.Path(panelId, "lblFirstSection"), App.Localize(designSection.ShipSectionAsset.Title));
						break;
					case ShipSectionType.Mission:
						this.App.UI.SetText(this.App.UI.Path(panelId, "lblSecondSection"), App.Localize(designSection.ShipSectionAsset.Title));
						break;
					case ShipSectionType.Engine:
						this.App.UI.SetText(this.App.UI.Path(panelId, "lblThirdSection"), App.Localize(designSection.ShipSectionAsset.Title));
						break;
				}
			}
			float scale = this.App.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.Elite) ? this.App.AssetDatabase.EliteUpkeepCostScale : 1f;
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblAttributeName"), !shipInfo.DesignInfo.isAttributesDiscovered || list1.Count <= 0 ? "" : App.Localize("@UI_" + list1.First<SectionEnumerations.DesignAttribute>().ToString()));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblUpkeep"), GameSession.CalculateShipUpkeepCost(this._game.AssetDatabase, shipInfo.DesignInfo, scale, fleetInfo.IsReserveFleet).ToString());
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblShipAge"), (this.App.GameDatabase.GetTurnCount() - shipInfo.ComissionDate).ToString() + " " + App.Localize("@UI_SHIPTOOLTIPAGE"));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblSupply"), string.Format("{0}/{1}", (object)shipInfo.DesignInfo.SupplyRequired, (object)shipInfo.DesignInfo.SupplyAvailable));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblPower"), string.Format("{0}/{1}", (object)shipInfo.DesignInfo.PowerRequired, (object)shipInfo.DesignInfo.PowerAvailable));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblCrew"), string.Format("{0}/{1}", (object)shipInfo.DesignInfo.CrewRequired, (object)shipInfo.DesignInfo.CrewAvailable));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblEndurance"), shipInfo.DesignInfo.GetEndurance(this._game.Game).ToString());
			this.App.UI.ClearItems("lstWeaponIcons");
			foreach (int num in intList)
			{
				string asset = this.App.GameDatabase.GetWeaponAsset(num);
				LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == asset));
				if (logicalWeapon != null)
				{
					this.App.UI.AddItem("lstWeaponIcons", string.Empty, num, "");
					this.App.UI.SetPropertyString(this.App.UI.Path(this.App.UI.GetItemGlobalID("lstWeaponIcons", string.Empty, num, ""), "imgWeaponIcon"), "sprite", logicalWeapon.IconSpriteName);
				}
			}
			List<SectionInstanceInfo> list2 = this.App.GameDatabase.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>();
			Dictionary<ArmorSide, float> dictionary1 = new Dictionary<ArmorSide, float>();
			Dictionary<ArmorSide, float> dictionary2 = new Dictionary<ArmorSide, float>();
			if (list2.Count > 0)
			{
				ArmorSide[] values = (ArmorSide[])Enum.GetValues(typeof(ArmorSide));
				foreach (SectionInstanceInfo sectionInstanceInfo in list2)
				{
					foreach (ArmorSide key in values)
					{
						if (key != ArmorSide.NumSides)
						{
							if (!dictionary1.ContainsKey(key))
								dictionary1.Add(key, 0.0f);
							if (!dictionary2.ContainsKey(key))
								dictionary2.Add(key, 0.0f);
							if (sectionInstanceInfo.Armor.ContainsKey(key))
							{
								Dictionary<ArmorSide, float> dictionary3;
								ArmorSide index1;
								(dictionary3 = dictionary1)[index1 = key] = dictionary3[index1] + (float)sectionInstanceInfo.Armor[key].GetTotalFilled();
								Dictionary<ArmorSide, float> dictionary4;
								ArmorSide index2;
								(dictionary4 = dictionary2)[index2 = key] = dictionary4[index2] + (float)sectionInstanceInfo.Armor[key].GetTotalPoints();
							}
						}
					}
				}
				float num1 = (double)dictionary2[ArmorSide.Top] == 0.0 ? 1f : dictionary1[ArmorSide.Top] / dictionary2[ArmorSide.Top];
				float num2 = (double)dictionary2[ArmorSide.Bottom] == 0.0 ? 1f : dictionary1[ArmorSide.Bottom] / dictionary2[ArmorSide.Bottom];
				float num3 = (double)dictionary2[ArmorSide.Left] == 0.0 ? 1f : dictionary1[ArmorSide.Left] / dictionary2[ArmorSide.Left];
				float num4 = (double)dictionary2[ArmorSide.Right] == 0.0 ? 1f : dictionary1[ArmorSide.Right] / dictionary2[ArmorSide.Right];
				float num5 = ShipTooltip.ArmorPanelShape.W / 2f;
				this.App.UI.SetShape("top_armor", 0, (int)((double)num5 * (1.0 - (double)num1)), (int)ShipTooltip.ArmorPanelShape.W, (int)((double)num5 * (double)num1));
				this.App.UI.SetShape("bottom_armor", 0, (int)num5, (int)ShipTooltip.ArmorPanelShape.W, (int)((double)num2 * (double)num5));
				this.App.UI.SetShape("left_armor", (int)((double)num5 * (1.0 - (double)num3)), 0, (int)((double)num5 * (double)num3), (int)ShipTooltip.ArmorPanelShape.H);
				this.App.UI.SetShape("right_armor", (int)num5, 0, (int)((double)num5 * (double)num4), (int)ShipTooltip.ArmorPanelShape.H);
			}
			else
			{
				float num = ShipTooltip.ArmorPanelShape.W / 2f;
				this.App.UI.SetShape("top_armor", 0, 0, (int)ShipTooltip.ArmorPanelShape.W, (int)num);
				this.App.UI.SetShape("bottom_armor", 0, (int)num, (int)ShipTooltip.ArmorPanelShape.W, (int)num);
				this.App.UI.SetShape("left_armor", 0, 0, (int)num, (int)ShipTooltip.ArmorPanelShape.H);
				this.App.UI.SetShape("right_armor", (int)num, 0, (int)num, (int)ShipTooltip.ArmorPanelShape.H);
			}
			this._builder.New(this.App.GetPlayer(shipInfo.DesignInfo.PlayerID), shipInfo.DesignInfo, shipInfo.ShipName, shipInfo.SerialNumber, false);
			this._ready = false;
			this._activated = false;
		}

		private void SyncDesignTooltipByDesign(string panelId, int designID)
		{
			DesignInfo designInfo = this._game.GameDatabase.GetDesignInfo(designID);
			List<SectionEnumerations.DesignAttribute> list = this._game.GameDatabase.GetDesignAttributesForDesign(designInfo.ID).ToList<SectionEnumerations.DesignAttribute>();
			List<int> intList = new List<int>();
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
				{
					if (weaponBank.WeaponID.HasValue && !intList.Contains(weaponBank.WeaponID.Value))
						intList.Add(weaponBank.WeaponID.Value);
				}
			}
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblDesignName"), designInfo.Name);
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblShipName"), "");
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblFirstSection"), ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Count<DesignSectionInfo>() > 0 ? designInfo.DesignSections[0].ShipSectionAsset.Title : "");
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblSecondSection"), ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Count<DesignSectionInfo>() > 1 ? designInfo.DesignSections[1].ShipSectionAsset.Title : "");
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblThirdSection"), ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Count<DesignSectionInfo>() > 2 ? designInfo.DesignSections[2].ShipSectionAsset.Title : "");
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblAttributeName"), !designInfo.isAttributesDiscovered || list.Count <= 0 ? "" : App.Localize("@UI_" + list.First<SectionEnumerations.DesignAttribute>().ToString()));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblUpkeep"), GameSession.CalculateShipUpkeepCost(this._game.AssetDatabase, designInfo, 1f, false).ToString());
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblShipAge"), "");
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblSupply"), string.Format("{0}/{1}", (object)designInfo.SupplyRequired, (object)designInfo.SupplyAvailable));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblPower"), string.Format("{0}/{1}", (object)designInfo.PowerRequired, (object)designInfo.PowerAvailable));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblCrew"), string.Format("{0}/{1}", (object)designInfo.CrewRequired, (object)designInfo.CrewAvailable));
			this.App.UI.SetText(this.App.UI.Path(panelId, "lblEndurance"), designInfo.GetEndurance(this._game.Game).ToString());
			this.App.UI.ClearItems("lstWeaponIcons");
			foreach (int num in intList)
			{
				string asset = this.App.GameDatabase.GetWeaponAsset(num);
				LogicalWeapon logicalWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == asset));
				if (logicalWeapon != null)
				{
					this.App.UI.AddItem("lstWeaponIcons", string.Empty, num, "");
					this.App.UI.SetPropertyString(this.App.UI.Path(this.App.UI.GetItemGlobalID("lstWeaponIcons", string.Empty, num, ""), "imgWeaponIcon"), "sprite", logicalWeapon.IconSpriteName);
				}
			}
			float num1 = ShipTooltip.ArmorPanelShape.W / 2f;
			this.App.UI.SetShape("top_armor", 0, 0, (int)ShipTooltip.ArmorPanelShape.W, (int)num1);
			this.App.UI.SetShape("bottom_armor", 0, (int)num1, (int)ShipTooltip.ArmorPanelShape.W, (int)num1);
			this.App.UI.SetShape("left_armor", 0, 0, (int)num1, (int)ShipTooltip.ArmorPanelShape.H);
			this.App.UI.SetShape("right_armor", (int)num1, 0, (int)num1, (int)ShipTooltip.ArmorPanelShape.H);
			this._builder.New(this.App.GetPlayer(designInfo.PlayerID), designInfo, "The Ship You Wish your Ship was", 0, false);
			this._ready = false;
			this._activated = false;
		}

		public void Update()
		{
			if (this._builder == null || this._shipHoloView == null || this._crits == null)
				return;
			this._builder.Update();
			if (this._builder.Ship == null || !this._builder.Ship.Active || (this._ready || this._crits == null) || !this._crits.IsReady())
				return;
			this._ready = true;
			if (!this._activated)
			{
				this._activated = true;
				this._crits.Activate();
				this._camera.MaxDistance = 2000f;
				this._camera.DesiredDistance = 300f;
				this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
				this._camera.DesiredPitch = MathHelper.DegreesToRadians(90f);
			}
			this._shipHoloView.SetShip(this._builder.Ship);
			this.App.UI.Send((object)"SetGameObject", (object)this.App.UI.Path(this._rootPanel, "ohShip"), (object)this._shipHoloView.ObjectID);
		}

		public void Clear()
		{
			this._builder.Clear();
		}

		public void Dispose(bool KeepRoot = false)
		{
			if (this._builder != null)
				this._builder.Dispose();
			if (this._shipHoloView != null)
				this._shipHoloView.Dispose();
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			if (!KeepRoot)
				this.App.UI.DestroyPanel(this._rootPanel);
			this._shipID = 0;
		}
	}
}
