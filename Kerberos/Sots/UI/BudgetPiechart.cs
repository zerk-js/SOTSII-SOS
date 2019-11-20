// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.BudgetPiechart
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System;

namespace Kerberos.Sots.UI
{
	internal class BudgetPiechart : Piechart
	{
		private readonly AssetDatabase _assetdb;

		public void SetSlices(Budget budget)
		{
			double num = Math.Max(budget.TotalRevenue + budget.NetSavingsLoss - budget.PendingStationsModulesCost - budget.PendingBuildShipsCost - budget.PendingBuildStationsCost, 0.0);
			if (num == 0.0)
				this.SetSlices(new PiechartSlice(this._assetdb.PieChartColourSavings, 1.0));
			else
				this.SetSlices(new PiechartSlice(this._assetdb.PieChartColourShipMaintenance, budget.UpkeepExpenses / num), new PiechartSlice(this._assetdb.PieChartColourPlanetaryDevelopment, budget.ColonySupportExpenses / num), new PiechartSlice(this._assetdb.PieChartColourDebtInterest, budget.DebtInterest / num), new PiechartSlice(this._assetdb.PieChartColourResearch, budget.ResearchSpending.ProjectedTotal / num), new PiechartSlice(this._assetdb.PieChartColourSecurity, budget.SecuritySpending.ProjectedTotal / num), new PiechartSlice(this._assetdb.PieChartColourStimulus, budget.StimulusSpending.ProjectedTotal / num), new PiechartSlice(this._assetdb.PieChartColourSavings, budget.NetSavingsIncome / num), new PiechartSlice(this._assetdb.PieChartColourCorruption, budget.CorruptionExpenses / num));
		}

		public BudgetPiechart(UICommChannel ui, string id, AssetDatabase assetdb)
		  : base(ui, id)
		{
			this._assetdb = assetdb;
		}
	}
}
