// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapBase
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
	internal abstract class StarMapBase : GameObject, IDisposable, IActive
	{
		private bool _selectEnabled = true;
		private bool _focusEnabled = true;
		private bool _mouseOverEnabled = true;
		public readonly SyncMap<StarMapProp, StellarPropInfo, StarMapBase.SyncContext> Props;
		public readonly SyncMap<StarMapSystem, StarSystemInfo, StarMapBase.SyncContext> Systems;
		public readonly SyncMap<StarMapFleet, FleetInfo, StarMapBase.SyncContext> Fleets;
		public readonly SyncMap<StarMapProvince, ProvinceInfo, StarMapBase.SyncContext> Provinces;
		public readonly SyncMap<StarMapNodeLine, NodeLineInfo, StarMapBase.SyncContext> NodeLines;
		public readonly SyncMap<StarMapTerrain, TerrainInfo, StarMapBase.SyncContext> Terrain;
		private bool _active;

		protected abstract void GetAdditionalParams(List<object> parms);

		protected virtual StarMapProp CreateProp(
		  GameObjectSet gos,
		  StellarPropInfo oi,
		  StarMapBase.SyncContext context)
		{
			return (StarMapProp)null;
		}

		protected virtual void UpdateProp(
		  StarMapProp o,
		  StellarPropInfo oi,
		  StarMapBase.SyncContext context)
		{
		}

		protected virtual StarMapSystem CreateSystem(
		  GameObjectSet gos,
		  StarSystemInfo oi,
		  StarMapBase.SyncContext context)
		{
			return (StarMapSystem)null;
		}

		protected virtual void UpdateSystem(
		  StarMapSystem o,
		  StarSystemInfo oi,
		  StarMapBase.SyncContext context)
		{
		}

		protected virtual StarMapFleet CreateFleet(
		  GameObjectSet gos,
		  FleetInfo oi,
		  StarMapBase.SyncContext context)
		{
			return (StarMapFleet)null;
		}

		protected virtual void UpdateFleet(
		  StarMapFleet o,
		  FleetInfo oi,
		  StarMapBase.SyncContext context)
		{
		}

		protected virtual StarMapProvince CreateProvince(
		  GameObjectSet gos,
		  ProvinceInfo oi,
		  StarMapBase.SyncContext context)
		{
			return (StarMapProvince)null;
		}

		protected virtual void UpdateProvince(
		  StarMapProvince o,
		  ProvinceInfo oi,
		  StarMapBase.SyncContext context)
		{
		}

		protected virtual StarMapFilter CreateFilter(
		  GameObjectSet gos,
		  StarMapViewFilter filter,
		  StarMapBase.SyncContext context)
		{
			return (StarMapFilter)null;
		}

		protected virtual StarMapNodeLine CreateNodeLine(
		  GameObjectSet gos,
		  NodeLineInfo oi,
		  StarMapBase.SyncContext context)
		{
			return (StarMapNodeLine)null;
		}

		protected virtual void UpdateNodeLine(
		  StarMapNodeLine o,
		  NodeLineInfo oi,
		  StarMapBase.SyncContext context)
		{
		}

		protected virtual StarMapTerrain CreateTerrain(
		  GameObjectSet gos,
		  TerrainInfo oi,
		  StarMapBase.SyncContext context)
		{
			return (StarMapTerrain)null;
		}

		protected virtual void UpdateTerrain(
		  StarMapTerrain o,
		  TerrainInfo oi,
		  StarMapBase.SyncContext context)
		{
		}

		public StarMapBase(App game, Sky sky)
		{
			this.Provinces = new SyncMap<StarMapProvince, ProvinceInfo, StarMapBase.SyncContext>(new Func<GameObjectSet, ProvinceInfo, StarMapBase.SyncContext, StarMapProvince>(this.CreateProvince), new Action<StarMapProvince, ProvinceInfo, StarMapBase.SyncContext>(this.UpdateProvince));
			this.Props = new SyncMap<StarMapProp, StellarPropInfo, StarMapBase.SyncContext>(new Func<GameObjectSet, StellarPropInfo, StarMapBase.SyncContext, StarMapProp>(this.CreateProp), new Action<StarMapProp, StellarPropInfo, StarMapBase.SyncContext>(this.UpdateProp));
			this.Systems = new SyncMap<StarMapSystem, StarSystemInfo, StarMapBase.SyncContext>(new Func<GameObjectSet, StarSystemInfo, StarMapBase.SyncContext, StarMapSystem>(this.CreateSystem), new Action<StarMapSystem, StarSystemInfo, StarMapBase.SyncContext>(this.UpdateSystem));
			this.Fleets = new SyncMap<StarMapFleet, FleetInfo, StarMapBase.SyncContext>(new Func<GameObjectSet, FleetInfo, StarMapBase.SyncContext, StarMapFleet>(this.CreateFleet), new Action<StarMapFleet, FleetInfo, StarMapBase.SyncContext>(this.UpdateFleet));
			this.NodeLines = new SyncMap<StarMapNodeLine, NodeLineInfo, StarMapBase.SyncContext>(new Func<GameObjectSet, NodeLineInfo, StarMapBase.SyncContext, StarMapNodeLine>(this.CreateNodeLine), new Action<StarMapNodeLine, NodeLineInfo, StarMapBase.SyncContext>(this.UpdateNodeLine));
			this.Terrain = new SyncMap<StarMapTerrain, TerrainInfo, StarMapBase.SyncContext>(new Func<GameObjectSet, TerrainInfo, StarMapBase.SyncContext, StarMapTerrain>(this.CreateTerrain), new Action<StarMapTerrain, TerrainInfo, StarMapBase.SyncContext>(this.UpdateTerrain));
			List<object> parms = new List<object>();
			int numMiniShips = game.AssetDatabase.GetNumMiniShips();
			parms.Add((object)numMiniShips);
			for (int id = 0; id < numMiniShips; ++id)
				parms.Add((object)game.AssetDatabase.GetMiniShipDirectoryFromID(id).Location);
			parms.Add((object)(sky != null ? sky.ObjectID : 0));
			this.GetAdditionalParams(parms);
			parms.Add((object)"StarSystemPopup");
			game.AddExistingObject((IGameObject)this, parms.ToArray());
		}

		public void Initialize(GameObjectSet gos, params object[] parms)
		{
			this.OnInitialize(gos, parms);
			this.PostObjectAddObjects(gos.ToArray<IGameObject>());
		}

		protected abstract void OnInitialize(GameObjectSet gos, params object[] parms);

		public void SetCamera(OrbitCameraController value)
		{
			this.PostSetProp("CameraController", (IGameObject)value);
		}

		public void SetFocus(IGameObject target)
		{
			this.SetFocus(target, float.MaxValue);
		}

		public void SetFocus(IGameObject target, float distance)
		{
			this.PostSetProp("Focus", (object)target.GetObjectID(), (object)distance);
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active == value)
					return;
				this._active = value;
				this.PostSetActive(value);
			}
		}

		public bool SelectEnabled
		{
			get
			{
				return this._selectEnabled;
			}
			set
			{
				if (this._selectEnabled == value)
					return;
				this._selectEnabled = value;
				this.PostSetProp(nameof(SelectEnabled), value);
			}
		}

		public bool MouseOverEnabled
		{
			get
			{
				return this._mouseOverEnabled;
			}
			set
			{
				if (this._mouseOverEnabled == value)
					return;
				this._mouseOverEnabled = value;
				this.PostSetProp(nameof(MouseOverEnabled), value);
			}
		}

		public bool FocusEnabled
		{
			get
			{
				return this._focusEnabled;
			}
			set
			{
				if (this._focusEnabled == value)
					return;
				this._focusEnabled = value;
				this.PostSetProp(nameof(FocusEnabled), value);
			}
		}

		public void Dispose()
		{
			if (this.App == null)
				return;
			this.App.ReleaseObject((IGameObject)this);
		}

		public class SyncContext
		{
			public readonly List<PlayerInfo> PlayerInfos;

			public SyncContext(GameDatabase db)
			{
				this.PlayerInfos = db.GetPlayerInfos().ToList<PlayerInfo>();
			}
		}
	}
}
