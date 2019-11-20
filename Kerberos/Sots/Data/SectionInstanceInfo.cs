// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.SectionInstanceInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class SectionInstanceInfo : IIDProvider
	{
		public Dictionary<ArmorSide, DamagePattern> Armor = new Dictionary<ArmorSide, DamagePattern>();
		public List<WeaponInstanceInfo> WeaponInstances = new List<WeaponInstanceInfo>();
		public List<ModuleInstanceInfo> ModuleInstances = new List<ModuleInstanceInfo>();
		public int SectionID;
		public int? ShipID;
		public int? StationID;
		public int Structure;
		public int Supply;
		public int Crew;
		public float Signature;
		public int RepairPoints;

		public int ID { get; set; }
	}
}
