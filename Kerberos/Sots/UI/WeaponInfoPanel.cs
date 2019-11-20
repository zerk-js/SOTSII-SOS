// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.WeaponInfoPanel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.UI
{
	internal class WeaponInfoPanel : PanelBinding
	{
		private string _contentPanelID;
		private readonly WeaponDamageGraphPanel _damageGraph;
		private readonly WeaponDeviationGraphPanel _deviationGraph;
		private readonly WeaponInfoPanel.Group _primaryGroup;
		private readonly WeaponInfoPanel.Group _comparativeGroup;
		private readonly WeaponScalarStats _scalarStats;

		public WeaponInfoPanel(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._contentPanelID = this.UI.Path(id, "content");
			this._damageGraph = new WeaponDamageGraphPanel(this.UI, this.UI.Path(this._contentPanelID, "graphGroup.damGraphFrame.damGraph"));
			this._deviationGraph = new WeaponDeviationGraphPanel(this.UI, this.UI.Path(this._contentPanelID, "graphGroup.devGraphFrame.devGraph"));
			this._scalarStats = new WeaponScalarStats(this.UI, this.UI.Path(this._contentPanelID, "scalarStats"));
			this._primaryGroup = new WeaponInfoPanel.Group(this.UI, this._contentPanelID, "1", string.Empty);
			this._comparativeGroup = new WeaponInfoPanel.Group(this.UI, this._contentPanelID, "2", AssetDatabase.CommonStrings.Localize("@UI_COMPARATIVE_WEAPON_VERSUS") + " ");
		}

		public void SetWeapons(LogicalWeapon primary, LogicalWeapon comparative)
		{
			WeaponRangeTable rangeTable1 = primary?.RangeTable;
			WeaponRangeTable rangeTable2 = comparative?.RangeTable;
			this._damageGraph.SetRangeTables(rangeTable1, rangeTable2);
			this._deviationGraph.SetRangeTables(rangeTable1, rangeTable2);
			this._scalarStats.SetWeapons(primary, comparative);
			this._primaryGroup.SetWeapon(primary);
			this._comparativeGroup.SetWeapon(comparative);
		}

		private class Group
		{
			private readonly ImageLabel _titleImageLabel;
			private readonly WeaponRangeInfo _pointBlankRangeInfo;
			private readonly WeaponRangeInfo _effectiveRangeInfo;
			private readonly WeaponRangeInfo _maximumRangeInfo;
			private readonly string _titlePrefix;

			public Group(UICommChannel ui, string rootId, string idSuffix, string titlePrefix)
			{
				this._titleImageLabel = new ImageLabel(ui, ui.Path(rootId, "title" + idSuffix));
				this._pointBlankRangeInfo = new WeaponRangeInfo(ui, ui.Path(rootId, "rangeInfo.pb" + idSuffix));
				this._effectiveRangeInfo = new WeaponRangeInfo(ui, ui.Path(rootId, "rangeInfo.eff" + idSuffix));
				this._maximumRangeInfo = new WeaponRangeInfo(ui, ui.Path(rootId, "rangeInfo.max" + idSuffix));
				this._titlePrefix = titlePrefix;
			}

			public void SetWeapon(LogicalWeapon weapon)
			{
				if (weapon != null)
				{
					this._titleImageLabel.Image.SetTexture(weapon.IconTextureName);
					this._titleImageLabel.Label.SetText(this._titlePrefix + weapon.Name);
					this._pointBlankRangeInfo.SetRangeInfo(weapon.RangeTable.PointBlank);
					this._effectiveRangeInfo.SetRangeInfo(weapon.RangeTable.Effective);
					this._maximumRangeInfo.SetRangeInfo(weapon.RangeTable.Maximum);
				}
				bool flag = weapon != null;
				this._titleImageLabel.SetVisible(flag);
				this._pointBlankRangeInfo.SetVisible(flag);
				this._effectiveRangeInfo.SetVisible(flag);
				this._maximumRangeInfo.SetVisible(flag);
			}
		}
	}
}
