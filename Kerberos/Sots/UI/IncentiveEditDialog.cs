// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.IncentiveEditDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class IncentiveEditDialog : Dialog
	{
		public static Dictionary<IncentiveType, SpinnerValueDescriptor> IncentiveTypeSpinnerDescriptors = new Dictionary<IncentiveType, SpinnerValueDescriptor>()
	{
	  {
		IncentiveType.ResearchPoints,
		new SpinnerValueDescriptor()
		{
		  min = 1.0,
		  max = 100.0,
		  rateOfChange = 1.0
		}
	  },
	  {
		IncentiveType.Savings,
		new SpinnerValueDescriptor()
		{
		  min = 50000.0,
		  max = 999999999999.0,
		  rateOfChange = 50000.0
		}
	  }
	};
		public const string DoneButton = "btnDone";
		public const string TypeList = "lstType";
		public const string ValueEditBox = "txtValue";
		private TreatyIncentiveInfo _editedIncentive;
		private Vector3 _panelColor;
		private ValueBoundSpinner _valueSpinner;

		public IncentiveEditDialog(
		  App game,
		  ref TreatyIncentiveInfo tci,
		  Vector3 panelColor,
		  string template = "TreatyConsequencePopup")
		  : base(game, template)
		{
			this._editedIncentive = tci;
			this._panelColor = panelColor;
		}

		public override void Initialize()
		{
			DiplomacyUI.SyncPanelColor(this._app, this.ID, this._panelColor);
			this._app.UI.ClearItems("lstType");
			foreach (KeyValuePair<IncentiveType, string> incentiveTypeLoc in TreatyEditDialog.IncentiveTypeLocMap)
				this._app.UI.AddItem("lstType", string.Empty, (int)incentiveTypeLoc.Key, App.Localize(incentiveTypeLoc.Value));
			this._app.UI.SetSelection("lstType", (int)this._editedIncentive.Type);
			this._app.UI.SetPropertyString("txtValue", "text", this._editedIncentive.IncentiveValue.ToString());
			this._valueSpinner = new ValueBoundSpinner(this.UI, "spnValue", IncentiveEditDialog.IncentiveTypeSpinnerDescriptors[this._editedIncentive.Type]);
			this._valueSpinner.ValueChanged += new ValueChangedEventHandler(this._valueSpinner_ValueChanged);
		}

		private void _valueSpinner_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			this._editedIncentive.IncentiveValue = (float)e.NewValue;
			this._app.UI.SetText("txtValue", this._editedIncentive.IncentiveValue.ToString());
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._valueSpinner.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Self | PanelBinding.PanelMessageTargetFlags.Recursive))
				return;
			if (msgType == "list_sel_changed")
			{
				if (!(panelName == "lstType"))
					return;
				this._editedIncentive.Type = (IncentiveType)int.Parse(msgParams[0]);
				this._valueSpinner.SetValueDescriptor(IncentiveEditDialog.IncentiveTypeSpinnerDescriptors[this._editedIncentive.Type]);
				this._editedIncentive.IncentiveValue = (float)this._valueSpinner.Value;
				this._app.UI.SetPropertyString("txtValue", "text", this._editedIncentive.IncentiveValue.ToString());
			}
			else if (msgType == "button_clicked")
			{
				if (!(panelName == "btnDone"))
					return;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(msgType == "text_changed"))
					return;
				float result = 0.0f;
				if (!float.TryParse(msgParams[0], out result))
					return;
				this._editedIncentive.IncentiveValue = result;
			}
		}

		public override string[] CloseDialog()
		{
			return new List<string>().ToArray();
		}
	}
}
