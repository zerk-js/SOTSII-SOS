// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialogs.CounterIntelSelectTechDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI.Dialogs
{
	internal class CounterIntelSelectTechDialog : Dialog
	{
		private readonly string LeftContentList = "CIleftContent";
		private readonly string RightContentList = "CIrightContent";
		private readonly string SystemButton = "systembtn";
		private readonly string ToggleButton = "techtoggle";
		private readonly string HeaderText = "headertxt";
		private readonly string DirectionTest = "dirtext";
		private int intelMissionID;
		private readonly GameSession _game;
		private IntelMissionInfo missioninfo;
		private Dictionary<int, string> _techFamilyPanels;
		private Dictionary<int, TechFamily> _FamilyID;
		private Dictionary<string, bool> _CheckedTechs;
		private Dictionary<string, string> _TechPanels;
		private Dictionary<string, string> _TechToggles;

		public CounterIntelSelectTechDialog(GameSession game, int intel_mission_id)
		  : base(game.App, "counterIntel_Standard")
		{
			this.intelMissionID = intel_mission_id;
			this.missioninfo = game.GameDatabase.GetIntelInfo(this.intelMissionID);
			this._game = game;
		}

		public override void Initialize()
		{
			this._techFamilyPanels = new Dictionary<int, string>();
			this._FamilyID = new Dictionary<int, TechFamily>();
			this._CheckedTechs = new Dictionary<string, bool>();
			this._TechPanels = new Dictionary<string, string>();
			this._TechToggles = new Dictionary<string, string>();
			this._app.UI.SetEnabled(this.UI.Path(this.ID, "okbtn"), false);
			this.PopulateTechFamilyList(true);
			this._game.UI.SetText(this.UI.Path(this.ID, this.HeaderText), App.Localize("@UI_COUNTER_INTEL_TECH"));
			this._game.UI.SetText(this.UI.Path(this.ID, this.DirectionTest), App.Localize("@UI_COUNTER_INTEL_TECH_DIR"));
		}

		private void PopulateTechFamilyList(bool SelectFirst = false)
		{
			int num = 0;
			foreach (TechFamily techFamily in this._game.AssetDatabase.MasterTechTree.TechFamilies)
			{
				TechFamily techf = techFamily;
				if (this._game.GameDatabase.GetPlayerTechInfos(this._game.LocalPlayer.ID).FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Tech>((Func<Tech, bool>)(y => y.Id == x.TechFileID)).Family == techf.Id)) != null)
				{
					this._game.UI.AddItem(this.LeftContentList, "", num, "", "FamilyTechCard");
					this._FamilyID.Add(num, techf);
					string itemGlobalId = this._game.UI.GetItemGlobalID(this.LeftContentList, "", num, "");
					this._techFamilyPanels.Add(num, itemGlobalId);
					this._game.UI.SetPropertyString(this._game.UI.Path(itemGlobalId, this.SystemButton), "id", this.SystemButton + "|" + (object)num);
					this._game.UI.SetText(this._game.UI.Path(itemGlobalId, "techLabel"), techf.Name);
					this._game.UI.SetPropertyString(this._game.UI.Path(itemGlobalId, "icon"), "sprite", Path.GetFileNameWithoutExtension(techf.Icon));
					++num;
				}
			}
			if (!SelectFirst || !this._techFamilyPanels.Any<KeyValuePair<int, string>>())
				return;
			this.SelectFamily(this._techFamilyPanels.First<KeyValuePair<int, string>>().Key);
		}

		private void SelectFamily(int familypanelid)
		{
			this.SetSyncedTechFamily(this._FamilyID[familypanelid]);
			foreach (int key in this._techFamilyPanels.Keys)
				this.UI.SetVisible(this.UI.Path(this._techFamilyPanels[key], "selected"), (key == familypanelid ? 1 : 0) != 0);
		}

		protected void SetSyncedTechFamily(TechFamily family)
		{
			this._game.UI.ClearItems(this.RightContentList);
			this._TechToggles.Clear();
			this._TechPanels.Clear();
			foreach (PlayerTechInfo playerTechInfo in this._game.GameDatabase.GetPlayerTechInfos(this._game.LocalPlayer.ID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Tech>((Func<Tech, bool>)(j => j.Id == x.TechFileID)).Family == family.Id)).ToList<PlayerTechInfo>())
			{
				PlayerTechInfo tech = playerTechInfo;
				if (this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Tech>((Func<Tech, bool>)(x => x.Id == tech.TechFileID)).Allows.Any<Allowable>((Func<Allowable, bool>)(x => (double)x.GetFactionProbabilityPercentage(this._game.LocalPlayer.Faction.Name.ToLower()) > 0.0)))
				{
					this._game.UI.AddItem(this.RightContentList, "", tech.TechID, "", "TechCard_Toggle");
					string itemGlobalId = this._game.UI.GetItemGlobalID(this.RightContentList, "", tech.TechID, "");
					this._TechPanels.Add(tech.TechFileID, itemGlobalId);
					this._game.UI.SetChecked(this.UI.Path(itemGlobalId, "techtoggle"), (!this._CheckedTechs.Any<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>)(x => x.Key == tech.TechFileID)) ? 0 : (this._CheckedTechs[tech.TechFileID] ? 1 : 0)) != 0);
					this._game.UI.SetVisible(this.UI.Path(itemGlobalId, "contentSelected"), (!this._CheckedTechs.Any<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>)(x => x.Key == tech.TechFileID)) ? 0 : (this._CheckedTechs[tech.TechFileID] ? 1 : 0)) != 0);
					this._game.UI.SetPropertyString(this.UI.Path(itemGlobalId, "techtoggle"), "id", "techtoggle|" + this._FamilyID.FirstOrDefault<KeyValuePair<int, TechFamily>>((Func<KeyValuePair<int, TechFamily>, bool>)(x => x.Value == family)).Key.ToString() + "|" + tech.TechFileID);
					string globalId = this._game.UI.GetGlobalID(this.UI.Path(itemGlobalId, "techtoggle|" + this._FamilyID.FirstOrDefault<KeyValuePair<int, TechFamily>>((Func<KeyValuePair<int, TechFamily>, bool>)(x => x.Value == family)).Key.ToString() + "|" + tech.TechFileID));
					this._TechToggles.Add(tech.TechFileID, globalId);
					this._game.UI.SetText(this._game.UI.Path(itemGlobalId, "techLabel"), this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Tech>((Func<Tech, bool>)(x => x.Id == tech.TechFileID)).Name);
					this._game.UI.SetPropertyString(this._game.UI.Path(itemGlobalId, "icon"), "sprite", Path.GetFileNameWithoutExtension(this._game.AssetDatabase.MasterTechTree.Technologies.FirstOrDefault<Tech>((Func<Tech, bool>)(x => x.Id == tech.TechFileID)).Icon));
				}
			}
		}

		private void SetChecked(int treeid, string techid, bool forcechecked = false)
		{
			foreach (string index in this._TechToggles.Keys.ToList<string>())
			{
				this._CheckedTechs[index] = index == techid;
				this._game.UI.SetChecked(this._TechToggles[index], index == techid);
				this._game.UI.SetVisible(this.UI.Path(this._TechPanels[index], "contentSelected"), (index == techid ? 1 : 0) != 0);
			}
			if (!this._CheckedTechs.Keys.Contains<string>(techid))
				this._CheckedTechs.Add(techid, true);
			foreach (int key in this._techFamilyPanels.Keys)
				this._game.UI.SetVisible(this._game.UI.Path(this._techFamilyPanels[key], "contentSelected"), (key == treeid ? 1 : 0) != 0);
			this._app.UI.SetEnabled(this.UI.Path(this.ID, "okbtn"), (this._CheckedTechs.Any<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>)(x => x.Value)) ? 1 : 0) != 0);
		}

		private void AutoSelect()
		{
			Random random = new Random();
			TechFamily fam = this._FamilyID.Values.ToArray<TechFamily>()[random.Next(0, this._FamilyID.Values.Count)];
			this.SetSyncedTechFamily(fam);
			this.SetChecked(this._FamilyID.FirstOrDefault<KeyValuePair<int, TechFamily>>((Func<KeyValuePair<int, TechFamily>, bool>)(x => x.Value == fam)).Key, this._TechPanels.Keys.ToArray<string>()[random.Next(0, this._TechPanels.Keys.Count)], false);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName.StartsWith(this.SystemButton))
					this.SelectFamily(int.Parse(panelName.Split('|')[1]));
				else if (panelName.StartsWith(this.ToggleButton))
				{
					string[] strArray = panelName.Split('|');
					this.SetChecked(int.Parse(strArray[1]), strArray[2], false);
				}
				else if (panelName == "autoselectbtn")
					this.AutoSelect();
				else if (panelName == "okbtn")
				{
					this.DeployCounterIntel();
					this._game.UI.CloseDialog((Dialog)this, true);
				}
			}
			int num = msgType == "list_sel_changed" ? 1 : 0;
		}

		private void DeployCounterIntel()
		{
			foreach (CounterIntelResponse counterIntelResponse in this._game.GameDatabase.GetCounterIntelResponses(this.intelMissionID).ToList<CounterIntelResponse>())
				this._app.GameDatabase.RemoveCounterIntelResponse(counterIntelResponse.ID);
			this._app.GameDatabase.InsertCounterIntelResponse(this.intelMissionID, false, this._CheckedTechs.First<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>)(x => x.Value)).Key);
		}

		protected override void OnUpdate()
		{
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
