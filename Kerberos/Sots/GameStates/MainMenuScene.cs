// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.MainMenuScene
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
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
	internal class MainMenuScene
	{
		private Dictionary<string, Player> _players = new Dictionary<string, Player>()
	{
	  {
		"human",
		(Player) null
	  },
	  {
		"hiver",
		(Player) null
	  },
	  {
		"morrigi",
		(Player) null
	  },
	  {
		"tarkas",
		(Player) null
	  },
	  {
		"zuul",
		(Player) null
	  },
	  {
		"liir_zuul",
		(Player) null
	  },
	  {
		"loa",
		(Player) null
	  }
	};
		private Dictionary<string, CombatAI> _combatAIs = new Dictionary<string, CombatAI>();
		private Random _rand = new Random();
		private GameObjectSet _set;
		private List<IGameObject> _postLoadedObjects;
		private OrbitCameraController _camera;
		private CombatInput _input;
		private bool _ready;
		private bool _active;
		private App _app;
		private StarSystem _starsystem;
		private DateTime _nextSwitchTime;

		public Vector3 CreateRandomShip(Vector3 off, string Faction = "", bool ForceDread = false)
		{
			Vector3 zero = Vector3.Zero;
			List<WeaponEnums.PayloadTypes> weaponTypes = new List<WeaponEnums.PayloadTypes>();
			weaponTypes.Add(WeaponEnums.PayloadTypes.Beam);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Bolt);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Emitter);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Missile);
			weaponTypes.Add(WeaponEnums.PayloadTypes.Torpedo);
			IEnumerable<LogicalWeapon> logicalWeapons = this._app.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.PayloadType == weaponTypes[new Random().Next(weaponTypes.Count<WeaponEnums.PayloadTypes>() - 1)]));
			KeyValuePair<string, Player> chosenPlayer = this._rand.Choose<KeyValuePair<string, Player>>((IEnumerable<KeyValuePair<string, Player>>)this._players);
			if (Faction != string.Empty && this._players.ContainsKey(Faction))
				chosenPlayer = this._players.FirstOrDefault<KeyValuePair<string, Player>>((Func<KeyValuePair<string, Player>, bool>)(x => x.Key == Faction));
			List<ShipSectionAsset> shipSectionAssetList1 = new List<ShipSectionAsset>()
	  {
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\dn_cmd_assault.section", (object) chosenPlayer.Key)))
	  };
			List<ShipSectionAsset> shipSectionAssetList2 = new List<ShipSectionAsset>()
	  {
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\dn_mis_armor.section", (object) chosenPlayer.Key)))
	  };
			List<ShipSectionAsset> shipSectionAssetList3 = new List<ShipSectionAsset>()
	  {
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\dn_eng_fusion.section", (object) chosenPlayer.Key)))
	  };
			List<ShipSectionAsset> shipSectionAssetList4 = new List<ShipSectionAsset>()
	  {
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_cmd.section", (object) chosenPlayer.Key))),
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_cmd_assault.section", (object) chosenPlayer.Key))),
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_cmd_hammerhead.section", (object) chosenPlayer.Key)))
	  };
			List<ShipSectionAsset> shipSectionAssetList5 = new List<ShipSectionAsset>()
	  {
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_armor.section", (object) chosenPlayer.Key))),
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_blazer.section", (object) chosenPlayer.Key))),
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_cnc.section", (object) chosenPlayer.Key))),
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_barrage.section", (object) chosenPlayer.Key))),
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_mis_projector.section", (object) chosenPlayer.Key)))
	  };
			List<ShipSectionAsset> shipSectionAssetList6 = new List<ShipSectionAsset>()
	  {
		this._app.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>) (x => x.FileName == string.Format("factions\\{0}\\sections\\cr_eng_fusion.section", (object) chosenPlayer.Key)))
	  };
			ShipSectionAsset[] shipSectionAssetArray = new ShipSectionAsset[3];
			if (this._rand.CoinToss(0.15) || ForceDread)
			{
				shipSectionAssetArray[0] = this._rand.Choose<ShipSectionAsset>((IList<ShipSectionAsset>)shipSectionAssetList2);
				shipSectionAssetArray[1] = this._rand.Choose<ShipSectionAsset>((IList<ShipSectionAsset>)shipSectionAssetList1);
				shipSectionAssetArray[2] = this._rand.Choose<ShipSectionAsset>((IList<ShipSectionAsset>)shipSectionAssetList3);
			}
			else
			{
				shipSectionAssetArray[0] = this._rand.Choose<ShipSectionAsset>((IList<ShipSectionAsset>)shipSectionAssetList4);
				shipSectionAssetArray[1] = this._rand.Choose<ShipSectionAsset>((IList<ShipSectionAsset>)shipSectionAssetList5);
				shipSectionAssetArray[2] = this._rand.Choose<ShipSectionAsset>((IList<ShipSectionAsset>)shipSectionAssetList6);
			}
			Ship ship = Ship.CreateShip(this._app, new CreateShipParams()
			{
				player = chosenPlayer.Value,
				sections = (IEnumerable<ShipSectionAsset>)shipSectionAssetArray,
				turretHousings = this._app.AssetDatabase.TurretHousings,
				weapons = this._app.AssetDatabase.Weapons,
				preferredWeapons = logicalWeapons,
				psionics = this._app.AssetDatabase.Psionics,
				faction = this._app.AssetDatabase.Factions.First<Faction>((Func<Faction, bool>)(x => x.Name == chosenPlayer.Key))
			});
			Vector3 vector3;
			ship.Position = vector3 = off;
			this._set.Add((IGameObject)ship);
			return vector3;
		}

		public void Enter(App app)
		{
			this._app = app;
			if (this._app.GameDatabase == null)
				this._app.NewGame();
			app.Game.SetLocalPlayer(app.GetPlayer(1));
			this._set = new GameObjectSet(app);
			this._postLoadedObjects = new List<IGameObject>();
			this._set.Add((IGameObject)new Sky(app, SkyUsage.InSystem, new Random().Next()));
			if (ScriptHost.AllowConsole)
				this._input = this._set.Add<CombatInput>();
			this._camera = this._set.Add<OrbitCameraController>();
			this._camera.SetAttractMode(true);
			this._camera.TargetPosition = new Vector3(500000f, 0.0f, 0.0f);
			this._camera.MinDistance = 1f;
			this._camera.MaxDistance = 11000f;
			this._camera.DesiredDistance = 11000f;
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-2f);
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(45f);
			int systemId = 0;
			IEnumerable<HomeworldInfo> homeworlds = this._app.GameDatabase.GetHomeworlds();
			HomeworldInfo homeworldInfo = homeworlds.FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.PlayerID == app.LocalPlayer.ID));
			if (homeworldInfo != null)
				systemId = homeworldInfo.SystemID;
			else if (homeworlds.Count<HomeworldInfo>() > 0)
				systemId = homeworlds.ElementAt<HomeworldInfo>(new Random().NextInclusive(0, homeworlds.Count<HomeworldInfo>() - 1)).SystemID;
			this._starsystem = new StarSystem(this._app, 1f, systemId, new Vector3(0.0f, 0.0f, 0.0f), false, (CombatSensor)null, true, 0, false, true);
			this._set.Add((IGameObject)this._starsystem);
			this._starsystem.PostSetProp("InputEnabled", false);
			this._starsystem.PostSetProp("RenderSuroundingItems", false);
			Vector3 vector1 = new Vector3();
			float num1 = 10000f;
			IEnumerable<PlanetInfo> infosOrbitingStar = this._app.GameDatabase.GetPlanetInfosOrbitingStar(systemId);
			bool flag1 = false;
			foreach (PlanetInfo planetInfo in infosOrbitingStar)
			{
				if (planetInfo != null)
				{
					ColonyInfo colonyInfoForPlanet = this._app.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
					if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this._app.LocalPlayer.ID)
					{
						vector1 = this._app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position;
						num1 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
						flag1 = true;
						break;
					}
				}
			}
			if (!flag1)
			{
				PlanetInfo[] array = infosOrbitingStar.ToArray<PlanetInfo>();
				if (array.Length > 0)
				{
					PlanetInfo planetInfo = array[new Random().Next(array.Length)];
					vector1 = this._app.GameDatabase.GetOrbitalTransform(planetInfo.ID).Position;
					num1 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
				}
			}
			this._camera.DesiredYaw = -(float)Math.Atan2((double)vector1.Z, (double)vector1.X);
			this._camera.TargetPosition = vector1;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this._camera.DesiredYaw, 0.0f, 0.0f);
			Vector3[] shuffledPlayerColors = Player.GetShuffledPlayerColors(this._rand);
			foreach (string index in this._players.Keys.ToList<string>())
			{
				this._players[index] = new Player(app, app.Game, new PlayerInfo()
				{
					FactionID = app.GameDatabase.GetFactionIdFromName(index),
					AvatarAssetPath = string.Empty,
					BadgeAssetPath = app.AssetDatabase.GetRandomBadgeTexture(index, this._rand),
					PrimaryColor = shuffledPlayerColors[0],
					SecondaryColor = new Vector3(this._rand.NextSingle(), this._rand.NextSingle(), this._rand.NextSingle())
				}, Player.ClientTypes.AI);
				this._set.Add((IGameObject)this._players[index]);
			}
			Vector3 zero = Vector3.Zero;
			double num2 = (double)Vector3.Cross(vector1, new Vector3(0.0f, 1f, 0.0f)).Normalize();
			float num3 = 500f;
			int num4 = 4;
			float num5 = num3 * (float)num4;
			Vector3 vector3_1 = new Vector3(-num5, 0.0f, -num5);
			Vector3 vector3_2 = vector1 + -rotationYpr.Forward * (num1 + 2000f + num5);
			List<Vector3> vector3List1 = new List<Vector3>();
			for (int index = 0; index < 81; ++index)
			{
				int num6 = index % 5;
				int num7 = index / 5;
				Vector3 vector3_3 = new Vector3(vector3_1.X + (float)num7 * num3, 0.0f, vector3_1.Z + (float)num6 * num3);
				vector3_3 += vector3_2;
				vector3List1.Add(vector3_3);
			}
			List<Vector3> vector3List2 = new List<Vector3>();
			foreach (Vector3 pos in vector3List1)
			{
				if (this.PositionCollidesWithObject(pos, 400f))
					vector3List2.Add(pos);
			}
			foreach (Vector3 vector3_3 in vector3List2)
				vector3List1.Remove(vector3_3);
			int num8 = this._rand.NextInclusive(6, 12);
			List<int> intList = new List<int>();
			for (int index1 = 0; index1 < num8; ++index1)
			{
				int index2 = 0;
				bool flag2 = true;
				for (int index3 = 0; flag2 && index3 < vector3List1.Count; ++index3)
				{
					index2 = this._rand.NextInclusive(0, Math.Max(vector3List1.Count - 1, 0));
					flag2 = intList.Contains(index2);
					if (intList.Count == vector3List1.Count)
						break;
				}
				Vector3 off = vector3List1.Count > 0 ? vector3List1[index2] : vector1;
				if (index1 < 3)
					zero += this.CreateRandomShip(off, "loa", index1 == 0);
				else
					zero += this.CreateRandomShip(off, "", false);
				if (!intList.Contains(index2))
					intList.Add(index2);
			}
			if (num8 <= 0)
				return;
			Vector3 vector3_4 = zero / (float)num8;
		}

		private bool PositionCollidesWithObject(Vector3 pos, float safeRadius)
		{
			foreach (IGameObject gameObject in this._starsystem.Crits.Objects)
			{
				bool flag = false;
				float num1 = 0.0f;
				Vector3 vector3 = Vector3.Zero;
				if (gameObject is StellarBody)
				{
					num1 = StarSystemVars.Instance.SizeToRadius((gameObject as StellarBody).PlanetInfo.Size) + 2000f;
					vector3 = (gameObject as StellarBody).Parameters.Position;
					flag = true;
				}
				else if (gameObject is Ship)
				{
					num1 = 1000f;
					vector3 = (gameObject as Ship).Position;
					flag = true;
				}
				if (flag)
				{
					float num2 = safeRadius + num1;
					if ((double)(pos - vector3).LengthSquared < (double)num2 * (double)num2)
						return true;
				}
			}
			return false;
		}

		public void Activate()
		{
			this._active = true;
		}

		public bool IsReady()
		{
			return this._set.IsReady();
		}

		public void Update()
		{
			if (this._active && !this._ready && this._set.IsReady())
			{
				this._ready = true;
				this._set.Activate();
				foreach (string key in this._players.Keys)
					this._combatAIs.Add(key, new CombatAI(this._app, this._players[key], false, this._starsystem, (Dictionary<int, DiplomacyState>)null, true));
			}
			if (!this._ready)
				return;
			List<IGameObject> gameObjectList = new List<IGameObject>();
			foreach (IGameObject postLoadedObject in this._postLoadedObjects)
			{
				if (postLoadedObject.ObjectStatus == GameObjectStatus.Ready)
				{
					if (postLoadedObject is IActive)
						(postLoadedObject as IActive).Active = true;
					this._set.Add(postLoadedObject);
					gameObjectList.Add(postLoadedObject);
				}
			}
			foreach (IGameObject gameObject in gameObjectList)
				this._postLoadedObjects.Remove(gameObject);
			DateTime now = DateTime.Now;
			if (now >= this._nextSwitchTime)
				this._nextSwitchTime = now + TimeSpan.FromSeconds(5.0);
			List<IGameObject> combatGameObjects = CombatAI.GetCombatGameObjects((IEnumerable<IGameObject>)this._set);
			foreach (CombatAI combatAi in this._combatAIs.Values)
				combatAi.Update(combatGameObjects);
		}

		public void Exit()
		{
			foreach (CombatAI combatAi in this._combatAIs.Values)
				combatAi?.Shutdown();
			this._combatAIs.Clear();
			foreach (IGameObject state in this._set.Objects.Where<IGameObject>((Func<IGameObject, bool>)(x => x is Ship)))
				state.PostSetProp("CombatHasEnded");
			this._set.Dispose();
		}

		public void AddObject(ScriptMessageReader data)
		{
			InteropGameObjectType interopGameObjectType = (InteropGameObjectType)data.ReadInteger();
			IGameObject gameObject = (IGameObject)null;
			if (gameObject == null)
				return;
			this._postLoadedObjects.Add(gameObject);
		}

		public void RemoveObject(ScriptMessageReader data)
		{
			int id = data.ReadInteger();
			IGameObject gameObject1 = this._set.Objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x =>
		   {
			   if (x.ObjectID == id)
				   return x is IDisposable;
			   return false;
		   }));
			if (gameObject1 != null)
			{
				(gameObject1 as IDisposable).Dispose();
				this._set.Remove(gameObject1);
			}
			else
			{
				IGameObject gameObject2 = this._app.GetGameObject(id);
				if (gameObject2 == null)
					return;
				this._app.ReleaseObject(gameObject2);
			}
		}

		public void RemoveObjects(ScriptMessageReader data)
		{
			for (int id = data.ReadInteger(); id != 0; id = data.ReadInteger())
			{
				IGameObject gameObject1 = this._set.Objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x =>
			   {
				   if (x.ObjectID == id)
					   return x is IDisposable;
				   return false;
			   }));
				if (gameObject1 != null)
				{
					(gameObject1 as IDisposable).Dispose();
					this._set.Remove(gameObject1);
				}
				else
				{
					IGameObject gameObject2 = this._app.GetGameObject(id);
					if (gameObject2 != null)
						this._app.ReleaseObject(gameObject2);
				}
			}
		}
	}
}
