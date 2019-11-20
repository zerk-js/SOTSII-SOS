// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ValueBoundSlider
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class ValueBoundSlider : Slider
	{
		public string ValueText { get; private set; }

		public string ValuePath { get; set; }

		protected virtual string OnFormatValueText(int value)
		{
			return value.ToString();
		}

		public override void SetEnabled(bool value)
		{
			base.SetEnabled(value);
			this.UI.SetVisible(this.ID, value);
		}

		private void PostValueText(int value)
		{
			this.ValueText = this.OnFormatValueText(value);
			this.UI.SetText(this.ValuePath, this.ValueText);
		}

		protected override void OnValueChanged(int newValue)
		{
			base.OnValueChanged(newValue);
			this.PostValueText(newValue);
		}

		protected override void OnInitialized()
		{
			base.OnInitialized();
			this.OnValueChanged(this.Value);
			this.PostValueText(this.Value);
		}

		protected ValueBoundSlider(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this.ValueText = string.Empty;
			this.ValuePath = this.UI.Path(id, "parent", "value");
		}

		public ValueBoundSlider(
		  UICommChannel ui,
		  string id,
		  int minValue,
		  int maxValue,
		  int initialValue)
		  : this(ui, id)
		{
			this.Initialize(minValue, maxValue, initialValue);
		}
	}
}
