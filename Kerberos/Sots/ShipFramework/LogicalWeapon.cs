// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalWeapon
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalWeapon : IDisposable
	{
		private const string DEFAULT_WEAPON_PATH = "props\\Weapons\\";
		public int UniqueWeaponID;
		public string FileName;
		public string WeaponName;
		public WeaponEnums.WeaponSizes DefaultWeaponSize;
		public WeaponEnums.PayloadTypes PayloadType;
		public WeaponEnums.PlagueType PlagueType;
		public LogicalTurretClass[] TurretClasses;
		public WeaponRangeTable RangeTable;
		public float PopDamage;
		public float InfraDamage;
		public float TerraDamage;
		public int Cost;
		public float Range;
		public float Speed;
		public float Mass;
		public float Acceleration;
		public float RechargeTime;
		public float Duration;
		public float TimeToLive;
		public float BuildupTime;
		public float ShotDelay;
		public float VolleyPeriod;
		public float VolleyDeviation;
		public float Health;
		public float TrackSpeedModifier;
		public int NumVolleys;
		public int CritHitRolls;
		public int NumArcs;
		public int ArmorPiercingLevel;
		public int DisruptorValue;
		public int DrainValue;
		public float SolutionTolerance;
		public float DumbFireTime;
		public float MalfunctionDamage;
		public float MalfunctionPercent;
		public float CritHitBonus;
		public float Signature;
		public float RicochetModifier;
		public float ExplosiveMinEffectRange;
		public float ExplosiveMaxEffectRange;
		public float DetonationRange;
		public float MaxGravityForce;
		public float GravityAffectRange;
		public float ArcRange;
		public float EMPRange;
		public float EMPDuration;
		public float DOTDamage;
		public float BeamDamagePeriod;
		public LogicalEffect MuzzleEffect;
		public LogicalEffect BuildupEffect;
		public LogicalEffect BulletEffect;
		public LogicalEffect ImpactEffect;
		public LogicalEffect PlanetImpactEffect;
		public LogicalEffect RicochetEffect;
		public bool isMuzzleEffectLooping;
		public bool isBuildupEffectLooping;
		public bool isBulletEffectLooping;
		public bool isImpactEffectLooping;
		public bool isRicochetEffectLooping;
		public string MuzzleSound;
		public string BuildupSound;
		public string ImpactSound;
		public string PlanetImpactSound;
		public string ExpireSound;
		public string BulletSound;
		public string RicochetSound;
		public int Crew;
		public bool isCrewPerBank;
		public int Power;
		public bool isPowerPerBank;
		public int Supply;
		public bool isSupplyPerBank;
		public bool IsVisible;
		public string Model;
		public string IconSpriteName;
		public string IconTextureName;
		public string SubWeapon;
		public string SecondaryWeapon;
		public int NumSubWeapons;
		public WeaponEnums.SubmunitionBlastType SubMunitionBlastType;
		public float SubmunitionConeDeviation;
		public string Animation;
		public float AnimationDelay;
		public WeaponEnums.WeaponTraits[] Traits;
		public string[] CompatibleSections;
		public string[] DeployableSections;
		public string[] CompatibleFactions;
		public LogicalShield.ShieldType[] PassThroughShields;
		public Tech[] RequiredTechs;
		public Kerberos.Sots.ShipFramework.DamagePattern[] DamagePattern;
		public MuzzleDescriptor MuzzleSize;
		public string DamageDecalMaterial;
		public float DamageDecalSize;
		private Weapon _weapon;
		private int _weaponRefCount;
		private readonly App _game;

		public string Name
		{
			get
			{
				return App.Localize("@" + this.WeaponName);
			}
		}

		public WeaponEnums.TurretClasses DefaultWeaponClass
		{
			get
			{
				return ((IEnumerable<LogicalTurretClass>)this.TurretClasses).First<LogicalTurretClass>().TurretClass;
			}
		}

		public Weapon GameObject
		{
			get
			{
				return this._weapon;
			}
		}

		public float GetRateOfFire()
		{
			if ((double)this.RechargeTime < 1.40129846432482E-45)
				return float.PositiveInfinity;
			return 1f / this.RechargeTime;
		}

		public bool IsPDWeapon()
		{
			return this.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight;
		}

		public LogicalWeapon(App game)
		{
			this._game = game;
		}

		public LogicalTurretClass GetLogicalTurretClassForMount(
		  WeaponEnums.WeaponSizes mountTurretSize,
		  WeaponEnums.TurretClasses mountTurretClass)
		{
			return LogicalTurretClass.GetLogicalTurretClassForMount((IEnumerable<LogicalTurretClass>)this.TurretClasses, this.DefaultWeaponSize, this.DefaultWeaponClass, mountTurretSize, mountTurretClass);
		}

		public static IEnumerable<LogicalWeapon> EnumerateWeaponFits(
		  string faction,
		  string sectionName,
		  IEnumerable<LogicalWeapon> weapons,
		  WeaponEnums.WeaponSizes mountTurretSize,
		  WeaponEnums.TurretClasses mountTurretClass)
		{
			foreach (LogicalWeapon weapon in weapons)
			{
				if (weapon.IsVisible && weapon.IsSectionCompatable(faction, sectionName) && LogicalTurretClass.GetLogicalTurretClassForMount((IEnumerable<LogicalTurretClass>)weapon.TurretClasses, weapon.DefaultWeaponSize, weapon.DefaultWeaponClass, mountTurretSize, mountTurretClass) != null)
					yield return weapon;
			}
		}

		public LogicalWeapon GetSecondaryWeapon(IEnumerable<LogicalWeapon> weapons)
		{
			return weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == this.SecondaryWeapon));
		}

		public LogicalWeapon GetSubWeapon(IEnumerable<LogicalWeapon> weapons)
		{
			return weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == this.SubWeapon));
		}

		public static WeaponModelPaths GetWeaponModelPaths(
		  LogicalWeapon weapon,
		  Faction faction)
		{
			WeaponModelPaths weaponModelPaths = new WeaponModelPaths();
			weaponModelPaths.ModelPath = "";
			weaponModelPaths.DefaultModelPath = "";
			if (weapon != null && !string.IsNullOrEmpty(weapon.Model))
			{
				weaponModelPaths.ModelPath = faction.GetWeaponModelPath(weapon.Model);
				weaponModelPaths.DefaultModelPath = "props\\Weapons\\" + weapon.Model;
			}
			return weaponModelPaths;
		}

		public bool IsSectionCompatable(string faction, string sectionName)
		{
			if (this.CompatibleFactions.Length > 0 && !((IEnumerable<string>)this.CompatibleFactions).Contains<string>(faction))
				return false;
			if (this.CompatibleSections.Length == 0 || sectionName == "")
				return true;
			return ((IEnumerable<string>)this.CompatibleSections).Contains<string>(sectionName);
		}

		public void AddGameObjectReference()
		{
			++this._weaponRefCount;
			if (this._weaponRefCount != 1)
				return;
			List<object> objectList = new List<object>();
			LogicalWeapon logicalWeapon1 = this._game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == this.SubWeapon));
			logicalWeapon1?.AddGameObjectReference();
			LogicalWeapon logicalWeapon2 = this._game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == this.SecondaryWeapon));
			logicalWeapon2?.AddGameObjectReference();
			objectList.Add((object)this.UniqueWeaponID);
			objectList.Add((object)(logicalWeapon1 != null ? logicalWeapon1.GameObject.ObjectID : 0));
			objectList.Add((object)(logicalWeapon2 != null ? logicalWeapon2.GameObject.ObjectID : 0));
			objectList.Add(string.IsNullOrEmpty(this.MuzzleEffect.Name) ? (object)"effects\\placeholder.effect" : (object)this.MuzzleEffect.Name);
			objectList.Add((object)this.isMuzzleEffectLooping);
			objectList.Add(string.IsNullOrEmpty(this.BuildupEffect.Name) ? (object)string.Empty : (object)this.BuildupEffect.Name);
			objectList.Add((object)this.isBuildupEffectLooping);
			objectList.Add(string.IsNullOrEmpty(this.BulletEffect.Name) ? (object)"effects\\placeholder.effect" : (object)this.BulletEffect.Name);
			objectList.Add((object)this.isBulletEffectLooping);
			objectList.Add(string.IsNullOrEmpty(this.ImpactEffect.Name) ? (object)"effects\\placeholder.effect" : (object)this.ImpactEffect.Name);
			objectList.Add((object)this.isImpactEffectLooping);
			objectList.Add(string.IsNullOrEmpty(this.PlanetImpactEffect.Name) ? (object)string.Empty : (object)this.ImpactEffect.Name);
			objectList.Add(string.IsNullOrEmpty(this.RicochetEffect.Name) ? (object)"effects\\Weapons\\Ricochet_Impact.effect" : (object)this.RicochetEffect.Name);
			objectList.Add((object)this.isRicochetEffectLooping);
			objectList.Add((object)this.MuzzleSound);
			objectList.Add((object)this.BuildupSound);
			objectList.Add((object)this.ImpactSound);
			objectList.Add((object)this.PlanetImpactSound);
			objectList.Add((object)this.ExpireSound);
			objectList.Add((object)this.BulletSound);
			objectList.Add((object)this.RicochetSound);
			objectList.Add((object)this.WeaponName);
			objectList.Add((object)this.IconSpriteName);
			objectList.Add((object)this.Name);
			objectList.Add((object)(int)this.PayloadType);
			objectList.Add((object)this.DefaultWeaponSize);
			objectList.Add((object)this.PlagueType);
			objectList.Add((object)this.NumVolleys);
			objectList.Add((object)this.NumSubWeapons);
			objectList.Add((object)(int)this.SubMunitionBlastType);
			objectList.Add((object)this.SubmunitionConeDeviation);
			objectList.Add((object)this.CritHitRolls);
			objectList.Add((object)this.NumArcs);
			objectList.AddRange(this.RangeTable.EnumerateScriptMessageParams());
			objectList.Add((object)this.PopDamage);
			objectList.Add((object)this.InfraDamage);
			objectList.Add((object)this.TerraDamage);
			objectList.Add((object)this.VolleyDeviation);
			objectList.Add((object)this.Speed);
			objectList.Add((object)this.Mass);
			objectList.Add((object)this.Acceleration);
			objectList.Add((object)this.Duration);
			objectList.Add((object)this.TimeToLive);
			objectList.Add((object)this.Health);
			objectList.Add((object)this.Range);
			objectList.Add((object)this.BuildupTime);
			objectList.Add((object)this.ShotDelay);
			objectList.Add((object)this.VolleyPeriod);
			objectList.Add((object)this.RechargeTime);
			objectList.Add((object)this.DumbFireTime);
			objectList.Add((object)this.CritHitBonus);
			objectList.Add((object)this.Signature);
			objectList.Add((object)this.RicochetModifier);
			objectList.Add((object)this.BeamDamagePeriod);
			objectList.Add((object)this.ExplosiveMinEffectRange);
			objectList.Add((object)this.ExplosiveMaxEffectRange);
			objectList.Add((object)this.DetonationRange);
			objectList.Add((object)this.MaxGravityForce);
			objectList.Add((object)this.GravityAffectRange);
			objectList.Add((object)this.ArcRange);
			objectList.Add((object)this.EMPRange);
			objectList.Add((object)this.EMPDuration);
			objectList.Add((object)this.DOTDamage);
			objectList.Add((object)this.Animation);
			objectList.Add((object)this.AnimationDelay);
			objectList.Add((object)(this.MuzzleSize.MuzzleType ?? WeaponEnums.MuzzleShape.Rectangle.ToString()));
			objectList.Add((object)this.MuzzleSize.Height);
			objectList.Add((object)this.MuzzleSize.Width);
			objectList.Add((object)this.Crew);
			objectList.Add((object)this.Power);
			objectList.Add((object)this.Supply);
			objectList.Add((object)this.isCrewPerBank);
			objectList.Add((object)this.isPowerPerBank);
			objectList.Add((object)this.isSupplyPerBank);
			objectList.Add((object)this.Traits.Length);
			foreach (WeaponEnums.WeaponTraits trait in this.Traits)
				objectList.Add((object)(int)trait);
			objectList.Add((object)this.ArmorPiercingLevel);
			objectList.Add((object)this.DisruptorValue);
			objectList.Add((object)this.DrainValue);
			foreach (Kerberos.Sots.ShipFramework.DamagePattern damagePattern in this.DamagePattern)
				objectList.Add((object)damagePattern);
			objectList.Add((object)this.DamageDecalMaterial);
			objectList.Add((object)this.DamageDecalSize);
			objectList.Add((object)this.PassThroughShields.Length);
			foreach (LogicalShield.ShieldType passThroughShield in this.PassThroughShields)
				objectList.Add((object)passThroughShield);
			this._weapon = this._game.AddObject<Weapon>(objectList.ToArray());
		}

		public void ReleaseGameObjectReference()
		{
			if (this._weaponRefCount == 0)
				throw new InvalidOperationException("Weapon reference count already 0.");
			--this._weaponRefCount;
			if (this._weaponRefCount != 0)
				return;
			this._game.ReleaseObject((IGameObject)this._weapon);
			this._weapon = (Weapon)null;
		}

		public void Dispose()
		{
			if (this._weapon == null)
				return;
			this._game.ReleaseObject((IGameObject)this._weapon);
			this._weapon = (Weapon)null;
		}

		public override string ToString()
		{
			return this.Name + "," + (object)this.DefaultWeaponSize;
		}
	}
}
