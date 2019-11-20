// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogLoaFleetCompositor
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DialogLoaFleetCompositor : Dialog
	{
		private string namedialog = "";
		private string invoicename = "IHaveNoIdeaWhatImDoing";
		private const string designlist = "gameFleetList";
		private const string workingdesignlist = "gameWorkingFleet";
		private const string okbtn = "okButton";
		private const string singleokbtn = "single_okButton";
		private const string cancelbtn = "cancelButton";
		private const string UIClassDDL = "gameClassList";
		private const string UIMissingRole = "requirement";
		private const string UICommandPoints = "CommandPointValue";
		private const string UIConstructionPoint = "ConstructionPointValue";
		private const string UIRiderValue = "BattleRiderValue";
		private Dictionary<int, int> SelectedDesigns;
		private Dictionary<int, int> ListDesignMap;
		private List<DialogLoaFleetCompositor.RiderStruct> RiderListMap;
		private DesignDetailsCard DesignDetailCard;
		private RealShipClasses SelectedClass;
		private MissionType _mission;
		private int _outputID;
		private static int _WorkingDesignListid;

		public DialogLoaFleetCompositor(App app, MissionType mission)
		  : base(app, "dialogLoaFleetCompositor")
		{
			this._mission = mission;
		}

		public override void Initialize()
		{
			this.DesignDetailCard = new DesignDetailsCard(this._app, this._app.GameDatabase.GetDesignInfosForPlayer(this._app.LocalPlayer.ID, RealShipClasses.Cruiser, true).First<DesignInfo>().ID, new int?(), this.UI, this._app.UI.Path(this.ID, "DesignDetailsCard"));
			this.SelectedDesigns = new Dictionary<int, int>();
			this.ListDesignMap = new Dictionary<int, int>();
			this.RiderListMap = new List<DialogLoaFleetCompositor.RiderStruct>();
			this.PopulateClassList(RealShipClasses.Cruiser);
			this.CheckMissionRequirements();
			this.SyncCompositionStats();
		}

		protected void SyncDesignListList(RealShipClasses shipclass)
		{
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, "gameFleetList"));
			bool flag = false;
			foreach (DesignInfo availableDesign in this.GetAvailableDesigns(shipclass))
			{
				this._app.UI.AddItem(this._app.UI.Path(this.ID, "gameFleetList"), "", availableDesign.ID, availableDesign.Name);
				string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "gameFleetList"), "", availableDesign.ID, "");
				if (!flag)
				{
					this._app.UI.SetSelection(this._app.UI.Path(this.ID, "gameFleetList"), availableDesign.ID);
					flag = true;
				}
				this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId, "designName"), "color", new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue));
				string str = "";
				if (availableDesign.Class == ShipClass.BattleRider)
				{
					str = "   " + App.Localize(availableDesign.GetMissionSectionAsset().Title);
					if (!this.CanMountBattleRider(availableDesign))
						this._app.UI.SetPropertyColor(this._app.UI.Path(itemGlobalId, "designName"), "color", new Vector3((float)byte.MaxValue, 0.0f, 0.0f));
				}
				this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "designName"), availableDesign.Name + "  [" + ((float)availableDesign.GetPlayerProductionCost(this._app.GameDatabase, availableDesign.PlayerID, !availableDesign.isPrototyped, new float?()) / 1000f).ToString("0.0K") + "]" + str);
				this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "designDeleteButton"), false);
			}
		}

		protected void SyncDesignWorkingList()
		{
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, "gameWorkingFleet"));
			this.ListDesignMap.Clear();
			foreach (int key in this.SelectedDesigns.Keys)
			{
				for (int index = 0; index < this.SelectedDesigns[key]; ++index)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(key);
					this._app.UI.AddItem(this._app.UI.Path(this.ID, "gameWorkingFleet"), "", DialogLoaFleetCompositor._WorkingDesignListid, designInfo.Name);
					string itemGlobalId = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "gameWorkingFleet"), "", DialogLoaFleetCompositor._WorkingDesignListid, "");
					this._app.UI.SetText(this._app.UI.Path(itemGlobalId, "designName"), designInfo.Name);
					this._app.UI.SetVisible(this._app.UI.Path(itemGlobalId, "designDeleteButton"), true);
					this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId, "designDeleteButton"), "id", "designDeleteButton|" + designInfo.ID.ToString() + "|" + DialogLoaFleetCompositor._WorkingDesignListid.ToString() + "|" + index.ToString());
					this._app.UI.SetPropertyString(itemGlobalId, "id", designInfo.ID.ToString() + "|" + index.ToString());
					this.ListDesignMap.Add(DialogLoaFleetCompositor._WorkingDesignListid, designInfo.ID);
					++DialogLoaFleetCompositor._WorkingDesignListid;
				}
			}
			this.CheckMissionRequirements();
			this.SyncCompositionStats();
		}

		protected void SyncCompositionStats()
		{
			List<DesignInfo> designInfoList = new List<DesignInfo>();
			foreach (int key in this.SelectedDesigns.Keys)
			{
				for (int index = 0; index < this.SelectedDesigns[key]; ++index)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(key);
					designInfoList.Add(designInfo);
				}
			}
			int num1 = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (DesignInfo designInfo in designInfoList)
			{
				num2 += designInfo.CommandPointCost;
				if (this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, designInfo.ID) > num3)
					num3 = this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, designInfo.ID);
				num1 += designInfo.GetPlayerProductionCost(this._app.GameDatabase, this._app.LocalPlayer.ID, false, new float?());
			}
			this._app.UI.SetText(this._app.UI.Path(this.ID, "CommandPointValue"), num2.ToString("N0") + "/" + num3.ToString("N0"));
			int cubeMassForTransit = Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(this._app.Game, this._app.LocalPlayer.ID);
			this._app.UI.SetText(this._app.UI.Path(this.ID, "ConstructionPointValue"), num1.ToString("N0") + "/" + cubeMassForTransit.ToString("N0"));
			int num4 = 0;
			int num5 = 0;
			foreach (DialogLoaFleetCompositor.RiderStruct riderList in this.RiderListMap)
			{
				foreach (DialogLoaFleetCompositor.RiderWingStruct riderWingStruct in riderList.WingData)
				{
					num5 += riderWingStruct.wingdata.SlotIndexes.Count;
					num4 += riderWingStruct.riders.Count;
				}
			}
			this._app.UI.SetText(this._app.UI.Path(this.ID, "BattleRiderValue"), num4.ToString() + "/" + num5.ToString());
		}

		protected override void OnUpdate()
		{
		}

		private void commitdesigns()
		{
			List<int> intList = new List<int>();
			foreach (int key in this.SelectedDesigns.Keys)
			{
				for (int index = 0; index < this.SelectedDesigns[key]; ++index)
					intList.Add(key);
			}
			this._outputID = this._app.GameDatabase.InsertLoaFleetComposition(this._app.LocalPlayer.ID, this.invoicename, (IEnumerable<int>)intList);
		}

		private void PopulateClassList(RealShipClasses shipClass)
		{
			List<RealShipClasses> list = this.GetAllowedShipClasses().ToList<RealShipClasses>();
			this._app.UI.ClearItems("gameClassList");
			foreach (RealShipClasses shipClass1 in list)
			{
				if (this.GetAvailableDesigns(shipClass1).Count<DesignInfo>() > 0)
					this._app.UI.AddItem("gameClassList", string.Empty, (int)shipClass1, shipClass1.Localize());
			}
			this.SelectedClass = shipClass;
			this._app.UI.SetSelection("gameClassList", (int)shipClass);
			this.SyncDesignListList(shipClass);
		}

		private IEnumerable<DesignInfo> GetAvailableDesigns(
		  RealShipClasses shipClass)
		{
			IEnumerable<DesignInfo> designInfosForPlayer = this._app.GameDatabase.GetVisibleDesignInfosForPlayer(this._app.LocalPlayer.ID, shipClass);
			List<DesignInfo> list = designInfosForPlayer.ToList<DesignInfo>();
			foreach (DesignInfo designInfo in designInfosForPlayer)
			{
				if (!this.IsDesignAllowed(designInfo) || !Kerberos.Sots.StarFleet.StarFleet.IsNewestRetrofit(designInfo, designInfosForPlayer))
					list.Remove(designInfo);
			}
			return (IEnumerable<DesignInfo>)list;
		}

		private IEnumerable<RealShipClasses> GetAllowedShipClasses()
		{
			foreach (RealShipClasses realShipClass in ShipClassExtensions.RealShipClasses)
			{
				if (DialogLoaFleetCompositor.IsShipClassAllowed(new RealShipClasses?(realShipClass)))
					yield return realShipClass;
			}
		}

		private bool IsDesignAllowed(DesignInfo designInfo)
		{
			if (DialogLoaFleetCompositor.IsShipClassAllowed(designInfo.GetRealShipClass()) && DialogLoaFleetCompositor.IsShipRoleAllowed(designInfo.Role) && !Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(this._app, designInfo))
				return designInfo.isPrototyped;
			return false;
		}

		public static bool IsShipRoleAllowed(ShipRole role)
		{
			switch (role)
			{
				case ShipRole.BOARDINGPOD:
				case ShipRole.BIOMISSILE:
				case ShipRole.TRAPDRONE:
				case ShipRole.ACCELERATOR_GATE:
				case ShipRole.LOA_CUBE:
					return false;
				default:
					return true;
			}
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
					return true;
				case RealShipClasses.Drone:
				case RealShipClasses.BoardingPod:
				case RealShipClasses.EscapePod:
				case RealShipClasses.AssaultShuttle:
				case RealShipClasses.Biomissile:
				case RealShipClasses.Station:
				case RealShipClasses.Platform:
				case RealShipClasses.SystemDefenseBoat:
				case RealShipClasses.NumShipClasses:
					return false;
				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}
		}

		private void CheckMissionRequirements()
		{
			ShipRole shipRole = this.CheckRequiredShips();
			List<DesignInfo> designInfoList = new List<DesignInfo>();
			foreach (int key in this.SelectedDesigns.Keys)
			{
				for (int index = 0; index < this.SelectedDesigns[key]; ++index)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(key);
					designInfoList.Add(designInfo);
				}
			}
			int num1 = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (DesignInfo designInfo in designInfoList)
			{
				num2 += designInfo.CommandPointCost;
				if (this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, designInfo.ID) > num3)
					num3 = this._app.GameDatabase.GetDesignCommandPointQuota(this._app.AssetDatabase, designInfo.ID);
				num1 += designInfo.GetPlayerProductionCost(this._app.GameDatabase, this._app.LocalPlayer.ID, false, new float?());
			}
			if (num3 >= num2)
			{
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "okButton"), false);
				switch (shipRole)
				{
					case ShipRole.COMMAND:
						this._app.UI.SetText(this._app.UI.Path(this.ID, "requirement"), "Fleet Requires Command Ship");
						break;
					case ShipRole.COLONIZER:
						this._app.UI.SetText(this._app.UI.Path(this.ID, "requirement"), "Fleet Requires Colonizer");
						break;
					case ShipRole.CONSTRUCTOR:
						this._app.UI.SetText(this._app.UI.Path(this.ID, "requirement"), "Fleet Requires Construction Ship");
						break;
					default:
						this._app.UI.SetText(this._app.UI.Path(this.ID, "requirement"), "");
						this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "okButton"), true);
						break;
				}
			}
			else
			{
				if (num3 >= num2)
					return;
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "okButton"), false);
				this._app.UI.SetText(this._app.UI.Path(this.ID, "requirement"), "Not Enough CP to support Fleet");
			}
		}

		private ShipRole CheckRequiredShips()
		{
			List<ShipRole> source = new List<ShipRole>();
			source.Add(ShipRole.COMMAND);
			if (this._mission == MissionType.CONSTRUCT_STN || this._mission == MissionType.UPGRADE_STN || this._mission == MissionType.SPECIAL_CONSTRUCT_STN)
				source.Add(ShipRole.CONSTRUCTOR);
			if (this._mission == MissionType.COLONIZATION || this._mission == MissionType.SUPPORT || this._mission == MissionType.EVACUATE)
				source.Add(ShipRole.COLONIZER);
			foreach (int key in this.SelectedDesigns.Keys)
			{
				if (this.SelectedDesigns[key] > 0)
				{
					DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(key);
					if (source.Contains(designInfo.Role))
						source.Remove(designInfo.Role);
				}
			}
			return source.FirstOrDefault<ShipRole>();
		}

		public bool CompositionCanSupportShip(int designid)
		{
			return true;
		}

		private void RemoveDesign(int designid, bool listid = true)
		{
			int removedinex = 0;
			if (listid)
			{
				removedinex = this.SelectedDesigns[this.ListDesignMap[designid]];
				if (this.SelectedDesigns.ContainsKey(this.ListDesignMap[designid]))
				{
					Dictionary<int, int> selectedDesigns;
					int listDesign;
					(selectedDesigns = this.SelectedDesigns)[listDesign = this.ListDesignMap[designid]] = selectedDesigns[listDesign] - 1;
				}
				designid = this.ListDesignMap[designid];
			}
			else if (this.SelectedDesigns.ContainsKey(designid))
			{
				Dictionary<int, int> selectedDesigns;
				int index;
				(selectedDesigns = this.SelectedDesigns)[index = designid] = selectedDesigns[index] - 1;
			}
			bool flag = false;
			foreach (DialogLoaFleetCompositor.RiderStruct riderStruct in this.RiderListMap.Where<DialogLoaFleetCompositor.RiderStruct>((Func<DialogLoaFleetCompositor.RiderStruct, bool>)(x =>
		   {
			   if (x.CarrierDesignID == designid)
				   return x.SelectedDesignCarrierKey == removedinex;
			   return false;
		   })))
			{
				flag = true;
				foreach (DialogLoaFleetCompositor.RiderWingStruct riderWingStruct in riderStruct.WingData)
				{
					using (List<int>.Enumerator enumerator = riderWingStruct.riders.GetEnumerator())
					{
						if (enumerator.MoveNext())
							this.RemoveDesign(enumerator.Current, false);
					}
				}
			}
			if (flag)
				this.RiderListMap.Remove(this.RiderListMap.First<DialogLoaFleetCompositor.RiderStruct>((Func<DialogLoaFleetCompositor.RiderStruct, bool>)(x =>
			   {
				   if (x.CarrierDesignID == designid)
					   return x.SelectedDesignCarrierKey == removedinex;
				   return false;
			   })));
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(designid);
			if (designInfo.Class != ShipClass.BattleRider)
				return;
			WeaponEnums.TurretClasses? turretclass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(designInfo);
			DialogLoaFleetCompositor.RiderStruct riderStruct1 = this.RiderListMap.FirstOrDefault<DialogLoaFleetCompositor.RiderStruct>((Func<DialogLoaFleetCompositor.RiderStruct, bool>)(x => x.WingData.Any<DialogLoaFleetCompositor.RiderWingStruct>((Func<DialogLoaFleetCompositor.RiderWingStruct, bool>)(j =>
		  {
			  WeaponEnums.TurretClasses turretClasses = j.wingdata.Class;
			  WeaponEnums.TurretClasses? nullable = turretclass;
			  if ((turretClasses != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				  return j.riders.Any<int>((Func<int, bool>)(k => k == designid));
			  return false;
		  }))));
			if (riderStruct1 != null)
			{
				DialogLoaFleetCompositor.RiderWingStruct riderWingStruct = riderStruct1.WingData.First<DialogLoaFleetCompositor.RiderWingStruct>((Func<DialogLoaFleetCompositor.RiderWingStruct, bool>)(x =>
			   {
				   WeaponEnums.TurretClasses turretClasses = x.wingdata.Class;
				   WeaponEnums.TurretClasses? nullable = turretclass;
				   if ((turretClasses != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
					   return x.riders.Any<int>((Func<int, bool>)(k => k == designid));
				   return false;
			   }));
				foreach (int slotIndex in riderWingStruct.wingdata.SlotIndexes)
					riderWingStruct.riders.Remove(designid);
				Dictionary<int, int> selectedDesigns;
				int index;
				(selectedDesigns = this.SelectedDesigns)[index = designid] = selectedDesigns[index] - (riderWingStruct.wingdata.SlotIndexes.Count - 1);
			}
			if (this.SelectedClass != RealShipClasses.BattleRider && this.SelectedClass != RealShipClasses.BattleShip && this.SelectedClass != RealShipClasses.BattleCruiser)
				return;
			this.SyncDesignListList(this.SelectedClass);
		}

		private bool CanMountBattleRider(DesignInfo design)
		{
			WeaponEnums.TurretClasses? turretclass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(design);
			return this.RiderListMap.FirstOrDefault<DialogLoaFleetCompositor.RiderStruct>((Func<DialogLoaFleetCompositor.RiderStruct, bool>)(x => x.WingData.Any<DialogLoaFleetCompositor.RiderWingStruct>((Func<DialogLoaFleetCompositor.RiderWingStruct, bool>)(j =>
		  {
			  WeaponEnums.TurretClasses turretClasses = j.wingdata.Class;
			  WeaponEnums.TurretClasses? nullable = turretclass;
			  if ((turretClasses != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				  return !j.riders.Any<int>();
			  return false;
		  })))) != null;
		}

		private void AddDesign(int designid)
		{
			DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(designid);
			if (designInfo.Class == ShipClass.BattleRider && !this.CanMountBattleRider(designInfo))
				return;
			if (!this.SelectedDesigns.ContainsKey(designid))
			{
				this.SelectedDesigns.Add(designid, 1);
			}
			else
			{
				Dictionary<int, int> selectedDesigns;
				int index;
				(selectedDesigns = this.SelectedDesigns)[index = designid] = selectedDesigns[index] + 1;
			}
			int selectedDesign = this.SelectedDesigns[designid];
			List<CarrierWingData> list = RiderManager.GetDesignBattleriderWingData(this._app, designInfo).ToList<CarrierWingData>();
			if (list.Any<CarrierWingData>())
			{
				DialogLoaFleetCompositor.RiderStruct riderStruct = new DialogLoaFleetCompositor.RiderStruct();
				foreach (CarrierWingData carrierWingData in list)
					riderStruct.WingData.Add(new DialogLoaFleetCompositor.RiderWingStruct()
					{
						wingdata = carrierWingData
					});
				riderStruct.CarrierDesignID = designid;
				riderStruct.SelectedDesignCarrierKey = selectedDesign;
				this.RiderListMap.Add(riderStruct);
			}
			if (designInfo.Class != ShipClass.BattleRider)
				return;
			WeaponEnums.TurretClasses? turretclass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(designInfo);
			DialogLoaFleetCompositor.RiderStruct riderStruct1 = this.RiderListMap.FirstOrDefault<DialogLoaFleetCompositor.RiderStruct>((Func<DialogLoaFleetCompositor.RiderStruct, bool>)(x => x.WingData.Any<DialogLoaFleetCompositor.RiderWingStruct>((Func<DialogLoaFleetCompositor.RiderWingStruct, bool>)(j =>
		  {
			  WeaponEnums.TurretClasses turretClasses = j.wingdata.Class;
			  WeaponEnums.TurretClasses? nullable = turretclass;
			  if ((turretClasses != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				  return !j.riders.Any<int>();
			  return false;
		  }))));
			if (riderStruct1 != null)
			{
				DialogLoaFleetCompositor.RiderWingStruct riderWingStruct = riderStruct1.WingData.First<DialogLoaFleetCompositor.RiderWingStruct>((Func<DialogLoaFleetCompositor.RiderWingStruct, bool>)(x =>
			   {
				   WeaponEnums.TurretClasses turretClasses = x.wingdata.Class;
				   WeaponEnums.TurretClasses? nullable = turretclass;
				   if ((turretClasses != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
					   return !x.riders.Any<int>();
				   return false;
			   }));
				foreach (int slotIndex in riderWingStruct.wingdata.SlotIndexes)
					riderWingStruct.riders.Add(designid);
				Dictionary<int, int> selectedDesigns;
				int index;
				(selectedDesigns = this.SelectedDesigns)[index = designid] = selectedDesigns[index] + (riderWingStruct.wingdata.SlotIndexes.Count - 1);
			}
			if (this.SelectedClass != RealShipClasses.BattleRider && this.SelectedClass != RealShipClasses.BattleShip && this.SelectedClass != RealShipClasses.BattleCruiser)
				return;
			this.SyncDesignListList(this.SelectedClass);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_item_dblclk")
			{
				if (panelName == "gameFleetList")
				{
					this.AddDesign(int.Parse(msgParams[0]));
					this.SyncDesignWorkingList();
				}
				else
				{
					if (!(panelName == "gameWorkingFleet"))
						return;
					this.RemoveDesign(int.Parse(msgParams[0].Split('|')[0]), true);
					this.SyncDesignWorkingList();
				}
			}
			else if (msgType == "list_sel_changed")
			{
				if (panelName == "gameFleetList")
					this.DesignDetailCard.SyncDesign(int.Parse(msgParams[0].Split('|')[0]), new int?());
				else if (panelName == "gameWorkingFleet")
				{
					int key = int.Parse(msgParams[0].Split('|')[0]);
					if (!this.ListDesignMap.ContainsKey(key))
						return;
					this.DesignDetailCard.SyncDesign(this.ListDesignMap[key], new int?());
				}
				else
				{
					if (!(panelName == "gameClassList"))
						return;
					RealShipClasses shipClass = (RealShipClasses)int.Parse(msgParams[0]);
					if (this.SelectedClass == shipClass)
						return;
					this.PopulateClassList(shipClass);
				}
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == "okButton" || panelName == "single_okButton")
					this.namedialog = this._app.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this._app, "Enter Name for Composition", "input a name for your fleet composition", "Composition", 1024, 3, false, EditBoxFilterMode.None), null);
				else if (panelName.StartsWith("designDeleteButton"))
				{
					this.RemoveDesign(int.Parse(panelName.Split('|')[2]), true);
					this.SyncDesignWorkingList();
				}
				else
				{
					if (!(panelName == "cancelButton"))
						return;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || !(panelName == this.namedialog) || !bool.Parse(msgParams[0]))
					return;
				this.invoicename = msgParams[1];
				this.commitdesigns();
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this._outputID.ToString() };
		}

		private class RiderStruct
		{
			public int CarrierDesignID;
			public int SelectedDesignCarrierKey;
			public List<DialogLoaFleetCompositor.RiderWingStruct> WingData;

			public RiderStruct()
			{
				this.WingData = new List<DialogLoaFleetCompositor.RiderWingStruct>();
			}
		}

		private class RiderWingStruct
		{
			public CarrierWingData wingdata;
			public List<int> riders;

			public RiderWingStruct()
			{
				this.riders = new List<int>();
			}
		}
	}
}
