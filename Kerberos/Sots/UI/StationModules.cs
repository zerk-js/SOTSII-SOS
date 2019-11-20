// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StationModules
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ModuleFramework;

namespace Kerberos.Sots.UI
{
	internal class StationModules
	{
		public static StationModules.StationModule[] Modules = new StationModules.StationModule[64]
		{
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_SENSOR"), "@UI_STATIONDETAILS_{0}_SENSORDESC", ModuleEnums.StationModuleType.Sensor, "Sensor"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_WAREHOUSE"), "@UI_STATIONDETAILS_{0}_WAREHOUSEDESC", ModuleEnums.StationModuleType.Warehouse, "Warehouse"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_REPAIR"), "@UI_STATIONDETAILS_{0}_REPAIRDESC", ModuleEnums.StationModuleType.Repair, "Repair"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_COMMAND"), "@UI_STATIONDETAILS_{0}_COMMANDDESC", ModuleEnums.StationModuleType.Command, "Command"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_DOCK"), "@UI_STATIONDETAILS_{0}_DOCKDESC", ModuleEnums.StationModuleType.Dock, "Dock"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_COMBAT"), "@UI_STATIONDETAILS_{0}_COMBATDESC", ModuleEnums.StationModuleType.Combat, "Combat"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_CUSTOMS"), "@UI_STATIONDETAILS_{0}_CUSTOMSDESC", ModuleEnums.StationModuleType.Customs, "Customs"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_TERRA"), "@UI_STATIONDETAILS_{0}_TERRADESC", ModuleEnums.StationModuleType.Terraform, "Terraform"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_EWPLAB"), "@UI_STATIONDETAILS_EWPLABDESC", ModuleEnums.StationModuleType.EWPLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_TRPLAB"), "@UI_STATIONDETAILS_TRPLABDESC", ModuleEnums.StationModuleType.TRPLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_NRGLAB"), "@UI_STATIONDETAILS_NRGLABDESC", ModuleEnums.StationModuleType.NRGLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_WARLAB"), "@UI_STATIONDETAILS_WARLABDESC", ModuleEnums.StationModuleType.WARLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_BALLAB"), "@UI_STATIONDETAILS_BALLABDESC", ModuleEnums.StationModuleType.BALLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_BIOLAB"), "@UI_STATIONDETAILS_BIOLABDESC", ModuleEnums.StationModuleType.BIOLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_INDLAB"), "@UI_STATIONDETAILS_INDLABDESC", ModuleEnums.StationModuleType.INDLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_CCCLAB"), "@UI_STATIONDETAILS_CCCLABDESC", ModuleEnums.StationModuleType.CCCLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_DRVLAB"), "@UI_STATIONDETAILS_DRVLABDESC", ModuleEnums.StationModuleType.DRVLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_POLLAB"), "@UI_STATIONDETAILS_POLLABDESC", ModuleEnums.StationModuleType.POLLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_PSILAB"), "@UI_STATIONDETAILS_PSILABDESC", ModuleEnums.StationModuleType.PSILab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_ENGLAB"), "@UI_STATIONDETAILS_ENGLABDESC", ModuleEnums.StationModuleType.ENGLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_BRDLAB"), "@UI_STATIONDETAILS_BRDLABDESC", ModuleEnums.StationModuleType.BRDLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_SLDLAB"), "@UI_STATIONDETAILS_SLDLABDESC", ModuleEnums.StationModuleType.SLDLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_CYBLAB"), "@UI_STATIONDETAILS_CYBLABDESC", ModuleEnums.StationModuleType.CYBLab, "Lab"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_HUMANHAB"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.HumanHabitation, "Habitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_TARKASHAB"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.TarkasHabitation, "Habitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LIIRHAB"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.LiirHabitation, "Habitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_HIVERHAB"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.HiverHabitation, "Habitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_MORRIGIHAB"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.MorrigiHabitation, "Habitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_ZUULHAB"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.ZuulHabitation, "Habitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LOAHAB"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.LoaHabitation, "Habitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_HUMANHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.HumanHabitationForeign, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_TARKASHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.TarkasHabitationForeign, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LIIRHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.LiirHabitationForeign, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_HIVERHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.HiverHabitationForeign, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_MORRIGIHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.MorrigiHabitationForeign, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_ZUULHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.ZuulHabitationForeign, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LOAHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_HABDESC", ModuleEnums.StationModuleType.LoaHabitationForeign, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_ALIENHAB"), "@UI_STATIONDETAILS_ALIEN_HABDESC", ModuleEnums.StationModuleType.AlienHabitation, "AlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGALIENHAB"), "@UI_STATIONDETAILS_ALIEN_LGHABDESC", ModuleEnums.StationModuleType.LargeAlienHabitation, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGHUMANHAB"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.HumanLargeHabitation, "LargeHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGTARKASHAB"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.TarkasLargeHabitation, "LargeHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGLIIRHAB"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.LiirLargeHabitation, "LargeHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGHIVERHAB"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.HiverLargeHabitation, "LargeHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGMORRIGIHAB"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.MorrigiLargeHabitation, "LargeHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGZUULHAB"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.ZuulLargeHabitation, "LargeHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGLOAHAB"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.LoaLargeHabitation, "LargeHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGHUMANHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.HumanLargeHabitationForeign, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGTARKASHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.TarkasLargeHabitationForeign, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGLIIRHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.LiirLargeHabitationForeign, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGHIVERHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.HiverLargeHabitationForeign, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGMORRIGIHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.MorrigiLargeHabitationForeign, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGZUULHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.ZuulLargeHabitationForeign, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LGLOAHAB_FOREIGN"), "@UI_STATIONDETAILS_{0}_LGHABDESC", ModuleEnums.StationModuleType.LoaLargeHabitationForeign, "LargeAlienHabitation"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_HUMANTM"), "@UI_STATIONDETAILS_HUMANTMDESC", ModuleEnums.StationModuleType.HumanTradeModule, "Trade"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_TARKASTM"), "@UI_STATIONDETAILS_TARKASTMDESC", ModuleEnums.StationModuleType.TarkasTradeModule, "Trade"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LIIRTM"), "@UI_STATIONDETAILS_LIIRTMDESC", ModuleEnums.StationModuleType.LiirTradeModule, "Trade"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_HIVERTM"), "@UI_STATIONDETAILS_HIVERTMDESC", ModuleEnums.StationModuleType.HiverTradeModule, "Trade"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_MORRIGITM"), "@UI_STATIONDETAILS_MORRIGITMDESC", ModuleEnums.StationModuleType.MorrigiTradeModule, "Trade"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_ZUULTM"), "@UI_STATIONDETAILS_ZUULTMDESC", ModuleEnums.StationModuleType.ZuulTradeModule, "Trade"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_LOATM"), "@UI_STATIONDETAILS_LOATMDESC", ModuleEnums.StationModuleType.LoaTradeModule, "Trade"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_BASTION"), "@UI_STATIONDETAILS_{0}_BASTIONDESC", ModuleEnums.StationModuleType.Bastion, "Bastion"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_AMP"), "@UI_STATIONDETAILS_{0}_AMPDESC", ModuleEnums.StationModuleType.Amp, "Amp"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_DEFENSE"), "@UI_STATIONDETAILS_DEFENSEDESC", ModuleEnums.StationModuleType.Defence, "Defence"),
	  new StationModules.StationModule(App.Localize("@UI_STATIONDETAILS_GATELAB"), "@UI_STATIONDETAILS_GATELABDESC", ModuleEnums.StationModuleType.GateLab, "GateLab")
		};

		public class StationModule
		{
			public ModuleEnums.StationModuleType SMType;
			public string SlotType;
			public string Name;
			public string Description;

			public StationModule(
			  string name,
			  string desc,
			  ModuleEnums.StationModuleType smtype,
			  string slottype)
			{
				this.Name = name;
				this.Description = desc;
				this.SMType = smtype;
				this.SlotType = slottype;
			}
		}
	}
}
