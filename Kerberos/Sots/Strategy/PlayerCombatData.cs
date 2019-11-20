// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.PlayerCombatData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class PlayerCombatData
	{
		private int _playerID;
		private int _fleetCount;
		private GameSession.VictoryStatus _victoryStatus;
		private List<Kerberos.Sots.Strategy.WeaponData> _weaponData;
		private List<Kerberos.Sots.Strategy.PlanetData> _planetData;
		private List<Kerberos.Sots.Strategy.ShipData> _shipData;

		private void Construct()
		{
			this._weaponData = new List<Kerberos.Sots.Strategy.WeaponData>();
			this._planetData = new List<Kerberos.Sots.Strategy.PlanetData>();
			this._shipData = new List<Kerberos.Sots.Strategy.ShipData>();
		}

		public PlayerCombatData(int playerID)
		{
			this._playerID = playerID;
			this.Construct();
		}

		public PlayerCombatData(ScriptMessageReader mr, int version)
		{
			this._playerID = mr.ReadInteger();
			this._victoryStatus = (GameSession.VictoryStatus)mr.ReadInteger();
			this.Construct();
			int num1 = mr.ReadInteger();
			for (int index = 0; index < num1; ++index)
				this._shipData.Add(new Kerberos.Sots.Strategy.ShipData()
				{
					designID = mr.ReadInteger(),
					damageDealt = mr.ReadSingle(),
					damageReceived = mr.ReadSingle(),
					killCount = mr.ReadInteger(),
					destroyed = mr.ReadBool()
				});
			int num2 = mr.ReadInteger();
			for (int index1 = 0; index1 < num2; ++index1)
			{
				Kerberos.Sots.Strategy.PlanetData planetData = new Kerberos.Sots.Strategy.PlanetData();
				planetData.orbitalObjectID = mr.ReadInteger();
				planetData.imperialDamage = mr.ReadDouble();
				int num3 = mr.ReadInteger();
				planetData.civilianDamage = new List<PopulationData>();
				for (int index2 = 0; index2 < num3; ++index2)
				{
					PopulationData populationData = new PopulationData()
					{
						faction = mr.ReadString(),
						damage = mr.ReadDouble()
					};
				}
				planetData.infrastructureDamage = mr.ReadSingle();
				planetData.terraDamage = mr.ReadSingle();
				this._planetData.Add(planetData);
			}
			int num4 = mr.ReadInteger();
			for (int index = 0; index < num4; ++index)
				this._weaponData.Add(new Kerberos.Sots.Strategy.WeaponData()
				{
					weaponID = mr.ReadInteger(),
					damageDealt = mr.ReadSingle()
				});
			if (version >= 1)
				this._fleetCount = mr.ReadInteger();
			else
				this._fleetCount = 0;
		}

		public void AddWeaponData(int ID, float damage)
		{
			this._weaponData.Add(new Kerberos.Sots.Strategy.WeaponData()
			{
				weaponID = ID,
				damageDealt = damage
			});
		}

		public void AddShipData(
		  int designID,
		  float damageDealt,
		  float damageReceived,
		  int kills,
		  bool destroyed)
		{
			this._shipData.Add(new Kerberos.Sots.Strategy.ShipData()
			{
				designID = designID,
				damageDealt = damageDealt,
				damageReceived = damageReceived,
				killCount = kills,
				destroyed = destroyed
			});
		}

		public void AddPlanetData(
		  int orbitalObjectID,
		  float terraDamage,
		  float infraDamage,
		  double imperialDamage,
		  List<PopulationData> civilianDamage)
		{
			this._planetData.Add(new Kerberos.Sots.Strategy.PlanetData()
			{
				imperialDamage = imperialDamage,
				civilianDamage = civilianDamage,
				orbitalObjectID = orbitalObjectID,
				terraDamage = terraDamage,
				infrastructureDamage = infraDamage
			});
		}

		public List<object> ToList()
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)this._playerID);
			objectList.Add((object)(int)this._victoryStatus);
			objectList.Add((object)this._shipData.Count<Kerberos.Sots.Strategy.ShipData>());
			foreach (Kerberos.Sots.Strategy.ShipData shipData in this._shipData)
			{
				objectList.Add((object)shipData.designID);
				objectList.Add((object)shipData.damageDealt);
				objectList.Add((object)shipData.damageReceived);
				objectList.Add((object)shipData.killCount);
				objectList.Add((object)shipData.destroyed);
			}
			objectList.Add((object)this._planetData.Count<Kerberos.Sots.Strategy.PlanetData>());
			foreach (Kerberos.Sots.Strategy.PlanetData planetData in this._planetData)
			{
				objectList.Add((object)planetData.orbitalObjectID);
				objectList.Add((object)planetData.imperialDamage);
				objectList.Add((object)planetData.civilianDamage.Count<PopulationData>());
				foreach (PopulationData populationData in planetData.civilianDamage)
				{
					objectList.Add((object)populationData.faction);
					objectList.Add((object)populationData.damage);
				}
				objectList.Add((object)planetData.infrastructureDamage);
				objectList.Add((object)planetData.terraDamage);
			}
			objectList.Add((object)this._weaponData.Count<Kerberos.Sots.Strategy.WeaponData>());
			foreach (Kerberos.Sots.Strategy.WeaponData weaponData in this._weaponData)
			{
				objectList.Add((object)weaponData.weaponID);
				objectList.Add((object)weaponData.damageDealt);
			}
			objectList.Add((object)this._fleetCount);
			return objectList;
		}

		public int PlayerID
		{
			get
			{
				return this._playerID;
			}
		}

		public int FleetCount
		{
			get
			{
				return this._fleetCount;
			}
			set
			{
				this._fleetCount = value;
			}
		}

		public GameSession.VictoryStatus VictoryStatus
		{
			get
			{
				return this._victoryStatus;
			}
			set
			{
				this._victoryStatus = value;
			}
		}

		public List<Kerberos.Sots.Strategy.WeaponData> WeaponData
		{
			get
			{
				return this._weaponData;
			}
		}

		public List<Kerberos.Sots.Strategy.PlanetData> PlanetData
		{
			get
			{
				return this._planetData;
			}
		}

		public List<Kerberos.Sots.Strategy.ShipData> ShipData
		{
			get
			{
				return this._shipData;
			}
		}
	}
}
