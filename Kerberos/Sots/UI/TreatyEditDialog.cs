// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.TreatyEditDialog
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
	internal class TreatyEditDialog : Dialog
	{
		public static Dictionary<LimitationTreatyType, SpinnerValueDescriptor> LimitationTypeSpinnerDescriptors = new Dictionary<LimitationTreatyType, SpinnerValueDescriptor>()
	{
	  {
		LimitationTreatyType.FleetSize,
		new SpinnerValueDescriptor()
		{
		  min = 1.0,
		  max = 100.0,
		  rateOfChange = 1.0
		}
	  },
	  {
		LimitationTreatyType.ShipClass,
		new SpinnerValueDescriptor()
		{
		  min = 1.0,
		  max = 100.0,
		  rateOfChange = 1.0
		}
	  },
	  {
		LimitationTreatyType.EmpireSize,
		new SpinnerValueDescriptor()
		{
		  min = 1.0,
		  max = 100.0,
		  rateOfChange = 1.0
		}
	  },
	  {
		LimitationTreatyType.ForgeGemWorlds,
		new SpinnerValueDescriptor()
		{
		  min = 1.0,
		  max = 100.0,
		  rateOfChange = 1.0
		}
	  },
	  {
		LimitationTreatyType.StationType,
		new SpinnerValueDescriptor()
		{
		  min = 1.0,
		  max = 100.0,
		  rateOfChange = 1.0
		}
	  }
	};
		public static List<LimitationTreatyType> EnableGroupList = new List<LimitationTreatyType>()
	{
	  LimitationTreatyType.ShipClass,
	  LimitationTreatyType.Weapon,
	  LimitationTreatyType.ResearchTree,
	  LimitationTreatyType.ResearchTech,
	  LimitationTreatyType.StationType
	};
		public static List<LimitationTreatyType> EnableValueList = new List<LimitationTreatyType>()
	{
	  LimitationTreatyType.FleetSize,
	  LimitationTreatyType.ShipClass,
	  LimitationTreatyType.EmpireSize,
	  LimitationTreatyType.ForgeGemWorlds,
	  LimitationTreatyType.StationType
	};
		public static Dictionary<ShipClass, string> ShipClassLimitationGroups = new Dictionary<ShipClass, string>()
	{
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
		public static Dictionary<StationType, string> StationTypeLimitationGroups = new Dictionary<StationType, string>()
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
		public static Dictionary<TreatyType, string> TreatyTypeLocMap = new Dictionary<TreatyType, string>()
	{
	  {
		TreatyType.Armistice,
		"@UI_TREATY_ARMISTICE"
	  },
	  {
		TreatyType.Trade,
		"@UI_TREATY_TRADE"
	  },
	  {
		TreatyType.Limitation,
		"@UI_TREATY_LIMITATION"
	  },
	  {
		TreatyType.Incorporate,
		"@UI_TREATY_INCORPORATE"
	  },
	  {
		TreatyType.Protectorate,
		"@UI_TREATY_PROTECTORATE"
	  }
	};
		public static Dictionary<LimitationTreatyType, string> LimitationTreatyTypeLocMap = new Dictionary<LimitationTreatyType, string>()
	{
	  {
		LimitationTreatyType.EmpireSize,
		"@UI_TREATY_LIMITATION_EMPIRESIZE"
	  },
	  {
		LimitationTreatyType.FleetSize,
		"@UI_TREATY_LIMITATION_FLEETSIZE"
	  },
	  {
		LimitationTreatyType.ForgeGemWorlds,
		"@UI_TREATY_LIMITATION_FORGEGEMWORLD"
	  },
	  {
		LimitationTreatyType.ResearchTech,
		"@UI_TREATY_LIMITATION_RESEARCH"
	  },
	  {
		LimitationTreatyType.ResearchTree,
		"@UI_TREATY_LIMITATION_RESEARCHTREE"
	  },
	  {
		LimitationTreatyType.ShipClass,
		"@UI_TREATY_LIMITATION_SHIPCLASS"
	  },
	  {
		LimitationTreatyType.StationType,
		"@UI_TREATY_LIMITATION_STATIONTYPE"
	  },
	  {
		LimitationTreatyType.Weapon,
		"@UI_TREATY_LIMITATION_WEAPON"
	  }
	};
		public static Dictionary<IncentiveType, string> IncentiveTypeLocMap = new Dictionary<IncentiveType, string>()
	{
	  {
		IncentiveType.Savings,
		"@UI_TREATY_INCENTIVE_SAVINGS"
	  }
	};
		public static Dictionary<ConsequenceType, string> ConsequenceTypeLocMap = new Dictionary<ConsequenceType, string>()
	{
	  {
		ConsequenceType.DiplomaticPointPenalty,
		"@UI_TREATY_CONSEQUENCE_DIPLOMATIC_POINTS"
	  },
	  {
		ConsequenceType.DiplomaticStatusPenalty,
		"@UI_TREATY_CONSEQUENCE_DIPLOMATIC_STATUS"
	  },
	  {
		ConsequenceType.Fine,
		"@UI_TREATY_CONSEQUENCE_FINE"
	  },
	  {
		ConsequenceType.Sanction,
		"@UI_TREATY_CONSEQUENCE_SANCTION"
	  },
	  {
		ConsequenceType.Trade,
		"@UI_TREATY_CONSEQUENCE_TRADE"
	  },
	  {
		ConsequenceType.War,
		"@UI_TREATY_CONSEQUENCE_WAR"
	  }
	};
		public static Dictionary<DiplomacyState, string> ArmisticeTypeLocMap = new Dictionary<DiplomacyState, string>()
	{
	  {
		DiplomacyState.WAR,
		"@UI_DIPLOMACY_STATE_WAR"
	  },
	  {
		DiplomacyState.CEASE_FIRE,
		"@UI_DIPLOMACY_STATE_CEASE_FIRE"
	  },
	  {
		DiplomacyState.NON_AGGRESSION,
		"@UI_DIPLOMACY_STATE_NON_AGGRESSION"
	  },
	  {
		DiplomacyState.NEUTRAL,
		"@UI_DIPLOMACY_STATE_NEUTRAL"
	  },
	  {
		DiplomacyState.PEACE,
		"@UI_DIPLOMACY_STATE_PEACE"
	  },
	  {
		DiplomacyState.ALLIED,
		"@UI_DIPLOMACY_STATE_ALLIED"
	  }
	};
		private int? _selectedConsequence = new int?();
		private int? _selectedIncentive = new int?();
		private string _consequenceDialogId = "";
		private string _incentiveDialogId = "";
		private const string _armisticePanel = "pnlArmisticePanel";
		private const string _durationPanel = "pnlDurationPanel";
		private const string _durationField = "txtDuration";
		private const string _limitationPanel = "pnlLimitationPanel";
		private const string _consequenceList = "lstConsequences";
		private const string _incentivesList = "lstIncentives";
		private const string _armisticeTypeList = "lstArmisticeType";
		private const string _treatyTypeList = "lstTreatyType";
		private const string _limitationTypeList = "lstLimitationType";
		private const string _limitationGroupList = "lstLimitationGroup";
		private const string _limitationValue = "txtLimitationValue";
		private const string _doneButton = "btnDone";
		private const string _cancelButton = "btnCancel";
		private const string _removeConsequenceButton = "btnRemoveConsequence";
		private const string _editConsequenceButton = "btnEditConsequence";
		private const string _addConsequenceButton = "btnAddConsequence";
		private const string _removeIncentiveButton = "btnRemoveIncentive";
		private const string _editIncentiveButton = "btnEditIncentive";
		private const string _addIncentiveButton = "btnAddIncentive";
		private bool hasTreaty;
		private TreatyInfo _editedTreaty;
		private PlayerInfo _receivingPlayer;
		private ValueBoundSpinner _durationSpinner;
		private ValueBoundSpinner _limitationValueSpinner;

		private Dictionary<DiplomacyStateChange, int> StateChangeMap
		{
			get
			{
				return this._app.AssetDatabase.DiplomacyStateChangeMap;
			}
		}

		public TreatyEditDialog(App game, TreatyInfo treaty, string template = "TreatyConfigurationPopup")
		  : base(game, template)
		{
			this._editedTreaty = treaty;
		}

		public TreatyEditDialog(App game, int OpposingPlayerId, string template = "TreatyConfigurationPopup")
		  : base(game, template)
		{
			this._editedTreaty = (TreatyInfo)new LimitationTreatyInfo();
			this._editedTreaty.InitiatingPlayerId = this._app.Game.LocalPlayer.ID;
			this._editedTreaty.ReceivingPlayerId = OpposingPlayerId;
			this._editedTreaty.Type = TreatyType.Armistice;
			this._editedTreaty.StartingTurn = this._app.GameDatabase.GetTurnCount() + 1;
		}

		public override void Initialize()
		{
			this.InitializePanel();
			this._receivingPlayer = this._app.GameDatabase.GetPlayerInfo(this._editedTreaty.ReceivingPlayerId);
			DiplomacyUI.SyncPanelColor(this._app, "pnlBackground", this._receivingPlayer.PrimaryColor);
			this.hasTreaty = true;
			this.SyncTreatyEditor();
			this._durationSpinner = new ValueBoundSpinner(this.UI, "spnDuration", 1.0, 500.0, 1.0, 1.0);
			this._limitationValueSpinner = new ValueBoundSpinner(this.UI, "spnLimitationValue", 1.0, (double)int.MaxValue, 1.0, 1.0);
			this._durationSpinner.ValueChanged += new ValueChangedEventHandler(this._durationSpinner_ValueChanged);
			this._limitationValueSpinner.ValueChanged += new ValueChangedEventHandler(this._limitationValueSpinner_ValueChanged);
			if (this._receivingPlayer.isStandardPlayer)
				return;
			this._editedTreaty.Type = TreatyType.Incorporate;
			this.SyncTreatyEditor();
		}

		private void _limitationValueSpinner_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			LimitationTreatyInfo editedTreaty = (LimitationTreatyInfo)this._editedTreaty;
			editedTreaty.LimitationAmount = (float)e.NewValue;
			this._app.UI.SetText("txtLimitationValue", editedTreaty.LimitationAmount.ToString());
		}

		private void _durationSpinner_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			this._editedTreaty.Duration = (int)e.NewValue;
			this._app.UI.SetText("txtDuration", this._editedTreaty.Duration.ToString());
		}

		public void InitializePanel()
		{
			DiplomacyState stateBetweenPlayers = this._app.GameDatabase.GetDiplomacyStateBetweenPlayers(this._editedTreaty.InitiatingPlayerId, this._editedTreaty.ReceivingPlayerId);
			this._app.UI.ClearItems("lstLimitationType");
			foreach (LimitationTreatyType index in Enum.GetValues(typeof(LimitationTreatyType)))
				this._app.UI.AddItem("lstLimitationType", string.Empty, (int)index, App.Localize(TreatyEditDialog.LimitationTreatyTypeLocMap[index]));
			this._app.UI.ClearItems("lstArmisticeType");
			foreach (DiplomacyState armisticeTypeMove in this.GetArmisticeTypeMoves(stateBetweenPlayers))
				this._app.UI.AddItem("lstArmisticeType", string.Empty, (int)armisticeTypeMove, App.Localize(TreatyEditDialog.ArmisticeTypeLocMap[armisticeTypeMove]));
		}

		private List<DiplomacyState> GetArmisticeTypeMoves(DiplomacyState currentState)
		{
			List<DiplomacyState> diplomacyStateList = new List<DiplomacyState>();
			foreach (DiplomacyStateChange key in this.StateChangeMap.Keys)
			{
				if (key.lower == currentState)
				{
					List<DiplomacyState> armisticeTypeMoves = this.GetArmisticeTypeMoves(key.upper);
					if (!armisticeTypeMoves.Contains(key.upper))
						armisticeTypeMoves.Add(key.upper);
					foreach (DiplomacyState diplomacyState in armisticeTypeMoves)
					{
						if (!diplomacyStateList.Contains(diplomacyState))
							diplomacyStateList.Add(diplomacyState);
					}
				}
			}
			return diplomacyStateList;
		}

		public void SyncTreatyEditor()
		{
			this._app.UI.ClearItems("lstTreatyType");
			DiplomacyState stateBetweenPlayers = this._app.GameDatabase.GetDiplomacyStateBetweenPlayers(this._editedTreaty.InitiatingPlayerId, this._editedTreaty.ReceivingPlayerId);
			if (this._receivingPlayer.isStandardPlayer)
			{
				if (this._durationSpinner != null)
					this._durationSpinner.SetEnabled(true);
				this._app.UI.AddItem("lstTreatyType", string.Empty, 0, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Armistice]));
				if (stateBetweenPlayers != DiplomacyState.WAR)
					this._app.UI.AddItem("lstTreatyType", string.Empty, 2, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Limitation]));
				if ((!this._app.AssetDatabase.GetFaction(this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.InitiatingPlayerId)).IsFactionIndependentTrader() || this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.InitiatingPlayerId) == this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.ReceivingPlayerId)) && (stateBetweenPlayers == DiplomacyState.NON_AGGRESSION || stateBetweenPlayers == DiplomacyState.PEACE || stateBetweenPlayers == DiplomacyState.ALLIED) || this._app.GameDatabase.GetPlayerFaction(this._editedTreaty.InitiatingPlayerId).Name == "morrigi" && (stateBetweenPlayers == DiplomacyState.CEASE_FIRE || stateBetweenPlayers == DiplomacyState.NON_AGGRESSION || (stateBetweenPlayers == DiplomacyState.PEACE || stateBetweenPlayers == DiplomacyState.ALLIED)))
					this._app.UI.AddItem("lstTreatyType", string.Empty, 1, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Trade]));
			}
			else
			{
				this.hasTreaty = false;
				if (this._durationSpinner != null)
					this._durationSpinner.SetEnabled(false);
				if (this._app.GetStratModifier<bool>(StratModifiers.AllowIncorporate, this._editedTreaty.InitiatingPlayerId))
				{
					this._app.UI.AddItem("lstTreatyType", string.Empty, 4, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Incorporate]));
					this.hasTreaty = true;
				}
				if (this._app.GetStratModifier<bool>(StratModifiers.AllowProtectorate, this._editedTreaty.InitiatingPlayerId) && !this._app.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>().Any<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
			   {
				   if (x.ReceivingPlayerId == this._editedTreaty.ReceivingPlayerId)
					   return x.Active;
				   return false;
			   })))
				{
					this._app.UI.AddItem("lstTreatyType", string.Empty, 3, App.Localize(TreatyEditDialog.TreatyTypeLocMap[TreatyType.Protectorate]));
					this.hasTreaty = true;
				}
				this._app.UI.SetEnabled("btnDone", this.hasTreaty);
			}
			this._app.UI.SetSelection("lstTreatyType", (int)this._editedTreaty.Type);
			this.SyncIncentives();
			this.SyncTreatyTypePanels();
		}

		public void SyncTreatyTypePanels()
		{
			if (this._editedTreaty.Type == TreatyType.Armistice)
			{
				this._app.UI.SetVisible("pnlArmisticePanel", true);
				this._app.UI.SetVisible("pnlDurationPanel", false);
				int receivingPlayerId = this._editedTreaty.ReceivingPlayerId;
				int id = this._editedTreaty.ID;
				if (!typeof(ArmisticeTreatyInfo).IsAssignableFrom(this._editedTreaty.GetType()))
				{
					this._editedTreaty = (TreatyInfo)new ArmisticeTreatyInfo();
					this._editedTreaty.ID = id;
					this._editedTreaty.InitiatingPlayerId = this._app.Game.LocalPlayer.ID;
					this._editedTreaty.ReceivingPlayerId = receivingPlayerId;
					this._editedTreaty.Type = TreatyType.Armistice;
					this._editedTreaty.StartingTurn = this._app.GameDatabase.GetTurnCount() + 1;
				}
				ArmisticeTreatyInfo editedTreaty = (ArmisticeTreatyInfo)this._editedTreaty;
				this._app.UI.SetEnabled("btnDone", editedTreaty.SuggestedDiplomacyState >= DiplomacyState.CEASE_FIRE);
				this._app.UI.SetSelection("lstArmisticeType", (int)editedTreaty.SuggestedDiplomacyState);
			}
			else
			{
				this._app.UI.SetVisible("pnlArmisticePanel", false);
				this._app.UI.SetVisible("pnlDurationPanel", true);
				if (this._durationSpinner != null)
					this._durationSpinner.SetValue((double)this._editedTreaty.Duration);
				this._app.UI.SetPropertyString("txtDuration", "text", this._editedTreaty.Duration.ToString());
				bool flag = this._editedTreaty.Type == TreatyType.Limitation;
				this._app.UI.SetVisible("pnlLimitationPanel", flag);
				if (!flag)
					return;
				int receivingPlayerId = this._editedTreaty.ReceivingPlayerId;
				int id = this._editedTreaty.ID;
				if (!typeof(LimitationTreatyInfo).IsAssignableFrom(this._editedTreaty.GetType()))
				{
					this._editedTreaty = (TreatyInfo)new LimitationTreatyInfo();
					this._editedTreaty.ID = id;
					this._editedTreaty.InitiatingPlayerId = this._app.Game.LocalPlayer.ID;
					this._editedTreaty.ReceivingPlayerId = receivingPlayerId;
					this._editedTreaty.Type = TreatyType.Limitation;
					this._editedTreaty.StartingTurn = this._app.GameDatabase.GetTurnCount() + 1;
				}
				LimitationTreatyInfo editedTreaty = (LimitationTreatyInfo)this._editedTreaty;
				this._app.UI.SetSelection("lstLimitationType", (int)editedTreaty.LimitationType);
				this._app.UI.SetEnabled("txtLimitationValue", TreatyEditDialog.EnableValueList.Contains(editedTreaty.LimitationType));
				this._app.UI.SetEnabled("lstLimitationGroup", TreatyEditDialog.EnableGroupList.Contains(editedTreaty.LimitationType));
				if (TreatyEditDialog.LimitationTypeSpinnerDescriptors.ContainsKey(editedTreaty.LimitationType))
				{
					this._limitationValueSpinner.SetValue((double)editedTreaty.LimitationAmount);
					this._limitationValueSpinner.SetValueDescriptor(TreatyEditDialog.LimitationTypeSpinnerDescriptors[editedTreaty.LimitationType]);
					editedTreaty.LimitationAmount = (float)this._limitationValueSpinner.Value;
					this._app.UI.SetPropertyString("txtLimitationValue", "text", editedTreaty.LimitationAmount.ToString());
				}
				this.SyncConsequences();
				this.SyncLimitationGroup(editedTreaty.LimitationType);
			}
		}

		private void SyncLimitationGroup(LimitationTreatyType ltt)
		{
			this._app.UI.ClearItems("lstLimitationGroup");
			switch (ltt)
			{
				case LimitationTreatyType.ShipClass:
					using (Dictionary<ShipClass, string>.Enumerator enumerator = TreatyEditDialog.ShipClassLimitationGroups.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<ShipClass, string> current = enumerator.Current;
							this._app.UI.AddItem("lstLimitationGroup", string.Empty, (int)current.Key, App.Localize(current.Value));
						}
						break;
					}
				case LimitationTreatyType.Weapon:
					using (IEnumerator<LogicalWeapon> enumerator = this._app.AssetDatabase.Weapons.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							LogicalWeapon current = enumerator.Current;
							this._app.UI.AddItem("lstLimitationGroup", string.Empty, current.UniqueWeaponID, current.WeaponName);
						}
						break;
					}
				case LimitationTreatyType.ResearchTree:
					int userItemId = 0;
					using (List<TechFamily>.Enumerator enumerator = this._app.AssetDatabase.MasterTechTree.TechFamilies.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							TechFamily current = enumerator.Current;
							this._app.UI.AddItem("lstLimitationGroup", string.Empty, userItemId, current.Name);
							++userItemId;
						}
						break;
					}
				case LimitationTreatyType.ResearchTech:
					using (List<Tech>.Enumerator enumerator = this._app.AssetDatabase.MasterTechTree.Technologies.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Tech current = enumerator.Current;
							int techId = this._app.GameDatabase.GetTechID(current.Id);
							PlayerTechInfo playerTechInfo1 = this._app.GameDatabase.GetPlayerTechInfo(this._editedTreaty.InitiatingPlayerId, techId);
							PlayerTechInfo playerTechInfo2 = this._app.GameDatabase.GetPlayerTechInfo(this._editedTreaty.ReceivingPlayerId, techId);
							if (playerTechInfo1 != null && playerTechInfo1.State != TechStates.Researched && (playerTechInfo1.State != TechStates.Researching && playerTechInfo2 != null) && (playerTechInfo2.State != TechStates.Researched && playerTechInfo2.State != TechStates.Researching))
								this._app.UI.AddItem("lstLimitationGroup", string.Empty, techId, current.Name);
						}
						break;
					}
				case LimitationTreatyType.StationType:
					using (Dictionary<StationType, string>.Enumerator enumerator = TreatyEditDialog.StationTypeLimitationGroups.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<StationType, string> current = enumerator.Current;
							this._app.UI.AddItem("lstLimitationGroup", string.Empty, (int)current.Key, App.Localize(current.Value));
						}
						break;
					}
			}
		}

		private void SyncConsequences()
		{
			this._app.UI.ClearItems("lstConsequences");
			for (int userItemId = 0; userItemId < this._editedTreaty.Consequences.Count; ++userItemId)
			{
				this._app.UI.AddItem("lstConsequences", string.Empty, userItemId, string.Empty);
				string itemGlobalId = this._app.UI.GetItemGlobalID("lstConsequences", string.Empty, userItemId, string.Empty);
				this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblHeader"), App.Localize(TreatyEditDialog.ConsequenceTypeLocMap[this._editedTreaty.Consequences[userItemId].Type]));
				this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblValue"), this._editedTreaty.Consequences[userItemId].ConsequenceValue.ToString());
			}
			if (!this._selectedConsequence.HasValue)
				return;
			this._app.UI.SetSelection("lstConsequences", this._selectedConsequence.Value);
		}

		private void SyncIncentives()
		{
			this._app.UI.ClearItems("lstIncentives");
			for (int userItemId = 0; userItemId < this._editedTreaty.Incentives.Count; ++userItemId)
			{
				if (this._editedTreaty.Incentives[userItemId].Type != IncentiveType.Savings || this._app.LocalPlayer.PlayerInfo.CanDebtSpend(this._app.AssetDatabase))
				{
					this._app.UI.AddItem("lstIncentives", string.Empty, userItemId, string.Empty);
					string itemGlobalId = this._app.UI.GetItemGlobalID("lstIncentives", string.Empty, userItemId, string.Empty);
					this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblHeader"), App.Localize(TreatyEditDialog.IncentiveTypeLocMap[this._editedTreaty.Incentives[userItemId].Type]));
					this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "lblValue"), this._editedTreaty.Incentives[userItemId].IncentiveValue.ToString());
				}
			}
			if (!this._selectedIncentive.HasValue)
				return;
			this._app.UI.SetSelection("lstIncentives", this._selectedIncentive.Value);
		}

		private void SyncRDPCost(TreatyInfo ti)
		{
			int treatyRdpCost = this._app.Game.GetTreatyRdpCost(ti);
			this._app.UI.SetButtonText("btnDone", string.Format("{0} ({1}: {2})", (object)App.Localize("@UI_GENERAL_DONE"), (object)App.Localize("@DIPLOMACY_RDP"), (object)treatyRdpCost));
			bool flag = true;
			if (this._editedTreaty.Type == TreatyType.Armistice)
				flag = ((ArmisticeTreatyInfo)this._editedTreaty).SuggestedDiplomacyState >= DiplomacyState.CEASE_FIRE;
			this._app.UI.SetEnabled("btnDone", treatyRdpCost <= this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).GetTotalDiplomacyPoints(this._app.GameDatabase.GetPlayerFactionID(ti.ReceivingPlayerId)) && this.hasTreaty && flag);
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			if (this._durationSpinner.TryPanelMessage(panelId, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive) || this._limitationValueSpinner.TryPanelMessage(panelId, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
				return;
			if (msgType == "list_sel_changed")
			{
				if (panelId == "lstTreatyType")
				{
					this._editedTreaty.Type = (TreatyType)int.Parse(msgParams[0]);
					this.SyncTreatyTypePanels();
					this.SyncRDPCost(this._editedTreaty);
				}
				else if (panelId == "lstLimitationType")
				{
					LimitationTreatyInfo editedTreaty = (LimitationTreatyInfo)this._editedTreaty;
					editedTreaty.LimitationType = (LimitationTreatyType)int.Parse(msgParams[0]);
					if (TreatyEditDialog.EnableValueList.Contains(editedTreaty.LimitationType))
					{
						this._limitationValueSpinner.SetValueDescriptor(TreatyEditDialog.LimitationTypeSpinnerDescriptors[editedTreaty.LimitationType]);
						editedTreaty.LimitationAmount = (float)this._limitationValueSpinner.Value;
						this._app.UI.SetPropertyString("txtLimitationValue", "text", editedTreaty.LimitationAmount.ToString());
					}
					this.SyncLimitationGroup(editedTreaty.LimitationType);
					this._app.UI.ClearSelection("lstLimitationGroup");
					this.SyncRDPCost((TreatyInfo)editedTreaty);
				}
				else if (panelId == "lstArmisticeType")
				{
					ArmisticeTreatyInfo editedTreaty = (ArmisticeTreatyInfo)this._editedTreaty;
					editedTreaty.SuggestedDiplomacyState = (DiplomacyState)int.Parse(msgParams[0]);
					this.SyncRDPCost((TreatyInfo)editedTreaty);
				}
				else if (panelId == "lstLimitationGroup")
				{
					if (int.Parse(msgParams[0]) == -1)
						return;
					LimitationTreatyInfo editedTreaty = (LimitationTreatyInfo)this._editedTreaty;
					switch (editedTreaty.LimitationType)
					{
						case LimitationTreatyType.ShipClass:
							editedTreaty.LimitationGroup = msgParams[0];
							break;
						case LimitationTreatyType.Weapon:
							editedTreaty.LimitationGroup = msgParams[0];
							break;
						case LimitationTreatyType.ResearchTree:
							editedTreaty.LimitationGroup = this._app.AssetDatabase.MasterTechTree.TechFamilies[int.Parse(msgParams[0])].Id;
							break;
						case LimitationTreatyType.ResearchTech:
							editedTreaty.LimitationGroup = msgParams[0];
							break;
						case LimitationTreatyType.StationType:
							editedTreaty.LimitationGroup = msgParams[0];
							break;
					}
				}
				else if (panelId == "lstConsequences")
					this._selectedConsequence = string.IsNullOrEmpty(msgParams[0]) ? new int?() : new int?(int.Parse(msgParams[0]));
				else if (panelId == "lstIncentives")
					this._selectedIncentive = string.IsNullOrEmpty(msgParams[0]) ? new int?() : new int?(int.Parse(msgParams[0]));
			}
			else if (msgType == "button_clicked")
			{
				if (panelId == "btnCancel")
					this._app.UI.CloseDialog((Dialog)this, true);
				if (panelId == "btnDone")
				{
					this._app.GameDatabase.SpendDiplomacyPoints(this._app.GameDatabase.GetPlayerInfo(this._editedTreaty.InitiatingPlayerId), this._app.GameDatabase.GetPlayerFactionID(this._editedTreaty.ReceivingPlayerId), this._app.Game.GetTreatyRdpCost(this._editedTreaty));
					this._app.GameDatabase.DeleteTreatyInfo(this._editedTreaty.ID);
					this._app.GameDatabase.InsertTreaty(this._editedTreaty);
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else if (panelId == "btnRemoveConsequence")
				{
					if (this._selectedConsequence.HasValue)
					{
						this._editedTreaty.Consequences.RemoveAt(this._selectedConsequence.Value);
						this.SyncConsequences();
					}
				}
				else if (panelId == "btnEditConsequence")
				{
					if (this._selectedConsequence.HasValue)
					{
						TreatyConsequenceInfo consequence = this._editedTreaty.Consequences[this._selectedConsequence.Value];
						this._consequenceDialogId = this._app.UI.CreateDialog((Dialog)new ConsequenceEditDialog(this._app, ref consequence, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
					}
				}
				else if (panelId == "btnAddConsequence")
				{
					TreatyConsequenceInfo tci = new TreatyConsequenceInfo();
					tci.TreatyId = this._editedTreaty.ID;
					this._editedTreaty.Consequences.Add(tci);
					this._consequenceDialogId = this._app.UI.CreateDialog((Dialog)new ConsequenceEditDialog(this._app, ref tci, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
				}
				else if (panelId == "btnRemoveIncentive")
				{
					if (this._selectedIncentive.HasValue)
					{
						this._editedTreaty.Incentives.RemoveAt(this._selectedIncentive.Value);
						this.SyncIncentives();
					}
				}
				else if (panelId == "btnEditIncentive")
				{
					if (this._selectedIncentive.HasValue)
					{
						TreatyIncentiveInfo incentive = this._editedTreaty.Incentives[this._selectedIncentive.Value];
						this._incentiveDialogId = this._app.UI.CreateDialog((Dialog)new IncentiveEditDialog(this._app, ref incentive, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
					}
				}
				else if (panelId == "btnAddIncentive")
				{
					TreatyIncentiveInfo tci = new TreatyIncentiveInfo();
					tci.TreatyId = this._editedTreaty.ID;
					this._editedTreaty.Incentives.Add(tci);
					this._incentiveDialogId = this._app.UI.CreateDialog((Dialog)new IncentiveEditDialog(this._app, ref tci, this._receivingPlayer.PrimaryColor, "TreatyConsequencePopup"), null);
				}
			}
			else if (msgType == "dialog_closed")
			{
				if (panelId == this._incentiveDialogId)
					this.SyncIncentives();
				else if (panelId == this._consequenceDialogId)
					this.SyncConsequences();
			}
			else if (msgType == "text_changed")
			{
				if (panelId == "txtDuration")
				{
					int result = 0;
					if (int.TryParse(msgParams[0], out result))
						this._editedTreaty.Duration = result;
				}
				else if (panelId == "txtLimitationValue")
				{
					float result = 0.0f;
					if (float.TryParse(msgParams[0], out result))
						((LimitationTreatyInfo)this._editedTreaty).LimitationAmount = result;
				}
			}
			base.OnPanelMessage(panelId, msgType, msgParams);
		}

		public override string[] CloseDialog()
		{
			return new List<string>().ToArray();
		}
	}
}
