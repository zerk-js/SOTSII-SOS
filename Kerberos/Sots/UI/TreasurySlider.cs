// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.TreasurySlider
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class TreasurySlider : ValueBoundSlider
	{
		public new void SetValue(int value)
		{
			base.SetValue(value);
		}

		protected override string OnFormatValueText(int value)
		{
			return this.Value.ToString("N0");
		}

		public TreasurySlider(
		  UICommChannel ui,
		  string id,
		  int initialTreasury,
		  int minTreasury,
		  int maxTreasury)
		  : base(ui, id)
		{
			this.Initialize(minTreasury, maxTreasury, initialTreasury);
		}
	}
}
