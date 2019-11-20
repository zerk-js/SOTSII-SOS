// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogLoaFleetSelector
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DialogLoaFleetSelector : Dialog
	{
		private static string UIProfileList = "gameProfileList";
		private static string UIShipList = "gameWorkingFleet";
		private static string UICreateCompositionBtn = "createButton";
		private static string SelectCompositionBtn = "okButton";
		private static string CancelBtn = "cancelButton";
		private static string UIBRAmount = "brAmount";
		private static string UICRAmount = "crAmount";
		private static string UIDNAmount = "dnAmount";
		private static string UILVAmount = "lvAmount";
		private static string UICPAmount = "cpAmount";
		private static string UIConstrictionPoints = "constructionAmount";
		private static int designlistid = 0;
		private int CompoToDelete = -1;
		private int? selectedcompo = new int?();
		private string CreateCompoDialog;
		private string ConfirmDeleteCompo;
		private bool _forceSelection;
		private MissionType _mission;
		private FleetInfo _basefleet;

		public DialogLoaFleetSelector(
		  App app,
		  MissionType mission,
		  FleetInfo basefleet,
		  bool ForceSelection = false)
		  : base(app, "dialogLoaFleetSelector")
		{
			this._mission = mission;
			this._basefleet = basefleet;
			this._forceSelection = ForceSelection;
		}

		public override void Initialize()
		{
			this.SyncDesignList();
			this._app.UI.SetEnabled(this._app.UI.Path(this.ID, DialogLoaFleetSelector.CancelBtn), (!this._forceSelection ? 1 : 0) != 0);
		}

		protected void SyncDesignList()
		{
			List<LoaFleetComposition> list = this._app.GameDatabase.GetLoaFleetCompositions().Where<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.PlayerID == this._app.LocalPlayer.ID)).ToList<LoaFleetComposition>();
			if (this._mission == MissionType.CONSTRUCT_STN || this._mission == MissionType.UPGRADE_STN || this._mission == MissionType.SPECIAL_CONSTRUCT_STN)
				list = list.Where<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.designs.Any<LoaFleetShipDef>((Func<LoaFleetShipDef, bool>)(j => ((IEnumerable<DesignSectionInfo>)this._app.GameDatabase.GetDesignInfo(j.DesignID).DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(i => i.ShipSectionAsset.ConstructionPoints > 0)))))).ToList<LoaFleetComposition>();
			if (this._mission == MissionType.COLONIZATION || this._mission == MissionType.SUPPORT || this._mission == MissionType.EVACUATE)
				list = list.Where<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.designs.Any<LoaFleetShipDef>((Func<LoaFleetShipDef, bool>)(j => ((IEnumerable<DesignSectionInfo>)this._app.GameDatabase.GetDesignInfo(j.DesignID).DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(i => i.ShipSectionAsset.ColonizationSpace > 0)))))).ToList<LoaFleetComposition>();
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UIProfileList));
			bool flag = false;
			foreach (LoaFleetComposition fleetComposition in list)
			{
				this._app.UI.AddItem(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UIProfileList), "", fleetComposition.ID, fleetComposition.Name);
				string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UIProfileList), "", fleetComposition.ID, "");
				if (!flag)
				{
					this._app.UI.SetSelection(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UIProfileList), fleetComposition.ID);
					flag = true;
				}
				this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "designName"), fleetComposition.Name);
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "designDeleteButton"), true);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "designDeleteButton"), "id", "designDeleteButton|" + fleetComposition.ID.ToString());
				int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, this._basefleet.ID);
				int num = 0;
				foreach (LoaFleetShipDef design in fleetComposition.designs)
					num += this._app.GameDatabase.GetDesignInfo(design.DesignID).GetPlayerProductionCost(this._app.GameDatabase, fleetComposition.PlayerID, false, new float?());
				this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId, "designName"), "color", num <= fleetLoaCubeValue ? new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue) : new Vector3((float)byte.MaxValue, 0.0f, 0.0f));
			}
		}

		protected void SyncCompoShips(int Compositionid)
		{
			//DialogLoaFleetSelector.<> c__DisplayClass13 CS$<> 8__locals1 = new DialogLoaFleetSelector.<> c__DisplayClass13();
			LoaFleetComposition loaFleetComposition = this._app.GameDatabase.GetLoaFleetCompositions().FirstOrDefault((LoaFleetComposition x) => x.ID == Compositionid);
			if (loaFleetComposition != null)
			{
				this.selectedcompo = new int?(Compositionid);
				List<DesignInfo> list = Kerberos.Sots.StarFleet.StarFleet.GetDesignBuildOrderForComposition(this._app.Game, this._basefleet.ID, loaFleetComposition, this._mission).ToList<DesignInfo>();
				int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, this._basefleet.ID);
				int num = 0;
				List<DesignInfo> list2 = (from X in list
										  where X.Class == ShipClass.BattleRider
										  select X).ToList<DesignInfo>();
				this._app.UI.ClearItems(this._app.UI.Path(new string[]
				{
					base.ID,
					DialogLoaFleetSelector.UIShipList
				}));
				foreach (DesignInfo designInfo in list)
				{
					if (designInfo.Class != ShipClass.BattleRider && !(designInfo.GetRealShipClass() == RealShipClasses.BoardingPod) && !(designInfo.GetRealShipClass() == RealShipClasses.Drone) && !(designInfo.GetRealShipClass() == RealShipClasses.EscapePod))
					{
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							base.ID,
							DialogLoaFleetSelector.UIShipList
						}), "", DialogLoaFleetSelector.designlistid, designInfo.Name);
						string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							base.ID,
							DialogLoaFleetSelector.UIShipList
						}), "", DialogLoaFleetSelector.designlistid, "");
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"designName"
						}), designInfo.Name);
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"designDeleteButton"
						}), false);
						if (num + designInfo.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null) <= fleetLoaCubeValue)
						{
							this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
							{
								itemGlobalID,
								"designName"
							}), "color", new Vector3(255f, 255f, 255f));
							num += designInfo.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null);
						}
						else
						{
							this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
							{
								itemGlobalID,
								"designName"
							}), "color", new Vector3(255f, 0f, 0f));
						}
						DialogLoaFleetSelector.designlistid++;
						List<CarrierWingData> list3 = RiderManager.GetDesignBattleriderWingData(this._app, designInfo).ToList<CarrierWingData>();
						using (List<CarrierWingData>.Enumerator enumerator2 = list3.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								//DialogLoaFleetSelector.<> c__DisplayClass16 CS$<> 8__locals2 = new DialogLoaFleetSelector.<> c__DisplayClass16();
								//CS$<> 8__locals2.CS$<> 8__locals14 = CS$<> 8__locals1;
								CarrierWingData wd = enumerator2.Current;
								List<DesignInfo> classriders = (from x in list2
																where StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
																select x).ToList<DesignInfo>();
								if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
								{
									BattleRiderTypes SelectedType = (from x in classriders
																	 where classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count
																	 select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
									DesignInfo designInfo2 = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
									foreach (int num2 in wd.SlotIndexes)
									{
										if (designInfo2 != null)
										{
											this._app.UI.AddItem(this._app.UI.Path(new string[]
											{
												base.ID,
												DialogLoaFleetSelector.UIShipList
											}), "", DialogLoaFleetSelector.designlistid, designInfo2.Name);
											itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
											{
												base.ID,
												DialogLoaFleetSelector.UIShipList
											}), "", DialogLoaFleetSelector.designlistid, "");
											this._app.UI.SetText(this._app.UI.Path(new string[]
											{
												itemGlobalID,
												"designName"
											}), designInfo2.Name);
											this._app.UI.SetVisible(this._app.UI.Path(new string[]
											{
												itemGlobalID,
												"designDeleteButton"
											}), false);
											if (num + designInfo2.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null) <= fleetLoaCubeValue)
											{
												this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
												{
													itemGlobalID,
													"designName"
												}), "color", new Vector3(255f, 255f, 255f));
												num += designInfo2.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, null);
											}
											else
											{
												this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
												{
													itemGlobalID,
													"designName"
												}), "color", new Vector3(255f, 0f, 0f));
											}
											list2.Remove(designInfo2);
											DialogLoaFleetSelector.designlistid++;
										}
									}
								}
							}
						}
					}
				}
				this.SyncCompoInfo(loaFleetComposition);
			}
		}
		protected void SyncCompoInfo(LoaFleetComposition composition)
		{
			List<DesignInfo> source = new List<DesignInfo>();
			foreach (int designID in composition.designs.Select<LoaFleetShipDef, int>((Func<LoaFleetShipDef, int>)(x => x.DesignID)))
				source.Add(this._app.GameDatabase.GetDesignInfo(designID));
			int num1 = 0;
			int num2 = 0;
			foreach (DesignInfo designInfo in source)
			{
				RealShipClasses? realShipClass1 = designInfo.GetRealShipClass();
				if ((realShipClass1.GetValueOrDefault() != RealShipClasses.BoardingPod ? 0 : (realShipClass1.HasValue ? 1 : 0)) == 0)
				{
					RealShipClasses? realShipClass2 = designInfo.GetRealShipClass();
					if ((realShipClass2.GetValueOrDefault() != RealShipClasses.Drone ? 0 : (realShipClass2.HasValue ? 1 : 0)) == 0)
					{
						RealShipClasses? realShipClass3 = designInfo.GetRealShipClass();
						if ((realShipClass3.GetValueOrDefault() != RealShipClasses.EscapePod ? 0 : (realShipClass3.HasValue ? 1 : 0)) == 0)
						{
							foreach (DesignSectionInfo designSection in designInfo.DesignSections)
								num1 += RiderManager.GetNumRiderSlots(this._app, designSection);
							if (designInfo.Class == ShipClass.BattleRider)
								++num2;
						}
					}
				}
			}
			this._app.UI.SetText(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UIBRAmount), num2.ToString() + "/" + num1.ToString());
			this._app.UI.SetText(this._app.UI.Path(this.ID, "totalShipsAmount"), source.Count<DesignInfo>().ToString());
			this._app.UI.SetText(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UICRAmount), source.Count<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class == ShipClass.Cruiser)).ToString());
			this._app.UI.SetText(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UIDNAmount), source.Count<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class == ShipClass.Dreadnought)).ToString());
			this._app.UI.SetText(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UILVAmount), source.Count<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class == ShipClass.Leviathan)).ToString());
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			bool flag = false;
			foreach (DesignInfo designInfo in source)
			{
				num4 += designInfo.CommandPointCost;
				if (designInfo.GetCommandPoints() > 0 && !flag)
				{
					num5 += this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, designInfo.ID);
					flag = true;
				}
				num3 += designInfo.GetPlayerProductionCost(this._app.GameDatabase, this._basefleet.PlayerID, false, new float?());
			}
			this._app.UI.SetText(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UICPAmount), num4.ToString("N0") + "/" + num5.ToString("N0"));
			this._app.UI.SetText(this._app.UI.Path(this.ID, DialogLoaFleetSelector.UIConstrictionPoints), num3.ToString("N0") + "/" + Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, this._basefleet.ID).ToString("N0"));
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_item_dblclk")
				return;
			if (msgType == "list_sel_changed")
			{
				if (!(panelName == DialogLoaFleetSelector.UIProfileList))
					return;
				this.SyncCompoShips(int.Parse(msgParams[0]));
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == DialogLoaFleetSelector.UICreateCompositionBtn)
					this.CreateCompoDialog = this._app.UI.CreateDialog((Dialog)new DialogLoaFleetCompositor(this._app, this._mission), null);
				else if (panelName == DialogLoaFleetSelector.SelectCompositionBtn)
				{
					if (!this.selectedcompo.HasValue)
						return;
					if (this._forceSelection)
					{
						this._app.GameDatabase.UpdateFleetCompositionID(this._basefleet.ID, this.selectedcompo);
						Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromComposition(this._app.Game, this._basefleet.ID, MissionType.NO_MISSION);
					}
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else if (panelName == DialogLoaFleetSelector.CancelBtn)
				{
					this.selectedcompo = new int?();
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					if (!panelName.StartsWith("designDeleteButton"))
						return;
					int compid = int.Parse(panelName.Split('|')[1]);
					LoaFleetComposition fleetComposition = this._app.GameDatabase.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.ID == compid));
					if (fleetComposition == null)
						return;
					this.CompoToDelete = compid;
					this.ConfirmDeleteCompo = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@UI_LOACOMP_CONFIRM_DELETE_TITLE"), string.Format(App.Localize("@UI_LOACOMP_CONFIRM_DELETE_MSG"), (object)fleetComposition.Name), "dialogGenericQuestion"), null);
				}
			}
			else
			{
				if (!(msgType == "dialog_closed"))
					return;
				if (panelName == this.CreateCompoDialog)
				{
					this.SyncDesignList();
				}
				else
				{
					if (!(panelName == this.ConfirmDeleteCompo))
						return;
					if (bool.Parse(msgParams[0]))
					{
						this._app.GameDatabase.DeleteLoaFleetCompositon(this.CompoToDelete);
						this.SyncDesignList();
					}
					this.CompoToDelete = -1;
					this.ConfirmDeleteCompo = null;
				}
			}
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this.selectedcompo.ToString() };
		}
	}
}
