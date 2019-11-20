// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.StimulusSpending
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;

namespace Kerberos.Sots.Strategy
{
	internal struct StimulusSpending
	{
		public double RequestedTotal;
		public double RequestedMining;
		public double RequestedColonization;
		public double RequestedTrade;
		public double ProjectedMining;
		public double ProjectedColonization;
		public double ProjectedTrade;

		public double ProjectedTotal
		{
			get
			{
				return this.ProjectedMining + this.ProjectedColonization + this.ProjectedTrade;
			}
		}

		public StimulusSpending(
		  PlayerInfo playerInfo,
		  double total,
		  SpendingPool pool,
		  SpendingCaps caps)
		{
			this.RequestedTotal = total;
			this.RequestedMining = total * (double)playerInfo.RateStimulusMining;
			this.RequestedColonization = total * (double)playerInfo.RateStimulusColonization;
			this.RequestedTrade = total - this.RequestedMining - this.RequestedColonization;
			this.ProjectedMining = pool.Distribute(this.RequestedMining, caps.StimulusMining);
			this.ProjectedColonization = pool.Distribute(this.RequestedColonization, caps.StimulusColonization);
			this.ProjectedTrade = pool.Distribute(this.RequestedTrade, caps.StimulusTrade);
		}
	}
}
