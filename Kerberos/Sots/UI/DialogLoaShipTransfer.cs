// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogLoaShipTransfer
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System;

namespace Kerberos.Sots.UI
{
	internal class DialogLoaShipTransfer : Dialog
	{
		private static string PointSlider = "LoaPointSlider";
		private static string dialogok = "loa_dialog_ok";
		private static string dialogeventtext = "event_text";
		private const string PointEditBox = "LoaPointValue";
		private FleetInfo _targetFleet;
		private FleetInfo _sourceFleet;
		private ShipInfo _ship;
		private int _defaulttransfer;
		private int _numtotransfer;

		public DialogLoaShipTransfer(
		  App app,
		  int targetFleet,
		  int sourceFleet,
		  int shipid,
		  int DefaultTransfer = 1)
		  : base(app, "LoaCubeTransfer")
		{
			this._targetFleet = app.GameDatabase.GetFleetInfo(targetFleet);
			this._sourceFleet = app.GameDatabase.GetFleetInfo(sourceFleet);
			this._ship = app.GameDatabase.GetShipInfo(shipid, false);
			this._defaulttransfer = DefaultTransfer;
		}

		public override void Initialize()
		{
			this._app.UI.SetSliderRange(this._app.UI.Path(this.ID, DialogLoaShipTransfer.PointSlider), this._defaulttransfer, this._ship.LoaCubes);
			this._app.UI.SetText(this._app.UI.Path(this.ID, DialogLoaShipTransfer.dialogeventtext), string.Format("Transfer cubes to {0} from {1}", (object)this._targetFleet.Name, (object)this._sourceFleet.Name));
			this._app.UI.SetSliderValue(this._app.UI.Path(this.ID, DialogLoaShipTransfer.PointSlider), this._defaulttransfer);
			this._app.UI.SetText(this._app.UI.Path(this.ID, "LoaPointValue"), this._defaulttransfer.ToString());
			this._numtotransfer = 1;
			this.UpdateSlider();
		}

		private void UpdateSlider()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, DialogLoaShipTransfer.PointSlider, "right_label"), "text", this._numtotransfer.ToString());
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_item_dblclk")
				return;
			if (msgType == "slider_value")
			{
				if (!(panelName == DialogLoaShipTransfer.PointSlider))
					return;
				this._numtotransfer = (int)float.Parse(msgParams[0]);
				this._app.UI.SetText(this._app.UI.Path(this.ID, "LoaPointValue"), msgParams[0]);
				this.UpdateSlider();
			}
			else if (msgType == "text_changed")
			{
				int result;
				if (int.TryParse(msgParams[0], out result))
				{
					int num = Math.Max(this._defaulttransfer, Math.Min(result, this._ship.LoaCubes));
					this._app.UI.SetSliderValue(this._app.UI.Path(this.ID, DialogLoaShipTransfer.PointSlider), num);
					this._numtotransfer = num;
				}
				else if (msgParams[0] == string.Empty)
				{
					int defaulttransfer = this._defaulttransfer;
					this._app.UI.SetSliderValue(this._app.UI.Path(this.ID, "LoaPointValue"), defaulttransfer);
					this._numtotransfer = defaulttransfer;
				}
				this.UpdateSlider();
			}
			else
			{
				if (msgType == "list_sel_changed")
					return;
				if (msgType == "button_clicked")
				{
					if (!(panelName == DialogLoaShipTransfer.dialogok))
						return;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					int num = msgType == "dialog_closed" ? 1 : 0;
				}
			}
		}

		public override string[] CloseDialog()
		{
			return new string[4]
			{
		this._targetFleet.ID.ToString(),
		this._sourceFleet.ID.ToString(),
		this._ship.ID.ToString(),
		this._numtotransfer.ToString()
			};
		}
	}
}
