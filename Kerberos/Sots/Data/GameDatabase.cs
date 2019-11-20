// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.GameDatabase
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.GenericFramework;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.SQLite;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Kerberos.Sots.Data
{
	internal class GameDatabase : IDisposable
	{
		private static Dictionary<int, Dictionary<StratModifiers, CachedStratMod>> _cachedStratMods = new Dictionary<int, Dictionary<StratModifiers, CachedStratMod>>();
		private static bool PlayersAlwaysFight = false;
		private string _location = ":memory:";
		private const string clientIdLine = "--Setting client ID to: ";
		public const string DefaultLiveLocation = ":memory:";
		public const int Version = 2080;
		public const int EOF_MIN_DB = 2000;
		public const int CombatDataVersion = 1;
		public const float DefaultSystemSensorRange = 5f;
		private SQLiteConnection db;
		private AssetDatabase assetdb;
		private int _clientId;
		private readonly DataObjectCache _dom;
		private int insertPlayerTechBranchCount;
		private List<MoveOrderInfo> TempMoveOrders;

		public void ReplayQueryHistory(string outputDatabaseFilename, int count)
		{
			GameDatabase gameDatabase = GameDatabase.New(this.GetGameName(), this.assetdb, false);
			foreach (Row row in this.db.ExecuteTableQuery(string.Format("SELECT query FROM query_history WHERE id <= {0}", (object)count), true).Rows)
			{
				if (row[0].StartsWith("--Setting client ID to: "))
				{
					int id = int.Parse(row[0].Substring("--Setting client ID to: ".Length));
					gameDatabase.SetClientId(id);
				}
				else
				{
					try
					{
						gameDatabase.db.ExecuteNonQuery(row[0], true, true);
					}
					catch (SQLiteException ex)
					{
						throw new SQLiteException(string.Format("While executing query: {0}\n{1}", (object)row[0], (object)ex.Message));
					}
				}
			}
			gameDatabase.Save(outputDatabaseFilename);
		}

		public void SetResearchEfficiency(int odds)
		{
			string name = "ResearchEfficiency";
			if (this.GetNameValue(name) == null)
				this.InsertNameValuePair(name, string.Empty);
			this.UpdateNameValuePair(name, odds.ToString());
		}

		public int GetResearchEfficiency()
		{
			string nameValue = this.GetNameValue("ResearchEfficiency");
			int result;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out result))
				return 0;
			return result;
		}

		public void SetEconomicEfficiency(int odds)
		{
			string name = "EconomicEfficiency";
			if (this.GetNameValue(name) == null)
				this.InsertNameValuePair(name, string.Empty);
			this.UpdateNameValuePair(name, odds.ToString());
		}

		public int GetEconomicEfficiency()
		{
			string nameValue = this.GetNameValue("EconomicEfficiency");
			int result;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out result))
				return 0;
			return result;
		}

		public void SetRandomEncounterFrequency(int odds)
		{
			string name = "RandomEncounterFrequency";
			if (this.GetNameValue(name) == null)
				this.InsertNameValuePair(name, string.Empty);
			this.UpdateNameValuePair(name, odds.ToString());
		}

		public int GetRandomEncounterFrequency()
		{
			string nameValue = this.GetNameValue("RandomEncounterFrequency");
			int result;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out result))
				return 0;
			return result;
		}

		public void SetTurnLength(GameDatabase.TurnLengthTypes type, float minutes)
		{
			string name = type == GameDatabase.TurnLengthTypes.Strategic ? "StrategicTurnLength" : "CombatTurnLength";
			if (this.GetNameValue(name) == null)
				this.InsertNameValuePair(name, string.Empty);
			int num = (int)Math.Floor((double)Math.Max(minutes, 0.0f) * 60.0);
			this.UpdateNameValuePair(name, minutes == float.MaxValue ? string.Empty : num.ToString());
		}

		public float GetTurnLength(GameDatabase.TurnLengthTypes type)
		{
			if (type == GameDatabase.TurnLengthTypes.Strategic)
				return float.MaxValue;
			string nameValue = this.GetNameValue(type == GameDatabase.TurnLengthTypes.Strategic ? "StrategicTurnLength" : "CombatTurnLength");
			int result;
			if (string.IsNullOrEmpty(nameValue) || !int.TryParse(nameValue, out result))
				return float.MaxValue;
			return (float)result / 60f;
		}

		public int GetSystemDefensePoints(int SystemID, int PlayerID)
		{
			StationInfo forSystemAndPlayer = this.GetNavalStationForSystemAndPlayer(SystemID, PlayerID);
			int num = 0;
			if (forSystemAndPlayer != null)
				num = this.GetDesignCommandPointQuota(this.assetdb, forSystemAndPlayer.DesignInfo.ID);
			if (this.GetColonyInfosForSystem(SystemID).ToList<ColonyInfo>().Count<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == PlayerID)) > 0)
				num += 3;
			return num;
		}

		public int GetAllocatedSystemDefensePoints(StarSystemInfo system, int playerId)
		{
			FleetInfo defenseFleetInfo = this.InsertOrGetDefenseFleetInfo(system.ID, playerId);
			if (defenseFleetInfo == null)
				return 0;
			return this.GetShipInfoByFleetID(defenseFleetInfo.ID, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.IsPlaced())).Sum<ShipInfo>((Func<ShipInfo, int>)(y => this.assetdb.DefenseManagerSettings.GetDefenseAssetCPCost(y.DesignInfo)));
		}

		public int GetDefenseAssetCPCost(int shipid)
		{
			ShipInfo shipInfo = this.GetShipInfo(shipid, false);
			if (shipInfo != null)
				return this.assetdb.DefenseManagerSettings.GetDefenseAssetCPCost(shipInfo.DesignInfo);
			return 0;
		}

		private CounterIntelResponse ParseCounterIntelResponse(Row row)
		{
			if (row == null)
				return (CounterIntelResponse)null;
			return new CounterIntelResponse()
			{
				ID = row[0].SQLiteValueToInteger(),
				IntelMissionID = row[1].SQLiteValueToInteger(),
				auto = row[2].SQLiteValueToBoolean(),
				value = row[3].ToSQLiteValue()
			};
		}

		public void InsertCounterIntelResponse(int intelmission_id, bool auto = true, string value = "")
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertCounterIntelResponse, (object)intelmission_id.ToSQLiteValue(), (object)auto.ToSQLiteValue(), (object)value), false, true);
		}

		public IEnumerable<CounterIntelResponse> GetCounterIntelResponses(
		  int intel_mission_id)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetCounterIntelResponsesForIntel, (object)intel_mission_id), true);
			foreach (Row row in t.Rows)
				yield return this.ParseCounterIntelResponse(row);
		}

		public void RemoveCounterIntelResponse(int responseid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveCounterIntelResponse, (object)responseid.ToSQLiteValue()), false, true);
		}

		private CounterIntelStingMission ParseIntelStingInfo(Row row)
		{
			if (row == null)
				return (CounterIntelStingMission)null;
			return new CounterIntelStingMission()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerId = row[1].SQLiteValueToInteger(),
				TargetPlayerId = row[2].SQLiteValueToInteger()
			};
		}

		public IEnumerable<CounterIntelStingMission> GetCountIntelStingsForPlayer(
		  int playerid)
		{
			GameDatabase.Warn("Enter GetCountIntelStingsForPlayer");
			// ISSUE: object of a compiler-generated type is created
			/*return (IEnumerable<CounterIntelStingMission>)new GameDatabase.< GetCountIntelStingsForPlayer > d__11(-2)
	
	  {

		<> 4__this = this,

		<> 3__playerid = playerid

	  };*/
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.SelectCounterIntelStingsForPlayer, (object)playerid), true);
			foreach (Row row in t.Rows)
				yield return this.ParseIntelStingInfo(row);
		}

		public IEnumerable<CounterIntelStingMission> GetCountIntelStingsAgainstPlayer(
	  int playerid)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.SelectCounterIntelStingsAgainstPlayer, (object)playerid), true);
			foreach (Row row in t.Rows)
				yield return this.ParseIntelStingInfo(row);
		}

		public IEnumerable<CounterIntelStingMission> GetCountIntelStingsForPlayerAgainstPlayer(
		  int playerid,
		  int targetplayer)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.SelectCounterIntelStingsForPlayerAgainstPlayer, (object)playerid, (object)targetplayer), true);
			foreach (Row row in t.Rows)
				yield return this.ParseIntelStingInfo(row);
		}

		public void InsertCounterIntelSting(int playerid, int targetplayerid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertCounterIntelSting, (object)playerid.ToSQLiteValue(), (object)targetplayerid.ToSQLiteValue()), false, true);
		}

		public void RemoveCounterIntelSting(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveCounterIntelSting, (object)id), false, true);
		}

		private IntelMissionInfo ParseIntelInfo(Row row)
		{
			if (row == null)
				return (IntelMissionInfo)null;
			return new IntelMissionInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerId = row[1].SQLiteValueToInteger(),
				TargetPlayerId = row[2].SQLiteValueToInteger(),
				BlamePlayer = row[3].SQLiteValueToNullableInteger(),
				Turn = row[4].SQLiteValueToInteger(),
				MissionType = (IntelMission)row[5].SQLiteValueToInteger()
			};
		}

		public IEnumerable<IntelMissionInfo> GetIntelInfosForPlayer(
		  int playerid)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelMissionsForPlayer, (object)playerid), true);
			foreach (Row row in t.Rows)
				yield return this.ParseIntelInfo(row);
		}

		public IntelMissionInfo GetIntelInfo(int intel_id)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.SelectIntelMissionInfo, (object)intel_id), true);
			if (source.Count<Row>() > 0)
				return this.ParseIntelInfo(source[0]);
			return (IntelMissionInfo)null;
		}

		public void InsertIntelMission(
		  int playerid,
		  int targetplayerid,
		  IntelMission type,
		  int? BlamePlayer = null)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelMission, (object)playerid.ToSQLiteValue(), (object)targetplayerid.ToSQLiteValue(), (object)this.GetTurnCount().ToSQLiteValue(), (object)((int)type).ToSQLiteValue(), (object)BlamePlayer.ToNullableSQLiteValue()), false, true);
		}

		public void RemoveIntelMission(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveIntelMission, (object)id.ToSQLiteValue()), false, true);
		}

		public int InsertLoaFleetComposition(int playerid, string Name, IEnumerable<int> Designs)
		{
			int num = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertLoaFleetComposition, (object)playerid.ToSQLiteValue(), (object)Name.ToSQLiteValue()));
			foreach (int design in Designs)
			{
				DesignInfo designInfo = this.GetDesignInfo(design);
				RealShipClasses? realShipClass1 = designInfo.GetRealShipClass();
				if ((realShipClass1.GetValueOrDefault() != RealShipClasses.Drone ? 0 : (realShipClass1.HasValue ? 1 : 0)) == 0)
				{
					RealShipClasses? realShipClass2 = designInfo.GetRealShipClass();
					if ((realShipClass2.GetValueOrDefault() != RealShipClasses.BoardingPod ? 0 : (realShipClass2.HasValue ? 1 : 0)) == 0)
					{
						RealShipClasses? realShipClass3 = designInfo.GetRealShipClass();
						if ((realShipClass3.GetValueOrDefault() != RealShipClasses.AssaultShuttle ? 0 : (realShipClass3.HasValue ? 1 : 0)) == 0 && !designInfo.IsLoaCube())
							this.db.ExecuteIntegerQuery(string.Format(Queries.InsertLoaFleetShipDef, (object)num, (object)design));
					}
				}
			}
			return num;
		}

		private LoaFleetComposition ParseLoaFleetComposition(Row row)
		{
			if (row == null)
				return (LoaFleetComposition)null;
			return new LoaFleetComposition()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToInteger(),
				Name = row[2].SQLiteValueToString()
			};
		}

		private LoaFleetShipDef ParseLoaFleetShipDef(Row row)
		{
			if (row == null)
				return (LoaFleetShipDef)null;
			return new LoaFleetShipDef()
			{
				ID = row[0].SQLiteValueToInteger(),
				CompositionID = row[1].SQLiteValueToInteger(),
				DesignID = row[2].SQLiteValueToInteger()
			};
		}

		public IEnumerable<LoaFleetComposition> GetLoaFleetCompositions()
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(Queries.SelectLoaFleetCompositions, true);
			List<LoaFleetComposition> fleetCompositionList = new List<LoaFleetComposition>();
			foreach (Row row in table)
				fleetCompositionList.Add(this.ParseLoaFleetComposition(row));
			foreach (LoaFleetComposition fleetComposition in fleetCompositionList)
			{
				fleetComposition.designs = new List<LoaFleetShipDef>();
				foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.SelectLoaShipDefForComposition, (object)fleetComposition.ID), true))
					fleetComposition.designs.Add(this.ParseLoaFleetShipDef(row));
			}
			return (IEnumerable<LoaFleetComposition>)fleetCompositionList;
		}

		public void DeleteLoaFleetCompositon(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DeleteLoaFleetCompositon, (object)id.ToSQLiteValue()), false, true);
		}

		public AssetDatabase AssetDatabase
		{
			get
			{
				return this.assetdb;
			}
		}

		public string LiveLocation
		{
			get
			{
				return this._location;
			}
		}

		internal void ChangeLiveLocationAndOpen(string value)
		{
			this.ChangeLiveLocation(value);
			ShellHelper.ShellOpen(value);
		}

		public void ChangeLiveLocation(string value)
		{
			this.db.SaveBackup(value);
			SQLiteConnection sqLiteConnection = new SQLiteConnection(value);
			this.db.Dispose();
			this.db = sqLiteConnection;
			this._location = value;
		}

		private int QueryVersion()
		{
			return int.Parse(this.GetNameValue("dbver"));
		}

		private GameDatabase(SQLiteConnection dbConnection, AssetDatabase assetdb)
		{
			if (dbConnection == null)
				throw new ArgumentNullException(nameof(dbConnection));
			this.db = dbConnection;
			this.assetdb = assetdb;
			this.db.ExecuteNonQuery("PRAGMA foreign_keys = TRUE;", true, true);
			this._dom = new DataObjectCache(this.db, assetdb);
		}

		public static GameDatabase New(
		  string gameName,
		  AssetDatabase assetdb,
		  bool initialize = true)
		{
			if (string.IsNullOrWhiteSpace(gameName))
				throw new ArgumentNullException("Game name must be a valid non-whitespace string.");
			gameName = gameName.Trim();
			SQLiteConnection dbConnection = new SQLiteConnection(":memory:");
			dbConnection.ExecuteNonQuery("PRAGMA synchronous = OFF;", false, true);
			dbConnection.ExecuteNonQuery("PRAGMA journal_mode = OFF;", false, true);
			string end;
			using (Stream manifestResourceStream = Assembly.GetCallingAssembly().GetManifestResourceStream("Kerberos.Sots.Data.empty_game.sql"))
			{
				using (StreamReader streamReader = new StreamReader(manifestResourceStream))
					end = streamReader.ReadToEnd();
			}
			dbConnection.ExecuteNonQueryReferenceUTF8(end);
			GameDatabase gameDatabase = new GameDatabase(dbConnection, assetdb);
			if (initialize)
			{
				gameDatabase.InsertNameValuePair("dbver", 2080.ToString());
				gameDatabase.InsertNameValuePair("game_name", gameName);
				gameDatabase.InsertNameValuePair("turn", 0.ToString());
				gameDatabase.InsertNameValuePair("time_stamp", (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString());
			}
			gameDatabase.ClearStratModCache();
			return gameDatabase;
		}

		public void QueryLogging(bool value)
		{
			this.db.LogQueries = true;
		}

		public void UpdateDBVer(int value)
		{
			this.UpdateNameValuePair("dbver", value.ToString());
		}

		public void SaveMultiplayerSyncPoint(string cacheDir)
		{
			this.Save(Path.Combine(cacheDir, "network.db"));
		}

		public static GameDatabase Load(string filename, AssetDatabase assetdb)
		{
			SQLiteConnection dbConnection = new SQLiteConnection(":memory:");
			dbConnection.ExecuteNonQuery("PRAGMA synchronous = OFF;", false, true);
			dbConnection.ExecuteNonQuery("PRAGMA journal_mode = OFF;", false, true);
			dbConnection.LoadBackup(filename);
			GameDatabase gameDatabase = new GameDatabase(dbConnection, assetdb);
			int dbver = gameDatabase.QueryVersion();
			if (dbver > 2080)
				throw new InvalidDataException(string.Format(AssetDatabase.CommonStrings.Localize("@ERROR_UNSUPPORTED_SAVE_GAME"), (object)2080, (object)dbver));
			if (dbver < 2000)
			{
				dbConnection.Dispose();
				throw new InvalidDataException(string.Format(AssetDatabase.CommonStrings.Localize("@PRE_EOF_SAVEGAME_UNSUPPORTED"), (object)2000, (object)dbver));
			}
			if (dbver < 2080)
				gameDatabase.Upgrade(dbver);
			gameDatabase.ClearStratModCache();
			if (ScriptHost.AllowConsole)
				gameDatabase.ValidateShipParents();
			return gameDatabase;
		}

		public static GameDatabase Connect(string filename, AssetDatabase assetdb)
		{
			return new GameDatabase(new SQLiteConnection(filename), assetdb);
		}

		private bool IncrementalUpgrade(int dbver)
		{
			if (dbver <= 2000)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n\r\n                    CREATE TABLE [gives]\r\n                    (\r\n\t                    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,\r\n\t                    [initiating_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,\r\n\t                    [receiving_player_id] INTEGER NOT NULL REFERENCES [players](id) ON UPDATE CASCADE,\r\n\t                    [give_type] INTEGER NOT NULL,\r\n\t                    [give_value] NUMERIC\r\n                    );\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2010);
				return true;
			}
			if (dbver == 2010)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE loa_fleet_compositions ADD COLUMN [visible] BOOLEAN NOT NULL DEFAULT 'True';\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2020);
				return true;
			}
			if (dbver == 2020)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE invoice_build_orders ADD COLUMN [loa_cubes]\tINTEGER NOT NULL DEFAULT 0;\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2030);
				return true;
			}
			if (dbver == 2030)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE players ADD COLUMN [auto_patrol] BOOLEAN NOT NULL DEFAULT 'False';\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2040);
				return true;
			}
			if (dbver == 2040)
			{
				this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE players ADD COLUMN [ai_difficulty] TEXT NOT NULL DEFAULT 'Normal';\r\n\r\n                    COMMIT;\r\n                ", true, true);
				this.UpdateDBVer(2050);
				return true;
			}
			if (dbver == 2050)
			{
				foreach (ShipInfo shipInfo in this.GetShipInfos(true).ToList<ShipInfo>())
				{
					ShipRole role = DesignLab.GetRole(((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission)).ShipSectionAsset);
					if (shipInfo.DesignInfo.Role != role)
					{
						shipInfo.DesignInfo.Role = role;
						this.UpdateDesign(shipInfo.DesignInfo);
					}
				}
				this.UpdateDBVer(2060);
				return true;
			}
			if (dbver == 2060)
			{
				foreach (ShipInfo shipInfo in this.GetShipInfos(true).ToList<ShipInfo>())
				{
					if (shipInfo.DesignInfo.Role != ShipRole.E_WARFARE)
					{
						ShipRole role = DesignLab.GetRole(((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission)).ShipSectionAsset);
						if (shipInfo.DesignInfo.Role != role)
						{
							shipInfo.DesignInfo.Role = role;
							this.UpdateDesign(shipInfo.DesignInfo);
						}
					}
				}
				this.UpdateDBVer(2070);
				return true;
			}
			if (dbver != 2070)
				return false;
			this.db.ExecuteNonQuery("\r\n                    BEGIN TRANSACTION;            \r\n                    \r\n                    ALTER TABLE government_actions ADD COLUMN [turn] INTEGER NOT NULL DEFAULT 0;\r\n                    ALTER TABLE players ADD COLUMN [rate_tax_prev] NUMERIC NOT NULL DEFAULT '0.05';\r\n\r\n                    COMMIT;\r\n                ", true, true);
			foreach (PlayerInfo playerInfo in this.GetStandardPlayerInfos().ToList<PlayerInfo>())
				this.UpdatePreviousTaxRate(playerInfo.ID, playerInfo.RateTax);
			this.UpdateDBVer(2080);
			return true;
		}

		public static bool CheckForPre_EOFSaves(App App)
		{
			SavedGameFilename[] allSaveGames = App.GetAllSaveGames();
			bool flag = false;
			foreach (SavedGameFilename savedGameFilename in allSaveGames)
			{
				using (GameDatabase gameDatabase = GameDatabase.Connect(savedGameFilename.RootedFilename, App.AssetDatabase))
				{
					if (gameDatabase.QueryVersion() < 2000)
					{
						gameDatabase.Dispose();
						File.Delete(savedGameFilename.RootedFilename);
						flag = true;
					}
				}
			}
			return flag;
		}

		private void FixGenUniqueTable(string table, string column)
		{
			int[] numArray = this.db.ExecuteIntegerArrayQuery(string.Format("SELECT {0} FROM {1} ORDER BY {0} DESC;", (object)column.ToSQLiteValue(), (object)table.ToSQLiteValue()));
			HashSet<int> intSet = new HashSet<int>();
			for (int index = 0; index < 16; ++index)
				intSet.Add(index);
			foreach (int num1 in numArray)
			{
				if (intSet.Count == 0)
					break;
				int num2 = num1 & 15;
				if (intSet.Contains(num2))
				{
					intSet.Remove(num2);
					this.db.ExecuteNonQuery(string.Format("INSERT OR IGNORE INTO gen_unique (generator, player_id) VALUES ('{0}', {1});", (object)table.ToSQLiteValue(), (object)num2.ToSQLiteValue()), true, true);
					this.db.ExecuteNonQuery(string.Format("UPDATE gen_unique SET current = {0} WHERE generator = '{1}' AND player_id = {2};", (object)(num1 >> 4).ToSQLiteValue(), (object)table.ToSQLiteValue(), (object)num2.ToSQLiteValue()), true, true);
				}
			}
		}

		private void ValidateBuildOrderPlayers()
		{
			this.RemoveBuildOrder(0);
			foreach (int buildOrderID in this.db.ExecuteIntegerArrayQuery("SELECT DISTINCT build_orders.id FROM build_orders\r\n                JOIN missions ON build_orders.mission_id=missions.id\r\n                JOIN designs ON build_orders.design_id=designs.id\r\n                JOIN fleets ON missions.fleet_id=fleets.id\r\n                WHERE fleets.player_id<>designs.player_id;"))
			{
				GameDatabase.Warn(string.Format("Removing build order {0} because design owner doesn't match fleet owner!", (object)buildOrderID));
				this.RemoveBuildOrder(buildOrderID);
			}
			int num = ScriptHost.AllowConsole ? 1 : 0;
		}

		private void ValidateShipParents()
		{
			foreach (int shipID in this.db.ExecuteIntegerArrayQuery("SELECT ships.id FROM ships\r\n                JOIN designs ON designs.id=ships.design_id\r\n                JOIN ships AS parent_ships ON ships.parent_id=parent_ships.id\r\n                JOIN designs AS parent_designs ON parent_designs.id=parent_ships.design_id\r\n                WHERE (ships.parent_id<>0 AND designs.player_id<>parent_designs.player_id)\r\n                ORDER BY designs.player_id;"))
			{
				GameDatabase.Warn(string.Format("Removing ship {0} because it was parented to another player's ship!", (object)shipID));
				this.RemoveShip(shipID);
			}
			int num = ScriptHost.AllowConsole ? 1 : 0;
		}

		private void FixShipStructures()
		{
			foreach (ShipInfo shipInfo in this.GetShipInfos(true).ToList<ShipInfo>())
			{
				foreach (SectionInstanceInfo sectionInstanceInfo in this.GetShipSectionInstances(shipInfo.ID).ToList<SectionInstanceInfo>())
				{
					SectionInstanceInfo sect = sectionInstanceInfo;
					DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ID == sect.SectionID));
					ShipSectionAsset shipSectionAsset = this.assetdb.GetShipSectionAsset(designSectionInfo.FilePath);
					List<string> techs = new List<string>();
					if (designSectionInfo.Techs.Count > 0)
					{
						foreach (int tech in designSectionInfo.Techs)
							techs.Add(this.GetTechFileID(tech));
					}
					int structureWithTech = Ship.GetStructureWithTech(this.assetdb, techs, shipSectionAsset.Structure);
					int minStructure = designSectionInfo.GetMinStructure(this, this.assetdb);
					if (sect.Structure > structureWithTech)
					{
						sect.Structure = structureWithTech;
						this.UpdateSectionInstance(sect);
					}
					else if (sect.Structure < minStructure)
					{
						sect.Structure = minStructure;
						this.UpdateSectionInstance(sect);
					}
					foreach (ModuleInstanceInfo module in this.GetModuleInstances(sect.ID).ToList<ModuleInstanceInfo>())
					{
						if (module.Structure < 0)
						{
							module.Structure = 0;
							this.UpdateModuleInstance(module);
						}
					}
					foreach (WeaponInstanceInfo weapon in this.GetWeaponInstances(sect.ID).ToList<WeaponInstanceInfo>())
					{
						if ((double)weapon.Structure < 0.0)
						{
							weapon.Structure = 0.0f;
							this.UpdateWeaponInstance(weapon);
						}
					}
				}
			}
		}

		private void FixStationModuleInstances()
		{
			foreach (StationInfo stationInfo in this.GetStationInfos().ToList<StationInfo>())
			{
				StationInfo si = stationInfo;
				if (si.DesignInfo.StationLevel >= 2)
				{
					SectionInstanceInfo sectionInstanceInfo = this.GetShipSectionInstances(si.ShipID).FirstOrDefault<SectionInstanceInfo>();
					List<ModuleInstanceInfo> source = sectionInstanceInfo != null ? this.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>() : new List<ModuleInstanceInfo>();
					List<LogicalModuleMount> list1 = ((IEnumerable<LogicalModuleMount>)this.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == si.DesignInfo.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
					foreach (DesignModuleInfo module1 in si.DesignInfo.DesignSections[0].Modules)
					{
						DesignModuleInfo module = module1;
						ModuleInstanceInfo mii = source.FirstOrDefault<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == module.MountNodeName));
						if (mii != null)
						{
							source.Remove(mii);
							list1.RemoveAll((Predicate<LogicalModuleMount>)(x => x.NodeName == mii.ModuleNodeID));
						}
					}
					if (source.Count != 0)
					{
						DesignInfo oldDesign = DesignLab.CreateStationDesignInfo(this.AssetDatabase, this, si.PlayerID, si.DesignInfo.StationType, si.DesignInfo.StationLevel - 1, false);
						string name = this.AssetDatabase.GetFaction(this.GetPlayerFactionID(si.PlayerID)).Name;
						StationModuleQueue.UpdateStationMapsForFaction(name);
						List<LogicalModuleMount> list2 = ((IEnumerable<LogicalModuleMount>)this.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == oldDesign.DesignSections[0].FilePath)).Modules).ToList<LogicalModuleMount>();
						foreach (ModuleInstanceInfo moduleInstanceInfo in source)
						{
							ModuleInstanceInfo moduleInst = moduleInstanceInfo;
							LogicalModuleMount logicalModuleMount1 = list2.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.NodeName == moduleInst.ModuleNodeID));
							if (logicalModuleMount1 != null)
							{
								ModuleEnums.StationModuleType type = (ModuleEnums.StationModuleType)Enum.Parse(typeof(ModuleEnums.StationModuleType), logicalModuleMount1.ModuleType);
								ModuleEnums.ModuleSlotTypes desiredModuleType = AssetDatabase.StationModuleTypeToMountTypeMap[type];
								if (desiredModuleType == ModuleEnums.ModuleSlotTypes.Habitation && name != AssetDatabase.GetModuleFactionName(type))
									desiredModuleType = ModuleEnums.ModuleSlotTypes.AlienHabitation;
								LogicalModuleMount logicalModuleMount2 = list1.FirstOrDefault<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.ModuleType == desiredModuleType.ToString()));
								if (logicalModuleMount2 != null)
								{
									moduleInst.ModuleNodeID = logicalModuleMount2.NodeName;
									this.UpdateModuleInstance(moduleInst);
									list1.Remove(logicalModuleMount2);
								}
							}
						}
					}
				}
			}
		}

		private void ValidateGardenerFleets()
		{
			int playerId = Gardeners.GetPlayerID(this);
			foreach (GardenerInfo gi in this.GetGardenerInfos().ToList<GardenerInfo>())
			{
				FleetInfo fleetInfo = this.GetFleetsByPlayerAndSystem(playerId, gi.SystemId, FleetType.FL_NORMAL).ToList<FleetInfo>().FirstOrDefault<FleetInfo>();
				if (fleetInfo != null)
				{
					gi.FleetId = fleetInfo.ID;
					this.UpdateGardenerInfo(gi);
				}
			}
		}

		private void Upgrade(int dbver)
		{
			for (int dbver1 = dbver; dbver1 < 2080; dbver1 = this.QueryVersion())
			{
				if (!this.IncrementalUpgrade(dbver1))
					throw new InvalidDataException("Cannot upgrade save game: Database version " + (object)dbver + " is not supported.");
			}
		}

		internal void SaveAndOpen(string filename)
		{
			this.Save(filename);
			ShellHelper.ShellOpen(filename);
		}

		private static void Trace(string message)
		{
			App.Log.Trace(message, "data");
		}

		private static void TraceVerbose(string message)
		{
			App.Log.Trace(message, "data", Kerberos.Sots.Engine.LogLevel.Verbose);
		}

		private static void Warn(string message)
		{
			App.Log.Warn(message, "data");
		}

		public void Save(string filename)
		{
			GameDatabase.Trace("Saving game database to '" + filename + "'...");
			try
			{
				this.db.SaveBackup(filename);
			}
			catch (Exception ex)
			{
				GameDatabase.Trace("FAILED.");
				throw;
			}
			GameDatabase.Trace("OK.");
		}

		public void ClearStratModCache()
		{
			GameDatabase._cachedStratMods.Clear();
		}

		public void Dispose()
		{
			this.db.Dispose();
		}

		public IntPtr GetDbPointer()
		{
			return this.db.GetDbPointer();
		}

		public void ReplaceMapWithExtremeStars()
		{
			string str1 = new StellarClass(Kerberos.Sots.StellarType.O, 0, Kerberos.Sots.StellarSize.VII).ToString();
			string str2 = new StellarClass(Kerberos.Sots.StellarType.M, 0, Kerberos.Sots.StellarSize.Ia).ToString();
			Random random = new Random();
			List<int> list = this.GetStarSystemIDs().ToList<int>();
			this._dom.star_systems.Clear();
			foreach (int num in list)
				this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemStellarClass, (object)num.ToSQLiteValue(), (object)(random.CoinToss(50) ? str1 : str2).ToSQLiteValue()), false, true);
		}

		private void UpdateAIIntelColony(
		  int playerID,
		  int planetID,
		  int owningPlayerID,
		  int colonyID,
		  double imperialPopulation)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIIntelColony, (object)playerID.ToSQLiteValue(), (object)owningPlayerID.ToSQLiteValue(), (object)planetID.ToSQLiteValue(), (object)colonyID.ToSQLiteValue(), (object)this.GetTurnCount().ToSQLiteValue(), (object)imperialPopulation.ToSQLiteValue()), true, true);
		}

		private void UpdateAIIntelPlanet(
		  int playerID,
		  int planetID,
		  int biosphere,
		  int resources,
		  float infrastructure,
		  float suitability)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIIntelPlanet, (object)playerID.ToSQLiteValue(), (object)planetID.ToSQLiteValue(), (object)this.GetTurnCount().ToSQLiteValue(), (object)biosphere.ToSQLiteValue(), (object)resources.ToSQLiteValue(), (object)infrastructure.ToSQLiteValue(), (object)suitability.ToSQLiteValue()), true, true);
		}

		public void PurgeOwnedColonyIntel(int playerID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.PurgeOwnedColonyIntel, (object)playerID.ToSQLiteValue()), false, true);
		}

		public void ShareSensorData(int receivingPlayer, int sourcePlayer)
		{
			foreach (int starSystemId in this.GetStarSystemIDs())
			{
				ExploreRecordInfo exploreRecord1 = this.GetExploreRecord(starSystemId, sourcePlayer);
				if (exploreRecord1 != null)
				{
					ExploreRecordInfo exploreRecord2 = this.GetExploreRecord(starSystemId, receivingPlayer);
					if (exploreRecord2 == null)
					{
						this.InsertExploreRecord(starSystemId, receivingPlayer, exploreRecord1.LastTurnExplored, exploreRecord1.Visible, exploreRecord1.Explored);
					}
					else
					{
						exploreRecord2.LastTurnExplored = Math.Max(exploreRecord1.LastTurnExplored, exploreRecord2.LastTurnExplored);
						exploreRecord2.Visible = exploreRecord1.Visible || exploreRecord2.Visible;
						exploreRecord2.Explored = exploreRecord1.Explored || exploreRecord2.Explored;
						this.UpdateExploreRecord(exploreRecord2);
					}
				}
			}
			foreach (AIColonyIntel aiColonyIntel in this.GetColonyIntelsForPlayer(sourcePlayer))
			{
				if (aiColonyIntel.ColonyID.HasValue)
				{
					AIColonyIntel colonyIntelForPlanet = this.GetColonyIntelForPlanet(receivingPlayer, aiColonyIntel.PlanetID);
					if (colonyIntelForPlanet == null || colonyIntelForPlanet.LastSeen < aiColonyIntel.LastSeen)
						this.UpdateAIIntelColony(receivingPlayer, aiColonyIntel.PlanetID, aiColonyIntel.OwningPlayerID, aiColonyIntel.ColonyID.Value, aiColonyIntel.ImperialPopulation);
				}
			}
			foreach (AIPlanetIntel aiPlanetIntel in this.GetPlanetIntelsForPlayer(sourcePlayer))
			{
				AIPlanetIntel planetIntel = this.GetPlanetIntel(receivingPlayer, aiPlanetIntel.PlanetID);
				if (planetIntel == null || planetIntel.LastSeen < aiPlanetIntel.LastSeen)
					this.UpdateAIIntelPlanet(receivingPlayer, aiPlanetIntel.PlanetID, aiPlanetIntel.Biosphere, aiPlanetIntel.Resources, aiPlanetIntel.Infrastructure, aiPlanetIntel.Suitability);
			}
		}

		public void UpdatePlayerViewWithStarSystem(int playerID, int systemID)
		{
			List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)this.GetStarSystemPlanetInfos(systemID)).ToList<PlanetInfo>();
			foreach (PlanetInfo planetInfo in list)
				this.db.ExecuteNonQuery(string.Format(Queries.RemoveNullAIColonyIntel, (object)playerID.ToSQLiteValue(), (object)planetInfo.ID.ToSQLiteValue()), true, true);
			foreach (ColonyInfo colonyInfo in this.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>())
				this.UpdateAIIntelColony(playerID, colonyInfo.OrbitalObjectID, colonyInfo.PlayerID, colonyInfo.ID, colonyInfo.ImperialPop);
			foreach (PlanetInfo planetInfo in list)
				this.UpdateAIIntelPlanet(playerID, planetInfo.ID, planetInfo.Biosphere, planetInfo.Resources, planetInfo.Infrastructure, planetInfo.Suitability);
		}

		public void LogComment(string comment)
		{
			this.db.LogComment(comment);
		}

		public List<string> GetUniqueGenerators()
		{
			List<string> stringList = new List<string>();
			foreach (Row row in this.db.ExecuteTableQuery(Queries.GetUniqueGenerators, false).Rows)
				stringList.Add(row[0]);
			return stringList;
		}

		private List<string> GetDatabaseHistoryCore(int? turn, out int lastId, int? id = null)
		{
			if (!turn.HasValue && id.HasValue)
				throw new ArgumentException("No query supports ID without a Turn.", nameof(turn));
			lastId = !id.HasValue ? 0 : id.Value;
			Kerberos.Sots.Data.SQLite.Table table = !turn.HasValue ? this.db.ExecuteTableQuery(Queries.GetDatabaseHistory, true) : (!id.HasValue ? this.db.ExecuteTableQuery(string.Format(Queries.GetDatabaseHistoryByTurn, (object)turn), true) : this.db.ExecuteTableQuery(string.Format(Queries.GetDatabaseHistoryByTurnAndId, (object)turn, (object)id), true));
			List<string> stringList = new List<string>();
			foreach (Row row in table.Rows)
			{
				string str = row[2];
				stringList.Add(str);
			}
			if (table.Rows.Length > 0)
				lastId = table.Rows[table.Rows.Length - 1][0].SQLiteValueToInteger();
			return stringList;
		}

		public List<string> GetDatabaseHistory(out int lastId)
		{
			return this.GetDatabaseHistoryCore(new int?(), out lastId, new int?());
		}

		public List<string> GetDatabaseHistoryForTurn(int turn, out int lastId, int? id = null)
		{
			return this.GetDatabaseHistoryCore(new int?(turn), out lastId, id);
		}

		public void InsertTurnOne()
		{
			this.UpdateNameValuePair("turn", 1.ToString());
		}

		public bool HasEndOfFleshExpansion()
		{
			return true;
		}

		public void SetClientId(int id)
		{
			this._clientId = id;
			this.db.LogComment("Setting client ID to: " + (object)id);
			this.db.ExecuteNonQuery(string.Format(Queries.SetClientID, (object)id), true, true);
		}

		public int GetClientId()
		{
			return this._clientId;
		}

		public int InsertWeapon(LogicalWeapon weapon, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertWeapon, (object)playerId, (object)weapon.FileName));
		}

		public IEnumerable<LogicalWeapon> GetAvailableWeapons(
		  AssetDatabase assetdb,
		  int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerWeapons, (object)playerId), true);
			return assetdb.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => ((IEnumerable<Row>)t.Rows).Any<Row>((Func<Row, bool>)(y => y[0] == x.FileName))));
		}

		public void RemoveWeapon(int? weaponId)
		{
			if (!weaponId.HasValue)
				return;
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveWeapon, (object)weaponId), false, true);
		}

		public IEnumerable<LogicalModule> GetAvailableModules(
		  AssetDatabase assetdb,
		  int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerModules, (object)playerId), true);
			return assetdb.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => ((IEnumerable<Row>)t.Rows).Any<Row>((Func<Row, bool>)(y => y[0] == x.ModulePath))));
		}

		public int InsertModule(LogicalModule module, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertModule, (object)module.ModulePath, (object)playerId));
		}

		public void RemoveModule(int moduleId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveModule, (object)moduleId), false, true);
		}

		public int InsertSpecialProject(
		  int playerId,
		  string name,
		  int cost,
		  SpecialProjectType type,
		  int techid = 0,
		  int encounterid = 0,
		  int fleetid = 0,
		  int targetplayerid = 0)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSpecialProject, (object)playerId.ToOneBasedSQLiteValue(), (object)name.ToSQLiteValue(), (object)cost.ToSQLiteValue(), (object)((int)type).ToSQLiteValue(), (object)techid.ToSQLiteValue(), (object)encounterid.ToOneBasedSQLiteValue(), (object)fleetid.ToOneBasedSQLiteValue(), (object)targetplayerid.ToOneBasedSQLiteValue()));
		}

		public void RemoveSpecialProject(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSpecialProject, (object)id), false, true);
		}

		public SpecialProjectInfo GetSpecialProjectInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSpecialProjectInfo, (object)id), true);
			if (source.Count<Row>() > 0)
				return this.ParseSpecialProjectInfo(source[0]);
			return (SpecialProjectInfo)null;
		}

		public bool GetHasPlayerStudiedIndependentRace(int Playerid, int IndependentRacePlayerID)
		{
			foreach (Row r in this.db.ExecuteTableQuery(string.Format(Queries.GetCompleteSpecialProjectsByPlayerID, (object)Playerid), true))
			{
				SpecialProjectInfo specialProjectInfo = this.ParseSpecialProjectInfo(r);
				if (specialProjectInfo.Type == SpecialProjectType.IndependentStudy && specialProjectInfo.TargetPlayerID == IndependentRacePlayerID)
					return true;
			}
			return false;
		}

		public bool GetHasPlayerStudyingIndependentRace(int Playerid, int IndependentRacePlayerID)
		{
			foreach (Row r in this.db.ExecuteTableQuery(string.Format(Queries.GetSpecialProjectInfosByPlayerID, (object)Playerid), true))
			{
				SpecialProjectInfo specialProjectInfo = this.ParseSpecialProjectInfo(r);
				if (specialProjectInfo.Type == SpecialProjectType.IndependentStudy && specialProjectInfo.TargetPlayerID == IndependentRacePlayerID)
					return true;
			}
			return false;
		}

		public bool GetHasPlayerStudiedSpecialProject(int Playerid, SpecialProjectType type)
		{
			foreach (Row r in this.db.ExecuteTableQuery(string.Format(Queries.GetCompleteSpecialProjectsByPlayerID, (object)Playerid), true))
			{
				if (this.ParseSpecialProjectInfo(r).Type == type)
					return true;
			}
			return false;
		}

		public bool GetHasPlayerStudyingSpecialProject(int Playerid, SpecialProjectType type)
		{
			foreach (Row r in this.db.ExecuteTableQuery(string.Format(Queries.GetSpecialProjectInfosByPlayerID, (object)Playerid), true))
			{
				if (this.ParseSpecialProjectInfo(r).Type == type)
					return true;
			}
			return false;
		}

		public IEnumerable<SpecialProjectInfo> GetSpecialProjectInfosByPlayerID(
		  int playerid,
		  bool onlyIncomplete)
		{
			foreach (Row r in this.db.ExecuteTableQuery(string.Format(onlyIncomplete ? Queries.GetIncompleteSpecialProjectInfosByPlayerID : Queries.GetSpecialProjectInfosByPlayerID, (object)playerid), true))
			{
				SpecialProjectInfo info = this.ParseSpecialProjectInfo(r);
				yield return info;
			}
		}

		public void UpdateSpecialProjectProgress(int id, int progress)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSpecialProjectProgress, (object)id, (object)progress), false, true);
		}

		public void UpdateSpecialProjectRate(int id, float rate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSpecialProjectRate, (object)id, (object)rate.ToSQLiteValue()), false, true);
		}

		public void UpdateStarSystemVisible(int id, bool isVisible)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateStarSystemVisible, (object)id.ToSQLiteValue(), (object)isVisible.ToSQLiteValue()), false, true);
		}

		public void UpdateStarSystemOpen(int id, bool isOpen)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateStarSystemOpen, (object)id.ToSQLiteValue(), (object)isOpen.ToSQLiteValue()), false, true);
		}

		public int InsertStarSystem(
		  int? id,
		  string name,
		  int? provinceId,
		  string stellarClass,
		  Vector3 origin,
		  bool isVisible,
		  bool isOpen = true,
		  int? terrainID = null)
		{
			this._dom.star_systems.Clear();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStarSystem, (object)(!id.HasValue ? this.db.ExecuteIntegerQuery(string.Format(Queries.InsertNewStellarInfo, (object)origin.ToSQLiteValue())) : this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStellarInfo, (object)id.Value.ToSQLiteValue(), (object)origin.ToSQLiteValue()))).ToSQLiteValue(), (object)name.ToSQLiteValue(), (object)provinceId.ToNullableSQLiteValue(), (object)stellarClass.ToSQLiteValue(), (object)isVisible.ToSQLiteValue(), (object)terrainID.ToNullableSQLiteValue(), (object)isOpen.ToSQLiteValue()));
		}

		public int InsertAsteroidBelt(
		  int? parentOrbitalObjectId,
		  int starSystemId,
		  OrbitalPath path,
		  string name,
		  int randomSeed)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertAsteroidBelt, (object)parentOrbitalObjectId.ToNullableSQLiteValue(), (object)starSystemId.ToSQLiteValue(), (object)path.ToString().ToSQLiteValue(), (object)name.ToNullableSQLiteValue(), (object)randomSeed.ToSQLiteValue()));
		}

		public int InsertLargeAsteroid(
		  int parentOrbitalObjectId,
		  int starSystemId,
		  OrbitalPath path,
		  string name,
		  int resources)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertLargeAsteroid, (object)parentOrbitalObjectId.ToSQLiteValue(), (object)starSystemId.ToSQLiteValue(), (object)path.ToString().ToSQLiteValue(), (object)name.ToNullableSQLiteValue(), (object)resources.ToSQLiteValue()));
		}

		public void UpdateLargeAsteroidInfo(LargeAsteroidInfo lai)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLargeAsteroidInfo, (object)lai.ID.ToSQLiteValue(), (object)lai.Resources.ToSQLiteValue()), false, true);
		}

		public void ExecuteDatabaseHistory(IDictionary<int, List<string>> history)
		{
			this._dom.Clear();
			GameDatabase._cachedStratMods.Clear();
			foreach (KeyValuePair<int, List<string>> keyValuePair in (IEnumerable<KeyValuePair<int, List<string>>>)history)
			{
				this.SetClientId(keyValuePair.Key);
				GameDatabase.Trace(string.Format("Executing {0} lines of DB history as {1}.", (object)keyValuePair.Value.Count, (object)this.GetClientId()));
				foreach (string query in keyValuePair.Value)
					this.db.ExecuteNonQuery(query, false, true);
			}
		}

		public int InsertPlanet(
		  int? parentOrbitalObjectId,
		  int starSystemId,
		  OrbitalPath path,
		  string name,
		  string type,
		  int? ringId,
		  float suitability,
		  int biosphere,
		  int resources,
		  float size)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertPlanet, (object)parentOrbitalObjectId.ToNullableSQLiteValue(), (object)starSystemId.ToSQLiteValue(), (object)path.ToString().ToSQLiteValue(), (object)name.ToNullableSQLiteValue(), (object)type.ToSQLiteValue(), (object)ringId.ToNullableSQLiteValue(), (object)suitability.ToSQLiteValue(), (object)biosphere.ToSQLiteValue(), (object)resources.ToSQLiteValue(), (object)size.ToSQLiteValue()));
		}

		public int InsertOrbitalObject(
		  int? parentOrbitalObjectId,
		  int starSystemId,
		  OrbitalPath path,
		  string name)
		{
			path.VerifyFinite();
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertOrbitalObject, (object)parentOrbitalObjectId.ToNullableSQLiteValue(), (object)starSystemId.ToSQLiteValue(), (object)path.ToString().ToSQLiteValue(), (object)name.ToNullableSQLiteValue()));
		}

		public int InsertColony(
		  int orbitalObjectID,
		  int playerID,
		  double impPop,
		  float civWeight,
		  int turn,
		  float infrastructure,
		  bool paintsystem = true)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalObjectID);
			if ((double)infrastructure > 0.0)
				this.UpdatePlanetInfrastructure(orbitalObjectID, infrastructure);
			if (paintsystem)
				Kerberos.Sots.GameStates.StarSystem.PaintSystemPlayerColor(this, this.GetOrbitalObjectInfo(orbitalObjectID).StarSystemID, playerID);
			int num = this._dom.colonies.Insert(new int?(), new ColonyInfo()
			{
				OrbitalObjectID = orbitalObjectID,
				PlayerID = playerID,
				ImperialPop = impPop,
				CivilianWeight = civWeight,
				TurnEstablished = turn
			});
			if (!this.GetReserveFleetID(playerID, orbitalObjectInfo.StarSystemID).HasValue)
				this.InsertReserveFleet(playerID, orbitalObjectInfo.StarSystemID);
			ColonyTrapInfo trapInfoByPlanetId = this.GetColonyTrapInfoByPlanetID(orbitalObjectID);
			if (trapInfoByPlanetId != null)
				this.RemoveColonyTrapInfo(trapInfoByPlanetId.ID);
			return num;
		}

		public int InsertStation(
		  int parentOrbitalObjectID,
		  int starSystemID,
		  OrbitalPath path,
		  string name,
		  int playerID,
		  DesignInfo design)
		{
			path.VerifyFinite();
			int num1 = this.InsertShip(0, design.ID, design.Name, (ShipParams)0, new int?(), 0);
			int num2 = this._dom.stations.Insert(new int?(), new StationInfo()
			{
				DesignInfo = design,
				OrbitalObjectInfo = new OrbitalObjectInfo()
				{
					Name = name,
					OrbitalPath = path,
					ParentID = new int?(parentOrbitalObjectID),
					StarSystemID = starSystemID
				},
				ShipID = num1,
				PlayerID = playerID
			});
			if (design.StationType == StationType.NAVAL)
				this.InsertOrGetReserveFleetID(starSystemID, playerID);
			return num2;
		}

		public void UpdateSystemCombatZones(int systemID, List<int> zones)
		{
			this._dom.star_systems.Clear();
			StringBuilder stringBuilder = new StringBuilder();
			for (int index = 0; index < zones.Count; ++index)
			{
				stringBuilder.Append(zones[index]);
				if (index != zones.Count - 1)
					stringBuilder.Append(",");
			}
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemCombatZones, (object)stringBuilder.ToString(), (object)systemID), false, true);
		}

		public void UpdateStation(StationInfo station)
		{
			this.UpdateDesign(station.DesignInfo);
			this.UpdateDesignSection(station.DesignInfo.DesignSections[0]);
			this._dom.stations.Update(station.OrbitalObjectInfo.ID, station);
		}

		public void UpdateDesign(DesignInfo design)
		{
			this._dom.designs.Update(design.ID, design);
			this._dom.CachedPlayerDesignNames.Clear();
		}

		public void RemoveDesign(int id)
		{
			this._dom.designs.Remove(id);
			this._dom.CachedPlayerDesignNames.Clear();
		}

		public void UpdateDesignSection(DesignSectionInfo dsi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignSection, (object)dsi.ID, (object)dsi.FilePath), false, true);
			this._dom.designs.Sync(dsi.DesignInfo.ID);
		}

		private void InsertNewShipSectionInstances(DesignInfo designInfo, int? shipId, int? stationId)
		{
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = this.assetdb.GetShipSectionAsset(designSection.FilePath);
				List<string> techs = new List<string>();
				if (designSection.Techs.Count > 0)
				{
					foreach (int tech in designSection.Techs)
						techs.Add(this.GetTechFileID(tech));
				}
				int supplyWithTech = Ship.GetSupplyWithTech(this.assetdb, techs, shipSectionAsset.Supply);
				int structureWithTech = Ship.GetStructureWithTech(this.assetdb, techs, shipSectionAsset.Structure);
				int num = this.InsertSectionInstance(designSection.ID, shipId, stationId, structureWithTech, (float)supplyWithTech, shipSectionAsset.Crew, shipSectionAsset.Signature, shipSectionAsset.RepairPoints);
				this.InsertNewArmorInstances(shipSectionAsset, this.GetDesignAttributesForDesign(designInfo.ID).ToList<SectionEnumerations.DesignAttribute>(), num);
				this.InsertNewShipWeaponInstancesForSection(((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).ToList<LogicalMount>(), designSection.WeaponBanks.ToList<WeaponBankInfo>(), num);
				if (designSection.Modules != null)
					this.InsertNewShipModuleInstances(shipSectionAsset, designSection.Modules, num);
			}
		}

		private void InsertNewArmorInstances(
		  ShipSectionAsset sectionAsset,
		  List<SectionEnumerations.DesignAttribute> attributes,
		  int sectionId)
		{
			Dictionary<ArmorSide, DamagePattern> dictionary = new Dictionary<ArmorSide, DamagePattern>();
			int num = 0;
			foreach (Kerberos.Sots.Framework.Size size in sectionAsset.Armor)
			{
				DamagePattern freshArmor = sectionAsset.CreateFreshArmor((ArmorSide)num, Ship.CalcArmorWidthModifier(attributes, 0));
				if (freshArmor.Height != 0 && freshArmor.Width != 0)
				{
					dictionary[(ArmorSide)num] = freshArmor;
					++num;
				}
			}
			this._dom.armor_instances.Insert(new int?(sectionId), dictionary);
		}

		private void InsertNewShipWeaponInstancesForSection(
		  List<LogicalMount> mounts,
		  List<WeaponBankInfo> banks,
		  int sectionInstId)
		{
			int index = 0;
			foreach (LogicalMount mount1 in mounts)
			{
				LogicalMount mount = mount1;
				if (!WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
				{
					float turretHealth = Ship.GetTurretHealth(mount.Bank.TurretSize);
					WeaponBankInfo weaponBankInfo = banks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x => x.BankGUID == mount.Bank.GUID));
					if (weaponBankInfo != null && weaponBankInfo.WeaponID.HasValue)
					{
						string weapon = this.GetWeaponAsset(weaponBankInfo.WeaponID.Value);
						LogicalWeapon logicalWeapon = this.assetdb.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weapon));
						if (logicalWeapon != null)
							turretHealth += logicalWeapon.Health;
					}
					this.InsertWeaponInstance(mount, sectionInstId, new int?(), index, turretHealth, mount.NodeName);
					++index;
				}
			}
		}

		private void InsertNewShipWeaponInstancesForModule(
		  List<LogicalMount> mounts,
		  int? weaponId,
		  int sectionInstId,
		  int moduleInstId)
		{
			string weapon = weaponId.HasValue ? this.GetWeaponAsset(weaponId.Value) : "";
			LogicalWeapon logicalWeapon = !string.IsNullOrEmpty(weapon) ? this.assetdb.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weapon)) : (LogicalWeapon)null;
			int index = 0;
			foreach (LogicalMount mount in mounts)
			{
				if (!WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
				{
					float turretHealth = Ship.GetTurretHealth(mount.Bank.TurretSize);
					if (logicalWeapon != null)
						turretHealth += logicalWeapon.Health;
					this.InsertWeaponInstance(mount, sectionInstId, new int?(moduleInstId), index, turretHealth, mount.NodeName);
					++index;
				}
			}
		}

		private void InsertNewShipModuleInstances(
		  ShipSectionAsset sectionAsset,
		  List<DesignModuleInfo> modules,
		  int sectionId)
		{
			foreach (DesignModuleInfo module1 in modules)
			{
				DesignModuleInfo module = module1;
				string mPath = this.GetModuleAsset(module.ModuleID);
				LogicalModule module2 = this.assetdb.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == mPath));
				int moduleInstId = this.InsertModuleInstance(((IEnumerable<LogicalModuleMount>)sectionAsset.Modules).First<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.NodeName == module.MountNodeName)), module2, sectionId);
				if (((IEnumerable<LogicalMount>)module2.Mounts).Count<LogicalMount>() > 0)
					this.InsertNewShipWeaponInstancesForModule(((IEnumerable<LogicalMount>)module2.Mounts).ToList<LogicalMount>(), module.WeaponID, sectionId, moduleInstId);
			}
		}

		private Dictionary<string, int> GetShipNamesByPlayer(int playerId)
		{
			Dictionary<string, int> dictionary;
			if (!this._dom.CachedPlayerShipNames.TryGetValue(playerId, out dictionary))
			{
				dictionary = new Dictionary<string, int>();
				this._dom.CachedPlayerShipNames[playerId] = dictionary;
			}
			return dictionary;
		}

		private List<string> GetFleetNamesByPlayer(int playerId)
		{
			return this.GetFleetInfosByPlayerID(playerId, FleetType.FL_NORMAL).ToList<FleetInfo>().Select<FleetInfo, string>((Func<FleetInfo, string>)(x => x.Name)).ToList<string>();
		}

		public string ResolveNewFleetName(App App, int playerId, string name)
		{
			bool flag = App.Game.NamesPool.GetFleetNamesForFaction(App.GameDatabase.GetFactionName(App.GameDatabase.GetPlayerFactionID(playerId))).Any<string>((Func<string, bool>)(x => x.Contains(name)));
			string str1 = name;
			List<string> fleetNamesByPlayer = this.GetFleetNamesByPlayer(playerId);
			if (fleetNamesByPlayer.Any<string>((Func<string, bool>)(x => x.StartsWith(name))) || flag)
			{
				int num = 0;
				foreach (string str2 in fleetNamesByPlayer.Where<string>((Func<string, bool>)(x => x.StartsWith(name))))
				{
					int result = 0;
					string[] strArray = str2.Split(' ');
					int.TryParse(strArray[strArray.Length - 1], out result);
					num = result > num ? result : num;
				}
				if (num == 0 && fleetNamesByPlayer.Where<string>((Func<string, bool>)(x => x.StartsWith(name))).Any<string>())
					num = 1;
				str1 = string.Format("{0} {1}", (object)str1, (object)(num + (flag ? 1 : 0)).ToString());
			}
			return str1;
		}

		public string ResolveNewShipName(int playerId, string name)
		{
			string str = name;
			int num = this.GetShipNamesByPlayer(playerId).Keys.Count<string>((Func<string, bool>)(x => x.Contains(name)));
			if (num > 0)
				str = string.Format("{0} - {1}", (object)str, (object)num.ToString());
			return str;
		}

		public void TransferShipToPlayer(ShipInfo ship, int newPlayerId)
		{
			List<SectionInstanceInfo> list = this.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
			Dictionary<SectionInstanceInfo, string> dictionary = new Dictionary<SectionInstanceInfo, string>();
			DesignInfo designInfo = ship.DesignInfo;
			foreach (SectionInstanceInfo sectionInstanceInfo in list)
			{
				SectionInstanceInfo sii = sectionInstanceInfo;
				DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ID == sii.SectionID));
				dictionary.Add(sii, designSectionInfo.FilePath);
			}
			designInfo.PlayerID = newPlayerId;
			int num = this.InsertDesignByDesignInfo(designInfo);
			foreach (KeyValuePair<SectionInstanceInfo, string> keyValuePair in dictionary)
			{
				KeyValuePair<SectionInstanceInfo, string> kvp = keyValuePair;
				DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.FilePath == kvp.Value));
				this.db.ExecuteNonQuery(string.Format(Queries.ChangeSectionInstanceSectionId, (object)kvp.Key.ID, (object)designSectionInfo.ID), false, true);
				this._dom.section_instances.Sync(kvp.Key.ID);
			}
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipDesign, (object)ship.ID, (object)num), false, true);
			this._dom.ships.Sync(ship.ID);
		}

		public int InsertShip(
		  int fleetID,
		  int designID,
		  string shipName = null,
		  ShipParams parms = (ShipParams)0,
		  int? aiFleetID = null,
		  int Loacubes = 0)
		{
			DesignInfo designInfo = this.GetDesignInfo(designID);
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo != null && designInfo.PlayerID != fleetInfo.PlayerID)
				throw new InvalidOperationException(string.Format("Mismatched design and fleet players (designID={0},design playerID={1},fleet playerID={2}).", (object)designID, (object)designInfo.PlayerID, (object)fleetInfo.PlayerID));
			bool flag = false;
			int num1 = 0;
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = this.assetdb.GetShipSectionAsset(designSection.FilePath);
				num1 += (int)shipSectionAsset.PsionicPowerLevel;
				flag = flag || shipSectionAsset.IsSuulka;
			}
			if (flag && shipName == null)
				shipName = designInfo.Name;
			if (!flag)
			{
				Faction faction = this.assetdb.GetFaction(this.GetPlayerFactionID(designInfo.PlayerID));
				if (faction != null)
					num1 = (int)((double)designInfo.CrewAvailable * (double)faction.PsionicPowerPerCrew);
			}
			this.AddNumShipsBuiltFromDesign(designID, 1);
			int shipsBuiltFromDesign = this.GetNumShipsBuiltFromDesign(designID);
			if (!flag && (shipName == null || shipName == designInfo.Name))
				shipName = this.GetDefaultShipName(designID, shipsBuiltFromDesign);
			shipName = this.ResolveNewShipName(designInfo.PlayerID, shipName);
			int turnCount = this.GetTurnCount();
			int num2 = 0;
			int shipId = this._dom.ships.Insert(new int?(), new ShipInfo()
			{
				FleetID = fleetID,
				DesignID = designID,
				DesignInfo = designInfo,
				ParentID = num2,
				ShipName = shipName,
				SerialNumber = shipsBuiltFromDesign,
				Params = parms,
				RiderIndex = -1,
				PsionicPower = num1,
				AIFleetID = aiFleetID,
				ComissionDate = turnCount,
				LoaCubes = Loacubes
			});
			this.TryAddFleetShip(shipId, fleetID);
			this.InsertNewShipSectionInstances(designInfo, new int?(shipId), new int?());
			this.AddCachedShipNameReference(designInfo.PlayerID, shipName);
			return shipId;
		}

		public int InsertDesignByDesignInfo(DesignInfo design)
		{
			DesignLab.SummarizeDesign(this.AssetDatabase, this, design);
			int num = this._dom.designs.Insert(new int?(), design);
			HashSet<string> stringSet;
			if (!this._dom.CachedPlayerDesignNames.TryGetValue(design.PlayerID, out stringSet))
			{
				stringSet = new HashSet<string>((IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);
				this._dom.CachedPlayerDesignNames.Add(design.PlayerID, stringSet);
			}
			stringSet.Add(design.Name);
			return num;
		}

		public void UpdateDesignAttributeDiscovered(int id, bool isAttributeDiscovered)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignAttributesDiscovered, (object)id.ToSQLiteValue(), (object)isAttributeDiscovered.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(id);
		}

		public void UpdateDesignPrototype(int id, bool isPrototyped)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignPrototype, (object)id.ToSQLiteValue(), (object)isPrototyped.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(id);
		}

		public void InsertDesignAttribute(int id, SectionEnumerations.DesignAttribute da)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDesignAttribute, (object)id.ToSQLiteValue(), (object)((int)da).ToSQLiteValue()), false, true);
		}

		public IEnumerable<SectionEnumerations.DesignAttribute> GetDesignAttributesForDesign(
		  int id)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetDesignAttributesForDesign, (object)id), true);
			foreach (Row row in t)
				yield return (SectionEnumerations.DesignAttribute)int.Parse(row[0]);
		}

		public IEnumerable<string> GetGetAllPlayerSectionIds(int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerShipSections, (object)playerId), true);
			foreach (Row row in t)
				yield return row[0];
		}

		public int InsertSectionAsset(string filepath, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSection, (object)filepath, (object)playerId));
		}

		private int InsertSectionInstance(
		  int sectionID,
		  int? shipID,
		  int? stationID,
		  int structure,
		  float supply,
		  int crew,
		  float signature,
		  int repairPoints)
		{
			return this._dom.section_instances.Insert(new int?(), new SectionInstanceInfo()
			{
				SectionID = sectionID,
				ShipID = shipID,
				StationID = stationID,
				Structure = structure,
				Supply = (int)supply,
				Crew = crew,
				Signature = signature,
				RepairPoints = repairPoints
			});
		}

		private void RemoveSectionInstance(int sectioninstanceID)
		{
			this._dom.section_instances.Remove(sectioninstanceID);
		}

		public int InsertWeaponInstance(
		  LogicalMount mount,
		  int sectionId,
		  int? moduleId,
		  int index,
		  float structure,
		  string nodeName)
		{
			return this._dom.weapon_instances.Insert(new int?(), new WeaponInstanceInfo()
			{
				SectionInstanceID = sectionId,
				ModuleInstanceID = moduleId,
				Structure = structure,
				MaxStructure = structure,
				WeaponID = index,
				NodeName = nodeName
			});
		}

		public int InsertModuleInstance(LogicalModuleMount mount, LogicalModule module, int sectionId)
		{
			return this._dom.module_instances.Insert(new int?(), new ModuleInstanceInfo()
			{
				SectionInstanceID = sectionId,
				RepairPoints = module.RepairPointsBonus,
				Structure = (int)module.Structure,
				ModuleNodeID = mount.NodeName
			});
		}

		public void RemovePlayerDesign(int designId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemovePlayerDesign, (object)designId.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(designId);
			this._dom.CachedPlayerDesignNames.Clear();
		}

		public void RemoveSection(int sectionId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSection, (object)sectionId), false, true);
		}

		public void SetLastTurnWithCombat(int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetLastTurnWithCombat, (object)turn), false, true);
		}

		public int GetLastTurnWithCombat()
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetLastTurnWithCombat));
		}

		public int InsertDesignModule(DesignModuleInfo value)
		{
			int num = DesignsCache.InsertDesignModuleInfo(this.db, value);
			this._dom.designs.Sync(value.DesignSectionInfo.DesignInfo.ID);
			return num;
		}

		public void UpdateQueuedModuleNodeName(DesignModuleInfo module)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateQueuedModuleNodeName, (object)module.ID.ToSQLiteValue(), (object)module.MountNodeName.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(module.DesignSectionInfo.DesignInfo.ID);
		}

		public void UpdateDesignModuleNodeName(DesignModuleInfo module)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateDesignModuleNodeName, (object)module.ID.ToSQLiteValue(), (object)module.MountNodeName.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(module.DesignSectionInfo.DesignInfo.ID);
		}

		public void RemoveDesignModule(DesignModuleInfo module)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveDesignModule, (object)module.ID.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(module.DesignSectionInfo.DesignInfo.ID);
		}

		public bool canBuildDesignOrder(DesignInfo di, int SystemId, out bool requiresPrototype)
		{
			requiresPrototype = false;
			if (di.isPrototyped)
				return true;
			if (this.GetDesignBuildOrders(di).ToList<BuildOrderInfo>().Count > 0)
				return this.GetBuildOrdersForSystem(SystemId).ToList<BuildOrderInfo>().Any<BuildOrderInfo>((Func<BuildOrderInfo, bool>)(x => x.DesignID == di.ID));
			requiresPrototype = true;
			return true;
		}

		public IEnumerable<BuildOrderInfo> GetDesignBuildOrders(DesignInfo di)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetDesignBuildOrders, (object)di.ID), true);
			foreach (Row row in t)
				yield return this.GetBuildOrderInfo(row);
		}

		public int InsertInvoiceBuildOrder(int invoiceId, int designID, string shipName, int loacubes = 0)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertInvoiceBuildOrder, (object)invoiceId.ToSQLiteValue(), (object)designID.ToSQLiteValue(), (object)shipName.ToSQLiteValue(), (object)loacubes.ToSQLiteValue()));
		}

		private int InsertBuildOrderCore(
		  int systemId,
		  int designId,
		  int progress,
		  int priority,
		  int? missionId,
		  string name,
		  int productionCost,
		  int? invoiceInstanceId,
		  int? aiFleetId,
		  int LoaCubes)
		{
			if (missionId.HasValue)
			{
				MissionInfo missionInfo = this.GetMissionInfo(missionId.Value);
				if (missionInfo.FleetID != 0)
				{
					FleetInfo fleetInfo = this.GetFleetInfo(missionInfo.FleetID);
					DesignInfo designInfo = this.GetDesignInfo(designId);
					if (fleetInfo.PlayerID != designInfo.PlayerID)
						throw new ArgumentException(string.Format("Tried inserting a build order belonging to player {0} into a fleet ({1}) belonging to player {1}.", (object)designInfo.PlayerID, (object)fleetInfo.ID, (object)fleetInfo.PlayerID));
				}
			}
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertBuildOrder, (object)systemId.ToSQLiteValue(), (object)designId.ToSQLiteValue(), (object)progress.ToSQLiteValue(), (object)priority.ToSQLiteValue(), (object)missionId.ToNullableSQLiteValue(), (object)name.ToSQLiteValue(), (object)productionCost.ToSQLiteValue(), (object)invoiceInstanceId.ToNullableSQLiteValue(), (object)aiFleetId.ToNullableSQLiteValue(), (object)LoaCubes.ToSQLiteValue()));
		}

		public int InsertBuildOrder(
		  int systemID,
		  int designID,
		  int progress,
		  int priority,
		  string shipName,
		  int productionTarget,
		  int? invoiceInstanceId = null,
		  int? aiFleetID = null,
		  int LoaCubes = 0)
		{
			if (priority == 0)
			{
				foreach (BuildOrderInfo buildOrderInfo in this.GetBuildOrdersForSystem(systemID))
				{
					if (buildOrderInfo.Priority > priority)
						priority = buildOrderInfo.Priority;
				}
				++priority;
			}
			return this.InsertBuildOrderCore(systemID, designID, progress, priority, new int?(), shipName, productionTarget, invoiceInstanceId, aiFleetID, LoaCubes);
		}

		public void InsertBuildOrders(
		  int systemId,
		  IEnumerable<int> designIDs,
		  int priorityOrder,
		  int missionID,
		  int? invoiceInstanceID = null,
		  int? aiFleetID = null)
		{
			if (designIDs.Count<int>() < 1)
				return;
			IEnumerable<BuildOrderInfo> buildOrdersForSystem = this.GetBuildOrdersForSystem(systemId);
			int priority1 = 1;
			if (buildOrdersForSystem.Count<BuildOrderInfo>() < 1)
			{
				foreach (int designId in designIDs)
				{
					DesignInfo designInfo = this.GetDesignInfo(designId);
					this.InsertBuildOrderCore(systemId, designId, 0, priority1, new int?(missionID), designInfo.Name, designInfo.ProductionCost, invoiceInstanceID, aiFleetID, 0);
					++priority1;
				}
			}
			else
			{
				int priority2 = 1;
				foreach (BuildOrderInfo buildOrder in buildOrdersForSystem)
				{
					if (priority2 == priorityOrder)
					{
						foreach (int designId in designIDs)
						{
							DesignInfo designInfo = this.GetDesignInfo(designId);
							this.InsertBuildOrderCore(systemId, designId, 0, priority2, new int?(missionID), designInfo.Name, designInfo.ProductionCost, invoiceInstanceID, aiFleetID, 0);
							++priority2;
						}
					}
					buildOrder.Priority = priority2;
					this.UpdateBuildOrder(buildOrder);
					++priority2;
				}
			}
		}

		private int InsertRetrofitOrderCore(
		  int systemId,
		  int designId,
		  int shipid,
		  int? invoiceInstanceId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertRetrofitOrder, (object)systemId.ToSQLiteValue(), (object)designId.ToSQLiteValue(), (object)shipid.ToSQLiteValue(), (object)invoiceInstanceId.ToNullableSQLiteValue()));
		}

		public int InsertRetrofitOrder(int systemID, int designID, int shipID, int? invoiceInstanceId = null)
		{
			return this.InsertRetrofitOrderCore(systemID, designID, shipID, invoiceInstanceId);
		}

		private RetrofitOrderInfo ParseRetrofitOrderInfo(Row row)
		{
			if (row == null)
				return (RetrofitOrderInfo)null;
			return new RetrofitOrderInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				DesignID = row[1].SQLiteValueToInteger(),
				ShipID = row[2].SQLiteValueToInteger(),
				SystemID = row[3].SQLiteValueToInteger(),
				InvoiceID = row[4].SQLiteValueToNullableInteger()
			};
		}

		public RetrofitOrderInfo GetRetrofitOrderInfo(int retrofitOrderId)
		{
			return this.ParseRetrofitOrderInfo(this.db.ExecuteTableQuery(string.Format(Queries.GetRetrofitOrderInfo, (object)retrofitOrderId), true)[0]);
		}

		public IEnumerable<RetrofitOrderInfo> GetRetrofitOrdersForInvoiceInstance(
		  int invoiceInstanceId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetRetrofitOrdersForInvoice, (object)invoiceInstanceId), true);
			foreach (Row row in t.Rows)
				yield return this.ParseRetrofitOrderInfo(row);
		}

		public IEnumerable<RetrofitOrderInfo> GetRetrofitOrdersForSystem(
		  int systemId)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetRetrofitOrdersForSystem, (object)systemId), true))
				yield return this.ParseRetrofitOrderInfo(row);
		}

		public void RemoveRetrofitOrder(int retrofitOrderID, bool destroyship = false, bool defenseasset = false)
		{
			ShipInfo shipInfo = this.GetShipInfo(this.GetRetrofitOrderInfo(retrofitOrderID).ShipID, true);
			FleetInfo fleetInfo = this.GetFleetInfo(shipInfo.FleetID);
			if (destroyship)
				this.RemoveShip(shipInfo.ID);
			else if (defenseasset)
				this.TransferShip(shipInfo.ID, this.InsertOrGetDefenseFleetID(fleetInfo.SystemID, fleetInfo.PlayerID));
			else
				this.TransferShip(shipInfo.ID, this.InsertOrGetReserveFleetID(fleetInfo.SystemID, fleetInfo.PlayerID));
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveRetrofitOrder, (object)retrofitOrderID), true);
		}

		public int InsertStationRetrofitOrder(int designid, int shipid)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStationRetrofitOrder, (object)designid.ToSQLiteValue(), (object)shipid.ToSQLiteValue()));
		}

		public void RemoveStationRetrofitOrder(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveStationRetrofitOrder, (object)id.ToSQLiteValue()), false, true);
		}

		public IEnumerable<StationRetrofitOrderInfo> GetStationRetrofitOrders()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetStationRetrofitOrders), true))
				yield return this.ParseStationRetrofitOrderInfo(row);
		}

		private StationRetrofitOrderInfo ParseStationRetrofitOrderInfo(Row row)
		{
			if (row == null)
				return (StationRetrofitOrderInfo)null;
			return new StationRetrofitOrderInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				DesignID = row[1].SQLiteValueToInteger(),
				ShipID = row[2].SQLiteValueToInteger()
			};
		}

		public void InsertPlayerTech(
		  int playerId,
		  string techId,
		  TechStates state,
		  double progress,
		  double totalCost,
		  int? turnResearched)
		{
			float num1 = 0.0f;
			float num2 = 0.01f;
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPlayerTech, (object)playerId.ToSQLiteValue(), (object)techId.ToSQLiteValue(), (object)((int)state).ToSQLiteValue(), (object)progress.ToSQLiteValue(), (object)totalCost.ToSQLiteValue(), (object)num1.ToSQLiteValue(), (object)num2.ToSQLiteValue(), (object)turnResearched.ToNullableSQLiteValue()), false, true);
			this._dom.player_techs.Clear();
		}

		public void InsertPlayerTechBranch(
		  int playerId,
		  int fromTechId,
		  int toTechId,
		  int researchCost,
		  float feasibility)
		{
			++this.insertPlayerTechBranchCount;
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPlayerTechBranch, (object)playerId, (object)fromTechId, (object)toTechId, (object)researchCost, (object)feasibility), false, true);
			this._dom.player_tech_branches.Clear();
		}

		public void InsertTech(string idFromFile)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertTech, (object)idFromFile), false, true);
		}

		public int InsertAdmiral(
		  int playerID,
		  int? homeworldID,
		  string name,
		  string race,
		  float age,
		  string gender,
		  float reaction,
		  float evasion,
		  int loyalty)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertAdmiral, (object)playerID.ToOneBasedSQLiteValue(), (object)homeworldID.ToNullableSQLiteValue(), (object)name.ToSQLiteValue(), (object)race.ToSQLiteValue(), (object)age.ToSQLiteValue(), (object)gender.ToSQLiteValue(), (object)reaction.ToSQLiteValue(), (object)evasion.ToSQLiteValue(), (object)loyalty.ToSQLiteValue(), (object)0, (object)0, (object)0, (object)0, (object)this.GetTurnCount(), (object)false.ToSQLiteValue()));
		}

		private int InsertFleetCore(
		  int playerID,
		  int admiralID,
		  int systemID,
		  int supportSystemID,
		  string name,
		  FleetType type)
		{
			this._dom.fleets.Clear();
			this._dom.CachedSystemHasGateFlags.Remove(new DataObjectCache.SystemPlayerID()
			{
				PlayerID = playerID,
				SystemID = systemID
			});
			int num1 = 0;
			float num2 = 0.0f;
			if (this.AssetDatabase.GetFaction(this.GetPlayerInfo(playerID).FactionID).Name == "loa")
				num2 = 10f;
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFleet, (object)playerID.ToSQLiteValue(), (object)admiralID.ToOneBasedSQLiteValue(), (object)systemID.ToOneBasedSQLiteValue(), (object)supportSystemID.ToOneBasedSQLiteValue(), (object)name.ToSQLiteValue(), (object)num1.ToSQLiteValue(), (object)num2.ToSQLiteValue(), (object)((int)type).ToSQLiteValue(), (object)systemID.ToOneBasedSQLiteValue(), (object)false.ToSQLiteValue()));
		}

		public int GetHomeSystem(GameSession sim, int missionId, FleetInfo fi)
		{
			if (!(this.GetStarSystemInfo(fi.SupportingSystemID) == (StarSystemInfo)null) && (this.GetColonyInfosForSystem(fi.SupportingSystemID).ToList<ColonyInfo>().Any<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)) || this.GetStationForSystemPlayerAndType(fi.SupportingSystemID, fi.PlayerID, StationType.NAVAL) != null))
				return fi.SupportingSystemID;
			int newHomeSystem = this.FindNewHomeSystem(fi);
			if (this.GetRemainingSupportPoints(sim, fi.PlayerID, newHomeSystem) < this.GetFleetCruiserEquivalent(fi.ID) || this.GetFleetCommandPointCost(fi.ID) > this.GetFleetCommandPointQuota(fi.ID))
				this.InsertWaypoint(missionId, WaypointType.DisbandFleet, new int?());
			return newHomeSystem;
		}

		public int FindNewHomeSystem(FleetInfo fi)
		{
			Vector3 vector3 = fi.SystemID == 0 ? this.GetFleetLocation(fi.ID, false).Coords : this.GetStarSystemOrigin(fi.SystemID);
			List<int> list = this.GetPlayerColonySystemIDs(fi.PlayerID).ToList<int>();
			int num1 = 0;
			float num2 = float.MaxValue;
			foreach (int systemId in list)
			{
				float length = (vector3 - this.GetStarSystemOrigin(systemId)).Length;
				if ((double)length < (double)num2)
				{
					num2 = length;
					num1 = systemId;
				}
			}
			return num1;
		}

		public int InsertFleet(
		  int playerID,
		  int admiralID,
		  int systemID,
		  int supportSystemID,
		  string name,
		  FleetType type = FleetType.FL_NORMAL)
		{
			return this.InsertFleetCore(playerID, admiralID, systemID, supportSystemID, name, type);
		}

		private Vector3 GetVFormationPositionAtIndex(int index)
		{
			int num1 = (index + 1) / 2;
			int num2 = (index + 1) / 2;
			int num3 = index % 2 == 0 ? 1 : -1;
			Vector3 vector3 = new Vector3();
			vector3.X = (float)num3 * 200f * (float)num2;
			vector3.Y = 0.0f;
			vector3.Z = 400f * (float)num1;
			vector3.Z -= 1300f;
			return vector3;
		}

		private Vector3 GetBackLinePositionAtIndex(int index)
		{
			int num = (int)((double)index / 5.0);
			Vector3 vector3;
			vector3.X = (float)((double)(index % 5) * 600.0 - 1200.0);
			switch (num)
			{
				case 0:
					vector3.Y = 0.0f;
					break;
				case 1:
					vector3.Y = 300f;
					break;
				default:
					vector3.Y = -300f;
					break;
			}
			vector3.Z = 2000f;
			return vector3;
		}

		public void LayoutFleet(int fleetID)
		{
			int index1 = 0;
			int index2 = 0;
			foreach (ShipInfo shipInfo in this.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>())
			{
				Vector3 vector3;
				if ((double)DesignLab.GetShipSize(this.GetDesignInfo(shipInfo.DesignID)).H > 10.0)
				{
					vector3 = this.GetBackLinePositionAtIndex(index2);
					++index2;
				}
				else
				{
					vector3 = this.GetVFormationPositionAtIndex(index1);
					++index1;
				}
				float num = 111.1111f;
				vector3.X = (float)Math.Floor((double)vector3.X / (double)num + 0.5) * num;
				vector3.Z = (float)Math.Floor((double)vector3.Z / (double)num + 0.5) * num;
				this.UpdateShipFleetPosition(shipInfo.ID, new Vector3?(vector3));
			}
		}

		public int InsertMoveOrder(
		  int fleetID,
		  int fromSystemID,
		  Vector3 fromCoords,
		  int toSystemID,
		  Vector3 toCoords)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertMoveOrder, (object)fleetID.ToSQLiteValue(), (object)fromSystemID.ToOneBasedSQLiteValue(), (object)fromCoords.ToSQLiteValue(), (object)toSystemID.ToOneBasedSQLiteValue(), (object)toCoords.ToSQLiteValue()));
		}

		public int InsertWaypoint(int missionID, WaypointType type, int? systemID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertWaypoint, (object)missionID.ToSQLiteValue(), (object)((int)type).ToSQLiteValue(), (object)systemID.ToNullableSQLiteValue()));
		}

		public void ClearWaypoints(int missionID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.ClearWaypoints, (object)missionID.ToSQLiteValue()), false, true);
		}

		public int InsertMission(
		  int fleetID,
		  MissionType type,
		  int systemID,
		  int orbitalObjectID,
		  int targetFleetID,
		  int duration,
		  bool useDirectRoute,
		  int? stationtype = null)
		{
			GameDatabase.Trace("InsertMission: " + (object)fleetID + " " + type.ToString() + " " + systemID.ToString());
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet)
				throw new InvalidOperationException("Caught attempt to send reserve fleet " + (object)fleetID + " on a mission.");
			MissionInfo missionByFleetId = this.GetMissionByFleetID(fleetID);
			if (missionByFleetId != null)
				this.RemoveMission(missionByFleetId.ID);
			return this._dom.missions.Insert(new int?(), new MissionInfo()
			{
				FleetID = fleetID,
				Type = type,
				TargetSystemID = systemID,
				TargetOrbitalObjectID = orbitalObjectID,
				TargetFleetID = targetFleetID,
				Duration = duration,
				UseDirectRoute = useDirectRoute,
				TurnStarted = this.GetTurnCount(),
				StartingSystem = fleetInfo.SystemID,
				StationType = stationtype
			});
		}

		public int InsertColonyTrap(int systemID, int planetID, int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo == null || !fleetInfo.IsTrapFleet)
				throw new InvalidOperationException("Insert Colony Trap Fleet " + (object)fleetID + " is invalid.");
			return this._dom.colony_traps.Insert(new int?(), new ColonyTrapInfo()
			{
				SystemID = systemID,
				PlanetID = planetID,
				FleetID = fleetID
			});
		}

		private PlayerClientInfo ParsePlayerClientInfo(Row row)
		{
			return new PlayerClientInfo()
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				UserName = row[1].SQLiteValueToString()
			};
		}

		public IEnumerable<PlayerClientInfo> GetPlayerClientInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(Queries.GetPlayerClientInfos, true))
				yield return this.ParsePlayerClientInfo(row);
		}

		public void InsertPlayerClientInfo(PlayerClientInfo value)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPlayerClientInfo, (object)value.PlayerID.ToSQLiteValue(), (object)value.UserName.ToSQLiteValue()), false, true);
		}

		public void UpdatePlayerClientInfo(PlayerClientInfo value)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerClientInfo, (object)value.PlayerID.ToSQLiteValue(), (object)value.UserName.ToSQLiteValue()), false, true);
		}

		public int? GetLastClientPlayerID(string username)
		{
			return this.db.ExecuteIntegerQueryDefault(string.Format(Queries.GetPlayerClientInfo, (object)username), new int?());
		}

		public int InsertPlayer(
		  string name,
		  string factionName,
		  int? homeworldID,
		  Vector3 primaryColor,
		  Vector3 secondaryColor,
		  string badgeAssetPath,
		  string avatarAssetPath,
		  double savings,
		  int subfactionIndex,
		  bool standardPlayer = true,
		  bool includeInDiplomacy = false,
		  bool isAIRebellionPlayer = false,
		  int team = 0,
		  AIDifficulty difficulty = AIDifficulty.Normal)
		{
			return this._dom.players.Insert(new int?(), new PlayerInfo()
			{
				Name = name,
				FactionID = this.GetFactionIdFromName(factionName),
				Homeworld = homeworldID,
				PrimaryColor = primaryColor,
				SecondaryColor = secondaryColor,
				BadgeAssetPath = badgeAssetPath,
				AvatarAssetPath = avatarAssetPath,
				Savings = savings,
				SubfactionIndex = subfactionIndex,
				isStandardPlayer = standardPlayer,
				includeInDiplomacy = includeInDiplomacy,
				isAIRebellionPlayer = isAIRebellionPlayer,
				Team = team,
				AIDifficulty = difficulty
			});
		}

		public void InsertMissingFactions(Random initializationRandomSeed)
		{
			foreach (Faction faction in this.assetdb.Factions)
			{
				float suitability = faction.ChooseIdealSuitability(initializationRandomSeed);
				this.InsertOrIgnoreFaction(faction.ID, faction.Name, suitability);
			}
		}

		public void UpdateFaction(FactionInfo fi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFactionInfo, (object)fi.ID.ToSQLiteValue(), (object)fi.Name.ToSQLiteValue(), (object)fi.IdealSuitability.ToSQLiteValue()), false, true);
		}

		public void InsertOrIgnoreFaction(int id, string factionName, float suitability)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertFaction, (object)id.ToSQLiteValue(), (object)factionName.ToSQLiteValue(), (object)suitability.ToSQLiteValue()), false, true);
		}

		public void InsertOrIgnoreAI(int playerID, AIStance stance = AIStance.EXPANDING)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertOrIgnoreAI, (object)playerID.ToSQLiteValue(), (object)((int)stance).ToSQLiteValue()), false, true);
		}

		public void InsertAIOldColonyOwner(int colonyId, int playerId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAIOldColonyOwner, (object)colonyId, (object)playerId), false, true);
		}

		public int[] GetAIOldColonyOwner(int playerId)
		{
			return this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetAIOldColoniesByPlayer, (object)playerId));
		}

		public void InsertAIStationIntel(int playerID, int intelOnPlayerID, int stationID, int level)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelStation, (object)playerID.ToSQLiteValue(), (object)intelOnPlayerID.ToSQLiteValue(), (object)stationID.ToSQLiteValue(), (object)this.GetTurnCount().ToSQLiteValue(), (object)level.ToSQLiteValue()), false, true);
		}

		public void InsertAIDesignIntel(
		  int playerID,
		  int intelOnPlayerID,
		  int designID,
		  bool salvaged)
		{
			int turnCount = this.GetTurnCount();
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelDesign, (object)playerID.ToSQLiteValue(), (object)intelOnPlayerID.ToSQLiteValue(), (object)designID.ToSQLiteValue(), (object)turnCount.ToSQLiteValue(), (object)turnCount.ToSQLiteValue(), (object)salvaged.ToSQLiteValue()), false, true);
		}

		public void InsertAIFleetIntel(
		  int playerID,
		  int intelOnPlayerID,
		  int system,
		  Vector3 coords,
		  int numDestroyers,
		  int numCruisers,
		  int numDreadnought,
		  int numLeviathan)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIntelFleet, (object)playerID.ToSQLiteValue(), (object)intelOnPlayerID.ToSQLiteValue(), (object)this.GetTurnCount().ToSQLiteValue(), (object)system.ToSQLiteValue(), (object)coords.ToSQLiteValue(), (object)numDestroyers.ToSQLiteValue(), (object)numCruisers.ToSQLiteValue(), (object)numDreadnought.ToSQLiteValue(), (object)numLeviathan.ToSQLiteValue()), false, true);
		}

		public void InsertAITechWeight(int playerID, string family, float weight)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAITechWeight, (object)family.ToSQLiteValue(), (object)playerID.ToSQLiteValue(), (object)0.0f.ToSQLiteValue(), (object)weight.ToSQLiteValue()), false, true);
		}

		public int InsertDiplomaticState(
		  int playerID,
		  int towardPlayerID,
		  DiplomacyState type,
		  int relations,
		  bool isEncountered,
		  bool reciprocal = false)
		{
			return this._dom.diplomacy_states.InsertDiplomaticState(playerID, towardPlayerID, type, relations, isEncountered, reciprocal);
		}

		public int InsertIndependentRace(
		  string name,
		  int orbitalObjectID,
		  double population,
		  int techLevel,
		  int rxHuman,
		  int rxTarka,
		  int rxLiir,
		  int rxZuul,
		  int rxMorrigi,
		  int rxHiver)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertIndependentRace, (object)name.ToSQLiteValue(), (object)orbitalObjectID.ToSQLiteValue(), (object)population.ToSQLiteValue(), (object)techLevel.ToSQLiteValue(), (object)rxHuman.ToSQLiteValue(), (object)rxTarka.ToSQLiteValue(), (object)rxLiir.ToSQLiteValue(), (object)rxZuul.ToSQLiteValue(), (object)rxMorrigi.ToSQLiteValue(), (object)rxHiver.ToSQLiteValue()));
		}

		public int InsertIndependentRaceColony(int raceID, int orbitalObjectID, double population)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertIndependentRaceColony, (object)orbitalObjectID.ToSQLiteValue(), (object)raceID.ToSQLiteValue(), (object)population.ToSQLiteValue()));
		}

		public int InsertFeasibilityStudy(int playerId, int techId, string projectName)
		{
			PlayerTechInfo playerTechInfo = this.GetPlayerTechInfo(playerId, techId);
			if (playerTechInfo.State != TechStates.Branch)
				throw new ArgumentException(string.Format("Player {0} cannot start of feasibility study for {1} (current state is {2})", (object)playerId, (object)playerTechInfo.TechFileID, (object)playerTechInfo.State));
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFeasibilityStudy, (object)playerId.ToSQLiteValue(), (object)1.ToSQLiteValue(), (object)projectName.ToSQLiteValue(), (object)techId.ToSQLiteValue(), (object)(playerTechInfo.ResearchCost / 10).ToSQLiteValue()));
		}

		private void GuaranteeAsset(string assetPath)
		{
			this.db.ExecuteNonQuery(string.Format("INSERT OR REPLACE INTO assets (id,path) VALUES ((SELECT id FROM assets WHERE path=\"{0}\"),\"{0}\")", (object)assetPath.ToSQLiteValue()), false, true);
		}

		private int InsertStellarProp(string assetPath, Matrix transform)
		{
			this.GuaranteeAsset(assetPath);
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertStellarProp, (object)assetPath.ToSQLiteValue(), (object)transform.ToSQLiteValue()));
		}

		public int InsertProvince(
		  string name,
		  int playerId,
		  IEnumerable<int> systemIds,
		  int capitalId)
		{
			int num = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertProvince, (object)playerId.ToSQLiteValue(), (object)name.ToSQLiteValue(), (object)capitalId.ToSQLiteValue()));
			foreach (int systemId in systemIds)
				this.UpdateSystemProvinceID(systemId, new int?(num));
			return num;
		}

		public void RemoveProvince(int provinceId)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveProvince, (object)provinceId), false, true);
		}

		public void UpdateSystemProvinceID(int systemId, int? provinceId)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemProvinceID, (object)systemId.ToSQLiteValue(), (object)provinceId.ToNullableSQLiteValue()), false, true);
		}

		public int InsertNodeLine(int systemAId, int systemBId, int health)
		{
			return this._dom.node_lines.Insert(new int?(), new NodeLineInfo()
			{
				System1ID = systemAId,
				System2ID = systemBId,
				Health = health
			});
		}

		public void RemoveNodeLine(int nodeid)
		{
			Kerberos.Sots.StarSystemPathing.StarSystemPathing.RemoveNodeLine(nodeid);
			this._dom.node_lines.Remove(nodeid);
		}

		public void UpdateNodeLineHealth(int nodeid, int health)
		{
			NodeLineInfo nodeLine = this._dom.node_lines[nodeid];
			nodeLine.Health = health;
			this._dom.node_lines.Update(nodeLine.ID, nodeLine);
		}

		public int InsertTerrain(string name, Vector3 origin)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertTerrain, (object)name.ToSQLiteValue(), (object)origin.ToSQLiteValue()));
		}

		public void FloodTest()
		{
			this.db.ExecuteNonQuery("BEGIN DEFERRED TRANSACTION", false, true);
			this.InsertOrIgnoreFaction(0, "blah", 12.34f);
			for (int index = 0; index < 1000; ++index)
				this.InsertPlayer("player" + (object)(index + 1), "blah", new int?(0), new Vector3((float)index, (float)index, (float)index), Vector3.One, "factions\\human\\badges\\badge01.tga", "factions\\human\\avatar\\avatar01.tga", 0.0, 0, true, false, false, 0, AIDifficulty.Normal);
			for (int index = 0; index < 1000; ++index)
				this.InsertStarSystem(new int?(index), "starsystem" + (object)(index + 1), new int?(0), "abc", new Vector3((float)index, (float)index, (float)index), true, true, new int?(0));
			for (int index = 0; index < 1000; ++index)
				this.InsertPlanet(new int?(), index / 2 + 1, OrbitalPath.Zero, "Planet" + (object)(index + 1), "barren", new int?(), 10f, 0, 0, 10f);
			this.db.ExecuteNonQuery("COMMIT TRANSACTION", false, true);
		}

		public int InsertCombatData(int systemID, int combatID, int turn, byte[] data)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertCombatData, (object)systemID.ToSQLiteValue(), (object)combatID.ToSQLiteValue(), (object)turn.ToSQLiteValue(), (object)data.ToSQLiteValue(), (object)1.ToSQLiteValue()));
		}

		public ScriptMessageReader GetMostRecentCombatData(int systemId)
		{
			string sqliteValue = this.db.ExecuteStringQuery(string.Format(Queries.GetMostRecentCombatData, (object)systemId));
			if (sqliteValue != null)
				return new ScriptMessageReader(true, new MemoryStream(sqliteValue.SQLiteValueToByteArray()));
			return (ScriptMessageReader)null;
		}

		public int[] GetRecentCombatTurns(int systemId, int oldestTurn)
		{
			int[] array = this.db.ExecuteIntegerArrayQuery(string.Format("SELECT turn FROM combat_data WHERE system_id={0} AND turn >= {1};", (object)systemId.ToSQLiteValue(), (object)oldestTurn.ToSQLiteValue()));
			Array.Sort<int>(array);
			return array;
		}

		public ScriptMessageReader GetCombatData(
		  int systemId,
		  int turn,
		  out int version)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetCombatData, (object)systemId, (object)turn), true).Rows)
			{
				string sqliteValue = row[0].SQLiteValueToString();
				version = row[1].SQLiteValueToInteger();
				if (sqliteValue != null)
					return new ScriptMessageReader(true, new MemoryStream(sqliteValue.SQLiteValueToByteArray()));
			}
			version = 0;
			return (ScriptMessageReader)null;
		}

		public ScriptMessageReader GetCombatData(
		  int systemId,
		  int combatID,
		  int turn,
		  out int version)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetCombatSpecificData, (object)systemId, (object)combatID, (object)turn), true).Rows)
			{
				string sqliteValue = row[0].SQLiteValueToString();
				version = row[1].SQLiteValueToInteger();
				if (sqliteValue != null)
					return new ScriptMessageReader(true, new MemoryStream(sqliteValue.SQLiteValueToByteArray()));
			}
			version = 0;
			return (ScriptMessageReader)null;
		}

		private IEnumerable<ScriptMessageReader> GetCombatDatas()
		{
			foreach (Row row in this.db.ExecuteTableQuery(Queries.GetCombatDatas, true))
			{
				byte[] data = row[0].SQLiteValueToByteArray();
				using (MemoryStream stream = new MemoryStream(data))
					yield return new ScriptMessageReader(true, stream);
			}
		}

		public void InsertColonyFaction(
		  int orbitalObjectID,
		  int factionId,
		  double civPopulation,
		  float civPopulationWeight,
		  int turnEstablished)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertColonyFaction, (object)orbitalObjectID.ToSQLiteValue(), (object)factionId.ToSQLiteValue(), (object)civPopulation.ToSQLiteValue(), (object)this.assetdb.CivilianPopulationStartMoral.ToSQLiteValue(), (object)civPopulationWeight.ToSQLiteValue(), (object)turnEstablished.ToSQLiteValue(), (object)this.assetdb.CivilianPopulationStartMoral.ToSQLiteValue()), false, true);
			this._dom.colonies.SyncRange((IEnumerable<int>)this._dom.colonies.Where<KeyValuePair<int, ColonyInfo>>((Func<KeyValuePair<int, ColonyInfo>, bool>)(x => x.Value.OrbitalObjectID == orbitalObjectID)).Select<KeyValuePair<int, ColonyInfo>, int>((Func<KeyValuePair<int, ColonyInfo>, int>)(y => y.Key)).ToList<int>());
		}

		public void AddPlanetToSystem(
		  int SystemId,
		  int? parentOrbitId,
		  string Name,
		  PlanetInfo pi,
		  int? OrbitNumber = null)
		{
			Random safeRandom = App.GetSafeRandom();
			float orbitStep = StarSystemVars.Instance.StarOrbitStep;
			float parentRadius = StarSystemVars.Instance.StarRadius(StellarClass.Parse(this.GetStarSystemInfo(SystemId).StellarClass).Size);
			int orbitNumber = OrbitNumber.HasValue ? OrbitNumber.Value : this.GetStarSystemOrbitalObjectInfos(SystemId).Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => !x.ParentID.HasValue)).Count<OrbitalObjectInfo>();
			if (parentOrbitId.HasValue)
			{
				orbitStep = this.GetPlanetInfo(parentOrbitId.Value) == null ? StarSystemVars.Instance.GasGiantOrbitStep : StarSystemVars.Instance.PlanetOrbitStep;
				parentRadius = StarSystemVars.Instance.SizeToRadius(this.GetPlanetInfo(parentOrbitId.Value).Size);
			}
			float eccentricity = safeRandom.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange);
			double num1 = (double)safeRandom.NextNormal(StarSystemVars.Instance.OrbitInclinationRange);
			float num2 = Kerberos.Sots.Orbit.CalcOrbitRadius(parentRadius, orbitStep, orbitNumber);
			float x1 = Ellipse.CalcSemiMinorAxis(num2, eccentricity);
			StarSystemInfo starSystemInfo = this.GetStarSystemInfo(SystemId);
			string name = string.IsNullOrEmpty(Name) ? starSystemInfo.Name + " " + (object)orbitNumber : Name;
			this.InsertPlanet(parentOrbitId, SystemId, new OrbitalPath()
			{
				Scale = new Vector2(x1, num2),
				InitialAngle = safeRandom.NextSingle() % 6.283185f
			}, name, pi.Type, new int?(), pi.Suitability, pi.Biosphere, pi.Resources, pi.Size);
		}

		public OrbitalPath OrbitNumberToOrbitalPath(
		  int orbitNumber,
		  Kerberos.Sots.StellarSize size,
		  float? orbitStep = null)
		{
			Random safeRandom = App.GetSafeRandom();
			float starOrbitStep = StarSystemVars.Instance.StarOrbitStep;
			if (orbitStep.HasValue)
				starOrbitStep = orbitStep.Value;
			float parentRadius = StarSystemVars.Instance.StarRadius(size);
			float eccentricity = safeRandom.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange);
			double num1 = (double)safeRandom.NextNormal(StarSystemVars.Instance.OrbitInclinationRange);
			float num2 = Kerberos.Sots.Orbit.CalcOrbitRadius(parentRadius, starOrbitStep, orbitNumber);
			float x = Ellipse.CalcSemiMinorAxis(num2, eccentricity);
			return new OrbitalPath()
			{
				Scale = new Vector2(x, num2),
				InitialAngle = safeRandom.NextSingle() % 6.283185f
			};
		}

		public void ImportStarMap(ref LegacyStarMap starmap, Random random, GameSession.Flags flags)
		{
			Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
			Dictionary<LegacyTerrain, int> dictionary2 = new Dictionary<LegacyTerrain, int>();
			foreach (LegacyTerrain index in starmap.Terrain)
			{
				int num = this.InsertTerrain(index.Name, index.Origin);
				dictionary2[index] = num;
			}
			foreach (Kerberos.Sots.StarSystem starSystem in starmap.Objects.OfType<Kerberos.Sots.StarSystem>())
			{
				Kerberos.Sots.StarSystem starsystem = starSystem;
				int starSystemId = this.InsertStarSystem(new int?(starsystem.Params.Guid.Value), starsystem.DisplayName, new int?(), starsystem.StellarClass.ToString(), starsystem.WorldTransform.Position, starsystem.Params.isVisible, true, starsystem.Terrain != null ? new int?(dictionary2[starsystem.Terrain]) : new int?());
				starsystem.ID = starSystemId;
				dictionary1.Add(starsystem.Params.Guid.Value, starsystem.ID);
				if ((flags & GameSession.Flags.NoOrbitalObjects) == (GameSession.Flags)0)
				{
					Dictionary<Kerberos.Sots.Data.StarMapFramework.Orbit, int> dictionary3 = new Dictionary<Kerberos.Sots.Data.StarMapFramework.Orbit, int>();
					List<IStellarEntity> list = starsystem.Objects.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x => x != starsystem.Star)).ToList<IStellarEntity>();
					while (list.Count > 0)
					{
						IStellarEntity stellarEntity = list[0];
						list.RemoveAt(0);
						int? parentOrbitalObjectId1 = stellarEntity.Orbit.Parent == starsystem.Star.Params ? new int?() : new int?(dictionary3[stellarEntity.Orbit.Parent]);
						OrbitalPath path = new OrbitalPath()
						{
							Scale = new Vector2(stellarEntity.Orbit.SemiMinorAxis, stellarEntity.Orbit.SemiMajorAxis),
							InitialAngle = (float)((double)stellarEntity.Orbit.Position * 2.0 * Math.PI)
						};
						PlanetInfo planetInfo = StarSystemHelper.InferPlanetInfo(stellarEntity.Params);
						if (planetInfo != null)
						{
							int num = this.InsertPlanet(parentOrbitalObjectId1, starSystemId, path, stellarEntity.Params.Name, planetInfo.Type, new int?(), planetInfo.Suitability, planetInfo.Biosphere, planetInfo.Resources, planetInfo.Size);
							dictionary3[stellarEntity.Params] = num;
							stellarEntity.ID = num;
						}
						else if (stellarEntity.Params is AsteroidOrbit)
						{
							int parentOrbitalObjectId2 = this.InsertAsteroidBelt(parentOrbitalObjectId1, starSystemId, path, stellarEntity.Params.Name, random.Next());
							dictionary3[stellarEntity.Params] = parentOrbitalObjectId2;
							stellarEntity.ID = parentOrbitalObjectId2;
							int num = random.Next(2, 5) - (3 - stellarEntity.Params.OrbitNumber);
							for (int index = 0; index < num; ++index)
							{
								int resources = random.Next(3000);
								this.InsertLargeAsteroid(parentOrbitalObjectId2, starSystemId, new OrbitalPath(), stellarEntity.Params.Name + (object)(char)(65 + index), resources);
							}
						}
						if (!dictionary3.ContainsKey(stellarEntity.Params))
							GameDatabase.Warn("Unhandled star system object type encountered during import to db: " + (object)stellarEntity.Params.GetType());
					}
				}
			}
			foreach (StellarProp stellarProp in starmap.Objects.OfType<StellarProp>())
				this.InsertStellarProp(stellarProp.Params.Model, stellarProp.Transform);
			foreach (NodeLine nodeLine in starmap.NodeLines)
			{
				NodeLine nodeline = nodeLine;
				if (starmap.Objects.OfType<Kerberos.Sots.StarSystem>().Any<Kerberos.Sots.StarSystem>((Func<Kerberos.Sots.StarSystem, bool>)(x =>
			   {
				   int? guid = x.Params.Guid;
				   int systemA = nodeline.SystemA;
				   if (guid.GetValueOrDefault() == systemA)
					   return guid.HasValue;
				   return false;
			   })) && starmap.Objects.OfType<Kerberos.Sots.StarSystem>().Any<Kerberos.Sots.StarSystem>((Func<Kerberos.Sots.StarSystem, bool>)(x =>
	   {
		   int? guid = x.Params.Guid;
		   int systemB = nodeline.SystemB;
		   if (guid.GetValueOrDefault() == systemB)
			   return guid.HasValue;
		   return false;
	   })) && !starmap.NodeLines.Any<NodeLine>((Func<NodeLine, bool>)(x =>
{
	if (x.SystemA == nodeline.SystemB && x.SystemB == nodeline.SystemA)
		return x.isPermanent == nodeline.isPermanent;
	return false;
})))
					this.InsertNodeLine(starmap.Objects.OfType<Kerberos.Sots.StarSystem>().First<Kerberos.Sots.StarSystem>((Func<Kerberos.Sots.StarSystem, bool>)(x =>
				   {
					   int? guid = x.Params.Guid;
					   int systemA = nodeline.SystemA;
					   if (guid.GetValueOrDefault() == systemA)
						   return guid.HasValue;
					   return false;
				   })).ID, starmap.Objects.OfType<Kerberos.Sots.StarSystem>().First<Kerberos.Sots.StarSystem>((Func<Kerberos.Sots.StarSystem, bool>)(x =>
		 {
			 int? guid = x.Params.Guid;
			 int systemB = nodeline.SystemB;
			 if (guid.GetValueOrDefault() == systemB)
				 return guid.HasValue;
			 return false;
		 })).ID, nodeline.isPermanent ? -1 : 1000);
			}
		}

		public float GetStationAdditionalStratSensorRange(StationInfo station)
		{
			DesignInfo designInfo = this.GetDesignInfo(station.DesignInfo.ID);
			int num1 = ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => x.Modules.Count<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(y => y.StationModuleType.Value == ModuleEnums.StationModuleType.Sensor))));
			float num2 = 0.0f;
			switch (designInfo.StationType)
			{
				case StationType.NAVAL:
					num2 += (float)num1 * 0.5f;
					break;
				case StationType.SCIENCE:
				case StationType.CIVILIAN:
					num2 += (float)num1 * 0.25f;
					break;
				case StationType.DIPLOMATIC:
					num2 += (float)num1 * 0.2f;
					break;
			}
			return station.DesignInfo.StratSensorRange + num2;
		}

		public float GetStationStratSensorRange(StationInfo station)
		{
			return station.GetBaseStratSensorRange() + this.GetStationAdditionalStratSensorRange(station);
		}

		public float GetSystemStratSensorRange(int systemid, int playerid)
		{
			DataObjectCache.SystemPlayerID key = new DataObjectCache.SystemPlayerID()
			{
				SystemID = systemid,
				PlayerID = playerid
			};
			float val1 = 0.0f;
			List<PlayerInfo> list = this.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (x.ID != playerid)
				   return this.GetDiplomacyStateBetweenPlayers(x.ID, playerid) == DiplomacyState.ALLIED;
			   return false;
		   })).ToList<PlayerInfo>();
			list.Add(this.GetPlayerInfo(playerid));
			if (!this._dom.CachedSystemStratSensorRanges.TryGetValue(key, out val1))
			{
				foreach (ColonyInfo colonyInfo in this.GetColonyInfosForSystem(systemid))
				{
					ColonyInfo colony = colonyInfo;
					if (list.Any<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == colony.PlayerID)))
						val1 = 5f;
				}
				foreach (PlayerInfo playerInfo in list)
				{
					foreach (StationInfo station in this.GetStationForSystemAndPlayer(systemid, playerInfo.ID))
						val1 = Math.Max(val1, this.GetStationStratSensorRange(station));
				}
				foreach (PlayerInfo playerInfo in list)
				{
					foreach (FleetInfo fleetInfo in this.GetFleetsByPlayerAndSystem(playerInfo.ID, systemid, FleetType.FL_ALL))
						val1 = Math.Max(val1, GameSession.GetFleetSensorRange(this.assetdb, this, fleetInfo.ID));
				}
				this._dom.CachedSystemStratSensorRanges.Add(key, val1);
			}
			return val1;
		}

		public int InsertNameValuePair(string name, string value)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertNameValuePair, (object)name, (object)value));
		}

		internal static string GetNameValue(SQLiteConnection db, string name)
		{
			return db.ExecuteStringQuery(string.Format(Queries.GetNameValue, (object)name));
		}

		public string GetNameValue(string name)
		{
			return GameDatabase.GetNameValue(this.db, name);
		}

		public T GetNameValue<T>(string name)
		{
			string str = this.db.ExecuteStringQuery(string.Format(Queries.GetNameValue, (object)name));
			if (string.IsNullOrEmpty(str))
				return default(T);
			return (T)Convert.ChangeType((object)str, typeof(T));
		}

		public T GetStratModifier<T>(StratModifiers modifier, int playerId, T defaultValue)
		{
			if (!GameDatabase._cachedStratMods.ContainsKey(playerId))
				GameDatabase._cachedStratMods.Add(playerId, new Dictionary<StratModifiers, CachedStratMod>());
			else if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier) && GameDatabase._cachedStratMods[playerId][modifier].isUpToDate)
				return (T)GameDatabase._cachedStratMods[playerId][modifier].CachedValue;
			string str = this.db.ExecuteStringQuery(string.Format(Queries.GetStratModifier, (object)modifier.ToString().ToSQLiteValue(), (object)playerId.ToSQLiteValue()));
			if (string.IsNullOrEmpty(str))
				return defaultValue;
			T obj = (T)Convert.ChangeType((object)str, typeof(T));
			if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier))
			{
				GameDatabase._cachedStratMods[playerId][modifier].CachedValue = (object)obj;
				GameDatabase._cachedStratMods[playerId][modifier].isUpToDate = true;
			}
			else
				GameDatabase._cachedStratMods[playerId].Add(modifier, new CachedStratMod()
				{
					CachedValue = (object)obj,
					isUpToDate = true
				});
			return obj;
		}

		public float GetStratModifierFloatToApply(StratModifiers modifier, int playerId)
		{
			float stratModifier = this.GetStratModifier<float>(modifier, playerId);
			return this.assetdb.GovEffects.GetStratModifierTotal(this, modifier, playerId, stratModifier);
		}

		public int GetStratModifierIntToApply(StratModifiers modifier, int playerId)
		{
			int stratModifier = this.GetStratModifier<int>(modifier, playerId);
			return this.assetdb.GovEffects.GetStratModifierTotal(this, modifier, playerId, stratModifier);
		}

		public T GetStratModifier<T>(StratModifiers modifier, int playerId)
		{
			if (!GameDatabase._cachedStratMods.ContainsKey(playerId))
				GameDatabase._cachedStratMods.Add(playerId, new Dictionary<StratModifiers, CachedStratMod>());
			else if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier) && GameDatabase._cachedStratMods[playerId][modifier].isUpToDate)
				return (T)GameDatabase._cachedStratMods[playerId][modifier].CachedValue;
			T obj = (T)Convert.ChangeType((object)this.db.ExecuteStringQuery(string.Format(Queries.GetStratModifier, (object)modifier.ToString().ToSQLiteValue(), (object)playerId.ToSQLiteValue())) ?? this.assetdb.DefaultStratModifiers[modifier], typeof(T));
			if (GameDatabase._cachedStratMods[playerId].ContainsKey(modifier))
			{
				GameDatabase._cachedStratMods[playerId][modifier].CachedValue = (object)obj;
				GameDatabase._cachedStratMods[playerId][modifier].isUpToDate = true;
			}
			else
				GameDatabase._cachedStratMods[playerId].Add(modifier, new CachedStratMod()
				{
					CachedValue = (object)obj,
					isUpToDate = true
				});
			return obj;
		}

		public void SetStratModifier(StratModifiers modifier, int playerId, object value)
		{
			if (GameDatabase._cachedStratMods.ContainsKey(playerId) && GameDatabase._cachedStratMods[playerId].ContainsKey(modifier))
				GameDatabase._cachedStratMods[playerId][modifier].isUpToDate = false;
			this.db.ExecuteNonQuery(string.Format(Queries.SetStratModifier, (object)modifier.ToString().ToSQLiteValue(), (object)playerId.ToSQLiteValue(), (object)value.ToString().ToSQLiteValue()), false, true);
		}

		private void AddNumShipsBuiltFromDesign(int designID, int count)
		{
			if (count <= 0)
				throw new ArgumentOutOfRangeException(nameof(count), "Must be > 0.");
			this.db.ExecuteNonQuery(string.Format(Queries.AddNumShipsBuiltFromDesign, (object)count.ToSQLiteValue(), (object)designID.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(designID);
		}

		private void AddNumShipsDestroyedFromDesign(int designID, int count)
		{
			if (count <= 0)
				throw new ArgumentOutOfRangeException(nameof(count), "Must be > 0.");
			this.db.ExecuteNonQuery(string.Format(Queries.AddNumShipsDestroyedFromDesign, (object)count.ToSQLiteValue(), (object)designID.ToSQLiteValue()), false, true);
			this._dom.designs.Sync(designID);
		}

		public IEnumerable<NodeLineInfo> GetNodeLines()
		{
			return this._dom.node_lines.Values;
		}

		public IEnumerable<NodeLineInfo> GetNonPermenantNodeLines()
		{
			return this._dom.node_lines.Values.Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
		   {
			   if (!x.IsPermenant && x.Health > -1)
				   return !x.IsLoaLine;
			   return false;
		   }));
		}

		public void SpendDiplomacyPoints(PlayerInfo spendingPlayer, int factionId, int pointCost)
		{
			int val2 = spendingPlayer.FactionDiplomacyPoints.ContainsKey(factionId) ? spendingPlayer.FactionDiplomacyPoints[factionId] : 0;
			int num = Math.Min(pointCost, val2);
			this.UpdateFactionDiplomacyPoints(spendingPlayer.ID, factionId, val2 - num);
			this.UpdateGenericDiplomacyPoints(spendingPlayer.ID, spendingPlayer.GenericDiplomacyPoints - (pointCost - num) * 2);
		}

		public NodeLineInfo GetNodeLine(int id)
		{
			return this._dom.node_lines.Values.FirstOrDefault<NodeLineInfo>((Func<NodeLineInfo, bool>)(x => x.ID == id));
		}

		public bool IsStealthFleet(int fleetId)
		{
			int techId = this.GetTechID("IND_Stealth_Armor");
			foreach (ShipInfo shipInfo in this.GetShipInfoByFleetID(fleetId, true).ToList<ShipInfo>())
			{
				foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
				{
					if (!designSection.Techs.Contains(techId))
						return false;
				}
			}
			return true;
		}

		public IEnumerable<NodeLineInfo> GetExploredNodeLines(int playerId)
		{
			int factionId = this.GetPlayerFactionID(playerId);
			string factionName = this.GetFactionName(factionId);
			if (factionName == "human" || factionName == "zuul" || factionName == "loa")
			{
				List<ExploreRecordInfo> ers = this._dom.explore_records.Values.Where<ExploreRecordInfo>((Func<ExploreRecordInfo, bool>)(x => x.PlayerId == playerId)).ToList<ExploreRecordInfo>();
				foreach (NodeLineInfo nodeLineInfo in this._dom.node_lines.Values)
				{
					NodeLineInfo nli = nodeLineInfo;
					if (ers.Any<ExploreRecordInfo>((Func<ExploreRecordInfo, bool>)(x =>
				   {
					   if (x.SystemId != nli.System1ID)
						   return x.SystemId == nli.System2ID;
					   return true;
				   })))
						yield return nli;
				}
			}
		}

		public NodeLineInfo GetNodeLineBetweenSystems(
		  int playerID,
		  int systemA,
		  int systemB,
		  bool isPermanent,
		  bool loaline = false)
		{
			if (!loaline)
				return this.GetExploredNodeLinesFromSystem(playerID, systemA).ToList<NodeLineInfo>().FirstOrDefault<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
			   {
				   if (x.System1ID == systemB || x.System2ID == systemB)
					   return x.IsPermenant == isPermanent;
				   return false;
			   }));
			return this.GetExploredNodeLinesFromSystem(playerID, systemA).ToList<NodeLineInfo>().FirstOrDefault<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
		   {
			   if ((x.System1ID == systemB || x.System2ID == systemB) && !x.IsPermenant)
				   return x.IsLoaLine == loaline;
			   return false;
		   }));
		}

		public void InsertLoaLineFleetRecord(int lineid, int fleetid)
		{
			this.db.ExecuteNonQuery(string.Format("INSERT INTO loa_line_records (node_line_id, fleet_id) VALUES ({0},{1})", (object)lineid.ToSQLiteValue(), (object)fleetid.ToSQLiteValue()), false, true);
			this._dom.node_lines.Sync(lineid);
		}

		public IEnumerable<int> GetFleetsForLoaLine(int lineID)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format("SELECT * FROM loa_line_records WHERE node_line_id = {0}", (object)lineID.ToSQLiteValue()), true);
			foreach (Row row in table)
				yield return row[1].SQLiteValueToInteger();
		}

		public IEnumerable<NodeLineInfo> GetExploredNodeLinesFromSystem(
		  int playerID,
		  int systemID)
		{
			List<NodeLineInfo> nlis = this._dom.node_lines.Values.Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
		   {
			   if (x.System1ID != systemID)
				   return x.System2ID == systemID;
			   return true;
		   })).ToList<NodeLineInfo>();
			List<ExploreRecordInfo> ers = this._dom.explore_records.Values.Where<ExploreRecordInfo>((Func<ExploreRecordInfo, bool>)(x => x.PlayerId == playerID)).ToList<ExploreRecordInfo>();
			foreach (NodeLineInfo nodeLineInfo in nlis)
			{
				NodeLineInfo nli = nodeLineInfo;
				if (ers.Any<ExploreRecordInfo>((Func<ExploreRecordInfo, bool>)(x =>
			   {
				   if (x.SystemId != nli.System1ID)
					   return x.SystemId == nli.System2ID;
				   return true;
			   })))
					yield return new NodeLineInfo()
					{
						ID = nli.ID,
						System1ID = nli.System1ID,
						System2ID = nli.System2ID,
						Health = nli.Health,
						IsLoaLine = nli.IsLoaLine
					};
			}
		}

		public HomeworldInfo GetPlayerHomeworld(int playerid)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerHomeworld, (object)playerid.ToSQLiteValue()), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() == 0)
				return (HomeworldInfo)null;
			Row row = table.Rows[0];
			HomeworldInfo homeworldInfo = new HomeworldInfo()
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				ColonyID = row[1].SQLiteValueToInteger(),
				SystemID = row[2].SQLiteValueToInteger()
			};
			ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(homeworldInfo.ColonyID);
			if (colonyInfoForPlanet == null || colonyInfoForPlanet.PlayerID != homeworldInfo.PlayerID)
				return (HomeworldInfo)null;
			homeworldInfo.ColonyID = colonyInfoForPlanet.ID;
			return homeworldInfo;
		}

		public IEnumerable<HomeworldInfo> GetHomeworlds()
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(Queries.GetHomeworlds, true);
			foreach (Row row in table)
			{
				HomeworldInfo hwi = new HomeworldInfo()
				{
					PlayerID = row[0].SQLiteValueToInteger(),
					ColonyID = row[1].SQLiteValueToInteger(),
					SystemID = row[2].SQLiteValueToInteger()
				};
				ColonyInfo ci = this.GetColonyInfoForPlanet(hwi.ColonyID);
				if (ci != null && ci.PlayerID == hwi.PlayerID)
				{
					hwi.ColonyID = ci.ID;
					yield return hwi;
				}
			}
		}

		public void UpdateHomeworldInfo(HomeworldInfo hw)
		{
			ColonyInfo colonyInfo = this.GetColonyInfo(hw.ColonyID);
			this.UpdatePlayerHomeworld(hw.PlayerID, colonyInfo.OrbitalObjectID);
		}

		internal static int GetTurnCount(SQLiteConnection db)
		{
			return GameDatabase.GetNameValue(db, "turn").SQLiteValueToInteger();
		}

		public int GetTurnCount()
		{
			return GameDatabase.GetTurnCount(this.db);
		}

		public Matrix GetOrbitalTransform(int orbitalId)
		{
			return this.GetOrbitalTransform(this.GetOrbitalObjectInfo(orbitalId));
		}

		public Matrix GetOrbitalTransform(OrbitalObjectInfo orbital)
		{
			int turnCount = this.GetTurnCount();
			return GameDatabase.CalcTransform(orbital.ID, (float)turnCount, this.GetStarSystemOrbitalObjectInfos(orbital.StarSystemID));
		}

		public void UpdateNameValuePair(string name, string value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateNameValuePair, (object)name.ToSQLiteValue(), (object)value.ToSQLiteValue()), true);
		}

		public void IncrementTurnCount()
		{
			int turnCount = this.GetTurnCount();
			this.UpdateNameValuePair("turn", (turnCount + 1).ToString());
			this._dom.CachedSystemStratSensorRanges.Clear();
			if ((turnCount + 1) % 20 != 0)
				return;
			this.db.VacuumDatabase();
		}

		public void CullQueryHistory(int numTurnsToKeep)
		{
			this.db.ExecuteNonQuery(string.Format("DELETE FROM query_history WHERE turn <= {0}", (object)(this.GetTurnCount() - numTurnsToKeep).ToSQLiteValue()), true, true);
		}

		public IEnumerable<int> GetStandardPlayerIDs()
		{
			return this._dom.players.GetStandardPlayerIDs();
		}

		public IEnumerable<int> GetPlayerIDs()
		{
			return this._dom.players.Keys;
		}

		public int GetPlayerResearchingTechID(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetPlayerResearchingTechID, (object)playerId.ToSQLiteValue()));
		}

		public int GetPlayerFeasibilityStudyTechId(int playerId)
		{
			FeasibilityStudyInfo studyInfoByPlayer = this.GetFeasibilityStudyInfoByPlayer(playerId);
			if (studyInfoByPlayer != null)
				return studyInfoByPlayer.TechID;
			return 0;
		}

		private PlayerTechInfo GetPlayerTechInfo(Row row)
		{
			return new PlayerTechInfo()
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				TechID = row[1].SQLiteValueToInteger(),
				State = (TechStates)row[2].SQLiteValueToInteger(),
				Progress = row[3].SQLiteValueToInteger(),
				TechFileID = row[4].SQLiteValueToString(),
				ResearchCost = row[5].SQLiteValueToInteger(),
				Feasibility = row[6].SQLiteValueToSingle(),
				PlayerFeasibility = row[7].SQLiteValueToSingle(),
				TurnResearched = row[8].SQLiteValueToNullableInteger()
			};
		}

		public PlayerTechInfo GetPlayerTechInfo(PlayerTechInfo.PrimaryKey id)
		{
			return this.GetPlayerTechInfo(id.PlayerID, id.TechID);
		}

		public PlayerTechInfo GetPlayerTechInfo(int playerId, int techId)
		{
			PlayerTechInfo playerTechInfo1 = this._dom.player_techs.Find(new PlayerTechInfo.PrimaryKey()
			{
				PlayerID = playerId,
				TechID = techId
			});
			if (playerTechInfo1 != null)
				return playerTechInfo1;
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerTechInfo, (object)playerId.ToSQLiteValue(), (object)techId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
				return (PlayerTechInfo)null;
			PlayerTechInfo playerTechInfo2 = this.GetPlayerTechInfo(table[0]);
			this._dom.player_techs.Cache(playerTechInfo2.ID, playerTechInfo2);
			return playerTechInfo2;
		}

		private ResearchProjectInfo GetResearchProjectInfo(Row row)
		{
			return new ResearchProjectInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToInteger(),
				State = (ProjectStates)row[2].SQLiteValueToInteger(),
				Name = row[3].SQLiteValueToString(),
				Progress = row[4].SQLiteValueToSingle()
			};
		}

		public IEnumerable<ResearchProjectInfo> GetResearchProjectInfos(
		  int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table rows = this.db.ExecuteTableQuery(string.Format(Queries.GetResearchProjectInfosForPlayer, (object)playerId.ToSQLiteValue()), true);
			foreach (Row row in rows)
				yield return this.GetResearchProjectInfo(row);
		}

		public IEnumerable<ResearchProjectInfo> GetResearchProjectInfos()
		{
			Kerberos.Sots.Data.SQLite.Table rows = this.db.ExecuteTableQuery(Queries.GetResearchProjectInfos, true);
			foreach (Row row in rows)
				yield return this.GetResearchProjectInfo(row);
		}

		public ResearchProjectInfo GetResearchProjectInfo(
		  int playerId,
		  int projectId)
		{
			return this.GetResearchProjectInfo(this.db.ExecuteTableQuery(string.Format(Queries.GetResearchProjectInfo, (object)playerId.ToSQLiteValue(), (object)projectId.ToSQLiteValue()), true)[0]);
		}

		public PlayerTechInfo GetLastPlayerTechResearched(int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLastPlayerTechResearched, (object)playerId), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() == 0)
				return (PlayerTechInfo)null;
			return this.GetPlayerTechInfo(table.Rows[0]);
		}

		private FeasibilityStudyInfo GetFeasibilityStudyInfo(Row row)
		{
			return new FeasibilityStudyInfo()
			{
				ProjectID = row[0].SQLiteValueToInteger(),
				TechID = row[1].SQLiteValueToInteger(),
				ResearchCost = row[2].SQLiteValueToSingle()
			};
		}

		public FeasibilityStudyInfo GetFeasibilityStudyInfo(int projectId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFeasibilityStudyInfo, (object)projectId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
				return (FeasibilityStudyInfo)null;
			return this.GetFeasibilityStudyInfo(table[0]);
		}

		public FeasibilityStudyInfo GetFeasibilityStudyInfo(
		  int playerId,
		  int techId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFeasibilityStudyInfoForTech, (object)playerId.ToSQLiteValue(), (object)techId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
				return (FeasibilityStudyInfo)null;
			return this.GetFeasibilityStudyInfo(table[0]);
		}

		public FeasibilityStudyInfo GetFeasibilityStudyInfoByPlayer(int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetFeasibilityStudyInfoForPlayer, (object)playerId.ToSQLiteValue()), true);
			if (table == null || table.Rows.Length == 0)
				return (FeasibilityStudyInfo)null;
			return this.GetFeasibilityStudyInfo(table[0]);
		}

		public void UpdatePlayerCurrentTradeIncome(int playerId, double value)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCurrentTradeIncome, (object)playerId, (object)value.ToSQLiteValue()), false, true);
			this._dom.players.Sync(playerId);
		}

		public double GetPlayerCurrentTradeIncome(int playerId)
		{
			return this._dom.players[playerId].CurrentTradeIncome;
		}

		public void UpdatePlayerAdditionalResearchPoints(int playerId, int researchPoints)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerAdditionalResearchPoints, (object)playerId, (object)researchPoints), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdateResearchProjectState(int projectId, ProjectStates state)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateResearchProjectState, (object)projectId.ToSQLiteValue(), (object)((int)state).ToSQLiteValue()), true);
		}

		private PlayerBranchInfo GetPlayerBranchInfo(Row row)
		{
			return new PlayerBranchInfo()
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				FromTechID = row[1].SQLiteValueToInteger(),
				ToTechID = row[2].SQLiteValueToInteger(),
				ResearchCost = row[3].SQLiteValueToInteger(),
				Feasibility = row[4].SQLiteValueToSingle()
			};
		}

		public IEnumerable<PlayerBranchInfo> GetBranchesToTech(
		  int playerId,
		  int techId)
		{
			if (this._dom.player_tech_branches.IsDirty)
			{
				foreach (Row row in this.db.ExecuteTableQuery("SELECT player_id,from_id,to_id,research_cost,feasibility FROM player_tech_branches", true))
				{
					PlayerBranchInfo playerBranchInfo = this.GetPlayerBranchInfo(row);
					this._dom.player_tech_branches.Cache(playerBranchInfo.ID, playerBranchInfo);
				}
				this._dom.player_tech_branches.IsDirty = false;
			}
			return this._dom.player_tech_branches.Values.Where<PlayerBranchInfo>((Func<PlayerBranchInfo, bool>)(x =>
		   {
			   if (x.PlayerID == playerId)
				   return x.ToTechID == techId;
			   return false;
		   }));
		}

		public IEnumerable<PlayerBranchInfo> GetUnlockedBranchesToTech(
		  int playerId,
		  int techId)
		{
			return this.GetBranchesToTech(playerId, techId).Where<PlayerBranchInfo>((Func<PlayerBranchInfo, bool>)(x => this.GetPlayerTechInfo(playerId, x.FromTechID).State != TechStates.Locked));
		}

		public void UpdateResearchProjectInfo(ResearchProjectInfo rpi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateResearchProjectInfo, (object)rpi.ID.ToSQLiteValue(), (object)rpi.PlayerID.ToSQLiteValue(), (object)rpi.Name.ToSQLiteValue(), (object)rpi.Progress.ToSQLiteValue(), (object)((int)rpi.State).ToSQLiteValue()), false, true);
		}

		public void UpdatePlayerTechState(int playerId, int techId, TechStates state)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerTechState, (object)playerId.ToSQLiteValue(), (object)techId.ToSQLiteValue(), (object)((int)state).ToSQLiteValue()), true);
			this._dom.player_techs.Clear();
		}

		public void UpdatePlayerTechInfo(PlayerTechInfo techInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerTechInfo, (object)techInfo.PlayerID.ToSQLiteValue(), (object)techInfo.TechID.ToSQLiteValue(), (object)((int)techInfo.State).ToSQLiteValue(), (object)techInfo.Progress.ToSQLiteValue(), (object)techInfo.ResearchCost.ToSQLiteValue(), (object)techInfo.Feasibility.ToSQLiteValue(), (object)techInfo.PlayerFeasibility.ToSQLiteValue(), (object)techInfo.TurnResearched.ToNullableSQLiteValue()), false, true);
			this._dom.player_techs.Clear();
		}

		public HashSet<string> GetResearchedTechGroups(int playerId)
		{
			List<PlayerTechInfo> source = new List<PlayerTechInfo>(this.GetPlayerTechInfos(playerId).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)));
			HashSet<string> stringSet = new HashSet<string>();
			foreach (BasicNameField techGroup1 in this.AssetDatabase.MasterTechTree.TechGroups)
			{
				BasicNameField techGroup = techGroup1;
				bool flag = false;
				foreach (Kerberos.Sots.Data.TechnologyFramework.Tech tech in this.AssetDatabase.MasterTechTree.Technologies.Where<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Group == techGroup.Name)))
				{
					Kerberos.Sots.Data.TechnologyFramework.Tech t = tech;
					if (source.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(y => y.TechFileID == t.Id)))
					{
						flag = true;
						break;
					}
				}
				if (flag)
					stringSet.Add(techGroup.Name);
			}
			return stringSet;
		}

		private bool TechMeetsRequirements(
		  AssetDatabase assetdb,
		  int playerId,
		  int techId,
		  HashSet<string> researchedGroups)
		{
			string techFileId = this.GetTechFileID(techId);
			foreach (BasicNameField require in assetdb.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techFileId)).Requires)
			{
				int techId1 = this.GetTechID(require.Name);
				if (techId1 == 0)
				{
					if (!researchedGroups.Contains(require.Name))
						return false;
				}
				else
				{
					PlayerTechInfo playerTechInfo = this.GetPlayerTechInfo(playerId, techId1);
					if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
						return false;
				}
			}
			return true;
		}

		private bool CalcResearchCost(
		  AssetDatabase assetdb,
		  int playerId,
		  int techId,
		  out int researchCost,
		  out float feasibility)
		{
			IEnumerable<PlayerBranchInfo> branchesToTech = this.GetBranchesToTech(playerId, techId);
			if (!branchesToTech.Any<PlayerBranchInfo>())
			{
				researchCost = 0;
				feasibility = 1f;
				return true;
			}
			researchCost = int.MaxValue;
			feasibility = 0.0f;
			foreach (PlayerBranchInfo playerBranchInfo in branchesToTech)
			{
				if (this.GetPlayerTechInfo(playerId, playerBranchInfo.FromTechID).State == TechStates.Researched)
				{
					researchCost = Math.Min(researchCost, playerBranchInfo.ResearchCost);
					feasibility = Math.Max(feasibility, playerBranchInfo.Feasibility);
				}
			}
			return researchCost != int.MaxValue;
		}

		private void UnlockTechs(AssetDatabase assetdb, int playerId)
		{
			HashSet<string> researchedTechGroups = this.GetResearchedTechGroups(playerId);
			foreach (PlayerTechInfo techInfo in this.GetPlayerTechInfos(playerId).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Locked)))
			{
				int researchCost;
				float feasibility;
				if (this.TechMeetsRequirements(assetdb, playerId, techInfo.TechID, researchedTechGroups) && this.CalcResearchCost(assetdb, playerId, techInfo.TechID, out researchCost, out feasibility))
				{
					techInfo.ResearchCost = researchCost;
					techInfo.Feasibility = feasibility;
					techInfo.PlayerFeasibility = Math.Max(Math.Min(feasibility, 1f), 0.01f);
					techInfo.State = (double)feasibility < 1.0 ? TechStates.Branch : TechStates.Core;
					this.UpdatePlayerTechInfo(techInfo);
				}
			}
		}

		private bool AutoAcquireTechs(AssetDatabase assetdb, int playerId)
		{
			bool flag = false;
			foreach (PlayerTechInfo playerTechInfo in this.GetPlayerTechInfos(playerId))
			{
				if (playerTechInfo.ResearchCost <= 0 && playerTechInfo.State != TechStates.Researched && playerTechInfo.State != TechStates.Locked)
				{
					flag = true;
					this.UpdatePlayerTechState(playerId, playerTechInfo.TechID, TechStates.Researched);
				}
			}
			return flag;
		}

		public void UpdateLockedTechs(AssetDatabase assetdb, int playerId)
		{
			do
			{
				this.UnlockTechs(assetdb, playerId);
			}
			while (this.AutoAcquireTechs(assetdb, playerId));
		}

		public void AcquireAdditionalInitialTechs(AssetDatabase assetdb, int playerId, int numTechs)
		{
			if (numTechs <= 0)
				return;
			int num = 0;
			while (num < numTechs)
			{
				List<PlayerTechInfo> list = this.GetPlayerTechInfos(playerId).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Core)).ToList<PlayerTechInfo>().OrderBy<PlayerTechInfo, int>((Func<PlayerTechInfo, int>)(y => y.ResearchCost)).ToList<PlayerTechInfo>();
				if (list.Count == 0)
					break;
				foreach (PlayerTechInfo techInfo in list)
				{
					techInfo.Progress = 100;
					techInfo.State = TechStates.Researched;
					this.UpdatePlayerTechInfo(techInfo);
					++num;
					if (num >= numTechs)
						break;
				}
				this.UpdateLockedTechs(assetdb, playerId);
			}
		}

		public IEnumerable<PlayerTechInfo> GetPlayerTechInfos(int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table rows = this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerTechInfos, (object)playerId.ToSQLiteValue()), true);
			foreach (Row row in rows)
				yield return this.GetPlayerTechInfo(row);
		}

		public string GetTechFileID(int techId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetTechFileID, (object)techId.ToSQLiteValue()));
		}

		public int GetTechID(string techFileId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetTechID, (object)techFileId.ToSQLiteValue()));
		}

		public IEnumerable<AITechWeightInfo> GetAITechWeightInfos(
		  int playerID)
		{
			Kerberos.Sots.Data.SQLite.Table rows = this.db.ExecuteTableQuery(string.Format(Queries.GetAITechWeights, (object)playerID.ToSQLiteValue()), true);
			foreach (Row row in rows)
				yield return new AITechWeightInfo()
				{
					PlayerID = playerID,
					Family = row[0].SQLiteValueToString(),
					TotalSpent = row[1].SQLiteValueToDouble(),
					Weight = row[2].SQLiteValueToSingle()
				};
		}

		public AITechWeightInfo GetAITechWeightInfo(int playerID, string family)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetAITechWeight, (object)family.ToSQLiteValue(), (object)playerID.ToSQLiteValue()), true);
			if (source.Count<Row>() < 1)
				return (AITechWeightInfo)null;
			Row row = source[0];
			return new AITechWeightInfo()
			{
				PlayerID = playerID,
				Family = family,
				TotalSpent = row[0].SQLiteValueToDouble(),
				Weight = row[1].SQLiteValueToSingle()
			};
		}

		public void UpdateAITechWeight(AITechWeightInfo techInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAITechWeight, (object)techInfo.Family.ToSQLiteValue(), (object)techInfo.PlayerID.ToSQLiteValue(), (object)techInfo.TotalSpent.ToSQLiteValue(), (object)techInfo.Weight.ToSQLiteValue()), false, true);
		}

		public void InsertAITechStyle(AITechStyleInfo style)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAITechStyle, (object)style.PlayerID.ToSQLiteValue(), (object)((int)style.TechFamily).ToSQLiteValue(), (object)style.CostFactor.ToSQLiteValue()), false, true);
		}

		public IEnumerable<AITechStyleInfo> GetAITechStyles(int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAITechStyles, (object)playerId.ToSQLiteValue()), true);
			if (table != null)
			{
				foreach (Row row in table)
					yield return new AITechStyleInfo()
					{
						PlayerID = row[0].SQLiteValueToInteger(),
						TechFamily = (TechFamilies)row[1].SQLiteValueToInteger(),
						CostFactor = row[2].SQLiteValueToSingle()
					};
			}
		}

		public AIInfo GetAIInfo(int playerID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetAIInfo, (object)playerID.ToSQLiteValue()), true);
			if (source.Count<Row>() < 1)
				return (AIInfo)null;
			Row row = source[0];
			return new AIInfo()
			{
				PlayerID = playerID,
				PlayerInfo = this.GetPlayerInfo(playerID),
				Stance = (AIStance)row[0].SQLiteValueToInteger(),
				Flags = (AIInfoFlags)row[1].SQLiteValueToInteger()
			};
		}

		public IEnumerable<AIFleetInfo> GetAIFleetInfos(int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table rows = this.db.ExecuteTableQuery(string.Format(Queries.GetAIFleetInfos, (object)playerId), true);
			foreach (Row row in rows)
				yield return new AIFleetInfo()
				{
					ID = row[0].SQLiteValueToInteger(),
					FleetID = row[2].SQLiteValueToNullableInteger(),
					FleetType = row[3].SQLiteValueToInteger(),
					SystemID = row[4].SQLiteValueToInteger(),
					InvoiceID = row[5].SQLiteValueToNullableInteger(),
					AdmiralID = row[6].SQLiteValueToNullableInteger(),
					FleetTemplate = row[7].SQLiteValueToString()
				};
		}

		public int InsertAIFleetInfo(int playerID, AIFleetInfo fleetInfo)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertAIFleetInfo, (object)playerID.ToSQLiteValue(), (object)fleetInfo.FleetID.ToNullableSQLiteValue(), (object)fleetInfo.FleetType.ToSQLiteValue(), (object)fleetInfo.SystemID.ToSQLiteValue(), (object)fleetInfo.InvoiceID.ToNullableSQLiteValue(), (object)fleetInfo.AdmiralID.ToNullableSQLiteValue(), (object)fleetInfo.FleetTemplate.ToSQLiteValue()));
		}

		public void UpdateAIFleetInfo(AIFleetInfo fleetInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIFleetInfo, (object)fleetInfo.ID, (object)fleetInfo.FleetID.ToNullableSQLiteValue(), (object)fleetInfo.FleetType.ToSQLiteValue(), (object)fleetInfo.SystemID.ToSQLiteValue(), (object)fleetInfo.InvoiceID.ToNullableSQLiteValue(), (object)fleetInfo.AdmiralID.ToNullableSQLiteValue()), false, true);
		}

		public void UpdateAIInfo(AIInfo aiInfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAIInfo, (object)aiInfo.PlayerID.ToSQLiteValue(), (object)((int)aiInfo.Stance).ToSQLiteValue(), (object)((int)aiInfo.Flags).ToSQLiteValue()), false, true);
		}

		public void RemoveAIFleetInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveAIInfo, (object)id.ToSQLiteValue()), false, true);
			foreach (int key in this._dom.ships.Values.Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
		   {
			   int? aiFleetId = x.AIFleetID;
			   int num = id;
			   if (aiFleetId.GetValueOrDefault() == num)
				   return aiFleetId.HasValue;
			   return false;
		   })).Select<ShipInfo, int>((Func<ShipInfo, int>)(y => y.ID)).ToList<int>())
				this._dom.ships.Sync(key);
		}

		private static AIColonyIntel GetColonyIntel(Row row)
		{
			return new AIColonyIntel()
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				OwningPlayerID = row[1].SQLiteValueToInteger(),
				PlanetID = row[2].SQLiteValueToInteger(),
				ColonyID = row[3].SQLiteValueToNullableInteger(),
				LastSeen = row[4].SQLiteValueToInteger(),
				ImperialPopulation = row[5].SQLiteValueToDouble()
			};
		}

		private AIColonyIntel GetColonyIntelWithFallback(
		  int playerID,
		  int? colonyID,
		  int? planetID,
		  Kerberos.Sots.Data.SQLite.Table table)
		{
			if (table.Rows.Length > 0)
				return GameDatabase.GetColonyIntel(table[0]);
			if (colonyID.HasValue)
				this.GetColonyInfo(colonyID.Value);
			else if (planetID.HasValue)
				this.GetColonyInfoForPlanet(planetID.Value);
			return (AIColonyIntel)null;
		}

		public IEnumerable<AIPlanetIntel> GetPlanetIntelsForPlayer(int playerID)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelPlanetForPlayer, (object)playerID.ToSQLiteValue()), true);
			foreach (Row row in table.Rows)
				yield return GameDatabase.GetPlanetIntel(row);
		}

		public IEnumerable<AIColonyIntel> GetColonyIntelsForPlayer(int playerID)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColonyForPlayer, (object)playerID.ToSQLiteValue()), true);
			foreach (Row row in table.Rows)
				yield return GameDatabase.GetColonyIntel(row);
		}

		public AIColonyIntel GetColonyIntel(int playerID, int colonyID)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColony, (object)playerID.ToSQLiteValue(), (object)colonyID.ToSQLiteValue()), true);
			return this.GetColonyIntelWithFallback(playerID, new int?(colonyID), new int?(), table);
		}

		public AIColonyIntel GetColonyIntelForPlanet(int playerID, int planetID)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColonyForPlanet, (object)playerID.ToSQLiteValue(), (object)planetID.ToSQLiteValue()), true);
			return this.GetColonyIntelWithFallback(playerID, new int?(), new int?(planetID), table);
		}

		public IEnumerable<AIColonyIntel> GetColonyIntelOfTargetPlayer(
		  int playerID,
		  int intelOnPlayerID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelColoniesOfTargetPlayer, (object)playerID.ToSQLiteValue(), (object)intelOnPlayerID.ToSQLiteValue()), true))
				yield return GameDatabase.GetColonyIntel(row);
		}

		private static AIPlanetIntel GetPlanetIntel(Row row)
		{
			return new AIPlanetIntel()
			{
				PlayerID = row[0].SQLiteValueToInteger(),
				PlanetID = row[1].SQLiteValueToInteger(),
				LastSeen = row[2].SQLiteValueToInteger(),
				Biosphere = row[3].SQLiteValueToInteger(),
				Resources = row[4].SQLiteValueToInteger(),
				Infrastructure = row[5].SQLiteValueToSingle(),
				Suitability = row[6].SQLiteValueToSingle()
			};
		}

		public AIPlanetIntel GetPlanetIntel(int playerID, int planet)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAIIntelPlanet, (object)playerID.ToSQLiteValue(), (object)planet.ToSQLiteValue()), true);
			if (table.Rows.Length != 0)
				return GameDatabase.GetPlanetIntel(table[0]);
			PlanetInfo planetInfo = this.GetPlanetInfo(planet);
			return new AIPlanetIntel()
			{
				Biosphere = planetInfo.Biosphere,
				Infrastructure = planetInfo.Infrastructure,
				LastSeen = 0,
				PlanetID = planet,
				PlayerID = playerID,
				Resources = planetInfo.Resources,
				Suitability = planetInfo.Suitability
			};
		}

		public AIStationIntel GetStationIntel(int playerID, int stationID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetIntelStation, (object)playerID.ToSQLiteValue(), (object)stationID.ToSQLiteValue()), true);
			if (source.Count<Row>() < 1)
				return (AIStationIntel)null;
			Row row = source[0];
			return new AIStationIntel()
			{
				PlayerID = playerID,
				IntelOnPlayerID = row[0].SQLiteValueToInteger(),
				StationID = stationID,
				LastSeen = row[1].SQLiteValueToInteger(),
				Level = row[2].SQLiteValueToInteger()
			};
		}

		public IEnumerable<AIStationIntel> GetStationIntelsOfTargetPlayer(
		  int playerID,
		  int intelOnPlayerID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelStationsOfTargetPlayer, (object)playerID.ToSQLiteValue(), (object)intelOnPlayerID.ToSQLiteValue()), true))
				yield return new AIStationIntel()
				{
					PlayerID = playerID,
					IntelOnPlayerID = intelOnPlayerID,
					StationID = row[0].SQLiteValueToInteger(),
					LastSeen = row[1].SQLiteValueToInteger(),
					Level = row[2].SQLiteValueToInteger()
				};
		}

		public void UpdateStationIntel(int playerID, int stationID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateIntelStation, (object)playerID.ToSQLiteValue(), (object)stationID.ToSQLiteValue(), (object)this.GetTurnCount().ToSQLiteValue()), false, true);
		}

		public void RemoveStationIntel(int playerID, int stationID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveIntelStation, (object)playerID.ToSQLiteValue(), (object)stationID.ToSQLiteValue()), false, true);
		}

		public IEnumerable<AIFleetIntel> GetFleetIntelsOfTargetPlayer(
		  int playerID,
		  int intelOnPlayerID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelFleetsOfTargetPlayer, (object)playerID.ToSQLiteValue(), (object)intelOnPlayerID.ToSQLiteValue()), true))
				yield return new AIFleetIntel()
				{
					PlayerID = playerID,
					IntelOnPlayerID = intelOnPlayerID,
					LastSeen = row[0].SQLiteValueToInteger(),
					LastSeenSystem = row[1].SQLiteValueToInteger(),
					LastSeenCoords = row[2].SQLiteValueToVector3(),
					NumDestroyers = row[3].SQLiteValueToInteger(),
					NumCruisers = row[4].SQLiteValueToInteger(),
					NumDreadnoughts = row[5].SQLiteValueToInteger(),
					NumLeviathans = row[6].SQLiteValueToInteger()
				};
		}

		public IEnumerable<AIDesignIntel> GetDesignIntelsOfTargetPlayer(
		  int playerID,
		  int intelOnPlayerID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetIntelDesignsOfTargetPlayer, (object)playerID, (object)intelOnPlayerID), true))
				yield return new AIDesignIntel()
				{
					PlayerID = playerID,
					IntelOnPlayerID = intelOnPlayerID,
					DesignID = row[0].SQLiteValueToInteger(),
					FirstSeen = row[1].SQLiteValueToInteger(),
					LastSeen = row[2].SQLiteValueToInteger(),
					Salvaged = row[3].SQLiteValueToBoolean()
				};
		}

		public int GetSectionAssetID(string filepath, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetSectionAssetID, (object)filepath.ToSQLiteValue(), (object)playerId.ToSQLiteValue()));
		}

		public int GetModuleID(string module, int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetModuleID, (object)module.ToSQLiteValue(), (object)playerId.ToSQLiteValue()));
		}

		public string GetModuleAsset(int moduleId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetModuleAsset, (object)moduleId.ToSQLiteValue()));
		}

		public int? GetWeaponID(string weapon, int playerId)
		{
			return this.db.ExecuteIntegerQueryDefault(string.Format(Queries.GetWeaponID, (object)playerId.ToSQLiteValue(), (object)weapon.ToSQLiteValue()), new int?());
		}

		public string GetWeaponAsset(int weaponId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetWeaponAsset, (object)weaponId.ToSQLiteValue()));
		}

		public string GetFactionName(int factionId)
		{
			return this.db.ExecuteStringQuery(string.Format(Queries.GetFactionName, (object)factionId.ToSQLiteValue()));
		}

		public int GetFactionIdFromName(string factionName)
		{
			return this.db.ExecuteTableQuery(string.Format(Queries.GetFactionIdFromName, (object)factionName.ToSQLiteValue()), true)[0][0].SQLiteValueToInteger();
		}

		public FactionInfo ParseFactionInfo(Row row)
		{
			return PlayersCache.GetFactionInfoFromRow(row);
		}

		public FactionInfo GetFactionInfo(int factionId)
		{
			return this.ParseFactionInfo(this.db.ExecuteTableQuery(string.Format(Queries.GetFactionInfo, (object)factionId.ToSQLiteValue()), true)[0]);
		}

		public float GetFactionSuitability(string faction)
		{
			return float.Parse(this.db.ExecuteTableQuery(string.Format(Queries.GetFactionSuitability, (object)faction), true).Rows[0].Values[0]);
		}

		public float GetFactionSuitability(int factionId)
		{
			return float.Parse(this.db.ExecuteTableQuery(string.Format(Queries.GetFactionSuitabilityById, (object)factionId), true).Rows[0].Values[0]);
		}

		public float GetPlayerSuitability(int playerID)
		{
			return this.GetFactionSuitability(this.GetFactionName(this.GetPlayerInfo(playerID).FactionID));
		}

		private PlayerInfo ParsePlayer(Row row)
		{
			return PlayersCache.GetPlayerInfoFromRow(this.db, row);
		}

		public void SyncTradeNodes(TradeResultsTable trt)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.ClearTradeResults, (object)(this.GetTurnCount() - 1)), false, true);
			foreach (KeyValuePair<int, TradeNode> tradeNode in trt.TradeNodes)
				this.db.ExecuteNonQuery(string.Format(Queries.InsertTradeResult, (object)tradeNode.Key.ToSQLiteValue(), (object)tradeNode.Value.Produced.ToSQLiteValue(), (object)tradeNode.Value.ProductionCapacity.ToSQLiteValue(), (object)tradeNode.Value.Consumption.ToSQLiteValue(), (object)tradeNode.Value.Freighters.ToSQLiteValue(), (object)tradeNode.Value.DockCapacity.ToSQLiteValue(), (object)tradeNode.Value.ExportInt.ToSQLiteValue(), (object)tradeNode.Value.ExportProv.ToSQLiteValue(), (object)tradeNode.Value.ExportLoc.ToSQLiteValue(), (object)tradeNode.Value.ImportInt.ToSQLiteValue(), (object)tradeNode.Value.ImportProv.ToSQLiteValue(), (object)tradeNode.Value.ImportLoc.ToSQLiteValue(), (object)tradeNode.Value.Range.ToSQLiteValue(), (object)this.GetTurnCount()), false, true);
		}

		public TradeResultsTable GetTradeResultsTable()
		{
			TradeResultsTable tradeResultsTable = new TradeResultsTable();
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetTradeResults, (object)(this.GetTurnCount() - 1)), true).Rows)
				tradeResultsTable.TradeNodes.Add(row[0].SQLiteValueToInteger(), this.ParseTradeNode(row));
			return tradeResultsTable;
		}

		public TradeResultsTable GetLastTradeResultsHistoryTable()
		{
			TradeResultsTable tradeResultsTable = new TradeResultsTable();
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetTradeResults, (object)(this.GetTurnCount() - 2)), true).Rows)
				tradeResultsTable.TradeNodes.Add(row[0].SQLiteValueToInteger(), this.ParseTradeNode(row));
			return tradeResultsTable;
		}

		public TradeNode ParseTradeNode(Row r)
		{
			return new TradeNode()
			{
				Produced = r[1].SQLiteValueToInteger(),
				ProductionCapacity = r[2].SQLiteValueToInteger(),
				Consumption = r[3].SQLiteValueToInteger(),
				Freighters = r[4].SQLiteValueToInteger(),
				DockCapacity = r[5].SQLiteValueToInteger(),
				ExportInt = r[6].SQLiteValueToInteger(),
				ExportProv = r[7].SQLiteValueToInteger(),
				ExportLoc = r[8].SQLiteValueToInteger(),
				ImportInt = r[9].SQLiteValueToInteger(),
				ImportProv = r[10].SQLiteValueToInteger(),
				ImportLoc = r[11].SQLiteValueToInteger(),
				Range = r[12].SQLiteValueToSingle(),
				Turn = r[13].SQLiteValueToInteger()
			};
		}

		public int GetPlayerBankruptcyTurns(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetBankruptcyTurns, (object)playerId));
		}

		public void UpdatePlayerBankruptcyTurns(int playerId, int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateBankruptcyTurns, (object)playerId, (object)turn), false, true);
		}

		public void UpdatePlayerIntelPoints(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerIntelPoints, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerCounterintelPoints(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCounterintelPoints, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerOperationsPoints(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerOperationsPoints, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerIntelAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerIntelAccumulator, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerCounterintelAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCounterintelAccumulator, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerOperationsAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerOperationsAccumulator, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerCivilianMiningAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCivilianMiningAccumulator, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerCivilianColonizationAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCivilianColonizationAccumulator, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePlayerCivilianTradeAccumulator(int playerId, int newValue)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCivilianTradeAccumulator, (object)playerId, (object)newValue), false, true);
			this._dom.players.Sync(playerId);
		}

		public Dictionary<int, int> GetFactionDiplomacyPoints(int playerId)
		{
			return PlayersCache.GetFactionDiplomacyPoints(this.db, playerId);
		}

		public IEnumerable<FactionInfo> GetFactions()
		{
			return PlayersCache.GetFactions(this.db);
		}

		public void UpdateFactionDiplomacyPoints(int playerId, int factionId, int newNumPoints)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFactionDiplomacyPoints, (object)playerId.ToSQLiteValue(), (object)factionId.ToSQLiteValue(), (object)newNumPoints.ToSQLiteValue()), false, true);
		}

		public void UpdateGenericDiplomacyPoints(int playerId, int newNumPoints)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateGenericDiplomacyPoints, (object)playerId.ToSQLiteValue(), (object)newNumPoints.ToSQLiteValue()), false, true);
			this._dom.players.Sync(playerId);
		}

		public IEnumerable<PlayerInfo> GetStandardPlayerInfos()
		{
			return this.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.isStandardPlayer));
		}

		public IEnumerable<PlayerInfo> GetIndyPlayerInfos()
		{
			List<Faction> indyFactions = this.assetdb.Factions.Where<Faction>((Func<Faction, bool>)(x => x.IsIndependent())).ToList<Faction>();
			return this.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return indyFactions.Any<Faction>((Func<Faction, bool>)(y => y.Name == x.Name));
			   return false;
		   }));
		}

		public IEnumerable<PlayerInfo> GetPlayerInfos()
		{
			return this._dom.players.Values;
		}

		public PlayerInfo GetPlayerInfo(int playerID)
		{
			if (playerID == 0 || !this._dom.players.ContainsKey(playerID))
				return (PlayerInfo)null;
			return this._dom.players[playerID];
		}

		public int GetOrInsertIndyPlayerId(
		  GameDatabase gamedb,
		  int factionId,
		  string factionName,
		  string avatarPath = "")
		{
			PlayerInfo playerInfo = this.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (x.FactionID == factionId && !x.isStandardPlayer)
				   return x.Name.Contains(factionName);
			   return false;
		   }));
			if (playerInfo != null)
				return playerInfo.ID;
			FactionInfo factionInfo = this.GetFactionInfo(factionId);
			return this.InsertPlayer(factionName, factionInfo.Name, new int?(), Vector3.One, Vector3.One, "", avatarPath, 0.0, 0, false, true, false, 0, AIDifficulty.Normal);
		}

		public int GetOrInsertIndyPlayerId(
		  GameSession sim,
		  int factionId,
		  string factionName,
		  string avatarPath = "")
		{
			PlayerInfo playerInfo = this.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (x.FactionID == factionId && !x.isStandardPlayer)
				   return x.Name.Contains(factionName);
			   return false;
		   }));
			if (playerInfo != null)
				return playerInfo.ID;
			FactionInfo factionInfo = this.GetFactionInfo(factionId);
			int playerID = this.InsertPlayer(factionName, factionInfo.Name, new int?(), Vector3.One, Vector3.One, "", avatarPath, 0.0, 0, false, true, false, 0, AIDifficulty.Normal);
			sim.AddPlayerObject(sim.GameDatabase.GetPlayerInfo(playerID), Player.ClientTypes.AI);
			return playerID;
		}

		public void DuplicateStratModifiers(int newPlayer, int oldPlayer)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DuplicateStratModifiers, (object)newPlayer, (object)oldPlayer), false, true);
		}

		public void DuplicateTechs(int newPlayer, int oldPlayer)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DuplicateTechs, (object)newPlayer, (object)oldPlayer), false, true);
		}

		public void UpdatePsionicPotential(int playerId, int newPotential)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerPsionicPotential, (object)playerId, (object)newPotential), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdateLastEncounterTurn(int id, int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLastEncounterTurn, (object)id, (object)turn), false, true);
			this._dom.players.Sync(id);
		}

		public void UpdatePlayerLastCombatTurn(int id, int turn)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerCombatTurn, (object)id, (object)turn), false, true);
			this._dom.players.Sync(id);
		}

		public void UpdatePlayerSavings(int playerID, double savings)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerSavings, (object)playerID, (object)savings), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerResearchBoost(int playerID, double boostamount)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerResearchBoost, (object)playerID, (object)boostamount), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerAutoPlaceDefenses(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoPlaceDefenses, (object)playerID, (object)value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerAutoRepairFleets(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoRepairFleets, (object)playerID, (object)value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerAutoUseGoop(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoUseGoop, (object)playerID, (object)value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerAutoUseJoker(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoUseJoker, (object)playerID, (object)value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerAutoUseAOE(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoAOE, (object)playerID, (object)value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerTeam(int playerID, int team)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerTeam, (object)playerID, (object)team.ToSQLiteValue()), true);
		}

		public void UpdatePlayerAutoPatrol(int playerID, bool value)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerAutoPatrol, (object)playerID, (object)value.ToSQLiteValue()), true);
			this._dom.players.Sync(playerID);
		}

		public void UpdatePlayerHomeworld(int playerid, int planetid)
		{
			ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(planetid);
			if (colonyInfoForPlanet != null)
			{
				int playerId = colonyInfoForPlanet.PlayerID;
			}
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlayerHomeWorld, (object)planetid, (object)playerid), false, true);
			this._dom.players.Sync(playerid);
		}

		public void SetPlayerDefeated(GameSession game, int playerID)
		{
			PlayerInfo playerInfo = this.GetPlayerInfo(playerID);
			bool isDefeated = playerInfo.isDefeated;
			this.db.ExecuteNonQuery(string.Format(Queries.SetPlayerDefeated, (object)playerID), false, true);
			this._dom.players.Sync(playerID);
			if (!isDefeated && playerInfo.isStandardPlayer)
			{
				foreach (int standardPlayerId in this.GetStandardPlayerIDs())
				{
					if (standardPlayerId != playerID)
						this.InsertTurnEvent(new TurnEvent()
						{
							EventType = TurnEventType.EV_EMPIRE_DESTROYED,
							EventMessage = TurnEventMessage.EM_EMPIRE_DESTROYED,
							TurnNumber = this.GetTurnCount(),
							PlayerID = standardPlayerId,
							TargetPlayerID = playerID
						});
				}
			}
			foreach (SuulkaInfo suulkaInfo in this.GetSuulkas().ToList<SuulkaInfo>())
			{
				if (suulkaInfo.PlayerID.HasValue && suulkaInfo.PlayerID.Value == playerID)
					this.ReturnSuulka(game, suulkaInfo.ID);
			}
			foreach (FleetInfo fleetInfo in this.GetFleetInfosByPlayerID(playerID, FleetType.FL_ALL).ToList<FleetInfo>())
				this.RemoveFleet(fleetInfo.ID);
			foreach (StationInfo stationInfo in this.GetStationInfosByPlayerID(playerID).ToList<StationInfo>())
				this.DestroyStation(game, stationInfo.OrbitalObjectID, 0);
			foreach (ColonyInfo colonyInfo in this.GetPlayerColoniesByPlayerId(playerID).ToList<ColonyInfo>())
				this.RemoveColonyOnPlanet(colonyInfo.OrbitalObjectID);
		}

		public void UpdatePlayerSliders(GameSession game, PlayerInfo p)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlayerSliderRates, (object)p.ID, (object)p.RateGovernmentResearch, (object)p.RateResearchCurrentProject, (object)p.RateResearchSpecialProject, (object)p.RateResearchSalvageResearch, (object)p.RateGovernmentStimulus, (object)p.RateGovernmentSecurity, (object)p.RateGovernmentSavings, (object)p.RateStimulusMining, (object)p.RateStimulusColonization, (object)p.RateStimulusTrade, (object)p.RateSecurityOperations, (object)p.RateSecurityIntelligence, (object)p.RateSecurityCounterIntelligence), true);
			this._dom.players.Sync(p.ID);
		}

		public void UpdateTaxRate(int playerId, float taxRate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateTaxRate, (object)playerId, (object)taxRate), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdatePreviousTaxRate(int playerId, float taxRate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePreviousTaxRate, (object)playerId, (object)taxRate), false, true);
			this._dom.players.Sync(playerId);
		}

		public void UpdateImmigrationRate(int playerId, float immigrationRate)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateImmigrationRate, (object)playerId, (object)immigrationRate), false, true);
			this._dom.players.Sync(playerId);
		}

		public IEnumerable<int> GetHomeWorldIDs()
		{
			return this._dom.players.Where<KeyValuePair<int, PlayerInfo>>((Func<KeyValuePair<int, PlayerInfo>, bool>)(x => x.Value.Homeworld.HasValue)).Select<KeyValuePair<int, PlayerInfo>, int>((Func<KeyValuePair<int, PlayerInfo>, int>)(y => y.Value.Homeworld.Value));
		}

		public GovernmentInfo GetGovernmentInfo(int playerID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetGovernmentInfo, (object)playerID), true)[0];
			return new GovernmentInfo()
			{
				PlayerID = playerID,
				Authoritarianism = float.Parse(row[0]),
				EconomicLiberalism = float.Parse(row[1]),
				CurrentType = (GovernmentInfo.GovernmentType)Enum.Parse(typeof(GovernmentInfo.GovernmentType), row[2])
			};
		}

		public GovernmentActionInfo ParseGovernmentActionInfo(Row r)
		{
			return new GovernmentActionInfo()
			{
				PlayerId = r[0].SQLiteValueToInteger(),
				ID = r[1].SQLiteValueToInteger(),
				Description = r[2].SQLiteValueToString(),
				AuthoritarianismChange = r[3].SQLiteValueToInteger(),
				EconLiberalismChange = r[4].SQLiteValueToInteger(),
				Turn = r[5].SQLiteValueToInteger()
			};
		}

		public IEnumerable<GovernmentActionInfo> GetGovernmentActions(
		  int playerID)
		{
			Kerberos.Sots.Data.SQLite.Table Table = this.db.ExecuteTableQuery(string.Format(Queries.GetGovernmentActions, (object)playerID), true);
			foreach (Row r in Table)
				yield return this.ParseGovernmentActionInfo(r);
		}

		public void InsertGovernmentAction(
		  int playerId,
		  string description,
		  string govAction,
		  int x = 0,
		  int y = 0)
		{
			GovActionValues govActionValues = (GovActionValues)null;
			if (!string.IsNullOrEmpty(govAction))
				govActionValues = this.assetdb.GetGovActionValues(govAction);
			if (govActionValues == null)
				govActionValues = new GovActionValues()
				{
					XChange = x,
					YChange = y
				};
			this.db.ExecuteNonQuery(string.Format(Queries.InsertGovernmentAction, (object)playerId, (object)description, (object)govActionValues.YChange, (object)govActionValues.XChange, (object)this.GetTurnCount(), (object)this.assetdb.MaxGovernmentShift), false, true);
		}

		public void UpdateGovernmentInfo(GovernmentInfo government)
		{
			float num1 = 1f;
			float num2 = -0.67f * (float)this.assetdb.MaxGovernmentShift;
			float num3 = 0.67f * (float)this.assetdb.MaxGovernmentShift;
			int num4 = 0;
			int num5 = 0;
			switch (government.CurrentType)
			{
				case GovernmentInfo.GovernmentType.Centrism:
					num4 = 1;
					num5 = 1;
					break;
				case GovernmentInfo.GovernmentType.Communalism:
					num4 = 2;
					num5 = 0;
					break;
				case GovernmentInfo.GovernmentType.Junta:
					num4 = 2;
					num5 = 1;
					break;
				case GovernmentInfo.GovernmentType.Plutocracy:
					num4 = 2;
					num5 = 2;
					break;
				case GovernmentInfo.GovernmentType.Socialism:
					num4 = 1;
					num5 = 0;
					break;
				case GovernmentInfo.GovernmentType.Mercantilism:
					num4 = 1;
					num5 = 2;
					break;
				case GovernmentInfo.GovernmentType.Cooperativism:
					num4 = 0;
					num5 = 0;
					break;
				case GovernmentInfo.GovernmentType.Anarchism:
					num4 = 0;
					num5 = 1;
					break;
				case GovernmentInfo.GovernmentType.Liberationism:
					num4 = 0;
					num5 = 2;
					break;
			}
			if (num4 == 0 && (double)government.Authoritarianism > (double)num2 + (double)num1)
				num4 = 1;
			else if (num4 == 1 && (double)government.Authoritarianism > (double)num3 + (double)num1)
				num4 = 2;
			else if (num4 == 2 && (double)government.Authoritarianism < (double)num3 - (double)num1)
				num4 = 1;
			else if (num4 == 1 && (double)government.Authoritarianism < (double)num2 - (double)num1)
				num4 = 0;
			if (num5 == 0 && (double)government.EconomicLiberalism > (double)num2 + (double)num1)
				num5 = 1;
			else if (num5 == 1 && (double)government.EconomicLiberalism > (double)num3 + (double)num1)
				num5 = 2;
			else if (num5 == 2 && (double)government.EconomicLiberalism < (double)num3 - (double)num1)
				num5 = 1;
			else if (num5 == 1 && (double)government.EconomicLiberalism < (double)num2 - (double)num1)
				num5 = 0;
			switch (num4)
			{
				case 0:
					switch (num5)
					{
						case 0:
							government.CurrentType = GovernmentInfo.GovernmentType.Cooperativism;
							break;
						case 1:
							government.CurrentType = GovernmentInfo.GovernmentType.Anarchism;
							break;
						default:
							government.CurrentType = GovernmentInfo.GovernmentType.Liberationism;
							break;
					}
					break;
				case 1:
					switch (num5)
					{
						case 0:
							government.CurrentType = GovernmentInfo.GovernmentType.Socialism;
							break;
						case 1:
							government.CurrentType = GovernmentInfo.GovernmentType.Centrism;
							break;
						default:
							government.CurrentType = GovernmentInfo.GovernmentType.Mercantilism;
							break;
					}
					break;
				default:
					switch (num5)
					{
						case 0:
							government.CurrentType = GovernmentInfo.GovernmentType.Communalism;
							break;
						case 1:
							government.CurrentType = GovernmentInfo.GovernmentType.Junta;
							break;
						default:
							government.CurrentType = GovernmentInfo.GovernmentType.Plutocracy;
							break;
					}
					break;
			}
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateGovernmentInfo, (object)government.PlayerID, (object)government.Authoritarianism, (object)government.EconomicLiberalism, (object)government.CurrentType), true);
		}

		public IEnumerable<int> GetStarSystemIDs()
		{
			this.CacheStarSystemInfos();
			return (IEnumerable<int>)this._dom.star_systems.Keys;
		}

		public int? GetStarSystemProvinceID(int systemId)
		{
			this.CacheStarSystemInfos();
			StarSystemInfo starSystemInfo;
			if (!this._dom.star_systems.TryGetValue(systemId, out starSystemInfo))
				return new int?();
			return starSystemInfo.ProvinceID;
		}

		public Vector3 GetStarSystemOrigin(int systemId)
		{
			return Vector3.Parse(this.db.ExecuteStringQuery(string.Format(Queries.GetStarSystemOrigin, (object)systemId)));
		}

		public void UpdateStarSystemOrigin(int systemId, Vector3 origin)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateStarSystemOrigin, (object)systemId, (object)origin.ToSQLiteValue()), false, true);
		}

		public IEnumerable<int> GetStarSystemOrbitalObjectIDs(int systemId)
		{
			return (IEnumerable<int>)this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetStarSystemOrbitalObjectsBySystemId, (object)systemId));
		}

		public static OrbitalObjectInfo GetOrbitalObjectInfo(
		  SQLiteConnection db,
		  int orbitalObjectID)
		{
			Kerberos.Sots.Data.SQLite.Table table = db.ExecuteTableQuery(string.Format(Queries.GetOrbitalObjectInfo, (object)orbitalObjectID), true);
			if (table.Rows.Length == 0)
				return (OrbitalObjectInfo)null;
			Row row = table[0];
			return new OrbitalObjectInfo()
			{
				ID = orbitalObjectID,
				ParentID = row[0].SQLiteValueToNullableInteger(),
				StarSystemID = row[1].SQLiteValueToInteger(),
				OrbitalPath = OrbitalPath.Parse(row[2].SQLiteValueToString()),
				Name = row[3].SQLiteValueToString()
			};
		}

		public OrbitalObjectInfo GetOrbitalObjectInfo(int orbitalObjectID)
		{
			return GameDatabase.GetOrbitalObjectInfo(this.db, orbitalObjectID);
		}

		public void RemoveStarSystem(int systemId)
		{
			foreach (int key in this._dom.missions.Values.Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.StartingSystem == systemId)).Select<MissionInfo, int>((Func<MissionInfo, int>)(y => y.ID)).ToList<int>())
				this._dom.missions.Sync(key);
			foreach (int trapID in this._dom.colony_traps.Values.Where<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.SystemID == systemId)).Select<ColonyTrapInfo, int>((Func<ColonyTrapInfo, int>)(y => y.ID)).ToList<int>())
				this.RemoveColonyTrapInfo(trapID);
			foreach (NodeLineInfo nodeLineInfo in this.GetNodeLines().ToList<NodeLineInfo>())
			{
				if (nodeLineInfo.System1ID == systemId || nodeLineInfo.System2ID == systemId)
				{
					foreach (int fleetID in this.GetFleetsForLoaLine(nodeLineInfo.ID).ToList<int>())
						this.RemoveFleet(fleetID);
					this.RemoveNodeLine(nodeLineInfo.ID);
				}
			}
			this._dom.star_systems.Clear();
			this._dom.fleets.Clear();
			this._dom.ships.Clear();
			this._dom.players.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveStarSystem, (object)systemId), false, true);
		}

		public void DestroyStarSystem(GameSession game, int systemId)
		{
			StarSystemInfo ssi = this.GetStarSystemInfo(systemId);
			foreach (OrbitalObjectInfo orbitalObjectInfo in this.GetStarSystemOrbitalObjectInfos(systemId).OrderBy<OrbitalObjectInfo, int>((Func<OrbitalObjectInfo, int>)(x => !x.ParentID.HasValue ? 1 : 0)).ToList<OrbitalObjectInfo>())
				this.DestroyOrbitalObject(game, orbitalObjectInfo.ID);
			foreach (FreighterInfo freighterInfo in this.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>())
				this.RemoveFreighterInfo(freighterInfo.ID);
			foreach (FleetInfo fleet in this.GetFleetInfos(FleetType.FL_ALL).ToList<FleetInfo>())
			{
				if (fleet.SupportingSystemID == systemId)
				{
					if (!fleet.IsNormalFleet)
						this.RemoveFleet(fleet.ID);
					List<int> list1 = this.GetPlayerColonySystemIDs(fleet.PlayerID).ToList<int>();
					list1.Remove(systemId);
					List<int> list2 = list1.OrderBy<int, float>((Func<int, float>)(x => (this.GetStarSystemOrigin(x) - ssi.Origin).Length)).ToList<int>();
					if (list2.Count == 0)
					{
						this.RemoveFleet(fleet.ID);
					}
					else
					{
						fleet.SupportingSystemID = list2.First<int>();
						this.UpdateFleetInfo(fleet);
					}
				}
			}
			List<FleetInfo> list3 = this.GetFleetInfos(FleetType.FL_ALL).ToList<FleetInfo>();
			foreach (MoveOrderInfo moveOrderInfo in this.GetMoveOrderInfos().ToList<MoveOrderInfo>())
			{
				if (moveOrderInfo.ToSystemID == systemId)
				{
					this.RemoveMoveOrder(moveOrderInfo.ID);
					this.InsertMoveOrder(moveOrderInfo.FleetID, 0, this.GetFleetLocation(moveOrderInfo.FleetID, false).Coords, moveOrderInfo.FromSystemID, Vector3.Zero);
				}
				else if (moveOrderInfo.FromSystemID == systemId)
				{
					this.InsertMoveOrder(moveOrderInfo.FleetID, 0, this.GetFleetLocation(moveOrderInfo.FleetID, false).Coords, moveOrderInfo.ToSystemID, Vector3.Zero);
					this.RemoveMoveOrder(moveOrderInfo.ID);
				}
			}
			foreach (MissionInfo missionInfo in this.GetMissionInfos().ToList<MissionInfo>())
			{
				if (missionInfo.TargetSystemID == systemId)
				{
					MissionInfo missionByFleetId = this.GetMissionByFleetID(missionInfo.FleetID);
					if (missionByFleetId != null && missionByFleetId.ID != missionInfo.ID)
					{
						this.RemoveMission(missionInfo.ID);
						continue;
					}
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, this.GetFleetInfo(missionInfo.FleetID), true);
				}
				foreach (WaypointInfo waypointInfo in this.GetWaypointsByMissionID(missionInfo.ID).ToList<WaypointInfo>())
				{
					int? systemId1 = waypointInfo.SystemID;
					int num = systemId;
					if ((systemId1.GetValueOrDefault() != num ? 0 : (systemId1.HasValue ? 1 : 0)) != 0)
						Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, this.GetFleetInfo(missionInfo.FleetID), true);
				}
			}
			foreach (FleetInfo fleetInfo in list3)
			{
				if (fleetInfo.SystemID == systemId)
				{
					PlayerInfo playerInfo = this.GetPlayerInfo(fleetInfo.PlayerID);
					if (!fleetInfo.IsNormalFleet || playerInfo == null || !playerInfo.isStandardPlayer)
					{
						this.RemoveFleet(fleetInfo.ID);
					}
					else
					{
						List<StarSystemInfo> list1 = this.GetStarSystemInfos().ToList<StarSystemInfo>();
						list1.RemoveAll((Predicate<StarSystemInfo>)(x => x.ID == systemId));
						List<StarSystemInfo> list2 = list1.OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(x => (x.Origin - ssi.Origin).Length)).ToList<StarSystemInfo>();
						Kerberos.Sots.StarFleet.StarFleet.ForceReturnMission(this, fleetInfo);
						this.InsertMoveOrder(fleetInfo.ID, 0, this.GetFleetLocation(fleetInfo.ID, false).Coords, list2.First<StarSystemInfo>().ID, Vector3.Zero);
						this.UpdateFleetLocation(fleetInfo.ID, 0, new int?());
						this.UpdateFleetInfo(fleetInfo);
					}
				}
			}
			foreach (RetrofitOrderInfo retrofitOrderInfo in this.GetRetrofitOrdersForSystem(systemId).ToList<RetrofitOrderInfo>())
				this.RemoveRetrofitOrder(retrofitOrderInfo.ID, true, false);
			foreach (FleetInfo fleetInfo in this.GetFleetInfos(FleetType.FL_LIMBOFLEET | FleetType.FL_TRAP).ToList<FleetInfo>())
			{
				if (fleetInfo.SystemID == systemId)
				{
					if (fleetInfo.Type == FleetType.FL_TRAP)
					{
						ColonyTrapInfo trapInfoByFleetId = this.GetColonyTrapInfoByFleetID(fleetInfo.ID);
						if (trapInfoByFleetId != null)
							this.RemoveColonyTrapInfo(trapInfoByFleetId.ID);
					}
					this.RemoveFleet(fleetInfo.ID);
				}
			}
			if (ssi.ProvinceID.HasValue)
				this.RemoveProvince(ssi.ProvinceID.Value);
			this.RemoveStarSystem(systemId);
		}

		public void RemoveOrbitalObject(int orbitalObjectID)
		{
			this.RemoveColonyOnPlanet(orbitalObjectID);
			ColonyTrapInfo trapInfoByPlanetId = this.GetColonyTrapInfoByPlanetID(orbitalObjectID);
			if (trapInfoByPlanetId != null)
				this.RemoveColonyTrapInfo(trapInfoByPlanetId.ID);
			if (this.GetOrbitalObjectInfo(orbitalObjectID) != null)
			{
				foreach (OrbitalObjectInfo orbitalObjectInfo in this.GetChildOrbitals(orbitalObjectID).ToList<OrbitalObjectInfo>())
					this.RemoveOrbitalObject(orbitalObjectInfo.ID);
			}
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveOrbitalObject, (object)orbitalObjectID), false, true);
			this._dom.players.Clear();
		}

		public void DestroyOrbitalObject(GameSession game, int orbitalObjectID)
		{
			List<StationInfo> stations = this.GetStationInfos().Where<StationInfo>((Func<StationInfo, bool>)(x =>
		   {
			   int? parentId = x.OrbitalObjectInfo.ParentID;
			   int num = orbitalObjectID;
			   if (parentId.GetValueOrDefault() == num)
				   return parentId.HasValue;
			   return false;
		   })).ToList<StationInfo>();
			foreach (StationInfo stationInfo in stations)
				this.DestroyStation(game, stationInfo.OrbitalObjectID, 0);
			ColonyInfo colony = this.GetColonyInfoForPlanet(orbitalObjectID);
			if (colony != null)
			{
				this.RemoveColonyOnPlanet(orbitalObjectID);
				HomeworldInfo hw = this.GetHomeworlds().ToList<HomeworldInfo>().FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.ColonyID == colony.ID));
				if (hw != null)
				{
					List<ColonyInfo> list = this.GetColonyInfos().ToList<ColonyInfo>().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == hw.PlayerID)).ToList<ColonyInfo>();
					list.OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => this.GetTotalPopulation(x)));
					ColonyInfo colonyInfo = list.FirstOrDefault<ColonyInfo>();
					if (colonyInfo != null)
					{
						hw.ColonyID = colonyInfo.ID;
						hw.SystemID = this.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID;
						this.UpdateHomeworldInfo(hw);
					}
				}
			}
			foreach (MissionInfo missionInfo in this.GetMissionInfos().ToList<MissionInfo>().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.TargetOrbitalObjectID != orbitalObjectID)
				   return stations.Any<StationInfo>((Func<StationInfo, bool>)(y => y.OrbitalObjectID == x.TargetOrbitalObjectID));
			   return true;
		   })).ToList<MissionInfo>())
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, this.GetFleetInfo(missionInfo.FleetID), true);
			if (this.GetOrbitalObjectInfo(orbitalObjectID) != null)
			{
				foreach (OrbitalObjectInfo orbitalObjectInfo in this.GetChildOrbitals(orbitalObjectID).ToList<OrbitalObjectInfo>())
					this.DestroyOrbitalObject(game, orbitalObjectInfo.ID);
			}
			this.RemoveOrbitalObject(orbitalObjectID);
		}

		public IEnumerable<int> GetAlliancePlayersByAllianceId(int allianceId)
		{
			return (IEnumerable<int>)this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetAlliancePlayersByAllianceId, (object)allianceId));
		}

		public int? GetSystemOwningPlayer(int systemId)
		{
			List<ColonyInfo> list1 = this.GetColonyInfosForSystem(systemId).ToList<ColonyInfo>();
			ColonyInfo colonyInfo1 = list1.FirstOrDefault<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.OwningColony));
			if (colonyInfo1 != null)
				return new int?(colonyInfo1.PlayerID);
			double num = 0.0;
			foreach (ColonyInfo colonyInfo2 in list1)
			{
				if (colonyInfo2.ImperialPop > num)
				{
					num = colonyInfo2.ImperialPop;
					colonyInfo1 = colonyInfo2;
				}
			}
			if (colonyInfo1 != null)
				return new int?(colonyInfo1.PlayerID);
			List<StationInfo> list2 = this.GetStationForSystem(systemId).ToList<StationInfo>();
			if (list2.Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.NAVAL)))
			{
				StationInfo stationInfo = list2.FirstOrDefault<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.NAVAL));
				if (stationInfo != null)
					return new int?(stationInfo.PlayerID);
			}
			return new int?();
		}

		public bool hasPermissionToBuildEnclave(int playerId, int orbitalObject)
		{
			int ssid = this.GetOrbitalObjectInfo(orbitalObject).StarSystemID;
			List<RequestInfo> list = this.GetRequestInfos().ToList<RequestInfo>();
			int? owningPlayer = this.GetSystemOwningPlayer(ssid);
			if (!owningPlayer.HasValue || owningPlayer.Value == playerId || this.GetPlayerInfo(owningPlayer.Value).includeInDiplomacy && this.GetDiplomacyStateBetweenPlayers(owningPlayer.Value, playerId) != DiplomacyState.WAR)
				return true;
			return list.FirstOrDefault<RequestInfo>((Func<RequestInfo, bool>)(x =>
		   {
			   if (x.Type == RequestType.EstablishEnclaveRequest && x.InitiatingPlayer == playerId)
			   {
				   int receivingPlayer = x.ReceivingPlayer;
				   int? nullable = owningPlayer;
				   if ((receivingPlayer != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
					   return (double)x.RequestValue == (double)ssid;
			   }
			   return false;
		   })) != null;
		}

		public IEnumerable<ColonyInfo> GetColonyInfos()
		{
			return this._dom.colonies.Values;
		}

		public ColonyInfo GetColonyInfo(int colonyID)
		{
			if (colonyID == 0 || !this._dom.colonies.ContainsKey(colonyID))
				return (ColonyInfo)null;
			return this._dom.colonies[colonyID];
		}

		public IEnumerable<int> GetPlayerColonySystemIDs(int playerid)
		{
			return this._dom.colonies.Where<KeyValuePair<int, ColonyInfo>>((Func<KeyValuePair<int, ColonyInfo>, bool>)(x => x.Value.PlayerID == playerid)).Select<KeyValuePair<int, ColonyInfo>, int>((Func<KeyValuePair<int, ColonyInfo>, int>)(y => y.Value.CachedStarSystemID)).Distinct<int>();
		}

		public IEnumerable<ColonyInfo> GetPlayerColoniesByPlayerId(int playerId)
		{
			return this._dom.colonies.Values.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerId));
		}

		public IEnumerable<ColonyInfo> GetColonyInfosForProvince(int provinceID)
		{
			foreach (int index in this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetColonyIDsForProvince, (object)provinceID.ToSQLiteValue())))
				yield return this._dom.colonies[index];
		}

		public IEnumerable<ColonyInfo> GetColonyInfosForSystem(int systemID)
		{
			return this._dom.colonies.Values.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.CachedStarSystemID == systemID));
		}

		public ColonyInfo GetColonyInfoForPlanet(int orbitalObjectID)
		{
			return this._dom.colonies.Values.FirstOrDefault<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.OrbitalObjectID == orbitalObjectID));
		}

		public string GetColonyName(int orbitalObjectID)
		{
			return this.GetOrbitalObjectInfo(orbitalObjectID).Name;
		}

		public void UpdateColony(ColonyInfo colony)
		{
			this._dom.colonies.Update(colony.ID, colony);
		}

		public void RemoveColonyOnPlanet(int planetID)
		{
			ColonyInfo colony = this.GetColonyInfoForPlanet(planetID);
			if (colony == null)
				return;
			foreach (ColonyFactionInfo faction in colony.Factions)
				this.RemoveCivilianPopulation(colony.OrbitalObjectID, faction.FactionID);
			this._dom.colonies.Remove(colony.ID);
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(colony.OrbitalObjectID);
			if (orbitalObjectInfo == null)
				return;
			int starSystemId = orbitalObjectInfo.StarSystemID;
			List<ColonyInfo> list = this.GetColonyInfosForSystem(starSystemId).ToList<ColonyInfo>();
			if (list.Count == 0)
			{
				foreach (InvoiceInstanceInfo invoiceInstanceInfo in this.GetInvoicesForSystem(colony.PlayerID, starSystemId).ToList<InvoiceInstanceInfo>())
					this.RemoveInvoiceInstance(invoiceInstanceInfo.ID);
				StarSystemInfo starSystemInfo = this.GetStarSystemInfo(starSystemId);
				if (starSystemInfo.ProvinceID.HasValue)
					this.RemoveProvince(starSystemInfo.ProvinceID.Value);
				int? reserveFleetId = this.GetReserveFleetID(colony.PlayerID, starSystemId);
				if (!reserveFleetId.HasValue)
					return;
				foreach (int num in this.GetShipsByFleetID(reserveFleetId.Value).ToList<int>())
				{
					int shipID = num;
					ShipInfo shipInfo = this.GetShipInfo(shipID, false);
					if (shipInfo != null && shipInfo.AIFleetID.HasValue && this.GetAIFleetInfos(colony.PlayerID).Any<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
				   {
					   int id = x.ID;
					   int? aiFleetId = this.GetShipInfo(shipID, false).AIFleetID;
					   int valueOrDefault = aiFleetId.GetValueOrDefault();
					   return id == valueOrDefault & aiFleetId.HasValue;
				   })))
						this.RemoveAIFleetInfo(this.GetShipInfo(shipID, false).AIFleetID.Value);
					this.RemoveShip(shipID);
				}
				this.RemoveFleet(reserveFleetId.Value);
			}
			else
			{
				if (!colony.OwningColony)
					return;
				ColonyInfo colonyInfo1 = colony;
				double num = 0.0;
				foreach (ColonyInfo colonyInfo2 in list)
				{
					if (colonyInfo2.ImperialPop > num)
					{
						num = colonyInfo2.ImperialPop;
						colony = colonyInfo2;
					}
				}
				colony.OwningColony = true;
				if (colony.PlayerID != colonyInfo1.PlayerID)
				{
					StarSystemInfo starSystemInfo = this.GetStarSystemInfo(this.GetOrbitalObjectInfo(colonyInfo1.OrbitalObjectID).StarSystemID);
					if (starSystemInfo.ProvinceID.HasValue)
						this.RemoveProvince(starSystemInfo.ProvinceID.Value);
					foreach (InvoiceInstanceInfo invoiceInstanceInfo in this.GetInvoicesForSystem(colonyInfo1.PlayerID, starSystemId).ToList<InvoiceInstanceInfo>())
						this.RemoveInvoiceInstance(invoiceInstanceInfo.ID);
				}
				this.UpdateColony(colony);
			}
		}

		public PlagueInfo ParsePlagueInfo(Row r)
		{
			return new PlagueInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				ColonyId = r[1].SQLiteValueToInteger(),
				PlagueType = (WeaponEnums.PlagueType)r[2].SQLiteValueToInteger(),
				InfectionRate = r[3].SQLiteValueToSingle(),
				InfectedPopulationImperial = r[4].SQLiteValueToDouble(),
				InfectedPopulationCivilian = r[5].SQLiteValueToDouble(),
				LaunchingPlayer = r[6].SQLiteValueToInteger()
			};
		}

		public int InsertPlagueInfo(PlagueInfo pi)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertPlague, (object)pi.ColonyId.ToSQLiteValue(), (object)((int)pi.PlagueType).ToSQLiteValue(), (object)pi.InfectionRate.ToSQLiteValue(), (object)pi.InfectedPopulationImperial.ToSQLiteValue(), (object)pi.InfectedPopulationCivilian.ToSQLiteValue(), (object)pi.LaunchingPlayer.ToSQLiteValue()));
		}

		public void UpdatePlagueInfo(PlagueInfo pi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlague, (object)pi.ID, (object)pi.ColonyId, (object)(int)pi.PlagueType, (object)pi.InfectionRate, (object)pi.InfectedPopulationImperial, (object)pi.InfectedPopulationCivilian, (object)pi.LaunchingPlayer), false, true);
		}

		public IEnumerable<PlagueInfo> GetPlagueInfoByColony(int colonyId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetPlagueInfosByColony, (object)colonyId), true);
			foreach (Row row in t.Rows)
				yield return this.ParsePlagueInfo(row);
		}

		public void RemovePlagueInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemovePlague, (object)id), false, true);
		}

		public void RemoveFeasibilityStudy(int projectId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFeasibilityStudy, (object)projectId), false, true);
		}

		public FreighterInfo ParseFreighterInfo(Row r)
		{
			return new FreighterInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				SystemId = r[1].SQLiteValueToInteger(),
				PlayerId = r[2].SQLiteValueToInteger(),
				Design = this.GetDesignInfo(r[3].SQLiteValueToInteger()),
				ShipId = r[4] != null ? r[4].SQLiteValueToInteger() : 0,
				IsPlayerBuilt = r[5] != null && r[5].SQLiteValueToBoolean()
			};
		}

		public IEnumerable<FreighterInfo> GetFreighterInfosForSystem(
		  int systemID)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetFreighterInfosForSystem, (object)systemID), true);
			foreach (Row r in t)
				yield return this.ParseFreighterInfo(r);
		}

		public IEnumerable<FreighterInfo> GetFreighterInfosBuiltByPlayer(
		  int playerID)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetFreighterInfosBuiltByPlayer, (object)playerID), true);
			foreach (Row r in t)
				yield return this.ParseFreighterInfo(r);
		}

		public void UpdateFreighterInfo(FreighterInfo fi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFreighterInfo, (object)fi.ID, (object)fi.SystemId, (object)fi.PlayerId), false, true);
		}

		public void RemoveFreighterInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFreighterInfo, (object)id), false, true);
		}

		public int InsertFreighterInfo(int SystemId, int PlayerId, ShipInfo ship, bool isPlayerBuilt)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFreighterInfo, (object)SystemId, (object)PlayerId, (object)ship.DesignID, (object)ship.ID, (object)isPlayerBuilt.ToSQLiteValue()));
		}

		public int InsertFreighter(int SystemId, int PlayerId, int DesignId, bool isPlayerBuilt)
		{
			int num = this.InsertShip(this.InsertOrGetLimboFleetID(SystemId, PlayerId), DesignId, null, (ShipParams)0, new int?(), 0);
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertFreighterInfo, (object)SystemId, (object)PlayerId, (object)DesignId, (object)num, (object)isPlayerBuilt.ToSQLiteValue()));
		}

		public IEnumerable<StationInfo> GetStationForSystemAndPlayer(
		  int systemID,
		  int playerID)
		{
			return this._dom.stations.Values.Where<StationInfo>((Func<StationInfo, bool>)(x =>
		   {
			   if (x.OrbitalObjectInfo.StarSystemID == systemID)
				   return x.PlayerID == playerID;
			   return false;
		   }));
		}

		public IEnumerable<StationInfo> GetStationForSystem(int systemID)
		{
			return this._dom.stations.Values.Where<StationInfo>((Func<StationInfo, bool>)(x => x.OrbitalObjectInfo.StarSystemID == systemID));
		}

		public IEnumerable<StationInfo> GetStationInfos()
		{
			return this._dom.stations.Values;
		}

		public IEnumerable<StationInfo> GetStationInfosByPlayerID(int playerID)
		{
			return this._dom.stations.Values.Where<StationInfo>((Func<StationInfo, bool>)(x => x.PlayerID == playerID));
		}

		public bool GetStationRequiresSupport(StationType type)
		{
			if (type != StationType.CIVILIAN && type != StationType.DIPLOMATIC && type != StationType.NAVAL)
				return type == StationType.SCIENCE;
			return true;
		}

		public int GetNumberMaxStationsSupportedBySystem(GameSession game, int systemID, int playerID)
		{
			if (this.IsSurveyed(playerID, systemID))
			{
				if (this.GetSystemOwningPlayer(systemID).HasValue)
				{
					int? systemOwningPlayer = this.GetSystemOwningPlayer(systemID);
					int num = playerID;
					if ((systemOwningPlayer.GetValueOrDefault() != num ? 0 : (systemOwningPlayer.HasValue ? 1 : 0)) == 0 && !StarMapState.SystemHasIndependentColony(game, systemID))
						goto label_11;
				}
				if (!((IEnumerable<PlanetInfo>)this.GetStarSystemPlanetInfos(systemID)).Any<PlanetInfo>())
					return 0;
				List<ColonyInfo> list = this.GetColonyInfosForSystem(systemID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerID)).ToList<ColonyInfo>();
				double num1 = 0.0;
				foreach (ColonyInfo ci in list)
					num1 += this.GetTotalPopulation(ci);
				return Math.Min(Math.Max((int)Math.Ceiling(num1 / (double)game.AssetDatabase.StationsPerPop), 1), 4);
			}
		label_11:
			return 0;
		}

		public StationInfo GetStationForSystemPlayerAndType(
		  int systemID,
		  int playerID,
		  StationType type)
		{
			return this._dom.stations.Values.FirstOrDefault<StationInfo>((Func<StationInfo, bool>)(x =>
		   {
			   if (x.OrbitalObjectInfo.StarSystemID == systemID && x.PlayerID == playerID)
				   return x.DesignInfo.StationType == type;
			   return false;
		   }));
		}

		public StationInfo GetNavalStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.NAVAL);
		}

		public StationInfo GetScienceStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.SCIENCE);
		}

		public StationInfo GetCivilianStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.CIVILIAN);
		}

		public StationInfo GetDiplomaticStationForSystemAndPlayer(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.DIPLOMATIC);
		}

		public StationInfo GetHiverGateForSystem(int systemID, int playerID)
		{
			return this.GetStationForSystemPlayerAndType(systemID, playerID, StationType.GATE);
		}

		public void RemoveStation(int stationID)
		{
			this._dom.stations.Remove(stationID);
			this._dom.CachedSystemHasGateFlags.Clear();
		}

		public void DestroyStation(GameSession game, int stationID, int ignoreMissionID = 0)
		{
			foreach (MissionInfo missionInfo in game.GameDatabase.GetMissionInfos().Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.ID != ignoreMissionID)
				   return x.TargetOrbitalObjectID == stationID;
			   return false;
		   })).ToList<MissionInfo>())
				Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, game.GameDatabase.GetFleetInfo(missionInfo.FleetID), false);
			StationInfo stationInfo = game.GameDatabase.GetStationInfo(stationID);
			if (stationInfo != null && stationInfo.DesignInfo.StationType == StationType.CIVILIAN)
			{
				OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(stationID);
				if (orbitalObjectInfo != null && !game.GameDatabase.GetStationForSystemAndPlayer(orbitalObjectInfo.StarSystemID, stationInfo.DesignInfo.PlayerID).Any<StationInfo>((Func<StationInfo, bool>)(x =>
			   {
				   if (x.ID != stationID)
					   return x.DesignInfo.StationType == StationType.CIVILIAN;
				   return false;
			   })))
				{
					foreach (FreighterInfo freighterInfo in game.GameDatabase.GetFreighterInfosForSystem(orbitalObjectInfo.StarSystemID).ToList<FreighterInfo>())
						game.GameDatabase.RemoveFreighterInfo(freighterInfo.ID);
				}
			}
			this.RemoveStation(stationID);
			this.RemoveOrbitalObject(stationID);
		}

		private static AdmiralInfo GetAdmiralInfo(Row row)
		{
			return new AdmiralInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToOneBasedIndex(),
				HomeworldID = row[2].SQLiteValueToNullableInteger(),
				Name = row[3].SQLiteValueToString(),
				Race = row[4].SQLiteValueToString(),
				Age = row[5].SQLiteValueToSingle(),
				Gender = row[6].SQLiteValueToString(),
				ReactionBonus = row[7].SQLiteValueToInteger(),
				EvasionBonus = row[8].SQLiteValueToInteger(),
				Loyalty = row[9].SQLiteValueToInteger(),
				BattlesFought = row[10].SQLiteValueToInteger(),
				BattlesWon = row[11].SQLiteValueToInteger(),
				MissionsAssigned = row[12].SQLiteValueToInteger(),
				MissionsAccomplished = row[13].SQLiteValueToInteger(),
				TurnCreated = row[14].SQLiteValueToInteger(),
				Engram = row[15].SQLiteValueToBoolean()
			};
		}

		public AdmiralInfo GetAdmiralInfo(int admiralID)
		{
			if (admiralID == 0)
				return (AdmiralInfo)null;
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetAdmiral, (object)admiralID), true);
			if (source.Count<Row>() <= 0)
				return (AdmiralInfo)null;
			return GameDatabase.GetAdmiralInfo(source[0]);
		}

		public IEnumerable<AdmiralInfo> GetAdmiralInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetAdmirals), true))
				yield return GameDatabase.GetAdmiralInfo(row);
		}

		public IEnumerable<AdmiralInfo> GetAdmiralInfosForPlayer(int playerID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetAdmiralsForPlayer, (object)playerID), true))
				yield return GameDatabase.GetAdmiralInfo(row);
		}

		public void UpdateAdmiralInfo(AdmiralInfo admiral)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAdmiralInfo, (object)admiral.ID, (object)admiral.PlayerID, (object)admiral.Age, (object)admiral.ReactionBonus, (object)admiral.EvasionBonus, (object)admiral.Loyalty, (object)admiral.BattlesFought, (object)admiral.BattlesWon, (object)admiral.MissionsAssigned, (object)admiral.MissionsAccomplished), false, true);
		}

		public void UpdateEngram(int id, bool engram)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAdmiralIsEngram, (object)id.ToSQLiteValue(), (object)engram.ToSQLiteValue()), false, true);
		}

		public void RemoveAdmiral(int admiralID)
		{
			this._dom.fleets.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveAdmiral, (object)admiralID), false, true);
		}

		public void AddAdmiralTrait(int admiralID, AdmiralInfo.TraitType trait, int level)
		{
			if (this.GetAdmiralInfo(admiralID).Engram)
				return;
			if (this.GetLevelForAdmiralTrait(admiralID, trait) > 0)
				this.UpdateAdmiralTrait(admiralID, trait, level);
			else
				this.db.ExecuteNonQuery(string.Format(Queries.AddAdmiralTrait, (object)admiralID, (object)(int)trait, (object)level), false, true);
		}

		public void ClearAdmiralTraits(int admiralID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.ClearAdmiralTraits, (object)admiralID), false, true);
		}

		public void UpdateAdmiralTrait(int admiralID, AdmiralInfo.TraitType trait, int level)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAdmiralTrait, (object)admiralID, (object)(int)trait, (object)level), false, true);
		}

		public int GetLevelForAdmiralTrait(int admiralID, AdmiralInfo.TraitType type)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetLevelForAdmiralTrait, (object)admiralID, (object)(int)type));
		}

		public IEnumerable<AdmiralInfo.TraitType> GetAdmiralTraits(int admiralID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetAdmiralTraits, (object)admiralID), true))
				yield return (AdmiralInfo.TraitType)row[0].SQLiteValueToInteger();
		}

		public float GetPlanetHazardRating(int playerId, int planetId, bool useIntel = false)
		{
			float idealSuitability = this.GetFactionInfo(this.GetPlayerFactionID(playerId)).IdealSuitability;
			float num = (useIntel ? this.GetPlanetIntel(playerId, planetId).Suitability : this.GetPlanetInfo(planetId).Suitability) - idealSuitability;
			if (this.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerId))
				num = 0.0f;
			return Math.Abs(num);
		}

		public float GetNonAbsolutePlanetHazardRating(int playerId, int planetId, bool useIntel = false)
		{
			float idealSuitability = this.GetFactionInfo(this.GetPlayerFactionID(playerId)).IdealSuitability;
			return (useIntel ? this.GetPlanetIntel(playerId, planetId).Suitability : this.GetPlanetInfo(planetId).Suitability) - idealSuitability;
		}

		public double GetCivilianPopulation(int orbitalId, int factionId, bool splitSlaves)
		{
			return !splitSlaves ? this.GetCivilianPopulations(orbitalId).ToList<ColonyFactionInfo>().Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop)) : this.GetCivilianPopulations(orbitalId).ToList<ColonyFactionInfo>().Where<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID == factionId)).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop));
		}

		public double GetSlavePopulation(int orbitalId, int factionid)
		{
			return this.GetCivilianPopulations(orbitalId).Where<ColonyFactionInfo>((Func<ColonyFactionInfo, bool>)(x => x.FactionID != factionid)).Sum<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => x.CivilianPop));
		}

		public double GetTotalPopulation(ColonyInfo ci)
		{
			Faction faction = this.AssetDatabase.GetFaction(this.GetPlayerFactionID(ci.PlayerID));
			return this.GetCivilianPopulation(ci.OrbitalObjectID, faction.ID, faction.HasSlaves()) + ci.ImperialPop;
		}

		public IEnumerable<ColonyFactionInfo> GetCivilianPopulations(
		  int orbitalObjectID)
		{
			return ColoniesCache.GetColonyFactionInfosFromOrbitalObjectID(this.db, orbitalObjectID);
		}

		public void RemoveCivilianPopulation(int orbitalObjectID, int factionId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveCivilianPopulation, (object)orbitalObjectID, (object)factionId), false, true);
			this._dom.colonies.SyncRange((IEnumerable<int>)this._dom.colonies.Where<KeyValuePair<int, ColonyInfo>>((Func<KeyValuePair<int, ColonyInfo>, bool>)(x => x.Value.OrbitalObjectID == orbitalObjectID)).Select<KeyValuePair<int, ColonyInfo>, int>((Func<KeyValuePair<int, ColonyInfo>, int>)(y => y.Key)).ToList<int>());
		}

		public void UpdateCivilianPopulation(ColonyFactionInfo civPop)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateColonyFaction, (object)civPop.OrbitalObjectID, (object)civPop.FactionID, (object)civPop.CivilianPop, (object)civPop.Morale, (object)civPop.CivPopWeight, (object)civPop.LastMorale), false, true);
			this._dom.colonies.SyncRange((IEnumerable<int>)this._dom.colonies.Where<KeyValuePair<int, ColonyInfo>>((Func<KeyValuePair<int, ColonyInfo>, bool>)(x => x.Value.OrbitalObjectID == civPop.OrbitalObjectID)).Select<KeyValuePair<int, ColonyInfo>, int>((Func<KeyValuePair<int, ColonyInfo>, int>)(y => y.Key)).ToList<int>());
		}

		public int CountMoons(int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalId);
			IEnumerable<OrbitalObjectInfo> orbitalObjectInfos = this.GetStarSystemOrbitalObjectInfos(orbitalObjectInfo.StarSystemID);
			IEnumerable<int> planets = this.GetStarSystemPlanets(orbitalObjectInfo.StarSystemID);
			return orbitalObjectInfos.Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x =>
		   {
			   if (x.ParentID.HasValue && x.ParentID.Value == orbitalId)
				   return planets.Contains<int>(x.ID);
			   return false;
		   })).Count<OrbitalObjectInfo>();
		}

		public IEnumerable<OrbitalObjectInfo> GetMoons(int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = this.GetOrbitalObjectInfo(orbitalId);
			IEnumerable<OrbitalObjectInfo> orbitalObjectInfos = this.GetStarSystemOrbitalObjectInfos(orbitalObjectInfo.StarSystemID);
			IEnumerable<int> planets = this.GetStarSystemPlanets(orbitalObjectInfo.StarSystemID);
			return orbitalObjectInfos.Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x =>
		   {
			   if (x.ParentID.HasValue && x.ParentID.Value == orbitalId)
				   return planets.Contains<int>(x.ID);
			   return false;
		   }));
		}

		public IEnumerable<OrbitalObjectInfo> GetChildOrbitals(int orbitalId)
		{
			return this.GetStarSystemOrbitalObjectInfos(this.GetOrbitalObjectInfo(orbitalId).StarSystemID).Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x =>
		   {
			   if (x.ParentID.HasValue)
				   return x.ParentID.Value == orbitalId;
			   return false;
		   }));
		}

		public IEnumerable<int> GetStarSystemPlanets(int systemId)
		{
			return (IEnumerable<int>)this.db.ExecuteIntegerArrayQuery(string.Format("SELECT id FROM planets,orbital_objects WHERE planets.orbital_object_id == orbital_objects.id AND orbital_objects.star_system_id == {0};", (object)systemId));
		}

		public List<int> ParseCombatZoneString(string value)
		{
			if (value == null)
				return (List<int>)null;
			List<int> intList = new List<int>();
			string str1 = value.SQLiteValueToString();
			if (string.IsNullOrEmpty(str1))
				return (List<int>)null;
			string str2 = str1;
			char[] chArray = new char[1] { ',' };
			foreach (string s in str2.Split(chArray))
				intList.Add(int.Parse(s));
			return intList;
		}

		public StarSystemInfo ParseStarSystemInfo(Row row)
		{
			StarSystemInfo starSystemInfo = new StarSystemInfo();
			starSystemInfo.ID = row[0].SQLiteValueToInteger();
			starSystemInfo.ProvinceID = row[1].SQLiteValueToNullableInteger();
			starSystemInfo.Name = row[2].SQLiteValueToString();
			starSystemInfo.StellarClass = row[3].SQLiteValueToString();
			starSystemInfo.IsVisible = row[4].SQLiteValueToBoolean();
			starSystemInfo.TerrainID = row[5].SQLiteValueToNullableInteger();
			starSystemInfo.ControlZones = this.ParseCombatZoneString(row[6]);
			starSystemInfo.IsOpen = row[7].SQLiteValueToBoolean();
			starSystemInfo.Origin = row[9].SQLiteValueToVector3();
			return starSystemInfo;
		}

		private void CacheStarSystemInfos()
		{
			if (!this._dom.star_systems.IsDirty)
				return;
			this._dom.star_systems.Clear();
			foreach (Row row in this.db.ExecuteTableQuery(Queries.GetStarSystemInfos, true))
			{
				StarSystemInfo starSystemInfo = this.ParseStarSystemInfo(row);
				this._dom.star_systems[starSystemInfo.ID] = starSystemInfo;
			}
			this._dom.star_systems.IsDirty = false;
		}

		public StarSystemInfo GetStarSystemInfo(int systemId)
		{
			if (systemId == 0)
				return (StarSystemInfo)null;
			this.CacheStarSystemInfos();
			StarSystemInfo starSystemInfo;
			this._dom.star_systems.TryGetValue(systemId, out starSystemInfo);
			return starSystemInfo;
		}

		public IEnumerable<StarSystemInfo> GetStarSystemInfos()
		{
			this.CacheStarSystemInfos();
			return this._dom.star_systems.Values.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => x.StellarClass != "Deepspace"));
		}

		public IEnumerable<StarSystemInfo> GetDeepspaceStarSystemInfos()
		{
			this.CacheStarSystemInfos();
			return this._dom.star_systems.Values.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => x.StellarClass == "Deepspace"));
		}

		public IEnumerable<StarSystemInfo> GetVisibleStarSystemInfos(
		  int playerId)
		{
			foreach (StarSystemInfo starSystemInfo in this.GetStarSystemInfos())
			{
				if (!starSystemInfo.IsVisible)
				{
					ExploreRecordInfo eri = this.GetExploreRecord(starSystemInfo.ID, playerId);
					if (eri != null && eri.Visible)
						yield return starSystemInfo;
				}
				else
					yield return starSystemInfo;
			}
		}

		public bool IsStarSystemVisibleToPlayer(int playerId, int systemId)
		{
			ExploreRecordInfo exploreRecord = this.GetExploreRecord(systemId, playerId);
			if (this.GetStarSystemInfo(systemId).IsVisible && exploreRecord != null)
				return exploreRecord.Visible;
			return false;
		}

		public LargeAsteroidInfo ParseLargeAsteroidInfo(Row row)
		{
			return new LargeAsteroidInfo()
			{
				ID = int.Parse(row[0]),
				Resources = int.Parse(row[1])
			};
		}

		public LargeAsteroidInfo GetLargeAsteroidInfo(int orbitalObjectId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLargeAsteroidInfo, (object)orbitalObjectId), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseLargeAsteroidInfo(table.Rows[0]);
			return (LargeAsteroidInfo)null;
		}

		public IEnumerable<LargeAsteroidInfo> GetLargeAsteroidsInAsteroidBelt(
		  int orbitalObjectId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetLargeAsteroidInfoInAsteroidBelt, (object)orbitalObjectId), true);
			foreach (Row row in t.Rows)
				yield return this.ParseLargeAsteroidInfo(row);
		}

		public IEnumerable<AsteroidBeltInfo> GetStarSystemAsteroidBeltInfos(
		  int systemId)
		{
			foreach (Row r in this.db.ExecuteTableQuery(string.Format(Queries.GetStarSystemAsteroidBeltInfos, (object)systemId), true))
				yield return this.ParseAsteroidBeltInfo(r);
		}

		public AsteroidBeltInfo ParseAsteroidBeltInfo(Row r)
		{
			return new AsteroidBeltInfo()
			{
				ID = int.Parse(r[0]),
				RandomSeed = int.Parse(r[1])
			};
		}

		public AsteroidBeltInfo GetAsteroidBeltInfo(int objectId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAsteroidBeltInfo, (object)objectId), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseAsteroidBeltInfo(table[0]);
			return (AsteroidBeltInfo)null;
		}

		public SpecialProjectInfo ParseSpecialProjectInfo(Row r)
		{
			return new SpecialProjectInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToOneBasedIndex(),
				Name = r[2].SQLiteValueToString(),
				Progress = r[3].SQLiteValueToInteger(),
				Cost = r[4].SQLiteValueToInteger(),
				Type = (SpecialProjectType)r[5].SQLiteValueToInteger(),
				TechID = r[6].SQLiteValueToOneBasedIndex(),
				Rate = r[7].SQLiteValueToSingle(),
				EncounterID = r[8].SQLiteValueToOneBasedIndex(),
				FleetID = r[9].SQLiteValueToOneBasedIndex(),
				TargetPlayerID = r[10].SQLiteValueToOneBasedIndex()
			};
		}

		public StationInfo GetStationInfo(int orbitalObjectID)
		{
			if (!this._dom.stations.ContainsKey(orbitalObjectID))
				return (StationInfo)null;
			return this._dom.stations[orbitalObjectID];
		}

		public PlanetInfo ParsePlanetInfo(Row row)
		{
			return new PlanetInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				Type = row[1].SQLiteValueToString(),
				RingID = row[2].SQLiteValueToNullableInteger(),
				Suitability = row[3].SQLiteValueToSingle(),
				Biosphere = row[4].SQLiteValueToInteger(),
				Resources = row[5].SQLiteValueToInteger(),
				Size = row[6].SQLiteValueToSingle(),
				Infrastructure = row[7].SQLiteValueToSingle(),
				MaxResources = row[8].SQLiteValueToInteger()
			};
		}

		public PlanetInfo[] GetStarSystemPlanetInfos(int systemId)
		{
			return this.db.ExecuteTableQuery(string.Format(Queries.GetStarSystemPlanetInfos, (object)systemId), true).Select<Row, PlanetInfo>((Func<Row, PlanetInfo>)(row => this.ParsePlanetInfo(row))).ToArray<PlanetInfo>();
		}

		public IEnumerable<PlanetInfo> GetPlanetInfosOrbitingStar(int systemId)
		{
			IEnumerable<int> planets = this.GetStarSystemPlanets(systemId);
			IEnumerable<OrbitalObjectInfo> orbitals = this.GetStarSystemOrbitalObjectInfos(systemId);
			foreach (int orbitalObjectID in planets.Where<int>((Func<int, bool>)(p => !orbitals.First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(o => o.ID == p)).ParentID.HasValue)))
				yield return this.GetPlanetInfo(orbitalObjectID);
		}

		public PlanetInfo GetPlanetInfo(int orbitalObjectID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetPlanetInfo, (object)orbitalObjectID), true);
			if (source.Count<Row>() == 0)
				return (PlanetInfo)null;
			return this.ParsePlanetInfo(source[0]);
		}

		public void UpdatePlanet(PlanetInfo planet)
		{
			planet.Suitability = Math.Min(Math.Max(Constants.MinSuitability, planet.Suitability), Constants.MaxSuitability);
			planet.Infrastructure = Math.Min(Math.Max(Constants.MinInfrastructure, planet.Infrastructure), Constants.MaxInfrastructure);
			planet.Biosphere = Math.Max(planet.Biosphere, 0);
			this.db.ExecuteTableQuery(string.Format(Queries.UpdatePlanet, (object)planet.ID, (object)planet.Suitability, (object)planet.Biosphere, (object)planet.Resources, (object)planet.Infrastructure, (object)planet.Size, (object)planet.MaxResources), true);
		}

		public void UpdateOrbitalObjectInfo(OrbitalObjectInfo info)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateOrbitalObjectInfo, (object)info.ID, info.ParentID.HasValue ? (object)info.ParentID.ToString() : (object)"NULL", (object)info.StarSystemID, (object)info.OrbitalPath, (object)info.Name), false, true);
		}

		public void UpdatePlanetInfrastructure(int orbitalObjectID, float infrastructure)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePlanetInfrastructure, (object)orbitalObjectID, (object)infrastructure), false, true);
		}

		public IEnumerable<OrbitalObjectInfo> GetStarSystemOrbitalObjectInfos(
		  int systemId)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetStarSystemOrbitalObjectInfos, (object)systemId), true))
				yield return new OrbitalObjectInfo()
				{
					ID = int.Parse(row[0]),
					ParentID = row[1] != null ? new int?(int.Parse(row[1])) : new int?(),
					StarSystemID = systemId,
					OrbitalPath = OrbitalPath.Parse(row[2]),
					Name = row[3]
				};
		}

		public int GetLastTurnExploredByPlayer(int playerID, int systemID)
		{
			ExploreRecordInfo exploreRecordInfo = this._dom.explore_records.Values.FirstOrDefault<ExploreRecordInfo>((Func<ExploreRecordInfo, bool>)(x =>
		   {
			   if (x.SystemId == systemID)
				   return x.PlayerId == playerID;
			   return false;
		   }));
			if (exploreRecordInfo != null)
				return exploreRecordInfo.LastTurnExplored;
			return 0;
		}

		public void UpdateExploreRecord(ExploreRecordInfo eri)
		{
			this._dom.explore_records.Update(ExploreRecordsCache.GetRecordKey(eri), eri);
		}

		public void SetStarSystemVisible(int systemId, bool visible)
		{
			this._dom.star_systems.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.SetStarSystemVisible, (object)systemId, (object)visible), false, true);
		}

		public void InsertExploreRecord(
		  int systemId,
		  int playerId,
		  int lastExplored,
		  bool visible = true,
		  bool explored = true)
		{
			this._dom.explore_records.Insert(new int?(), new ExploreRecordInfo()
			{
				SystemId = systemId,
				PlayerId = playerId,
				LastTurnExplored = lastExplored,
				Explored = explored,
				Visible = visible
			});
		}

		public ExploreRecordInfo GetExploreRecord(int StarSystemId, int PlayerId)
		{
			return this._dom.explore_records.Values.FirstOrDefault<ExploreRecordInfo>((Func<ExploreRecordInfo, bool>)(x =>
		   {
			   if (x.SystemId == StarSystemId)
				   return x.PlayerId == PlayerId;
			   return false;
		   }));
		}

		public ProvinceInfo GetProvinceInfo(int provinceId)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetProvinceInfo, (object)provinceId), true)[0];
			return new ProvinceInfo()
			{
				ID = int.Parse(row[0]),
				PlayerID = int.Parse(row[1]),
				Name = row[2],
				CapitalSystemID = int.Parse(row[3])
			};
		}

		public void UpdateProvinceInfo(ProvinceInfo pi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateProvinceInfo, (object)pi.ID, (object)pi.PlayerID, (object)pi.Name, (object)pi.CapitalSystemID), false, true);
		}

		public TerrainInfo ParseTerrainInfo(Row row)
		{
			return new TerrainInfo()
			{
				ID = int.Parse(row[0]),
				Name = row[1],
				Origin = Vector3.Parse(row[2])
			};
		}

		public IEnumerable<TerrainInfo> GetTerrainInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(Queries.GetTerrainInfos, true))
				yield return this.ParseTerrainInfo(row);
		}

		public IEnumerable<ProvinceInfo> GetProvinceInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(Queries.GetProvinceInfos, true))
				yield return new ProvinceInfo()
				{
					ID = int.Parse(row[0]),
					PlayerID = int.Parse(row[1]),
					Name = row[2],
					CapitalSystemID = int.Parse(row[3])
				};
		}

		public int GetProvinceCapitalSystemID(int provinceId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetProvinceCapitalSystemID, (object)provinceId));
		}

		private FleetInfo GetFleetInfo(Row row)
		{
			if (row == null)
				return (FleetInfo)null;
			return new FleetInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToInteger(),
				AdmiralID = row[2].SQLiteValueToOneBasedIndex(),
				SystemID = row[3].SQLiteValueToOneBasedIndex(),
				SupportingSystemID = row[4].SQLiteValueToOneBasedIndex(),
				Name = row[5].SQLiteValueToString(),
				TurnsAway = row[6].SQLiteValueToInteger(),
				SupplyRemaining = row[7].SQLiteValueToSingle(),
				Type = (FleetType)row[8].SQLiteValueToInteger(),
				PreviousSystemID = row[9].SQLiteValueToNullableInteger(),
				Preferred = row[10].SQLiteValueToBoolean(),
				LastTurnAccelerated = row[11].SQLiteValueToInteger(),
				FleetConfigID = row[12].SQLiteValueToNullableInteger()
			};
		}

		private void TryRemoveFleetShip(int shipId, int fleetId)
		{
			List<int> intList = (List<int>)null;
			if (!this._dom.fleetShips.TryGetValue(fleetId, out intList))
				return;
			intList.Remove(shipId);
		}

		private void TryAddFleetShip(int shipId, int fleetId)
		{
			if (fleetId == 0)
				return;
			List<int> intList = (List<int>)null;
			if (!this._dom.fleetShips.TryGetValue(fleetId, out intList))
			{
				intList = new List<int>();
				this._dom.fleetShips[fleetId] = intList;
			}
			intList.Add(shipId);
		}

		private void CacheFleetInfos()
		{
			if (!this._dom.fleets.IsDirty)
				return;
			this._dom.fleets.Clear();
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetFleetInfos, (object)479.ToSQLiteValue()), true))
			{
				FleetInfo fleetInfo = this.GetFleetInfo(row);
				this._dom.fleets[fleetInfo.ID] = fleetInfo;
			}
			this._dom.fleetShips.Clear();
			foreach (ShipInfo shipInfo in this._dom.ships.Values)
				this.TryAddFleetShip(shipInfo.ID, shipInfo.FleetID);
			this._dom.fleets.IsDirty = false;
		}

		public IEnumerable<FleetInfo> GetFleetInfos(FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.Where<FleetInfo>((Func<FleetInfo, bool>)(x => (x.Type & filter) != (FleetType)0));
		}

		public FleetInfo GetFleetInfo(int fleetID)
		{
			if (fleetID == 0)
				return (FleetInfo)null;
			this.CacheFleetInfos();
			FleetInfo fleetInfo;
			this._dom.fleets.TryGetValue(fleetID, out fleetInfo);
			return fleetInfo;
		}

		public FleetInfo GetFleetInfo(int fleetID, FleetType filter)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo == null || (fleetInfo.Type & filter) == (FleetType)0)
				return (FleetInfo)null;
			return fleetInfo;
		}

		public IEnumerable<FleetInfo> GetFleetInfoBySystemID(
		  int systemID,
		  FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.SystemID == systemID)
				   return (x.Type & filter) != (FleetType)0;
			   return false;
		   }));
		}

		public FleetInfo GetFleetInfoByFleetName(string name, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (name == x.Name)
				   return (x.Type & filter) != (FleetType)0;
			   return false;
		   }));
		}

		public IEnumerable<FleetInfo> GetFleetInfosByPlayerID(
		  int playerID,
		  FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.PlayerID == playerID)
				   return (x.Type & filter) != (FleetType)0;
			   return false;
		   }));
		}

		public int GetFleetFaction(int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo == null)
				return 0;
			return this.GetPlayerInfo(fleetInfo.PlayerID).FactionID;
		}

		public FleetInfo GetFleetInfoByAdmiralID(int admiralID, FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.FirstOrDefault<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.AdmiralID == admiralID)
				   return (x.Type & filter) != (FleetType)0;
			   return false;
		   }));
		}

		public IEnumerable<FleetInfo> GetFleetsBySupportingSystem(
		  int systemId,
		  FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.SupportingSystemID == systemId)
				   return (x.Type & filter) != (FleetType)0;
			   return false;
		   }));
		}

		public IEnumerable<FleetInfo> GetFleetsByPlayerAndSystem(
		  int playerID,
		  int systemID,
		  FleetType filter = FleetType.FL_NORMAL)
		{
			this.CacheFleetInfos();
			return this._dom.fleets.Values.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.PlayerID == playerID && x.SystemID == systemID)
				   return (x.Type & filter) != (FleetType)0;
			   return false;
		   }));
		}

		public int InsertReserveFleet(int playerID, int systemID)
		{
			return this.InsertFleetCore(playerID, 0, systemID, 0, App.Localize("@FLEET_RESERVE_NAME"), FleetType.FL_RESERVE);
		}

		public int InsertDefenseFleet(int playerID, int systemID)
		{
			return this.InsertFleetCore(playerID, 0, systemID, 0, App.Localize("@FLEET_DEFENSE_NAME"), FleetType.FL_DEFENSE);
		}

		public int? GetReserveFleetID(int playerID, int systemID)
		{
			return this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_RESERVE).FirstOrDefault<FleetInfo>()?.ID;
		}

		private int InsertOrGetReserveFleetID(int systemID, int playerID)
		{
			int? nullable = this.GetReserveFleetID(playerID, systemID);
			if (!nullable.HasValue)
				nullable = new int?(this.InsertReserveFleet(playerID, systemID));
			return nullable.Value;
		}

		public int? GetDefenseFleetID(int systemID, int playerID)
		{
			return this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_DEFENSE).FirstOrDefault<FleetInfo>()?.ID;
		}

		private int InsertOrGetDefenseFleetID(int systemID, int playerID)
		{
			FleetInfo fleetInfo = this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_DEFENSE).FirstOrDefault<FleetInfo>();
			return fleetInfo != null ? fleetInfo.ID : this.InsertDefenseFleet(playerID, systemID);
		}

		public FleetInfo InsertOrGetReserveFleetInfo(int systemID, int playerID)
		{
			return this.GetFleetInfo(this.InsertOrGetReserveFleetID(systemID, playerID));
		}

		public FleetInfo InsertOrGetDefenseFleetInfo(int systemID, int playerID)
		{
			return this.GetFleetInfo(this.InsertOrGetDefenseFleetID(systemID, playerID));
		}

		public FleetInfo GetDefenseFleetInfo(int systemID, int playerID)
		{
			int? defenseFleetId = this.GetDefenseFleetID(systemID, playerID);
			if (!defenseFleetId.HasValue)
				return (FleetInfo)null;
			return this.GetFleetInfo(defenseFleetId.Value);
		}

		public List<int> GetNPCPlayersBySystem(int systemId)
		{
			List<int> intList = new List<int>();
			foreach (FleetInfo fleetInfo in this.GetFleetInfoBySystemID(systemId, FleetType.FL_ALL).ToList<FleetInfo>())
			{
				PlayerInfo playerInfo = this.GetPlayerInfo(fleetInfo.PlayerID);
				if (playerInfo != null && !playerInfo.isStandardPlayer)
					intList.Add(playerInfo.ID);
			}
			return intList;
		}

		public IEnumerable<MissionInfo> GetMissionsBySystemDest(int systemID)
		{
			return this._dom.missions.Values.Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.TargetSystemID == systemID));
		}

		public IEnumerable<MissionInfo> GetMissionsByPlanetDest(
		  int orbitalObjectID)
		{
			return this._dom.missions.Values.Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.TargetOrbitalObjectID == orbitalObjectID));
		}

		public FleetLocation GetFleetLocation(int fleetID, bool gapAroundStars = false)
		{
			FleetLocation fleetLocation = new FleetLocation();
			fleetLocation.FleetID = fleetID;
			MoveOrderInfo orderInfoByFleetId = this.GetMoveOrderInfoByFleetID(fleetID);
			if (orderInfoByFleetId != null)
			{
				Vector3 zero1 = Vector3.Zero;
				Vector3 zero2 = Vector3.Zero;
				Vector3 vector3_1 = orderInfoByFleetId.FromSystemID != 0 ? this.GetStarSystemInfo(orderInfoByFleetId.FromSystemID).Origin : orderInfoByFleetId.FromCoords;
				Vector3 vector3_2 = orderInfoByFleetId.ToSystemID != 0 ? this.GetStarSystemInfo(orderInfoByFleetId.ToSystemID).Origin : orderInfoByFleetId.ToCoords;
				Vector3 vector3_3 = vector3_2 - vector3_1;
				float length = vector3_3.Length;
				if (gapAroundStars && (double)length >= 0.00999999977648258)
				{
					float num1 = 0.1f;
					float num2 = length - num1 * 2f;
					if ((double)num2 < 0.0)
						num2 = 0.0f;
					float num3 = (num2 * orderInfoByFleetId.Progress + num1) / length;
					vector3_3 *= num3;
				}
				else
					vector3_3 *= orderInfoByFleetId.Progress;
				fleetLocation.Coords = vector3_1 + vector3_3;
				fleetLocation.SystemID = (double)orderInfoByFleetId.Progress != 0.0 ? 0 : orderInfoByFleetId.FromSystemID;
				Vector3 vector3_4 = vector3_2 - vector3_1;
				if ((double)vector3_4.Length >= 0.00999999977648258)
				{
					fleetLocation.Direction = new Vector3?(Vector3.Normalize(vector3_4));
					fleetLocation.FromSystemCoords = new Vector3?(vector3_1);
					fleetLocation.ToSystemCoords = new Vector3?(vector3_2);
				}
				else
					fleetLocation.Direction = new Vector3?();
			}
			else
			{
				FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
				if (fleetInfo != null)
				{
					fleetLocation.SystemID = fleetInfo.SystemID;
					StarSystemInfo starSystemInfo = this.GetStarSystemInfo(fleetInfo.SystemID);
					if (starSystemInfo != (StarSystemInfo)null)
						fleetLocation.Coords = starSystemInfo.Origin;
					fleetLocation.Direction = new Vector3?();
				}
			}
			return fleetLocation;
		}

		public bool FleetHasCurvatureComp(FleetInfo fi)
		{
			int curveId = this.GetTechID("DRV_Curvature_Compensator");
			foreach (ShipInfo shipInfo in this.GetShipInfoByFleetID(fi.ID, true).ToList<ShipInfo>())
			{
				if (shipInfo.ParentID == 0 && !((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x =>
			  {
				  if (!x.ShipSectionAsset.FileName.Contains("antimatter_enhanced") && !x.ShipSectionAsset.FileName.Contains("enhancedantimatter"))
					  return x.Techs.Any<int>((Func<int, bool>)(y => y == curveId));
				  return true;
			  })))
					return false;
			}
			return true;
		}

		public FleetLocation GetLiirFleetLocation(int fleetID)
		{
			FleetLocation fleetLocation = new FleetLocation();
			fleetLocation.FleetID = fleetID;
			MoveOrderInfo orderInfoByFleetId = this.GetMoveOrderInfoByFleetID(fleetID);
			if (orderInfoByFleetId != null)
			{
				Vector3 zero1 = Vector3.Zero;
				Vector3 zero2 = Vector3.Zero;
				if (orderInfoByFleetId.FromSystemID == 0)
				{
					Vector3 fromCoords = orderInfoByFleetId.FromCoords;
				}
				else
				{
					Vector3 origin1 = this.GetStarSystemInfo(orderInfoByFleetId.FromSystemID).Origin;
				}
				if (orderInfoByFleetId.ToSystemID == 0)
				{
					Vector3 toCoords = orderInfoByFleetId.ToCoords;
				}
				else
				{
					Vector3 origin2 = this.GetStarSystemInfo(orderInfoByFleetId.ToSystemID).Origin;
				}
			}
			else
			{
				FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
				fleetLocation.SystemID = fleetInfo.SystemID;
				StarSystemInfo starSystemInfo = this.GetStarSystemInfo(fleetInfo.SystemID);
				fleetLocation.Coords = starSystemInfo.Origin;
			}
			return fleetLocation;
		}

		public IEnumerable<SuulkaInfo> GetSuulkas()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkas), true))
				yield return this.GetSuulkaInfo(row);
		}

		public IEnumerable<SuulkaInfo> GetPlayerSuulkas(int? playerID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetPlayerSuulkas, (object)playerID.ToNullableSQLiteValue()), true))
				yield return this.GetSuulkaInfo(row);
		}

		public SuulkaInfo GetSuulkaByShipID(int shipID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkaByShipID, (object)shipID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
				return this.GetSuulkaInfo(source.First<Row>());
			return (SuulkaInfo)null;
		}

		public SuulkaInfo GetSuulkaByAdmiralID(int admiralID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkaByAdmiralID, (object)admiralID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
				return this.GetSuulkaInfo(source.First<Row>());
			return (SuulkaInfo)null;
		}

		public SuulkaInfo GetSuulkaByStationID(int stationID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulkaByStationID, (object)stationID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
				return this.GetSuulkaInfo(source.First<Row>());
			return (SuulkaInfo)null;
		}

		public SuulkaInfo GetSuulka(int suulkaID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSuulka, (object)suulkaID.ToSQLiteValue()), true);
			if (source.Count<Row>() > 0)
				return this.GetSuulkaInfo(source.First<Row>());
			return (SuulkaInfo)null;
		}

		public void UpdateSuulkaPlayer(int suulkaID, int playerID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateSuulkaPlayer, (object)suulkaID.ToSQLiteValue(), (object)playerID.ToSQLiteValue()), true);
		}

		public void UpdateSuulkaArrivalTurns(int suulkaID, int turns)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateSuulkaArrivalTurns, (object)suulkaID.ToSQLiteValue(), (object)turns.ToSQLiteValue()), true);
		}

		public void UpdateSuulkaStation(int suulkaID, int stationID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateSuulkaStation, (object)suulkaID.ToSQLiteValue(), (object)stationID.ToSQLiteValue()), true);
		}

		public void RemoveSuulka(int suulkaID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSuulka, (object)suulkaID.ToSQLiteValue()), false, true);
		}

		public void ReturnSuulka(GameSession game, int suulkaID)
		{
			SuulkaInfo suulka = this.GetSuulka(suulkaID);
			if (suulka == null)
				return;
			ShipInfo shipInfo1 = new ShipInfo();
			ShipInfo shipInfo2 = this.GetShipInfo(suulka.ShipID, true);
			if (shipInfo2 == null)
				return;
			FleetInfo fleetInfo = this.GetFleetInfo(shipInfo2.FleetID);
			this.RemoveSuulka(suulka.ID);
			this.RemoveShip(suulka.ShipID);
			this.RemoveAdmiral(suulka.AdmiralID);
			DesignInfo design = new DesignInfo();
			design.PlayerID = 0;
			design.Name = shipInfo2.DesignInfo.Name;
			design.DesignSections = shipInfo2.DesignInfo.DesignSections;
			this.InsertSuulka(new int?(), this.InsertShip(0, this.InsertDesignByDesignInfo(design), null, (ShipParams)0, new int?(), 0), this.InsertAdmiral(0, new int?(), design.Name, "suulka", 0.0f, "male", 100f, 100f, 0), new int?(), -1);
			if (fleetInfo == null)
				return;
			if (this.GetShipsByFleetID(fleetInfo.ID).Count<int>() == 0)
			{
				this.RemoveFleet(fleetInfo.ID);
			}
			else
			{
				MissionInfo missionByFleetId = this.GetMissionByFleetID(fleetInfo.ID);
				if (missionByFleetId != null)
				{
					Kerberos.Sots.StarFleet.StarFleet.CancelMission(game, fleetInfo, true);
					if (this.GetWaypointsByMissionID(missionByFleetId.ID).Any<WaypointInfo>((Func<WaypointInfo, bool>)(x => x.Type == WaypointType.DisbandFleet)))
						return;
					this.InsertWaypoint(missionByFleetId.ID, WaypointType.DisbandFleet, new int?());
				}
				else
				{
					this.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_FLEET_DISBANDED,
						EventMessage = TurnEventMessage.EM_FLEET_DISBANDED,
						PlayerID = fleetInfo.PlayerID,
						FleetID = fleetInfo.ID,
						SystemID = fleetInfo.SystemID,
						TurnNumber = this.GetTurnCount(),
						ShowsDialog = false
					});
					int? reserveFleetId = this.GetReserveFleetID(fleetInfo.PlayerID, fleetInfo.SystemID);
					if (reserveFleetId.HasValue)
					{
						foreach (ShipInfo shipInfo3 in this.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
						{
							this.UpdateShipAIFleetID(shipInfo3.ID, new int?());
							this.TransferShip(shipInfo3.ID, reserveFleetId.Value);
						}
					}
					this.RemoveFleet(fleetInfo.ID);
				}
			}
		}

		public int InsertSuulka(
		  int? playerID,
		  int shipID,
		  int admiralID,
		  int? stationID = null,
		  int arrivalTurns = -1)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSuulka, (object)playerID.ToNullableSQLiteValue(), (object)shipID.ToSQLiteValue(), (object)admiralID.ToSQLiteValue(), (object)stationID.ToNullableSQLiteValue(), (object)arrivalTurns.ToSQLiteValue()));
		}

		public void UpdateSectionInstance(SectionInstanceInfo section)
		{
			this._dom.section_instances.Update(section.ID, section);
		}

		public void UpdateArmorInstances(
		  int sectionInstanceId,
		  Dictionary<ArmorSide, DamagePattern> patterns)
		{
			if (!this._dom.armor_instances.ContainsKey(sectionInstanceId))
				return;
			this._dom.armor_instances.Update(sectionInstanceId, patterns);
		}

		public void UpdateWeaponInstance(WeaponInstanceInfo weapon)
		{
			this._dom.weapon_instances.Update(weapon.ID, weapon);
		}

		public void UpdateModuleInstance(ModuleInstanceInfo module)
		{
			this._dom.module_instances.Update(module.ID, module);
		}

		private void UpdateCachedFleetInfo(FleetInfo fleet)
		{
			FleetInfo fleetInfo;
			if (!this._dom.fleets.TryGetValue(fleet.ID, out fleetInfo))
				return;
			fleetInfo.CopyFrom(fleet);
		}

		private void UpdateCachedFleetPreferred(int id, bool preferred)
		{
			FleetInfo fleetInfo;
			if (!this._dom.fleets.TryGetValue(id, out fleetInfo))
				return;
			fleetInfo.Preferred = preferred;
		}

		private void RemoveCachedFleet(int fleetID)
		{
			this._dom.fleets.Remove(fleetID);
			this._dom.fleetShips.Remove(fleetID);
		}

		public void UpdateFleetInfo(FleetInfo fleet)
		{
			this._dom.CachedSystemStratSensorRanges.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFleetInfo, (object)fleet.ID, (object)fleet.AdmiralID.ToOneBasedSQLiteValue(), (object)fleet.SupportingSystemID.ToOneBasedSQLiteValue(), (object)fleet.Name, (object)fleet.TurnsAway, (object)fleet.SupplyRemaining, (object)fleet.PlayerID), false, true);
			this.UpdateCachedFleetInfo(fleet);
		}

		public void UpdateCachedFleetLocation(int fleetID, int systemID, int? prevSystemID = null)
		{
			FleetInfo fleetInfo;
			if (!this._dom.fleets.TryGetValue(fleetID, out fleetInfo))
				return;
			fleetInfo.SystemID = systemID;
			fleetInfo.PreviousSystemID = prevSystemID;
		}

		public void UpdateFleetAccelerated(GameSession game, int fleetID, int? turn = null)
		{
			int num = this.GetTurnCount();
			if (turn.HasValue)
				num = turn.Value;
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(game, fleetInfo.ID) > Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(game, fleetInfo.PlayerID))
				num = -10;
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFleetLastAccelerated, (object)fleetID, (object)num), false, true);
			fleetInfo.LastTurnAccelerated = num;
			this.UpdateCachedFleetInfo(fleetInfo);
		}

		public bool IsInAccelRange(int fleetid)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetid);
			MissionInfo missionByFleetId = this.GetMissionByFleetID(fleetInfo.ID);
			WaypointInfo waypointInfo = (WaypointInfo)null;
			if (missionByFleetId != null)
				waypointInfo = this.GetNextWaypointForMission(missionByFleetId.ID);
			int systemB = missionByFleetId.TargetSystemID;
			if (waypointInfo != null && waypointInfo.SystemID.HasValue)
				systemB = waypointInfo.SystemID.Value;
			else if (waypointInfo != null && waypointInfo.Type == WaypointType.ReturnHome)
				systemB = fleetInfo.SupportingSystemID;
			if (missionByFleetId != null && fleetInfo.SystemID == 0 && fleetInfo.PreviousSystemID.HasValue)
			{
				int? previousSystemId = fleetInfo.PreviousSystemID;
				int num = systemB;
				if ((previousSystemId.GetValueOrDefault() != num ? 1 : (!previousSystemId.HasValue ? 1 : 0)) != 0)
				{
					FleetLocation fleetLocation = this.GetFleetLocation(fleetid, false);
					NodeLineInfo lineBetweenSystems = this.GetNodeLineBetweenSystems(fleetInfo.PlayerID, fleetInfo.PreviousSystemID.Value, systemB, false, true);
					if (lineBetweenSystems != null)
					{
						foreach (int fleetID in this.GetFleetsForLoaLine(lineBetweenSystems.ID).ToList<int>())
						{
							this.GetFleetInfo(fleetID);
							if ((double)(this.GetFleetLocation(fleetID, false).Coords - fleetLocation.Coords).Length <= 4.0)
								return true;
						}
					}
				}
			}
			return false;
		}

		public void UpdateFleetCompositionID(int fleetID, int? CompositionID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateFleetCompositionID, (object)fleetID.ToSQLiteValue(), (object)CompositionID.ToNullableSQLiteValue()), false, true);
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			fleetInfo.FleetConfigID = CompositionID;
			this.UpdateCachedFleetInfo(fleetInfo);
		}

		public void SaveCurrentFleetCompositionToFleet(int fleetid)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetid);
			List<ShipInfo> list = this.GetShipInfoByFleetID(fleetid, false).ToList<ShipInfo>();
			if (!list.Any<ShipInfo>())
				return;
			int num = this.InsertLoaFleetComposition(fleetInfo.PlayerID, fleetInfo.Name, list.Select<ShipInfo, int>((Func<ShipInfo, int>)(x => x.DesignID)));
			this.UpdateFleetCompositionID(fleetid, new int?(num));
		}

		public void UpdateFleetLocation(int fleetID, int systemID, int? prevSystemID = null)
		{
			int? nullable = prevSystemID;
			if ((nullable.GetValueOrDefault() != 0 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
				prevSystemID = new int?();
			this._dom.CachedSystemStratSensorRanges.Clear();
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateFleetLocation, (object)fleetID.ToSQLiteValue(), (object)systemID.ToOneBasedSQLiteValue(), (object)prevSystemID.ToNullableSQLiteValue()), true);
			this.UpdateCachedFleetLocation(fleetID, systemID, prevSystemID);
		}

		public void UpdateFleetPreferred(int fleetID, bool preferred)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateFleetPreferred, (object)fleetID.ToSQLiteValue(), (object)preferred.ToSQLiteValue()), true);
			this.UpdateCachedFleetPreferred(fleetID, preferred);
		}

		public void RemoveFleet(int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			MissionInfo missionByFleetId;
			do
			{
				missionByFleetId = this.GetMissionByFleetID(fleetID);
				if (missionByFleetId != null)
					this.RemoveMission(missionByFleetId.ID);
			}
			while (missionByFleetId != null);
			if (fleetInfo != null && fleetInfo.IsGateFleet)
				this._dom.CachedSystemHasGateFlags.Clear();
			this._dom.CachedSystemStratSensorRanges.Clear();
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFleet, (object)fleetID), false, true);
			foreach (int key in this._dom.ships.Values.Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.FleetID == fleetID)).Select<ShipInfo, int>((Func<ShipInfo, int>)(y => y.ID)).ToList<int>())
				this._dom.ships.Remove(key);
			foreach (int key in this._dom.missions.Values.Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.TargetFleetID == fleetID)).Select<MissionInfo, int>((Func<MissionInfo, int>)(y => y.ID)).ToList<int>())
				this._dom.missions.Sync(key);
			this.RemoveCachedFleet(fleetID);
		}

		public MoveOrderInfo ParseMoveOrder(Row row)
		{
			return new MoveOrderInfo()
			{
				ID = int.Parse(row[0]),
				FleetID = int.Parse(row[1]),
				FromSystemID = string.IsNullOrEmpty(row[2]) ? 0 : int.Parse(row[2]),
				FromCoords = string.IsNullOrEmpty(row[3]) ? Vector3.Zero : Vector3.Parse(row[3]),
				ToSystemID = string.IsNullOrEmpty(row[4]) ? 0 : int.Parse(row[4]),
				ToCoords = string.IsNullOrEmpty(row[5]) ? Vector3.Zero : Vector3.Parse(row[5]),
				Progress = float.Parse(row[6])
			};
		}

		public IEnumerable<MoveOrderInfo> GetMoveOrderInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetMoveOrderInfos), true))
				yield return this.ParseMoveOrder(row);
		}

		public MoveOrderInfo GetMoveOrderInfoByFleetID(int fleetID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetMoveOrderInfoByFleetID, (object)fleetID), true);
			if (source.Count<Row>() == 0)
				return (MoveOrderInfo)null;
			return this.ParseMoveOrder(source[0]);
		}

		public IEnumerable<MoveOrderInfo> GetMoveOrdersByDestinationSystem(
		  int systemID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetMoveOrdersByDestinationSystem, (object)systemID), true))
				yield return this.ParseMoveOrder(row);
		}

		public void UpdateMoveOrder(int moveID, float progress)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateMoveOrder, (object)moveID, (object)progress), true);
		}

		public IEnumerable<MoveOrderInfo> GetTempMoveOrders()
		{
			if (this.TempMoveOrders == null)
				this.TempMoveOrders = new List<MoveOrderInfo>();
			return (IEnumerable<MoveOrderInfo>)this.TempMoveOrders;
		}

		public void ClearTempMoveOrders()
		{
			if (this.TempMoveOrders == null)
				return;
			this.TempMoveOrders.Clear();
		}

		public void RemoveMoveOrder(int moveID)
		{
			if (this.TempMoveOrders == null)
				this.TempMoveOrders = new List<MoveOrderInfo>();
			this.TempMoveOrders.Add(this.GetMoveOrderInfos().FirstOrDefault<MoveOrderInfo>((Func<MoveOrderInfo, bool>)(x => x.ID == moveID)));
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveMoveOrder, (object)moveID), true);
		}

		public void RemoveTurnEvent(int turnEventID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveTurnEvent, (object)turnEventID), true);
		}

		public TurnEvent ParseTurnEvent(Row row)
		{
			return new TurnEvent()
			{
				ID = row[0].SQLiteValueToInteger(),
				EventType = (TurnEventType)row[1].SQLiteValueToInteger(),
				EventMessage = (TurnEventMessage)row[2].SQLiteValueToInteger(),
				EventDesc = row[3].SQLiteValueToString(),
				PlayerID = row[4].SQLiteValueToOneBasedIndex(),
				SystemID = row[5].SQLiteValueToOneBasedIndex(),
				OrbitalID = row[6].SQLiteValueToOneBasedIndex(),
				ColonyID = row[7].SQLiteValueToOneBasedIndex(),
				FleetID = row[8].SQLiteValueToOneBasedIndex(),
				TechID = row[9].SQLiteValueToOneBasedIndex(),
				MissionID = row[10].SQLiteValueToOneBasedIndex(),
				DesignID = row[11].SQLiteValueToOneBasedIndex(),
				FeasibilityPercent = row[12].SQLiteValueToInteger(),
				TurnNumber = row[13].SQLiteValueToInteger(),
				ShowsDialog = row[14].SQLiteValueToBoolean(),
				AdmiralID = row[15].SQLiteValueToOneBasedIndex(),
				TreatyID = row[16].SQLiteValueToOneBasedIndex(),
				SpecialProjectID = row[17].SQLiteValueToOneBasedIndex(),
				SystemID2 = row[18].SQLiteValueToOneBasedIndex(),
				PlagueType = (WeaponEnums.PlagueType)row[19].SQLiteValueToOneBasedIndex(),
				ImperialPop = row[20] == null ? 0.0f : row[20].SQLiteValueToSingle(),
				CivilianPop = row[21] == null ? 0.0f : row[21].SQLiteValueToSingle(),
				Infrastructure = row[22] == null ? 0.0f : row[22].SQLiteValueToSingle(),
				CombatID = row[23].SQLiteValueToOneBasedIndex(),
				TargetPlayerID = row[24].SQLiteValueToOneBasedIndex(),
				ShipID = row[25].SQLiteValueToOneBasedIndex(),
				ProvinceID = row[26].SQLiteValueToOneBasedIndex(),
				DesignAttribute = (SectionEnumerations.DesignAttribute)row[27].SQLiteValueToOneBasedIndex(),
				ArrivalTurns = row[28].SQLiteValueToOneBasedIndex(),
				NamesList = row[29] == null ? "" : row[29].SQLiteValueToString(),
				NumShips = row[30].SQLiteValueToOneBasedIndex(),
				Savings = row[31] == null ? 0.0 : row[31].SQLiteValueToDouble(),
				EventSoundCueName = row[32] == null ? "" : row[32].SQLiteValueToString(),
				Param1 = row[33] == null ? "" : row[33].SQLiteValueToString(),
				dialogShown = row[34].SQLiteValueToBoolean(),
				EventName = row[35].SQLiteValueToString()
			};
		}

		public IEnumerable<TurnEvent> GetTurnEventsByPlayerID(int playerID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetTurnEventsByPlayerID, (object)playerID), true))
				yield return this.ParseTurnEvent(row);
		}

		public IEnumerable<TurnEvent> GetTurnEventsByTurnNumber(
		  int turnnumber,
		  int playerID)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetTurnEventsByTurnNumber, (object)turnnumber, (object)playerID), true);
			foreach (Row row in table.Rows)
				yield return this.ParseTurnEvent(row);
		}

		public bool GetTurnHasEventType(int playerID, int turnnumber, TurnEventType type)
		{
			return ((IEnumerable<Row>)this.db.ExecuteTableQuery(string.Format(Queries.GetTurnHasEventType, (object)playerID, (object)turnnumber, (object)(int)type), true).Rows).Count<Row>() > 0;
		}

		public void UpdateTurnEventDialogShown(int EventID, bool shown)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateTurnEventDialogShown, (object)EventID.ToSQLiteValue(), (object)shown.ToSQLiteValue()), false, true);
		}

		public void UpdateTurnEventSoundQue(int EventID, string queName)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateTurnEventSoundQue, (object)EventID.ToSQLiteValue(), (object)queName.ToSQLiteValue()), false, true);
		}

		public void InsertTurnEvent(TurnEvent ev)
		{
			if (string.IsNullOrEmpty(ev.EventDesc))
				ev.RebuildEventDesc(this);
			if (string.IsNullOrEmpty(ev.EventName))
				ev.RebuildEventName(this);
			this.db.ExecuteNonQuery(string.Format(Queries.InsertTurnEvent, (object)((int)ev.EventType).ToSQLiteValue(), (object)((int)ev.EventMessage).ToSQLiteValue(), (object)ev.EventDesc.ToSQLiteValue(), (object)ev.PlayerID.ToOneBasedSQLiteValue(), (object)ev.SystemID.ToOneBasedSQLiteValue(), (object)ev.OrbitalID.ToOneBasedSQLiteValue(), (object)ev.ColonyID.ToOneBasedSQLiteValue(), (object)ev.FleetID.ToOneBasedSQLiteValue(), (object)ev.TechID.ToOneBasedSQLiteValue(), (object)ev.MissionID.ToOneBasedSQLiteValue(), (object)ev.DesignID.ToOneBasedSQLiteValue(), (object)ev.FeasibilityPercent.ToSQLiteValue(), (object)ev.TurnNumber.ToSQLiteValue(), (object)ev.ShowsDialog.ToSQLiteValue(), (object)ev.AdmiralID.ToOneBasedSQLiteValue(), (object)ev.TreatyID.ToOneBasedSQLiteValue(), (object)ev.SpecialProjectID.ToOneBasedSQLiteValue(), (object)ev.SystemID2.ToOneBasedSQLiteValue(), (object)((int)ev.PlagueType).ToSQLiteValue(), (object)ev.ImperialPop.ToSQLiteValue(), (object)ev.CivilianPop.ToSQLiteValue(), (object)ev.Infrastructure.ToSQLiteValue(), (object)ev.CombatID.ToSQLiteValue(), (object)ev.TargetPlayerID.ToOneBasedSQLiteValue(), (object)ev.ShipID.ToOneBasedSQLiteValue(), (object)ev.ProvinceID.ToOneBasedSQLiteValue(), (object)((int)ev.DesignAttribute).ToSQLiteValue(), (object)ev.ArrivalTurns.ToSQLiteValue(), (object)ev.NamesList.ToSQLiteValue(), (object)ev.NumShips.ToSQLiteValue(), (object)ev.Savings.ToSQLiteValue(), (object)ev.EventSoundCueName.ToSQLiteValue(), (object)ev.Param1.ToSQLiteValue(), (object)ev.dialogShown.ToSQLiteValue(), (object)ev.EventName.ToSQLiteValue()), false, true);
		}

		public WaypointInfo ParseWaypointInfo(Row r)
		{
			return new WaypointInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				MissionID = r[1].SQLiteValueToInteger(),
				Type = (WaypointType)r[2].SQLiteValueToInteger(),
				SystemID = r[3].SQLiteValueToNullableInteger()
			};
		}

		public IEnumerable<WaypointInfo> GetWaypointsByMissionID(int missionID)
		{
			foreach (Row r in this.db.ExecuteTableQuery(string.Format(Queries.GetWaypointsByMissionID, (object)missionID), true))
				yield return this.ParseWaypointInfo(r);
		}

		public WaypointInfo GetNextWaypointForMission(int missionID)
		{
			return this.GetWaypointsByMissionID(missionID).ToList<WaypointInfo>().FirstOrDefault<WaypointInfo>((Func<WaypointInfo, bool>)(x => x.Type != WaypointType.Intercepted));
		}

		public void RemoveWaypoint(int waypointId)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveWaypoint, (object)waypointId), true);
		}

		public IEnumerable<MissionInfo> GetPlayerMissionInfosAtSystem(
		  int playerId,
		  int systemId)
		{
			return this._dom.missions.Values.Where<MissionInfo>((Func<MissionInfo, bool>)(x =>
		   {
			   if (x.TargetSystemID == systemId)
				   return this.GetFleetInfo(x.FleetID).PlayerID == playerId;
			   return false;
		   }));
		}

		public IEnumerable<MissionInfo> GetMissionInfos()
		{
			return this._dom.missions.Values;
		}

		public MissionInfo GetMissionInfo(int missionID)
		{
			if (!this._dom.missions.ContainsKey(missionID))
				return (MissionInfo)null;
			return this._dom.missions[missionID];
		}

		public MissionInfo GetMissionByFleetID(int fleetID)
		{
			return this._dom.missions.Values.FirstOrDefault<MissionInfo>((Func<MissionInfo, bool>)(x => x.FleetID == fleetID));
		}

		public void RemoveMission(int missionID)
		{
			GameDatabase.TraceVerbose("Remove: " + missionID.ToString());
			MissionInfo missionInfo = this.GetMissionInfo(missionID);
			if (missionInfo != null && missionInfo.Type == MissionType.CONSTRUCT_STN)
			{
				StationInfo stationInfo = this.GetStationInfo(missionInfo.TargetOrbitalObjectID);
				if (stationInfo != null && stationInfo.DesignInfo.StationLevel == 0)
					this.RemoveStation(missionInfo.TargetOrbitalObjectID);
			}
			this._dom.missions.Remove(missionID);
		}

		public void UpdateMission(MissionInfo mission)
		{
			GameDatabase.TraceVerbose("UpdateMission: " + mission.ID.ToString());
			this._dom.missions.Update(mission.ID, mission);
		}

		public IEnumerable<ColonyTrapInfo> GetColonyTrapInfos()
		{
			return this._dom.colony_traps.Values;
		}

		public IEnumerable<ColonyTrapInfo> GetColonyTrapInfosAtSystem(
		  int systemId)
		{
			return this._dom.colony_traps.Values.Where<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.SystemID == systemId));
		}

		public ColonyTrapInfo GetColonyTrapInfo(int trapID)
		{
			if (!this._dom.colony_traps.ContainsKey(trapID))
				return (ColonyTrapInfo)null;
			return this._dom.colony_traps[trapID];
		}

		public ColonyTrapInfo GetColonyTrapInfoBySystemIDAndPlanetID(
		  int systemID,
		  int planetID)
		{
			return this._dom.colony_traps.Values.FirstOrDefault<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x =>
		   {
			   if (x.SystemID == systemID)
				   return x.PlanetID == planetID;
			   return false;
		   }));
		}

		public ColonyTrapInfo GetColonyTrapInfoByFleetID(int fleetID)
		{
			return this._dom.colony_traps.Values.FirstOrDefault<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.FleetID == fleetID));
		}

		public ColonyTrapInfo GetColonyTrapInfoByPlanetID(int planetID)
		{
			return this._dom.colony_traps.Values.FirstOrDefault<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.PlanetID == planetID));
		}

		public void RemoveColonyTrapInfo(int trapID)
		{
			ColonyTrapInfo colonyTrapInfo = this.GetColonyTrapInfo(trapID);
			if (colonyTrapInfo == null)
				return;
			this.RemoveFleet(colonyTrapInfo.FleetID);
			this._dom.colony_traps.Remove(trapID);
		}

		public IEnumerable<int> GetShipsByFleetID(int fleetID)
		{
			return this._dom.ships.Values.Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.FleetID == fleetID)).Select<ShipInfo, int>((Func<ShipInfo, int>)(y => y.ID));
		}

		public IEnumerable<int> GetShipsByAIFleetID(int aiFleetID)
		{
			return this._dom.ships.Values.Where<ShipInfo>((Func<ShipInfo, bool>)(x =>
		   {
			   if (x.AIFleetID.HasValue)
				   return x.AIFleetID.Value == aiFleetID;
			   return false;
		   })).Select<ShipInfo, int>((Func<ShipInfo, int>)(y => y.ID));
		}

		public Vector3? GetShipFleetPosition(int shipID)
		{
			return this._dom.ships[shipID].ShipFleetPosition;
		}

		public Matrix? GetShipSystemPosition(int shipID)
		{
			return this._dom.ships[shipID].ShipSystemPosition;
		}

		public void UpdateShipFleetPosition(int shipID, Vector3? position)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipFleetPosition, (object)shipID, (object)position.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipID);
		}

		public void UpdateShipSystemPosition(int shipID, Matrix? position)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipSystemPosition, (object)shipID, (object)position.ToNullableSQLiteValue()), false, true);
			this._dom.ships.Sync(shipID);
		}

		public void UpdateShipRiderIndex(int shipID, int index)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipRiderIndex, (object)shipID, (object)index.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipID);
		}

		public IEnumerable<ShipInfo> GetShipInfos(bool getDesignInfos = false)
		{
			return this._dom.ships.Values;
		}

		public IEnumerable<ShipInfo> GetShipInfoByFleetID(
		  int fleetID,
		  bool getDesignInfo = false)
		{
			if (this._dom.fleetShips.ContainsKey(fleetID))
			{
				foreach (ShipInfo shipInfo in this._dom.fleetShips[fleetID].Select<int, ShipInfo>((Func<int, ShipInfo>)(x => this.GetShipInfo(x, false))))
					yield return shipInfo;
			}
		}

		public ShipInfo GetShipInfo(int shipID, bool getDesign = false)
		{
			if (!this._dom.ships.ContainsKey(shipID))
				return (ShipInfo)null;
			return this._dom.ships[shipID];
		}

		private SuulkaInfo GetSuulkaInfo(Row row)
		{
			return new SuulkaInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToNullableInteger(),
				ShipID = row[2].SQLiteValueToInteger(),
				AdmiralID = row[3].SQLiteValueToOneBasedIndex(),
				StationID = row[4].SQLiteValueToNullableInteger(),
				ArrivalTurns = row[5].SQLiteValueToInteger()
			};
		}

		public SectionInstanceInfo GetShipSectionInstance(int sectionInstanceID)
		{
			return this._dom.section_instances[sectionInstanceID];
		}

		public IEnumerable<SectionInstanceInfo> GetShipSectionInstances(
		  int shipID)
		{
			return this._dom.section_instances.Values.Where<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x =>
		   {
			   int? shipId = x.ShipID;
			   int num = shipID;
			   if (shipId.GetValueOrDefault() == num)
				   return shipId.HasValue;
			   return false;
		   }));
		}

		private SDBInfo GetSDBInfo(Row row)
		{
			return new SDBInfo()
			{
				OrbitalId = row[0].SQLiteValueToInteger(),
				ShipId = row[1].SQLiteValueToInteger()
			};
		}

		public IEnumerable<SDBInfo> GetSDBInfoFromOrbital(int orbitalID)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetSDBByOrbital, (object)orbitalID), true))
				yield return this.GetSDBInfo(row);
		}

		public SDBInfo GetSDBInfoFromShip(int shipID)
		{
			Kerberos.Sots.Data.SQLite.Table source = this.db.ExecuteTableQuery(string.Format(Queries.GetSDBByShip, (object)shipID), true);
			if (((IEnumerable<Row>)source.Rows).Count<Row>() > 0)
				return this.GetSDBInfo(source.First<Row>());
			return (SDBInfo)null;
		}

		public void RemoveSDBByShipID(int shipID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSDBByShip, (object)shipID), false, true);
		}

		public void RemoveSDBbyOrbitalID(int orbitalID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveSDBByOrbital, (object)orbitalID), false, true);
		}

		public int InsertSDB(int OrbitalID, int shipID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertSDB, (object)OrbitalID, (object)shipID));
		}

		public void InsertUISliderNotchSetting(
		  int playerid,
		  UISlidertype type,
		  double slidervalue = 0.0,
		  int colonyid = 0)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertUINotchSetting, (object)playerid, (object)(int)type, (object)slidervalue.ToSQLiteValue(), (object)colonyid.ToOneBasedSQLiteValue()), false, true);
		}

		public void UpdateUISliderNotchSetting(UISliderNotchInfo notchinfo)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateUINotchSetting, (object)notchinfo.Id, (object)notchinfo.PlayerId, (object)(int)notchinfo.Type, (object)notchinfo.SliderValue.ToSQLiteValue(), (object)notchinfo.ColonyId.ToNullableSQLiteValue()), false, true);
		}

		public void DeleteUISliderNotchSetting(int playerid, UISlidertype type)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveUINotchSetting, (object)playerid, (object)(int)type), false, true);
		}

		public void DeleteUISliderNotchSettingForColony(int playerid, int ColonyId, UISlidertype type)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveUINotchSettingForColony, (object)playerid, (object)(int)type, (object)ColonyId), false, true);
		}

		private UISliderNotchInfo parseSliderNotchSetting(Row row)
		{
			if (row == null)
				return (UISliderNotchInfo)null;
			return new UISliderNotchInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				PlayerId = row[1].SQLiteValueToInteger(),
				Type = (UISlidertype)row[2].SQLiteValueToInteger(),
				SliderValue = row[3].SQLiteValueToDouble(),
				ColonyId = row[4].SQLiteValueToNullableInteger()
			};
		}

		public UISliderNotchInfo GetSliderNotchSettingInfo(
		  int playerid,
		  UISlidertype type)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetUINotchSetting, (object)playerid, (object)(int)type), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.parseSliderNotchSetting(((IEnumerable<Row>)table.Rows).First<Row>());
			return (UISliderNotchInfo)null;
		}

		public UISliderNotchInfo GetSliderNotchSettingInfoForColony(
		  int playerid,
		  int ColonyId,
		  UISlidertype type)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetUINotchSettingForColony, (object)playerid, (object)(int)type, (object)ColonyId), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.parseSliderNotchSetting(((IEnumerable<Row>)table.Rows).First<Row>());
			return (UISliderNotchInfo)null;
		}

		public Dictionary<ArmorSide, DamagePattern> GetArmorInstances(
		  int sectionInstanceId)
		{
			if (this._dom.armor_instances.ContainsKey(sectionInstanceId))
				return this._dom.armor_instances[sectionInstanceId];
			return ArmorInstancesCache.EmptyArmorInstances;
		}

		public IEnumerable<WeaponInstanceInfo> GetWeaponInstances(
		  int sectionInstanceId)
		{
			return this._dom.weapon_instances.EnumerateBySectionInstanceID(sectionInstanceId);
		}

		public IEnumerable<ModuleInstanceInfo> GetModuleInstances(
		  int sectionInstanceId)
		{
			return this._dom.module_instances.EnumerateBySectionInstanceID(sectionInstanceId);
		}

		public IEnumerable<ShipInfo> GetBattleRidersByParentID(int parentID)
		{
			return this._dom.ships.Values.Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.ParentID == parentID));
		}

		public void RemoveShipFromFleet(int shipID)
		{
		}

		public void TransferShip(int shipID, int toFleetID)
		{
			if (shipID < 0)
				return;
			ShipInfo shipInfo1 = this.GetShipInfo(shipID, false);
			FleetInfo fleetInfo1 = this.GetFleetInfo(shipInfo1.FleetID);
			FleetInfo fleetInfo2 = this.GetFleetInfo(toFleetID);
			GameDatabase.Trace(string.Format("Transferring ship({0}) into fleet({1}).", (object)shipID, (object)toFleetID));
			if (fleetInfo1 != null && fleetInfo1.PlayerID != fleetInfo2.PlayerID)
				throw new InvalidOperationException(string.Format("Oops! Someone tried transferring a ship to a different player's fleet (from fleetID={0}, to fleetID={1})", (object)shipInfo1.FleetID, (object)toFleetID));
			this.TryRemoveFleetShip(shipID, shipInfo1.FleetID);
			this.db.ExecuteTableQuery(string.Format(Queries.TransferShip, (object)shipID, (object)toFleetID), true);
			this._dom.ships.Sync(shipID);
			this.TryAddFleetShip(shipID, toFleetID);
			this.UpdateShipFleetPosition(shipID, new Vector3?());
			FleetInfo fleetInfo3 = this.GetFleetInfo(toFleetID);
			if (fleetInfo3.SystemID == fleetInfo3.SupportingSystemID)
			{
				fleetInfo3.SupplyRemaining = Kerberos.Sots.StarFleet.StarFleet.GetSupplyCapacity(this, toFleetID);
				this.UpdateFleetInfo(fleetInfo3);
			}
			foreach (ShipInfo shipInfo2 in this.GetBattleRidersByParentID(shipID).ToList<ShipInfo>())
				this.TransferShip(shipInfo2.ID, toFleetID);
		}

		private void RemoveCachedShipNameReference(int playerId, string shipName)
		{
			Dictionary<string, int> dictionary1;
			int num;
			if (!this._dom.CachedPlayerShipNames.TryGetValue(playerId, out dictionary1) || !dictionary1.TryGetValue(shipName, out num))
				return;
			if (num <= 1)
			{
				dictionary1.Remove(shipName);
			}
			else
			{
				Dictionary<string, int> dictionary2;
				string index;
				(dictionary2 = dictionary1)[index = shipName] = dictionary2[index] - 1;
			}
		}

		private void AddCachedShipNameReference(int playerId, string shipName)
		{
			Dictionary<string, int> dictionary1;
			if (!this._dom.CachedPlayerShipNames.TryGetValue(playerId, out dictionary1))
			{
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				this._dom.CachedPlayerShipNames[playerId] = dictionary2;
				dictionary2[shipName] = 1;
			}
			else if (dictionary1.ContainsKey(shipName))
			{
				Dictionary<string, int> dictionary2;
				string index;
				(dictionary2 = dictionary1)[index = shipName] = dictionary2[index] + 1;
			}
			else
				dictionary1[shipName] = 1;
		}

		public void RemoveShip(int shipID)
		{
			ShipInfo shipInfo1 = this.GetShipInfo(shipID, false);
			if (shipInfo1 == null)
				return;
			this.AddNumShipsDestroyedFromDesign(shipInfo1.DesignID, 1);
			this._dom.ships.Remove(shipID);
			this.TryRemoveFleetShip(shipID, shipInfo1.FleetID);
			this._dom.CachedSystemHasGateFlags.Clear();
			foreach (ShipInfo shipInfo2 in this.GetBattleRidersByParentID(shipID).ToList<ShipInfo>())
				this.RemoveShip(shipInfo2.ID);
			this.RemoveCachedShipNameReference(this.GetDesignInfo(shipInfo1.DesignID).PlayerID, shipInfo1.ShipName);
		}

		public void SetShipParent(int shipID, int parentID)
		{
			if (parentID != 0)
			{
				ShipInfo shipInfo1 = this.GetShipInfo(shipID, true);
				ShipInfo shipInfo2 = this.GetShipInfo(parentID, true);
				if (shipInfo1.DesignInfo.PlayerID != shipInfo2.DesignInfo.PlayerID)
					throw new ArgumentException(string.Format("Attempted to parent player {0} ship {1} ({2}) to player {3} ship {4} ({5}).", (object)shipInfo1.DesignInfo.PlayerID, (object)shipInfo1.ID, (object)shipInfo1.ShipName, (object)shipInfo2.DesignInfo.PlayerID, (object)shipInfo2.ID, (object)shipInfo2.ShipName), nameof(parentID));
			}
			this.db.ExecuteNonQuery(string.Format(Queries.SetShipParentID, (object)shipID, (object)parentID), false, true);
			this._dom.ships.Sync(shipID);
		}

		public void UpdateShipDesign(int shipId, int newdesignid, int? stationId = null)
		{
			ShipInfo shipInfo = this.GetShipInfo(shipId, false);
			DesignInfo designInfo = this.GetDesignInfo(newdesignid);
			if (shipInfo == null || designInfo == null)
				return;
			foreach (SectionInstanceInfo sectionInstanceInfo in this.GetShipSectionInstances(shipId).ToList<SectionInstanceInfo>())
				this.RemoveSectionInstance(sectionInstanceInfo.ID);
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipDesign, (object)shipId.ToSQLiteValue(), (object)newdesignid.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
			this.InsertNewShipSectionInstances(designInfo, new int?(shipInfo.ID), stationId);
		}

		public void UpdateShipParams(int shipId, ShipParams parms)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipParams, (object)shipId.ToSQLiteValue(), (object)((int)parms).ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}

		public void UpdateShipAIFleetID(int shipId, int? aiFleetID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipAIFleetID, (object)shipId.ToSQLiteValue(), (object)aiFleetID.ToNullableSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}

		public void UpdateShipObtainedSlaves(int shipId, double slaves)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipSlavesObtained, (object)shipId.ToSQLiteValue(), (object)slaves.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}

		public void UpdateShipLoaCubes(int shipId, int loacubes)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipLoaCubes, (object)shipId.ToSQLiteValue(), (object)loacubes.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}

		public void UpdateShipPsionicPower(int shipId, int power)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateShipPsionicPower, (object)shipId.ToSQLiteValue(), (object)power.ToSQLiteValue()), false, true);
			this._dom.ships.Sync(shipId);
		}

		public DesignInfo GetDesignInfo(int designID)
		{
			if (designID == 0)
				return (DesignInfo)null;
			return this._dom.designs[designID];
		}

		private HashSet<string> GetDesignNamesByPlayer(int playerId)
		{
			HashSet<string> stringSet;
			if (!this._dom.CachedPlayerDesignNames.TryGetValue(playerId, out stringSet))
			{
				stringSet = new HashSet<string>((IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase);
				foreach (string str in this.GetDesignInfosForPlayer(playerId).Select<DesignInfo, string>((Func<DesignInfo, string>)(x => x.Name)))
					stringSet.Add(str);
				this._dom.CachedPlayerDesignNames.Add(playerId, stringSet);
			}
			return stringSet;
		}

		public string ResolveNewDesignName(int playerId, string name)
		{
			string pattern1 = " [Mm][Kk]\\. [0-9]+$";
			string pattern2 = "[0-9]+$";
			string str1;
			int num;
			if (Regex.IsMatch(name, pattern1, RegexOptions.CultureInvariant))
			{
				str1 = Regex.Replace(name, pattern1, string.Empty, RegexOptions.CultureInvariant);
				num = int.Parse(Regex.Match(name, pattern2, RegexOptions.CultureInvariant).ToString());
			}
			else
			{
				str1 = name;
				num = 1;
			}
			string str2 = name;
			for (HashSet<string> designNamesByPlayer = this.GetDesignNamesByPlayer(playerId); designNamesByPlayer.Contains(str2); str2 = string.Format("{0} Mk. {1}", (object)str1, (object)num))
				++num;
			return str2;
		}

		public int GetNumShipsBuiltFromDesign(int designID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumShipsBuiltFromDesign, (object)designID));
		}

		public int GetNumShipsDestroyedFromDesign(int designID)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumShipsDestroyedFromDesign, (object)designID));
		}

		public int GetNumColonies(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumColoniesByPlayer, (object)playerId));
		}

		public int GetNumProvinces(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumProvincesByPlayer, (object)playerId));
		}

		public int GetNumShips(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetNumShipsByPlayer, (object)playerId));
		}

		public double GetNumCivilians(int playerId)
		{
			Faction faction = this.assetdb.GetFaction(this.GetPlayerInfo(playerId).FactionID);
			double num = 0.0;
			foreach (ColonyInfo colonyInfo in this.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerId)))
				num += this.GetCivilianPopulation(colonyInfo.OrbitalObjectID, faction.ID, faction.HasSlaves());
			return num;
		}

		public double GetNumImperials(int playerId)
		{
			return this.GetColonyInfos().Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerId)).Sum<ColonyInfo>((Func<ColonyInfo, double>)(x => x.ImperialPop));
		}

		public double GetEmpirePopulation(int playerId)
		{
			return 0.0 + this.GetNumCivilians(playerId) + this.GetNumImperials(playerId);
		}

		public int GetEmpireBiosphere(int playerId)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.GetEmpireBiosphere, (object)playerId));
		}

		public float GetEmpireEconomy(int playerId)
		{
			List<ColonyInfo> list = this.GetPlayerColoniesByPlayerId(playerId).ToList<ColonyInfo>();
			if (list.Count > 0)
				return list.Average<ColonyInfo>((Func<ColonyInfo, float>)(x => x.EconomyRating));
			return 0.0f;
		}

		public int? GetEmpireMorale(int playerId)
		{
			int playerFactionId = this.GetPlayerFactionID(playerId);
			List<ColonyFactionInfo> source = new List<ColonyFactionInfo>();
			foreach (ColonyInfo colonyInfo in this.GetPlayerColoniesByPlayerId(playerId).ToList<ColonyInfo>())
			{
				foreach (ColonyFactionInfo faction in colonyInfo.Factions)
				{
					if (faction.FactionID == playerFactionId)
					{
						source.Add(faction);
						break;
					}
				}
			}
			if (source.Count == 0)
				return new int?();
			return new int?((int)source.Average<ColonyFactionInfo>((Func<ColonyFactionInfo, double>)(x => (double)x.Morale)));
		}

		public IEnumerable<DesignInfo> GetDesignInfosForPlayer(int playerID)
		{
			return this._dom.designs.Values.Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.PlayerID == playerID));
		}

		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayer(int playerID)
		{
			foreach (int designID in this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetVisibleDesignIDsForPlayer, (object)playerID)))
				yield return this.GetDesignInfo(designID);
		}

		public IEnumerable<DesignInfo> GetDesignInfosForPlayer(
		  int playerID,
		  RealShipClasses shipClass,
		  bool retrieveSections = true)
		{
			return this._dom.designs.Values.Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
		   {
			   if (x.PlayerID != playerID)
				   return false;
			   RealShipClasses? realShipClass = x.GetRealShipClass();
			   RealShipClasses realShipClasses = shipClass;
			   if (realShipClass.GetValueOrDefault() == realShipClasses)
				   return realShipClass.HasValue;
			   return false;
		   }));
		}

		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayer(
		  int playerID,
		  RealShipClasses shipClass)
		{
			return this.GetVisibleDesignInfosForPlayer(playerID).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
		   {
			   RealShipClasses? realShipClass = x.GetRealShipClass();
			   RealShipClasses realShipClasses = shipClass;
			   if (realShipClass.GetValueOrDefault() == realShipClasses)
				   return realShipClass.HasValue;
			   return false;
		   }));
		}

		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayerAndRole(
		  int playerID,
		  ShipRole role,
		  bool retrieveSections = true)
		{
			return this.GetVisibleDesignInfosForPlayer(playerID).Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.Role == role));
		}

		public IEnumerable<DesignInfo> GetVisibleDesignInfosForPlayerAndRole(
		  int playerID,
		  ShipRole role,
		  WeaponRole? weaponRole)
		{
			return this.GetVisibleDesignInfosForPlayer(playerID).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
		   {
			   if (x.Role != role)
				   return false;
			   if (!weaponRole.HasValue)
				   return true;
			   WeaponRole weaponRole1 = x.WeaponRole;
			   WeaponRole? nullable = weaponRole;
			   if (weaponRole1 == nullable.GetValueOrDefault())
				   return nullable.HasValue;
			   return false;
		   }));
		}

		private void CopyBuildOrderInfo(BuildOrderInfo output, Row row)
		{
			output.ID = row[0].SQLiteValueToInteger();
			output.DesignID = row[1].SQLiteValueToInteger();
			output.Priority = row[2].SQLiteValueToInteger();
			output.SystemID = row[3].SQLiteValueToInteger();
			output.Progress = row[4].SQLiteValueToInteger();
			output.MissionID = row[5].SQLiteValueToOneBasedIndex();
			output.ShipName = row[6].SQLiteValueToString();
			output.ProductionTarget = row[7].SQLiteValueToInteger();
			output.InvoiceID = row[8].SQLiteValueToNullableInteger();
			output.AIFleetID = row[9].SQLiteValueToNullableInteger();
			output.LoaCubes = row[10].SQLiteValueToInteger();
		}

		private InvoiceBuildOrderInfo ParseInvoiceBuildOrderInfo(Row row)
		{
			return new InvoiceBuildOrderInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				DesignID = row[1].SQLiteValueToInteger(),
				ShipName = row[2].SQLiteValueToString(),
				InvoiceID = row[3].SQLiteValueToInteger(),
				LoaCubes = row[4].SQLiteValueToInteger()
			};
		}

		private BuildOrderInfo GetBuildOrderInfo(Row row)
		{
			BuildOrderInfo output = new BuildOrderInfo();
			this.CopyBuildOrderInfo(output, row);
			return output;
		}

		public BuildOrderInfo GetBuildOrderInfo(int buildOrderId)
		{
			return this.GetBuildOrderInfo(this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrderInfo, (object)buildOrderId), true)[0]);
		}

		public InvoiceInstanceInfo ParseInvoiceInstanceInfo(Row r)
		{
			return new InvoiceInstanceInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToInteger(),
				SystemID = r[2].SQLiteValueToInteger(),
				Name = r[3].SQLiteValueToString()
			};
		}

		public InvoiceInfo ParseInvoiceInfo(Row r)
		{
			return new InvoiceInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerID = r[1].SQLiteValueToInteger(),
				Name = r[2].SQLiteValueToString(),
				isFavorite = r[3].SQLiteValueToBoolean()
			};
		}

		public InvoiceInfo GetInvoiceInfo(int id, int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInfo, (object)id, (object)playerId), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseInvoiceInfo(table.Rows[0]);
			return (InvoiceInfo)null;
		}

		public IEnumerable<InvoiceInfo> GetInvoiceInfosForPlayer(int playerId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInfosForPlayer, (object)playerId), true);
			foreach (Row row in t.Rows)
				yield return this.ParseInvoiceInfo(row);
		}

		public IEnumerable<InvoiceBuildOrderInfo> GetInvoiceBuildOrders(
		  int invoiceId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceBuildOrders, (object)invoiceId), true);
			foreach (Row row in t.Rows)
				yield return this.ParseInvoiceBuildOrderInfo(row);
		}

		public IEnumerable<BuildOrderInfo> GetBuildOrdersForInvoiceInstance(
		  int invoiceInstanceId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrderForInvoice, (object)invoiceInstanceId), true);
			foreach (Row row in t.Rows)
				yield return this.GetBuildOrderInfo(row);
		}

		public InvoiceInstanceInfo GetInvoiceInstanceInfo(int id)
		{
			return this.ParseInvoiceInstanceInfo(((IEnumerable<Row>)this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInstance, (object)id), true).Rows).First<Row>());
		}

		public int InsertInvoice(string name, int playerId, bool isFavorite)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertInvoice, (object)name, (object)playerId, (object)isFavorite.ToSQLiteValue()));
		}

		public int InsertInvoiceInstance(int playerId, int systemId, string name)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertInvoiceInstance, (object)playerId, (object)systemId, (object)name));
		}

		public void RemoveFavoriteInvoice(int invoiceId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveFavoriteInvoice, (object)invoiceId), false, true);
		}

		public void RemoveInvoiceInstance(int invoiceInstanceId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveInvoiceInstance, (object)invoiceInstanceId), false, true);
		}

		public IEnumerable<InvoiceInstanceInfo> GetInvoicesForSystem(
		  int playerId,
		  int systemId)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetInvoiceInstancesBySystemAndPlayer, (object)playerId, (object)systemId), true);
			foreach (Row row in t.Rows)
				yield return this.ParseInvoiceInstanceInfo(row);
		}

		public IEnumerable<BuildOrderInfo> GetBuildOrdersForSystem(int systemId)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrdersForSystem, (object)systemId), true))
				yield return this.GetBuildOrderInfo(row);
		}

		public BuildOrderInfo GetFirstBuildOrderForSite(int buildSiteID)
		{
			int num = 0;
			BuildOrderInfo output = new BuildOrderInfo();
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetBuildOrdersForSystem, (object)buildSiteID), true))
			{
				if (num == 0 || num > int.Parse(row[2]))
				{
					this.CopyBuildOrderInfo(output, row);
					num = int.Parse(row[2]);
				}
			}
			return output;
		}

		public void UpdateBuildOrder(BuildOrderInfo buildOrder)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateBuildOrder, (object)buildOrder.ID, (object)buildOrder.ProductionTarget, (object)buildOrder.Priority, (object)buildOrder.Progress, (object)buildOrder.ShipName.ToSQLiteValue()), true);
		}

		public void RemoveBuildOrder(int buildOrderID)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.RemoveBuildOrder, (object)buildOrderID), true);
		}

		private BuildSiteInfo GetBuildSiteInfo(Row row)
		{
			return new BuildSiteInfo()
			{
				ID = int.Parse(row[0]),
				StationID = int.Parse(row[1]),
				PlanetID = int.Parse(row[2]),
				ShipID = int.Parse(row[3]),
				Resources = int.Parse(row[4])
			};
		}

		public void UpdateBuildSiteResources(int buildSiteID, int resources)
		{
			this.db.ExecuteTableQuery(string.Format(Queries.UpdateBuildSiteResources, (object)buildSiteID, (object)resources), true);
		}

		public void RemoveBuildSite(int buildSiteID)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveBuildSite, (object)buildSiteID), false, true);
		}

		private DesignModuleInfo ParseDesignModuleInfo(
		  DesignSectionInfo parent,
		  Row row)
		{
			return DesignsCache.GetDesignModuleInfoFromRow(this.db, row, parent);
		}

		public IEnumerable<DesignModuleInfo> GetQueuedStationModules(
		  DesignSectionInfo sectionInfo)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetQueuedStationModuleInfos, (object)sectionInfo.ID.ToSQLiteValue()), true);
			foreach (Row row in table.Rows)
				yield return this.ParseDesignModuleInfo(sectionInfo, row);
		}

		public void RemoveQueuedStationModule(int queuedModuleId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveQueuedStationModule, (object)queuedModuleId), false, true);
		}

		public int InsertQueuedStationModule(
		  int designSectionId,
		  int moduleId,
		  int? weaponId,
		  string mountId,
		  ModuleEnums.StationModuleType moduleType)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertQueuedStationModule.ToSQLiteValue(), (object)designSectionId.ToSQLiteValue(), (object)moduleId.ToSQLiteValue(), (object)weaponId.ToNullableSQLiteValue(), (object)mountId.ToSQLiteValue(), (object)((int)moduleType).ToSQLiteValue()));
		}

		public IEnumerable<string> GetDesignSectionNames(int designID)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetDesignSectionNames, (object)designID), true);
			foreach (Row row in table.Rows)
				yield return row.Values[0];
		}

		public IEnumerable<StellarPropInfo> GetStellarProps()
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(Queries.GetStellarProps, true);
			foreach (Row row in table.Rows)
				yield return new StellarPropInfo()
				{
					ID = int.Parse(row[0]),
					AssetPath = row[1],
					Transform = Matrix.Parse(row[2])
				};
		}

		public int GetPlayerFactionID(int playerId)
		{
			return this._dom.players.GetPlayerFactionID(playerId);
		}

		public FactionInfo GetPlayerFaction(int playerId)
		{
			return this.GetFactionInfo(this._dom.players.GetPlayerFactionID(playerId));
		}

		private ArmisticeTreatyInfo ParseArmisticeTreatyInfo(
		  Row r,
		  ArmisticeTreatyInfo ati)
		{
			if (r[13] != null)
				ati.SuggestedDiplomacyState = (DiplomacyState)r[13].SQLiteValueToInteger();
			return ati;
		}

		private LimitationTreatyInfo ParseLimitationTreatyInfo(
		  Row r,
		  LimitationTreatyInfo lti)
		{
			lti.LimitationType = (LimitationTreatyType)r[9].SQLiteValueToInteger();
			lti.LimitationAmount = r[10].SQLiteValueToSingle();
			lti.LimitationGroup = r[11].SQLiteValueToString();
			return lti;
		}

		private TreatyInfo ParseTreatyInfo(
		  Row r,
		  TreatyInfo ti,
		  List<TreatyConsequenceInfo> treatyConsequences,
		  List<TreatyIncentiveInfo> treatyIncentives)
		{
			ti.ID = r[0].SQLiteValueToInteger();
			ti.InitiatingPlayerId = r[1].SQLiteValueToInteger();
			ti.ReceivingPlayerId = r[2].SQLiteValueToInteger();
			ti.Type = (TreatyType)r[3].SQLiteValueToInteger();
			ti.Duration = r[4].SQLiteValueToInteger();
			ti.StartingTurn = r[5].SQLiteValueToInteger();
			ti.Active = r[6].SQLiteValueToBoolean();
			ti.Removed = r[7].SQLiteValueToBoolean();
			ti.Consequences.AddRange((IEnumerable<TreatyConsequenceInfo>)treatyConsequences.Where<TreatyConsequenceInfo>((Func<TreatyConsequenceInfo, bool>)(x => x.TreatyId == ti.ID)).ToList<TreatyConsequenceInfo>());
			ti.Incentives.AddRange((IEnumerable<TreatyIncentiveInfo>)treatyIncentives.Where<TreatyIncentiveInfo>((Func<TreatyIncentiveInfo, bool>)(x => x.TreatyId == ti.ID)).ToList<TreatyIncentiveInfo>());
			return ti;
		}

		private TreatyInfo ParseTreatyInfo(
		  Row r,
		  List<TreatyConsequenceInfo> treatyConsequences,
		  List<TreatyIncentiveInfo> treatyIncentives)
		{
			if (r[3].SQLiteValueToInteger() == 0)
			{
				ArmisticeTreatyInfo treatyInfo = (ArmisticeTreatyInfo)this.ParseTreatyInfo(r, (TreatyInfo)new ArmisticeTreatyInfo(), treatyConsequences, treatyIncentives);
				return (TreatyInfo)this.ParseArmisticeTreatyInfo(r, treatyInfo);
			}
			if (r[3].SQLiteValueToInteger() != 2)
				return this.ParseTreatyInfo(r, new TreatyInfo(), treatyConsequences, treatyIncentives);
			LimitationTreatyInfo treatyInfo1 = (LimitationTreatyInfo)this.ParseTreatyInfo(r, (TreatyInfo)new LimitationTreatyInfo(), treatyConsequences, treatyIncentives);
			return (TreatyInfo)this.ParseLimitationTreatyInfo(r, treatyInfo1);
		}

		public void InsertGive(GiveInfo give)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertGive, (object)give.InitiatingPlayer, (object)give.ReceivingPlayer, (object)(int)give.Type, (object)give.GiveValue), false, true);
			this.InsertDiplomacyActionHistoryEntry(give.InitiatingPlayer, give.ReceivingPlayer, this.GetTurnCount(), DiplomacyAction.GIVE, new int?((int)give.Type), new float?(), new int?(), new int?(), new float?());
			switch (give.Type)
			{
				case GiveType.GiveSavings:
					this.InsertGovernmentAction(give.InitiatingPlayer, App.Localize("@GA_GIVESAVINGS"), "GiveSavings", 0, 0);
					break;
				case GiveType.GiveResearchPoints:
					this.InsertGovernmentAction(give.InitiatingPlayer, App.Localize("@GA_GIVERESEARCHPOINTS"), "GiveResearch", 0, 0);
					break;
			}
		}

		public void InsertRequest(RequestInfo request)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertRequest, (object)request.InitiatingPlayer, (object)request.ReceivingPlayer, (object)(int)request.Type, (object)request.RequestValue, (object)(int)request.State), false, true);
			this.InsertDiplomacyActionHistoryEntry(request.InitiatingPlayer, request.ReceivingPlayer, this.GetTurnCount(), DiplomacyAction.REQUEST, new int?((int)request.Type), new float?(), new int?(), new int?(), new float?());
			switch (request.Type)
			{
				case RequestType.SavingsRequest:
					this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTSAVINGS"), "RequestSavings", 0, 0);
					break;
				case RequestType.SystemInfoRequest:
					this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTSYSTEMINFO"), "RequestSystemInfo", 0, 0);
					break;
				case RequestType.ResearchPointsRequest:
					this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTRESEARCHPOINTS"), "RequestResearch", 0, 0);
					break;
				case RequestType.MilitaryAssistanceRequest:
					this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTMILITARYASSISTANCE"), "RequestMilitaryAssistance", 0, 0);
					break;
				case RequestType.GatePermissionRequest:
					this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTGATEPERMISSION"), "RequestGatePermission", 0, 0);
					break;
				case RequestType.EstablishEnclaveRequest:
					this.InsertGovernmentAction(request.InitiatingPlayer, App.Localize("@GA_REQUESTESTABLISHENCLAVE"), "RequestEstablishEnclave", 0, 0);
					break;
			}
		}

		public void InsertDemand(DemandInfo demand)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDemand, (object)demand.InitiatingPlayer, (object)demand.ReceivingPlayer, (object)(int)demand.Type, (object)demand.DemandValue, (object)(int)demand.State), false, true);
			this.InsertDiplomacyActionHistoryEntry(demand.InitiatingPlayer, demand.ReceivingPlayer, this.GetTurnCount(), DiplomacyAction.DEMAND, new int?((int)demand.Type), new float?(), new int?(), new int?(), new float?());
			switch (demand.Type)
			{
				case DemandType.SavingsDemand:
					this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSAVINGS"), "DemandSavings", 0, 0);
					break;
				case DemandType.SystemInfoDemand:
					this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSYSTEMINFO"), "DemandSystemInfo", 0, 0);
					break;
				case DemandType.ResearchPointsDemand:
					this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDRESEARCHPOINTS"), "DemandResearch", 0, 0);
					break;
				case DemandType.SlavesDemand:
					this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSLAVES"), "DemandSlaves", 0, 0);
					break;
				case DemandType.WorldDemand:
					this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSYSTEM"), "DemandSystem", 0, 0);
					break;
				case DemandType.ProvinceDemand:
					this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDPROVINCE"), "DemandProvince", 0, 0);
					break;
				case DemandType.SurrenderDemand:
					this.InsertGovernmentAction(demand.InitiatingPlayer, App.Localize("@GA_DEMANDSURRENDER"), "DemandSurrender", 0, 0);
					break;
			}
		}

		private RequestInfo ParseRequest(Row r)
		{
			return new RequestInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				InitiatingPlayer = r[1].SQLiteValueToInteger(),
				ReceivingPlayer = r[2].SQLiteValueToInteger(),
				Type = (RequestType)r[3].SQLiteValueToInteger(),
				RequestValue = r[4].SQLiteValueToSingle(),
				State = (AgreementState)r[5].SQLiteValueToInteger()
			};
		}

		private GiveInfo ParseGive(Row r)
		{
			return new GiveInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				InitiatingPlayer = r[1].SQLiteValueToInteger(),
				ReceivingPlayer = r[2].SQLiteValueToInteger(),
				Type = (GiveType)r[3].SQLiteValueToInteger(),
				GiveValue = r[4].SQLiteValueToSingle()
			};
		}

		private DemandInfo ParseDemand(Row r)
		{
			return new DemandInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				InitiatingPlayer = r[1].SQLiteValueToInteger(),
				ReceivingPlayer = r[2].SQLiteValueToInteger(),
				Type = (DemandType)r[3].SQLiteValueToInteger(),
				DemandValue = r[4].SQLiteValueToSingle(),
				State = (AgreementState)r[5].SQLiteValueToInteger()
			};
		}

		public IEnumerable<DemandInfo> GetDemandInfos()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(Queries.GetDemandInfos, true);
			foreach (Row row in t.Rows)
				yield return this.ParseDemand(row);
		}

		public IEnumerable<RequestInfo> GetRequestInfos()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(Queries.GetRequestInfos, true);
			foreach (Row row in t.Rows)
				yield return this.ParseRequest(row);
		}

		public IEnumerable<GiveInfo> GetGiveInfos()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(Queries.GetGiveInfos, true);
			foreach (Row row in t.Rows)
				yield return this.ParseGive(row);
		}

		public void ClearGiveInfos()
		{
			this.db.ExecuteNonQuery(Queries.ClearGives, false, true);
		}

		public DemandInfo GetDemandInfo(int id)
		{
			return this.ParseDemand(this.db.ExecuteTableQuery(string.Format(Queries.GetDemandInfo, (object)id), true).Rows[0]);
		}

		public RequestInfo GetRequestInfo(int id)
		{
			return this.ParseRequest(this.db.ExecuteTableQuery(string.Format(Queries.GetRequestInfo, (object)id), true).Rows[0]);
		}

		public void SetRequestState(AgreementState state, int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetRequestState, (object)id, (object)(int)state), false, true);
		}

		public void SetDemandState(AgreementState state, int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetDemandState, (object)id, (object)(int)state), false, true);
		}

		private TreatyConsequenceInfo ParseTreatyConsequenceInfo(Row r)
		{
			return new TreatyConsequenceInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				TreatyId = r[1].SQLiteValueToInteger(),
				Type = (ConsequenceType)r[2].SQLiteValueToInteger(),
				ConsequenceValue = r[3].SQLiteValueToSingle()
			};
		}

		private TreatyIncentiveInfo ParseTreatyIncentiveInfo(Row r)
		{
			return new TreatyIncentiveInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				TreatyId = r[1].SQLiteValueToInteger(),
				Type = (IncentiveType)r[2].SQLiteValueToInteger(),
				IncentiveValue = r[3].SQLiteValueToSingle()
			};
		}

		public IEnumerable<TreatyInfo> GetTreatyInfos()
		{
			List<TreatyConsequenceInfo> treatyConsequences = new List<TreatyConsequenceInfo>();
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(Queries.GetTreatyConsequences, true);
			foreach (Row row in t.Rows)
				treatyConsequences.Add(this.ParseTreatyConsequenceInfo(row));
			List<TreatyIncentiveInfo> treatyIncentives = new List<TreatyIncentiveInfo>();
			t = this.db.ExecuteTableQuery(Queries.GetTreatyIncentives, true);
			foreach (Row row in t.Rows)
				treatyIncentives.Add(this.ParseTreatyIncentiveInfo(row));
			t = this.db.ExecuteTableQuery(Queries.GetTreatyInfos, true);
			foreach (Row row in t.Rows)
				yield return this.ParseTreatyInfo(row, treatyConsequences, treatyIncentives);
		}

		public void InsertTreaty(TreatyInfo ti)
		{
			int num = this.db.ExecuteIntegerQuery(string.Format(Queries.InsertTreatyInfo, (object)ti.InitiatingPlayerId.ToSQLiteValue(), (object)ti.ReceivingPlayerId.ToSQLiteValue(), (object)((int)ti.Type).ToSQLiteValue(), (object)ti.Duration.ToSQLiteValue(), (object)ti.StartingTurn.ToSQLiteValue(), (object)ti.Active.ToSQLiteValue(), (object)ti.Removed.ToSQLiteValue()));
			ti.ID = num;
			this.InsertDiplomacyActionHistoryEntry(ti.InitiatingPlayerId, ti.ReceivingPlayerId, this.GetTurnCount(), DiplomacyAction.TREATY, new int?((int)ti.Type), new float?(), new int?(), new int?(), new float?());
			if (ti.Type != TreatyType.Incorporate && ti.Type != TreatyType.Protectorate)
			{
				if (ti.Type == TreatyType.Trade)
					this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYTRADE"), "TreatyTrade", 0, 0);
				else if (ti.Type == TreatyType.Limitation)
				{
					LimitationTreatyInfo limitationTreatyInfo = (LimitationTreatyInfo)ti;
					this.db.ExecuteNonQuery(string.Format(Queries.InsertLimitationTreatyInfo, (object)limitationTreatyInfo.ID.ToSQLiteValue(), (object)((int)limitationTreatyInfo.LimitationType).ToSQLiteValue(), (object)limitationTreatyInfo.LimitationAmount.ToSQLiteValue(), (object)limitationTreatyInfo.LimitationGroup.ToSQLiteValue()), false, true);
					switch (limitationTreatyInfo.LimitationType)
					{
						case LimitationTreatyType.FleetSize:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYFLEETSIZELIMIT"), "TreatyFleetSizeLimit", 0, 0);
							break;
						case LimitationTreatyType.ShipClass:
							switch (limitationTreatyInfo.LimitationGroup)
							{
								case "Dreadnought":
									this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYSHIPCLASSDREAD"), "TreatyShipClassDread", 0, 0);
									break;
								case "Leviathan":
									this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYSHIPCLASSLEV"), "TreatyShipClassLev", 0, 0);
									break;
							}
							break;
						case LimitationTreatyType.EmpireSize:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYEMPIRESIZELIMIT"), "TreatyEmpireSizeLimit", 0, 0);
							break;
						case LimitationTreatyType.ForgeGemWorlds:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYDEVELOPMENTLIMIT"), "TreatyDevelopmentLimit", 0, 0);
							break;
						case LimitationTreatyType.StationType:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYSTATIONTYPE"), "TreatyStationType", 0, 0);
							break;
					}
				}
				else if (ti.Type == TreatyType.Armistice)
				{
					ArmisticeTreatyInfo armisticeTreatyInfo = (ArmisticeTreatyInfo)ti;
					this.db.ExecuteNonQuery(string.Format(Queries.InsertArmisticeTreatyInfo, (object)armisticeTreatyInfo.ID.ToSQLiteValue(), (object)((int)armisticeTreatyInfo.SuggestedDiplomacyState).ToSQLiteValue()), false, true);
					switch (armisticeTreatyInfo.SuggestedDiplomacyState)
					{
						case DiplomacyState.NON_AGGRESSION:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYNONAGGRO"), "TreatyNonAggro", 0, 0);
							break;
						case DiplomacyState.ALLIED:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYALLIED"), "TreatyAllied", 0, 0);
							break;
						case DiplomacyState.PEACE:
							this.InsertGovernmentAction(ti.InitiatingPlayerId, App.Localize("@GA_TREATYPEACE"), "TreatyPeace", 0, 0);
							break;
					}
				}
			}
			foreach (TreatyConsequenceInfo consequence in ti.Consequences)
				this.db.ExecuteNonQuery(string.Format(Queries.InsertTreatyConsequenceInfo, (object)ti.ID.ToSQLiteValue(), (object)((int)consequence.Type).ToSQLiteValue(), (object)consequence.ConsequenceValue.ToSQLiteValue()), false, true);
			foreach (TreatyIncentiveInfo incentive in ti.Incentives)
				this.db.ExecuteNonQuery(string.Format(Queries.InsertTreatyIncentiveInfo, (object)ti.ID.ToSQLiteValue(), (object)((int)incentive.Type).ToSQLiteValue(), (object)incentive.IncentiveValue.ToSQLiteValue()), false, true);
		}

		public void RemoveTreatyInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetTreatyRemoved, (object)id), false, true);
		}

		public void DeleteTreatyInfo(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveTreaty, (object)id), false, true);
		}

		public void SetTreatyActive(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.SetTreatyActive, (object)id), false, true);
		}

		public void UpdateDiplomacyInfo(DiplomacyInfo di)
		{
			di.Relations = Math.Max(Math.Min(di.Relations, DiplomacyInfo.MaxDeplomacyRelations), DiplomacyInfo.MinDeplomacyRelations);
			this._dom.diplomacy_states.Update(di.ID, di);
		}

		public DiplomacyInfo GetDiplomacyInfo(int playerID, int towardsPlayerID)
		{
			DiplomacyInfo diplomacyInfoByPlayer = this._dom.diplomacy_states.GetDiplomacyInfoByPlayer(playerID, towardsPlayerID);
			if (diplomacyInfoByPlayer != null)
				return diplomacyInfoByPlayer;
			return this._dom.diplomacy_states[this._dom.diplomacy_states.Insert(new int?(), new DiplomacyInfo()
			{
				PlayerID = playerID,
				TowardsPlayerID = towardsPlayerID,
				State = DiplomacyState.NEUTRAL,
				isEncountered = false
			})];
		}

		public void UpdateDiplomacyState(
		  int playerID,
		  int towardsPlayerID,
		  DiplomacyState state,
		  int relations,
		  bool reciprocal = true)
		{
			relations = Math.Max(Math.Min(relations, DiplomacyInfo.MaxDeplomacyRelations), DiplomacyInfo.MinDeplomacyRelations);
			if (this.GetPlayerInfo(playerID).IsOnTeam(this.GetPlayerInfo(towardsPlayerID)) && state != DiplomacyState.ALLIED)
				state = DiplomacyState.ALLIED;
			DiplomacyInfo diplomacyInfo1 = this.GetDiplomacyInfo(playerID, towardsPlayerID);
			diplomacyInfo1.State = state;
			diplomacyInfo1.Relations = relations;
			this._dom.diplomacy_states.Update(diplomacyInfo1.ID, diplomacyInfo1);
			if (!reciprocal)
				return;
			DiplomacyInfo diplomacyInfo2 = this.GetDiplomacyInfo(towardsPlayerID, playerID);
			diplomacyInfo2.State = state;
			diplomacyInfo2.Relations = relations;
			this._dom.diplomacy_states.Update(diplomacyInfo2.ID, diplomacyInfo2);
		}

		public void ChangeDiplomacyState(int playerID, int towardsPlayerID, DiplomacyState state)
		{
			if (this.GetPlayerInfo(playerID).IsOnTeam(this.GetPlayerInfo(towardsPlayerID)) && state != DiplomacyState.ALLIED)
				return;
			DiplomacyInfo diplomacyInfo = this.GetDiplomacyInfo(playerID, towardsPlayerID);
			if (diplomacyInfo.State == DiplomacyState.ALLIED & state != DiplomacyState.ALLIED)
				this.InsertTurnEvent(new TurnEvent()
				{
					EventType = TurnEventType.EV_LEFT_ALLIANCE,
					EventMessage = TurnEventMessage.EM_LEFT_ALLIANCE,
					PlayerID = playerID,
					TargetPlayerID = towardsPlayerID,
					TurnNumber = this.GetTurnCount(),
					ShowsDialog = false
				});
			this.UpdateDiplomacyState(playerID, towardsPlayerID, state, diplomacyInfo.Relations, true);
		}

		public void InsertDiplomacyReactionHistoryEntry(
		  int player1Id,
		  int player2Id,
		  int turn,
		  int reactionValue,
		  StratModifiers reaction)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDiplomacyReactionHistoryEntry, (object)player1Id.ToSQLiteValue(), (object)player2Id.ToSQLiteValue(), (object)turn.ToSQLiteValue(), (object)reactionValue.ToSQLiteValue(), (object)((int)reaction).ToSQLiteValue()), false, true);
		}

		public void InsertDiplomacyActionHistoryEntry(
		  int player1Id,
		  int player2Id,
		  int turn,
		  DiplomacyAction action,
		  int? actionSubType,
		  float? actionData,
		  int? duration,
		  int? consequenceType,
		  float? consequenceData)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertDiplomacyActionHistoryEntry, (object)player1Id.ToSQLiteValue(), (object)player2Id.ToSQLiteValue(), (object)turn.ToSQLiteValue(), (object)((int)action).ToSQLiteValue(), (object)actionSubType.ToNullableSQLiteValue(), (object)actionData.ToNullableSQLiteValue(), (object)duration.ToNullableSQLiteValue(), (object)consequenceType.ToNullableSQLiteValue(), (object)consequenceData.ToNullableSQLiteValue()), false, true);
		}

		public DiplomacyReactionHistoryEntryInfo ParseDiplomacyReactionHistoryEntry(
		  Row r)
		{
			return new DiplomacyReactionHistoryEntryInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerId = r[1].SQLiteValueToInteger(),
				TowardsPlayerId = r[2].SQLiteValueToInteger(),
				TurnCount = r[3].SQLiteValueToNullableInteger(),
				Difference = r[4].SQLiteValueToInteger(),
				Reaction = (StratModifiers)r[5].SQLiteValueToInteger()
			};
		}

		public IEnumerable<DiplomacyReactionHistoryEntryInfo> GetDiplomacyReactionHistory(
		  int player1Id,
		  int player2Id,
		  int turnCount,
		  int turnRange)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetDiplomacyReactionHistory, (object)turnCount, (object)turnRange, (object)player1Id, (object)player2Id), true);
			foreach (Row row in t.Rows)
				yield return this.ParseDiplomacyReactionHistoryEntry(row);
		}

		public DiplomacyActionHistoryEntryInfo ParseDiplomacyActionHistoryEntry(
		  Row r)
		{
			return new DiplomacyActionHistoryEntryInfo()
			{
				ID = r[0].SQLiteValueToInteger(),
				PlayerId = r[1].SQLiteValueToInteger(),
				TowardsPlayerId = r[2].SQLiteValueToInteger(),
				TurnCount = r[3].SQLiteValueToNullableInteger(),
				Action = (DiplomacyAction)r[4].SQLiteValueToInteger(),
				ActionSubType = r[5].SQLiteValueToNullableInteger(),
				ActionData = r[6].SQLiteValueToNullableSingle(),
				Duration = r[7].SQLiteValueToNullableInteger(),
				ConsequenceType = r[8].SQLiteValueToNullableInteger(),
				ConsequenceData = r[9].SQLiteValueToNullableSingle()
			};
		}

		public IEnumerable<DiplomacyActionHistoryEntryInfo> GetDiplomacyActionHistory(
		  int player1Id,
		  int player2Id,
		  int turnCount,
		  int turnRange)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetDiplomacyActionHistory, (object)turnCount, (object)turnRange, (object)player1Id, (object)player2Id), true);
			foreach (Row row in t.Rows)
				yield return this.ParseDiplomacyActionHistoryEntry(row);
		}

		public void ApplyDiplomacyReaction(
		  int playerID,
		  int towardsPlayerID,
		  int reactionAmount,
		  StratModifiers? reactionValueHintToRecord,
		  int reactionMultiplier = 1)
		{
			if (reactionAmount == 0)
				return;
			int num1 = 0;
			if (reactionAmount >= 0)
				num1 = (int)((double)this.GetStratModifierFloatToApply(StratModifiers.DiplomaticReactionBonus, playerID) * (double)reactionAmount);
			int num2 = 0;
			if (reactionAmount < 0)
				num2 = (int)((double)this.GetStratModifierFloatToApply(StratModifiers.NegativeRelationsModifier, playerID) * (double)reactionAmount);
			int num3;
			int num4 = (int)((double)(num3 = num1 + num2) * (double)this.assetdb.GovEffects.GetDiplomacyBonus(this, this.assetdb, this.GetPlayerInfo(playerID), this.GetPlayerInfo(towardsPlayerID)));
			reactionAmount = (num3 + num4) * reactionMultiplier;
			if (reactionValueHintToRecord.HasValue)
				this.InsertDiplomacyReactionHistoryEntry(playerID, towardsPlayerID, this.GetTurnCount(), reactionAmount, reactionValueHintToRecord.Value);
			DiplomacyInfo diplomacyInfo = this.GetDiplomacyInfo(playerID, towardsPlayerID);
			int relations = diplomacyInfo.Relations + reactionAmount;
			if (relations < DiplomacyInfo.MinDeplomacyRelations)
				relations = DiplomacyInfo.MinDeplomacyRelations;
			if (relations > DiplomacyInfo.MaxDeplomacyRelations)
				relations = DiplomacyInfo.MaxDeplomacyRelations;
			if (relations == diplomacyInfo.Relations)
				return;
			this.UpdateDiplomacyState(playerID, towardsPlayerID, diplomacyInfo.State, relations, false);
		}

		public void ApplyDiplomacyReaction(
		  int playerID,
		  int towardsPlayerID,
		  StratModifiers reactionValue,
		  int reactionMultiplier = 1)
		{
			if (reactionMultiplier == 0)
				return;
			int modifierIntToApply = this.GetStratModifierIntToApply(reactionValue, towardsPlayerID);
			this.ApplyDiplomacyReaction(playerID, towardsPlayerID, modifierIntToApply, new StratModifiers?(reactionValue), reactionMultiplier);
		}

		public IEnumerable<IndependentRaceInfo> GetIndependentRaces()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRaces), true))
				yield return new IndependentRaceInfo()
				{
					ID = int.Parse(row[0]),
					Name = row[1],
					OrbitalObjectID = int.Parse(row[2]),
					Population = double.Parse(row[3]),
					TechLevel = int.Parse(row[4]),
					ReactionHuman = int.Parse(row[5]),
					ReactionTarka = int.Parse(row[6]),
					ReactionLiir = int.Parse(row[7]),
					ReactionZuul = int.Parse(row[8]),
					ReactionMorrigi = int.Parse(row[9]),
					ReactionHiver = int.Parse(row[10])
				};
		}

		public IndependentRaceInfo GetIndependentRace(int raceID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRace, (object)raceID), true)[0];
			return new IndependentRaceInfo()
			{
				ID = raceID,
				Name = row[0],
				OrbitalObjectID = int.Parse(row[1]),
				Population = double.Parse(row[2]),
				TechLevel = int.Parse(row[3]),
				ReactionHuman = int.Parse(row[4]),
				ReactionTarka = int.Parse(row[5]),
				ReactionLiir = int.Parse(row[6]),
				ReactionZuul = int.Parse(row[7]),
				ReactionMorrigi = int.Parse(row[8]),
				ReactionHiver = int.Parse(row[9])
			};
		}

		public IndependentRaceInfo GetIndependentRaceForWorld(int orbitalObjectID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRace, (object)orbitalObjectID), true)[0];
			return new IndependentRaceInfo()
			{
				ID = int.Parse(row[0]),
				Name = row[1],
				OrbitalObjectID = orbitalObjectID,
				Population = double.Parse(row[2]),
				TechLevel = int.Parse(row[3]),
				ReactionHuman = int.Parse(row[4]),
				ReactionTarka = int.Parse(row[5]),
				ReactionLiir = int.Parse(row[6]),
				ReactionZuul = int.Parse(row[7]),
				ReactionMorrigi = int.Parse(row[8]),
				ReactionHiver = int.Parse(row[9])
			};
		}

		public IndependentRaceColonyInfo GetIndependentRaceColonyInfo(
		  int orbitalObjectID)
		{
			Row row = this.db.ExecuteTableQuery(string.Format(Queries.GetIndependentRaceColony, (object)orbitalObjectID), true)[0];
			return new IndependentRaceColonyInfo()
			{
				OrbitalObjectID = orbitalObjectID,
				RaceID = int.Parse(row[0]),
				Population = double.Parse(row[1])
			};
		}

		public EncounterInfo ParseEncounterInfo(Row row)
		{
			return new EncounterInfo()
			{
				Id = int.Parse(row[0]),
				Type = (EasterEgg)Enum.Parse(typeof(EasterEgg), row[1])
			};
		}

		public IEnumerable<EncounterInfo> GetEncounterInfos()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetEncounterInfos), true);
			foreach (Row row in t)
				yield return this.ParseEncounterInfo(row);
		}

		public EncounterInfo GetEncounterInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetEncounterInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseEncounterInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (EncounterInfo)null;
		}

		public void RemoveEncounter(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveEncounter, (object)id), false, true);
		}

		public int GetEncounterOrbitalId(EasterEgg type, int systemId)
		{
			foreach (EncounterInfo encounterInfo in this.GetEncounterInfos())
			{
				if (encounterInfo.Type == type)
				{
					switch (encounterInfo.Type)
					{
						case EasterEgg.EE_SWARM:
							SwarmerInfo swarmerInfo = this.GetSwarmerInfo(encounterInfo.Id);
							if (swarmerInfo != null && swarmerInfo.SystemId == systemId)
								return swarmerInfo.OrbitalId;
							continue;
						case EasterEgg.EE_ASTEROID_MONITOR:
							AsteroidMonitorInfo asteroidMonitorInfo = this.GetAsteroidMonitorInfo(encounterInfo.Id);
							if (asteroidMonitorInfo != null && asteroidMonitorInfo.SystemId == systemId)
								return asteroidMonitorInfo.OrbitalId;
							continue;
						case EasterEgg.EE_VON_NEUMANN:
							VonNeumannInfo vonNeumannInfo = this.GetVonNeumannInfo(encounterInfo.Id);
							if (vonNeumannInfo != null && vonNeumannInfo.SystemId == systemId)
								return vonNeumannInfo.OrbitalId;
							continue;
						default:
							continue;
					}
				}
			}
			return 0;
		}

		public int GetNumEncountersOfType(EasterEgg type)
		{
			int num = 0;
			foreach (EncounterInfo encounterInfo in this.GetEncounterInfos().ToList<EncounterInfo>())
			{
				if (encounterInfo.Type == type)
					++num;
			}
			return num;
		}

		public int GetEncounterIDAtSystem(EasterEgg type, int systemId)
		{
			foreach (EncounterInfo encounterInfo in this.GetEncounterInfos())
			{
				if (encounterInfo.Type == type)
				{
					switch (encounterInfo.Type)
					{
						case EasterEgg.EE_SWARM:
							SwarmerInfo swarmerInfo = this.GetSwarmerInfo(encounterInfo.Id);
							if (swarmerInfo != null && swarmerInfo.SystemId == systemId)
								return swarmerInfo.Id;
							continue;
						case EasterEgg.EE_ASTEROID_MONITOR:
							AsteroidMonitorInfo asteroidMonitorInfo = this.GetAsteroidMonitorInfo(encounterInfo.Id);
							if (asteroidMonitorInfo != null && asteroidMonitorInfo.SystemId == systemId)
								return asteroidMonitorInfo.Id;
							continue;
						case EasterEgg.EE_VON_NEUMANN:
							VonNeumannInfo vonNeumannInfo = this.GetVonNeumannInfo(encounterInfo.Id);
							if (vonNeumannInfo != null && vonNeumannInfo.SystemId == systemId)
								return vonNeumannInfo.Id;
							continue;
						case EasterEgg.EE_GARDENERS:
							GardenerInfo gardenerInfo = this.GetGardenerInfo(encounterInfo.Id);
							if (gardenerInfo != null && gardenerInfo.SystemId == systemId)
								return gardenerInfo.Id;
							continue;
						default:
							continue;
					}
				}
			}
			return 0;
		}

		public int GetEncounterPlayerId(GameSession sim, int systemId)
		{
			foreach (EncounterInfo encounterInfo in this.GetEncounterInfos())
			{
				switch (encounterInfo.Type)
				{
					case EasterEgg.EE_SWARM:
						SwarmerInfo swarmerInfo = this.GetSwarmerInfo(encounterInfo.Id);
						if (swarmerInfo != null && swarmerInfo.SystemId == systemId && sim.ScriptModules.Swarmers != null)
							return sim.ScriptModules.Swarmers.PlayerID;
						continue;
					case EasterEgg.EE_ASTEROID_MONITOR:
						AsteroidMonitorInfo asteroidMonitorInfo = this.GetAsteroidMonitorInfo(encounterInfo.Id);
						if (asteroidMonitorInfo != null && asteroidMonitorInfo.SystemId == systemId && sim.ScriptModules.AsteroidMonitor != null)
							return sim.ScriptModules.AsteroidMonitor.PlayerID;
						continue;
					case EasterEgg.EE_VON_NEUMANN:
						VonNeumannInfo vonNeumannInfo = this.GetVonNeumannInfo(encounterInfo.Id);
						if (vonNeumannInfo != null && vonNeumannInfo.SystemId == systemId && sim.ScriptModules.VonNeumann != null)
							return sim.ScriptModules.VonNeumann.PlayerID;
						continue;
					case EasterEgg.EE_GARDENERS:
						GardenerInfo gardenerInfo = this.GetGardenerInfo(encounterInfo.Id);
						if (gardenerInfo != null && gardenerInfo.SystemId == systemId && sim.ScriptModules.Gardeners != null)
							return sim.ScriptModules.Gardeners.PlayerID;
						continue;
					default:
						continue;
				}
			}
			return 0;
		}

		public LocustSwarmInfo ParseLocustSwarmInfo(Row row)
		{
			return new LocustSwarmInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				FleetId = row[1].SQLiteValueToNullableInteger(),
				Resources = row[2].SQLiteValueToInteger(),
				NumDrones = row[3].SQLiteValueToInteger()
			};
		}

		public LocustSwarmScoutInfo ParseLocustSwarmScoutInfo(Row row)
		{
			return new LocustSwarmScoutInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				LocustInfoId = row[1].SQLiteValueToInteger(),
				ShipId = row[2].SQLiteValueToInteger(),
				TargetSystemId = row[3].SQLiteValueToInteger(),
				NumDrones = row[4].SQLiteValueToInteger()
			};
		}

		public LocustSwarmScoutTargetInfo ParseLocustSwarmScoutTargetInfo(
		  Row row)
		{
			return new LocustSwarmScoutTargetInfo()
			{
				SystemId = row[0].SQLiteValueToInteger(),
				IsHostile = row[1].SQLiteValueToBoolean()
			};
		}

		public void UpdateLocustSwarmInfo(LocustSwarmInfo li)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLocustSwarmInfo, (object)li.Id.ToSQLiteValue(), (object)li.FleetId.ToNullableSQLiteValue(), (object)li.Resources.ToSQLiteValue(), (object)li.NumDrones.ToSQLiteValue()), false, true);
		}

		public void InsertLocustSwarmInfo(LocustSwarmInfo li)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmInfo, (object)EasterEgg.GM_LOCUST_SWARM.ToString(), (object)li.FleetId.ToNullableSQLiteValue(), (object)li.Resources.ToSQLiteValue(), (object)li.NumDrones.ToSQLiteValue()), false, true);
		}

		public LocustSwarmInfo GetLocustSwarmInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseLocustSwarmInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (LocustSwarmInfo)null;
		}

		public IEnumerable<LocustSwarmInfo> GetLocustSwarmInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmInfos), true))
				yield return this.ParseLocustSwarmInfo(row);
		}

		public void InsertLocustSwarmTarget(int encounterId, int systemId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmTarget, (object)systemId), false, true);
		}

		public IEnumerable<int> GetLocustSwarmTargets()
		{
			return (IEnumerable<int>)this.db.ExecuteIntegerArrayQuery(string.Format(Queries.GetLocustSwarmTargets));
		}

		public void InsertLocustSwarmScoutInfo(LocustSwarmScoutInfo ls)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmScoutInfo, (object)ls.LocustInfoId.ToSQLiteValue(), (object)ls.ShipId.ToSQLiteValue(), (object)ls.TargetSystemId.ToSQLiteValue(), (object)ls.NumDrones.ToSQLiteValue()), false, true);
		}

		public void UpdateLocustSwarmScoutInfo(LocustSwarmScoutInfo ls)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLocustSwarmScoutInfo, (object)ls.Id.ToSQLiteValue(), (object)ls.TargetSystemId.ToSQLiteValue(), (object)ls.NumDrones.ToSQLiteValue()), false, true);
		}

		public LocustSwarmScoutInfo GetLocustSwarmScoutInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseLocustSwarmScoutInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (LocustSwarmScoutInfo)null;
		}

		public IEnumerable<LocustSwarmScoutInfo> GetLocustSwarmScoutInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutInfos), true))
				yield return this.ParseLocustSwarmScoutInfo(row);
		}

		public IEnumerable<LocustSwarmScoutInfo> GetLocustSwarmScoutsForLocustNest(
		  int worldId)
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutsForLocustNest, (object)worldId.ToSQLiteValue()), true))
				yield return this.ParseLocustSwarmScoutInfo(row);
		}

		public void InsertLocustSwarmScoutTargetInfo(LocustSwarmScoutTargetInfo lst)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertLocustSwarmScoutTargetInfo, (object)lst.SystemId.ToSQLiteValue(), (object)lst.IsHostile.ToSQLiteValue()), false, true);
		}

		public void UpdateLocustSwarmScoutTargetInfo(LocustSwarmScoutTargetInfo lst)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateLocustSwarmScoutTargetInfo, (object)lst.SystemId.ToSQLiteValue(), (object)lst.IsHostile.ToSQLiteValue()), false, true);
		}

		public LocustSwarmScoutTargetInfo GetLocustSwarmScoutTargetInfos(
		  int systemId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutTargetInfo, (object)systemId), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseLocustSwarmScoutTargetInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (LocustSwarmScoutTargetInfo)null;
		}

		public IEnumerable<LocustSwarmScoutTargetInfo> GetLocustSwarmScoutTargetInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetLocustSwarmScoutTargetInfos), true))
				yield return this.ParseLocustSwarmScoutTargetInfo(row);
		}

		public SystemKillerInfo ParseSystemKillerInfo(Row row)
		{
			return new SystemKillerInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				FleetId = row[1].SQLiteValueToNullableInteger(),
				Target = row[2].SQLiteValueToVector3()
			};
		}

		public void UpdateSystemKillerInfo(SystemKillerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSystemKillerInfo, (object)si.Id.ToSQLiteValue(), (object)si.FleetId.ToNullableSQLiteValue(), (object)si.Target.ToSQLiteValue()), false, true);
		}

		public void InsertSystemKillerInfo(SystemKillerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertSystemKillerInfo, (object)EasterEgg.GM_SYSTEM_KILLER.ToString(), (object)si.FleetId.ToNullableSQLiteValue(), (object)si.Target.ToSQLiteValue()), false, true);
		}

		public SystemKillerInfo GetSystemKillerInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSystemKillerInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseSystemKillerInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (SystemKillerInfo)null;
		}

		public NeutronStarInfo ParseNeutronStarInfo(Row row)
		{
			return new NeutronStarInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				FleetId = row[1].SQLiteValueToInteger(),
				TargetSystemId = row[2].SQLiteValueToInteger(),
				DeepSpaceSystemId = row[3].SQLiteValueToNullableInteger(),
				DeepSpaceOrbitalId = row[4].SQLiteValueToNullableInteger()
			};
		}

		public void InsertNeutronStarInfo(NeutronStarInfo nsi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertNeutronStarInfo, (object)EasterEgg.GM_NEUTRON_STAR.ToString(), (object)nsi.FleetId.ToSQLiteValue(), (object)nsi.TargetSystemId.ToSQLiteValue(), (object)nsi.DeepSpaceSystemId.ToNullableSQLiteValue(), (object)nsi.DeepSpaceOrbitalId.ToNullableSQLiteValue()), false, true);
		}

		public NeutronStarInfo GetNeutronStarInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetNeutronStarInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseNeutronStarInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (NeutronStarInfo)null;
		}

		public IEnumerable<NeutronStarInfo> GetNeutronStarInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetNeutronStarInfos), true))
				yield return this.ParseNeutronStarInfo(row);
		}

		public SuperNovaInfo ParseSuperNovaInfo(Row row)
		{
			return new SuperNovaInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				TurnsRemaining = row[2].SQLiteValueToInteger()
			};
		}

		public void InsertSuperNovaInfo(SuperNovaInfo sni)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertSuperNovaInfo, (object)EasterEgg.GM_SUPER_NOVA.ToString(), (object)sni.SystemId.ToSQLiteValue(), (object)sni.TurnsRemaining.ToSQLiteValue()), false, true);
		}

		public void UpdateSuperNovaInfo(SuperNovaInfo sni)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSuperNovaInfo, (object)sni.Id.ToSQLiteValue(), (object)sni.TurnsRemaining.ToSQLiteValue()), false, true);
		}

		public SuperNovaInfo GetSuperNovaInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSuperNovaInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseSuperNovaInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (SuperNovaInfo)null;
		}

		public IEnumerable<SuperNovaInfo> GetSuperNovaInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetSuperNovaInfos), true))
				yield return this.ParseSuperNovaInfo(row);
		}

		public PirateBaseInfo ParsePirateBaseInfo(Row row)
		{
			return new PirateBaseInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				BaseStationId = row[2].SQLiteValueToInteger(),
				TurnsUntilAddShip = row[3].SQLiteValueToInteger(),
				NumShips = row[4].SQLiteValueToInteger()
			};
		}

		public void InsertPirateBaseInfo(PirateBaseInfo pbi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPirateBaseInfo, (object)EasterEgg.EE_PIRATE_BASE.ToString(), (object)pbi.SystemId.ToSQLiteValue(), (object)pbi.BaseStationId.ToSQLiteValue(), (object)pbi.TurnsUntilAddShip.ToSQLiteValue(), (object)pbi.NumShips.ToSQLiteValue()), false, true);
		}

		public void UpdatePirateBaseInfo(PirateBaseInfo pbi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdatePirateBaseInfo, (object)pbi.Id.ToSQLiteValue(), (object)pbi.TurnsUntilAddShip.ToSQLiteValue(), (object)pbi.NumShips.ToSQLiteValue()), false, true);
		}

		public PirateBaseInfo GetPirateBaseInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetPirateBaseInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParsePirateBaseInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (PirateBaseInfo)null;
		}

		public IEnumerable<PirateBaseInfo> GetPirateBaseInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetPirateBaseInfos), true))
				yield return this.ParsePirateBaseInfo(row);
		}

		public SystemKillerInfo GetSystemKillerInfoByFleetID(int fleetId)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSystemKillerInfoByFleetID, (object)fleetId), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseSystemKillerInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (SystemKillerInfo)null;
		}

		public MorrigiRelicInfo ParseMorrigiRelicInfo(Row row)
		{
			return new MorrigiRelicInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				FleetId = row[2].SQLiteValueToInteger(),
				IsAggressive = row[3].SQLiteValueToBoolean()
			};
		}

		public void UpdateMorrigiRelicInfo(MorrigiRelicInfo mi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateMorrigiRelicInfo, (object)mi.Id, (object)mi.SystemId, (object)mi.FleetId, (object)mi.IsAggressive.ToSQLiteValue()), false, true);
		}

		public void InsertMorrigiRelicInfo(MorrigiRelicInfo mi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertMorrigiRelicInfo, (object)EasterEgg.EE_MORRIGI_RELIC.ToString(), (object)mi.SystemId, (object)mi.FleetId, (object)mi.IsAggressive.ToSQLiteValue()), false, true);
		}

		public MorrigiRelicInfo GetMorrigiRelicInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetMorrigiRelicInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseMorrigiRelicInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (MorrigiRelicInfo)null;
		}

		public IEnumerable<MorrigiRelicInfo> GetMorrigiRelicInfos()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(Queries.GetMorrigiRelicInfos, true);
			foreach (Row row in t.Rows)
				yield return this.ParseMorrigiRelicInfo(row);
		}

		public AsteroidMonitorInfo ParseAsteroidMonitorInfo(Row row)
		{
			return new AsteroidMonitorInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				OrbitalId = row[2].SQLiteValueToInteger(),
				IsAggressive = row[3].SQLiteValueToBoolean()
			};
		}

		public void UpdateAsteroidMonitorInfo(AsteroidMonitorInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateAsteroidMonitorInfo, (object)gi.Id, (object)gi.SystemId, (object)gi.OrbitalId, (object)gi.IsAggressive.ToSQLiteValue()), false, true);
		}

		public void InsertAsteroidMonitorInfo(AsteroidMonitorInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertAsteroidMonitorInfo, (object)EasterEgg.EE_ASTEROID_MONITOR.ToString(), (object)gi.SystemId, (object)gi.OrbitalId, (object)gi.IsAggressive.ToSQLiteValue()), false, true);
		}

		public IEnumerable<AsteroidMonitorInfo> GetAllAsteroidMonitorInfos()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(Queries.GetAsteroidMonitorInfos, true);
			foreach (Row row in t.Rows)
				yield return this.ParseAsteroidMonitorInfo(row);
		}

		public AsteroidMonitorInfo GetAsteroidMonitorInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetAsteroidMonitorInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseAsteroidMonitorInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (AsteroidMonitorInfo)null;
		}

		public GardenerInfo ParseGardenerInfo(Row row)
		{
			GardenerInfo gardenerInfo = new GardenerInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				FleetId = row[2].SQLiteValueToInteger(),
				GardenerFleetId = row[3].SQLiteValueToInteger(),
				TurnsToWait = row[4].SQLiteValueToInteger(),
				IsGardener = row[5].SQLiteValueToBoolean(),
				DeepSpaceSystemId = row[6].SQLiteValueToNullableInteger(),
				DeepSpaceOrbitalId = row[7].SQLiteValueToNullableInteger()
			};
			foreach (Row row1 in this.db.ExecuteTableQuery(string.Format(Queries.GetProteanShipOrbitMaps, (object)gardenerInfo.Id), true).Rows)
				gardenerInfo.ShipOrbitMap.Add(row1[0].SQLiteValueToInteger(), row1[1].SQLiteValueToInteger());
			return gardenerInfo;
		}

		public void InsertProteanShipOrbitMap(int encounterId, int shipId, int orbitalObjectId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertProteanShipOrbitMap, (object)encounterId, (object)shipId, (object)orbitalObjectId), false, true);
		}

		public void UpdateGardenerInfo(GardenerInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateGardenerInfo, (object)gi.Id, (object)gi.SystemId.ToSQLiteValue(), (object)gi.GardenerFleetId.ToSQLiteValue(), (object)gi.TurnsToWait.ToSQLiteValue()), false, true);
		}

		public void InsertGardenerInfo(GardenerInfo gi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertGardenerInfo, gi.IsGardener ? (object)EasterEgg.GM_GARDENER.ToString() : (object)EasterEgg.EE_GARDENERS.ToString(), (object)gi.SystemId.ToSQLiteValue(), (object)gi.FleetId.ToSQLiteValue(), (object)gi.GardenerFleetId.ToSQLiteValue(), (object)gi.TurnsToWait.ToSQLiteValue(), (object)gi.IsGardener.ToSQLiteValue(), (object)gi.DeepSpaceSystemId.ToNullableSQLiteValue(), (object)gi.DeepSpaceOrbitalId.ToNullableSQLiteValue()), false, true);
		}

		public GardenerInfo GetGardenerInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetGardenerInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseGardenerInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (GardenerInfo)null;
		}

		public IEnumerable<GardenerInfo> GetGardenerInfos()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(Queries.GetGardenerInfos, true);
			foreach (Row row in t.Rows)
				yield return this.ParseGardenerInfo(row);
		}

		public SwarmerInfo ParseSwarmerInfo(Row row)
		{
			return new SwarmerInfo()
			{
				Id = int.Parse(row[0]),
				SystemId = int.Parse(row[1]),
				OrbitalId = int.Parse(row[2]),
				GrowthStage = int.Parse(row[3]),
				HiveFleetId = row[4].SQLiteValueToNullableInteger(),
				QueenFleetId = row[5].SQLiteValueToNullableInteger(),
				SpawnHiveDelay = row[6].SQLiteValueToInteger()
			};
		}

		public void UpdateSwarmerInfo(SwarmerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateSwarmerInfo, (object)si.Id, (object)si.SystemId, (object)si.OrbitalId, (object)si.GrowthStage, (object)si.HiveFleetId.ToNullableSQLiteValue(), (object)si.QueenFleetId.ToNullableSQLiteValue(), (object)si.SpawnHiveDelay), false, true);
		}

		public void InsertSwarmerInfo(SwarmerInfo si)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertSwarmerInfo, (object)EasterEgg.EE_SWARM.ToString(), (object)si.SystemId, (object)si.OrbitalId, (object)si.GrowthStage, (object)si.HiveFleetId.ToNullableSQLiteValue(), (object)si.QueenFleetId.ToNullableSQLiteValue(), (object)si.SpawnHiveDelay), false, true);
		}

		public SwarmerInfo GetSwarmerInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetSwarmerInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseSwarmerInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (SwarmerInfo)null;
		}

		public IEnumerable<SwarmerInfo> GetSwarmerInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetSwarmerInfos), true))
				yield return this.ParseSwarmerInfo(row);
		}

		public VonNeumannTargetInfo ParseVonNeumannTargetInfo(Row row)
		{
			return new VonNeumannTargetInfo()
			{
				SystemId = int.Parse(row[0]),
				ThreatLevel = int.Parse(row[1])
			};
		}

		public VonNeumannInfo ParseVonNeumannInfo(Row row)
		{
			VonNeumannInfo vonNeumannInfo = new VonNeumannInfo()
			{
				Id = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				OrbitalId = row[2].SQLiteValueToInteger(),
				Resources = row[3].SQLiteValueToInteger(),
				ConstructionProgress = row[4].SQLiteValueToInteger(),
				ResourcesCollectedLastTurn = row[5].SQLiteValueToInteger(),
				ProjectDesignId = row[6].SQLiteValueToNullableInteger(),
				LastCollectionSystem = row[7].SQLiteValueToInteger(),
				LastTargetSystem = row[8].SQLiteValueToInteger(),
				LastCollectionTurn = row[9].SQLiteValueToInteger(),
				LastTargetTurn = row[10].SQLiteValueToInteger(),
				FleetId = row[11].SQLiteValueToNullableInteger()
			};
			int? projectDesignId = vonNeumannInfo.ProjectDesignId;
			if ((projectDesignId.GetValueOrDefault() != 0 ? 0 : (projectDesignId.HasValue ? 1 : 0)) != 0)
				vonNeumannInfo.ProjectDesignId = new int?();
			foreach (Row row1 in this.db.ExecuteTableQuery(string.Format(Queries.GetVonNeumannTargetInfos, (object)vonNeumannInfo.Id), true))
				vonNeumannInfo.TargetInfos.Add(this.ParseVonNeumannTargetInfo(row1));
			return vonNeumannInfo;
		}

		public IEnumerable<VonNeumannInfo> GetVonNeumannInfos()
		{
			foreach (Row row in this.db.ExecuteTableQuery(string.Format(Queries.GetVonNeumannInfos), true))
				yield return this.ParseVonNeumannInfo(row);
		}

		public void UpdateVonNeumannTargetInfo(int encounterId, VonNeumannTargetInfo vi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateVonNeumannTargetInfo, (object)encounterId, (object)vi.SystemId, (object)vi.ThreatLevel), false, true);
		}

		public void RemoveVonNeumannTargetInfo(int encounterId, int systemId)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveVonNeumannTargetInfo, (object)encounterId, (object)systemId), false, true);
		}

		public void InsertVonNeumannInfo(VonNeumannInfo vi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertVonNeumannInfo, (object)EasterEgg.EE_VON_NEUMANN.ToString(), (object)vi.SystemId, (object)vi.OrbitalId, (object)vi.FleetId.ToNullableSQLiteValue(), (object)vi.Resources, (object)vi.ConstructionProgress), false, true);
		}

		public void UpdateVonNeumannInfo(VonNeumannInfo vi)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.UpdateVonNeumannInfo, (object)vi.Id.ToSQLiteValue(), (object)vi.SystemId.ToSQLiteValue(), (object)vi.OrbitalId.ToSQLiteValue(), (object)vi.Resources.ToSQLiteValue(), (object)vi.ConstructionProgress.ToSQLiteValue(), (object)vi.ResourcesCollectedLastTurn.ToSQLiteValue(), (object)vi.ProjectDesignId.ToNullableSQLiteValue(), (object)vi.LastCollectionSystem.ToSQLiteValue(), (object)vi.LastTargetSystem.ToSQLiteValue(), (object)vi.LastCollectionTurn.ToSQLiteValue(), (object)vi.LastTargetTurn.ToSQLiteValue(), (object)vi.FleetId.ToNullableSQLiteValue()), false, true);
			foreach (VonNeumannTargetInfo targetInfo in vi.TargetInfos)
				this.UpdateVonNeumannTargetInfo(vi.Id, targetInfo);
		}

		public VonNeumannInfo GetVonNeumannInfo(int id)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetVonNeumannInfo, (object)id), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseVonNeumannInfo(((IEnumerable<Row>)table.Rows).First<Row>());
			return (VonNeumannInfo)null;
		}

		private int InsertLimboFleet(int playerID, int systemID)
		{
			return this.InsertFleetCore(playerID, 0, systemID, 0, "Limbo", FleetType.FL_LIMBOFLEET);
		}

		public int InsertOrGetLimboFleetID(int systemID, int playerID)
		{
			FleetInfo fleetInfo = this.GetFleetsByPlayerAndSystem(playerID, systemID, FleetType.FL_LIMBOFLEET).FirstOrDefault<FleetInfo>();
			return fleetInfo != null ? fleetInfo.ID : this.InsertLimboFleet(playerID, systemID);
		}

		public void RetrofitShip(int shipid, int systemid, int playerid, int? retrofitorderid = null)
		{
			int limboFleetId = this.InsertOrGetLimboFleetID(systemid, playerid);
			this.TransferShip(shipid, limboFleetId);
			if (!retrofitorderid.HasValue)
				retrofitorderid = new int?(this.InsertInvoiceInstance(playerid, systemid, "Retrofit Order"));
			DesignInfo newestRetrofitDesign = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(this.GetShipInfo(shipid, true).DesignInfo, this.GetDesignInfosForPlayer(playerid));
			this.InsertRetrofitOrder(systemid, newestRetrofitDesign.ID, shipid, new int?(retrofitorderid.Value));
		}

		public void RetrofitShips(IEnumerable<int> ships, int systemid, int playerid)
		{
			int num = this.InsertInvoiceInstance(playerid, systemid, "Retrofit Order");
			foreach (int ship in ships)
				this.RetrofitShip(ship, systemid, playerid, new int?(num));
		}

		private EmpireHistoryData ParseEmpireHistoryData(Row row)
		{
			return new EmpireHistoryData()
			{
				id = row[0].SQLiteValueToInteger(),
				playerID = row[1].SQLiteValueToInteger(),
				turn = row[2].SQLiteValueToInteger(),
				colonies = row[3].SQLiteValueToInteger(),
				provinces = row[4].SQLiteValueToInteger(),
				bases = row[5].SQLiteValueToInteger(),
				fleets = row[6].SQLiteValueToInteger(),
				ships = row[7].SQLiteValueToInteger(),
				empire_pop = row[8].SQLiteValueToDouble(),
				empire_economy = row[9].SQLiteValueToSingle(),
				empire_biosphere = row[10].SQLiteValueToInteger(),
				empire_trade = row[11].SQLiteValueToDouble(),
				empire_morale = row[12].SQLiteValueToInteger(),
				savings = row[13].SQLiteValueToDouble(),
				psi_potential = row[14].SQLiteValueToInteger()
			};
		}

		public EmpireHistoryData GetLastEmpireHistoryForPlayer(int playerid)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetEmpireHistoryByPlayerIDAndTurn, (object)playerid, (object)(this.GetTurnCount() - 1)), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseEmpireHistoryData(table.Rows[0]);
			return (EmpireHistoryData)null;
		}

		public int InsertEmpireHistoryForPlayer(EmpireHistoryData history)
		{
			this.RemoveEmpireHistoryForPlayer(history.playerID);
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertEmpireHistory, (object)history.playerID, (object)this.GetTurnCount(), (object)history.colonies, (object)history.provinces, (object)history.bases, (object)history.fleets, (object)history.ships, (object)history.empire_pop, (object)history.empire_economy, (object)history.empire_biosphere, (object)history.empire_trade, (object)history.empire_morale, (object)history.savings, (object)history.psi_potential));
		}

		public void RemoveEmpireHistoryForPlayer(int playerid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveEmpireHistoryByPlayerID, (object)playerid), false, true);
		}

		private ColonyHistoryData ParseColonyHistoryData(Row row)
		{
			return new ColonyHistoryData()
			{
				id = row[0].SQLiteValueToInteger(),
				colonyID = row[1].SQLiteValueToInteger(),
				turn = row[2].SQLiteValueToInteger(),
				resources = row[3].SQLiteValueToInteger(),
				biosphere = row[4].SQLiteValueToInteger(),
				infrastructure = row[5].SQLiteValueToSingle(),
				hazard = row[6].SQLiteValueToSingle(),
				income = row[7].SQLiteValueToInteger(),
				econ_rating = row[8].SQLiteValueToSingle(),
				life_support_cost = row[9].SQLiteValueToInteger(),
				industrial_output = row[10].SQLiteValueToInteger(),
				civ_pop = row[11].SQLiteValueToDouble(),
				imp_pop = row[12].SQLiteValueToDouble(),
				slave_pop = row[13].SQLiteValueToDouble()
			};
		}

		public ColonyHistoryData GetLastColonyHistoryForColony(int colonyid)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetColonyHistoryByColonyIDAndTurn, (object)colonyid, (object)(this.GetTurnCount() - 1)), true);
			if (((IEnumerable<Row>)table.Rows).Count<Row>() > 0)
				return this.ParseColonyHistoryData(table.Rows[0]);
			return (ColonyHistoryData)null;
		}

		public int InsertColonyHistory(ColonyHistoryData history)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertColonyHistory, (object)history.colonyID, (object)this.GetTurnCount(), (object)history.resources, (object)history.biosphere, (object)history.infrastructure, (object)history.hazard, (object)history.income, (object)history.econ_rating, (object)history.life_support_cost, (object)history.industrial_output, (object)history.civ_pop, (object)history.imp_pop, (object)history.slave_pop));
		}

		public void RemoveColonyHistory(int colonyid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveColonyHistoryByColonyID, (object)colonyid), false, true);
		}

		public void RemoveAllColonyHistory()
		{
			this.db.ExecuteNonQuery(Queries.RemoveAllColonyHistory, false, true);
		}

		private ColonyFactionMoraleHistory ParseColonyFactionMoraleHistory(
		  Row row)
		{
			return new ColonyFactionMoraleHistory()
			{
				id = row[0].SQLiteValueToInteger(),
				colonyID = row[1].SQLiteValueToInteger(),
				turn = row[2].SQLiteValueToInteger(),
				factionid = row[3].SQLiteValueToInteger(),
				morale = row[4].SQLiteValueToInteger(),
				population = row[5].SQLiteValueToDouble()
			};
		}

		public IEnumerable<ColonyFactionMoraleHistory> GetLastColonyMoraleHistoryForColony(
		  int colonyid)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetColonyFactionMoraleHistoryByColonyIDAndTurn, (object)colonyid, (object)(this.GetTurnCount() - 1)), true);
			foreach (Row row in t.Rows)
				yield return this.ParseColonyFactionMoraleHistory(row);
		}

		public int InsertColonyMoraleHistory(ColonyFactionMoraleHistory history)
		{
			return this.db.ExecuteIntegerQuery(string.Format(Queries.InsertColonyFactionMoraleHistory, (object)history.colonyID, (object)this.GetTurnCount(), (object)history.factionid, (object)history.morale, (object)history.population));
		}

		public void RemoveColonyMoraleHistory(int colonyid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveColonyFactionMoraleHistoryByColonyID, (object)colonyid), false, true);
		}

		public IEnumerable<DesignInfo> GetDesignsEncountered(int playerid)
		{
			Kerberos.Sots.Data.SQLite.Table table = this.db.ExecuteTableQuery(string.Format(Queries.GetEnounteredDesigns, (object)playerid.ToSQLiteValue()), true);
			List<DesignInfo> designInfoList = new List<DesignInfo>();
			foreach (Row row in table.Rows)
			{
				DesignInfo di = this.GetDesignInfo(row[1].SQLiteValueToInteger());
				if (di != null && this.GetStandardPlayerIDs().Any<int>((Func<int, bool>)(x => x == di.PlayerID)))
					designInfoList.Add(di);
			}
			return (IEnumerable<DesignInfo>)designInfoList;
		}

		public void InsertDesignEncountered(int playerid, int designid)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertEnounteredDesign, (object)playerid.ToSQLiteValue(), (object)designid.ToSQLiteValue()), false, true);
		}

		public void RemoveMoraleEventHistory(int eventid)
		{
			this._dom.morale_event_history.Remove(eventid);
		}

		public void RemoveAllMoraleEventsHistory()
		{
			this.db.ExecuteNonQuery(string.Format(Queries.RemoveAllMoraleHistoryEvents, (object)(this.GetTurnCount() - 1)), false, true);
			this._dom.morale_event_history.Clear();
		}

		public IEnumerable<MoraleEventHistory> GetMoraleHistoryEventsForColony(
		  int colonyid)
		{
			ColonyInfo ci = this.GetColonyInfo(colonyid);
			StarSystemInfo ssi = this.GetStarSystemInfo(ci.CachedStarSystemID);
			return (IEnumerable<MoraleEventHistory>)this._dom.morale_event_history.Where<KeyValuePair<int, MoraleEventHistory>>((Func<KeyValuePair<int, MoraleEventHistory>, bool>)(x =>
		  {
			  if (x.Value.turn == this.GetTurnCount() - 1)
				  return x.Value.playerId == ci.PlayerID;
			  return false;
		  })).Select<KeyValuePair<int, MoraleEventHistory>, MoraleEventHistory>((Func<KeyValuePair<int, MoraleEventHistory>, MoraleEventHistory>)(x => x.Value)).ToList<MoraleEventHistory>().Where<MoraleEventHistory>((Func<MoraleEventHistory, bool>)(x =>
	{
		int? colonyId = x.colonyId;
		int num = colonyid;
		if ((colonyId.GetValueOrDefault() != num ? 0 : (colonyId.HasValue ? 1 : 0)) == 0)
		{
			int? systemId = x.systemId;
			int cachedStarSystemId = ci.CachedStarSystemID;
			if ((systemId.GetValueOrDefault() != cachedStarSystemId ? 0 : (systemId.HasValue ? 1 : 0)) == 0)
			{
				if (ssi.ProvinceID.HasValue)
				{
					int? provinceId1 = x.provinceId;
					int? provinceId2 = ssi.ProvinceID;
					if ((provinceId1.GetValueOrDefault() != provinceId2.GetValueOrDefault() ? 0 : (provinceId1.HasValue == provinceId2.HasValue ? 1 : 0)) != 0)
						goto label_7;
				}
				if (!x.colonyId.HasValue && !x.systemId.HasValue)
					return !x.provinceId.HasValue;
				return false;
			}
		}
	label_7:
		return true;
	})).ToList<MoraleEventHistory>();
		}

		public IEnumerable<MoraleEventHistory> GetMoraleHistoryEventsForPlayer(
		  int playerid)
		{
			return (IEnumerable<MoraleEventHistory>)this._dom.morale_event_history.Where<KeyValuePair<int, MoraleEventHistory>>((Func<KeyValuePair<int, MoraleEventHistory>, bool>)(x =>
		  {
			  if (x.Value.turn == this.GetTurnCount() - 1)
				  return x.Value.playerId == playerid;
			  return false;
		  })).Select<KeyValuePair<int, MoraleEventHistory>, MoraleEventHistory>((Func<KeyValuePair<int, MoraleEventHistory>, MoraleEventHistory>)(x => x.Value)).ToList<MoraleEventHistory>();
		}

		public int InsertMoralHistoryEvent(
		  MoralEvent type,
		  int playerid,
		  int value,
		  int? colonyid,
		  int? systemid,
		  int? provinceid)
		{
			return this._dom.morale_event_history.Insert(new int?(), new MoraleEventHistory()
			{
				playerId = playerid,
				value = value,
				moraleEvent = type,
				colonyId = colonyid,
				systemId = systemid,
				provinceId = provinceid,
				turn = this.GetTurnCount()
			});
		}

		private IncomingRandomInfo ParseIncomingRandom(Row row)
		{
			return new IncomingRandomInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				PlayerId = row[1].SQLiteValueToInteger(),
				SystemId = row[2].SQLiteValueToInteger(),
				type = (RandomEncounter)row[3].SQLiteValueToInteger(),
				turn = row[4].SQLiteValueToInteger()
			};
		}

		public IEnumerable<IncomingRandomInfo> GetIncomingRandomsForPlayerThisTurn(
		  int playerid)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.SelectIncomingRandomsForPlayer, (object)playerid, (object)this.GetTurnCount()), true);
			foreach (Row row in t.Rows)
				yield return this.ParseIncomingRandom(row);
		}

		public void InsertIncomingRandom(
		  int playerid,
		  int systemid,
		  RandomEncounter type,
		  int turntoappear)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIncomingRandom, (object)playerid.ToSQLiteValue(), (object)systemid.ToSQLiteValue(), (object)((int)type).ToSQLiteValue(), (object)turntoappear.ToSQLiteValue()), false, true);
		}

		public void RemoveIncomingRandom(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DeleteIncomingRandom, (object)id.ToSQLiteValue()), false, true);
		}

		private IncomingGMInfo ParseIncomingGM(Row row)
		{
			return new IncomingGMInfo()
			{
				ID = row[0].SQLiteValueToInteger(),
				SystemId = row[1].SQLiteValueToInteger(),
				type = (EasterEgg)row[2].SQLiteValueToInteger(),
				turn = row[3].SQLiteValueToInteger()
			};
		}

		public IEnumerable<IncomingGMInfo> GetIncomingGMsThisTurn()
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.SelectIncomingGMsForPlayer, (object)this.GetTurnCount()), true);
			foreach (Row row in t.Rows)
				yield return this.ParseIncomingGM(row);
		}

		public void InsertIncomingGM(int systemid, EasterEgg type, int turntoappear)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.InsertIncomingGM, (object)systemid.ToSQLiteValue(), (object)((int)type).ToSQLiteValue(), (object)turntoappear.ToSQLiteValue()), false, true);
		}

		public void RemoveIncomingGM(int id)
		{
			this.db.ExecuteNonQuery(string.Format(Queries.DeleteIncomingGM, (object)id.ToSQLiteValue()), false, true);
		}

		private PiracyFleetDetectionInfo ParsePiracyFleetDetetcionInfo(Row row)
		{
			return new PiracyFleetDetectionInfo()
			{
				FleetID = row[0].SQLiteValueToInteger(),
				PlayerID = row[1].SQLiteValueToInteger()
			};
		}

		public IEnumerable<PiracyFleetDetectionInfo> GetPiracyFleetDetectionInfoForFleet(
		  int fleetid)
		{
			Kerberos.Sots.Data.SQLite.Table t = this.db.ExecuteTableQuery(string.Format(Queries.GetPiracyFleetDectectionInfoForFleet, (object)fleetid), true);
			foreach (Row row in t.Rows)
				yield return this.ParsePiracyFleetDetetcionInfo(row);
		}

		public void InsertPiracyFleetDetectionInfo(int fleetid, int playerID)
		{
			MissionInfo missionByFleetId = this.GetMissionByFleetID(fleetid);
			if (missionByFleetId == null || missionByFleetId.Type != MissionType.PIRACY)
				return;
			this.db.ExecuteNonQuery(string.Format(Queries.InsertPiracyFleetDetection, (object)fleetid.ToSQLiteValue(), (object)playerID.ToSQLiteValue(), (object)missionByFleetId.ID.ToSQLiteValue()), false, true);
		}

		public static Matrix CalcTransform(
		  int orbitalId,
		  float t,
		  IEnumerable<OrbitalObjectInfo> orbitals)
		{
			t = 0.0f;
			OrbitalObjectInfo orbitalObjectInfo = orbitals.First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.ID == orbitalId));
			Matrix matrix = orbitalObjectInfo.OrbitalPath.GetTransform((double)t);
			if (orbitalObjectInfo.ParentID.HasValue && orbitalObjectInfo.ParentID.Value != 0)
				matrix = GameDatabase.CalcTransform(orbitalObjectInfo.ParentID.Value, t, orbitals) * matrix;
			return matrix;
		}

		public int GetFleetSupportingSystem(int fleetID)
		{
			FleetInfo fleetInfo = this.GetFleetInfo(fleetID);
			if (fleetInfo != null && fleetInfo.SupportingSystemID != 0)
				return fleetInfo.SupportingSystemID;
			return 0;
		}

		public bool IsSurveyed(int playerID, int systemID)
		{
			return this.GetLastTurnExploredByPlayer(playerID, systemID) > 0;
		}

		public bool CanSurvey(int playerID, int systemID)
		{
			return !this.IsSurveyed(playerID, systemID);
		}

		public bool CanColonize(int playerID, int systemID, int MaxHazard)
		{
			if (this.GetLastTurnExploredByPlayer(playerID, systemID) < 1)
				return false;
			foreach (int num in this.GetStarSystemPlanets(systemID).ToList<int>())
			{
				if (this.CanColonizePlanet(playerID, num, MaxHazard) && this.hasPermissionToBuildEnclave(playerID, num))
					return true;
			}
			return false;
		}

		public bool CanRelocate(GameSession sim, int playerId, int systemId)
		{
			return this.GetRemainingSupportPoints(sim, systemId, playerId) > 0;
		}

		public int GetFleetCruiserEquivalent(int fleetId)
		{
			int num = 0;
			foreach (ShipInfo shipInfo in this.GetShipInfoByFleetID(fleetId, true).ToList<ShipInfo>())
				num += this.GetShipCruiserEquivalent(shipInfo.DesignInfo);
			return num;
		}

		public int GetShipCruiserEquivalent(DesignInfo shipDesign)
		{
			if (shipDesign == null || shipDesign.IsPlatform() || shipDesign.IsSDB())
				return 0;
			switch (shipDesign.Class)
			{
				case ShipClass.Cruiser:
					return 1;
				case ShipClass.Dreadnought:
					return 3;
				case ShipClass.Leviathan:
					return 9;
				default:
					return 0;
			}
		}

		public int GetFleetCruiserEquivalentEstimate(int fleetId, int shipid)
		{
			int num = 0;
			foreach (ShipInfo shipInfo in this.GetShipInfoByFleetID(fleetId, false).ToList<ShipInfo>())
			{
				if (!shipInfo.DesignInfo.IsPlatform() && !shipInfo.DesignInfo.IsSDB())
				{
					switch (this.GetDesignInfo(shipInfo.DesignID).Class)
					{
						case ShipClass.Cruiser:
							++num;
							continue;
						case ShipClass.Dreadnought:
							num += 3;
							continue;
						case ShipClass.Leviathan:
							num += 9;
							continue;
						default:
							continue;
					}
				}
			}
			ShipInfo shipInfo1 = this.GetShipInfo(shipid, true);
			if (!shipInfo1.DesignInfo.IsPlatform() && !shipInfo1.DesignInfo.IsSDB())
			{
				switch (shipInfo1.DesignInfo.Class)
				{
					case ShipClass.Cruiser:
						++num;
						break;
					case ShipClass.Dreadnought:
						num += 3;
						break;
					case ShipClass.Leviathan:
						num += 9;
						break;
				}
			}
			return num;
		}

		public string GetGameName()
		{
			return this.GetNameValue("game_name");
		}

		public int GetRemainingSupportPoints(GameSession sim, int systemId, int playerId)
		{
			int num = 0;
			List<FleetInfo> list = this.GetFleetsBySupportingSystem(systemId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			foreach (MissionInfo missionInfo in this.GetMissionsBySystemDest(systemId).Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.RELOCATION)).ToList<MissionInfo>())
			{
				FleetInfo fleetInfo = this.GetFleetInfo(missionInfo.FleetID);
				if (fleetInfo != null && fleetInfo.PlayerID == playerId && (!Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(sim, fleetInfo) && fleetInfo.Type != FleetType.FL_CARAVAN) && !list.Contains(fleetInfo))
					list.Add(fleetInfo);
			}
			foreach (FleetInfo fleetInfo in list)
			{
				MissionInfo missionByFleetId = this.GetMissionByFleetID(fleetInfo.ID);
				if (fleetInfo.PlayerID == playerId && (fleetInfo.SupportingSystemID != systemId || missionByFleetId == null || missionByFleetId.Type != MissionType.RELOCATION))
					num += this.GetFleetCruiserEquivalent(fleetInfo.ID);
			}
			return this.GetSystemSupportedCruiserEquivalent(sim, systemId, playerId) - num;
		}

		public int GetSystemSupportedCruiserEquivalent(GameSession sim, int systemid, int playerId)
		{
			int num = sim.GameDatabase.GetColonyInfosForSystem(systemid).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x =>
		   {
			   if (x.PlayerID == playerId)
				   return x.CurrentStage != ColonyStage.Colony;
			   return false;
		   })).ToList<ColonyInfo>().Count * sim.AssetDatabase.ColonyFleetSupportPoints;
			foreach (StationInfo stationInfo in sim.GameDatabase.GetStationForSystemAndPlayer(systemid, playerId).ToList<StationInfo>())
				num += this.GetStationSupportedCruiserEquivalent(sim, stationInfo);
			return num;
		}

		private int GetStationSupportedCruiserEquivalent(GameSession sim, StationInfo stationInfo)
		{
			if (stationInfo.DesignInfo.StationType != StationType.NAVAL)
				return 0;
			int num = stationInfo.DesignInfo.DesignSections[0].Modules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
		   {
			   ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
			   if (stationModuleType.GetValueOrDefault() == ModuleEnums.StationModuleType.Warehouse)
				   return stationModuleType.HasValue;
			   return false;
		   })).ToList<DesignModuleInfo>().Count * 2;
			switch (stationInfo.DesignInfo.StationLevel)
			{
				case 1:
					return sim.AssetDatabase.StationLvl1FleetSupportPoints + num;
				case 2:
					return sim.AssetDatabase.StationLvl2FleetSupportPoints + num;
				case 3:
					return sim.AssetDatabase.StationLvl3FleetSupportPoints + num;
				case 4:
					return sim.AssetDatabase.StationLvl4FleetSupportPoints + num;
				case 5:
					return sim.AssetDatabase.StationLvl5FleetSupportPoints + num;
				default:
					return 0;
			}
		}

		public bool CanSupportPlanet(int playerId, int planetId)
		{
			ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(planetId);
			if (colonyInfoForPlanet != null)
			{
				PlanetInfo planetInfo = this.GetPlanetInfo(planetId);
				if (colonyInfoForPlanet.PlayerID == playerId)
				{
					if (this.GetStratModifier<bool>(StratModifiers.RequiresSterileEnvironment, playerId))
					{
						if (planetInfo.Biosphere > 0)
							return true;
					}
					else if ((double)this.GetPlanetHazardRating(playerId, planetId, false) != 0.0 || (double)planetInfo.Infrastructure != 1.0)
						return true;
				}
			}
			return false;
		}

		public bool CanInvadePlanet(int playerID, int planetId)
		{
			AIColonyIntel colonyIntelForPlanet = this.GetColonyIntelForPlanet(playerID, planetId);
			return colonyIntelForPlanet != null && playerID != colonyIntelForPlanet.OwningPlayerID;
		}

		public bool CanColonizePlanet(int playerID, int planetId, int MaxHazard)
		{
			PlanetInfo planetInfo = this.GetPlanetInfo(planetId);
			if (planetInfo != null && StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
			{
				bool flag1 = this.GetColonyIntelForPlanet(playerID, planetId) == null;
				bool flag2 = (double)this.GetPlanetHazardRating(playerID, planetId, true) <= (double)MaxHazard || this.assetdb.GetFaction(this.GetPlayerInfo(playerID).FactionID).Name == "loa";
				if (flag1 && flag2)
					return true;
			}
			return false;
		}

		public bool IsHazardousPlanet(int playerID, int planetId, int MaxHazard)
		{
			PlanetInfo planetInfo = this.GetPlanetInfo(planetId);
			return planetInfo == null || !StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()) || (double)this.GetPlanetHazardRating(playerID, planetId, true) > (double)MaxHazard;
		}

		public bool CanConstructStation(App game, int playerId, int systemId, bool canBuildGate)
		{
			if (!this.IsSurveyed(playerId, systemId) || !this.GetColonyInfosForSystem(systemId).Any<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerId)) && !this.GetStratModifier<bool>(StratModifiers.AllowDeepSpaceConstruction, playerId) && !this.GetColonyInfosForSystem(systemId).Any<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.IsIndependentColony(game))))
				return false;
			StationTypeFlags stationTypeFlags = (StationTypeFlags)0;
			for (int index = 0; index < 8; ++index)
				stationTypeFlags |= ((StationType)index).ToFlags();
			if (!canBuildGate)
				stationTypeFlags &= ~StationTypeFlags.GATE;
			for (int index = 0; index < 8; ++index)
			{
				if ((stationTypeFlags & ((StationType)index).ToFlags()) != (StationTypeFlags)0 && this.GetStationForSystemPlayerAndType(systemId, playerId, (StationType)index) == null)
					return true;
			}
			return false;
		}

		public bool CanPatrol(int playerID, int systemID)
		{
			if (this.GetLastTurnExploredByPlayer(playerID, systemID) < 1)
				return false;
			int? systemOwningPlayer = this.GetSystemOwningPlayer(systemID);
			int num = playerID;
			return systemOwningPlayer.GetValueOrDefault() == num & systemOwningPlayer.HasValue;
		}

		public bool CanInterdict(int playerID, int systemID)
		{
			return this.SystemContainsEnemyColony(playerID, systemID) || this.SystemContainsEnemyStation(playerID, systemID) && StarMap.IsInRange(this, playerID, systemID);
		}

		public bool CanStrike(int playerID, int systemID)
		{
			if (!this.GetSystemOwningPlayer(systemID).HasValue)
				return true;
			int? systemOwningPlayer = this.GetSystemOwningPlayer(systemID);
			int num = playerID;
			if (!(systemOwningPlayer.GetValueOrDefault() == num & systemOwningPlayer.HasValue))
				return this.IsStarSystemVisibleToPlayer(playerID, systemID);
			return false;
		}

		public bool CanInvade(int playerID, int systemID)
		{
			return this.SystemContainsEnemyColony(playerID, systemID);
		}

		public bool CanSupport(int playerID, int systemID)
		{
			return this.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>().Any<ColonyInfo>((Func<ColonyInfo, bool>)(x =>
		   {
			   if (x.PlayerID == playerID)
				   return this.CanSupportPlanet(x.PlayerID, x.OrbitalObjectID);
			   return false;
		   }));
		}

		public bool CanPirate(int playerID, int systemID)
		{
			int? systemOwningPlayer = this.GetSystemOwningPlayer(systemID);
			if (!systemOwningPlayer.HasValue || !this.GetStratModifier<bool>(StratModifiers.AllowPrivateerMission, playerID, false))
				return false;
			return systemOwningPlayer.Value != playerID;
		}

		public bool PirateFleetVisibleToPlayer(int fleetID, int PlayerID)
		{
			MissionInfo missionByFleetId = this.GetMissionByFleetID(fleetID);
			return missionByFleetId == null || missionByFleetId.Type != MissionType.PIRACY || (this.GetFleetInfo(fleetID).PlayerID == PlayerID || this.GetPiracyFleetDetectionInfoForFleet(fleetID).ToList<PiracyFleetDetectionInfo>().Any<PiracyFleetDetectionInfo>((Func<PiracyFleetDetectionInfo, bool>)(x => x.PlayerID == PlayerID)));
		}

		public bool SystemHasAccelerator(int systemid, int playerid)
		{
			bool flag = false;
			foreach (FleetInfo fleetInfo in this.GetFleetInfoBySystemID(systemid, FleetType.FL_ACCELERATOR))
			{
				if (fleetInfo.PlayerID == playerid)
				{
					flag = true;
					break;
				}
			}
			return flag;
		}

		public bool CanGate(int playerID, int systemID)
		{
			return !this.SystemHasGate(systemID, playerID);
		}

		public bool SystemHasGate(int systemid, int playerid)
		{
			bool flag = false;
			DataObjectCache.SystemPlayerID key = new DataObjectCache.SystemPlayerID()
			{
				SystemID = systemid,
				PlayerID = playerid
			};
			if (!this._dom.CachedSystemHasGateFlags.TryGetValue(key, out flag))
			{
				foreach (FleetInfo fleetInfo in this.GetFleetInfoBySystemID(systemid, FleetType.FL_GATE))
				{
					if (fleetInfo.PlayerID == playerid)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					foreach (StationInfo stationInfo in this.GetStationForSystemAndPlayer(systemid, playerid))
					{
						if (stationInfo.DesignInfo.StationType == StationType.GATE && stationInfo.DesignInfo.StationLevel > 0)
						{
							flag = true;
							break;
						}
					}
				}
				this._dom.CachedSystemHasGateFlags.Add(key, flag);
			}
			return flag;
		}

		public bool SystemContainsEnemyStation(int playerID, int systemID)
		{
			return this.GetStationForSystem(systemID).Where<StationInfo>((Func<StationInfo, bool>)(x => x.PlayerID != playerID)).Count<StationInfo>() > 0;
		}

		public bool SystemContainsEnemyColony(int playerID, int systemID)
		{
			foreach (OrbitalObjectInfo orbitalObjectInfo in this.GetStarSystemOrbitalObjectInfos(systemID))
			{
				AIColonyIntel colonyIntelForPlanet = this.GetColonyIntelForPlanet(playerID, orbitalObjectInfo.ID);
				if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != playerID && !this.GetPlayerInfo(playerID).IsOnTeam(this.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID)))
					return true;
			}
			return false;
		}

		public int GetShipCommandPointQuota(int shipId)
		{
			return this.GetDesignCommandPointQuota(this.assetdb, this.GetShipInfo(shipId, false).DesignID);
		}

		public int GetDesignCommandPointQuota(AssetDatabase adb, int designID)
		{
			int val1 = 0;
			DesignInfo designInfo = this.GetDesignInfo(designID);
			int num = 0;
			if (this.PlayerHasTech(designInfo.PlayerID, "CCC_Combat_Algorithms"))
				num += adb.GetTechBonus<int>("CCC_Combat_Algorithms", "commandpoints");
			if (this.PlayerHasTech(designInfo.PlayerID, "CCC_Flag_Central_Command"))
				num += adb.GetTechBonus<int>("CCC_Flag_Central_Command", "commandpoints");
			if (this.PlayerHasTech(designInfo.PlayerID, "CCC_Holo-Tactics"))
				num += adb.GetTechBonus<int>("CCC_Holo-Tactics", "commandpoints");
			if (this.PlayerHasTech(designInfo.PlayerID, "PSI_Warmind"))
				num += adb.GetTechBonus<int>("PSI_Warmind", "commandpoints");
			if (designInfo != null)
			{
				foreach (DesignSectionInfo designSection in designInfo.DesignSections)
				{
					foreach (DesignModuleInfo module in designSection.Modules)
					{
						ModuleEnums.StationModuleType? stationModuleType = module.StationModuleType;
						if ((stationModuleType.GetValueOrDefault() != ModuleEnums.StationModuleType.Command ? 0 : (stationModuleType.HasValue ? 1 : 0)) != 0)
							++num;
					}
					val1 = Math.Max(val1, adb.GetShipSectionAsset(designSection.FilePath).CommandPoints);
				}
			}
			if (val1 != 0)
				return val1 + num;
			return 0;
		}

		public int GetCommandPointCost(int designID)
		{
			return this.GetDesignInfo(designID).CommandPointCost;
		}

		public int GetShipsCommandPointQuota(IEnumerable<ShipInfo> ships)
		{
			int val1 = 0;
			foreach (ShipInfo ship in ships)
				val1 = Math.Max(val1, this.GetShipCommandPointQuota(ship.ID));
			return val1;
		}

		public int GetShipsCommandPointCost(IEnumerable<ShipInfo> ships)
		{
			int num = 0;
			foreach (ShipInfo ship in ships)
				num += this.GetShipCommandPointCost(ship.ID, false);
			return num;
		}

		public int GetFleetCommandPointQuota(int fleetid)
		{
			return this.GetShipsCommandPointQuota(this.GetShipInfoByFleetID(fleetid, false));
		}

		public int GetFleetCommandPointCost(int fleetid)
		{
			return this.GetShipsCommandPointCost(this.GetShipInfoByFleetID(fleetid, false));
		}

		public int GetShipCommandPointCost(int shipID, bool battleriders)
		{
			int num = 0;
			switch (this.GetDesignInfo(this.GetShipInfo(shipID, false).DesignID).Class)
			{
				case ShipClass.Cruiser:
					num = 6;
					break;
				case ShipClass.Dreadnought:
					num = 18;
					break;
				case ShipClass.Leviathan:
					num = 54;
					break;
				case ShipClass.BattleRider:
					num = 0;
					break;
			}
			return num;
		}

		public bool IsPlayerAdjacent(GameSession game, int toPlayerID, int otherPlayerID)
		{
			float driveSpeedForPlayer = this.FindCurrentDriveSpeedForPlayer(toPlayerID);
			float range = !this.GetFactionName(this.GetPlayerFactionID(toPlayerID)).Contains("hiver") ? driveSpeedForPlayer * 2f : driveSpeedForPlayer * 12f;
			List<StarSystemInfo> starSystemInfoList = new List<StarSystemInfo>();
			foreach (StarSystemInfo starSystemInfo in game.GameDatabase.GetStarSystemInfos())
			{
				if (this.IsStarSystemVisibleToPlayer(toPlayerID, starSystemInfo.ID) || this.GetLastTurnExploredByPlayer(toPlayerID, starSystemInfo.ID) > 0)
				{
					foreach (PlanetInfo systemPlanetInfo in game.GameDatabase.GetStarSystemPlanetInfos(starSystemInfo.ID))
					{
						ColonyInfo colonyInfo = game.GameDatabase.GetColonyInfo(systemPlanetInfo.ID);
						if (colonyInfo != null && colonyInfo.PlayerID == otherPlayerID || (this.SystemHasGate(starSystemInfo.ID, otherPlayerID) || this.SystemHasAccelerator(starSystemInfo.ID, otherPlayerID)))
						{
							starSystemInfoList.Add(starSystemInfo);
							break;
						}
					}
				}
			}
			foreach (StellarInfo stellarInfo in starSystemInfoList)
			{
				foreach (StarSystemInfo starSystemInfo in this.GetSystemsInRange(stellarInfo.Origin, range))
				{
					if (this.SystemHasGate(starSystemInfo.ID, toPlayerID) || this.SystemHasAccelerator(starSystemInfo.ID, toPlayerID))
						return true;
					foreach (PlanetInfo systemPlanetInfo in this.GetStarSystemPlanetInfos(starSystemInfo.ID))
					{
						ColonyInfo colonyInfoForPlanet = this.GetColonyInfoForPlanet(systemPlanetInfo.ID);
						if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == toPlayerID)
							return true;
					}
				}
			}
			return false;
		}

		public DiplomacyState GetDiplomacyStateBetweenPlayers(
		  int playerID,
		  int otherPlayerID)
		{
			if (playerID == 0 || otherPlayerID == 0)
				return DiplomacyState.WAR;
			DiplomacyState state1 = this.GetDiplomacyInfo(playerID, otherPlayerID).State;
			DiplomacyState state2 = this.GetDiplomacyInfo(otherPlayerID, playerID).State;
			if (state1 != state2)
				throw new InvalidDataException("Inconsistency in player diplomacy states");
			return state1;
		}

		public bool ShouldPlayersFight(int playerID1, int playerID2, int systemID)
		{
			if (GameDatabase.PlayersAlwaysFight && playerID1 != playerID2)
				return true;
			switch (this.GetDiplomacyStateBetweenPlayers(playerID1, playerID2))
			{
				case DiplomacyState.UNKNOWN:
				case DiplomacyState.WAR:
					return true;
				default:
					return false;
			}
		}

		public float FindCurrentDriveSpeedForPlayer(int playerID)
		{
			float num = -1f;
			foreach (DesignInfo designInfo in this.GetDesignInfosForPlayer(playerID))
			{
				foreach (ShipSectionAsset shipSectionAsset in ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)))
				{
					if (shipSectionAsset != null)
					{
						if ((double)shipSectionAsset.NodeSpeed > 0.0 && (double)shipSectionAsset.NodeSpeed > (double)num)
							num = shipSectionAsset.NodeSpeed;
						else if ((double)shipSectionAsset.FtlSpeed > 0.0 && (double)shipSectionAsset.FtlSpeed > (double)num)
							num = shipSectionAsset.FtlSpeed;
					}
				}
			}
			return num;
		}

		public float GetHiverCastingDistance(int playerId)
		{
			Random safeRandom = App.GetSafeRandom();
			float stratModifier = this.GetStratModifier<float>(StratModifiers.GateCastDistance, playerId);
			float num = stratModifier * this.GetStratModifier<float>(StratModifiers.GateCastDeviation, playerId);
			return stratModifier + (float)(-(double)num + (double)safeRandom.NextSingle() * ((double)num * 2.0));
		}

		public IEnumerable<StarSystemInfo> GetSystemsInRange(
		  Vector3 from,
		  float range)
		{
			List<StarSystemInfo> starSystemInfoList = new List<StarSystemInfo>();
			foreach (StarSystemInfo starSystemInfo in this.GetStarSystemInfos())
			{
				if ((double)(from - starSystemInfo.Origin).Length <= (double)range)
					starSystemInfoList.Add(starSystemInfo);
			}
			return (IEnumerable<StarSystemInfo>)starSystemInfoList;
		}

		public bool PlayerHasTech(int playerID, string techName)
		{
			int techId = this.GetTechID(techName);
			if (techId > 0)
			{
				PlayerTechInfo playerTechInfo = this.GetPlayerTechInfo(playerID, techId);
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
					return true;
			}
			return this.GetResearchedTechGroups(playerID).Contains(techName);
		}

		public bool PlayerHasAntimatter(int playerID)
		{
			return this.PlayerHasTech(playerID, "NRG_Anti-Matter");
		}

		public bool PlayerHasDreadnoughts(int playerID)
		{
			return this.PlayerHasTech(playerID, "ENG_Dreadnought_Construction");
		}

		public bool PlayerHasLeviathans(int playerID)
		{
			return this.PlayerHasTech(playerID, "ENG_Leviathian_Construction");
		}

		public float GetLaggingTechWeightBonus(int playerID, string family)
		{
			double num1 = 0.0;
			double num2 = 0.0;
			int num3 = 0;
			foreach (AITechWeightInfo aiTechWeightInfo in this.GetAITechWeightInfos(playerID))
			{
				num2 += aiTechWeightInfo.TotalSpent;
				++num3;
				if (aiTechWeightInfo.Family == family)
					num1 = aiTechWeightInfo.TotalSpent;
			}
			if (num3 > 0)
				num2 /= (double)num3;
			if (num1 < num2 * 0.5)
				return 1.5f;
			return num1 < num2 ? 1.2f : 1f;
		}

		public static ShipSectionAsset GetDesignSection(
		  GameSession game,
		  int designID,
		  ShipSectionType sectionType)
		{
			foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(designID))
			{
				string sectionName = designSectionName;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
				if (shipSectionAsset != null && shipSectionAsset.Type == sectionType)
					return shipSectionAsset;
			}
			return (ShipSectionAsset)null;
		}

		public string GetDefaultShipName(int designID, int serial)
		{
			DesignInfo designInfo = this.GetDesignInfo(designID);
			string str = designInfo.Name;
			if (str == null)
			{
				switch (designInfo.Class)
				{
					case ShipClass.Cruiser:
						str = "CR";
						break;
					case ShipClass.Dreadnought:
						str = "DN";
						break;
					case ShipClass.Leviathan:
						str = "LV";
						break;
					case ShipClass.BattleRider:
						str = "DE";
						break;
				}
				str = str + "-" + (object)designID;
			}
			return str + "-" + (object)serial;
		}

		public float ScoreSystemAsPeripheral(int systemID, int playerID)
		{
			Vector3 zero = Vector3.Zero;
			int num1 = 0;
			foreach (ColonyInfo colonyInfo in this.GetPlayerColoniesByPlayerId(playerID))
			{
				Vector3 origin = this.GetStarSystemInfo(this.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID).Origin;
				zero += origin;
				++num1;
			}
			if (num1 == 0)
				return 0.0f;
			Vector3 vector3 = zero / (float)num1;
			float num2 = 0.0f;
			foreach (ColonyInfo colonyInfo in this.GetPlayerColoniesByPlayerId(playerID))
			{
				Vector3 origin = this.GetStarSystemInfo(this.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID).Origin;
				num2 += (origin - vector3).Length;
			}
			float num3 = num2 / (float)num1;
			Vector3 starSystemOrigin = this.GetStarSystemOrigin(systemID);
			return (vector3 - starSystemOrigin).Length / num3;
		}

		public string GetMapName()
		{
			return this.GetNameValue("map_name");
		}

		public double GetUnixTimeCreated()
		{
			string nameValue = this.GetNameValue("time_stamp");
			if (string.IsNullOrEmpty(nameValue))
				return 0.0;
			return double.Parse(nameValue);
		}

		public enum TurnLengthTypes
		{
			Strategic,
			Combat,
		}
	}
}
