// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.Turret
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_TURRET)]
	internal class Turret : MountObject
	{
		private LogicalWeapon _weapon;

		public LogicalWeapon Weapon
		{
			get
			{
				return this._weapon;
			}
			set
			{
				if (this._weapon != null)
					throw new InvalidOperationException("Cannot change a turret's weapon once it has been set.");
				this._weapon = value;
			}
		}

		private float CalcRateOfFire(
		  List<SectionEnumerations.DesignAttribute> attributes)
		{
			if (attributes.Count == 0)
				return 1f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Aces_And_Eights))
				return 0.85f;
			return attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Yellow_Streak) ? 1.1f : 1f;
		}

		private float CalcBallisticWeaponRangeModifier(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Sniper))
				value *= 1.25f;
			return value;
		}

		private float CalcAccuracyModifier(
		  List<SectionEnumerations.DesignAttribute> attributes)
		{
			float num = 0.0f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Dead_Eye))
				num = 0.1f;
			return Math.Max(1f - num, 0.0f);
		}

		public Turret(App game, Turret.TurretDescription description)
		{
			List<SectionEnumerations.DesignAttribute> attributes = new List<SectionEnumerations.DesignAttribute>();
			if (description.SInfo != null)
				attributes = game.GameDatabase.GetDesignAttributesForDesign(description.SInfo.DesignID).ToList<SectionEnumerations.DesignAttribute>();
			List<object> objectList = new List<object>();
			objectList.Add((object)description.Weapon.GameObject.ObjectID);
			objectList.Add((object)description.Ship.ObjectID);
			objectList.Add((object)description.Section.ObjectID);
			objectList.Add((object)(description.Module != null ? description.Module.ObjectID : 0));
			objectList.Add((object)description.WeaponBank.ObjectID);
			objectList.Add((object)description.WeaponModels.WeaponModelPath.ModelPath);
			objectList.Add((object)description.WeaponModels.WeaponModelPath.DefaultModelPath);
			objectList.Add((object)description.WeaponModels.SubWeaponModelPath.ModelPath);
			objectList.Add((object)description.WeaponModels.SubWeaponModelPath.DefaultModelPath);
			objectList.Add((object)description.WeaponModels.SecondaryWeaponModelPath.ModelPath);
			objectList.Add((object)description.WeaponModels.SecondaryWeaponModelPath.DefaultModelPath);
			objectList.Add((object)description.WeaponModels.SecondarySubWeaponModelPath.ModelPath);
			objectList.Add((object)description.WeaponModels.SecondarySubWeaponModelPath.DefaultModelPath);
			objectList.Add((object)ScriptHost.AllowConsole);
			objectList.Add((object)(description.TurretBase != null ? description.TurretBase.ObjectID : 0));
			objectList.Add((object)description.CollisionShapeRadius);
			objectList.Add((object)(description.TurretCollisionShape != null ? description.TurretCollisionShape.ObjectID : 0));
			objectList.Add((object)description.TurretIndex);
			objectList.Add((object)(float)((double)description.MaxTurretHealth + (double)description.Weapon.Health));
			objectList.Add((object)description.TurretHealth);
			objectList.Add((object)(float)((double)description.Housing.TrackSpeed + (double)description.Weapon.TrackSpeedModifier));
			objectList.Add((object)this.CalcRateOfFire(attributes));
			objectList.Add((object)description.Weapon.CritHitBonus);
			objectList.Add((object)this.CalcBallisticWeaponRangeModifier(attributes, 1f));
			float num1 = 1f;
			float num2 = (description.Ship.RealShipClass == RealShipClasses.Drone ? 1.5f : 1f) * description.TechModifiers.ROFModifier;
			if (description.Ship.CombatAI == SectionEnumerations.CombatAiType.SwarmerQueen)
				num2 *= 1.25f;
			if (description.Fleet != null)
			{
				if (game.GameDatabase.GetAdmiralTraits(description.Fleet.AdmiralID).ToList<AdmiralInfo.TraitType>().Contains(AdmiralInfo.TraitType.DrillSergeant))
					num1 -= 0.1f;
				if (description.Fleet.Type == FleetType.FL_NORMAL && (double)description.Fleet.SupplyRemaining == 0.0 && (((IEnumerable<WeaponEnums.WeaponTraits>)description.Weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic) || description.Weapon.PayloadType == WeaponEnums.PayloadTypes.Missile))
					num2 *= 0.2f;
			}
			objectList.Add((object)num2);
			objectList.Add((object)(float)((double)description.Ship.AccuracyModifier * (double)this.CalcAccuracyModifier(attributes) * (double)num1));
			objectList.Add((object)description.Ship.PDAccuracyModifier);
			objectList.Add((object)description.Weapon.MalfunctionPercent);
			objectList.Add((object)description.Weapon.MalfunctionDamage);
			objectList.Add((object)description.TechModifiers.DamageModifier);
			objectList.Add((object)description.TechModifiers.SpeedModifier);
			objectList.Add((object)description.TechModifiers.AccelModifier);
			objectList.Add((object)description.TechModifiers.MassModifier);
			objectList.Add((object)description.TechModifiers.RangeModifier);
			objectList.Add((object)description.TechModifiers.SmartNanites);
			objectList.Add((object)description.TurretModelName);
			objectList.Add((object)description.BarrelModelName);
			objectList.Add((object)description.DestroyedTurretEffect.Name);
			objectList.Add((object)description.Mount.NodeName);
			objectList.Add((object)description.Mount.FireAnimName);
			objectList.Add((object)description.Mount.ReloadAnimName);
			objectList.Add((object)description.Weapon.SolutionTolerance);
			objectList.Add((object)description.Mount.Yaw.Min);
			objectList.Add((object)description.Mount.Yaw.Max);
			objectList.Add((object)description.Mount.Pitch.Min);
			objectList.Add((object)description.Mount.Pitch.Max);
			objectList.Add((object)description.LogicalBank.TurretSize);
			objectList.Add((object)description.LogicalBank.TurretClass);
			game.AddExistingObject((IGameObject)this, objectList.ToArray());
			this._weapon = description.Weapon;
			this.ParentID = description.ParentObject.ObjectID;
			this.NodeName = description.Mount.NodeName;
			this.SetTag((object)description.Mount);
		}

		public override void Dispose()
		{
			base.Dispose();
			this._weapon = (LogicalWeapon)null;
		}

		public enum FiringEnum
		{
			NotFiring,
			Firing,
			Failed,
			Completed,
		}

		public class TurretDescription
		{
			public LogicalWeapon Weapon;
			public LogicalMount Mount;
			public ShipInfo SInfo;
			public FleetInfo Fleet;
			public Ship Ship;
			public Section Section;
			public Module Module;
			public IGameObject ParentObject;
			public LogicalBank LogicalBank;
			public WeaponBank WeaponBank;
			public TurretBase TurretBase;
			public GenericCollisionShape TurretCollisionShape;
			public LogicalEffect DestroyedTurretEffect;
			public LogicalTurretHousing Housing;
			public WeaponTechModifiers TechModifiers;
			public string TurretModelName;
			public string BarrelModelName;
			public float CollisionShapeRadius;
			public MountObject.WeaponModels WeaponModels;
			public float MaxTurretHealth;
			public float TurretHealth;
			public int TurretIndex;
		}
	}
}
