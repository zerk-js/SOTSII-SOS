// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.SpendingCaps
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;

namespace Kerberos.Sots.Strategy
{
	internal class SpendingCaps
	{
		public readonly double StimulusMining;
		public readonly double StimulusColonization;
		public readonly double StimulusTrade;
		public readonly double SecurityOperations;
		public readonly double SecurityIntelligence;
		public readonly double SecurityCounterIntelligence;
		public readonly double ResearchCurrentProject;
		public readonly double ResearchSpecialProject;
		public readonly double ResearchSalvageResearch;

		public SpendingCaps(GameDatabase db, PlayerInfo playerInfo, BudgetProjection projection)
		{
			int researchingTechId = db.GetPlayerResearchingTechID(playerInfo.ID);
			int feasibilityStudyTechId = db.GetPlayerFeasibilityStudyTechId(playerInfo.ID);
			bool flag = researchingTechId != 0 || feasibilityStudyTechId != 0;
			if (projection == BudgetProjection.Actual)
			{
				this.StimulusMining = 0.0;
				this.StimulusColonization = 0.0;
				this.StimulusTrade = double.MaxValue;
				this.SecurityOperations = double.MaxValue;
				this.SecurityIntelligence = double.MaxValue;
				this.SecurityCounterIntelligence = double.MaxValue;
				this.ResearchCurrentProject = flag ? double.MaxValue : 0.0;
				this.ResearchSpecialProject = 0.0;
				this.ResearchSalvageResearch = 0.0;
			}
			else
			{
				this.StimulusMining = double.MaxValue;
				this.StimulusColonization = double.MaxValue;
				this.StimulusTrade = double.MaxValue;
				this.SecurityOperations = double.MaxValue;
				this.SecurityIntelligence = double.MaxValue;
				this.SecurityCounterIntelligence = double.MaxValue;
				this.ResearchCurrentProject = double.MaxValue;
				this.ResearchSpecialProject = double.MaxValue;
				this.ResearchSalvageResearch = double.MaxValue;
			}
		}
	}
}
