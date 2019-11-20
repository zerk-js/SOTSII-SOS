// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.PlanetInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class PlanetInfo : IIDProvider
	{
		public string Type;
		public int? RingID;
		public float Suitability;
		public int Biosphere;
		public int Resources;
		public int MaxResources;
		public float Infrastructure;
		public float Size;

		public int ID { get; set; }

		public static bool AreSame(PlanetInfo s1, PlanetInfo s2)
		{
			if (s1.ID == s2.ID && s1.Type == s2.Type && !s1.RingID.HasValue == !s2.RingID.HasValue && ((!s1.RingID.HasValue || s1.RingID.Value == s2.RingID.Value) && ((double)s1.Suitability == (double)s2.Suitability && s1.Biosphere == s2.Biosphere)) && (s1.Resources == s2.Resources && (double)s1.Infrastructure == (double)s2.Infrastructure))
				return (double)s1.Size == (double)s2.Size;
			return false;
		}
	}
}
