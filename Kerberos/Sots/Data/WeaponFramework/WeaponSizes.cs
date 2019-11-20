// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.WeaponSizes
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public static class WeaponSizes
	{
		private static readonly Dictionary<WeaponEnums.WeaponSizes, WeaponEnums.WeaponSizes[]> AllowedMountWeaponSizeMap = new Dictionary<WeaponEnums.WeaponSizes, WeaponEnums.WeaponSizes[]>();

		static WeaponSizes()
		{
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.VeryLight] = new WeaponEnums.WeaponSizes[1];
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.Light] = new WeaponEnums.WeaponSizes[2]
			{
		WeaponEnums.WeaponSizes.VeryLight,
		WeaponEnums.WeaponSizes.Light
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.Medium] = new WeaponEnums.WeaponSizes[2]
			{
		WeaponEnums.WeaponSizes.Light,
		WeaponEnums.WeaponSizes.Medium
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.Heavy] = new WeaponEnums.WeaponSizes[3]
			{
		WeaponEnums.WeaponSizes.Light,
		WeaponEnums.WeaponSizes.Medium,
		WeaponEnums.WeaponSizes.Heavy
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.VeryHeavy] = new WeaponEnums.WeaponSizes[3]
			{
		WeaponEnums.WeaponSizes.Medium,
		WeaponEnums.WeaponSizes.Heavy,
		WeaponEnums.WeaponSizes.VeryHeavy
			};
			WeaponSizes.AllowedMountWeaponSizeMap[WeaponEnums.WeaponSizes.SuperHeavy] = new WeaponEnums.WeaponSizes[3]
			{
		WeaponEnums.WeaponSizes.Heavy,
		WeaponEnums.WeaponSizes.VeryHeavy,
		WeaponEnums.WeaponSizes.SuperHeavy
			};
		}

		public static int GuessNumBarrels(
		  WeaponEnums.WeaponSizes weaponSize,
		  WeaponEnums.WeaponSizes mountSize)
		{
			return Math.Max(1, mountSize - weaponSize + 1);
		}

		public static bool WeaponSizeFitsMount(
		  WeaponEnums.WeaponSizes weaponSize,
		  WeaponEnums.WeaponSizes mountSize)
		{
			return ((IEnumerable<WeaponEnums.WeaponSizes>)WeaponSizes.AllowedMountWeaponSizeMap[mountSize]).Contains<WeaponEnums.WeaponSizes>(weaponSize);
		}
	}
}
