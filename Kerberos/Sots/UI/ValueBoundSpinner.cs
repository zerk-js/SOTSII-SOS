// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ValueBoundSpinner
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.UI
{
	internal class ValueBoundSpinner : Spinner
	{
		public double Value { get; private set; }

		public double MinValue { get; private set; }

		public double MaxValue { get; private set; }

		public double RateOfChange { get; private set; }

		public string ValueText { get; private set; }

		public string ValuePath { get; private set; }

		private void PostValueChanged(double value)
		{
			this.Value = value;
			this.OnValueChanged(value);
			if (this.ValueChanged != null)
				this.ValueChanged((object)this, new ValueChangedEventArgs(this.Value));
			this.PostValueText(value);
		}

		public void SetValueDescriptor(SpinnerValueDescriptor svd)
		{
			this.MinValue = svd.min;
			this.MaxValue = svd.max;
			this.RateOfChange = svd.rateOfChange;
			if ((this.Value - svd.max) % svd.rateOfChange == 0.0)
				this.Value = Math.Max(svd.min, Math.Min(svd.max, this.Value));
			else
				this.Value = svd.min;
		}

		public void SetValue(double value)
		{
			if (Math.Max(Math.Min(value, this.MaxValue), this.MinValue) == this.Value)
				return;
			this.PostValueChanged(value);
		}

		public void SetMin(double min)
		{
			this.MinValue = min;
		}

		public void SetMax(double max)
		{
			this.MaxValue = max;
		}

		public void SetRateOfChange(double rateOfChange)
		{
			this.RateOfChange = rateOfChange;
		}

		protected virtual string OnFormatValueText(double value)
		{
			return value.ToString();
		}

		private void PostValueText(double value)
		{
			this.ValueText = this.OnFormatValueText(value);
			this.UI.SetText(this.ValuePath, this.ValueText);
		}

		protected override void OnClick(Spinner.Direction direction)
		{
			base.OnClick(direction);
			switch (direction)
			{
				case Spinner.Direction.Up:
					this.SetValue(this.Value + this.RateOfChange);
					break;
				case Spinner.Direction.Down:
					this.SetValue(this.Value - this.RateOfChange);
					break;
			}
		}

		public event ValueChangedEventHandler ValueChanged;

		protected virtual void OnValueChanged(double newValue)
		{
		}

		public ValueBoundSpinner(UICommChannel ui, string id, SpinnerValueDescriptor svd)
		  : this(ui, id, svd.min, svd.max, svd.min, svd.rateOfChange)
		{
		}

		public ValueBoundSpinner(
		  UICommChannel ui,
		  string id,
		  SpinnerValueDescriptor svd,
		  double initialValue)
		  : this(ui, id, svd.min, svd.max, initialValue, svd.rateOfChange)
		{
		}

		public ValueBoundSpinner(
		  UICommChannel ui,
		  string id,
		  double minValue,
		  double maxValue,
		  double initialValue,
		  double rateOfChange = 1.0)
		  : base(ui, id)
		{
			this.ValueText = string.Empty;
			this.ValuePath = this.UI.Path(id, "parent", "value");
			this.MinValue = minValue;
			this.MaxValue = Math.Max(minValue, maxValue);
			this.RateOfChange = rateOfChange;
			this.PostValueChanged(initialValue);
		}
	}
}
