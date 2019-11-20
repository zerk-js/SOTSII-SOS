// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PercentageSlider
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class PercentageSlider : ValueBoundSlider
	{
		protected override string OnFormatValueText(int value)
		{
			return this.Value.ToString() + "%";
		}

		public PercentageSlider(
		  UICommChannel ui,
		  string id,
		  int initialPercentage,
		  int minPercentage,
		  int maxPercentage)
		  : base(ui, id)
		{
			this.Initialize(minPercentage, maxPercentage, initialPercentage);
		}
	}
}
