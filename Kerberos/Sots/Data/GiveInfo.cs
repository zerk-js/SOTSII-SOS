// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.GiveInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class GiveInfo
	{
		public int InitiatingPlayer;
		public int ReceivingPlayer;
		public GiveType Type;
		public float GiveValue;

		public int ID { get; set; }

		public string ToString(GameDatabase game)
		{
			string str = "Unknown request type!";
			if ((double)this.GiveValue == 0.0)
				return "";
			switch (this.Type)
			{
				case GiveType.GiveSavings:
					str = string.Format(App.Localize("@UI_DIPLOMACY_GIVE_SAVINGS"), (object)this.GiveValue);
					break;
				case GiveType.GiveResearchPoints:
					str = string.Format(App.Localize("@UI_DIPLOMACY_GIVE_RESEARCH_MONEY"), (object)this.GiveValue);
					break;
			}
			return str;
		}
	}
}
