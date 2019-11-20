// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.GovernmentInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	public class GovernmentInfo
	{
		public int PlayerID;
		public float Authoritarianism;
		public float EconomicLiberalism;
		public GovernmentInfo.GovernmentType CurrentType;

		public enum GovernmentType
		{
			Centrism,
			Communalism,
			Junta,
			Plutocracy,
			Socialism,
			Mercantilism,
			Cooperativism,
			Anarchism,
			Liberationism,
		}
	}
}
