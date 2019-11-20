// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.AIFleetInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class AIFleetInfo : IIDProvider
	{
		public int? FleetID;
		public int FleetType;
		public int SystemID;
		public int? InvoiceID;
		public int? AdmiralID;
		public string FleetTemplate;

		public AIFleetInfo()
		{
		}

		public AIFleetInfo(
		  int? fleetID,
		  int fleetType,
		  int systemID,
		  int? invoiceID,
		  int admiralID,
		  string fleetTemplate)
		{
			this.FleetID = fleetID;
			this.FleetType = fleetType;
			this.SystemID = systemID;
			this.InvoiceID = invoiceID;
			this.AdmiralID = new int?(admiralID);
			this.FleetTemplate = fleetTemplate;
		}

		public int ID { get; set; }

		public override string ToString()
		{
			return string.Format("ID={0},FleetID={1},FleetType={2}", (object)this.ID, this.FleetID.HasValue ? (object)this.FleetID.Value.ToString() : (object)"null", (object)this.FleetType.ToString());
		}
	}
}
