// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.FleetTemplateComparision
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class FleetTemplateComparision : IComparer<FleetTemplate>
	{
		private AIStance _stance;
		private List<AIFleetInfo> _nonDefendAIFleets;

		public FleetTemplateComparision(AIStance stance, List<AIFleetInfo> nonDefendAIFleets)
		{
			this._stance = stance;
			this._nonDefendAIFleets = nonDefendAIFleets;
		}

		public int Compare(FleetTemplate a, FleetTemplate b)
		{
			if (a == b)
				return 0;
			int num1 = this._nonDefendAIFleets.Count<AIFleetInfo>((Func<AIFleetInfo, bool>)(x => x.FleetTemplate == a.Name));
			int num2 = this._nonDefendAIFleets.Count<AIFleetInfo>((Func<AIFleetInfo, bool>)(x => x.FleetTemplate == b.Name));
			int num3 = (int)Math.Floor((1.0 - (double)num1 / (double)a.MinFleetsForStance[this._stance]) * 100.0 * (double)a.StanceWeights[this._stance]);
			int num4 = (int)Math.Floor((1.0 - (double)num2 / (double)b.MinFleetsForStance[this._stance]) * 100.0 * (double)b.StanceWeights[this._stance]);
			if (num3 > num4)
				return -1;
			if (num3 < num4)
				return 1;
			int num5 = (a.MinFleetsForStance[this._stance] - num1) * a.StanceWeights[this._stance];
			int num6 = (b.MinFleetsForStance[this._stance] - num2) * b.StanceWeights[this._stance];
			if (num5 > num6)
				return -1;
			if (num5 < num6)
				return 1;
			if (a.MinFleetsForStance[this._stance] * a.StanceWeights[this._stance] > b.MinFleetsForStance[this._stance] * b.StanceWeights[this._stance])
				return -1;
			if (a.MinFleetsForStance[this._stance] * a.StanceWeights[this._stance] < b.MinFleetsForStance[this._stance] * b.StanceWeights[this._stance])
				return 1;
			bool flag1 = FleetTemplateComparision.MustHaveTemplate(a);
			bool flag2 = FleetTemplateComparision.MustHaveTemplate(b);
			if (num1 == 0 & flag1 && (!flag2 || num2 > 0))
				return -1;
			if (((num1 > 0 ? 1 : (!flag1 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0 && num2 == 0)
				return 1;
			int missionPriority1 = this.GetMissionPriority(a.MissionTypes);
			int missionPriority2 = this.GetMissionPriority(b.MissionTypes);
			if (missionPriority1 > missionPriority2)
				return -1;
			if (missionPriority1 < missionPriority2)
				return 1;
			if (a.TemplateID < b.TemplateID)
				return -1;
			return a.TemplateID > b.TemplateID ? 1 : 0;
		}

		private int GetMissionPriority(List<MissionType> missions)
		{
			int num = 0;
			foreach (MissionType mission in missions)
			{
				switch (mission)
				{
					case MissionType.COLONIZATION:
					case MissionType.SUPPORT:
						num += 3;
						continue;
					case MissionType.SURVEY:
						num += 7;
						continue;
					case MissionType.CONSTRUCT_STN:
					case MissionType.UPGRADE_STN:
						num += 2;
						continue;
					case MissionType.PATROL:
					case MissionType.INTERDICTION:
					case MissionType.STRIKE:
					case MissionType.INVASION:
						++num;
						continue;
					case MissionType.GATE:
						num += 9;
						continue;
					case MissionType.DEPLOY_NPG:
						num += 8;
						continue;
					default:
						continue;
				}
			}
			return num;
		}

		public static bool MustHaveTemplate(FleetTemplate template)
		{
			foreach (MissionType missionType in template.MissionTypes)
			{
				switch (missionType)
				{
					case MissionType.COLONIZATION:
					case MissionType.SUPPORT:
					case MissionType.SURVEY:
					case MissionType.CONSTRUCT_STN:
					case MissionType.UPGRADE_STN:
					case MissionType.GATE:
					case MissionType.DEPLOY_NPG:
						return true;
					default:
						continue;
				}
			}
			return false;
		}
	}
}
