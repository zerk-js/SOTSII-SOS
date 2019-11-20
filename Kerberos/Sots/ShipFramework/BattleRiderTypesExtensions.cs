// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.BattleRiderTypesExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.ShipFramework
{
	internal static class BattleRiderTypesExtensions
	{
		public static bool IsBattleRiderType(this BattleRiderTypes type)
		{
			switch (type)
			{
				case BattleRiderTypes.nodefighter:
				case BattleRiderTypes.patrol:
				case BattleRiderTypes.scout:
				case BattleRiderTypes.spinal:
				case BattleRiderTypes.escort:
				case BattleRiderTypes.interceptor:
				case BattleRiderTypes.torpedo:
				case BattleRiderTypes.battlerider:
					return true;
				default:
					return false;
			}
		}

		public static bool IsControllableBattleRider(this BattleRiderTypes type)
		{
			switch (type)
			{
				case BattleRiderTypes.Unspecified:
				case BattleRiderTypes.battlerider:
					return true;
				default:
					return false;
			}
		}
	}
}
