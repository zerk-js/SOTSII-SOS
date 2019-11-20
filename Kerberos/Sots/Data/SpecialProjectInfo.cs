// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.SpecialProjectInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.Data
{
	internal class SpecialProjectInfo : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public int Progress;
		public int Cost;
		public SpecialProjectType Type;
		public int TechID;
		public float Rate;
		public int EncounterID;
		public int FleetID;
		public int TargetPlayerID;

		public int ID { get; set; }
	}
}
