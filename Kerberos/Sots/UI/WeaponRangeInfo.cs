// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.WeaponRangeInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.UI
{
	internal class WeaponRangeInfo : PanelBinding
	{
		private readonly Label _rangeLabel;
		private readonly Label _deviationLabel;
		private readonly Label _damageLabel;

		public WeaponRangeInfo(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._rangeLabel = new Label(ui, this.UI.Path(this.ID, "rangeLabel"));
			this._deviationLabel = new Label(ui, this.UI.Path(this.ID, "deviationLabel"));
			this._damageLabel = new Label(ui, this.UI.Path(this.ID, "damageLabel"));
		}

		public void SetRangeInfo(WeaponRangeTableItem rangeTableItem)
		{
			this._rangeLabel.SetText(rangeTableItem.Range.ToString("N0"));
			this._deviationLabel.SetText(rangeTableItem.Deviation.ToString("N1"));
			this._damageLabel.SetText(rangeTableItem.Damage.ToString("N0"));
		}
	}
}
