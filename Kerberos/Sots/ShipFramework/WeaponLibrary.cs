// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.WeaponLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.ShipFramework
{
	internal static class WeaponLibrary
	{
		public static LogicalWeapon CreateLogicalWeaponFromFile(
		  App app,
		  string weaponFile,
		  int uniqueWeaponID = -1)
		{
			Kerberos.Sots.Data.WeaponFramework.Weapon w = new Kerberos.Sots.Data.WeaponFramework.Weapon();
			WeaponXmlUtility.LoadWeaponFromXml(weaponFile, ref w);
			LogicalWeapon logicalWeapon = new LogicalWeapon(app)
			{
				UniqueWeaponID = uniqueWeaponID,
				FileName = Path.Combine("weapons", Path.GetFileName(weaponFile)),
				WeaponName = Path.GetFileNameWithoutExtension(weaponFile),
				PayloadType = !string.IsNullOrEmpty(w.PayloadType) ? (WeaponEnums.PayloadTypes)Enum.Parse(typeof(WeaponEnums.PayloadTypes), w.PayloadType) : WeaponEnums.PayloadTypes.Bolt,
				PlagueType = !string.IsNullOrEmpty(w.PlagueType) ? (WeaponEnums.PlagueType)Enum.Parse(typeof(WeaponEnums.PlagueType), w.PlagueType) : WeaponEnums.PlagueType.NONE,
				Duration = w.Duration,
				TimeToLive = w.TimeToLive,
				Model = w.Model,
				IsVisible = w.isVisible,
				SubWeapon = w.SubmunitionType,
				SecondaryWeapon = w.SubweaponType,
				NumSubWeapons = w.SubmunitionAmount,
				SubMunitionBlastType = !string.IsNullOrEmpty(w.SubmunitionBlastType) ? (WeaponEnums.SubmunitionBlastType)Enum.Parse(typeof(WeaponEnums.SubmunitionBlastType), w.SubmunitionBlastType) : WeaponEnums.SubmunitionBlastType.Focus,
				SubmunitionConeDeviation = w.SubmunitionConeDeviation,
				CritHitRolls = Math.Max(w.CritHitRolls, 1),
				NumArcs = w.NumArcs,
				Cost = w.Cost,
				Range = w.EffectiveRanges.MaxRange,
				IconSpriteName = w.Icon
			};
			logicalWeapon.IconTextureName = Path.Combine("weapons\\Icons", logicalWeapon.IconSpriteName + ".bmp");
			logicalWeapon.RechargeTime = w.BaseRechargeTime;
			logicalWeapon.BuildupTime = w.BuildupDelay;
			logicalWeapon.ShotDelay = w.VolleyDelay;
			logicalWeapon.VolleyPeriod = w.VolleyPeriod;
			logicalWeapon.VolleyDeviation = w.BaseVolleyDeviation;
			logicalWeapon.Health = w.Health;
			logicalWeapon.TrackSpeedModifier = w.TrackSpeedModifier;
			logicalWeapon.NumVolleys = Math.Max(w.Volleys, 1);
			logicalWeapon.DefaultWeaponSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), w.WeaponSize);
			logicalWeapon.Speed = w.MuzzleSpeed;
			logicalWeapon.Mass = w.RoundMass;
			logicalWeapon.Acceleration = w.ShotAcceleration;
			logicalWeapon.SolutionTolerance = w.SolutionTolerance;
			logicalWeapon.DumbFireTime = w.DumbfireTime;
			logicalWeapon.CritHitBonus = w.CritHitBonus;
			logicalWeapon.Signature = w.Signature;
			logicalWeapon.RicochetModifier = w.BaseRicochetModifier;
			logicalWeapon.MalfunctionDamage = w.MalfunctionDamage;
			logicalWeapon.MalfunctionPercent = w.MalfunctionPercent;
			logicalWeapon.Crew = w.Crew;
			logicalWeapon.isCrewPerBank = w.isCrewPerBank;
			logicalWeapon.Power = w.Power;
			logicalWeapon.isPowerPerBank = w.isPowerPerBank;
			logicalWeapon.Supply = w.Supply;
			logicalWeapon.isSupplyPerBank = w.isSupplyPerBank;
			logicalWeapon.TurretClasses = w.TurretClasses.SelectMany<TurretClass, LogicalTurretClass>((Func<TurretClass, IEnumerable<LogicalTurretClass>>)(x => x.GetLogicalTurretClasses(false))).ToArray<LogicalTurretClass>();
			logicalWeapon.Traits = w.Attributes.Select<Kerberos.Sots.Data.WeaponFramework.Attribute, WeaponEnums.WeaponTraits>((Func<Kerberos.Sots.Data.WeaponFramework.Attribute, WeaponEnums.WeaponTraits>)(x => (WeaponEnums.WeaponTraits)Enum.Parse(typeof(WeaponEnums.WeaponTraits), x.Name))).ToArray<WeaponEnums.WeaponTraits>();
			logicalWeapon.CompatibleSections = w.CompatibleSections.Select<Section, string>((Func<Section, string>)(x => x.Name)).ToArray<string>();
			logicalWeapon.DeployableSections = w.DeployableSections.Select<Section, string>((Func<Section, string>)(x => x.Name)).ToArray<string>();
			logicalWeapon.CompatibleFactions = w.CompatibleFactions.Select<CompatibleFaction, string>((Func<CompatibleFaction, string>)(x => x.Name)).ToArray<string>();
			logicalWeapon.RangeTable = new WeaponRangeTable();
			logicalWeapon.RangeTable.PointBlank.Range = w.EffectiveRanges.PbRange;
			logicalWeapon.RangeTable.PointBlank.Damage = w.EffectiveRanges.PbDamage;
			logicalWeapon.RangeTable.PointBlank.Deviation = w.EffectiveRanges.PbDeviation;
			logicalWeapon.RangeTable.Effective.Range = w.EffectiveRanges.EffectiveRange;
			logicalWeapon.RangeTable.Effective.Damage = w.EffectiveRanges.EffectiveDamage;
			logicalWeapon.RangeTable.Effective.Deviation = w.EffectiveRanges.EffectiveDeviation;
			logicalWeapon.RangeTable.Maximum.Range = w.EffectiveRanges.MaxRange;
			logicalWeapon.RangeTable.Maximum.Damage = w.EffectiveRanges.MaxDamage;
			logicalWeapon.RangeTable.Maximum.Deviation = w.EffectiveRanges.MaxDeviation;
			logicalWeapon.RangeTable.PlanetRange = w.EffectiveRanges.PlanetRange;
			logicalWeapon.PopDamage = w.PopDamage;
			logicalWeapon.InfraDamage = w.InfraDamage;
			logicalWeapon.TerraDamage = w.TerraDamage;
			logicalWeapon.MuzzleSize = w.MuzzleSize;
			logicalWeapon.Animation = w.Animation ?? string.Empty;
			logicalWeapon.AnimationDelay = w.AnimationDelay;
			logicalWeapon.ExplosiveMinEffectRange = w.ExplosiveMinEffectRange;
			logicalWeapon.ExplosiveMaxEffectRange = w.ExplosiveMaxEffectRange;
			logicalWeapon.DetonationRange = w.DetonationRange;
			logicalWeapon.MaxGravityForce = w.MaxGravityForce;
			logicalWeapon.GravityAffectRange = w.GravityAffectRange;
			logicalWeapon.ArcRange = w.ArcRange;
			logicalWeapon.EMPRange = w.EMPRange;
			logicalWeapon.EMPDuration = w.EMPDuration;
			logicalWeapon.DOTDamage = w.DOT;
			logicalWeapon.BeamDamagePeriod = (double)w.BeamDamagePeriod > 0.0 ? w.BeamDamagePeriod : 0.5f;
			List<DamagePattern> damagePatternList = new List<DamagePattern>();
			logicalWeapon.ArmorPiercingLevel = w.ArmorPiercingLevel;
			logicalWeapon.DisruptorValue = w.DisruptorValue;
			logicalWeapon.DrainValue = w.DrainValue;
			damagePatternList.Add(new DamagePattern(w.PbGrid.Width, w.PbGrid.Height, w.PbGrid.CollisionX, w.PbGrid.CollisionY, w.PbGrid.Data));
			damagePatternList.Add(new DamagePattern(w.EffectiveGrid.Width, w.EffectiveGrid.Height, w.EffectiveGrid.CollisionX, w.EffectiveGrid.CollisionY, w.EffectiveGrid.Data));
			damagePatternList.Add(new DamagePattern(w.MaxGrid.Width, w.MaxGrid.Height, w.MaxGrid.CollisionX, w.MaxGrid.CollisionY, w.MaxGrid.Data));
			logicalWeapon.DamagePattern = damagePatternList.ToArray();
			logicalWeapon.MuzzleEffect = new LogicalEffect()
			{
				Name = w.MuzzleEffect
			};
			logicalWeapon.BuildupEffect = new LogicalEffect()
			{
				Name = w.BuildupEffect
			};
			logicalWeapon.ImpactEffect = new LogicalEffect()
			{
				Name = w.ImpactEffect
			};
			logicalWeapon.PlanetImpactEffect = new LogicalEffect()
			{
				Name = w.PlanetImpactEffect
			};
			logicalWeapon.BulletEffect = new LogicalEffect()
			{
				Name = w.BulletEffect
			};
			logicalWeapon.RicochetEffect = new LogicalEffect()
			{
				Name = w.RicochetEffect
			};
			logicalWeapon.DamageDecalMaterial = w.DecalMaterial;
			logicalWeapon.DamageDecalSize = w.DecalSize;
			logicalWeapon.isMuzzleEffectLooping = w.isMuzzleEffectLooping;
			logicalWeapon.isBuildupEffectLooping = w.isBuildupEffectLooping;
			logicalWeapon.isImpactEffectLooping = w.isImpactEffectLooping;
			logicalWeapon.isBulletEffectLooping = w.isBulletEffectLooping;
			logicalWeapon.isRicochetEffectLooping = w.isRicochetEffectLooping;
			logicalWeapon.MuzzleSound = w.MuzzleSound;
			logicalWeapon.BuildupSound = w.BuildupSound;
			logicalWeapon.ImpactSound = w.ImpactSound;
			logicalWeapon.PlanetImpactSound = w.PlanetImpactSound ?? string.Empty;
			logicalWeapon.ExpireSound = w.ExpireSound;
			logicalWeapon.BulletSound = w.BulletSound;
			logicalWeapon.RicochetSound = w.RicochetSound;
			logicalWeapon.RequiredTechs = w.RequiredTech.ToArray();
			logicalWeapon.PassThroughShields = w.PassThroughShields.Select<PassableShield, LogicalShield.ShieldType>((Func<PassableShield, LogicalShield.ShieldType>)(x => (LogicalShield.ShieldType)Enum.Parse(typeof(LogicalShield.ShieldType), x.Name))).ToArray<LogicalShield.ShieldType>();
			return logicalWeapon;
		}

		public static IEnumerable<LogicalWeapon> Enumerate(App app)
		{
			string[] files = ScriptHost.FileSystem.FindFiles("weapons\\*.weapon");
			List<LogicalWeapon> logicalWeaponList = new List<LogicalWeapon>();
			int uniqueWeaponID = 0;
			foreach (string weaponFile in files)
			{
				++uniqueWeaponID;
				try
				{
					LogicalWeapon logicalWeaponFromFile = WeaponLibrary.CreateLogicalWeaponFromFile(app, weaponFile, uniqueWeaponID);
					logicalWeaponList.Add(logicalWeaponFromFile);
				}
				catch (Exception ex)
				{
					App.Log.Trace(string.Format("Weapon failed to load: {0} \r\n Exception: {1}", (object)weaponFile, (object)ex.Message), "data");
				}
			}
			return (IEnumerable<LogicalWeapon>)logicalWeaponList;
		}
	}
}
