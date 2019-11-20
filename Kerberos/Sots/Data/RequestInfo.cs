// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.RequestInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class RequestInfo
	{
		public int InitiatingPlayer;
		public int ReceivingPlayer;
		public RequestType Type;
		public float RequestValue;
		public AgreementState State;

		public int ID { get; set; }

		public string ToString(GameDatabase game)
		{
			string str = "Unknown request type!";
			if ((double)this.RequestValue == 0.0)
				return "";
			switch (this.Type)
			{
				case RequestType.SavingsRequest:
					str = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SAVINGS_DESC"), (object)this.RequestValue);
					break;
				case RequestType.SystemInfoRequest:
					str = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SYSTEMINFO_DESC"), (object)game.GetStarSystemInfo((int)this.RequestValue).Name);
					break;
				case RequestType.ResearchPointsRequest:
					str = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_RESEARCHPOINTS_DESC"), (object)this.RequestValue);
					break;
				case RequestType.MilitaryAssistanceRequest:
					str = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_MILITARYASSISTANCE_DESC"), (object)game.GetStarSystemInfo((int)this.RequestValue).Name);
					break;
				case RequestType.GatePermissionRequest:
					str = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_GATEPERMISSION_DESC"), (object)game.GetStarSystemInfo((int)this.RequestValue).Name);
					break;
				case RequestType.EstablishEnclaveRequest:
					str = string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_ESTABLISHENCLAVE_DESC"), (object)game.GetStarSystemInfo((int)this.RequestValue).Name);
					break;
			}
			return str;
		}
	}
}
