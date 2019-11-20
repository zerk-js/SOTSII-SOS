// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SpecialProjectDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class SpecialProjectDialog : Dialog
	{
		private static readonly string UIGovernmentResearchSlider = "research_sliderD";
		private static readonly string UICurrentProjectSlider = "currentProjectSliderD";
		private static readonly string UISpecialProjectSlider = "specialProjectSliderD";
		private static readonly string UISalvageResearchSlider = "salvageResearchSliderD";
		private List<SpecialProjectInfo> _rates = new List<SpecialProjectInfo>();
		private SpecialProjectInfo _contextProject;
		private string _confirmProjectChangeDialog;
		private int _researchPoints;
		private int _specialPoints;
		private BudgetPiechart _piechart;
		private BudgetPiechart _behindPiechart;

		public SpecialProjectDialog(App game, string template = "dialogSpecialProjects")
		  : base(game, template)
		{
		}

		public override void Initialize()
		{
			this._app.UI.UnlockUI();
			this._piechart = new BudgetPiechart(this._app.UI, this._app.UI.Path(this.ID, "pnlScreenLeft", "piechartD"), this._app.AssetDatabase);
			this._behindPiechart = new BudgetPiechart(this._app.UI, "piechart", this._app.AssetDatabase);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "specialProjectsHeader"), "text", App.Localize("@UI_RESEARCH_CONTEXT_SPECIAL"));
			this.RefreshProjects();
			EmpireBarUI.SyncResearchSlider(this._app, SpecialProjectDialog.UIGovernmentResearchSlider, this._piechart);
		}

		private void SyncRates()
		{
			foreach (SpecialProjectInfo rate in this._rates)
				this._app.GameDatabase.UpdateSpecialProjectRate(rate.ID, rate.Rate);
		}

		private void RefreshProjects()
		{
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, "specialList"));
			this.SyncRates();
			this._rates.Clear();
			List<SpecialProjectInfo> list = this._app.GameDatabase.GetSpecialProjectInfosByPlayerID(this._app.LocalPlayer.ID, true).Where<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x => x.Type != SpecialProjectType.Salvage)).ToList<SpecialProjectInfo>().OrderBy<SpecialProjectInfo, bool>((Func<SpecialProjectInfo, bool>)(x => x.Progress < 0)).ToList<SpecialProjectInfo>();
			if ((double)list.Sum<SpecialProjectInfo>((Func<SpecialProjectInfo, float>)(x => x.Rate)) < 0.99)
			{
				SpecialProjectInfo specialProjectInfo = list.FirstOrDefault<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x => x.Progress >= 0));
				if (specialProjectInfo != null)
					specialProjectInfo.Rate = 1f;
			}
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
			this._researchPoints = this._app.Game.ConvertToResearchPoints(this._app.LocalPlayer.ID, Budget.GenerateBudget(this._app.Game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Actual).ResearchSpending.RequestedTotal);
			this._specialPoints = (int)((double)this._researchPoints * (double)playerInfo.RateResearchSpecialProject);
			foreach (SpecialProjectInfo specialProjectInfo in list)
			{
				this._app.UI.AddItem(this._app.UI.Path(this.ID, "specialList"), "", specialProjectInfo.ID, "");
				string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "specialList"), "", specialProjectInfo.ID, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "projectName"), "text", specialProjectInfo.Name);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "description"), "text", GameSession.GetSpecialProjectDescription(specialProjectInfo.Type));
				if (specialProjectInfo.Progress >= 0)
				{
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "startButton"), false);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "cancelButton"), true);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "activeIndicator"), true);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "projectRate"), true);
				}
				else
				{
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "startButton"), true);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "cancelButton"), false);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "activeIndicator"), false);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "projectRate"), false);
				}
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "startButton"), "id", "startButton|" + specialProjectInfo.ID.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "cancelButton"), "id", "cancelButton|" + specialProjectInfo.ID.ToString());
				this._app.UI.SetSliderValue(this._app.UI.Path(itemGlobalId, "projectProgress"), (int)((double)specialProjectInfo.Progress / (double)specialProjectInfo.Cost * 100.0));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "projectRate"), "id", "projectRate|" + specialProjectInfo.ID.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "projectTurnCount"), "id", "projectTurnCount|" + specialProjectInfo.ID.ToString());
				this._rates.Add(specialProjectInfo);
			}
			this.RefreshSliders();
			this.UpdateResearchSliders(playerInfo, "");
		}

		private void SetRate(int id, float value)
		{
			SpecialProjectInfo specialProjectInfo1 = this._rates.FirstOrDefault<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x => x.ID == id));
			List<SpecialProjectInfo> list = this._rates.Where<SpecialProjectInfo>((Func<SpecialProjectInfo, bool>)(x => x.Progress >= 0)).ToList<SpecialProjectInfo>();
			if (specialProjectInfo1 != null)
			{
				float num1 = specialProjectInfo1.Rate - value;
				float num2 = 0.0f;
				float num3 = this._rates.Sum<SpecialProjectInfo>((Func<SpecialProjectInfo, float>)(x => x.Rate));
				if (this._rates.Count > 1 && (double)num3 >= 0.999)
				{
					int num4 = 100;
					do
					{
						--num4;
						foreach (SpecialProjectInfo specialProjectInfo2 in list)
						{
							if (specialProjectInfo2.ID != id)
							{
								float num5 = Math.Abs(num1 / (float)(this._rates.Count - 1));
								if ((double)num1 < 0.0)
								{
									if ((double)specialProjectInfo2.Rate - (double)num5 > 0.0)
									{
										specialProjectInfo2.Rate -= num5;
									}
									else
									{
										num5 = specialProjectInfo2.Rate;
										specialProjectInfo2.Rate = 0.0f;
									}
									num2 += num5;
									if ((double)num2 >= (double)Math.Abs(num1))
									{
										specialProjectInfo2.Rate += num2 - Math.Abs(num1);
										num2 = Math.Abs(num1);
										break;
									}
								}
								else
								{
									if ((double)specialProjectInfo2.Rate + (double)num5 < 1.0)
									{
										specialProjectInfo2.Rate += num5;
									}
									else
									{
										num5 = 1f - specialProjectInfo2.Rate;
										specialProjectInfo2.Rate = 1f;
									}
									num2 += num5;
									if ((double)num2 >= (double)num1)
									{
										specialProjectInfo2.Rate -= num2 - num1;
										num2 = num1;
										break;
									}
								}
							}
						}
					}
					while ((double)num2 < (double)Math.Abs(num1) - 9.99999974737875E-05 && num4 > 0);
					specialProjectInfo1.Rate -= num1;
				}
				else
					specialProjectInfo1.Rate = 1f;
			}
			this.RefreshSliders();
		}

		private void RefreshSliders()
		{
			foreach (SpecialProjectInfo rate in this._rates)
			{
				this._app.UI.SetSliderValue("projectRate|" + rate.ID.ToString(), (int)((double)rate.Rate * 100.0));
				int num1 = rate.Cost - rate.Progress;
				int num2 = this._specialPoints > 0 ? (int)((double)num1 / ((double)rate.Rate * (double)this._specialPoints)) : 10000;
				if (rate.Progress == -1)
					num2 = this._specialPoints > 0 ? num1 / this._specialPoints : 10000;
				++num2;
				if (num2 < 0)
					num2 = 10000;
				if (num2 < 5000)
					this._app.UI.SetPropertyString("projectTurnCount|" + rate.ID.ToString(), "text", num2.ToString() + " Turns");
				else
					this._app.UI.SetPropertyString("projectTurnCount|" + rate.ID.ToString(), "text", "∞ Turns");
				if (rate.Progress >= 0)
				{
					if (num2 > 100)
						num2 = 100;
					float num3 = (float)num2 / 100f;
					this._app.UI.SetPropertyColorNormalized("projectTurnCount|" + rate.ID.ToString(), "color", new Vector3(Math.Min(num3 * (num3 + 1f), 1f), Math.Min((float)((1.0 - (double)num3) * ((double)num3 + 1.0)), 1f), 0.0f));
				}
				else
					this._app.UI.SetPropertyColorNormalized("projectTurnCount|" + rate.ID.ToString(), "color", new Vector3(0.8f, 0.8f, 0.8f));
			}
		}

		private void UpdateResearchSliders(PlayerInfo playerInfo, string iChanged)
		{
			double num1 = (1.0 - (double)playerInfo.RateGovernmentResearch) * 100.0;
			double num2 = (double)playerInfo.RateResearchCurrentProject * 100.0;
			double num3 = (double)playerInfo.RateResearchSpecialProject * 100.0;
			double num4 = (double)playerInfo.RateResearchSalvageResearch * 100.0;
			if (iChanged != SpecialProjectDialog.UIGovernmentResearchSlider)
				this._app.UI.SetSliderValue(SpecialProjectDialog.UIGovernmentResearchSlider, (int)num1);
			if (iChanged != SpecialProjectDialog.UICurrentProjectSlider)
				this._app.UI.SetSliderValue(SpecialProjectDialog.UICurrentProjectSlider, (int)num2);
			if (iChanged != SpecialProjectDialog.UISpecialProjectSlider)
				this._app.UI.SetSliderValue(SpecialProjectDialog.UISpecialProjectSlider, (int)num3);
			if (iChanged != SpecialProjectDialog.UISalvageResearchSlider)
				this._app.UI.SetSliderValue(SpecialProjectDialog.UISalvageResearchSlider, (int)num4);
			this._app.UI.SetSliderValue("research_slider", (int)num1);
			this._app.UI.SetSliderValue("gameEmpireResearchSlider", (int)num1);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
					this._app.UI.CloseDialog((Dialog)this, true);
				else if (panelName.Contains("startButton"))
				{
					SpecialProjectInfo specialProjectInfo = this._app.GameDatabase.GetSpecialProjectInfo(int.Parse(panelName.Split('|')[1]));
					if (specialProjectInfo == null || specialProjectInfo.Progress != -1)
						return;
					this._contextProject = specialProjectInfo;
					this._confirmProjectChangeDialog = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@UI_SPECIAL_RESEARCH_START_TITLE"), App.Localize("@UI_SPECIAL_RESEARCH_START_DESC"), "dialogGenericQuestion"), null);
				}
				else
				{
					if (!panelName.Contains("cancelButton"))
						return;
					SpecialProjectInfo specialProjectInfo = this._app.GameDatabase.GetSpecialProjectInfo(int.Parse(panelName.Split('|')[1]));
					if (specialProjectInfo == null || specialProjectInfo.Progress <= -1)
						return;
					this._contextProject = specialProjectInfo;
					this._confirmProjectChangeDialog = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@UI_SPECIAL_RESEARCH_CANCEL_TITLE"), App.Localize("@UI_SPECIAL_RESEARCH_CANCEL_DESC"), "dialogGenericQuestion"), null);
				}
			}
			else if (msgType == "slider_value")
			{
				if (panelName.Contains("projectRate"))
				{
					this.SetRate(int.Parse(panelName.Split('|')[1]), (float)int.Parse(msgParams[0]) / 100f);
				}
				else
				{
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
					if (panelName == SpecialProjectDialog.UIGovernmentResearchSlider)
					{
						float num = ((float)int.Parse(msgParams[0]) / 100f).Clamp(0.0f, 1f);
						playerInfo.RateGovernmentResearch = 1f - num;
						if (this._app.GameDatabase.GetSliderNotchSettingInfo(playerInfo.ID, UISlidertype.SecuritySlider) != null)
							EmpireSummaryState.DistributeGovernmentSpending(this._app.Game, EmpireSummaryState.GovernmentSpendings.Security, (float)Math.Min((double)Budget.GenerateBudget(this._app.Game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic).RequiredSecurity / 100.0, 1.0), playerInfo);
						else
							this._app.GameDatabase.UpdatePlayerSliders(this._app.Game, playerInfo);
						this._researchPoints = this._app.Game.ConvertToResearchPoints(this._app.LocalPlayer.ID, Budget.GenerateBudget(this._app.Game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Actual).ResearchSpending.RequestedTotal);
						this._specialPoints = (int)((double)this._researchPoints * (double)playerInfo.RateResearchSpecialProject);
						Budget budget = Budget.GenerateBudget(this._app.Game, playerInfo, (IEnumerable<DesignInfo>)null, BudgetProjection.Pessimistic);
						this._piechart.SetSlices(budget);
						this._behindPiechart.SetSlices(budget);
						this._app.UI.ShakeViolently("piechartD");
					}
					else if (panelName == SpecialProjectDialog.UICurrentProjectSlider)
					{
						EmpireSummaryState.DistibuteResearchSpending(this._app.Game, this._app.GameDatabase, EmpireSummaryState.ResearchSpendings.CurrentProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
						this._specialPoints = (int)((double)this._researchPoints * (double)playerInfo.RateResearchSpecialProject);
					}
					else if (panelName == SpecialProjectDialog.UISpecialProjectSlider)
					{
						EmpireSummaryState.DistibuteResearchSpending(this._app.Game, this._app.GameDatabase, EmpireSummaryState.ResearchSpendings.SpecialProject, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
						this._specialPoints = (int)((double)this._researchPoints * (double)playerInfo.RateResearchSpecialProject);
					}
					else if (panelName == SpecialProjectDialog.UISalvageResearchSlider)
					{
						EmpireSummaryState.DistibuteResearchSpending(this._app.Game, this._app.GameDatabase, EmpireSummaryState.ResearchSpendings.SalvageResearch, (float)int.Parse(msgParams[0]) / 100f, playerInfo);
						this._specialPoints = (int)((double)this._researchPoints * (double)playerInfo.RateResearchSpecialProject);
					}
					this.UpdateResearchSliders(playerInfo, panelName);
					this.RefreshSliders();
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || !(panelName == this._confirmProjectChangeDialog))
					return;
				if (bool.Parse(msgParams[0]) && this._contextProject != null)
				{
					if (this._contextProject.Progress == -1)
					{
						this._app.GameDatabase.UpdateSpecialProjectProgress(this._contextProject.ID, 0);
						this.RefreshProjects();
					}
					else
					{
						this._app.GameDatabase.RemoveSpecialProject(this._contextProject.ID);
						this.RefreshProjects();
					}
				}
				this._contextProject = (SpecialProjectInfo)null;
				this._confirmProjectChangeDialog = "";
			}
		}

		public override string[] CloseDialog()
		{
			this.SyncRates();
			this._piechart = (BudgetPiechart)null;
			this._behindPiechart = (BudgetPiechart)null;
			return (string[])null;
		}
	}
}
