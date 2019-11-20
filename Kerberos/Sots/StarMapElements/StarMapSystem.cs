// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapSystem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPSYSTEM)]
	internal class StarMapSystem : StarMapObject
	{
		private bool _enabled;

		public StarMapSystem(App game, string modelName, Vector3 position, float scale, string label)
		{
			game.AddExistingObject((IGameObject)this, (object)modelName);
			this.PostSetProp("Label", label);
			this.PostSetScale(scale);
			this.PostSetPosition(position);
			this._enabled = true;
		}

		public void SetTradeValues(
		  GameSession _game,
		  TradeNode node,
		  TradeNode historynode,
		  int systemId)
		{
			this.PostSetProp("TradeNode", (object)node.Produced, (object)node.ProductionCapacity, (object)node.Consumption, (object)node.Freighters, (object)node.DockCapacity, (object)node.ExportInt, (object)node.ExportProv, (object)node.ExportLoc, (object)node.ImportInt, (object)node.ImportProv, (object)node.ImportLoc, (object)node.Range, (object)historynode.Produced, (object)historynode.Freighters, (object)historynode.ImportInt);
			List<FreighterInfo> freighterInfoList = _game.GameDatabase == null ? new List<FreighterInfo>() : _game.GameDatabase.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>();
			Dictionary<FreighterInfo, int> source = new Dictionary<FreighterInfo, int>();
			foreach (FreighterInfo key in freighterInfoList)
				source.Add(key, ((IEnumerable<DesignSectionInfo>)key.Design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => x.ShipSectionAsset.FreighterSpace)));
			Dictionary<FreighterInfo, int> dictionary = source.OrderByDescending<KeyValuePair<FreighterInfo, int>, int>((Func<KeyValuePair<FreighterInfo, int>, int>)(x => x.Value)).ToDictionary<KeyValuePair<FreighterInfo, int>, FreighterInfo, int>((Func<KeyValuePair<FreighterInfo, int>, FreighterInfo>)(y => y.Key), (Func<KeyValuePair<FreighterInfo, int>, int>)(y => y.Value));
			List<object> objectList1 = new List<object>();
			objectList1.Add((object)dictionary.Where<KeyValuePair<FreighterInfo, int>>((Func<KeyValuePair<FreighterInfo, int>, bool>)(x =>
		  {
			  if (x.Key.PlayerId == _game.LocalPlayer.ID)
				  return !x.Key.IsPlayerBuilt;
			  return true;
		  })).Count<KeyValuePair<FreighterInfo, int>>());
			objectList1.AddRange(dictionary.Where<KeyValuePair<FreighterInfo, int>>((Func<KeyValuePair<FreighterInfo, int>, bool>)(x =>
		   {
			   if (x.Key.PlayerId == _game.LocalPlayer.ID)
				   return !x.Key.IsPlayerBuilt;
			   return true;
		   })).Select<KeyValuePair<FreighterInfo, int>, int>((Func<KeyValuePair<FreighterInfo, int>, int>)(x => x.Value)).Cast<object>());
			this.PostSetProp("FreighterCapacities", objectList1.ToArray());
			List<object> objectList2 = new List<object>();
			objectList2.Add((object)dictionary.Where<KeyValuePair<FreighterInfo, int>>((Func<KeyValuePair<FreighterInfo, int>, bool>)(x =>
		  {
			  if (x.Key.PlayerId == _game.LocalPlayer.ID)
				  return x.Key.IsPlayerBuilt;
			  return false;
		  })).Count<KeyValuePair<FreighterInfo, int>>());
			objectList2.AddRange(dictionary.Where<KeyValuePair<FreighterInfo, int>>((Func<KeyValuePair<FreighterInfo, int>, bool>)(x =>
		   {
			   if (x.Key.PlayerId == _game.LocalPlayer.ID)
				   return x.Key.IsPlayerBuilt;
			   return false;
		   })).Select<KeyValuePair<FreighterInfo, int>, int>((Func<KeyValuePair<FreighterInfo, int>, int>)(x => x.Value)).Cast<object>());
			this.PostSetProp("PlayerFreighterCapacities", objectList2.ToArray());
		}

		public void SetProductionValues(int prod, int maxProd)
		{
			this.PostSetProp("ProdValues", (object)prod, (object)maxProd);
		}

		public void SetProvince(StarMapProvince value)
		{
			this.PostSetProp("Province", (IGameObject)value);
		}

		public void SetPlayers(Player[] players)
		{
			CommonMessageExtensions.PostSetProp(this, "Players", (IGameObject[])players);
		}

		public void SetPlayersWithGates(Player[] players)
		{
			CommonMessageExtensions.PostSetProp(this, "PlayersWithGates", (IGameObject[])players);
		}

		public void SetPlayersWithAccelerators(Player[] players)
		{
			CommonMessageExtensions.PostSetProp(this, "PlayersWithAccelerators", (IGameObject[])players);
		}

		public void SetNavalCapacity(int capacity)
		{
			this.PostSetProp("NavalCapacity", capacity);
		}

		public void SetNavalUsage(int usage)
		{
			this.PostSetProp("NavalUsage", usage);
		}

		public void SetHasNavalStation(bool value)
		{
			this.PostSetProp("HasNavalStation", value);
		}

		public void SetHasScienceStation(bool value)
		{
			this.PostSetProp("HasScienceStation", value);
		}

		public void SetHasTradeStation(bool value)
		{
			this.PostSetProp("HasTradeStation", value);
		}

		public void SetHasDiploStation(bool value)
		{
			this.PostSetProp("HasDiploStation", value);
		}

		public void SetHasLoaGate(bool value)
		{
			this.PostSetProp("HasLoaGate", value);
		}

		public void SetStationCapacity(int value)
		{
			this.PostSetProp("StationsSupported", value);
		}

		public void SetColonyTrapped(bool value)
		{
			this.PostSetProp("ColonyTrapped", value);
		}

		public void SetSupportRange(float value)
		{
			this.PostSetProp("SupportRange", value);
		}

		public void SetPlayerBadge(string value)
		{
			this.PostSetProp("OwnerBadge", value);
		}

		public void SetOwningPlayer(Player player)
		{
			this.PostSetProp("SystemOwner", (IGameObject)player);
		}

		public void SetIsEnabled(bool value)
		{
			this.PostSetProp("Enabled", value);
			this._enabled = value;
		}

		public bool GetIsEnabled()
		{
			return this._enabled;
		}

		public void SetIsSurveyed(bool value)
		{
			this.PostSetProp("Surveyed", value);
		}

		public void SetTerrain(StarMapTerrain value)
		{
			this.PostSetProp("Terrain", (IGameObject)value);
		}
	}
}
