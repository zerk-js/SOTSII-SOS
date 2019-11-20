// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ProvinceEditStarMapStateMode
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class ProvinceEditStarMapStateMode : StarMapStateMode
	{
		private List<int> ProvincePool = new List<int>();
		private List<StarSystemInfo> Systems = new List<StarSystemInfo>();
		private const string UIProvinceEditWindow = "pnlProvinceEditWindow";
		private const string UICommitButton = "btnNextStage";
		private const string UIDescriptionLabel = "lblDescription";
		private const string UITitleLabel = "lblTitle";
		private StarMap _starmap;
		private StarMapState _state;
		private ProvinceEditStarMapStateMode.ProvinceEditStage _stage;
		private string _provinceNameDialog;
		private float MaxProvinceDistance;
		private int MaxSystemsInProvince;
		private int MinSystemsInProvince;
		private List<int> SystemPool;
		private StarSystemInfo Capital;

		public ProvinceEditStarMapStateMode(GameSession sim, StarMapState state, StarMap starmap)
		  : base(sim)
		{
			this._starmap = starmap;
			this._state = state;
		}

		public override void Initialize()
		{
			this.MaxProvinceDistance = this.App.GetStratModifier<float>(StratModifiers.MaxProvincePlanetRange, this.Sim.LocalPlayer.ID);
			this.MaxSystemsInProvince = this.App.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, this.Sim.LocalPlayer.ID);
			this.MinSystemsInProvince = this.App.GetStratModifier<int>(StratModifiers.MinProvincePlanets, this.Sim.LocalPlayer.ID);
			this._starmap.Select((IGameObject)null);
			this._starmap.SelectEnabled = true;
			this.SystemPool = this.GetProvinceableSystems().ToList<int>();
			this.UpdateProvincePool();
			this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.SystemSelect;
			this.App.UI.SetText(this.App.UI.Path("pnlProvinceEditWindow", "lblDescription"), string.Format(App.Localize("@UI_PROVINCE_EDIT_SYSTEM_SELECT"), (object)(this.MinSystemsInProvince - this.Systems.Count)));
			this.App.UI.SetEnabled(this.App.UI.Path("pnlProvinceEditWindow", "btnNextStage"), false);
			this.App.UI.SetVisible("pnlProvinceEditWindow", true);
		}

		public override void Terminate()
		{
			foreach (int index in this.SystemPool)
			{
				this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)false, (object)this._starmap.Systems.Reverse[index]);
				this._starmap.PostSetProp("ProvincePoolEffect", (object)false, (object)this._starmap.Systems.Reverse[index]);
			}
			this._starmap.SelectEnabled = true;
			this.App.UI.SetVisible("pnlProvinceEditWindow", false);
		}

		public override bool OnGameObjectClicked(IGameObject obj)
		{
			if (obj == null || !typeof(StarMapSystem).IsAssignableFrom(obj.GetType()))
				return true;
			switch (this._stage)
			{
				case ProvinceEditStarMapStateMode.ProvinceEditStage.SystemSelect:
					StarSystemInfo starSystemInfo1 = this.Sim.GameDatabase.GetStarSystemInfo(this._starmap.Systems.Forward[(StarMapSystem)obj]);
					if (!this.Systems.Contains(starSystemInfo1))
					{
						if (this.ProvincePool.Contains(starSystemInfo1.ID))
						{
							this.Systems.Add(starSystemInfo1);
							this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)true, (object)obj);
							this.UpdateProvincePool();
						}
					}
					else
					{
						this.Systems.Remove(starSystemInfo1);
						this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)false, (object)obj);
						foreach (StarSystemInfo target in new List<StarSystemInfo>((IEnumerable<StarSystemInfo>)this.Systems))
						{
							if (this.Systems.Count > 1)
							{
								if (!(this.Systems.First<StarSystemInfo>() == target) && !this.isChained(new List<StarSystemInfo>()
				{
				  this.Systems.First<StarSystemInfo>()
				}, this.Systems, target))
								{
									this.Systems.Remove(target);
									this._starmap.PostSetProp("ProvinceSystemSelectEffect", (object)false, (object)this._starmap.Systems.Reverse[target.ID]);
								}
							}
							else
								break;
						}
						this.UpdateProvincePool();
					}
					this.App.UI.SetText(this.App.UI.Path("pnlProvinceEditWindow", "lblDescription"), string.Format(App.Localize("@UI_PROVINCE_EDIT_SYSTEM_SELECT"), (object)Math.Max(this.MinSystemsInProvince - this.Systems.Count, 0)));
					this.App.UI.SetEnabled(this.App.UI.Path("pnlProvinceEditWindow", "btnNextStage"), (this.Systems.Count >= this.MinSystemsInProvince ? 1 : 0) != 0);
					break;
				case ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect:
					StarSystemInfo starSystemInfo2 = this.Sim.GameDatabase.GetStarSystemInfo(this._starmap.Systems.Forward[(StarMapSystem)obj]);
					if (this.Systems.Contains(starSystemInfo2) && starSystemInfo2 != (StarSystemInfo)null && starSystemInfo2 != this.Capital)
					{
						if (this.Capital != (StarSystemInfo)null)
							this._starmap.PostSetProp("ProvinceCapitalEffect", (object)false, (object)this._starmap.Systems.Reverse[this.Capital.ID]);
						this.Capital = starSystemInfo2;
						this._starmap.PostSetProp("ProvinceCapitalEffect", (object)true, (object)obj);
					}
					this.App.UI.SetEnabled(this.App.UI.Path("pnlProvinceEditWindow", "btnNextStage"), (this.Capital != (StarSystemInfo)null ? 1 : 0) != 0);
					break;
			}
			return true;
		}

		public override bool OnGameObjectMouseOver(IGameObject obj)
		{
			return false;
		}

		public override bool OnUIButtonPressed(string panelName)
		{
			if (this._stage == ProvinceEditStarMapStateMode.ProvinceEditStage.SystemSelect && panelName == "btnNextStage")
			{
				this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect;
				this.App.UI.SetText(this.App.UI.Path("pnlProvinceEditWindow", "lblDescription"), App.Localize("@UI_PROVINCE_EDIT_CAPITAL_SELECT"));
				this.App.UI.SetEnabled(this.App.UI.Path("pnlProvinceEditWindow", "btnNextStage"), false);
				return true;
			}
			if (this._stage != ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect || !(panelName == "btnNextStage"))
				return false;
			this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.ProvinceName;
			this._provinceNameDialog = this.App.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this.App, App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_TITLE"), App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_DESC"), this.Sim.NamesPool.GetProvinceName(this.Sim.LocalPlayer.Faction.Name), 1024, 2, true, EditBoxFilterMode.None), null);
			this.App.UI.SetVisible("pnlProvinceEditWindow", false);
			return true;
		}

		public override bool OnUIDialogClosed(string panelName, string[] msgParams)
		{
			if (!(panelName == this._provinceNameDialog))
				return false;
			if (bool.Parse(msgParams[0]))
			{
				if (this.Sim.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>().Any<ProvinceInfo>((Func<ProvinceInfo, bool>)(x => x.Name == msgParams[1])))
				{
					this._provinceNameDialog = this.App.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this.App, App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_TITLE"), App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_DESC"), this.Sim.NamesPool.GetProvinceName(this.Sim.LocalPlayer.Faction.Name), 1024, 2, true, EditBoxFilterMode.None), null);
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, App.Localize("@UI_DIALOGDUPLICATEPROVINCE_TITLE"), string.Format(App.Localize("@UI_DIALOGDUPLICATEPROVINCE_DESC"), (object)msgParams[1]), "dialogGenericMessage"), null);
					return false;
				}
				this.Sim.GameDatabase.InsertProvince(msgParams[1], this.Sim.LocalPlayer.ID, this.Systems.Select<StarSystemInfo, int>((Func<StarSystemInfo, int>)(x => x.ID)), this.Capital.ID);
				GameSession.ApplyMoralEvent(this.App, MoralEvent.ME_PROVINCE_FORMED, this.Sim.LocalPlayer.ID, new int?(), new int?(), new int?());
				this._starmap.Sync(this._state.GetCrits());
				this._state.SetProvinceMode(false);
			}
			else
			{
				this.App.UI.SetVisible("pnlProvinceEditWindow", true);
				this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect;
			}
			return true;
		}

		private static bool IsSystemProvinceable(GameSession sim, int SystemId)
		{
			if (sim.GameDatabase.GetStarSystemProvinceID(SystemId).HasValue)
				return false;
			foreach (int orbitalObjectID in sim.GameDatabase.GetStarSystemOrbitalObjectIDs(SystemId).ToList<int>())
			{
				ColonyInfo colonyInfoForPlanet = sim.GameDatabase.GetColonyInfoForPlanet(orbitalObjectID);
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == sim.LocalPlayer.ID && Colony.IsColonySelfSufficient(sim, colonyInfoForPlanet, sim.GameDatabase.GetPlanetInfo(orbitalObjectID)))
					return true;
			}
			return false;
		}

		private bool isChained(
		  List<StarSystemInfo> curChain,
		  List<StarSystemInfo> availableSystems,
		  StarSystemInfo target)
		{
			List<StarSystemInfo> systemsInRange = new List<StarSystemInfo>();
			foreach (StarSystemInfo starSystemInfo in curChain)
			{
				StarSystemInfo ssi = starSystemInfo;
				systemsInRange.AddRange(availableSystems.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x =>
			   {
				   if (!systemsInRange.Contains(x) && !curChain.Contains(x))
					   return (double)(x.Origin - ssi.Origin).Length <= (double)this.MaxProvinceDistance;
				   return false;
			   })));
			}
			if (systemsInRange.Count<StarSystemInfo>() == 0)
				return false;
			if (systemsInRange.Contains(target))
				return true;
			foreach (StarSystemInfo starSystemInfo in systemsInRange)
			{
				if (this.isChained(new List<StarSystemInfo>((IEnumerable<StarSystemInfo>)curChain)
		{
		  starSystemInfo
		}, availableSystems, target))
					return true;
			}
			return false;
		}

		private IEnumerable<int> GetProvinceableSystems()
		{
			return (IEnumerable<int>)this.Sim.GameDatabase.GetPlayerColonySystemIDs(this.Sim.LocalPlayer.ID).ToList<int>().Where<int>((Func<int, bool>)(x => ProvinceEditStarMapStateMode.IsSystemProvinceable(this.Sim, x))).ToList<int>();
		}

		private void UpdateProvincePool()
		{
			this.ProvincePool.Clear();
			foreach (int systemId in this.SystemPool)
			{
				if (this.Systems.Count <= 0)
				{
					this._starmap.PostSetProp("ProvincePoolEffect", (object)true, (object)this._starmap.Systems.Reverse[systemId]);
					this.ProvincePool.Add(systemId);
				}
				else if (this.Systems.Count >= this.MaxSystemsInProvince)
				{
					this._starmap.PostSetProp("ProvincePoolEffect", (object)false, (object)this._starmap.Systems.Reverse[systemId]);
					this.ProvincePool.Remove(systemId);
				}
				else
				{
					StarSystemInfo ssi = this.Sim.GameDatabase.GetStarSystemInfo(systemId);
					if (!this.Systems.Contains(ssi) && this.Systems.Any<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => (double)(x.Origin - ssi.Origin).Length <= (double)this.MaxProvinceDistance)))
					{
						this._starmap.PostSetProp("ProvincePoolEffect", (object)true, (object)this._starmap.Systems.Reverse[systemId]);
						this.ProvincePool.Add(systemId);
					}
					else
					{
						this._starmap.PostSetProp("ProvincePoolEffect", (object)false, (object)this._starmap.Systems.Reverse[systemId]);
						this.ProvincePool.Remove(systemId);
					}
				}
			}
		}

		private enum ProvinceEditStage
		{
			SystemSelect,
			CapitalSelect,
			ProvinceName,
		}
	}
}
