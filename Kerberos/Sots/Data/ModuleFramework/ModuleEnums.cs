// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ModuleFramework.ModuleEnums
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Data.ModuleFramework
{
	public class ModuleEnums
	{
		public static Dictionary<string, ModuleEnums.StationModuleType> FactionLargeHabitationModules = new Dictionary<string, ModuleEnums.StationModuleType>()
	{
	  {
		"human",
		ModuleEnums.StationModuleType.HumanLargeHabitation
	  },
	  {
		"tarkas",
		ModuleEnums.StationModuleType.TarkasLargeHabitation
	  },
	  {
		"liir_zuul",
		ModuleEnums.StationModuleType.LiirLargeHabitation
	  },
	  {
		"hiver",
		ModuleEnums.StationModuleType.HiverLargeHabitation
	  },
	  {
		"morrigi",
		ModuleEnums.StationModuleType.MorrigiLargeHabitation
	  },
	  {
		"zuul",
		ModuleEnums.StationModuleType.ZuulLargeHabitation
	  },
	  {
		"loa",
		ModuleEnums.StationModuleType.LoaLargeHabitation
	  }
	};
		public static Dictionary<string, ModuleEnums.StationModuleType> FactionHabitationModules = new Dictionary<string, ModuleEnums.StationModuleType>()
	{
	  {
		"human",
		ModuleEnums.StationModuleType.HumanHabitation
	  },
	  {
		"tarkas",
		ModuleEnums.StationModuleType.TarkasHabitation
	  },
	  {
		"liir_zuul",
		ModuleEnums.StationModuleType.LiirHabitation
	  },
	  {
		"hiver",
		ModuleEnums.StationModuleType.HiverHabitation
	  },
	  {
		"morrigi",
		ModuleEnums.StationModuleType.MorrigiHabitation
	  },
	  {
		"zuul",
		ModuleEnums.StationModuleType.ZuulHabitation
	  },
	  {
		"loa",
		ModuleEnums.StationModuleType.LoaHabitation
	  }
	};
		public static List<ModuleEnums.StationModuleType> HabitationModuleTypes = new List<ModuleEnums.StationModuleType>()
	{
	  ModuleEnums.StationModuleType.HumanHabitation,
	  ModuleEnums.StationModuleType.TarkasHabitation,
	  ModuleEnums.StationModuleType.LiirHabitation,
	  ModuleEnums.StationModuleType.HiverHabitation,
	  ModuleEnums.StationModuleType.MorrigiHabitation,
	  ModuleEnums.StationModuleType.ZuulHabitation,
	  ModuleEnums.StationModuleType.LoaHabitation,
	  ModuleEnums.StationModuleType.HumanHabitationForeign,
	  ModuleEnums.StationModuleType.TarkasHabitationForeign,
	  ModuleEnums.StationModuleType.LiirHabitationForeign,
	  ModuleEnums.StationModuleType.HiverHabitationForeign,
	  ModuleEnums.StationModuleType.MorrigiHabitationForeign,
	  ModuleEnums.StationModuleType.ZuulHabitationForeign,
	  ModuleEnums.StationModuleType.LoaHabitationForeign
	};
		public static List<ModuleEnums.StationModuleType> LargeHabitationModuleTypes = new List<ModuleEnums.StationModuleType>()
	{
	  ModuleEnums.StationModuleType.HumanLargeHabitation,
	  ModuleEnums.StationModuleType.TarkasLargeHabitation,
	  ModuleEnums.StationModuleType.LiirLargeHabitation,
	  ModuleEnums.StationModuleType.HiverLargeHabitation,
	  ModuleEnums.StationModuleType.MorrigiLargeHabitation,
	  ModuleEnums.StationModuleType.ZuulLargeHabitation,
	  ModuleEnums.StationModuleType.LoaLargeHabitation,
	  ModuleEnums.StationModuleType.HumanLargeHabitationForeign,
	  ModuleEnums.StationModuleType.TarkasLargeHabitationForeign,
	  ModuleEnums.StationModuleType.LiirLargeHabitationForeign,
	  ModuleEnums.StationModuleType.HiverLargeHabitationForeign,
	  ModuleEnums.StationModuleType.MorrigiLargeHabitationForeign,
	  ModuleEnums.StationModuleType.ZuulLargeHabitationForeign,
	  ModuleEnums.StationModuleType.LoaLargeHabitationForeign
	};
		public static List<ModuleEnums.StationModuleType> TradeModuleTypes = new List<ModuleEnums.StationModuleType>()
	{
	  ModuleEnums.StationModuleType.HumanTradeModule,
	  ModuleEnums.StationModuleType.TarkasTradeModule,
	  ModuleEnums.StationModuleType.LiirTradeModule,
	  ModuleEnums.StationModuleType.HiverTradeModule,
	  ModuleEnums.StationModuleType.MorrigiTradeModule,
	  ModuleEnums.StationModuleType.ZuulTradeModule,
	  ModuleEnums.StationModuleType.LoaTradeModule
	};
		public static List<ModuleEnums.StationModuleType> LabTypes = new List<ModuleEnums.StationModuleType>()
	{
	  ModuleEnums.StationModuleType.EWPLab,
	  ModuleEnums.StationModuleType.TRPLab,
	  ModuleEnums.StationModuleType.NRGLab,
	  ModuleEnums.StationModuleType.WARLab,
	  ModuleEnums.StationModuleType.BALLab,
	  ModuleEnums.StationModuleType.BIOLab,
	  ModuleEnums.StationModuleType.INDLab,
	  ModuleEnums.StationModuleType.CCCLab,
	  ModuleEnums.StationModuleType.DRVLab,
	  ModuleEnums.StationModuleType.POLLab,
	  ModuleEnums.StationModuleType.PSILab,
	  ModuleEnums.StationModuleType.ENGLab,
	  ModuleEnums.StationModuleType.BRDLab,
	  ModuleEnums.StationModuleType.SLDLab,
	  ModuleEnums.StationModuleType.CYBLab
	};
		public static readonly string[] _ModuleTypes = Enum.GetNames(typeof(ModuleEnums.ModuleSlotTypes));
		public static readonly string[] _ModuleAbilities = Enum.GetNames(typeof(ModuleEnums.ModuleAbilities));
		public static readonly string[] _ModuleSizes = Enum.GetNames(typeof(ModuleEnums.ModuleSizes));

		public enum StationModuleType
		{
			Sensor,
			Customs,
			Combat,
			Repair,
			Warehouse,
			Command,
			Dock,
			Terraform,
			Bastion,
			Amp,
			GateLab,
			Defence,
			AlienHabitation,
			Habitation,
			HumanHabitation,
			TarkasHabitation,
			LiirHabitation,
			HiverHabitation,
			MorrigiHabitation,
			ZuulHabitation,
			LoaHabitation,
			HumanHabitationForeign,
			TarkasHabitationForeign,
			LiirHabitationForeign,
			HiverHabitationForeign,
			MorrigiHabitationForeign,
			ZuulHabitationForeign,
			LoaHabitationForeign,
			LargeHabitation,
			LargeAlienHabitation,
			HumanLargeHabitation,
			TarkasLargeHabitation,
			LiirLargeHabitation,
			HiverLargeHabitation,
			MorrigiLargeHabitation,
			ZuulLargeHabitation,
			LoaLargeHabitation,
			HumanLargeHabitationForeign,
			TarkasLargeHabitationForeign,
			LiirLargeHabitationForeign,
			HiverLargeHabitationForeign,
			MorrigiLargeHabitationForeign,
			ZuulLargeHabitationForeign,
			LoaLargeHabitationForeign,
			HumanTradeModule,
			TarkasTradeModule,
			LiirTradeModule,
			HiverTradeModule,
			MorrigiTradeModule,
			ZuulTradeModule,
			LoaTradeModule,
			Trade,
			Lab,
			EWPLab,
			TRPLab,
			NRGLab,
			WARLab,
			BALLab,
			BIOLab,
			INDLab,
			CCCLab,
			DRVLab,
			POLLab,
			PSILab,
			ENGLab,
			BRDLab,
			SLDLab,
			CYBLab,
			MemoryAugmentation,
			LargeMemoryAugmentation,
			NumModuleTypes,
		}

		public enum ModuleSlotTypes
		{
			Cruiser,
			CruiserEngine,
			Dreadnought,
			DreadnoughtEngine,
			ZuulCruiser,
			ZuulCruiserEngine,
			ZuulDreadnought,
			ZuulDreadnoughtEngine,
			SuulkaDreadnought,
			Lab,
			Sensor,
			Customs,
			Combat,
			Repair,
			Warehouse,
			Command,
			Dock,
			Habitation,
			LargeHabitation,
			AlienHabitation,
			LargeAlienHabitation,
			Terraform,
			Bastion,
			Defence,
			Amp,
			GateLab,
			CometFragment_01,
			CometFragment_02,
			CometFragment_03,
			CometFragment_04,
			CometFragment_05,
			CometFragment_06,
			CometFragment_07,
			CometFragment_08,
			CometFragment_09,
			CometFragment_10,
			MemoryAugmentation,
			LargeMemoryAugmentation,
		}

		public enum ModuleAbilities
		{
			None,
			Tentacle,
			Tendril,
			KingfisherRider,
			KarnakTargeting,
			GoopArmorRepair,
			JokerECM,
			AbaddonLaser,
		}

		public enum ModuleSizes
		{
			Cruiser,
			Dreadnought,
			Station,
		}
	}
}
