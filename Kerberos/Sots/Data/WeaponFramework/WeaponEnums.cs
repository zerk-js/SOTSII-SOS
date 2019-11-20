// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.WeaponEnums
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public class WeaponEnums
	{
		public static readonly string[] _weaponSizes = Enum.GetNames(typeof(WeaponEnums.WeaponSizes));
		private static readonly WeaponEnums.TurretClasses[] _battleRiderTypes = new WeaponEnums.TurretClasses[8]
		{
	  WeaponEnums.TurretClasses.Biomissile,
	  WeaponEnums.TurretClasses.Drone,
	  WeaponEnums.TurretClasses.AssaultShuttle,
	  WeaponEnums.TurretClasses.DestroyerRider,
	  WeaponEnums.TurretClasses.CruiserRider,
	  WeaponEnums.TurretClasses.DreadnoughtRider,
	  WeaponEnums.TurretClasses.BoardingPod,
	  WeaponEnums.TurretClasses.EscapePod
		};
		public static readonly string[] _weaponTraits = Enum.GetNames(typeof(WeaponEnums.WeaponTraits));
		public static readonly string[] _plagueTypes = Enum.GetNames(typeof(WeaponEnums.PlagueType));
		public static readonly string[] _factions = new string[7]
		{
	  "human",
	  "hiver",
	  "tarkas",
	  "zuul",
	  "morrigi",
	  "liir_zuul",
	  "loa"
		};
		public static readonly string[] _payloadTypes = Enum.GetNames(typeof(WeaponEnums.PayloadTypes));
		public static readonly string[] _weaponIcons = new string[154]
		{
	  "bal_APdrv_heavy",
	  "bal_APdrv_heavy_Spinal",
	  "bal_APdrv_mass",
	  "bal_APgauss",
	  "bal_burster",
	  "bal_disruptorwhip",
	  "bal_drv_heavy",
	  "bal_drv_heavy_spinal",
	  "bal_drv_leech",
	  "bal_drv_mass",
	  "bal_drv_shield",
	  "bal_flechette",
	  "bal_gauss",
	  "bal_gauss_pd",
	  "bal_grapple",
	  "bal_heap_small",
	  "bal_heap_med",
	  "bal_heap_large",
	  "bal_hvystormer",
	  "bal_railcannon",
	  "bal_shotgun",
	  "bal_shredder",
	  "bal_siege",
	  "bal_Sniper",
	  "bal_stormer",
	  "bal_thumper",
	  "bal_tumbler",
	  "bem_green",
	  "bem_uv",
	  "bem_grav",
	  "bem_xray",
	  "bem_meson",
	  "bem_meson_spinal",
	  "bem_neut",
	  "bem_neut_spinal",
	  "bem_part",
	  "bem_part_spinal",
	  "bem_pos",
	  "bem_pos_spinal",
	  "bem_prjmeson",
	  "bem_Pulsedgrav",
	  "bem_shield",
	  "bem_tractor",
	  "bio_assim",
	  "bio_beast",
	  "bio_nanvir",
	  "bio_plague",
	  "bio_retplague",
	  "brd_boardingpod",
	  "brd_drone",
	  "brd_prisonershuttle",
	  "brd_shuttle",
	  "brd_shuttleadv",
	  "brd_tarkahunter",
	  "can_am",
	  "can_fus",
	  "can_grav",
	  "can_HvyAm",
	  "can_HvyFus",
	  "can_hvyinertial",
	  "can_HvyPlasma",
	  "can_inertial",
	  "can_plasma",
	  "can_prjam",
	  "can_prjfus",
	  "can_prjplasma",
	  "col_cracker_am",
	  "col_cracker_fus",
	  "col_cracker_grav",
	  "col_cracker_implo",
	  "col_cracker_leap",
	  "col_cracker_nuke",
	  "col_crybaby",
	  "col_flock",
	  "col_tarpit",
	  "Dsc_Chakkar",
	  "Dsc_Chakram",
	  "Dsc_WarQ",
	  "emt_heavy",
	  "emt_light",
	  "emt_medium",
	  "hvy_bem_cutting",
	  "hvy_bem_cutting_free",
	  "hvy_bem_hclas",
	  "hvy_bem_hclas_free",
	  "hvy_bem_lancer",
	  "hvy_bem_lancer_free",
	  "las_green",
	  "las_pd",
	  "las_red",
	  "las_uv",
	  "las_xray",
	  "min_am",
	  "min_Clk",
	  "min_fus",
	  "min_grav",
	  "min_implo",
	  "min_leap",
	  "min_nuke",
	  "mis",
	  "mis_blaststorm",
	  "mis_cor",
	  "mis_defplat",
	  "mis_dfire",
	  "mis_heavy_iobm",
	  "mis_interceptor_pd",
	  "mis_kinetic",
	  "mis_mirv",
	  "mis_mirv_iobm",
	  "mis_nancor",
	  "mis_node",
	  "mis_planet",
	  "mis_planet_hvy",
	  "mis_planet_mirv",
	  "mis_polaris",
	  "mis_polarisblastbeam",
	  "mis_thud",
	  "NodeCannon",
	  "phs",
	  "phs_pd",
	  "phs_pulsed",
	  "spyship",
	  "trp_am",
	  "trp_amdet",
	  "trp_disruptor",
	  "trp_empulsar",
	  "trp_fus",
	  "trp_fusdet",
	  "trp_gluonic",
	  "trp_gravpulsar",
	  "trp_kelvinic",
	  "trp_mesonic",
	  "trp_photon",
	  "trp_plasma",
	  "trp_pulsar",
	  "wraith",
	  "_mis_mirv_warhead",
	  "_mis_planet_mirv_warhead",
	  "enveloping_fusion_torpedos",
	  "ionic_torpedos",
	  "bal_protean_mass_driver",
	  "bem_disintegration_vonneumann",
	  "bem_disintegration_peacekeeper",
	  "bem_locus",
	  "bem_suulka_cannon",
	  "bem_systemkiller",
	  "bem_variable_phaser",
	  "bem_variable_laser",
	  "bem_damper",
	  "trp_antimatterenveloping",
	  "trp_fusionenveloping",
	  "col_emitter",
	  "col_cloak",
	  "bal_absorber_harpoon"
		};
		public static readonly string[] _shieldTypes = Enum.GetNames(typeof(LogicalShield.ShieldType));

		public static bool RequiresDesign(
		  WeaponEnums.TurretClasses turretClass,
		  out RealShipClasses correspondingShipClass)
		{
			switch (turretClass)
			{
				case WeaponEnums.TurretClasses.Biomissile:
					correspondingShipClass = RealShipClasses.Biomissile;
					return true;
				case WeaponEnums.TurretClasses.Drone:
					correspondingShipClass = RealShipClasses.Drone;
					return true;
				case WeaponEnums.TurretClasses.AssaultShuttle:
					correspondingShipClass = RealShipClasses.AssaultShuttle;
					return true;
				case WeaponEnums.TurretClasses.BoardingPod:
					correspondingShipClass = RealShipClasses.BoardingPod;
					return true;
				default:
					correspondingShipClass = RealShipClasses.AssaultShuttle;
					return false;
			}
		}

		public static bool DesignIsSelectable(WeaponEnums.TurretClasses turretClass)
		{
			switch (turretClass)
			{
				case WeaponEnums.TurretClasses.Drone:
				case WeaponEnums.TurretClasses.AssaultShuttle:
					return true;
				default:
					return false;
			}
		}

		public static bool IsBattleRider(WeaponEnums.TurretClasses shipType)
		{
			return ((IEnumerable<WeaponEnums.TurretClasses>)WeaponEnums._battleRiderTypes).Contains<WeaponEnums.TurretClasses>(shipType);
		}

		public static bool IsPlanetAssaultWeapon(WeaponEnums.TurretClasses tc)
		{
			switch (tc)
			{
				case WeaponEnums.TurretClasses.Biomissile:
				case WeaponEnums.TurretClasses.AssaultShuttle:
				case WeaponEnums.TurretClasses.Siege:
					return true;
				default:
					return false;
			}
		}

		public static bool IsWeaponBattleRider(WeaponEnums.TurretClasses shipType)
		{
			switch (shipType)
			{
				case WeaponEnums.TurretClasses.Biomissile:
				case WeaponEnums.TurretClasses.Drone:
				case WeaponEnums.TurretClasses.AssaultShuttle:
				case WeaponEnums.TurretClasses.BoardingPod:
					return true;
				default:
					return false;
			}
		}

		public enum MuzzleShape
		{
			Rectangle,
			Oval,
		}

		public enum WeaponSizes
		{
			VeryLight,
			Light,
			Medium,
			Heavy,
			VeryHeavy,
			SuperHeavy,
		}

		public enum SubmunitionBlastType
		{
			Focus,
			Burst,
		}

		public enum TurretClasses
		{
			Standard,
			Missile,
			IOBM,
			PolarisMissile,
			FreeBeam,
			HeavyBeam,
			Torpedo,
			Spinal,
			Strafe,
			Impactor,
			COL,
			Projector,
			Minelayer,
			Biomissile,
			Drone,
			AssaultShuttle,
			DestroyerRider,
			CruiserRider,
			DreadnoughtRider,
			NodeCannon,
			NodeMissile,
			SuulkaTentCannon,
			BoardingPod,
			EscapePod,
			Siege,
		}

		public enum WeaponTraits
		{
			Energy,
			Ballistic,
			Explosive,
			Corrosive,
			Nanite,
			Gas,
			Tracking,
			Cloaking,
			Grav,
			Draining,
			Mesonic,
			Laser,
			StandOff,
			Brawler,
			PointDefence,
			Bombardment,
			Imploding,
			Disabling,
			Detonating,
			Exclusive,
			Upgradable,
			Disintegrating,
			PlanetKilling,
			ShieldBreaker,
			Ionic,
			LockOn,
			StructOnly,
			Sticky,
			Kelvinik,
			Inertial,
			Phantom,
		}

		public enum PlagueType
		{
			NONE,
			BASIC,
			RETRO,
			BEAST,
			ASSIM,
			NANO,
			XOMBIE,
			ZUUL,
		}

		public enum PayloadTypes
		{
			Bolt,
			SiegeDriver,
			Beam,
			MegaBeam,
			Missile,
			Torpedo,
			BattleRider,
			Mine,
			COL,
			TarPit,
			GravCannon,
			CryBaby,
			Emitter,
			GrappleHook,
			Shield,
			DOTCloud,
			NodeCannon,
			NodeMissile,
			JaggerMissile,
		}
	}
}
