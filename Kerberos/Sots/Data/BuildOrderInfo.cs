// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.BuildOrderInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class BuildOrderInfo : IIDProvider
	{
		public string ShipName = string.Empty;
		public int SystemID;
		public int DesignID;
		public int Priority;
		public int Progress;
		public int ProductionTarget;
		public int MissionID;
		public int? InvoiceID;
		public int? AIFleetID;
		public int LoaCubes;

		public int ID { get; set; }
	}
}
