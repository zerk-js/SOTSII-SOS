// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RequestScalarDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class RequestScalarDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string MinValueLabel = "lblMinValue";
		private const string MaxValueLabel = "lblMaxValue";
		private const string ValueSlider = "sldValue";
		private const string ValueEditBox = "txtValue";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private int _otherPlayer;
		private RequestType _type;
		private RequestInfo _request;

		public RequestScalarDialog(App game, RequestType type, int otherPlayer, string template = "dialogRequestScalar")
		  : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._request = new RequestInfo();
			this._request.InitiatingPlayer = game.LocalPlayer.ID;
			this._request.ReceivingPlayer = this._otherPlayer;
			this._request.State = AgreementState.Unrequested;
			this._request.Type = type;
		}

		public override void Initialize()
		{
			DiplomacyUI.SyncDiplomacyPopup(this._app, this.ID, this._otherPlayer);
			this.SyncScalar();
		}

		private void SyncScalar()
		{
			float num1 = 0.0f;
			float num2 = 0.0f;
			switch (this._type)
			{
				case RequestType.SavingsRequest:
					num1 = 2.5E+07f;
					num2 = 1f;
					break;
				case RequestType.ResearchPointsRequest:
					num1 = 2.5E+07f;
					num2 = 1f;
					break;
			}
			this._app.UI.SetText("lblHeader", string.Format(App.Localize(RequestTypeDialog.RequestTypeLocMap[this._type]), (object)this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type)));
			this._app.UI.SetSliderRange("sldValue", (int)num2, (int)num1);
			this._app.UI.SetText("lblMinValue", num2.ToString("N0"));
			this._app.UI.SetText("lblMaxValue", num1.ToString("N0"));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnFinishRequest")
				{
					this._app.GameDatabase.SpendDiplomacyPoints(this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID), this._app.GameDatabase.GetPlayerFactionID(this._otherPlayer), this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type));
					this._app.GameDatabase.InsertRequest(this._request);
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					if (!(panelName == "btnCancel"))
						return;
					this._request = (RequestInfo)null;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else if (msgType == "slider_value")
			{
				if (!(panelName == "sldValue"))
					return;
				this._request.RequestValue = float.Parse(msgParams[0]);
				this._app.UI.SetText("txtValue", msgParams[0]);
			}
			else
			{
				int result;
				if (!(msgType == "text_changed") || !int.TryParse(msgParams[0], out result))
					return;
				this._request.RequestValue = (float)result;
				this._app.UI.SetSliderValue("sldValue", result);
			}
		}

		public override string[] CloseDialog()
		{
			List<string> stringList = new List<string>();
			if (this._request == null)
				stringList.Add("true");
			else
				stringList.Add("false");
			return stringList.ToArray();
		}
	}
}
