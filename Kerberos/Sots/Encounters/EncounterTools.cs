// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.EncounterTools
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal static class EncounterTools
	{
		public static int AddOrGetEncounterDesignInfo(
		  this GameDatabase db,
		  IEnumerable<DesignInfo> designs,
		  int playerID,
		  string name,
		  string factionName,
		  params string[] sectionFilename)
		{
			for (int index = 0; index < ((IEnumerable<string>)sectionFilename).Count<string>(); ++index)
				sectionFilename[index] = string.Format("factions\\{0}\\sections\\{1}", (object)factionName, (object)sectionFilename[index]);
			DesignInfo designInfo1 = designs.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.Name == name));
			if (designInfo1 != null)
				return designInfo1.ID;
			DesignInfo designInfo2 = new DesignInfo(playerID, name, sectionFilename);
			if (string.IsNullOrEmpty(name))
				designInfo2.Name = DesignLab.GenerateDesignName(db.AssetDatabase, db, (DesignInfo)null, designInfo2, DesignLab.NameGenerators.FactionRandom);
			return db.InsertDesignByDesignInfo(designInfo2);
		}

		public static List<StarSystemInfo> GetClosestStars(
		  GameDatabase game,
		  Vector3 origin)
		{
			return game.GetStarSystemInfos().ToList<StarSystemInfo>().OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(x => (x.Origin - origin).LengthSquared)).ToList<StarSystemInfo>();
		}

		public static List<StarSystemInfo> GetClosestStars(
		  GameDatabase game,
		  StarSystemInfo systemInfo)
		{
			List<StarSystemInfo> list = game.GetStarSystemInfos().ToList<StarSystemInfo>().OrderBy<StarSystemInfo, float>((Func<StarSystemInfo, float>)(x => (x.Origin - systemInfo.Origin).LengthSquared)).ToList<StarSystemInfo>();
			list.RemoveAll((Predicate<StarSystemInfo>)(x => x.ID == systemInfo.ID));
			return list;
		}

		public static List<StarSystemInfo> GetClosestStars(
		  GameDatabase game,
		  int SystemId)
		{
			return EncounterTools.GetClosestStars(game, game.GetStarSystemInfo(SystemId));
		}

		public static float DistanceBetween(StarSystemInfo a, StarSystemInfo b)
		{
			return (b.Origin - a.Origin).Length;
		}

		public static bool IsSystemInhabited(GameDatabase gamedb, int SystemId)
		{
			foreach (OrbitalObjectInfo orbitalObjectInfo in gamedb.GetStarSystemOrbitalObjectInfos(SystemId).ToList<OrbitalObjectInfo>())
			{
				if (gamedb.GetColonyInfoForPlanet(orbitalObjectInfo.ID) != null)
					return true;
			}
			return false;
		}

		public static List<KeyValuePair<StarSystemInfo, Vector3>> GetOutlyingStars(
		  GameDatabase gamedb)
		{
			App.GetSafeRandom();
			float num1 = 10f;
			int num2 = 20;
			List<StarSystemInfo> list1 = gamedb.GetStarSystemInfos().ToList<StarSystemInfo>();
			List<StarSystemInfo> list2 = list1.Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => EncounterTools.IsSystemInhabited(gamedb, x.ID))).ToList<StarSystemInfo>();
			List<StarSystemInfo> starSystemInfoList = new List<StarSystemInfo>();
			int count = list1.Count;
			foreach (StarSystemInfo starSystemInfo in list1)
			{
				StarSystemInfo si = starSystemInfo;
				if (!list2.Contains(si) && (double)num1 < (double)list2.Min<StarSystemInfo>((Func<StarSystemInfo, float>)(x => (x.Origin - si.Origin).Length)))
					starSystemInfoList.Add(si);
			}
			Dictionary<StarSystemInfo, Vector3> source = new Dictionary<StarSystemInfo, Vector3>();
			foreach (StarSystemInfo starSystemInfo in starSystemInfoList)
			{
				StarSystemInfo si = starSystemInfo;
				list1.Sort((Comparison<StarSystemInfo>)((x, y) => EncounterTools.DistanceBetween(x, si).CompareTo(EncounterTools.DistanceBetween(y, si))));
				Vector3 vector3 = new Vector3(0.0f);
				for (int index = 0; index < num2 && index < count; ++index)
				{
					if (si != list1[index])
						vector3 += Vector3.Normalize(si.Origin - list1[index].Origin);
				}
				source.Add(si, vector3);
			}
			return source.OrderBy<KeyValuePair<StarSystemInfo, Vector3>, float>((Func<KeyValuePair<StarSystemInfo, Vector3>, float>)(x => -x.Value.Length)).ToList<KeyValuePair<StarSystemInfo, Vector3>>();
		}
	}
}
