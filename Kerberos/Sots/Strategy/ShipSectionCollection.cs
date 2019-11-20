// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.ShipSectionCollection
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class ShipSectionCollection
	{
		private static readonly HashSet<ShipSectionAsset> Empty = new HashSet<ShipSectionAsset>();
		private readonly Dictionary<ShipSectionCollection.Key, HashSet<ShipSectionAsset>> _bykey = new Dictionary<ShipSectionCollection.Key, HashSet<ShipSectionAsset>>();
		private readonly HashSet<ShipSectionAsset> _all = new HashSet<ShipSectionAsset>();

		public ShipSectionCollection(
		  GameDatabase gameDatabase,
		  AssetDatabase assetDatabase,
		  Player player,
		  string[] availableSectionIds)
		{
			int playerFactionId = gameDatabase.GetPlayerFactionID(player.ID);
			string playerFaction = gameDatabase.GetFactionName(playerFactionId);
			this._bykey.Clear();
			this._all.Clear();
			foreach (ShipSectionAsset shipSectionAsset in assetDatabase.ShipSections.Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (((IEnumerable<string>)availableSectionIds).Contains<string>(x.FileName) && x.Faction == playerFaction)
				   return !x.IsSuulka;
			   return false;
		   })))
				this._all.Add(shipSectionAsset);
			foreach (ShipSectionAsset shipSectionAsset in this._all)
			{
				ShipSectionCollection.Key key = new ShipSectionCollection.Key()
				{
					Class = shipSectionAsset.Class,
					Type = shipSectionAsset.Type
				};
				HashSet<ShipSectionAsset> shipSectionAssetSet;
				if (!this._bykey.TryGetValue(key, out shipSectionAssetSet))
				{
					shipSectionAssetSet = new HashSet<ShipSectionAsset>();
					this._bykey.Add(key, shipSectionAssetSet);
				}
				shipSectionAssetSet.Add(shipSectionAsset);
			}
		}

		public HashSet<ShipSectionAsset> GetSectionsByType(
		  ShipClass shipClass,
		  ShipSectionType type)
		{
			HashSet<ShipSectionAsset> shipSectionAssetSet;
			if (!this._bykey.TryGetValue(new ShipSectionCollection.Key()
			{
				Type = type,
				Class = shipClass
			}, out shipSectionAssetSet))
				return ShipSectionCollection.Empty;
			return shipSectionAssetSet;
		}

		public HashSet<ShipSectionAsset> GetAllSections()
		{
			return this._all;
		}

		private struct Key
		{
			public ShipSectionType Type;
			public ShipClass Class;
		}
	}
}
