// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DesignsCache
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal sealed class DesignsCache : RowCache<int, DesignInfo>
	{
		public DesignsCache(SQLiteConnection db, AssetDatabase assets)
		  : base(db, assets)
		{
		}

		private static ModulePsionicInfo GetModulePsionicInfoFromRow(Row row)
		{
			return new ModulePsionicInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				DesignModuleID = row[1].SQLiteValueToInteger(),
				Ability = (SectionEnumerations.PsionicAbility)row[2].SQLiteValueToInteger()
			};
		}

		private static IEnumerable<ModulePsionicInfo> GetModulePsionicInfosByDesignModule(
		  SQLiteConnection db,
		  int designModuleId)
		{
			foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetModulePsionicsByDesignModule, (object)designModuleId.ToSQLiteValue()), true))
				yield return DesignsCache.GetModulePsionicInfoFromRow(row);
		}

		internal static DesignModuleInfo GetDesignModuleInfoFromRow(
		  SQLiteConnection db,
		  Row row,
		  DesignSectionInfo designSection)
		{
			int integer = row[0].SQLiteValueToInteger();
			row[1].SQLiteValueToInteger();
			DesignModuleInfo designModuleInfo1 = new DesignModuleInfo();
			designModuleInfo1.ID = integer;
			designModuleInfo1.DesignSectionInfo = designSection;
			designModuleInfo1.ModuleID = row[2].SQLiteValueToInteger();
			designModuleInfo1.WeaponID = row[3].SQLiteValueToNullableInteger();
			designModuleInfo1.MountNodeName = row[4].SQLiteValueToString();
			DesignModuleInfo designModuleInfo2 = designModuleInfo1;
			int? nullableInteger = row[5].SQLiteValueToNullableInteger();
			ModuleEnums.StationModuleType? nullable = nullableInteger.HasValue ? new ModuleEnums.StationModuleType?((ModuleEnums.StationModuleType)nullableInteger.GetValueOrDefault()) : new ModuleEnums.StationModuleType?();
			designModuleInfo2.StationModuleType = nullable;
			designModuleInfo1.DesignID = row.Count<string>() > 6 ? row[6].SQLiteValueToNullableInteger() : new int?();
			designModuleInfo1.PsionicAbilities = DesignsCache.GetModulePsionicInfosByDesignModule(db, integer).ToList<ModulePsionicInfo>();
			return designModuleInfo1;
		}

		private static IEnumerable<DesignModuleInfo> GetModuleInfosForDesignSection(
		  SQLiteConnection db,
		  DesignSectionInfo designSection)
		{
			foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetDesignModuleInfos, (object)designSection.ID.ToSQLiteValue()), true))
				yield return DesignsCache.GetDesignModuleInfoFromRow(db, row, designSection);
		}

		private static WeaponBankInfo GetWeaponBankInfoFromRow(Row row)
		{
			return new WeaponBankInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				WeaponID = row[1].SQLiteValueToNullableInteger(),
				DesignID = row[2].SQLiteValueToNullableInteger(),
				BankGUID = row[3].SQLiteValueToGuid(),
				FiringMode = row[4].SQLiteValueToNullableInteger(),
				FilterMode = row[5].SQLiteValueToNullableInteger()
			};
		}

		private static IEnumerable<WeaponBankInfo> GetWeaponBankInfosForDesignSection(
		  SQLiteConnection db,
		  int designSectionID)
		{
			foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetWeaponBankInfos, (object)designSectionID.ToSQLiteValue()), true))
				yield return DesignsCache.GetWeaponBankInfoFromRow(row);
		}

		private static IEnumerable<int> GetTechsForDesignSection(
		  SQLiteConnection db,
		  int designSectionId)
		{
			foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetDesignSectionTechs, (object)designSectionId), true))
				yield return row[0].SQLiteValueToInteger();
		}

		private static DesignSectionInfo GetDesignSectionInfoFromRow(
		  Row row,
		  SQLiteConnection db,
		  AssetDatabase assets)
		{
			int integer = row[0].SQLiteValueToInteger();
			string filename = row[1].SQLiteValueToString();
			DesignSectionInfo designSection = new DesignSectionInfo()
			{
				ID = integer,
				FilePath = filename,
				ShipSectionAsset = assets.GetShipSectionAsset(filename),
				WeaponBanks = DesignsCache.GetWeaponBankInfosForDesignSection(db, integer).ToList<WeaponBankInfo>(),
				Techs = DesignsCache.GetTechsForDesignSection(db, integer).ToList<int>()
			};
			List<DesignModuleInfo> list = DesignsCache.GetModuleInfosForDesignSection(db, designSection).ToList<DesignModuleInfo>();
			designSection.Modules = new List<DesignModuleInfo>();
			foreach (LogicalModuleMount module in designSection.ShipSectionAsset.Modules)
			{
				LogicalModuleMount mount = module;
				DesignModuleInfo designModuleInfo = list.FirstOrDefault<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == mount.NodeName));
				if (designModuleInfo != null)
				{
					designSection.Modules.Add(designModuleInfo);
					list.Remove(designModuleInfo);
				}
			}
			return designSection;
		}

		private static IEnumerable<DesignSectionInfo> GetDesignSectionInfos(
		  SQLiteConnection db,
		  AssetDatabase assets,
		  int designID)
		{
			foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetDesignSectionInfo, (object)designID), true))
				yield return DesignsCache.GetDesignSectionInfoFromRow(row, db, assets);
		}

		private static DesignInfo GetDesignInfoFromRow(
		  SQLiteConnection db,
		  AssetDatabase assets,
		  Row row)
		{
			int integer = row[0].SQLiteValueToInteger();
			DesignInfo designInfo = new DesignInfo()
			{
				ID = integer,
				DesignSections = DesignsCache.GetDesignSectionInfos(db, assets, integer).ToArray<DesignSectionInfo>(),
				PlayerID = row[1].SQLiteValueToOneBasedIndex(),
				Name = row[2],
				Armour = int.Parse(row[3]),
				Structure = float.Parse(row[4]),
				NumTurrets = int.Parse(row[5]),
				Mass = float.Parse(row[6]),
				Acceleration = float.Parse(row[8]),
				TopSpeed = float.Parse(row[9]),
				SavingsCost = int.Parse(row[10]),
				ProductionCost = int.Parse(row[11]),
				Class = (ShipClass)int.Parse(row[12]),
				CrewAvailable = int.Parse(row[13]),
				PowerAvailable = int.Parse(row[14]),
				SupplyAvailable = int.Parse(row[15]),
				CrewRequired = int.Parse(row[16]),
				PowerRequired = int.Parse(row[17]),
				SupplyRequired = int.Parse(row[18]),
				NumBuilt = int.Parse(row[19]),
				DesignDate = int.Parse(row[20]),
				Role = (ShipRole)int.Parse(row[21]),
				WeaponRole = (WeaponRole)int.Parse(row[22]),
				isPrototyped = bool.Parse(row[23]),
				isAttributesDiscovered = bool.Parse(row[24]),
				StationType = (StationType)int.Parse(row[25]),
				StationLevel = int.Parse(row[26]),
				PriorityWeaponName = row[28] ?? string.Empty,
				NumDestroyed = int.Parse(row[29]),
				RetrofitBaseID = row[30].SQLiteValueToOneBasedIndex()
			};
			designInfo.HackValidateRole();
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
				designSection.DesignInfo = designInfo;
			return designInfo;
		}

		protected override IEnumerable<KeyValuePair<int, DesignInfo>> OnSynchronizeWithDatabase(
		  SQLiteConnection db,
		  IEnumerable<int> range)
		{
			if (range == null)
			{
				foreach (Row row in db.ExecuteTableQuery(Queries.GetDesignInfos, true))
				{
					DesignInfo o = DesignsCache.GetDesignInfoFromRow(db, this.Assets, row);
					yield return new KeyValuePair<int, DesignInfo>(o.ID, o);
				}
			}
			else
			{
				foreach (int num in range)
				{
					foreach (Row row in db.ExecuteTableQuery(string.Format(Queries.GetDesignInfo, (object)num.ToSQLiteValue()), true))
					{
						DesignInfo o = DesignsCache.GetDesignInfoFromRow(db, this.Assets, row);
						yield return new KeyValuePair<int, DesignInfo>(o.ID, o);
					}
				}
			}
		}

		private static int InsertWeaponBank(
		  SQLiteConnection db,
		  int designSectionId,
		  WeaponBankInfo value)
		{
			return db.ExecuteIntegerQuery(string.Format(Queries.InsertWeaponBank, (object)designSectionId.ToSQLiteValue(), (object)value.WeaponID.ToNullableSQLiteValue(), (object)value.DesignID.ToNullableSQLiteValue(), (object)value.FiringMode.ToNullableSQLiteValue(), (object)value.FilterMode.ToNullableSQLiteValue(), (object)value.BankGUID.ToSQLiteValue()));
		}

		internal static int InsertDesignModuleInfo(SQLiteConnection db, DesignModuleInfo value)
		{
			SQLiteConnection sqLiteConnection = db;
			string insertDesignModule = Queries.InsertDesignModule;
			object[] objArray1 = new object[6]
			{
		(object) value.DesignSectionInfo.ID.ToSQLiteValue(),
		(object) value.MountNodeName.ToSQLiteValue(),
		(object) value.ModuleID.ToSQLiteValue(),
		(object) value.WeaponID.ToNullableSQLiteValue(),
		null,
		null
			};
			object[] objArray2 = objArray1;
			ModuleEnums.StationModuleType? stationModuleType = value.StationModuleType;
			string nullableSqLiteValue = (stationModuleType.HasValue ? new int?((int)stationModuleType.GetValueOrDefault()) : new int?()).ToNullableSQLiteValue();
			objArray2[4] = (object)nullableSqLiteValue;
			objArray1[5] = (object)value.DesignID.ToNullableSQLiteValue();
			object[] objArray3 = objArray1;
			string query = string.Format(insertDesignModule, objArray3);
			int num = sqLiteConnection.ExecuteIntegerQuery(query);
			if (value.PsionicAbilities == null)
				value.PsionicAbilities = new List<ModulePsionicInfo>();
			foreach (ModulePsionicInfo psionicAbility in value.PsionicAbilities)
				psionicAbility.ID = db.ExecuteIntegerQuery(string.Format(Queries.InsertModulePsionicAbility, (object)num.ToSQLiteValue(), (object)((int)psionicAbility.Ability).ToSQLiteValue()));
			return num;
		}

		protected override int OnInsert(SQLiteConnection db, int? key, DesignInfo value)
		{
			if (key.HasValue)
				throw new ArgumentOutOfRangeException(nameof(key), "Design insertion does not permit explicit specification of an ID.");
			if (ScriptHost.AllowConsole)
			{
				if (value.DesignSections == null || value.DesignSections.Length == 0)
					throw new InvalidDataException("DesignInfo does not supply any DesignSections.");
			}
			else
				RowCache<int, DesignInfo>.Warn(string.Format("DesignsCache.OnInsert: DesignInfo does not supply any DesignSections (player={0}, stationType={1}, stationLevel={2})", (object)value.PlayerID, (object)value.StationType.ToString(), (object)value.StationLevel.ToString()));
			value.HackValidateRole();
			int num = db.ExecuteIntegerQuery(string.Format(Queries.InsertDesign, (object)value.PlayerID.ToOneBasedSQLiteValue(), (object)value.Name.ToSQLiteValue(), (object)value.Armour.ToSQLiteValue(), (object)value.Structure.ToSQLiteValue(), (object)value.NumTurrets.ToSQLiteValue(), (object)value.Mass.ToSQLiteValue(), (object)0.ToSQLiteValue(), (object)value.Acceleration.ToSQLiteValue(), (object)value.TopSpeed.ToSQLiteValue(), (object)value.SavingsCost.ToSQLiteValue(), (object)value.ProductionCost.ToSQLiteValue(), (object)((int)value.Class).ToSQLiteValue(), (object)value.CrewAvailable.ToSQLiteValue(), (object)value.PowerAvailable.ToSQLiteValue(), (object)value.SupplyAvailable.ToSQLiteValue(), (object)value.CrewRequired.ToSQLiteValue(), (object)value.PowerRequired.ToSQLiteValue(), (object)value.SupplyRequired.ToSQLiteValue(), (object)GameDatabase.GetTurnCount(db).ToSQLiteValue(), (object)((int)value.Role).ToSQLiteValue(), (object)((int)value.WeaponRole).ToSQLiteValue(), (object)value.isPrototyped.ToSQLiteValue(), (object)value.isAttributesDiscovered.ToSQLiteValue(), (object)((int)value.StationType).ToSQLiteValue(), (object)value.StationLevel.ToSQLiteValue(), (object)value.PriorityWeaponName.ToSQLiteValue(), (object)value.RetrofitBaseID.ToOneBasedSQLiteValue()));
			value.ID = num;
			foreach (DesignSectionInfo designSection in value.DesignSections)
			{
				designSection.DesignInfo = value;
				int designSectionId = db.ExecuteIntegerQuery(string.Format(Queries.InsertDesignSection.ToSQLiteValue(), (object)num.ToSQLiteValue(), (object)designSection.FilePath.ToSQLiteValue()));
				designSection.ID = designSectionId;
				if (designSection.WeaponBanks == null)
					designSection.WeaponBanks = new List<WeaponBankInfo>();
				foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
					weaponBank.ID = DesignsCache.InsertWeaponBank(db, designSectionId, weaponBank);
				if (designSection.Modules == null)
					designSection.Modules = new List<DesignModuleInfo>();
				foreach (DesignModuleInfo module in designSection.Modules)
				{
					module.DesignSectionInfo = designSection;
					module.ID = DesignsCache.InsertDesignModuleInfo(db, module);
				}
				if (designSection.Techs == null)
					designSection.Techs = new List<int>();
				foreach (int tech in designSection.Techs)
					db.ExecuteIntegerQuery(string.Format(Queries.InsertDesignSectionTech, (object)designSectionId.ToSQLiteValue(), (object)tech.ToSQLiteValue()));
			}
			return num;
		}

		protected override void OnRemove(SQLiteConnection db, int key)
		{
			db.ExecuteIntegerQuery(string.Format(Queries.RemoveDesign, (object)key.ToSQLiteValue()));
		}

		protected override void OnUpdate(SQLiteConnection db, int key, DesignInfo value)
		{
			db.ExecuteIntegerQuery(string.Format(Queries.UpdateDesign, (object)value.ID.ToSQLiteValue(), (object)value.PlayerID.ToSQLiteValue(), (object)value.Name.ToSQLiteValue(), (object)value.Armour.ToSQLiteValue(), (object)value.Structure.ToSQLiteValue(), (object)value.NumTurrets.ToSQLiteValue(), (object)value.Mass.ToSQLiteValue(), (object)0.ToSQLiteValue(), (object)value.Acceleration.ToSQLiteValue(), (object)value.TopSpeed.ToSQLiteValue(), (object)value.SavingsCost.ToSQLiteValue(), (object)value.ProductionCost.ToSQLiteValue(), (object)((int)value.Class).ToSQLiteValue(), (object)value.CrewAvailable.ToSQLiteValue(), (object)value.PowerAvailable.ToSQLiteValue(), (object)value.SupplyAvailable.ToSQLiteValue(), (object)value.CrewRequired.ToSQLiteValue(), (object)value.PowerRequired.ToSQLiteValue(), (object)value.SupplyRequired.ToSQLiteValue(), (object)GameDatabase.GetTurnCount(db).ToSQLiteValue(), (object)((int)value.Role).ToSQLiteValue(), (object)((int)value.WeaponRole).ToSQLiteValue(), (object)value.isPrototyped.ToSQLiteValue(), (object)value.isAttributesDiscovered.ToSQLiteValue(), (object)((int)value.StationType).ToSQLiteValue(), (object)value.StationLevel.ToSQLiteValue()));
		}
	}
}
