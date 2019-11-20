// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapLobby
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPLOBBY)]
	internal class StarMapLobby : StarMapBase
	{
		private List<ServerInfo> _servers = new List<ServerInfo>();
		public readonly SyncMap<StarMapServer, ServerInfo, StarMapBase.SyncContext> Servers = new SyncMap<StarMapServer, ServerInfo, StarMapBase.SyncContext>(new Func<GameObjectSet, ServerInfo, StarMapBase.SyncContext, StarMapServer>(StarMapLobby.CreateServer), (Action<StarMapServer, ServerInfo, StarMapBase.SyncContext>)null);

		public StarMapLobby(App game, Sky sky)
		  : base(game, sky)
		{
		}

		public void AddServer(GameObjectSet gos, ServerInfo si)
		{
			this._servers.Add(si);
			this.PostObjectAddObjects((IGameObject[])this.Servers.Sync(gos, (IEnumerable<ServerInfo>)this._servers, (StarMapBase.SyncContext)null, false).ToArray<StarMapServer>());
		}

		protected override void GetAdditionalParams(List<object> parms)
		{
		}

		protected static StarMapServer CreateServer(
		  GameObjectSet gos,
		  ServerInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapServer starMapServer = new StarMapServer(gos.App, oi.Origin, oi.name, oi.map, oi.version, oi.players, oi.maxPlayers, oi.ping, oi.passworded);
			gos.Add((IGameObject)starMapServer);
			return starMapServer;
		}

		protected override StarMapProp CreateProp(
		  GameObjectSet gos,
		  StellarPropInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapProp starMapProp = new StarMapProp(gos.App, oi.AssetPath, oi.Transform.Position, oi.Transform.EulerAngles, 1f);
			gos.Add((IGameObject)starMapProp);
			return starMapProp;
		}

		protected override StarMapTerrain CreateTerrain(
		  GameObjectSet gos,
		  TerrainInfo oi,
		  StarMapBase.SyncContext context)
		{
			StarMapTerrain starMapTerrain = new StarMapTerrain(gos.App, oi.Origin, oi.Name);
			gos.Add((IGameObject)starMapTerrain);
			return starMapTerrain;
		}

		protected override StarMapSystem CreateSystem(
		  GameObjectSet gos,
		  StarSystemInfo oi,
		  StarMapBase.SyncContext context)
		{
			StellarClass stellarClass = StellarClass.Parse(oi.StellarClass);
			StarDisplayParams displayParams = StarHelper.GetDisplayParams(stellarClass);
			StarMapSystem starMapSystem = new StarMapSystem(gos.App, displayParams.AssetPath, oi.Origin, StarHelper.CalcRadius(stellarClass.Size) / StarSystemVars.Instance.StarRadiusIa, oi.Name);
			gos.Add((IGameObject)starMapSystem);
			return starMapSystem;
		}

		protected override void OnInitialize(GameObjectSet gos, params object[] parms)
		{
		}

		public void ClearServers(GameObjectSet gos)
		{
			this._servers.Clear();
			this.Servers.Sync(gos, (IEnumerable<ServerInfo>)this._servers, (StarMapBase.SyncContext)null, false);
		}
	}
}
