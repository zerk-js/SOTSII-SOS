// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalTurretClass
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kerberos.Sots.ShipFramework
{
	public class LogicalTurretClass
	{
		public WeaponEnums.TurretClasses TurretClass;
		public WeaponEnums.WeaponSizes TurretSize;
		public string BarrelModelName;
		public string TurretModelName;
		public string BaseModelName;

		public static LogicalTurretClass GetLogicalTurretClassForMount(
		  IEnumerable<LogicalTurretClass> turretClasses,
		  string defaultWeaponSize,
		  string defaultWeaponClass,
		  string mountSize,
		  string mountClass)
		{
			WeaponEnums.WeaponSizes result1;
			WeaponEnums.TurretClasses result2;
			WeaponEnums.WeaponSizes result3;
			WeaponEnums.TurretClasses result4;
			if (!Enum.TryParse<WeaponEnums.WeaponSizes>(defaultWeaponSize, out result1) || !Enum.TryParse<WeaponEnums.TurretClasses>(defaultWeaponClass, out result2) || (!Enum.TryParse<WeaponEnums.WeaponSizes>(mountSize, out result3) || !Enum.TryParse<WeaponEnums.TurretClasses>(mountClass, out result4)))
				return (LogicalTurretClass)null;
			return LogicalTurretClass.GetLogicalTurretClassForMount(turretClasses, result1, result2, result3, result4);
		}

		public static LogicalTurretClass GetLogicalTurretClassForMount(
		  IEnumerable<LogicalTurretClass> turretClasses,
		  WeaponEnums.WeaponSizes defaultWeaponSize,
		  WeaponEnums.TurretClasses defaultWeaponClass,
		  WeaponEnums.WeaponSizes mountSize,
		  WeaponEnums.TurretClasses mountClass)
		{
			foreach (LogicalTurretClass turretClass in turretClasses)
			{
				if (turretClass.TurretClass == mountClass && turretClass.TurretSize == mountSize)
					return turretClass;
			}
			return (LogicalTurretClass)null;
		}

		internal string GetBaseModel(Faction faction, LogicalMount mount, LogicalTurretHousing housing)
		{
			if (!string.IsNullOrEmpty(mount.BaseOverload))
				return faction.GetWeaponModelPath(mount.BaseOverload);
			if (!string.IsNullOrEmpty(this.BaseModelName))
				return faction.GetWeaponModelPath(this.BaseModelName);
			if (!string.IsNullOrEmpty(housing.BaseModelName))
				return faction.GetWeaponModelPath(housing.BaseModelName);
			return string.Empty;
		}

		internal string GetBaseDamageModel(
		  Faction faction,
		  LogicalMount mount,
		  LogicalTurretHousing housing)
		{
			string baseModel = this.GetBaseModel(faction, mount, housing);
			if (!string.IsNullOrEmpty(baseModel))
				return Path.Combine(Path.GetDirectoryName(baseModel), Path.GetFileNameWithoutExtension(baseModel) + "_Damaged" + Path.GetExtension(baseModel));
			return string.Empty;
		}

		internal string GetTurretModelName(
		  Faction faction,
		  LogicalMount mount,
		  LogicalTurretHousing housing)
		{
			if (!string.IsNullOrEmpty(mount.TurretOverload))
				return faction.GetWeaponModelPath(mount.TurretOverload);
			if (!string.IsNullOrEmpty(this.TurretModelName))
				return faction.GetWeaponModelPath(this.TurretModelName);
			if (!string.IsNullOrEmpty(housing.ModelName))
				return faction.GetWeaponModelPath(housing.ModelName);
			return faction.GetWeaponModelPath("Turret_Dummy.scene");
		}

		internal string GetBarrelModelName(Faction faction, LogicalMount mount)
		{
			if (!string.IsNullOrEmpty(mount.BarrelOverload))
				return faction.GetWeaponModelPath(mount.BarrelOverload);
			if (!string.IsNullOrEmpty(this.BarrelModelName))
				return faction.GetWeaponModelPath(this.BarrelModelName);
			return faction.GetWeaponModelPath("Turret_Dummy.scene");
		}
	}
}
