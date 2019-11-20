// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.FleetManagement
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using System;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class FleetManagement
	{
		public FleetInfo Fleet;
		public MissionEstimate MissionTime;
		public FleetTypeFlags FleetTypes;
		public int Score;
		public int FleetStrength;

		public FleetManagement()
		{
			this.Fleet = (FleetInfo)null;
			this.MissionTime = (MissionEstimate)null;
			this.Score = 0;
			this.FleetStrength = 0;
		}

		public static FleetTypeFlags GetFleetTypeFlags(
		  App app,
		  int fleetID,
		  int playerID,
		  bool isLoa)
		{
			if (isLoa || app.GameDatabase.GetPlayerFaction(playerID).Name == "loa")
				return FleetTypeFlags.ANY;
			AIFleetInfo aiFleetInfo = app.GameDatabase.GetAIFleetInfos(playerID).FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
		   {
			   int? fleetId = x.FleetID;
			   int num = fleetID;
			   return fleetId.GetValueOrDefault() == num & fleetId.HasValue;
		   }));
			FleetTemplate template = (FleetTemplate)null;
			string templateName = aiFleetInfo != null ? aiFleetInfo.FleetTemplate : DesignLab.DeduceFleetTemplate(app.GameDatabase, app.Game, fleetID);
			if (!string.IsNullOrEmpty(templateName))
				template = app.GameDatabase.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == templateName));
			if (template == null)
				template = app.GameDatabase.AssetDatabase.FleetTemplates.FirstOrDefault<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == "DEFAULT_COMBAT"));
			return FleetManagement.GetFleetTypeFlags(template, isLoa);
		}

		public static FleetTypeFlags GetFleetTypeFlags(FleetTemplate template, bool isLoa)
		{
			if (template == null)
				return FleetTypeFlags.UNKNOWN;
			FleetTypeFlags fleetTypeFlags = FleetTypeFlags.UNKNOWN;
			if (isLoa && template.MissionTypes.Contains(MissionType.SURVEY))
				fleetTypeFlags |= FleetTypeFlags.NPG;
			foreach (MissionType missionType in template.MissionTypes)
			{
				switch (missionType)
				{
					case MissionType.COLONIZATION:
						fleetTypeFlags |= FleetTypeFlags.COLONIZE;
						continue;
					case MissionType.SURVEY:
						fleetTypeFlags |= FleetTypeFlags.SURVEY;
						continue;
					case MissionType.CONSTRUCT_STN:
					case MissionType.UPGRADE_STN:
						fleetTypeFlags |= FleetTypeFlags.CONSTRUCTION;
						continue;
					case MissionType.PATROL:
						fleetTypeFlags |= FleetTypeFlags.PATROL;
						continue;
					case MissionType.INTERDICTION:
					case MissionType.STRIKE:
					case MissionType.INVASION:
						fleetTypeFlags |= FleetTypeFlags.COMBAT;
						fleetTypeFlags |= FleetTypeFlags.PLANETATTACK;
						continue;
					case MissionType.GATE:
						fleetTypeFlags |= FleetTypeFlags.GATE;
						continue;
					default:
						continue;
				}
			}
			return fleetTypeFlags;
		}
	}
}
