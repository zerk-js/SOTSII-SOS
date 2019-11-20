// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ShipDesignUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal static class ShipDesignUI
	{
		public static void SyncCost(App game, string panel, DesignInfo design)
		{
			int num = design.SavingsCost;
			int playerProductionCost = design.GetPlayerProductionCost(game.GameDatabase, game.LocalPlayer.ID, true, new float?());
			string text1 = GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, false).ToString("N0");
			string text2 = string.Format("({0})", (object)GameSession.CalculateShipUpkeepCost(game.AssetDatabase, design, 1f, true).ToString("N0"));
			switch (design.Class)
			{
				case ShipClass.Cruiser:
					num = (int)((double)design.SavingsCost * (double)game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierCR, game.LocalPlayer.ID));
					break;
				case ShipClass.Dreadnought:
					num = (int)((double)design.SavingsCost * (double)game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierDN, game.LocalPlayer.ID));
					break;
				case ShipClass.Leviathan:
					num = (int)((double)design.SavingsCost * (double)game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierLV, game.LocalPlayer.ID));
					break;
				case ShipClass.Station:
					RealShipClasses? realShipClass = design.GetRealShipClass();
					if ((realShipClass.GetValueOrDefault() != RealShipClasses.Platform ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
					{
						num = (int)((double)design.SavingsCost * (double)game.GetStratModifier<float>(StratModifiers.PrototypeSavingsCostModifierPF, game.LocalPlayer.ID));
						break;
					}
					break;
			}
			string text3 = design.SavingsCost.ToString("N0");
			string text4 = design.GetPlayerProductionCost(game.GameDatabase, game.LocalPlayer.ID, false, new float?()).ToString("N0");
			string text5 = num.ToString("N0");
			string text6 = playerProductionCost.ToString("N0");
			game.UI.SetText(game.UI.Path(panel, "gameShipSavCost"), text3);
			game.UI.SetText(game.UI.Path(panel, "gameShipConCost"), text4);
			game.UI.SetText(game.UI.Path(panel, "gameProtoSavCost"), text5);
			game.UI.SetText(game.UI.Path(panel, "gameProtoConCost"), text6);
			game.UI.SetText(game.UI.Path(panel, "gameShipUpkeepCost"), text1);
			game.UI.SetText(game.UI.Path(panel, "gameShipResUpkeepCost"), text2);
		}

		public static void SyncSpeed(App game, DesignInfo design)
		{
			double mass = (double)design.Mass;
			float acceleration = design.Acceleration;
			float topSpeed = design.TopSpeed;
			float num1 = topSpeed / acceleration;
			float num2 = 0.0f;
			float num3 = 0.0f;
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = designSection.ShipSectionAsset;
				if (shipSectionAsset != null)
				{
					if ((double)shipSectionAsset.NodeSpeed > 0.0)
						num3 = shipSectionAsset.NodeSpeed;
					if ((double)shipSectionAsset.FtlSpeed > 0.0)
						num2 = shipSectionAsset.FtlSpeed;
				}
			}
			float num4 = 0.0f;
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				DesignSectionInfo sectionInfo = designSection;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionInfo.FilePath));
				num4 += shipSectionAsset.Maneuvering.RotationSpeed;
			}
			string text1 = string.Format("{0} kg", (object)design.Mass);
			string text2 = string.Format("{0} km/s\x00B2", (object)design.Acceleration);
			string text3 = string.Format("{0} deg/s", (object)num4);
			string text4 = string.Format("{0} km/s (in {1}s)", (object)topSpeed, (object)Math.Max(1, (int)num1));
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(design.PlayerID);
			if (game.AssetDatabase.GetFaction(playerInfo.FactionID).CanUseNodeLine(new bool?()))
				game.UI.SetText("gameShipFTLSpeed", string.Format("{0} ly", (object)num3));
			else
				game.UI.SetText("gameShipFTLSpeed", string.Format("{0} ly", (object)num2));
			game.UI.SetText("gameShipTopSpeedTime", text4);
			game.UI.SetText("gameShipTurnSpeed", text3);
			game.UI.SetText("gameShipThrust", text2);
			game.UI.SetText("gameShipMass", text1);
			game.UI.SetPropertyFloat("gameSpeedGraph", "accel", acceleration);
			game.UI.SetPropertyFloat("gameSpeedGraph", "max_speed", topSpeed);
		}

		public static void SyncSupplies(App game, DesignInfo design)
		{
			int propertyValue1 = (int)((double)design.SupplyAvailable * (double)game.GetStratModifier<float>(StratModifiers.ShipSupplyModifier, design.PlayerID));
			int supplyRequired = design.SupplyRequired;
			int propertyValue2 = propertyValue1;
			int crewAvailable = design.CrewAvailable;
			int crewRequired = design.CrewRequired;
			int propertyValue3 = crewAvailable;
			int powerAvailable = design.PowerAvailable;
			int powerRequired = design.PowerRequired;
			int propertyValue4 = powerAvailable;
			int endurance = design.GetEndurance(game.Game);
			float num = 1f;
			PlayerInfo pi = game.GameDatabase.GetPlayerInfo(design.PlayerID);
			if (pi != null)
			{
				Faction faction = game.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == pi.FactionID));
				if (faction != null)
					num = faction.CrewEfficiencyValue;
			}
			game.UI.SetPropertyInt("gameShipSupply", "max", propertyValue1);
			game.UI.SetPropertyInt("gameShipSupply", "req", supplyRequired);
			game.UI.SetPropertyInt("gameShipSupply", "cur", propertyValue2);
			game.UI.SetPropertyInt("gameShipCrew", "max", crewAvailable);
			game.UI.SetPropertyInt("gameShipCrew", "req", (int)((double)crewRequired / (double)num));
			game.UI.SetPropertyInt("gameShipCrew", "cur", propertyValue3);
			game.UI.SetPropertyInt("gameShipEnergy", "max", powerAvailable);
			game.UI.SetPropertyInt("gameShipEnergy", "req", powerRequired);
			game.UI.SetPropertyInt("gameShipEnergy", "cur", propertyValue4);
			game.UI.SetPropertyString("gameShipEnduranceVal", "text", string.Format("{0}T", (object)endurance));
			if (design.Role == ShipRole.DRONE || design.Role == ShipRole.ASSAULTSHUTTLE)
			{
				game.UI.SetVisible("gameShipEnduranceVal", false);
				game.UI.SetVisible("gameShipEnduranceLab", false);
			}
			else
			{
				game.UI.SetVisible("gameShipEnduranceVal", true);
				game.UI.SetVisible("gameShipEnduranceLab", true);
			}
			if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(design.PlayerID)).Name == "loa" || design.Role == ShipRole.DRONE || design.Role == ShipRole.ASSAULTSHUTTLE)
			{
				game.UI.SetVisible("gameShipCrew", false);
				game.UI.SetVisible("gameShipCrewIcon", false);
			}
			else
			{
				game.UI.SetVisible("gameShipCrew", true);
				game.UI.SetVisible("gameShipCrewIcon", true);
			}
		}

		public static void PopulateDesignList(
		  App game,
		  string designListId,
		  IEnumerable<InvoiceInfo> invoices)
		{
			game.UI.ClearItems(designListId);
			foreach (InvoiceInfo invoice in invoices)
			{
				game.UI.AddItem(designListId, string.Empty, invoice.ID, invoice.Name);
				game.UI.SetItemPropertyString(designListId, string.Empty, invoice.ID, "designName", "text", invoice.Name);
				game.UI.SetItemPropertyString(designListId, string.Empty, invoice.ID, "designDeleteButton", "id", "designDeleteButton|" + invoice.ID.ToString() + "|invoice");
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
				if (!Kerberos.Sots.StarFleet.StarFleet.DesignIsSuulka(game, design) && !design.IsAccelerator() && (!design.IsLoaCube() && BuildScreenState.IsShipRoleAllowed(design.Role)))
				{
					game.UI.AddItem(designListId, string.Empty, design.ID, design.Name);
					game.UI.SetItemPropertyString(designListId, string.Empty, design.ID, "designName", "text", design.Name);
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

		public static ShipSectionAsset GetSectionAsset(
		  App game,
		  DesignInfo design,
		  ShipSectionType sectionType)
		{
			foreach (DesignSectionInfo designSection1 in design.DesignSections)
			{
				DesignSectionInfo designSection = designSection1;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == designSection.FilePath));
				if (shipSectionAsset.Type == sectionType)
					return shipSectionAsset;
			}
			return (ShipSectionAsset)null;
		}

		public static void SyncSectionArmor(
		  App game,
		  string panelId,
		  ShipSectionAsset sectionAsset,
		  DesignInfo design)
		{
			string panelId1 = game.UI.Path(panelId, "partArmor");
			string panelId2 = game.UI.Path(panelId, "partArmorTop");
			string panelId3 = game.UI.Path(panelId, "partArmorBtm");
			string panelId4 = game.UI.Path(panelId, "partArmorSide");
			string panelId5 = game.UI.Path(panelId, "partStruct");
			string panelId6 = game.UI.Path(panelId, "partStructBar");
			int num1 = 0;
			int num2 = sectionAsset.Structure;
			if (design != null)
			{
				DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)design.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.FilePath == sectionAsset.FileName));
				List<string> list = designSectionInfo.Techs.Select<int, string>((Func<int, string>)(x => game.GameDatabase.GetTechFileID(x))).ToList<string>();
				num1 = Ship.GetArmorBonusFromTech(game.AssetDatabase, list);
				num2 = Ship.GetStructureWithTech(game.AssetDatabase, list, num2);
				foreach (DesignModuleInfo module in designSectionInfo.Modules)
				{
					string moduleAsset = game.GameDatabase.GetModuleAsset(module.ModuleID);
					LogicalModule logicalModule = game.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleAsset));
					if (logicalModule != null)
						num2 += (int)logicalModule.StructureBonus;
				}
			}
			int propertyValue1 = sectionAsset.Armor[1].Y + num1;
			int propertyValue2 = sectionAsset.Armor[3].Y + num1;
			int propertyValue3 = Math.Max(sectionAsset.Armor[2].Y, sectionAsset.Armor[0].Y) + num1;
			int num3 = sectionAsset.Armor[1].X * (sectionAsset.Armor[1].Y + num1) + sectionAsset.Armor[3].X * (sectionAsset.Armor[3].Y + num1) + sectionAsset.Armor[0].X * (sectionAsset.Armor[0].Y + num1) + sectionAsset.Armor[2].X * (sectionAsset.Armor[2].Y + num1);
			int propertyValue4 = 10;
			int propertyValue5 = 10000;
			string text1 = num3.ToString("N0");
			string text2 = num2.ToString("N0");
			game.UI.SetText(panelId1, text1);
			game.UI.SetPropertyInt(panelId2, "value", propertyValue1);
			game.UI.SetPropertyInt(panelId3, "value", propertyValue2);
			game.UI.SetPropertyInt(panelId4, "value", propertyValue3);
			game.UI.SetPropertyInt(panelId2, "max_value", propertyValue4);
			game.UI.SetPropertyInt(panelId3, "max_value", propertyValue4);
			game.UI.SetPropertyInt(panelId4, "max_value", propertyValue4);
			game.UI.SetText(panelId5, text2);
			game.UI.SetPropertyInt(panelId6, "value", num2);
			game.UI.SetPropertyInt(panelId6, "max_value", propertyValue5);
		}
	}
}
