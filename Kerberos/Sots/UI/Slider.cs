// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Slider
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.UI
{
	internal class Slider : PanelBinding
	{
		public int Value { get; private set; }

		public int MinValue { get; private set; }

		public int MaxValue { get; private set; }

		protected void SetValue(int value)
		{
			value = Math.Max(Math.Min(value, this.MaxValue), this.MinValue);
			if (value == this.Value)
				return;
			this.UI.SetSliderValue(this.ID, value);
			this.PostValueChanged(value);
		}

		public event ValueChangedEventHandler ValueChanged;

		protected virtual void OnValueChanged(int newValue)
		{
		}

		private void PostValueChanged(int value)
		{
			this.Value = value;
			this.OnValueChanged(this.Value);
			if (this.ValueChanged == null)
				return;
			this.ValueChanged((object)this, new ValueChangedEventArgs((double)this.Value));
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			if (!(msgType == "slider_value"))
				return;
			this.PostValueChanged(int.Parse(msgParams[0]));
		}

		protected virtual void OnInitialized()
		{
		}

		protected void Initialize(int minValue, int maxValue, int initialValue)
		{
			this.UI.InitializeSlider(this.ID, minValue, maxValue, initialValue);
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this.Value = initialValue;
			this.OnInitialized();
		}

		public Slider(UICommChannel ui, string id)
		  : base(ui, id)
		{
		}
	}
}
