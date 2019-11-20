// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.TimeSlider
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.UI
{
	internal class TimeSlider : ValueBoundSlider
	{
		public bool SupportsInfinity { get; private set; }

		public float MinTimeInMinutes { get; private set; }

		public float MaxTimeInMinutes { get; private set; }

		public float TimeInMinutesGranularity { get; private set; }

		public float TimeInMinutes { get; private set; }

		protected override string OnFormatValueText(int value)
		{
			if (this.TimeInMinutes == float.MaxValue)
				return "∞";
			TimeSpan timeSpan = TimeSpan.FromMinutes((double)this.TimeInMinutes);
			return timeSpan.Minutes.ToString() + ":" + timeSpan.Seconds.ToString("00");
		}

		private int GetMaxOrInfinityValue()
		{
			return (int)(((double)this.MaxTimeInMinutes - (double)this.MinTimeInMinutes) / (double)this.TimeInMinutesGranularity) + (this.SupportsInfinity ? 1 : 0);
		}

		private int TimeInMinutesToValue(float value)
		{
			if (this.SupportsInfinity && value == float.MaxValue)
				return this.GetMaxOrInfinityValue();
			return (int)(((double)value - (double)this.MinTimeInMinutes) / (double)this.TimeInMinutesGranularity);
		}

		private float ValueToTimeInMinutes(int value)
		{
			if (this.SupportsInfinity && value == this.GetMaxOrInfinityValue())
				return float.MaxValue;
			return this.TimeInMinutesGranularity * (float)value + this.MinTimeInMinutes;
		}

		protected override void OnValueChanged(int newValue)
		{
			this.TimeInMinutes = this.ValueToTimeInMinutes(newValue);
			base.OnValueChanged(newValue);
		}

		public TimeSlider(
		  UICommChannel ui,
		  string id,
		  float initialTimeInMinutes,
		  float minTimeInMinutes,
		  float maxTimeInMinutes,
		  float granularityInMinutes,
		  bool supportsInfinity)
		  : base(ui, id)
		{
			this.TimeInMinutesGranularity = granularityInMinutes;
			this.MinTimeInMinutes = minTimeInMinutes;
			this.MaxTimeInMinutes = maxTimeInMinutes;
			this.SupportsInfinity = supportsInfinity;
			this.Initialize(this.TimeInMinutesToValue(minTimeInMinutes), this.GetMaxOrInfinityValue(), this.TimeInMinutesToValue(initialTimeInMinutes));
			this.TimeInMinutes = initialTimeInMinutes;
		}
	}
}
