// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.IndyDesc
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.PlayerFramework
{
	public class IndyDesc
	{
		public string BaseFactionSuitability;
		public float Suitability;
		public float BasePopulationMod;
		public float BiosphereMod;
		public float TradeFTL;
		public int TechLevel;
		public string StellarBodyType;
		public int MinPlanetSize;
		public int MaxPlanetSize;
		public List<SpecialAttribute> CoreSpecialAttributes;
		public List<SpecialAttribute> RandomSpecialAttributes;
	}
}
