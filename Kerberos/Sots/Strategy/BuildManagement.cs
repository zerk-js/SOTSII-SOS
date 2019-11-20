// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.BuildManagement
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class BuildManagement
	{
		public FleetInfo FleetToFill;
		public List<BuildScreenState.InvoiceItem> Invoices;
		public int BuildTime;
		public int? AIFleetID;

		public BuildManagement()
		{
			this.FleetToFill = (FleetInfo)null;
			this.Invoices = new List<BuildScreenState.InvoiceItem>();
			this.BuildTime = 0;
			this.AIFleetID = new int?();
		}
	}
}
