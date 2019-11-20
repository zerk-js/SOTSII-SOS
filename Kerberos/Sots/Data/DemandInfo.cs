// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DemandInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class DemandInfo
	{
		public int InitiatingPlayer;
		public int ReceivingPlayer;
		public DemandType Type;
		public float DemandValue;
		public AgreementState State;

		public int ID { get; set; }

		public string ToString(GameDatabase game)
		{
			string str = "Unknown demand type!";
			if (this.Type != DemandType.SurrenderDemand && (double)this.DemandValue == 0.0)
				return "";
			switch (this.Type)
			{
				case DemandType.SavingsDemand:
					str = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SAVINGS_DESC"), (object)this.DemandValue);
					break;
				case DemandType.SystemInfoDemand:
					str = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SYSTEMINFO_DESC"), (object)game.GetStarSystemInfo((int)this.DemandValue).Name);
					break;
				case DemandType.ResearchPointsDemand:
					str = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_RESEARCH_DESC"), (object)this.DemandValue);
					break;
				case DemandType.SlavesDemand:
					str = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SLAVES_DESC"), (object)this.DemandValue);
					break;
				case DemandType.WorldDemand:
					str = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_WORLD_DESC"), (object)game.GetStarSystemInfo((int)this.DemandValue).Name);
					break;
				case DemandType.ProvinceDemand:
					str = string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_PROVINCE_DESC"), (object)game.GetProvinceInfo((int)this.DemandValue).Name);
					break;
				case DemandType.SurrenderDemand:
					str = App.Localize("@UI_DIPLOMACY_DEMAND_SURRENDER_DESC");
					break;
			}
			return str;
		}
	}
}
