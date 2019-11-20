// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.BuildScreenState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class BuildScreenState : GameState, IKeyBindListener
	{
		private static readonly string UIInvoiceItemList = "lstInvoiceItemList";
		private static readonly string UIBuildOrderList = "lstBuildOrderList";
		private static readonly string UIInvoiceTotalSavings = "lblInvoiceTotalSavings";
		private static readonly string UIInvoiceTotalTurns = "lblInvoiceTotalTurns";
		private static readonly string UIInvoiceList = "gameInvoiceList";
		private static readonly string UIClassList = "gameClassList";
		private static readonly string UIDesignList = "gameDesignList";
		private static readonly string UIPlanetDetails = "gamePlanetDetails";
		private static readonly string UISubmitOrder = "gameSubmitOrder";
		private static readonly string UICloseInvoiceSummary = "btnCloseInvoiceSummary";
		private static readonly string UIRemoveInvoiceItem = "btnRemoveInvoiceItem";
		private static readonly string UIRemoveOrderItem = "btnRemoveOrderItem";
		private static readonly string UIOrderInvoiceItems = "ConstructionOrder";
		private static readonly string UIInvoiceSummaryName = "lblInvoiceName";
		private static readonly string UIBuildOrderPanel = "pnlInvoiceSummary";
		private static readonly string UIExitButton = "gameExitButton";
		private static readonly string UIDesignScreenButton = "gameDesignScreen";
		private static readonly string UISystemMap = "partMiniSystem";
		private static readonly string UICurrentMaintenance = "financeCurrent";
		private static readonly string UIProjectedCost = "financeProjectedCost";
		private static readonly string UITotalMaintenance = "financeTotal";
		private static readonly string UISysName = "sysnameValue";
		private static readonly string UISysProductionValue = "sysproductionValue";
		private static readonly string UISysIncomeValue = "sysincomeValue";
		private static readonly string UICvsTValue = "CvsTValue";
		private static readonly string UISubmitDialogOk = "submit_dialog_ok";
		private static readonly string UISubmitDialogCancel = "submit_dialog_cancel";
		private static readonly string UIInvoiceName = "edit_design_name";
		private static readonly string UILoaDialogOk = "loa_dialog_ok";
		private static readonly string UILoaDialogCancel = "loa_dialog_cancel";
		private static readonly string UIInvoiceRemove = "invoiceRemove";
		private static readonly string UIFavInvoiceRemove = "faveinvoiceRemove";
		private static readonly string UIShipRemove = "shipRemove";
		private static readonly string UIAddToInvoiceFavorites = "addToInvoiceFavorites";
		private static readonly string UIInvoiceSummaryPopup = "pnlInvoiceSummaryPopup";
		private int _minLoaCubeval = 1000;
		private int _maxLoaCubeval = 10000;
		private int _loaSliderNotch = 10000;
		private string _invoiceName = "";
		private int? _selectedInvoiceItem = new int?();
		private int? _selectedBuildOrder = new int?();
		private int? _selectedInvoice = new int?();
		private int? _selectedFavInvoice = new int?();
		private List<DesignInfo> _designList = new List<DesignInfo>();
		private List<InvoiceInfo> _invoiceList = new List<InvoiceInfo>();
		private List<BuildScreenState.InvoiceItem> _invoiceItems = new List<BuildScreenState.InvoiceItem>();
		private const string UIPiechartPanelId = "piechart";
		private ShipHoloView _shipHoloView;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private ShipBuilder _builder;
		private BudgetPiechart _piechart;
		private bool _addToFavorites;
		private int _deleteItemID;
		private string _deleteItemDialog;
		private string _deleteInvoiceDialog;
		private string _confirmDiscardInvoiceDialog;
		private bool _confirmInvoiceDialogActive;
		private int _loacubeval;
		private float _totalShipProductionRate;
		private int _selectedOrder;
		private int _selectedSystem;
		private RealShipClasses? _selectedClass;
		private WeaponHoverPanel _weaponTooltip;
		private ModuleHoverPanel _moduleTooltip;
		private int _selectedDesign;
		private int HACK_OrderID;

		private void SyncWeaponUi()
		{
		}

		public void SyncSystemDetails()
		{
			int selectedSystem = this._selectedSystem;
			if (selectedSystem == 0)
				return;
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(selectedSystem);
			this.App.UI.SetPropertyString(BuildScreenState.UISysName, "text", starSystemInfo.Name);
			StarSystemMapUI.Sync(this.App, selectedSystem, BuildScreenState.UISystemMap, false);
		}

		private int SelectedOrder
		{
			get
			{
				return this._selectedOrder;
			}
			set
			{
				if (this._selectedOrder == value)
					return;
				this._selectedOrder = value;
				if (this.SelectedOrder == 0)
					return;
				this.SetSelectedDesign(this.App.GameDatabase.GetDesignInfo(this.App.GameDatabase.GetBuildOrdersForSystem(this._selectedSystem).First<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x => x.ID == this.SelectedOrder)).DesignID).ID, string.Empty);
			}
		}

		public BuildScreenState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this.App.UI.LoadScreen("Build");
			if (this.App.LocalPlayer == null)
			{
				this.App.NewGame();
				int? homeworld = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).Homeworld;
				if (!homeworld.HasValue)
					throw new ArgumentException("Build screen requires a home world.");
				this._selectedSystem = homeworld.Value;
			}
			else if (((IEnumerable<object>)stateParams).Count<object>() > 0)
				this._selectedSystem = (int)stateParams[0];
			this._crits = new GameObjectSet(this.App);
			this._camera = this._crits.Add<OrbitCameraController>((object)string.Empty);
			this._shipHoloView = new ShipHoloView(this.App, this._camera);
			this._crits.Add((IGameObject)this._shipHoloView);
			this._builder = new ShipBuilder(this.App);
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("Build");
			this._piechart = new BudgetPiechart(this.App.UI, "piechart", this.App.AssetDatabase);
			this._confirmInvoiceDialogActive = false;
			this.App.UI.ClearItems(BuildScreenState.UIDesignList);
			this.App.UI.SetPropertyBool(this.App.UI.Path(BuildScreenState.UIPlanetDetails, "partTradeSlider"), "only_user_events", true);
			this.App.UI.SetPropertyBool(this.App.UI.Path(BuildScreenState.UIPlanetDetails, "partShipConSlider"), "only_user_events", true);
			this.App.UI.Send((object)"SetGameObject", (object)"designShip", (object)this._shipHoloView.ObjectID);
			EmpireBarUI.SyncTitleFrame(this.App);
			int selectedSystem = this._selectedSystem;
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(selectedSystem);
			StationInfo systemPlayerAndType = this.App.GameDatabase.GetStationForSystemPlayerAndType(selectedSystem, this.App.LocalPlayer.ID, StationType.NAVAL);
			string str = "";
			if (systemPlayerAndType != null)
				str = string.Format(", {0}", (object)systemPlayerAndType.DesignInfo.Name);
			this._camera.Active = true;
			this._camera.MaxDistance = 2000f;
			this._camera.DesiredDistance = 800f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this.SetSelectedSystem(selectedSystem, "init");
			this.PopulateClassList(new RealShipClasses?(RealShipClasses.Cruiser));
			this.PopulateDesignList(this._selectedClass);
			this.SyncConstructionSite(this.App);
			this.SyncFinancialDetails(this.App);
			this.SyncSystemDetails();
			this.PopulateInvoiceList();
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			this.App.UI.ForceLayout(BuildScreenState.UIInvoiceList);
			this.App.UI.SetPropertyString("gameScreenFrame.TopBar.Screen_Title", "text", string.Format("System: {0}{1}", (object)starSystemInfo.Name, (object)str));
			this.App.UI.AutoSize("gameScreenFrame.TopBar.Screen_Title");
			this._minLoaCubeval = this.App.AssetDatabase.MinLoaCubesOnBuild;
			this._maxLoaCubeval = this.App.AssetDatabase.MaxLoaCubesOnBuild;
			this._loacubeval = this._minLoaCubeval;
			this.App.UI.SetPropertyString(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider", "right_label"), "text", this._loacubeval.ToString());
			this.App.UI.SetSliderRange(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider"), this._minLoaCubeval, this._maxLoaCubeval);
			this.App.UI.SetSliderValue(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider"), this._minLoaCubeval);
			this.App.UI.SetSliderTolerance(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider"), 1000);
			for (int minLoaCubeval = this._minLoaCubeval; minLoaCubeval <= this._maxLoaCubeval; minLoaCubeval += this._loaSliderNotch)
				this.App.UI.AddSliderNotch(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider"), minLoaCubeval);
			this.App.UI.SetText(this.App.UI.Path("LoaCubeDialog", "LoaPointValue"), this._minLoaCubeval.ToString());
			this.App.UI.SetVisible("Build.loaBuildWarning", this.App.LocalPlayer.Faction.Name == "loa" && this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).Savings < 0.0);
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
			this.App.UI.SetVisible("title_prev", true);
			this.App.UI.SetVisible("title_next", true);
		}

		protected void SelectNextSystem(bool reverse = false)
		{
			List<int> list = this.App.GameDatabase.GetPlayerColonySystemIDs(this.App.LocalPlayer.ID).ToList<int>();
			list.Sort();
			if (reverse)
				list.Reverse();
			int num = this._selectedSystem != list.Last<int>() ? list[list.IndexOf(this._selectedSystem) + 1] : list.First<int>();
			if (num == -1 || num == this._selectedSystem)
				return;
			this._selectedSystem = num;
			int selectedSystem = this._selectedSystem;
			StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(selectedSystem);
			StationInfo systemPlayerAndType = this.App.GameDatabase.GetStationForSystemPlayerAndType(selectedSystem, this.App.LocalPlayer.ID, StationType.NAVAL);
			string str = "";
			if (systemPlayerAndType != null)
				str = string.Format(", {0}", (object)systemPlayerAndType.DesignInfo.Name);
			this._camera.Active = true;
			this._camera.MaxDistance = 2000f;
			this._camera.DesiredDistance = 800f;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this.SetSelectedSystem(selectedSystem, "init");
			this.PopulateClassList(new RealShipClasses?(RealShipClasses.Cruiser));
			this.PopulateDesignList(this._selectedClass);
			this.SyncConstructionSite(this.App);
			this.SyncFinancialDetails(this.App);
			this.SyncSystemDetails();
			this.PopulateInvoiceList();
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			this.App.UI.ForceLayout(BuildScreenState.UIInvoiceList);
			this.App.UI.SetPropertyString("gameScreenFrame.TopBar.Screen_Title", "text", string.Format("System: {0}{1}", (object)starSystemInfo.Name, (object)str));
			this.App.UI.AutoSize("gameScreenFrame.TopBar.Screen_Title");
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			this._piechart = (BudgetPiechart)null;
			this._camera.Active = false;
			this._camera.TargetID = 0;
			this._builder.Dispose();
			if (this._crits == null)
				return;
			this._crits.Dispose();
			this._crits = (GameObjectSet)null;
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (this._piechart.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self))
				return;
			if (msgType == "dialog_closed")
			{
				if (panelName == this._confirmDiscardInvoiceDialog)
				{
					if (bool.Parse(msgParams[0]))
					{
						this._invoiceItems.Clear();
						this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
					}
				}
				else if (panelName == this._deleteItemDialog)
				{
					if (bool.Parse(msgParams[0]))
					{
						this.App.GameDatabase.RemovePlayerDesign(this._deleteItemID);
						this.PopulateClassList(this._selectedClass);
						this.PopulateDesignList(this._selectedClass);
					}
				}
				else if (panelName == this._deleteInvoiceDialog && bool.Parse(msgParams[0]))
				{
					this.App.GameDatabase.RemoveFavoriteInvoice(this._deleteItemID);
					this.PopulateDesignList(this._selectedClass);
				}
			}
			else if (msgType == "button_clicked")
			{
				if (panelName.Contains("designDeleteButton"))
				{
					string[] strArray = panelName.Split('|');
					this._deleteItemID = int.Parse(strArray[1]);
					if (((IEnumerable<string>)strArray).Count<string>() == 3)
					{
						InvoiceInfo invoiceInfo = this.App.GameDatabase.GetInvoiceInfo(this._deleteItemID, this.App.LocalPlayer.ID);
						if (invoiceInfo != null)
							this._deleteInvoiceDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_DESIGN_DELETE_INVOICE_TITLE"), string.Format(App.Localize("@UI_DESIGN_DELETE_INVOICE_DESC"), (object)invoiceInfo.Name), "dialogGenericQuestion"), null);
					}
					else
					{
						DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(this._deleteItemID);
						if (designInfo != null)
							this._deleteItemDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, App.Localize("@UI_DESIGN_DELETE_TITLE"), string.Format(App.Localize("@UI_DESIGN_DELETE_DESC"), (object)designInfo.Name), "dialogGenericQuestion"), null);
					}
				}
				else if (panelName == BuildScreenState.UIAddToInvoiceFavorites)
					this._addToFavorites = !this._addToFavorites;
				else if (panelName == "gameTutorialButton")
					this.App.UI.SetVisible("BuildScreenTutorial", true);
				else if (panelName == "buildScreenTutImage")
					this.App.UI.SetVisible("BuildScreenTutorial", false);
				else if (panelName == BuildScreenState.UIInvoiceRemove)
				{
					this.RemoveInvoice(this._selectedInvoice);
					this._selectedInvoice = new int?();
				}
				else if (panelName == BuildScreenState.UICloseInvoiceSummary)
				{
					this.App.UI.SetVisible(BuildScreenState.UIInvoiceSummaryPopup, false);
					this.PopulateInvoiceList();
				}
				else if (panelName == BuildScreenState.UIFavInvoiceRemove)
				{
					this.RemoveFavInvoice(this._selectedFavInvoice);
					this._selectedFavInvoice = new int?();
				}
				else if (panelName == BuildScreenState.UIRemoveInvoiceItem)
				{
					this.RemoveBuildOrder(this._selectedBuildOrder);
					this.SyncCost(this.App, this.App.GameDatabase.GetDesignInfo(this.SelectedDesign));
					this._selectedBuildOrder = new int?();
				}
				else if (panelName == BuildScreenState.UIRemoveOrderItem)
				{
					this.RemoveInvoiceItem(this._selectedInvoiceItem);
					this.SyncCost(this.App, this.App.GameDatabase.GetDesignInfo(this.SelectedDesign));
					this._selectedInvoiceItem = new int?();
				}
				else if (panelName == BuildScreenState.UISubmitDialogOk)
				{
					this.SubmitOrder(this._invoiceName);
					this.App.UI.SetPropertyString(BuildScreenState.UIInvoiceName, "text", "");
					this.App.UI.SetEnabled(BuildScreenState.UISubmitDialogOk, false);
					this.App.UI.SetVisible("submit_dialog", false);
					this._confirmInvoiceDialogActive = false;
					this._selectedInvoiceItem = new int?();
				}
				else if (panelName == BuildScreenState.UISubmitDialogCancel)
				{
					this.App.UI.SetVisible("submit_dialog", false);
					this._confirmInvoiceDialogActive = false;
				}
				else if (panelName == BuildScreenState.UILoaDialogOk)
				{
					this.AddOrder(this._designList.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsLoaCube())).ID, true, true);
					this.App.UI.SetVisible("LoaCubeDialog", false);
				}
				else if (panelName == BuildScreenState.UILoaDialogCancel)
					this.App.UI.SetVisible("LoaCubeDialog", false);
				else if (panelName == "game_budget_pie")
				{
					this.App.UI.LockUI();
					this.App.SwitchGameState<EmpireSummaryState>();
				}
				else if (panelName == BuildScreenState.UIExitButton)
				{
					if (this._invoiceItems.Count > 0)
					{
						this._confirmDiscardInvoiceDialog = this.App.UI.CreateDialog((Dialog)new GenericQuestionDialog(this.App, "@UI_BUILD_PENDING_INVOICE_TITLE", "@UI_BUILD_PENDING_INVOICE_DESC", "dialogGenericQuestion"), null);
					}
					else
					{
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
					}
				}
				else if (panelName == BuildScreenState.UIDesignScreenButton)
				{
					this.App.UI.LockUI();
					this.App.SwitchGameState<DesignScreenState>((object)false, (object)nameof(BuildScreenState));
				}
				else if (panelName == BuildScreenState.UISubmitOrder)
				{
					IEnumerable<InvoiceInfo> invoiceInfosForPlayer = this.App.GameDatabase.GetInvoiceInfosForPlayer(this.App.LocalPlayer.ID);
					int num = 1;
					while (true)
					{
						this._invoiceName = string.Format("Invoice #{0}", (object)num);
						bool flag = false;
						foreach (InvoiceInfo invoiceInfo in invoiceInfosForPlayer)
						{
							if (invoiceInfo.Name == this._invoiceName)
							{
								flag = true;
								break;
							}
						}
						if (flag)
							++num;
						else
							break;
					}
					this.App.UI.SetEnabled(BuildScreenState.UISubmitDialogOk, true);
					this.App.UI.SetPropertyString(BuildScreenState.UIInvoiceName, "text", this._invoiceName);
					this.App.UI.SetVisible("submit_dialog", true);
					this._confirmInvoiceDialogActive = true;
				}
				else if (panelName == "title_prev")
					this.SelectNextSystem(true);
				else if (panelName == "title_next")
					this.SelectNextSystem(false);
				else if (panelName == "gameAddDesignButton")
				{
					if (!this.SelectedClass.HasValue)
					{
						if (this._selectedFavInvoice.HasValue && this._selectedFavInvoice.HasValue)
							this.SetSelectedInvoice(this._selectedFavInvoice.Value);
					}
					else
						this.AddOrder(this.SelectedDesign, true, false);
				}
				else if (panelName.StartsWith("DeleteOrder"))
					this.RemoveInvoiceItem(new int?(int.Parse(panelName.Replace("DeleteOrder", string.Empty))));
				else if (panelName == "gameRandomizeSummaryShipNames")
				{
					List<BuildScreenState.InvoiceItem> invoiceItemList = new List<BuildScreenState.InvoiceItem>();
					foreach (BuildOrderInfo buildOrder in this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>())
					{
						DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(buildOrder.DesignID);
						buildOrder.ShipName = this.App.Game.NamesPool.GetShipName(this.App.Game, this.App.LocalPlayer.ID, designInfo.Class, (IEnumerable<string>)null);
						string listId = this.App.UI.Path(BuildScreenState.UIBuildOrderPanel, BuildScreenState.UIBuildOrderList);
						string subPanelId = string.Format("{0}{1}", (object)BuildScreenState.UIBuildOrderList, (object)buildOrder.ID);
						this.App.UI.SetItemPropertyString(listId, string.Empty, buildOrder.ID, subPanelId, "text", buildOrder.ShipName);
						this.App.GameDatabase.UpdateBuildOrder(buildOrder);
					}
				}
				else if (panelName == "gameRandomizeShipNames")
				{
					foreach (BuildScreenState.InvoiceItem invoiceItem in this._invoiceItems)
					{
						DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(invoiceItem.DesignID);
						invoiceItem.ShipName = this.App.Game.NamesPool.GetShipName(this.App.Game, this.App.LocalPlayer.ID, designInfo.Class, this._invoiceItems.Select<BuildScreenState.InvoiceItem, string>((Func<BuildScreenState.InvoiceItem, string>)(x => x.ShipName)));
					}
					if (this._invoiceItems.Any<BuildScreenState.InvoiceItem>())
						this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
				}
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == BuildScreenState.UIClassList)
				{
					int id = BuildScreenState.ParseId(msgParams[0]);
					this.SetSelectedShipClass(id != -1 ? new RealShipClasses?((RealShipClasses)id) : new RealShipClasses?(), string.Empty);
				}
				else if (panelName == BuildScreenState.UIDesignList)
				{
					if (this._selectedClass.HasValue)
						this.SetSelectedDesign(BuildScreenState.ParseId(msgParams[0]), BuildScreenState.UIDesignList);
					else
						this.SetFavInvoice(new int?(BuildScreenState.ParseId(msgParams[0])));
				}
				else if (panelName == BuildScreenState.UIInvoiceList)
					this._selectedInvoice = new int?(BuildScreenState.ParseId(msgParams[0]));
				else if (panelName == BuildScreenState.UIInvoiceItemList)
					this._selectedInvoiceItem = new int?(BuildScreenState.ParseId(msgParams[0]));
				else if (panelName == BuildScreenState.UIBuildOrderList)
					this._selectedBuildOrder = new int?(BuildScreenState.ParseId(msgParams[0]));
			}
			else if (msgType == "list_item_dblclk")
			{
				if (panelName == BuildScreenState.UIDesignList)
				{
					int id = BuildScreenState.ParseId(msgParams[0]);
					if (!this.SelectedClass.HasValue)
						this.SetSelectedInvoice(id);
					else
						this.AddOrder(id, true, false);
				}
				else if (panelName == BuildScreenState.UIInvoiceList)
				{
					List<BuildScreenState.InvoiceItem> items = new List<BuildScreenState.InvoiceItem>();
					foreach (BuildOrderInfo buildOrderInfo in this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>())
						items.Insert(0, new BuildScreenState.InvoiceItem()
						{
							DesignID = buildOrderInfo.DesignID,
							ShipName = buildOrderInfo.ShipName,
							TempOrderID = buildOrderInfo.ID,
							Progress = (int)((double)buildOrderInfo.Progress * 100.0 / (double)buildOrderInfo.ProductionTarget),
							isPrototypeOrder = !this.App.GameDatabase.GetDesignInfo(buildOrderInfo.DesignID).isPrototyped
						});
					bool retrofitinvoice = false;
					int shipid = 0;
					foreach (RetrofitOrderInfo retrofitOrderInfo in this.App.GameDatabase.GetRetrofitOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<RetrofitOrderInfo>())
					{
						ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(retrofitOrderInfo.ShipID, true);
						items.Insert(0, new BuildScreenState.InvoiceItem()
						{
							DesignID = retrofitOrderInfo.DesignID,
							ShipName = shipInfo.ShipName + " (" + shipInfo.DesignInfo.Name + ")",
							TempOrderID = retrofitOrderInfo.ID,
							Progress = 0,
							isPrototypeOrder = false
						});
						retrofitinvoice = true;
						shipid = retrofitOrderInfo.ShipID;
					}
					this.SyncInvoiceItemsList(BuildScreenState.UIBuildOrderPanel, BuildScreenState.UIBuildOrderList, items, this.App.GameDatabase.GetInvoiceInstanceInfo(this._selectedInvoice.Value).Name, retrofitinvoice, shipid);
					this.App.UI.SetVisible(BuildScreenState.UIInvoiceSummaryPopup, true);
				}
			}
			else if (msgType == "slider_value")
			{
				if (panelName == "LoaPointSlider")
				{
					this._loacubeval = (int)float.Parse(msgParams[0]);
					this.App.UI.SetPropertyString(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider", "right_label"), "text", this._loacubeval.ToString());
					this.App.UI.SetText(this.App.UI.Path("LoaCubeDialog", "LoaPointValue"), this._loacubeval.ToString());
					this.SyncCost(this.App, this.App.GameDatabase.GetDesignInfo(this.SelectedDesign));
				}
			}
			else if (msgType == "text_changed")
			{
				if (panelName.StartsWith(BuildScreenState.UIInvoiceItemList))
				{
					int orderId = int.Parse(panelName.Replace(BuildScreenState.UIInvoiceItemList, ""));
					this._invoiceItems.Single<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x => x.TempOrderID == orderId)).ShipName = msgParams[0];
				}
				else if (panelName.StartsWith(BuildScreenState.UIBuildOrderList))
				{
					BuildOrderInfo buildOrderInfo = this.App.GameDatabase.GetBuildOrderInfo(int.Parse(panelName.Replace(BuildScreenState.UIBuildOrderList, "")));
					buildOrderInfo.ShipName = msgParams[0];
					this.App.GameDatabase.UpdateBuildOrder(buildOrderInfo);
				}
				else if (panelName == BuildScreenState.UIInvoiceName)
					this._invoiceName = msgParams[0];
				else if (panelName == "LoaPointValue")
				{
					int result;
					if (int.TryParse(msgParams[0], out result))
					{
						int num = Math.Max(this._minLoaCubeval, Math.Min(result, this._maxLoaCubeval));
						this.App.UI.SetSliderValue(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider"), num);
						this.App.UI.SetPropertyString(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider", "right_label"), "text", num.ToString());
						this._loacubeval = num;
					}
					else if (msgParams[0] == string.Empty)
					{
						int minLoaCubeval = this._minLoaCubeval;
						this.App.UI.SetSliderValue(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider"), minLoaCubeval);
						this.App.UI.SetPropertyString(this.App.UI.Path("LoaCubeDialog", "LoaPointSlider", "right_label"), "text", minLoaCubeval.ToString());
						this._loacubeval = minLoaCubeval;
					}
				}
			}
			this.App.UI.SetEnabled(BuildScreenState.UIInvoiceRemove, this._selectedInvoice.HasValue);
			this.App.UI.SetEnabled(BuildScreenState.UIShipRemove, this._selectedInvoiceItem.HasValue);
			this.App.UI.SetEnabled(BuildScreenState.UIFavInvoiceRemove, this._selectedFavInvoice.HasValue);
			bool flag1 = false;
			bool flag2 = false;
			DesignInfo designInfo1 = this.App.GameDatabase.GetDesignInfo(this._selectedDesign);
			if (designInfo1 != null)
			{
				if (!designInfo1.isPrototyped)
				{
					if (this.App.GameDatabase.GetDesignBuildOrders(designInfo1).ToList<BuildOrderInfo>().Count > 0)
					{
						flag1 = true;
						this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_PROTOTYPE_ALREADY"));
					}
					else if (this._invoiceItems.Count<BuildScreenState.InvoiceItem>() > 0)
					{
						flag1 = true;
						this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_CANNOT_ADD_PROTOTYPE"));
						DesignInfo designInfo2 = this.App.GameDatabase.GetDesignInfo(this._invoiceItems[0].DesignID);
						if (designInfo2 != null && !designInfo2.isPrototyped)
							flag2 = true;
					}
				}
				else if (this._invoiceItems.Count<BuildScreenState.InvoiceItem>() > 0)
				{
					DesignInfo designInfo2 = this.App.GameDatabase.GetDesignInfo(this._invoiceItems[0].DesignID);
					if (designInfo2 != null && !designInfo2.isPrototyped)
					{
						flag1 = true;
						this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_CANNOT_ADD_PROTOTYPE"));
						flag2 = true;
					}
				}
			}
			if (flag1)
			{
				this.App.UI.SetEnabled("gameAddDesignButton", false);
				if (!flag2)
					return;
				this.App.UI.SetVisible("invoicePanelNormal", false);
				this.App.UI.SetVisible("invoicePanelPrototype", true);
				this.App.UI.SetPropertyString("newInvoiceText", "text", App.Localize("@UI_BUILD_NEW_PROTOTYPE"));
			}
			else
			{
				this.App.UI.SetEnabled("gameAddDesignButton", true);
				this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_ADDTOBUILD"));
				this.App.UI.SetVisible("invoicePanelNormal", true);
				this.App.UI.SetVisible("invoicePanelPrototype", false);
				this.App.UI.SetPropertyString("newInvoiceText", "text", App.Localize("@UI_BUILD_NEW_INVOICE"));
			}
		}

		private void RemoveInvoice(int? invoiceId)
		{
			if (!invoiceId.HasValue)
				return;
			List<BuildOrderInfo> list = this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(invoiceId.Value).ToList<BuildOrderInfo>();
			if (list.Any<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x => !this.App.GameDatabase.GetDesignInfo(x.DesignID).isPrototyped)))
			{
				foreach (BuildOrderInfo buildOrderInfo in list.Where<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x => !this.App.GameDatabase.GetDesignInfo(x.DesignID).isPrototyped)).ToList<BuildOrderInfo>())
				{
					BuildOrderInfo bi = buildOrderInfo;
					if (this._invoiceItems.Any<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x => x.DesignID == bi.DesignID)))
						this._invoiceItems.First<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x => x.DesignID == bi.DesignID)).isPrototypeOrder = true;
				}
			}
			foreach (BuildOrderInfo buildOrderInfo in this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(invoiceId.Value))
				this.App.GameDatabase.RemoveBuildOrder(buildOrderInfo.ID);
			foreach (RetrofitOrderInfo retrofitOrderInfo in this.App.GameDatabase.GetRetrofitOrdersForInvoiceInstance(invoiceId.Value))
				this.App.GameDatabase.RemoveRetrofitOrder(retrofitOrderInfo.ID, false, false);
			this.App.UI.RemoveItems(BuildScreenState.UIInvoiceList, invoiceId.Value);
			this.App.GameDatabase.RemoveInvoiceInstance(invoiceId.Value);
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			this.SyncCost(this.App, this.App.GameDatabase.GetDesignInfo(this._selectedDesign));
		}

		private void RemoveBuildOrder(int? orderId)
		{
			if (!orderId.HasValue)
				return;
			List<BuildOrderInfo> list = this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>();
			if (list.Count <= 1)
				return;
			BuildOrderInfo bi = list.First<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x =>
		   {
			   int id = x.ID;
			   int? nullable = orderId;
			   if (id == nullable.GetValueOrDefault())
				   return nullable.HasValue;
			   return false;
		   }));
			DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(bi.DesignID);
			if (!this.App.GameDatabase.GetDesignInfo(bi.DesignID).isPrototyped && list.Any<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x =>
		   {
			   if (x != bi)
				   return x.DesignID == bi.DesignID;
			   return false;
		   })))
			{
				BuildOrderInfo buildOrder = list.First<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x =>
			   {
				   if (x != bi)
					   return x.DesignID == bi.DesignID;
				   return false;
			   }));
				buildOrder.ProductionTarget = designInfo.GetPlayerProductionCost(this.App.GameDatabase, this.App.LocalPlayer.ID, true, new float?());
				this.App.GameDatabase.UpdateBuildOrder(buildOrder);
			}
			this.App.GameDatabase.RemoveBuildOrder(orderId.Value);
			List<BuildScreenState.InvoiceItem> items = new List<BuildScreenState.InvoiceItem>();
			foreach (BuildOrderInfo buildOrderInfo in this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(this._selectedInvoice.Value).ToList<BuildOrderInfo>())
				items.Insert(0, new BuildScreenState.InvoiceItem()
				{
					DesignID = buildOrderInfo.DesignID,
					ShipName = buildOrderInfo.ShipName,
					TempOrderID = buildOrderInfo.ID,
					Progress = (int)((double)buildOrderInfo.Progress * 100.0 / (double)buildOrderInfo.ProductionTarget),
					isPrototypeOrder = this.App.GameDatabase.GetDesignInfo(buildOrderInfo.DesignID).ProductionCost < buildOrderInfo.ProductionTarget
				});
			this.SyncInvoiceItemsList(BuildScreenState.UIBuildOrderPanel, BuildScreenState.UIBuildOrderList, items, this.App.GameDatabase.GetInvoiceInstanceInfo(this._selectedInvoice.Value).Name, false, 0);
		}

		private void RemoveInvoiceItem(int? orderId)
		{
			if (!orderId.HasValue)
				return;
			BuildScreenState.InvoiceItem found = this._invoiceItems.FirstOrDefault<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x =>
		   {
			   int tempOrderId = x.TempOrderID;
			   int? nullable = orderId;
			   if (tempOrderId == nullable.GetValueOrDefault())
				   return nullable.HasValue;
			   return false;
		   }));
			if (found == null)
				return;
			if (found.isPrototypeOrder && this._invoiceItems.Any<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x =>
		   {
			   if (x != found)
				   return x.DesignID == found.DesignID;
			   return false;
		   })))
				this._invoiceItems.First<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x =>
			   {
				   if (x != found)
					   return x.DesignID == found.DesignID;
				   return false;
			   })).isPrototypeOrder = true;
			this._invoiceItems.Remove(found);
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			this.App.UI.SetEnabled(BuildScreenState.UISubmitOrder, this._invoiceItems.Count != 0);
			this.SyncFinancialDetails(this.App);
		}

		private void RemoveFavInvoice(int? favInvoiceId)
		{
			if (!favInvoiceId.HasValue)
				return;
			this.App.GameDatabase.RemoveFavoriteInvoice(favInvoiceId.Value);
			this.PopulateDesignList(this._selectedClass);
		}

		private static bool IsShipClassAllowed(RealShipClasses? value)
		{
			if (!value.HasValue)
				return false;
			switch (value.Value)
			{
				case RealShipClasses.Cruiser:
				case RealShipClasses.Dreadnought:
				case RealShipClasses.Leviathan:
				case RealShipClasses.BattleRider:
				case RealShipClasses.BattleCruiser:
				case RealShipClasses.BattleShip:
				case RealShipClasses.Platform:
				case RealShipClasses.SystemDefenseBoat:
					return true;
				case RealShipClasses.Drone:
				case RealShipClasses.BoardingPod:
				case RealShipClasses.EscapePod:
				case RealShipClasses.AssaultShuttle:
				case RealShipClasses.Biomissile:
				case RealShipClasses.Station:
				case RealShipClasses.NumShipClasses:
					return false;
				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}

		public static bool IsShipRoleAllowed(ShipRole role)
		{
			switch (role)
			{
				case ShipRole.BOARDINGPOD:
				case ShipRole.BIOMISSILE:
				case ShipRole.TRAPDRONE:
				case ShipRole.ACCELERATOR_GATE:
					return false;
				default:
					return true;
			}
		}

		private IEnumerable<RealShipClasses> GetAllowedShipClasses()
		{
			foreach (RealShipClasses realShipClass in ShipClassExtensions.RealShipClasses)
			{
				if (BuildScreenState.IsShipClassAllowed(new RealShipClasses?(realShipClass)))
					yield return realShipClass;
			}
		}

		private bool IsDesignAllowed(DesignInfo designInfo)
		{
			if (BuildScreenState.IsShipClassAllowed(designInfo.GetRealShipClass()) && BuildScreenState.IsShipRoleAllowed(designInfo.Role))
				return !Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this.App, designInfo);
			return false;
		}

		private IEnumerable<DesignInfo> GetAvailableDesigns(
		  RealShipClasses shipClass)
		{
			IEnumerable<DesignInfo> designInfosForPlayer = this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID, shipClass);
			List<DesignInfo> list = designInfosForPlayer.ToList<DesignInfo>();
			foreach (DesignInfo designInfo in designInfosForPlayer)
			{
				if (!this.IsDesignAllowed(designInfo) || !Kerberos.Sots.StarFleet.StarFleet.IsNewestRetrofit(designInfo, designInfosForPlayer))
					list.Remove(designInfo);
			}
			return (IEnumerable<DesignInfo>)list;
		}

		private int SelectedSystem
		{
			get
			{
				return this._selectedSystem;
			}
		}

		private void SetSelectedSystem(int systemId, string trigger)
		{
			this._selectedSystem = systemId;
			this.App.UI.SetText("gameSystemTitle", string.Format("{0} System", (object)this.App.GameDatabase.GetStarSystemInfo(systemId).Name));
			StarSystemMapUI.Sync(this.App, systemId, "gameSystemMiniMap", false);
		}

		private void PopulateClassList(RealShipClasses? shipClass)
		{
			List<RealShipClasses> list = this.GetAllowedShipClasses().ToList<RealShipClasses>();
			this.App.UI.ClearItems(BuildScreenState.UIClassList);
			foreach (RealShipClasses shipClass1 in list)
			{
				if (this.GetAvailableDesigns(shipClass1).Count<DesignInfo>() > 0)
					this.App.UI.AddItem(BuildScreenState.UIClassList, string.Empty, (int)shipClass1, shipClass1.Localize());
			}
			this.App.UI.AddItem(BuildScreenState.UIClassList, string.Empty, -1, App.Localize("@UI_BUILD_FAVORITE_INVOICES"));
			this._selectedDesign = 0;
			if (shipClass.HasValue && !this.GetAvailableDesigns(shipClass.Value).Any<DesignInfo>())
			{
				int index = list.FindIndex((Predicate<RealShipClasses>)(x => this.GetAvailableDesigns(x).Any<DesignInfo>()));
				if (index < 0)
					this.SetSelectedShipClass(new RealShipClasses?(), string.Empty);
				else
					this.SetSelectedShipClass(new RealShipClasses?(list[index]), string.Empty);
			}
			else
				this.SetSelectedShipClass(shipClass, string.Empty);
		}

		private RealShipClasses? SelectedClass
		{
			get
			{
				return this._selectedClass;
			}
		}

		private void SetSelectedInvoice(int invoiceId)
		{
			this._invoiceItems.Clear();
			this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
			bool playSound = true;
			foreach (InvoiceBuildOrderInfo invoiceBuildOrderInfo in this.App.GameDatabase.GetInvoiceBuildOrders(invoiceId).ToList<InvoiceBuildOrderInfo>())
			{
				this._loacubeval = this.App.GameDatabase.GetDesignInfo(invoiceBuildOrderInfo.DesignID).IsLoaCube() ? invoiceBuildOrderInfo.LoaCubes : 0;
				this.AddOrder(invoiceBuildOrderInfo.DesignID, playSound, true);
				playSound = false;
			}
		}

		private void SetSelectedShipClass(RealShipClasses? id, string trigger)
		{
			RealShipClasses? selectedClass = this._selectedClass;
			RealShipClasses? nullable = id;
			if ((selectedClass.GetValueOrDefault() != nullable.GetValueOrDefault() ? 0 : (selectedClass.HasValue == nullable.HasValue ? 1 : 0)) != 0)
				return;
			this._selectedClass = id;
			if (trigger != BuildScreenState.UIClassList)
				this.App.UI.SetSelection(BuildScreenState.UIClassList, id.HasValue ? (int)id.Value : -1);
			this.PopulateDesignList(id);
		}

		private void PopulateDesignList(RealShipClasses? shipClass)
		{
			if (shipClass.HasValue)
			{
				this._designList = this.GetAvailableDesigns(shipClass.Value).ToList<DesignInfo>();
				BuildScreenState.PopulateDesignList(this.App, BuildScreenState.UIDesignList, (IEnumerable<DesignInfo>)this._designList);
				if (this._designList.Count <= 0)
					return;
				this.SetSelectedDesign(this._designList[0].ID, string.Empty);
			}
			else
			{
				this._invoiceList = this.App.GameDatabase.GetInvoiceInfosForPlayer(this.App.Game.LocalPlayer.ID).Where<InvoiceInfo>((Func<InvoiceInfo, bool>)(x => x.isFavorite)).ToList<InvoiceInfo>();
				ShipDesignUI.PopulateDesignList(this.App, BuildScreenState.UIDesignList, (IEnumerable<InvoiceInfo>)this._invoiceList);
				if (this._invoiceList.Count <= 0)
					return;
				this.SetFavInvoice(new int?(this._invoiceList[0].ID));
			}
		}

		private int SelectedDesign
		{
			get
			{
				return this._selectedDesign;
			}
		}

		private void SetSelectedDesign(int designId, string trigger)
		{
			if (trigger != BuildScreenState.UIDesignList)
			{
				if (designId == 0)
					this.App.UI.ClearSelection(BuildScreenState.UIDesignList);
				else
					this.App.UI.SetSelection(BuildScreenState.UIDesignList, designId);
			}
			if (this._selectedDesign == designId)
				return;
			this._selectedDesign = designId;
			if (designId == 0)
				return;
			DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(designId);
			this.App.UI.SetEnabled("gameAddDesignButton", true);
			this.App.UI.SetText("designNameTag", designInfo.Name);
			if (designInfo.isAttributesDiscovered)
			{
				IEnumerable<SectionEnumerations.DesignAttribute> attributesForDesign = this.App.GameDatabase.GetDesignAttributesForDesign(designInfo.ID);
				if (attributesForDesign.Count<SectionEnumerations.DesignAttribute>() > 0)
				{
					this.App.UI.SetVisible("attributeNameTagPanel", true);
					this.App.UI.SetText("attributeNameTagPanel.attributeNameTag", App.Localize("@UI_" + attributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString()));
					this.App.UI.SetTooltip("attributeNameTagPanel", App.Localize("@UI_" + attributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString() + "_TOOLTIP"));
				}
				else
					this.App.UI.SetVisible("attributeNameTagPanel", false);
			}
			else
				this.App.UI.SetVisible("attributeNameTagPanel", false);
			if (((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Count<DesignSectionInfo>() > 2)
			{
				this.App.UI.SetVisible("commandTag", true);
				this.App.UI.SetVisible("missionTag", true);
				this.App.UI.SetVisible("engineTag", true);
			}
			else if (((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Count<DesignSectionInfo>() > 1)
			{
				this.App.UI.SetVisible("commandTag", false);
				this.App.UI.SetVisible("missionTag", true);
				this.App.UI.SetVisible("engineTag", true);
			}
			else if (((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Count<DesignSectionInfo>() > 0)
			{
				this.App.UI.SetVisible("commandTag", false);
				this.App.UI.SetVisible("missionTag", true);
				this.App.UI.SetVisible("engineTag", false);
			}
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				switch (designSection.ShipSectionAsset.Type)
				{
					case ShipSectionType.Command:
						this.App.UI.SetText("commandTag", App.Localize(designSection.ShipSectionAsset.Title));
						break;
					case ShipSectionType.Mission:
						this.App.UI.SetText("missionTag", App.Localize(designSection.ShipSectionAsset.Title));
						break;
					case ShipSectionType.Engine:
						this.App.UI.SetText("engineTag", App.Localize(designSection.ShipSectionAsset.Title));
						break;
				}
			}
			if (!designInfo.isPrototyped)
			{
				this.App.UI.SetText("gameAddDesignButton", App.Localize("@UI_BUILD_PROTOTYPE_DESIGN"));
				this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_PROTOTYPE_DESIGN"));
			}
			else
			{
				this.App.UI.SetText("gameAddDesignButton", App.Localize("@UI_BUILD_ADD_TO_INVOICE"));
				this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_ADDTOBUILD"));
				this.App.UI.SetText("gameShipsProduced", this.App.GameDatabase.GetNumShipsBuiltFromDesign(this._selectedDesign).ToString());
				this.App.UI.SetText("gameShipsDestroyed", this.App.GameDatabase.GetNumShipsDestroyedFromDesign(this._selectedDesign).ToString());
				this.App.UI.SetText("gameDesignComissionHeader", string.Format(App.Localize("@UI_DESIGN_DATE_HEADER"), (object)designInfo.DesignDate));
			}
			this.SyncCost(this.App, designInfo);
			ShipDesignUI.SyncSupplies(this.App, designInfo);
			ShipDesignUI.SyncSpeed(this.App, designInfo);
			if (this._weaponTooltip == null)
				this._weaponTooltip = new WeaponHoverPanel(this.App.UI, "ShipInfo.WeaponPanel", "weaponInfo");
			if (this._moduleTooltip == null)
				this._moduleTooltip = new ModuleHoverPanel(this.App.UI, "ShipInfo.WeaponPanel", "moduleInfo");
			List<LogicalWeapon> source1 = new List<LogicalWeapon>();
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				foreach (WeaponBankInfo weaponBank in (IEnumerable<WeaponBankInfo>)designSection.WeaponBanks)
				{
					if (weaponBank.WeaponID.HasValue)
					{
						string weaponPath = this.App.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
						LogicalWeapon weapon = this.App.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponPath));
						if (weapon != null && source1.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weapon.FileName)).Count<LogicalWeapon>() == 0)
							source1.Add(weapon);
					}
				}
			}
			this._weaponTooltip.SetAvailableWeapons((IEnumerable<LogicalWeapon>)source1, true);
			List<LogicalModule> source2 = new List<LogicalModule>();
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				foreach (DesignModuleInfo module1 in (IEnumerable<DesignModuleInfo>)designSection.Modules)
				{
					string modulePath = this.App.GameDatabase.GetModuleAsset(module1.ModuleID);
					LogicalModule module = this.App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modulePath));
					if (module != null && source2.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == module.ModulePath)).Count<LogicalModule>() == 0)
						source2.Add(module);
				}
			}
			this._moduleTooltip.SetAvailableModules((IEnumerable<LogicalModule>)source2, (LogicalModule)null, false);
			this._builder.New(this.App.LocalPlayer, designInfo, designInfo.Name, 0, true);
		}

		private void SetFavInvoice(int? favInvoiceId)
		{
			this._selectedFavInvoice = favInvoiceId;
			if (!this._selectedFavInvoice.HasValue)
				return;
			List<InvoiceBuildOrderInfo> list = this.App.GameDatabase.GetInvoiceBuildOrders(this._selectedFavInvoice.Value).ToList<InvoiceBuildOrderInfo>();
			if (list.Count <= 0)
				return;
			this.SetSelectedDesign(list[0].DesignID, BuildScreenState.UIDesignList);
		}

		private List<DesignInfo> GetAdditionalShipDesigns(GameDatabase db, int playerID)
		{
			List<DesignInfo> designInfoList = new List<DesignInfo>();
			foreach (InvoiceInstanceInfo invoiceInstanceInfo in db.GetInvoicesForSystem(playerID, this._selectedSystem).ToList<InvoiceInstanceInfo>())
				designInfoList.AddRange(db.GetBuildOrdersForInvoiceInstance(invoiceInstanceInfo.ID).Select<BuildOrderInfo, DesignInfo>((Func<BuildOrderInfo, DesignInfo>)(x => db.GetDesignInfo(x.DesignID))));
			designInfoList.AddRange(this._invoiceItems.Select<BuildScreenState.InvoiceItem, DesignInfo>((Func<BuildScreenState.InvoiceItem, DesignInfo>)(x => db.GetDesignInfo(x.DesignID))));
			return designInfoList;
		}

		public void SyncFinancialDetails(App game)
		{
			Budget budget = Budget.GenerateBudget(game.Game, game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID), (IEnumerable<DesignInfo>)this.GetAdditionalShipDesigns(game.GameDatabase, game.LocalPlayer.ID), BudgetProjection.Pessimistic);
			this.App.UI.SetPropertyString(BuildScreenState.UICurrentMaintenance, "text", budget.CurrentShipUpkeepExpenses.ToString("N0"));
			this.App.UI.SetPropertyString(BuildScreenState.UIProjectedCost, "text", budget.AdditionalUpkeepExpenses.ToString("N0"));
			this.App.UI.SetPropertyString(BuildScreenState.UITotalMaintenance, "text", budget.UpkeepExpenses.ToString("N0"));
			this._piechart.SetSlices(budget);
		}

		public void SyncConstructionSite(App app)
		{
			double constructionRate = 0.0;
			double totalRevenue = 0.0;
			this._totalShipProductionRate = 0.0f;
			BuildScreenState.ObtainConstructionCosts(out this._totalShipProductionRate, out constructionRate, out totalRevenue, app, this._selectedSystem, app.LocalPlayer.ID);
			app.UI.SetPropertyString(BuildScreenState.UISysProductionValue, "text", this._totalShipProductionRate.ToString("N0"));
			app.UI.SetPropertyString(BuildScreenState.UISysIncomeValue, "text", totalRevenue.ToString("N0"));
			app.UI.SetPropertyString(BuildScreenState.UICvsTValue, "text", string.Format("{0}%", (object)constructionRate.ToString("N0")));
		}

		public static void ObtainConstructionCosts(
		  out float productionRate,
		  out double constructionRate,
		  out double totalRevenue,
		  App app,
		  int systemID,
		  int playerID)
		{
			productionRate = 0.0f;
			constructionRate = 0.0;
			totalRevenue = 0.0;
			List<ColonyInfo> colonyInfoList = new List<ColonyInfo>();
			foreach (int orbitalObjectID in app.GameDatabase.GetStarSystemPlanets(systemID).ToList<int>())
			{
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectID);
				if (colonyInfoForPlanet != null)
				{
					totalRevenue += Colony.GetTaxRevenue(app, colonyInfoForPlanet);
					constructionRate += (double)colonyInfoForPlanet.ShipConRate;
					productionRate += Colony.GetConstructionPoints(app.Game, colonyInfoForPlanet);
					colonyInfoList.Add(colonyInfoForPlanet);
				}
			}
			productionRate *= app.Game.GetStationBuildModifierForSystem(systemID, playerID);
			constructionRate = constructionRate * 100.0 / (double)colonyInfoList.Count;
		}

		public void SyncCost(App app, DesignInfo design)
		{
			if (design == null)
				return;
			if (design.isPrototyped || this._invoiceItems.Any<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x => x.DesignID == design.ID)))
			{
				app.UI.SetVisible("ShipCost", false);
				app.UI.SetVisible("ShipProductionCost", true);
				this.SyncProductionCost(app, "ShipProductionCost", design);
			}
			else
			{
				app.UI.SetVisible("ShipCost", true);
				app.UI.SetVisible("ShipProductionCost", false);
				ShipDesignUI.SyncCost(app, "ShipCost", design);
			}
		}

		public static int ParseId(string msgParam)
		{
			if (string.IsNullOrEmpty(msgParam))
				return 0;
			return int.Parse(msgParam);
		}

		protected override void OnUpdate()
		{
			this._builder.Update();
			if (this._builder.Ship == null || this._builder.Loading || (!this._builder.Ship.Active || this._camera.TargetID == this._builder.Ship.ObjectID))
				return;
			this._camera.TargetID = this._builder.Ship.ObjectID;
			this._shipHoloView.SetUseViewport(true);
			this._shipHoloView.SetShip(this._builder.Ship);
		}

		public override bool IsReady()
		{
			if (this._crits != null && this._crits.IsReady())
				return base.IsReady();
			return false;
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		private void AddOrder(int designId, bool playSound = true, bool bypassLoa = false)
		{
			if (designId == 0)
				return;
			DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(designId);
			if (designInfo.IsLoaCube() && (!bypassLoa || this._loacubeval <= 0))
			{
				this.App.UI.SetVisible("LoaCubeDialog", true);
			}
			else
			{
				++this.HACK_OrderID;
				designInfo = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(designInfo, this.App.GameDatabase.GetDesignInfosForPlayer(this.App.LocalPlayer.ID));
				string str = designInfo.Name ?? "USS Placeholder";
				if (this._invoiceItems.Count > 0 && this._invoiceItems[0].isPrototypeOrder)
					return;
				if (!designInfo.isPrototyped)
				{
					this.App.UI.SetEnabled("gameAddDesignButton", false);
					this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_PROTOTYPE_ALREADY"));
					if (this._invoiceItems.Count > 0 || this.App.GameDatabase.GetDesignBuildOrders(designInfo).Count<BuildOrderInfo>() > 0)
						return;
				}
				else
				{
					this.App.UI.SetEnabled("gameAddDesignButton", true);
					this.App.UI.SetTooltip("gameAddDesignButton", App.Localize("@UI_TOOLTIP_ADDTOBUILD"));
				}
				List<InvoiceInstanceInfo> list = this.App.GameDatabase.GetInvoicesForSystem(this.App.LocalPlayer.ID, this._selectedSystem).ToList<InvoiceInstanceInfo>();
				this._invoiceItems.Add(new BuildScreenState.InvoiceItem()
				{
					DesignID = designId,
					TempOrderID = this.HACK_OrderID,
					ShipName = str,
					Progress = -1,
					isPrototypeOrder = !designInfo.isPrototyped && !this._invoiceItems.Any<BuildScreenState.InvoiceItem>((Func<BuildScreenState.InvoiceItem, bool>)(x =>
				   {
					   if (x.DesignID == designInfo.ID)
						   return x.isPrototypeOrder;
					   return false;
				   })) && !list.Any<InvoiceInstanceInfo>((Func<InvoiceInstanceInfo, bool>)(x => this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(x.ID).Any<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(y => y.DesignID == designInfo.ID)))),
					LoaCubes = designInfo.IsLoaCube() ? this._loacubeval : 0
				});
				if (playSound)
					this.App.PostRequestGuiSound("build_addtoinvoice");
				this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
				this.SyncCost(this.App, this.App.GameDatabase.GetDesignInfo(designId));
				this.SyncFinancialDetails(this.App);
				this.App.UI.SetEnabled(BuildScreenState.UISubmitOrder, this._invoiceItems.Count != 0);
			}
		}

		public static void PopulateDesignList(
		  App game,
		  string designListId,
		  IEnumerable<DesignInfo> designs)
		{
			game.UI.ClearItems(designListId);
			foreach (DesignInfo design in designs)
			{
				if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(game, design) && !design.IsAccelerator() && BuildScreenState.IsShipRoleAllowed(design.Role))
				{
					game.UI.AddItem(designListId, string.Empty, design.ID, design.Name);
					if (game.LocalPlayer.Faction.Name == "loa" && !design.IsLoaCube())
						game.UI.SetItemPropertyString(designListId, string.Empty, design.ID, "designName", "text", design.Name + "  [" + ((float)design.GetPlayerProductionCost(game.GameDatabase, design.PlayerID, !design.isPrototyped, new float?()) / 1000f).ToString("0.0K") + "]");
					else
						game.UI.SetItemPropertyString(designListId, string.Empty, design.ID, "designName", "text", design.Name);
					if (design.IsLoaCube())
					{
						string itemGlobalId = game.UI.GetItemGlobalID(designListId, string.Empty, design.ID, "");
						game.UI.SetVisible(game.UI.Path(itemGlobalId, "designDeleteButton"), false);
					}
					game.UI.SetItemPropertyString(designListId, string.Empty, design.ID, "designDeleteButton", "id", "designDeleteButton|" + design.ID.ToString());
					if (!design.isPrototyped)
					{
						if (game.GameDatabase.GetDesignBuildOrders(design).ToList<BuildOrderInfo>().Count > 0)
							game.UI.SetItemPropertyColor(designListId, string.Empty, design.ID, "designName", "color", new Vector3(0.0f, 80f, 104f));
						else
							game.UI.SetItemPropertyColor(designListId, string.Empty, design.ID, "designName", "color", new Vector3(147f, 64f, 147f));
					}
					else
						game.UI.SetItemPropertyColor(designListId, string.Empty, design.ID, "designName", "color", new Vector3(11f, 157f, 194f));
				}
			}
		}

		public static int GetBuildInvoiceCost(App app, List<BuildScreenState.InvoiceItem> items)
		{
			int num = 0;
			foreach (BuildScreenState.InvoiceItem invoiceItem in items)
			{
				DesignInfo designInfo = app.GameDatabase.GetDesignInfo(invoiceItem.DesignID);
				num += BuildScreenState.GetDesignCost(app, designInfo, invoiceItem.LoaCubes);
			}
			return num;
		}

		public static int GetDesignCost(App app, DesignInfo shipDesign, int loaCubes)
		{
			if (shipDesign == null)
				return 0;
			int num = shipDesign.SavingsCost;
			if (shipDesign.IsLoaCube())
				num = loaCubes * app.AssetDatabase.LoaCostPerCube;
			else if (!shipDesign.isPrototyped)
			{
				switch (shipDesign.Class)
				{
					case ShipClass.Cruiser:
						num = (int)((double)shipDesign.SavingsCost * (double)app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierCR, shipDesign.PlayerID));
						break;
					case ShipClass.Dreadnought:
						num = (int)((double)shipDesign.SavingsCost * (double)app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierDN, shipDesign.PlayerID));
						break;
					case ShipClass.Leviathan:
						num = (int)((double)shipDesign.SavingsCost * (double)app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierLV, shipDesign.PlayerID));
						break;
					case ShipClass.Station:
						RealShipClasses? realShipClass = shipDesign.GetRealShipClass();
						if ((realShipClass.GetValueOrDefault() != RealShipClasses.Platform ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
						{
							num = (int)((double)shipDesign.SavingsCost * (double)app.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierPF, shipDesign.PlayerID));
							break;
						}
						break;
				}
			}
			return num;
		}

		private void SyncInvoiceItemsList(
		  string parentPanel,
		  string listPanel,
		  List<BuildScreenState.InvoiceItem> items,
		  string title,
		  bool retrofitinvoice = false,
		  int shipid = 0)
		{
			string listId = this.App.UI.Path(parentPanel, listPanel);
			this.App.UI.SetPropertyString(BuildScreenState.UIInvoiceSummaryName, "text", title);
			this.App.UI.SetVisible("gameRandomizeSummaryShipNames", !retrofitinvoice);
			this.App.UI.ClearItems(listId);
			foreach (BuildScreenState.InvoiceItem invoiceItem in items)
			{
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(invoiceItem.DesignID);
				string propertyValue = string.Format("{0}{1}", (object)listPanel, (object)invoiceItem.TempOrderID);
				string name = designInfo.Name;
				string str = designInfo.GetRealShipClass().LocalizeAbbr();
				this.App.UI.AddItem(listId, string.Empty, invoiceItem.TempOrderID, string.Empty);
				this.App.UI.SetItemPropertyString(listId, string.Empty, invoiceItem.TempOrderID, "class", "text", str);
				this.App.UI.SetItemPropertyString(listId, string.Empty, invoiceItem.TempOrderID, "design", "text", invoiceItem.isPrototypeOrder ? string.Format("{0} {1}", (object)App.Localize("@UI_BUILD_PROTOTYPE"), (object)name) : name);
				this.App.UI.SetItemPropertyString(listId, string.Empty, invoiceItem.TempOrderID, "name", "text", invoiceItem.ShipName);
				this.App.UI.SetItemPropertyString(listId, string.Empty, invoiceItem.TempOrderID, "progress", "text", invoiceItem.Progress == -1 ? "" : string.Format("{0}%", (object)invoiceItem.Progress));
				string itemGlobalId = this.App.UI.GetItemGlobalID(listId, string.Empty, invoiceItem.TempOrderID, string.Empty);
				this.App.UI.SetEnabled(this.App.UI.Path(itemGlobalId, "name"), (!retrofitinvoice ? 1 : 0) != 0);
				this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "name"), "id", propertyValue);
			}
			if (items.Count > 0)
			{
				if (!retrofitinvoice)
				{
					int buildInvoiceCost = BuildScreenState.GetBuildInvoiceCost(this.App, items);
					int buildTime = BuildScreenState.GetBuildTime(this.App, (IEnumerable<BuildScreenState.InvoiceItem>)items, this._totalShipProductionRate);
					string text = buildTime == 1 ? string.Format("1 {0}", (object)App.Localize("@UI_GENERAL_TURN")) : string.Format("{0} {1}", (object)buildTime, (object)App.Localize("@UI_GENERAL_TURNS"));
					if (buildTime == 0)
						text = "∞";
					this.App.UI.SetText(this.App.UI.Path(parentPanel, BuildScreenState.UIInvoiceTotalSavings), buildInvoiceCost.ToString("N0"));
					this.App.UI.SetText(this.App.UI.Path(parentPanel, BuildScreenState.UIInvoiceTotalTurns), text);
					this.App.UI.SetEnabled(this.App.UI.Path(parentPanel, BuildScreenState.UISubmitOrder), true);
				}
				else
				{
					ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(shipid, true);
					if (shipInfo == null)
						return;
					int retrofitCost = Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, shipInfo.DesignInfo, Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(shipInfo.DesignInfo, this.App.GameDatabase.GetVisibleDesignInfosForPlayer(this.App.LocalPlayer.ID)));
					double requiredToRetrofit = Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, shipInfo, items.Count);
					string text = requiredToRetrofit == 1.0 ? string.Format("1 {0}", (object)App.Localize("@UI_GENERAL_TURN")) : string.Format("{0} {1}", (object)requiredToRetrofit, (object)App.Localize("@UI_GENERAL_TURNS"));
					if (requiredToRetrofit == 0.0)
						text = "∞";
					this.App.UI.SetText(this.App.UI.Path(parentPanel, BuildScreenState.UIInvoiceTotalSavings), retrofitCost.ToString("N0"));
					this.App.UI.SetText(this.App.UI.Path(parentPanel, BuildScreenState.UIInvoiceTotalTurns), text);
					this.App.UI.SetEnabled(this.App.UI.Path(parentPanel, BuildScreenState.UISubmitOrder), true);
				}
			}
			else
			{
				this.App.UI.SetText(this.App.UI.Path(parentPanel, BuildScreenState.UIInvoiceTotalSavings), "0");
				this.App.UI.SetText(this.App.UI.Path(parentPanel, BuildScreenState.UIInvoiceTotalTurns), "0");
				this.App.UI.SetEnabled(this.App.UI.Path(parentPanel, BuildScreenState.UISubmitOrder), false);
			}
		}

		private void SyncProductionCost(App game, string panel, DesignInfo design)
		{
			string text1 = design.IsLoaCube() ? (this._loacubeval * game.AssetDatabase.LoaCostPerCube).ToString("N0") : design.SavingsCost.ToString("N0");
			string text2 = design.GetPlayerProductionCost(game.GameDatabase, game.LocalPlayer.ID, !design.isPrototyped, design.IsLoaCube() ? new float?((float)this._loacubeval) : new float?()).ToString("N0");
			string text3 = GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, false).ToString("N0");
			string text4 = string.Format("({0})", (object)GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, true).ToString("N0"));
			game.UI.SetText(game.UI.Path(panel, "gameShipSavCost"), text1);
			game.UI.SetText(game.UI.Path(panel, "gameShipConCost"), text2);
			game.UI.SetText(game.UI.Path(panel, "gameShipUpkeepCost"), text3);
			game.UI.SetText(game.UI.Path(panel, "gameShipResUpkeepCost"), text4);
		}

		private void SubmitOrder(string invoiceName)
		{
			if (this._invoiceItems.Count == 0)
				return;
			int invoiceId = this.App.GameDatabase.InsertInvoice(invoiceName, this.App.LocalPlayer.ID, this._addToFavorites);
			int num = this.App.GameDatabase.InsertInvoiceInstance(this.App.LocalPlayer.ID, this._selectedSystem, invoiceName);
			foreach (BuildScreenState.InvoiceItem invoiceItem in this._invoiceItems)
			{
				this.App.GameDatabase.InsertInvoiceBuildOrder(invoiceId, invoiceItem.DesignID, invoiceItem.ShipName, invoiceItem.LoaCubes);
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(invoiceItem.DesignID);
				bool requiresPrototype;
				if (this.App.GameDatabase.canBuildDesignOrder(designInfo, this._selectedSystem, out requiresPrototype))
				{
					this.App.GameDatabase.InsertBuildOrder(this._selectedSystem, invoiceItem.DesignID, 0, 0, invoiceItem.ShipName, designInfo.IsLoaCube() ? invoiceItem.LoaCubes : designInfo.GetPlayerProductionCost(this.App.GameDatabase, this.App.LocalPlayer.ID, requiresPrototype, new float?()), new int?(num), new int?(), invoiceItem.LoaCubes);
					if (requiresPrototype)
						this.App.PostRequestSpeech(string.Format("STRAT_037-01_{0}_PrototypeOrderConfirm", (object)this.App.LocalPlayer.Faction.Name), 50, 120, 0.0f);
					else
						this.App.PostRequestSpeech(string.Format("STRAT_033-01_{0}_OrderToBuildShipsConfirmation", (object)this.App.LocalPlayer.Faction.Name), 50, 120, 0.0f);
				}
			}
			this.App.PostRequestGuiSound("build_submitorder");
			this._invoiceItems.Clear();
			this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
			this.PopulateInvoiceList();
			this.PopulateClassList(this._selectedClass);
			this.PopulateDesignList(this._selectedClass);
			this.SyncInvoiceItemsList(BuildScreenState.UIOrderInvoiceItems, BuildScreenState.UIInvoiceItemList, this._invoiceItems, App.Localize("@UI_BUILD_NEW_INVOICE"), false, 0);
			this._addToFavorites = false;
			this.App.UI.SetChecked(BuildScreenState.UIAddToInvoiceFavorites, false);
			this.App.UI.SetEnabled(BuildScreenState.UISubmitOrder, this._invoiceItems.Count != 0);
		}

		public static int GetBuildTime(
		  App app,
		  IEnumerable<BuildScreenState.InvoiceItem> DesignIds,
		  float productionRate)
		{
			float num1 = productionRate;
			if ((double)productionRate < 1.0)
				return 0;
			float modifierFloatToApply = app.GameDatabase.GetStratModifierFloatToApply(StratModifiers.PrototypeTimeModifier, app.LocalPlayer.ID);
			int num2 = 0;
			foreach (BuildScreenState.InvoiceItem designId in DesignIds)
			{
				DesignInfo designInfo = app.GameDatabase.GetDesignInfo(designId.DesignID);
				int num3 = designInfo.GetPlayerProductionCost(app.GameDatabase, app.LocalPlayer.ID, designId.isPrototypeOrder, new float?());
				if (designInfo.IsLoaCube())
					num3 = designInfo.GetPlayerProductionCost(app.GameDatabase, app.LocalPlayer.ID, designId.isPrototypeOrder, new float?((float)designId.LoaCubes));
				if (designId.isPrototypeOrder)
					num3 = (int)((double)num3 * (double)modifierFloatToApply);
				num2 += num3;
			}
			return (int)Math.Ceiling((double)num2 / (double)num1);
		}

		private void PopulateInvoiceList()
		{
			List<InvoiceInstanceInfo> list1 = this.App.GameDatabase.GetInvoicesForSystem(this.App.LocalPlayer.ID, this._selectedSystem).ToList<InvoiceInstanceInfo>();
			this.App.UI.ClearItems(BuildScreenState.UIInvoiceList);
			foreach (InvoiceInstanceInfo invoiceInstanceInfo in list1)
			{
				List<BuildOrderInfo> list2 = this.App.GameDatabase.GetBuildOrdersForInvoiceInstance(invoiceInstanceInfo.ID).ToList<BuildOrderInfo>();
				if (list2 != null && list2.Count > 0)
				{
					float shipProductionRate = this._totalShipProductionRate;
					int num1 = 0;
					int num2 = 0;
					foreach (BuildOrderInfo buildOrderInfo in list2)
					{
						num2 += buildOrderInfo.Progress;
						num1 += buildOrderInfo.ProductionTarget;
					}
					int num3 = (int)Math.Ceiling((double)(num1 - num2) / (double)shipProductionRate);
					this.App.UI.AddItem(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, string.Empty);
					this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoiceName", "text", invoiceInstanceInfo.Name);
					if ((double)shipProductionRate < 1.0)
						this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoiceTime", "text", string.Format("-"));
					else if (num3 != 1)
						this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoiceTime", "text", string.Format("{0} {1}", (object)num3, (object)App.Localize("@UI_GENERAL_TURNS")));
					else
						this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoiceTime", "text", string.Format("{0} {1}", (object)num3, (object)App.Localize("@UI_GENERAL_TURN")));
					string itemGlobalId = this.App.UI.GetItemGlobalID(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, string.Empty);
					if (num2 > 0)
					{
						float num4 = (float)num2 * 100f / (float)num1;
						this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "invoicePercent"), true);
						this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoicePercent", "text", string.Format("{0:0}%", (object)num4));
					}
					else
						this.App.UI.SetVisible(this.App.UI.Path(itemGlobalId, "invoicePercent"), false);
				}
				List<RetrofitOrderInfo> list3 = this.App.GameDatabase.GetRetrofitOrdersForInvoiceInstance(invoiceInstanceInfo.ID).ToList<RetrofitOrderInfo>();
				if (list3 != null && list3.Count > 0)
				{
					foreach (RetrofitOrderInfo retrofitOrderInfo in list3)
						;
					double requiredToRetrofit = Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(this.App, this.App.GameDatabase.GetShipInfo(list3[0].ShipID, true), list3.Count);
					this.App.UI.AddItem(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, string.Empty);
					this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoiceName", "text", invoiceInstanceInfo.Name);
					if (requiredToRetrofit != 1.0)
						this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoiceTime", "text", string.Format("{0} {1}", (object)requiredToRetrofit, (object)App.Localize("@UI_GENERAL_TURNS")));
					else
						this.App.UI.SetItemPropertyString(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, "invoiceTime", "text", string.Format("{0} {1}", (object)requiredToRetrofit, (object)App.Localize("@UI_GENERAL_TURN")));
					this.App.UI.SetVisible(this.App.UI.Path(this.App.UI.GetItemGlobalID(BuildScreenState.UIInvoiceList, string.Empty, invoiceInstanceInfo.ID, string.Empty), "invoicePercent"), false);
				}
			}
		}

		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(this.Name) && this.App.UI.GetTopDialog() == null && !this._confirmInvoiceDialogActive)
			{
				switch (action)
				{
					case HotKeyManager.HotKeyActions.State_Starmap:
						this._invoiceItems.Clear();
						this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
						return true;
					case HotKeyManager.HotKeyActions.State_BuildScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_DesignScreen:
						this._invoiceItems.Clear();
						this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						this.App.UI.LockUI();
						this.App.SwitchGameState<DesignScreenState>((object)false, (object)this.Name);
						return true;
					case HotKeyManager.HotKeyActions.State_ResearchScreen:
						this._invoiceItems.Clear();
						this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						this.App.UI.LockUI();
						this.App.SwitchGameState<ResearchScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						this._invoiceItems.Clear();
						this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						this.App.UI.LockUI();
						this.App.SwitchGameState<EmpireSummaryState>();
						return true;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						this._invoiceItems.Clear();
						this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						this.App.UI.LockUI();
						this.App.SwitchGameState<SotspediaState>();
						return true;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						this._invoiceItems.Clear();
						this.App.UI.ClearItems(BuildScreenState.UIInvoiceItemList);
						this.App.UI.LockUI();
						this.App.SwitchGameState<DiplomacyScreenState>();
						return true;
				}
			}
			return false;
		}

		public class InvoiceItem
		{
			public int DesignID;
			public int TempOrderID;
			public string ShipName;
			public int Progress;
			public bool isPrototypeOrder;
			public int LoaCubes;
		}
	}
}
