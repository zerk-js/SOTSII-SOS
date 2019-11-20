// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.CombatAIDamageData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Xml;

namespace Kerberos.Sots
{
	public struct CombatAIDamageData
	{
		public int Crew;
		public int Population;
		public float InfraDamage;
		public float TeraDamage;

		public void SetDataFromElement(XmlElement data)
		{
			this.Crew = int.Parse(data.GetAttribute("crew"));
			this.Population = int.Parse(data.GetAttribute("population"));
			this.InfraDamage = float.Parse(data.GetAttribute("infra"));
			this.TeraDamage = float.Parse(data.GetAttribute("tera"));
		}
	}
}
