// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.IndependentRaceInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class IndependentRaceInfo : IIDProvider
	{
		public string Name;
		public int OrbitalObjectID;
		public double Population;
		public int TechLevel;
		public int ReactionHuman;
		public int ReactionTarka;
		public int ReactionLiir;
		public int ReactionZuul;
		public int ReactionMorrigi;
		public int ReactionHiver;

		public int ID { get; set; }
	}
}
