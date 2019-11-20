// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.WeaponScalarStats
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.UI
{
	internal class WeaponScalarStats : PanelBinding
	{
		private readonly ImageLabel _rateOfFireLabel;
		private readonly ImageLabel _popDamageLabel;
		private readonly ImageLabel _infraDamageLabel;
		private readonly ImageLabel _terraDamageLabel;

		public WeaponScalarStats(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._rateOfFireLabel = new ImageLabel(ui, this.UI.Path(this.ID, "rofIconLabel"));
			this._popDamageLabel = new ImageLabel(ui, this.UI.Path(this.ID, "popIconLabel"));
			this._infraDamageLabel = new ImageLabel(ui, this.UI.Path(this.ID, "infraIconLabel"));
			this._terraDamageLabel = new ImageLabel(ui, this.UI.Path(this.ID, "terraIconLabel"));
		}

		private static string GetSignSuffix(float delta)
		{
			if ((double)delta > 0.0)
				return " (+)";
			if ((double)delta < 0.0)
				return " (-)";
			return string.Empty;
		}

		public void SetWeapons(LogicalWeapon primary, LogicalWeapon comparative)
		{
			bool flag = primary != null;
			this._rateOfFireLabel.Label.SetVisible(flag);
			this._popDamageLabel.Label.SetVisible(flag);
			this._infraDamageLabel.Label.SetVisible(flag);
			this._terraDamageLabel.Label.SetVisible(flag);
			if (primary == null)
				return;
			float num1 = primary.GetRateOfFire() * 60f;
			float popDamage1 = primary.PopDamage;
			float num2 = primary.InfraDamage * 100f;
			float terraDamage1 = primary.TerraDamage;
			if (comparative != null)
			{
				float num3 = comparative.GetRateOfFire() * 60f;
				float popDamage2 = comparative.PopDamage;
				float num4 = comparative.InfraDamage * 100f;
				float terraDamage2 = comparative.TerraDamage;
				float delta1 = num1 - num3;
				float delta2 = popDamage1 - popDamage2;
				float delta3 = num2 - num4;
				float delta4 = terraDamage1 - terraDamage2;
				this._rateOfFireLabel.Label.SetText(num1.ToString("N1") + WeaponScalarStats.GetSignSuffix(delta1));
				this._popDamageLabel.Label.SetText(popDamage1.ToString("N0") + WeaponScalarStats.GetSignSuffix(delta2));
				this._infraDamageLabel.Label.SetText(num2.ToString("N3") + WeaponScalarStats.GetSignSuffix(delta3));
				this._terraDamageLabel.Label.SetText(terraDamage1.ToString("N2") + WeaponScalarStats.GetSignSuffix(delta4));
			}
			else
			{
				this._rateOfFireLabel.Label.SetText(num1.ToString("N1"));
				this._popDamageLabel.Label.SetText(popDamage1.ToString("N0"));
				this._infraDamageLabel.Label.SetText(num2.ToString("N5"));
				this._terraDamageLabel.Label.SetText(terraDamage1.ToString("N2"));
			}
		}
	}
}
