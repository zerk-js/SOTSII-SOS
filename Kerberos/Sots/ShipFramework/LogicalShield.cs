// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalShield
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalShield
	{
		public string Name = "";
		public string TechID = "";
		public ShieldData CRShieldData = new ShieldData();
		public ShieldData DNShieldData = new ShieldData();
		public LogicalShield.ShieldType Type;

		public enum ShieldType
		{
			DEFLECTOR_SHIELD,
			DISRUPTOR_SHIELD,
			SHIELD_MK_I,
			SHIELD_MK_II,
			SHIELD_MK_III,
			SHIELD_MK_IV,
			STRUCTURAL_FIELDS,
			RECHARGERS,
			MESON_SHIELD,
			GRAV_SHIELD,
			SHIELD_PROJECTORS,
			FOCUSED_SHIELDING,
			PSI_SHIELD,
		}
	}
}
