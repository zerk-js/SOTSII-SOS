// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarSystemPathing.StarSystemPathing
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.StarSystemPathing
{
	internal class StarSystemPathing
	{
		private static Dictionary<int, Dictionary<int, List<LinkNodeChild>>> _playerSystemNodes = new Dictionary<int, Dictionary<int, List<LinkNodeChild>>>();

		public static void LoadAllNodes(GameSession game, GameDatabase db)
		{
			Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes.Clear();
			List<PlayerInfo> list = db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<Player> playerList = new List<Player>();
			foreach (PlayerInfo playerInfo in list)
			{
				Player playerObject = game.GetPlayerObject(playerInfo.ID);
				if (playerObject.Faction.CanUseNodeLine(new bool?()) || playerObject.Faction.CanUseAccelerators())
				{
					Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes.Add(playerObject.ID, new Dictionary<int, List<LinkNodeChild>>());
					playerList.Add(playerObject);
				}
			}
			foreach (StarSystemInfo starSystemInfo in db.GetStarSystemInfos().ToList<StarSystemInfo>())
			{
				foreach (Player player1 in playerList)
				{
					Player player = player1;
					foreach (NodeLineInfo nodeLineInfo in db.GetExploredNodeLinesFromSystem(player.ID, starSystemInfo.ID).Where<NodeLineInfo>((Func<NodeLineInfo, bool>)(x =>
				   {
					   if (x.IsPermenant == player.Faction.CanUseNodeLine(new bool?(true)))
						   return x.IsLoaLine == player.Faction.CanUseAccelerators();
					   return false;
				   })).ToList<NodeLineInfo>())
						Kerberos.Sots.StarSystemPathing.StarSystemPathing.AddSystemNode(db, player.ID, nodeLineInfo.System1ID, nodeLineInfo.System2ID, nodeLineInfo.ID);
				}
			}
		}

		public static void AddSystemNode(
		  GameDatabase db,
		  int playerId,
		  int fromSystemId,
		  int toSystemId,
		  int nodeId)
		{
			if (!Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes.ContainsKey(playerId))
				return;
			if (!Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId].ContainsKey(fromSystemId))
				Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId].Add(fromSystemId, new List<LinkNodeChild>());
			if (!Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId].ContainsKey(toSystemId))
				Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId].Add(toSystemId, new List<LinkNodeChild>());
			float length = (db.GetStarSystemOrigin(fromSystemId) - db.GetStarSystemOrigin(toSystemId)).Length;
			if (!Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId][fromSystemId].Any<LinkNodeChild>((Func<LinkNodeChild, bool>)(x => x.SystemId == toSystemId)))
				Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId][fromSystemId].Add(new LinkNodeChild()
				{
					SystemId = toSystemId,
					NodeId = nodeId,
					Distance = length
				});
			if (Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId][toSystemId].Any<LinkNodeChild>((Func<LinkNodeChild, bool>)(x => x.SystemId == fromSystemId)))
				return;
			Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId][toSystemId].Add(new LinkNodeChild()
			{
				SystemId = fromSystemId,
				NodeId = nodeId,
				Distance = length
			});
		}

		public static void RemoveNodeLine(int nodeId)
		{
			foreach (KeyValuePair<int, Dictionary<int, List<LinkNodeChild>>> playerSystemNode in Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes)
			{
				foreach (KeyValuePair<int, List<LinkNodeChild>> keyValuePair in playerSystemNode.Value)
					keyValuePair.Value.RemoveAll((Predicate<LinkNodeChild>)(x => x.NodeId == nodeId));
			}
		}

		public static List<int> FindClosestPath(
		  GameDatabase db,
		  int playerId,
		  int fromSystemId,
		  int toSystemId,
		  bool nodeLinesOnly)
		{
			if (!Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes.ContainsKey(playerId) || fromSystemId == 0 || toSystemId == 0)
				return new List<int>();
			int num1 = Kerberos.Sots.StarSystemPathing.StarSystemPathing.ClosestNodeSystem(db, playerId, fromSystemId);
			int toNodeSystem = Kerberos.Sots.StarSystemPathing.StarSystemPathing.ClosestNodeSystem(db, playerId, toSystemId);
			if (num1 <= 0 || toNodeSystem <= 0 || nodeLinesOnly && (num1 != fromSystemId || toNodeSystem != toSystemId))
				return new List<int>();
			foreach (KeyValuePair<int, List<LinkNodeChild>> keyValuePair in Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId])
			{
				foreach (LinkNodeChild linkNodeChild in keyValuePair.Value)
				{
					linkNodeChild.HasBeenChecked = linkNodeChild.SystemId == num1;
					linkNodeChild.ParentLink = (LinkNodeChild)null;
					linkNodeChild.TotalDistance = 0.0f;
				}
			}
			LinkNodeChild from = new LinkNodeChild()
			{
				ParentLink = (LinkNodeChild)null,
				HasBeenChecked = true,
				SystemId = num1,
				Distance = 0.0f,
				TotalDistance = 0.0f,
				NodeId = 0
			};
			Kerberos.Sots.StarSystemPathing.StarSystemPathing.LinkNodes(playerId, from, toNodeSystem, 0.0f);
			List<LinkNodeChild> linkNodeChildList = new List<LinkNodeChild>();
			foreach (KeyValuePair<int, List<LinkNodeChild>> keyValuePair in Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId])
				linkNodeChildList.AddRange((IEnumerable<LinkNodeChild>)keyValuePair.Value.Where<LinkNodeChild>((Func<LinkNodeChild, bool>)(x => x.SystemId == toNodeSystem)).ToList<LinkNodeChild>());
			LinkNodeChild linkNodeChild1 = (LinkNodeChild)null;
			float num2 = float.MaxValue;
			foreach (LinkNodeChild linkNodeChild2 in linkNodeChildList)
			{
				bool flag = false;
				for (LinkNodeChild linkNodeChild3 = linkNodeChild2; linkNodeChild3 != null; linkNodeChild3 = linkNodeChild3.ParentLink)
				{
					if (linkNodeChild3.SystemId == num1)
					{
						flag = true;
						break;
					}
				}
				if (flag && (double)linkNodeChild2.TotalDistance < (double)num2)
				{
					linkNodeChild1 = linkNodeChild2;
					num2 = linkNodeChild2.TotalDistance;
				}
			}
			List<int> source = new List<int>();
			if (linkNodeChild1 != null)
			{
				if (linkNodeChild1.SystemId != toSystemId)
					source.Add(toSystemId);
				for (; linkNodeChild1 != null; linkNodeChild1 = linkNodeChild1.ParentLink)
					source.Add(linkNodeChild1.SystemId);
				if (source.ElementAt<int>(source.Count - 1) != fromSystemId)
					source.Add(fromSystemId);
			}
			source.Reverse();
			foreach (KeyValuePair<int, List<LinkNodeChild>> keyValuePair in Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId])
			{
				foreach (LinkNodeChild linkNodeChild2 in keyValuePair.Value)
				{
					linkNodeChild2.HasBeenChecked = false;
					linkNodeChild2.ParentLink = (LinkNodeChild)null;
					linkNodeChild2.TotalDistance = 0.0f;
				}
			}
			return source;
		}

		private static void LinkNodes(int playerId, LinkNodeChild from, int toId, float totalDist)
		{
			if (from == null)
				return;
			foreach (LinkNodeChild from1 in Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId][from.SystemId])
			{
				float totalDist1 = from1.Distance + totalDist;
				if (!from1.HasBeenChecked || (double)totalDist1 < (double)from1.TotalDistance)
				{
					from1.TotalDistance = totalDist1;
					from1.ParentLink = from;
					from1.HasBeenChecked = true;
					if (from1.SystemId != toId)
						Kerberos.Sots.StarSystemPathing.StarSystemPathing.LinkNodes(playerId, from1, toId, totalDist1);
				}
			}
		}

		private static int ClosestNodeSystem(GameDatabase db, int playerId, int systemId)
		{
			if (Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId].ContainsKey(systemId))
				return systemId;
			StarSystemInfo starSystemInfo1 = db.GetStarSystemInfo(systemId);
			if (starSystemInfo1 == (StarSystemInfo)null)
				return systemId;
			int num1 = -1;
			float num2 = float.MaxValue;
			foreach (KeyValuePair<int, List<LinkNodeChild>> keyValuePair in Kerberos.Sots.StarSystemPathing.StarSystemPathing._playerSystemNodes[playerId])
			{
				StarSystemInfo starSystemInfo2 = db.GetStarSystemInfo(keyValuePair.Key);
				if (!(starSystemInfo2 == (StarSystemInfo)null))
				{
					float lengthSquared = (starSystemInfo2.Origin - starSystemInfo1.Origin).LengthSquared;
					if ((double)lengthSquared < (double)num2)
					{
						num2 = lengthSquared;
						num1 = keyValuePair.Key;
					}
				}
			}
			return num1;
		}
	}
}
