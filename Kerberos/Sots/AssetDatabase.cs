// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.AssetDatabase
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots
{
	internal class AssetDatabase : IDisposable
	{
		public static Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>> MoralModifierMap = new Dictionary<GovernmentInfo.GovernmentType, Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>>()
	{
	  {
		GovernmentInfo.GovernmentType.Centrism,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_GHOSTSHIP_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 10
			  }
			}
		  },
		  {
			MoralEvent.ME_1MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_3MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_ENEMY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_RANDOM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_GM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FORGE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_GEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -10
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LEVIATHAN,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FLAGSHIP,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LVL5_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_MEDIA_HERO_WIN,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_MEDIA_HERO_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -6
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_TAX_INCREASED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_TAX_DECREASED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_IN_SYSTEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_WAR_DECLARED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_BETRAYAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_BETRAYED_LARGER,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_BETRAYED_SMALLER,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_KICKED_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_200MILLION_CIV_DEATHS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_PLANET_PSI_DRAINED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_PLANET_HEALTH_DRAINED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -9
			  }
			}
		  },
		  {
			MoralEvent.ME_PLAGUE_OUTBREAK,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_BELOW_15,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_0,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_10MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_25MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_WORLD_COLONIZED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_PROVINCE_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_LVL5_STATION_BUILT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 4
			  }
			}
		  },
		  {
			MoralEvent.ME_FLAGSHIP_BUILT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_LEVIATHAN_BUILT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_LEVIATHAN_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_FLEET_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_PERFECT_RANDOM_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_GM_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_FRIENDLY_SUULKA_SUMMONED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_SUULKA_SUMMONED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_PIRATES_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_INCORPORATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_CONQUERED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_PROVINCE_CAPTURED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_EMPIRE_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_EMPIRE_SURRENDER,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 10
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_SYSTEM_CONQUERED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_CIVILIAN_STATION_BUILT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_PEACE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_TRADE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_GEM_WORLD_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_FORGE_WORLD_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_ADMIRAL_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_ABOVE_85,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_100,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ASTEROID_STRIKE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_OVERPOPULATION_PLAYER,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_OVERPOPULATION_PLANET,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_EVAC_OVERPOPULATION_PLANET,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_REPLICANTS_ON,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_SUPER_NOVA_RADIATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_ABANDON_COLONY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -10
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_SYSTEM_CLOSE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -15
			  }
			}
		  },
		  {
			MoralEvent.ME_SYSTEM_OPEN,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 10
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Junta,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_LOSE_WORLD_ENEMY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FORGE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -12
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LEVIATHAN,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -6
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FLAGSHIP,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LVL5_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -6
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_IN_SYSTEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_WAR_DECLARED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_BETRAYAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_KICKED_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_INCORPORATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_PEACE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_TRADE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_GEM_WORLD_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_FORGE_WORLD_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_ADMIRAL_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Plutocracy,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_1MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_3MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_RANDOM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_GM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FORGE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_BELOW_15,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_0,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_10MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_25MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_PERFECT_RANDOM_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_GM_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  }
			}
		  },
		  {
			MoralEvent.ME_PIRATES_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_TRADE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_FORGE_WORLD_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_ABOVE_85,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_100,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 3
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Mercantilism,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_1MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_3MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_ENEMY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_RANDOM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_GM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FORGE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_GEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -6
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -15
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LEVIATHAN,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FLAGSHIP,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LVL5_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_TAX_INCREASED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_IN_SYSTEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_WAR_DECLARED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_BETRAYAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_KICKED_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_200MILLION_CIV_DEATHS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_PLANET_PSI_DRAINED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_PLANET_HEALTH_DRAINED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -15
			  }
			}
		  },
		  {
			MoralEvent.ME_PLAGUE_OUTBREAK,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_BELOW_15,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_0,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_10MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_25MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 4
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_TRADE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 4
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_ABOVE_85,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_100,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 4
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Liberationism,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_1MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_3MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -6
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_ENEMY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FORGE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_GEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -6
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -13
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LEVIATHAN,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FLAGSHIP,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LVL5_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_TAX_INCREASED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_IN_SYSTEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_WAR_DECLARED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_BETRAYAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_KICKED_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_200MILLION_CIV_DEATHS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_BELOW_15,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_0,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -6
			  }
			}
		  },
		  {
			MoralEvent.ME_10MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_25MILLION_SAVINGS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_WORLD_COLONIZED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_PROVINCE_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  }
			}
		  },
		  {
			MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -10
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 10
			  }
			}
		  },
		  {
			MoralEvent.ME_PIRATES_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_INCORPORATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_CONQUERED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_PEACE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 4
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_TRADE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 4
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_ABOVE_85,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_ECONOMY_100,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = 6
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Anarchism,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_WAR_DECLARED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_BETRAYAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  }
			}
		  },
		  {
			MoralEvent.ME_KICKED_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_200MILLION_CIV_DEATHS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_PLANET_PSI_DRAINED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_WORLD_COLONIZED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_PROVINCE_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  }
			}
		  },
		  {
			MoralEvent.ME_PERFECT_RANDOM_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_GM_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  }
			}
		  },
		  {
			MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -10
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 10
			  }
			}
		  },
		  {
			MoralEvent.ME_PIRATES_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_INCORPORATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_CONQUERED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_PEACE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 6
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_TRADE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_GEM_WORLD_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 4
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Cooperativism,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_1MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_3MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_ENEMY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_RANDOM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_TAX_INCREASED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_WAR_DECLARED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_WITHDRAW_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_KICKED_FROM_ALLIANCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_200MILLION_CIV_DEATHS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_WORLD_COLONIZED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_PROVINCE_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 4
			  }
			}
		  },
		  {
			MoralEvent.ME_FRIENDLY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_SUULKA_DEFEATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 7
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_INCORPORATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_CONQUERED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_PEACE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  },
		  {
			MoralEvent.ME_GEM_WORLD_FORMED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 5
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 3
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Socialism,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_LOSE_WORLD_ENEMY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_RANDOM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_GM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FORGE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -4
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_GEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -7
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -10
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -7
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -15
			  }
			}
		  },
		  {
			MoralEvent.ME_200MILLION_CIV_DEATHS,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_PLANET_PSI_DRAINED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -6
			  }
			}
		  },
		  {
			MoralEvent.ME_PLANET_HEALTH_DRAINED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -11
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_INCORPORATED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 2
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_CONQUERED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_INDEPENDANT_DESTROYED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_PEACE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 4
			  }
			}
		  },
		  {
			MoralEvent.ME_FORM_TRADE_TREATY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 1
			  }
			}
		  }
		}
	  },
	  {
		GovernmentInfo.GovernmentType.Communalism,
		new Dictionary<MoralEvent, List<AssetDatabase.MoralModifier>>()
		{
		  {
			MoralEvent.ME_1MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_3MILLION_DEBT,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_WORLD_ENEMY,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FORGE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_GEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -3
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Province,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_PROVINCE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_EMPIRE_CAPITAL,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -5
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LEVIATHAN,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_FLAGSHIP,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_LVL5_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_LOSE_STATION,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 0
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_KILLED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = -1
			  }
			}
		  },
		  {
			MoralEvent.ME_ADMIRAL_CONVERTED,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.AllColonies,
				value = -2
			  },
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.Colony,
				value = -3
			  }
			}
		  },
		  {
			MoralEvent.ME_ENEMY_IN_SYSTEM,
			new List<AssetDatabase.MoralModifier>()
			{
			  new AssetDatabase.MoralModifier()
			  {
				type = AssetDatabase.MoraleModifierType.System,
				value = 0
			  }
			}
		  }
		}
	  }
	};
		public static Dictionary<ModuleEnums.StationModuleType, string> StationModuleTypeAssetMap = new Dictionary<ModuleEnums.StationModuleType, string>()
	{
	  {
		ModuleEnums.StationModuleType.Sensor,
		"factions\\{0}\\modules\\sn_sensor.module"
	  },
	  {
		ModuleEnums.StationModuleType.Customs,
		"factions\\{0}\\modules\\sn_customs.module"
	  },
	  {
		ModuleEnums.StationModuleType.Combat,
		"factions\\{0}\\modules\\sn_combat.module"
	  },
	  {
		ModuleEnums.StationModuleType.Repair,
		"factions\\{0}\\modules\\sn_repair.module"
	  },
	  {
		ModuleEnums.StationModuleType.Warehouse,
		"factions\\{0}\\modules\\sn_warehouse.module"
	  },
	  {
		ModuleEnums.StationModuleType.Command,
		"factions\\{0}\\modules\\sn_command.module"
	  },
	  {
		ModuleEnums.StationModuleType.Dock,
		"factions\\{0}\\modules\\sn_dock.module"
	  },
	  {
		ModuleEnums.StationModuleType.Terraform,
		"factions\\{0}\\modules\\sn_terraform.module"
	  },
	  {
		ModuleEnums.StationModuleType.Bastion,
		"factions\\{0}\\modules\\sn_bastion.module"
	  },
	  {
		ModuleEnums.StationModuleType.Amp,
		"factions\\{0}\\modules\\sn_gate_amplifier.module"
	  },
	  {
		ModuleEnums.StationModuleType.Defence,
		"factions\\{0}\\modules\\sn_defence.module"
	  },
	  {
		ModuleEnums.StationModuleType.AlienHabitation,
		"factions\\{0}\\modules\\sn_habitation_{0}.module"
	  },
	  {
		ModuleEnums.StationModuleType.Habitation,
		"factions\\{0}\\modules\\sn_habitation_{0}.module"
	  },
	  {
		ModuleEnums.StationModuleType.HumanHabitation,
		"factions\\human\\modules\\sn_habitation_human.module"
	  },
	  {
		ModuleEnums.StationModuleType.TarkasHabitation,
		"factions\\tarkas\\modules\\sn_habitation_tarkas.module"
	  },
	  {
		ModuleEnums.StationModuleType.LiirHabitation,
		"factions\\liir_zuul\\modules\\sn_habitation_liir_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.HiverHabitation,
		"factions\\hiver\\modules\\sn_habitation_hiver.module"
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiHabitation,
		"factions\\morrigi\\modules\\sn_habitation_morrigi.module"
	  },
	  {
		ModuleEnums.StationModuleType.ZuulHabitation,
		"factions\\zuul\\modules\\sn_habitation_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.LoaHabitation,
		"factions\\loa\\modules\\sn_habitation_loa.module"
	  },
	  {
		ModuleEnums.StationModuleType.LargeHabitation,
		"factions\\{0}\\modules\\sn_large_habitation_{0}.module"
	  },
	  {
		ModuleEnums.StationModuleType.LargeAlienHabitation,
		"factions\\{0}\\modules\\sn_large_habitation_{0}.module"
	  },
	  {
		ModuleEnums.StationModuleType.HumanLargeHabitation,
		"factions\\human\\modules\\sn_large_habitation_human.module"
	  },
	  {
		ModuleEnums.StationModuleType.TarkasLargeHabitation,
		"factions\\tarkas\\modules\\sn_large_habitation_tarkas.module"
	  },
	  {
		ModuleEnums.StationModuleType.LiirLargeHabitation,
		"factions\\liir_zuul\\modules\\sn_large_habitation_liir_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.HiverLargeHabitation,
		"factions\\hiver\\modules\\sn_large_habitation_hiver.module"
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiLargeHabitation,
		"factions\\morrigi\\modules\\sn_large_habitation_morrigi.module"
	  },
	  {
		ModuleEnums.StationModuleType.ZuulLargeHabitation,
		"factions\\zuul\\modules\\sn_large_habitation_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.LoaLargeHabitation,
		"factions\\loa\\modules\\sn_large_habitation_loa.module"
	  },
	  {
		ModuleEnums.StationModuleType.HumanHabitationForeign,
		"factions\\human\\modules\\sn_habitation_human.module"
	  },
	  {
		ModuleEnums.StationModuleType.TarkasHabitationForeign,
		"factions\\tarkas\\modules\\sn_habitation_tarkas.module"
	  },
	  {
		ModuleEnums.StationModuleType.LiirHabitationForeign,
		"factions\\liir_zuul\\modules\\sn_habitation_liir_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.HiverHabitationForeign,
		"factions\\hiver\\modules\\sn_habitation_hiver.module"
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiHabitationForeign,
		"factions\\morrigi\\modules\\sn_habitation_morrigi.module"
	  },
	  {
		ModuleEnums.StationModuleType.ZuulHabitationForeign,
		"factions\\zuul\\modules\\sn_habitation_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.LoaHabitationForeign,
		"factions\\loa\\modules\\sn_habitation_loa.module"
	  },
	  {
		ModuleEnums.StationModuleType.HumanLargeHabitationForeign,
		"factions\\human\\modules\\sn_large_habitation_human.module"
	  },
	  {
		ModuleEnums.StationModuleType.TarkasLargeHabitationForeign,
		"factions\\tarkas\\modules\\sn_large_habitation_tarkas.module"
	  },
	  {
		ModuleEnums.StationModuleType.LiirLargeHabitationForeign,
		"factions\\liir_zuul\\modules\\sn_large_habitation_liir_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.HiverLargeHabitationForeign,
		"factions\\hiver\\modules\\sn_large_habitation_hiver.module"
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiLargeHabitationForeign,
		"factions\\morrigi\\modules\\sn_large_habitation_morrigi.module"
	  },
	  {
		ModuleEnums.StationModuleType.ZuulLargeHabitationForeign,
		"factions\\zuul\\modules\\sn_large_habitation_zuul.module"
	  },
	  {
		ModuleEnums.StationModuleType.LoaLargeHabitationForeign,
		"factions\\loa\\modules\\sn_large_habitation_loa.module"
	  },
	  {
		ModuleEnums.StationModuleType.GateLab,
		"factions\\{0}\\modules\\sn_gate_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.Lab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.EWPLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.TRPLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.NRGLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.WARLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.BALLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.BIOLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.INDLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.CCCLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.DRVLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.POLLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.PSILab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.ENGLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.BRDLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.SLDLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  },
	  {
		ModuleEnums.StationModuleType.CYBLab,
		"factions\\{0}\\modules\\sn_lab.module"
	  }
	};
		public static Dictionary<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes> StationModuleTypeToMountTypeMap = new Dictionary<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes>()
	{
	  {
		ModuleEnums.StationModuleType.Sensor,
		ModuleEnums.ModuleSlotTypes.Sensor
	  },
	  {
		ModuleEnums.StationModuleType.Customs,
		ModuleEnums.ModuleSlotTypes.Customs
	  },
	  {
		ModuleEnums.StationModuleType.Combat,
		ModuleEnums.ModuleSlotTypes.Combat
	  },
	  {
		ModuleEnums.StationModuleType.Repair,
		ModuleEnums.ModuleSlotTypes.Repair
	  },
	  {
		ModuleEnums.StationModuleType.Warehouse,
		ModuleEnums.ModuleSlotTypes.Warehouse
	  },
	  {
		ModuleEnums.StationModuleType.Command,
		ModuleEnums.ModuleSlotTypes.Command
	  },
	  {
		ModuleEnums.StationModuleType.Dock,
		ModuleEnums.ModuleSlotTypes.Dock
	  },
	  {
		ModuleEnums.StationModuleType.Terraform,
		ModuleEnums.ModuleSlotTypes.Terraform
	  },
	  {
		ModuleEnums.StationModuleType.Bastion,
		ModuleEnums.ModuleSlotTypes.Bastion
	  },
	  {
		ModuleEnums.StationModuleType.Amp,
		ModuleEnums.ModuleSlotTypes.Amp
	  },
	  {
		ModuleEnums.StationModuleType.Defence,
		ModuleEnums.ModuleSlotTypes.Defence
	  },
	  {
		ModuleEnums.StationModuleType.GateLab,
		ModuleEnums.ModuleSlotTypes.GateLab
	  },
	  {
		ModuleEnums.StationModuleType.AlienHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.Habitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.HumanHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.TarkasHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.LiirHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.HiverHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.ZuulHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.LoaHabitation,
		ModuleEnums.ModuleSlotTypes.Habitation
	  },
	  {
		ModuleEnums.StationModuleType.LargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.LargeAlienHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.HumanLargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.TarkasLargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.LiirLargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.HiverLargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiLargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.ZuulLargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.LoaLargeHabitation,
		ModuleEnums.ModuleSlotTypes.LargeHabitation
	  },
	  {
		ModuleEnums.StationModuleType.HumanHabitationForeign,
		ModuleEnums.ModuleSlotTypes.AlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.TarkasHabitationForeign,
		ModuleEnums.ModuleSlotTypes.AlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.LiirHabitationForeign,
		ModuleEnums.ModuleSlotTypes.AlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.HiverHabitationForeign,
		ModuleEnums.ModuleSlotTypes.AlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiHabitationForeign,
		ModuleEnums.ModuleSlotTypes.AlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.ZuulHabitationForeign,
		ModuleEnums.ModuleSlotTypes.AlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.LoaHabitationForeign,
		ModuleEnums.ModuleSlotTypes.AlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.HumanLargeHabitationForeign,
		ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.TarkasLargeHabitationForeign,
		ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.LiirLargeHabitationForeign,
		ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.HiverLargeHabitationForeign,
		ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.MorrigiLargeHabitationForeign,
		ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.ZuulLargeHabitationForeign,
		ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.LoaLargeHabitationForeign,
		ModuleEnums.ModuleSlotTypes.LargeAlienHabitation
	  },
	  {
		ModuleEnums.StationModuleType.Lab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.EWPLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.TRPLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.NRGLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.WARLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.BALLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.BIOLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.INDLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.CCCLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.DRVLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.POLLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.PSILab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.ENGLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.BRDLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.SLDLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  },
	  {
		ModuleEnums.StationModuleType.CYBLab,
		ModuleEnums.ModuleSlotTypes.Lab
	  }
	};
		private Dictionary<string, AssetDatabase.MiniMapData> _miniShipMap = new Dictionary<string, AssetDatabase.MiniMapData>();
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> DiplomaticStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>()
	{
	  {
		1,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.AlienHabitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Customs,
			1
		  }
		}
	  },
	  {
		2,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			2
		  },
		  {
			ModuleEnums.StationModuleType.AlienHabitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  }
		}
	  },
	  {
		3,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			2
		  },
		  {
			ModuleEnums.StationModuleType.AlienHabitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  }
		}
	  },
	  {
		4,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			3
		  },
		  {
			ModuleEnums.StationModuleType.AlienHabitation,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.LargeHabitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.LargeAlienHabitation,
			1
		  }
		}
	  }
	};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> NavalStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>()
	{
	  {
		1,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Warehouse,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Repair,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Command,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  }
		}
	  },
	  {
		2,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Warehouse,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Repair,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Command,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			2
		  }
		}
	  },
	  {
		3,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Warehouse,
			5
		  },
		  {
			ModuleEnums.StationModuleType.Repair,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Command,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			3
		  }
		}
	  },
	  {
		4,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Warehouse,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Repair,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Command,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			5
		  },
		  {
			ModuleEnums.StationModuleType.Combat,
			3
		  }
		}
	  }
	};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> ScienceStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>()
	{
	  {
		1,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Lab,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  }
		}
	  },
	  {
		2,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Lab,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  }
		}
	  },
	  {
		3,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Lab,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Warehouse,
			2
		  }
		}
	  },
	  {
		4,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Lab,
			5
		  },
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Warehouse,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  }
		}
	  }
	};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> CivilianStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>()
	{
	  {
		1,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Warehouse,
			1
		  }
		}
	  },
	  {
		2,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Warehouse,
			1
		  }
		}
	  },
	  {
		3,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Warehouse,
			1
		  },
		  {
			ModuleEnums.StationModuleType.AlienHabitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  }
		}
	  },
	  {
		4,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.LargeHabitation,
			3
		  },
		  {
			ModuleEnums.StationModuleType.LargeAlienHabitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Warehouse,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  }
		}
	  }
	};
		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> GateStationUpgradeRequirements = new Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>()
	{
	  {
		1,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Bastion,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Amp,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  }
		}
	  },
	  {
		2,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Amp,
			3
		  },
		  {
			ModuleEnums.StationModuleType.GateLab,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Bastion,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			2
		  }
		}
	  },
	  {
		3,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  },
		  {
			ModuleEnums.StationModuleType.GateLab,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Bastion,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			1
		  },
		  {
			ModuleEnums.StationModuleType.Combat,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Amp,
			3
		  }
		}
	  },
	  {
		4,
		new Dictionary<ModuleEnums.StationModuleType, int>()
		{
		  {
			ModuleEnums.StationModuleType.Habitation,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Dock,
			1
		  },
		  {
			ModuleEnums.StationModuleType.GateLab,
			3
		  },
		  {
			ModuleEnums.StationModuleType.Bastion,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Sensor,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Combat,
			2
		  },
		  {
			ModuleEnums.StationModuleType.Amp,
			4
		  }
		}
	  }
	};
		private Dictionary<RandomEncounter, int> _randomEncounterOdds = new Dictionary<RandomEncounter, int>();
		private Dictionary<EasterEgg, int> _easterEggOdds = new Dictionary<EasterEgg, int>();
		private Dictionary<EasterEgg, int> _gmOdds = new Dictionary<EasterEgg, int>();
		private GovernmentEffects _governmentEffects = new GovernmentEffects();
		private Dictionary<StratModifiers, object> _defaultStratModifiers = new Dictionary<StratModifiers, object>()
	{
	  {
		StratModifiers.StartProvincePlanets,
		(object) 3
	  },
	  {
		StratModifiers.MinProvincePlanets,
		(object) 3
	  },
	  {
		StratModifiers.MaxProvincePlanets,
		(object) 3
	  },
	  {
		StratModifiers.MaxProvincePlanetRange,
		(object) 8f
	  },
	  {
		StratModifiers.AllowPoliceInCombat,
		(object) false
	  },
	  {
		StratModifiers.PoliceMoralBonus,
		(object) 0
	  },
	  {
		StratModifiers.AllowWorldSurrender,
		(object) false
	  },
	  {
		StratModifiers.AllowProvinceSurrender,
		(object) false
	  },
	  {
		StratModifiers.AllowEmpireSurrender,
		(object) false
	  },
	  {
		StratModifiers.PrototypeConstructionCostModifierPF,
		(object) 3f
	  },
	  {
		StratModifiers.PrototypeSavingsCostModifierPF,
		(object) 3f
	  },
	  {
		StratModifiers.PrototypeConstructionCostModifierCR,
		(object) 2.5f
	  },
	  {
		StratModifiers.ConstructionCostModifierCR,
		(object) 1f
	  },
	  {
		StratModifiers.PrototypeConstructionCostModifierDN,
		(object) 2f
	  },
	  {
		StratModifiers.ConstructionCostModifierDN,
		(object) 1f
	  },
	  {
		StratModifiers.PrototypeConstructionCostModifierLV,
		(object) 1.5f
	  },
	  {
		StratModifiers.ConstructionCostModifierLV,
		(object) 1f
	  },
	  {
		StratModifiers.ConstructionCostModifierSN,
		(object) 1f
	  },
	  {
		StratModifiers.PrototypeSavingsCostModifierCR,
		(object) 4f
	  },
	  {
		StratModifiers.PrototypeSavingsCostModifierDN,
		(object) 3f
	  },
	  {
		StratModifiers.PrototypeSavingsCostModifierLV,
		(object) 2.5f
	  },
	  {
		StratModifiers.PrototypeTimeModifier,
		(object) 1f
	  },
	  {
		StratModifiers.ShowPrototypeDesignAttributes,
		(object) false
	  },
	  {
		StratModifiers.BadDesignAttributePercent,
		(object) 10
	  },
	  {
		StratModifiers.GoodDesignAttributePercent,
		(object) 10
	  },
	  {
		StratModifiers.IndustrialOutputModifier,
		(object) 1f
	  },
	  {
		StratModifiers.PopulationGrowthModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AdditionalMaxCivilianPopulation,
		(object) 0.0f
	  },
	  {
		StratModifiers.AdditionalMaxImperialPopulation,
		(object) 0.0f
	  },
	  {
		StratModifiers.SalvageModifier,
		(object) 0.0f
	  },
	  {
		StratModifiers.OddsOfRandomEncounter,
		(object) 0.1f
	  },
	  {
		StratModifiers.OverharvestModifier,
		(object) 10f
	  },
	  {
		StratModifiers.MinOverharvestRate,
		(object) 0.0f
	  },
	  {
		StratModifiers.OverharvestFromPopulationModifier,
		(object) 20f
	  },
	  {
		StratModifiers.OverPopulationPercentage,
		(object) 0.51f
	  },
	  {
		StratModifiers.TechFeasibilityDeviation,
		(object) 0.5f
	  },
	  {
		StratModifiers.SlaveDeathRateModifier,
		(object) 1f
	  },
	  {
		StratModifiers.NavyStationSensorCloakBonus,
		(object) 1f
	  },
	  {
		StratModifiers.ScienceStationSensorCloakBonus,
		(object) 1f
	  },
	  {
		StratModifiers.AllowPrivateerMission,
		(object) false
	  },
	  {
		StratModifiers.StripMiningMaximum,
		(object) 0.1f
	  },
	  {
		StratModifiers.TerraformingModifier,
		(object) 1f
	  },
	  {
		StratModifiers.BiosphereDestructionModifier,
		(object) 1f
	  },
	  {
		StratModifiers.PlagueDeathModifier,
		(object) 1f
	  },
	  {
		StratModifiers.ColonySupportCostModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AllowHardenedStructures,
		(object) false
	  },
	  {
		StratModifiers.AllowPlanetBeam,
		(object) false
	  },
	  {
		StratModifiers.AllowMirvPlanetaryMissiles,
		(object) false
	  },
	  {
		StratModifiers.AllowDeepSpaceConstruction,
		(object) false
	  },
	  {
		StratModifiers.SlaveProductionModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AllowTradeEnclave,
		(object) false
	  },
	  {
		StratModifiers.AllowProtectorate,
		(object) false
	  },
	  {
		StratModifiers.AllowIncorporate,
		(object) false
	  },
	  {
		StratModifiers.AllowAlienPopulations,
		(object) false
	  },
	  {
		StratModifiers.AlienCivilianTaxRate,
		(object) 1f
	  },
	  {
		StratModifiers.ComparativeAnalysys,
		(object) false
	  },
	  {
		StratModifiers.MassInductionProjectors,
		(object) false
	  },
	  {
		StratModifiers.StandingNeutrinoWaves,
		(object) false
	  },
	  {
		StratModifiers.AllowIdealAlienGrowthRate,
		(object) false
	  },
	  {
		StratModifiers.AllowAlienImmigration,
		(object) false
	  },
	  {
		StratModifiers.DiplomaticOfferingModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AdmiralCareerModifier,
		(object) 1f
	  },
	  {
		StratModifiers.ChanceOfPirates,
		(object) 1f
	  },
	  {
		StratModifiers.AIBenefitBonus,
		(object) 1f
	  },
	  {
		StratModifiers.AIResearchBonus,
		(object) 0.0f
	  },
	  {
		StratModifiers.AIRevenueBonus,
		(object) 0.0f
	  },
	  {
		StratModifiers.AIProductionBonus,
		(object) 0.0f
	  },
	  {
		StratModifiers.ConstructionPointBonus,
		(object) 1f
	  },
	  {
		StratModifiers.AllowAIRebellion,
		(object) false
	  },
	  {
		StratModifiers.ImmuneToSpectre,
		(object) false
	  },
	  {
		StratModifiers.WarpDriveStratSignatureModifier,
		(object) 1f
	  },
	  {
		StratModifiers.UseFastestShipForFTLSpeed,
		(object) false
	  },
	  {
		StratModifiers.AllowSuperWorlds,
		(object) false
	  },
	  {
		StratModifiers.DiplomacyPointCostModifier,
		(object) 1f
	  },
	  {
		StratModifiers.NegativeRelationsModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AllowOneFightRebellionEnding,
		(object) false
	  },
	  {
		StratModifiers.DiplomaticReactionBonus,
		(object) 1f
	  },
	  {
		StratModifiers.MoralBonus,
		(object) 0
	  },
	  {
		StratModifiers.PsiResearchModifier,
		(object) 1f
	  },
	  {
		StratModifiers.ResearchModifier,
		(object) 1f
	  },
	  {
		StratModifiers.GlobalResearchModifier,
		(object) 1f
	  },
	  {
		StratModifiers.ResearchBreakthroughModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AllowFarSense,
		(object) false
	  },
	  {
		StratModifiers.C3ResearchModifier,
		(object) 1f
	  },
	  {
		StratModifiers.LeviathanResearchModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AsteroidMonitorResearchModifier,
		(object) 1f
	  },
	  {
		StratModifiers.AdmiralReactionModifier,
		(object) 1f
	  },
	  {
		StratModifiers.GrandMenaceWarningTime,
		(object) 0
	  },
	  {
		StratModifiers.RandomEncounterWarningPercent,
		(object) 0.0f
	  },
	  {
		StratModifiers.SurveyTimeModifier,
		(object) 1f
	  },
	  {
		StratModifiers.DomeStageModifier,
		(object) 0
	  },
	  {
		StratModifiers.CavernDmodModifier,
		(object) 0.0f
	  },
	  {
		StratModifiers.IntelSuccessModifier,
		(object) 1f
	  },
	  {
		StratModifiers.EnemyIntelSuccessModifier,
		(object) 1f
	  },
	  {
		StratModifiers.EnemyOperationsSuccessModifier,
		(object) 1f
	  },
	  {
		StratModifiers.CounterIntelSuccessModifier,
		(object) 1f
	  },
	  {
		StratModifiers.ShipSupplyModifier,
		(object) 1f
	  },
	  {
		StratModifiers.WarehouseCapacityModifier,
		(object) 1f
	  },
	  {
		StratModifiers.ScrapShipModifier,
		(object) 1f
	  },
	  {
		StratModifiers.MaxColonizableHazard,
		(object) 650
	  },
	  {
		StratModifiers.MaxFlockBonusMod,
		(object) 1f
	  },
	  {
		StratModifiers.EnableTrade,
		(object) false
	  },
	  {
		StratModifiers.TradeRevenue,
		(object) 1f
	  },
	  {
		StratModifiers.TaxRevenue,
		(object) 1f
	  },
	  {
		StratModifiers.IORevenue,
		(object) 1f
	  },
	  {
		StratModifiers.TradeRangeModifier,
		(object) 1f
	  },
	  {
		StratModifiers.GateCastDistance,
		(object) 0.0f
	  },
	  {
		StratModifiers.GateCastDeviation,
		(object) 0.05f
	  },
	  {
		StratModifiers.BoreSpeedModifier,
		(object) 0.66f
	  },
	  {
		StratModifiers.BoardingPartyModifier,
		(object) 1f
	  },
	  {
		StratModifiers.PhaseDislocationARBonus,
		(object) 0
	  },
	  {
		StratModifiers.PsiPotentialModifier,
		(object) 1f
	  },
	  {
		StratModifiers.PsiPotentialApplyModifier,
		(object) 100f
	  },
	  {
		StratModifiers.RequiresSterileEnvironment,
		(object) false
	  },
	  {
		StratModifiers.MutableFleets,
		(object) false
	  },
	  {
		StratModifiers.ColonyStarvation,
		(object) true
	  },
	  {
		StratModifiers.DiplomacyReactionKillShips,
		(object) -5
	  },
	  {
		StratModifiers.DiplomacyReactionKillColony,
		(object) -75
	  },
	  {
		StratModifiers.DiplomacyReactionKillEnemy,
		(object) 4
	  },
	  {
		StratModifiers.DiplomacyReactionColonizeClaimedWorld,
		(object) -20
	  },
	  {
		StratModifiers.DiplomacyReactionKillRaceWorld,
		(object) -60
	  },
	  {
		StratModifiers.DiplomacyReactionKillGrandMenace,
		(object) 100
	  },
	  {
		StratModifiers.DiplomacyReactionInvadeIndependentWorld,
		(object) -10
	  },
	  {
		StratModifiers.DiplomacyReactionSellSlaves,
		(object) -75
	  },
	  {
		StratModifiers.DiplomacyReactionAIRebellion,
		(object) -100
	  },
	  {
		StratModifiers.DiplomacyReactionKillSuulka,
		(object) 50
	  },
	  {
		StratModifiers.DiplomacyReactionMoney,
		(object) 20
	  },
	  {
		StratModifiers.DiplomacyReactionResearch,
		(object) 10
	  },
	  {
		StratModifiers.DiplomacyReactionSlave,
		(object) 0
	  },
	  {
		StratModifiers.DiplomacyReactionStarChamber,
		(object) 50
	  },
	  {
		StratModifiers.DiplomacyReactionDeclareWar,
		(object) -2000
	  },
	  {
		StratModifiers.DiplomacyReactionPeaceTreaty,
		(object) 5
	  },
	  {
		StratModifiers.DiplomacyReactionBiggerEmpire,
		(object) -3
	  },
	  {
		StratModifiers.DiplomacyReactionSmallerEmpire,
		(object) 0
	  },
	  {
		StratModifiers.DiplomacyReactionBetrayed,
		(object) 25
	  },
	  {
		StratModifiers.DiplomacyReactionBetrayal,
		(object) -100
	  },
	  {
		StratModifiers.DiplomacyDemandWeight,
		(object) 1f
	  },
	  {
		StratModifiers.DiplomacyRequestWeight,
		(object) 1f
	  },
	  {
		StratModifiers.DiplomacyReactionRandomReductionHackMinimum,
		(object) -20
	  },
	  {
		StratModifiers.DiplomacyReactionRandomReductionHackMaximum,
		(object) -1
	  }
	};
		private Vector3 _pieChartColourShipMaintenance = new Vector3();
		private Vector3 _pieChartColourPlanetaryDevelopment = new Vector3();
		private Vector3 _pieChartColourDebtInterest = new Vector3();
		private Vector3 _pieChartColourResearch = new Vector3();
		private Vector3 _pieChartColourSecurity = new Vector3();
		private Vector3 _pieChartColourStimulus = new Vector3();
		private Vector3 _pieChartColourSavings = new Vector3();
		private Vector3 _pieChartColourCorruption = new Vector3();
		private int _accumulatedKnowledgeWeaponPerBattleMin = 5;
		private int _accumulatedKnowledgeWeaponPerBattleMax = 10;
		private int[] _upkeepScienceStation = new int[5];
		private int[] _upkeepNavalStation = new int[5];
		private int[] _upkeepDiplomaticStation = new int[5];
		private int[] _upkeepCivilianStation = new int[5];
		private int[] _upkeepGateStation = new int[5];
		private float _eliteUpkeepCostScale = 1.5f;
		private float _slewModeMultiplier = 1f;
		private float _slewModeDecelMultiplier = 1f;
		private float _slewModeExitRange = 10000f;
		private float _slewModeEnterOffset = 2000f;
		private MineFieldParams _mineFieldParams = new MineFieldParams();
		private SpecialProjectData _specialProjectData = new SpecialProjectData();
		private DefenseManagerSettings _defenseManagerSettings = new DefenseManagerSettings();
		public Vector3 RandomEncounterPrimaryColor = new Vector3(0.3f, 0.3f, 0.3f);
		private AssetDatabase.CritHitChances[] _critHitChances = new AssetDatabase.CritHitChances[8];
		private readonly Random _random = new Random();
		private Dictionary<AIDifficulty, Dictionary<DifficultyModifiers, float>> AIDifficultyBonuses;
		private Dictionary<string, Dictionary<string, object>> _techBonuses;
		private Dictionary<string, GovActionValues> _govActionModifiers;
		private int _randomEncMinTurns;
		private int _randomEncTurnsToResetOdds;
		private float _randomEncMinOdds;
		private float _randomEncMaxOdds;
		private float _randomEncBaseOdds;
		private float _randomEncDecOddsCombat;
		private float _randomEncIncOddsIdle;
		private float _randomEncIncOddsRounds;
		private int _randomEncTurnsToExclude;
		private float _randomEncSinglePlayerOdds;
		private int _largeCombatThreshold;
		private float _infrastructureSupplyRatio;
		private float _populationNoise;
		private float _civilianPopulationGrowthRateMod;
		private float _civilianPopulationTriggerAmount;
		private float _civilianPopulationStartAmount;
		private int _civilianPopulationStartMoral;
		private int _diplomacyPointsPerProvince;
		private int[] _diplomacyPointsPerStation;
		private float _globalProductionModifier;
		private float _stationSupportRangeModifier;
		private float _colonySupportCostFactor;
		private float _baseCorruptionRate;
		private int _evacCivPerCol;
		private int _maxGovernmentShift;
		private float _defaultTacSensorRange;
		private float _defaultBRTacSensorRange;
		private float _defaultPlanetTacSensorRange;
		private float _defaultStratSensorRange;
		private float _policePatrolRadius;
		private int _grandMenaceMinTurn;
		private int _grandMenaceChance;
		private XmlElement _globals;
		private Dictionary<string, object> _cachedGlobals;
		private SwarmerGlobalData _globalSwarmerData;
		private MeteorShowerGlobalData _globalMeteorShowerData;
		private CometGlobalData _globalCometData;
		private NeutronStarGlobalData _globalNeutronStarData;
		private SuperNovaGlobalData _globalSuperNovaData;
		private SlaverGlobalData _globalSlaverData;
		private SpectreGlobalData _globalSpectreData;
		private AsteroidMonitorGlobalData _globalAsteroidMonitorData;
		private MorrigiRelicGlobalData _globalMorrigiRelicData;
		private GardenerGlobalData _globalGardenerData;
		private VonNeumannGlobalData _globalVonNeumannData;
		private LocustGlobalData _globalLocustData;
		private PiracyGlobalData _globalPiracyData;
		private GlobalSpotterRangeData _globalSpotterRanges;
		private float _aiRebellionChance;
		private float _aiRebellionColonyPercent;
		private float _encounterMinStartOffset;
		private float _encounterMaxStartOffset;
		private float _interceptThreshold;
		private float _tradePointPerFreighterFleet;
		private float _populationPerTradePoint;
		private float _incomePerInternationalTradePointMoved;
		private float _incomePerProvincialTradePointMoved;
		private float _incomePerGenericTradePointMoved;
		private float _taxDivider;
		private float _maxDebtMultiplier;
		private int _bankruptcyTurns;
		private float _provinceTradeModifier;
		private float _empireTradeModifier;
		private float _citizensPerImmigrationPoint;
		private int _moralBonusPerPoliceShip;
		private float _populationPerPlanetBeam;
		private float _populationPerPlanetMirv;
		private float _populationPerPlanetMissile;
		private float _populationPerHeavyPlanetMissile;
		private float _planetMissileLaunchDelay;
		private float _planetBeamLaunchDelay;
		private float _forgeWorldImpMaxBonus;
		private float _forgeWorldIOBonus;
		private float _gemWorldCivMaxBonus;
		private int _superWorldSizeConstraint;
		private float _superWorldModifier;
		private float _maxOverharvestRate;
		private float _randomEncOddsPerOrbital;
		private int _securityPointCost;
		private int _requiredIntelPointsForMission;
		private int _requiredCounterIntelPointsForMission;
		private int _colonyFleetSupportPoints;
		private int _stationLvl1FleetSupportPoints;
		private int _stationLvl2FleetSupportPoints;
		private int _stationLvl3FleetSupportPoints;
		private int _stationLvl4FleetSupportPoints;
		private int _stationLvl5FleetSupportPoints;
		private float _minSlaveDeathRate;
		private float _maxSlaveDeathRate;
		private float _imperialProductionMultiplier;
		private float _slaveProductionMultiplier;
		private float _civilianProductionMultiplier;
		private int _miningStationIOBonus;
		private float _flockMaxBonus;
		private float _flockBRBonus;
		private float _flockCRBonus;
		private float _flockDNBonus;
		private float _flockLVBonus;
		private int _flockBRCountBonus;
		private int _flockCRCountBonus;
		private int _flockDNCountBonus;
		private int _flockLVCountBonus;
		private int _declareWarPointCost;
		private int _requestResearchPointCost;
		private int _requestMilitaryAssistancePointCost;
		private int _requestGatePointCost;
		private int _requestEnclavePointCost;
		private int _requestSystemInfoPointCost;
		private int _requestSavingsPointCost;
		private int _demandSavingsPointCost;
		private int _demandResearchPointCost;
		private int _demandSystemInfoPointCost;
		private int _demandSlavesPointCost;
		private int _demandSystemPointCost;
		private int _demandProvincePointCost;
		private int _demandEmpirePointCost;
		private int _treatyArmisticeWarNeutralPointCost;
		private int _treatyArmisticeNeutralCeasefirePointCost;
		private int _treatyArmisticeNeutralNonAggroPointCost;
		private int _treatyArmisticeCeaseFireNonAggroPointCost;
		private int _treatyArmisticeCeaseFirePeacePointCost;
		private int _treatyArmisticeNonAggroPeaceCost;
		private int _treatyArmisticePeaceAlliancePointCost;
		private int _treatyTradePointCost;
		private int _treatyProtectoratePointCost;
		private int _treatyIncorporatePointCost;
		private int _treatyLimitationShipClassPointCost;
		private int _treatyLimitationFleetsPointCost;
		private int _treatyLimitationWeaponsPointCost;
		private int _treatyLimitationResearchTechPointCost;
		private int _treatyLimitationResearchTreePointCost;
		private int _treatyLimitationColoniesPointCost;
		private int _treatyLimitationForgeGemWorldsPointCost;
		private int _treatyLimitationStationType;
		private int _stimulusColonizationBonus;
		private int _stimulusMiningMin;
		private int _stimulusMiningMax;
		private int _stimulusColonizationMin;
		private int _stimulusColonizationMax;
		private int _stimulusTradeMin;
		private int _stimulusTradeMax;
		private int _StationsPerPopulation;
		private int _minLoaCubesOnBuild;
		private int _maxLoaCubesOnBuild;
		private int _LoaCostPerCube;
		private float _LoaDistanceBetweenGates;
		private int _LoaBaseMaxMass;
		private int _LoaMassInductionProjectorsMaxMass;
		private int _LoaMassStandingPulseWavesMaxMass;
		private float _LoaGateSystemMargin;
		private float _LoaTechModMod;
		private float _HomeworldTaxBonusMod;
		private int _upkeepBattleRider;
		private int _upkeepCruiser;
		private int _upkeepDreadnaught;
		private int _upkeepLeviathan;
		private int _upkeepDefensePlatform;
		private float _starSystemEntryPointRange;
		private float _tacStealthArmorBonus;
		private string[] _commonMaterialDictionaries;
		private SkyDefinition[] _skyDefinitions;
		private LogicalTurretHousing[] _turretHousings;
		private LogicalWeapon[] _weapons;
		private LogicalModule[] _modules;
		private LogicalModule[] _modulesToAssignByDefault;
		private LogicalPsionic[] _psionics;
		private SuulkaPsiBonus[] _suulkaPsiBonuses;
		private LogicalShield[] _shields;
		private LogicalShipSpark[] _shipSparks;
		private LogicalEffect _shipEMPEffect;
		private Faction[] _factions;
		private Race[] _races;
		private HashSet<ShipSectionAsset> _shipSections;
		private Dictionary<string, ShipSectionAsset> _shipSectionsByFilename;
		private TechTree _masterTechTree;
		private Kerberos.Sots.Data.TechnologyFramework.Tech[] _masterTechTreeRoots;
		private List<string> _techTreeModels;
		private List<string> _techTreeRoots;
		private static CommonStrings _commonStrings;
		private readonly PlanetGraphicsRules _planetgenrules;
		private readonly string[] _splashScreenImageNames;
		private List<FleetTemplate> _fleetTemplates;
		public readonly Dictionary<DiplomacyStateChange, int> DiplomacyStateChangeMap;

		private static ModuleEnums.ModuleSlotTypes? GetEquivalentModuleSlotType(
		  ModuleEnums.StationModuleType value)
		{
			switch (value)
			{
				case ModuleEnums.StationModuleType.Sensor:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Sensor);
				case ModuleEnums.StationModuleType.Customs:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Customs);
				case ModuleEnums.StationModuleType.Combat:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Combat);
				case ModuleEnums.StationModuleType.Repair:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Repair);
				case ModuleEnums.StationModuleType.Warehouse:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Warehouse);
				case ModuleEnums.StationModuleType.Command:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Command);
				case ModuleEnums.StationModuleType.Dock:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Dock);
				case ModuleEnums.StationModuleType.Terraform:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Terraform);
				case ModuleEnums.StationModuleType.Bastion:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Bastion);
				case ModuleEnums.StationModuleType.Amp:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Amp);
				case ModuleEnums.StationModuleType.GateLab:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.GateLab);
				case ModuleEnums.StationModuleType.Defence:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Defence);
				case ModuleEnums.StationModuleType.AlienHabitation:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.AlienHabitation);
				case ModuleEnums.StationModuleType.Habitation:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Habitation);
				case ModuleEnums.StationModuleType.LargeHabitation:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.LargeHabitation);
				case ModuleEnums.StationModuleType.LargeAlienHabitation:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.LargeAlienHabitation);
				case ModuleEnums.StationModuleType.Lab:
					return new ModuleEnums.ModuleSlotTypes?(ModuleEnums.ModuleSlotTypes.Lab);
				default:
					return new ModuleEnums.ModuleSlotTypes?();
			}
		}

		private static IEnumerable<ModuleEnums.StationModuleType> ResolveSpecificStationModuleTypes(
		  ModuleEnums.StationModuleType value)
		{
			ModuleEnums.ModuleSlotTypes? slotType = AssetDatabase.GetEquivalentModuleSlotType(value);
			if (slotType.HasValue)
				return AssetDatabase.StationModuleTypeToMountTypeMap.Where<KeyValuePair<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes>>((Func<KeyValuePair<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes>, bool>)(x =>
			   {
				   ModuleEnums.ModuleSlotTypes moduleSlotTypes = x.Value;
				   ModuleEnums.ModuleSlotTypes? nullable = slotType;
				   if (moduleSlotTypes == nullable.GetValueOrDefault())
					   return nullable.HasValue;
				   return false;
			   })).Select<KeyValuePair<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes>, ModuleEnums.StationModuleType>((Func<KeyValuePair<ModuleEnums.StationModuleType, ModuleEnums.ModuleSlotTypes>, ModuleEnums.StationModuleType>)(x => x.Key));
			return (IEnumerable<ModuleEnums.StationModuleType>)EmptyEnumerable<ModuleEnums.StationModuleType>.Default;
		}

		public static IEnumerable<ModuleEnums.StationModuleType> ResolveSpecificStationModuleTypes(
		  string faction,
		  ModuleEnums.StationModuleType value)
		{
			return AssetDatabase.ResolveSpecificStationModuleTypes(value);
		}

		public AssetDatabase.MiniMapData GetMiniShipDirectoryFromID(int id)
		{
			return this._miniShipMap.Values.FirstOrDefault<AssetDatabase.MiniMapData>((Func<AssetDatabase.MiniMapData, bool>)(x => x.ID == id));
		}

		public int GetNumMiniShips()
		{
			return this._miniShipMap.Values.Count;
		}

		public AssetDatabase.MiniMapData GetMiniShipDirectory(
		  App game,
		  string faction,
		  FleetType ft,
		  List<ShipInfo> fleetComposition)
		{
			AssetDatabase.MiniMapData miniMapData;
			if (this._miniShipMap.TryGetValue(AssetDatabase.GetMiniMapType(game, faction, ft, fleetComposition), out miniMapData))
				return miniMapData;
			return new AssetDatabase.MiniMapData();
		}

		private static string GetMiniMapType(
		  App game,
		  string faction,
		  FleetType ft,
		  List<ShipInfo> fleetComposition)
		{
			if (fleetComposition.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == game.Game.ScriptModules.Gardeners.GardenerDesignId)))
				return "gardener";
			switch (faction)
			{
				case "human":
				case "hiver":
				case "liir_zuul":
				case "morrigi":
				case "tarkas":
				case "zuul":
				case "slavers":
				case "swarm":
				case "vonneumann":
					return faction;
				case "loa":
					if (ft == FleetType.FL_ACCELERATOR)
						return "loa_gate";
					return faction;
				case "independant_race_a":
					return "indy_a";
				case "independant_race_b":
					return "indy_b";
				case "grandmenaces":
					if (game.Game.ScriptModules.SystemKiller != null && fleetComposition.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == game.Game.ScriptModules.SystemKiller.SystemKillerDesignId)))
						return "systemkiller";
					if (game.Game.ScriptModules.NeutronStar != null && fleetComposition.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == game.Game.ScriptModules.NeutronStar.NeutronDesignId)))
						return "neutronstar";
					break;
				case "locusts":
					return !fleetComposition.Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == game.Game.ScriptModules.Locust.WorldShipDesignId)) ? "locust_moon" : "locust_world";
				case "protean":
					return string.Empty;
			}
			return string.Empty;
		}

		public Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>> GetStationUpgradeRequirements(
		  StationType type)
		{
			switch (type)
			{
				case StationType.NAVAL:
					return this.NavalStationUpgradeRequirements;
				case StationType.SCIENCE:
					return this.ScienceStationUpgradeRequirements;
				case StationType.CIVILIAN:
					return this.CivilianStationUpgradeRequirements;
				case StationType.DIPLOMATIC:
					return this.DiplomaticStationUpgradeRequirements;
				case StationType.GATE:
					return this.GateStationUpgradeRequirements;
				default:
					return (Dictionary<int, Dictionary<ModuleEnums.StationModuleType, int>>)null;
			}
		}

		private void LoadMoralEventModifiers(XmlDocument moralEvents)
		{
			if (moralEvents == null)
				return;
			XmlElement moralEvent = moralEvents["MoralEventModifiers"];
			if (moralEvent == null)
				return;
			foreach (GovernmentInfo.GovernmentType index in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
			{
				XmlElement xmlElement1 = moralEvent[index.ToString()];
				if (xmlElement1 != null)
				{
					foreach (MoralEvent key in Enum.GetValues(typeof(MoralEvent)))
					{
						XmlElement xmlElement2 = xmlElement1[key.ToString()];
						if (xmlElement2 != null)
						{
							if (AssetDatabase.MoralModifierMap[index].ContainsKey(key))
								AssetDatabase.MoralModifierMap[index][key].Clear();
							else
								AssetDatabase.MoralModifierMap[index].Add(key, new List<AssetDatabase.MoralModifier>());
							string attribute1 = xmlElement2.GetAttribute("ApplyMoralEventToAllColonies");
							if (!string.IsNullOrEmpty(attribute1))
								AssetDatabase.MoralModifierMap[index][key].Add(new AssetDatabase.MoralModifier()
								{
									type = AssetDatabase.MoraleModifierType.AllColonies,
									value = int.Parse(attribute1)
								});
							string attribute2 = xmlElement2.GetAttribute("ApplyMoralEventToColony");
							if (!string.IsNullOrEmpty(attribute2))
								AssetDatabase.MoralModifierMap[index][key].Add(new AssetDatabase.MoralModifier()
								{
									type = AssetDatabase.MoraleModifierType.Colony,
									value = int.Parse(attribute2)
								});
							string attribute3 = xmlElement2.GetAttribute("ApplyMoralEventToProvince");
							if (!string.IsNullOrEmpty(attribute3))
								AssetDatabase.MoralModifierMap[index][key].Add(new AssetDatabase.MoralModifier()
								{
									type = AssetDatabase.MoraleModifierType.Province,
									value = int.Parse(attribute3)
								});
							string attribute4 = xmlElement2.GetAttribute("ApplyMoralEventToSystem");
							if (!string.IsNullOrEmpty(attribute4))
								AssetDatabase.MoralModifierMap[index][key].Add(new AssetDatabase.MoralModifier()
								{
									type = AssetDatabase.MoraleModifierType.System,
									value = int.Parse(attribute4)
								});
						}
					}
				}
			}
		}

		public GovernmentEffects GovEffects
		{
			get
			{
				return this._governmentEffects;
			}
		}

		public void LoadDefaultStratModifiers(XmlDocument defaultStratMods)
		{
			if (defaultStratMods == null)
				return;
			XmlElement defaultStratMod = defaultStratMods["StratModifiers"];
			if (defaultStratMod == null)
				return;
			foreach (StratModifiers index in Enum.GetValues(typeof(StratModifiers)))
			{
				if (defaultStratMod[index.ToString()] != null)
					this._defaultStratModifiers[index] = !(this._defaultStratModifiers[index] is bool) ? (!(this._defaultStratModifiers[index] is int) ? (!(this._defaultStratModifiers[index] is float) ? (!(this._defaultStratModifiers[index] is double) ? (object)XmlHelper.GetData<string>(defaultStratMod, index.ToString()) : (object)XmlHelper.GetData<double>(defaultStratMod, index.ToString())) : (object)XmlHelper.GetData<float>(defaultStratMod, index.ToString())) : (object)XmlHelper.GetData<int>(defaultStratMod, index.ToString())) : (object)XmlHelper.GetData<bool>(defaultStratMod, index.ToString());
			}
		}

		public void LoadTechBonusValues(XmlDocument techbonuses)
		{
			if (techbonuses == null)
				return;
			XmlElement techbonuse = techbonuses["TechBonuses"];
			if (techbonuse == null)
				return;
			this._techBonuses = new Dictionary<string, Dictionary<string, object>>();
			foreach (XmlElement xmlElement in techbonuse.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "TechBonus")))
			{
				string attribute1 = xmlElement.GetAttribute("techID");
				if (!string.IsNullOrEmpty(attribute1) && !this._techBonuses.ContainsKey(attribute1))
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					foreach (XmlAttribute attribute2 in (XmlNamedNodeMap)xmlElement.Attributes)
					{
						if (attribute2.Name != "techID")
							dictionary.Add(attribute2.Name, (object)attribute2.Value);
					}
					this._techBonuses.Add(attribute1, dictionary);
				}
			}
		}

		private void InitializeAIDifficultyBonuses()
		{
			this.AIDifficultyBonuses = new Dictionary<AIDifficulty, Dictionary<DifficultyModifiers, float>>();
			this.AIDifficultyBonuses[AIDifficulty.Easy] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.ResearchBonus] = 0.0f;
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.RevenueBonus] = 0.0f;
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.ProductionBonus] = 0.0f;
			this.AIDifficultyBonuses[AIDifficulty.Easy][DifficultyModifiers.PopulationGrowthBonus] = 0.0f;
			this.AIDifficultyBonuses[AIDifficulty.Normal] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.ResearchBonus] = 0.05f;
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.RevenueBonus] = 0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.ProductionBonus] = 0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Normal][DifficultyModifiers.PopulationGrowthBonus] = 0.25f;
			this.AIDifficultyBonuses[AIDifficulty.Hard] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.ResearchBonus] = 0.1f;
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.RevenueBonus] = 0.5f;
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.ProductionBonus] = 0.5f;
			this.AIDifficultyBonuses[AIDifficulty.Hard][DifficultyModifiers.PopulationGrowthBonus] = 0.5f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard] = new Dictionary<DifficultyModifiers, float>();
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.ResearchBonus] = 0.15f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.RevenueBonus] = 1f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.ProductionBonus] = 1f;
			this.AIDifficultyBonuses[AIDifficulty.VeryHard][DifficultyModifiers.PopulationGrowthBonus] = 1f;
		}

		public float GetAIModifier(App game, DifficultyModifiers dm, int playerId)
		{
			if (this.AIDifficultyBonuses == null)
				return 0.0f;
			Player player = game.GetPlayer(playerId);
			if (player == null || !player.IsAI() || (!player.IsStandardPlayer || game.GameSetup == null))
				return 0.0f;
			return this.AIDifficultyBonuses[game.GameSetup.Players[player.ID - 1].AIDifficulty][dm];
		}

		public T GetTechBonus<T>(string techID, string bonusType)
		{
			T obj1 = default(T);
			if (this._techBonuses == null)
			{
				App.Log.Warn("Tech bonuses not found, missing techbonuses.xml document", "data");
				return obj1;
			}
			Dictionary<string, object> dictionary;
			if (this._techBonuses.TryGetValue(techID, out dictionary))
			{
				object obj2;
				if (dictionary.TryGetValue(bonusType, out obj2))
					obj1 = (T)Convert.ChangeType(obj2, typeof(T));
				else
					App.Log.Warn("Did not find bonus " + bonusType + " in tech " + techID, "data");
			}
			else
				App.Log.Warn("Did not find tech " + techID, "data");
			return obj1;
		}

		public void LoadGovernmentActionModifiers(XmlDocument govActions)
		{
			if (govActions == null)
				return;
			XmlElement govAction = govActions["GovernmentAction"];
			if (govAction == null)
				return;
			this._govActionModifiers = new Dictionary<string, GovActionValues>();
			foreach (XmlElement xmlElement in govAction.OfType<XmlElement>())
			{
				if (!this._govActionModifiers.ContainsKey(xmlElement.Name))
				{
					string attribute1 = xmlElement.GetAttribute("xchange");
					string attribute2 = xmlElement.GetAttribute("ychange");
					if (!string.IsNullOrEmpty(attribute1) && !string.IsNullOrEmpty(attribute2))
						this._govActionModifiers.Add(xmlElement.Name, new GovActionValues()
						{
							XChange = int.Parse(attribute1),
							YChange = int.Parse(attribute2)
						});
				}
			}
		}

		public GovActionValues GetGovActionValues(string ga)
		{
			GovActionValues govActionValues = (GovActionValues)null;
			if (this._govActionModifiers.TryGetValue(ga, out govActionValues))
				return govActionValues;
			App.Log.Warn("Did not find Government Action " + ga, "data");
			return (GovActionValues)null;
		}

		public int RandomEncMinTurns
		{
			get
			{
				return this._randomEncMinTurns;
			}
		}

		public int RandomEncTurnsToResetOdds
		{
			get
			{
				return this._randomEncTurnsToResetOdds;
			}
		}

		public float RandomEncMinOdds
		{
			get
			{
				return this._randomEncMinOdds;
			}
		}

		public float RandomEncMaxOdds
		{
			get
			{
				return this._randomEncMaxOdds;
			}
		}

		public float RandomEncBaseOdds
		{
			get
			{
				return this._randomEncBaseOdds;
			}
		}

		public float RandomEncDecOddsCombat
		{
			get
			{
				return this._randomEncDecOddsCombat;
			}
		}

		public float RandomEncIncOddsIdle
		{
			get
			{
				return this._randomEncIncOddsIdle;
			}
		}

		public float RandomEncIncOddsRounds
		{
			get
			{
				return this._randomEncIncOddsRounds;
			}
		}

		public int RandomEncTurnsToExclude
		{
			get
			{
				return this._randomEncTurnsToExclude;
			}
		}

		public float RandomEncSinglePlayerOdds
		{
			get
			{
				return this._randomEncSinglePlayerOdds;
			}
		}

		public int LargeCombatThreshold
		{
			get
			{
				return this._largeCombatThreshold;
			}
		}

		public float InfrastructureSupplyRatio
		{
			get
			{
				return this._infrastructureSupplyRatio;
			}
		}

		public float PopulationNoise
		{
			get
			{
				return this._populationNoise;
			}
		}

		public float CivilianPopulationGrowthRateMod
		{
			get
			{
				return this._civilianPopulationGrowthRateMod;
			}
		}

		public float CivilianPopulationTriggerAmount
		{
			get
			{
				return this._civilianPopulationTriggerAmount;
			}
		}

		public float CivilianPopulationStartAmount
		{
			get
			{
				return this._civilianPopulationStartAmount;
			}
		}

		public int CivilianPopulationStartMoral
		{
			get
			{
				return this._civilianPopulationStartMoral;
			}
		}

		public int DiplomacyPointsPerProvince
		{
			get
			{
				return this._diplomacyPointsPerProvince;
			}
		}

		public int[] DiplomacyPointsPerStation
		{
			get
			{
				return this._diplomacyPointsPerStation;
			}
		}

		public float GlobalProductionModifier
		{
			get
			{
				return this._globalProductionModifier;
			}
		}

		public float StationSupportRangeModifier
		{
			get
			{
				return this._stationSupportRangeModifier;
			}
		}

		public float ColonySupportCostFactor
		{
			get
			{
				return this._colonySupportCostFactor;
			}
		}

		public float BaseCorruptionRate
		{
			get
			{
				return this._baseCorruptionRate;
			}
		}

		public int EvacCivPerCol
		{
			get
			{
				return this._evacCivPerCol;
			}
		}

		public int MaxGovernmentShift
		{
			get
			{
				return this._maxGovernmentShift;
			}
		}

		public float DefaultTacSensorRange
		{
			get
			{
				return this._defaultTacSensorRange;
			}
		}

		public float DefaultBRTacSensorRange
		{
			get
			{
				return this._defaultBRTacSensorRange;
			}
		}

		public float DefaultPlanetSensorRange
		{
			get
			{
				return this._defaultPlanetTacSensorRange;
			}
		}

		public float DefaultStratSensorRange
		{
			get
			{
				return this._defaultStratSensorRange;
			}
		}

		public float PolicePatrolRadius
		{
			get
			{
				return this._policePatrolRadius;
			}
		}

		public int GrandMenaceMinTurn
		{
			get
			{
				return this._grandMenaceMinTurn;
			}
		}

		public int GrandMenaceChance
		{
			get
			{
				return this._grandMenaceChance;
			}
		}

		public SwarmerGlobalData GlobalSwarmerData
		{
			get
			{
				return this._globalSwarmerData;
			}
		}

		public MeteorShowerGlobalData GlobalMeteorShowerData
		{
			get
			{
				return this._globalMeteorShowerData;
			}
		}

		public CometGlobalData GlobalCometData
		{
			get
			{
				return this._globalCometData;
			}
		}

		public NeutronStarGlobalData GlobalNeutronStarData
		{
			get
			{
				return this._globalNeutronStarData;
			}
		}

		public SuperNovaGlobalData GlobalSuperNovaData
		{
			get
			{
				return this._globalSuperNovaData;
			}
		}

		public SlaverGlobalData GlobalSlaverData
		{
			get
			{
				return this._globalSlaverData;
			}
		}

		public SpectreGlobalData GlobalSpectreData
		{
			get
			{
				return this._globalSpectreData;
			}
		}

		public AsteroidMonitorGlobalData GlobalAsteroidMonitorData
		{
			get
			{
				return this._globalAsteroidMonitorData;
			}
		}

		public MorrigiRelicGlobalData GlobalMorrigiRelicData
		{
			get
			{
				return this._globalMorrigiRelicData;
			}
		}

		public GardenerGlobalData GlobalGardenerData
		{
			get
			{
				return this._globalGardenerData;
			}
		}

		public VonNeumannGlobalData GlobalVonNeumannData
		{
			get
			{
				return this._globalVonNeumannData;
			}
		}

		public LocustGlobalData GlobalLocustData
		{
			get
			{
				return this._globalLocustData;
			}
		}

		public PiracyGlobalData GlobalPiracyData
		{
			get
			{
				return this._globalPiracyData;
			}
		}

		public GlobalSpotterRangeData GlobalSpotterRangeData
		{
			get
			{
				return this._globalSpotterRanges;
			}
		}

		public float AIRebellionChance
		{
			get
			{
				return this._aiRebellionChance;
			}
		}

		public float AIRebellionColonyPercent
		{
			get
			{
				return this._aiRebellionColonyPercent;
			}
		}

		public float MinEncounterStartPos
		{
			get
			{
				return this._encounterMinStartOffset;
			}
		}

		public float MaxEncounterStartPos
		{
			get
			{
				return this._encounterMaxStartOffset;
			}
		}

		public float InterceptThreshold
		{
			get
			{
				return this._interceptThreshold;
			}
		}

		public float TradePointPerFreightFleet
		{
			get
			{
				return this._tradePointPerFreighterFleet;
			}
		}

		public float PopulationPerTradePoint
		{
			get
			{
				return this._populationPerTradePoint;
			}
		}

		public float IncomePerInternationalTradePointMoved
		{
			get
			{
				return this._incomePerInternationalTradePointMoved;
			}
		}

		public float IncomePerProvincialTradePointMoved
		{
			get
			{
				return this._incomePerProvincialTradePointMoved;
			}
		}

		public float IncomePerGenericTradePointMoved
		{
			get
			{
				return this._incomePerGenericTradePointMoved;
			}
		}

		public float TaxDivider
		{
			get
			{
				return this._taxDivider;
			}
		}

		public float MaxDebtMultiplier
		{
			get
			{
				return this._maxDebtMultiplier;
			}
		}

		public int BankruptcyTurns
		{
			get
			{
				return this._bankruptcyTurns;
			}
		}

		public float ProvinceTradeModifier
		{
			get
			{
				return this._provinceTradeModifier;
			}
		}

		public float EmpireTradeModifier
		{
			get
			{
				return this._empireTradeModifier;
			}
		}

		public float CitizensPerImmigrationPoint
		{
			get
			{
				return this._citizensPerImmigrationPoint;
			}
		}

		public int MoralBonusPerPoliceShip
		{
			get
			{
				return this._moralBonusPerPoliceShip;
			}
		}

		public float PopulationPerPlanetBeam
		{
			get
			{
				return this._populationPerPlanetBeam;
			}
		}

		public float PopulationPerPlanetMirv
		{
			get
			{
				return this._populationPerPlanetMirv;
			}
		}

		public float PopulationPerPlanetMissile
		{
			get
			{
				return this._populationPerPlanetMissile;
			}
		}

		public float PopulationPerHeavyPlanetMissile
		{
			get
			{
				return this._populationPerHeavyPlanetMissile;
			}
		}

		public float PlanetMissileDelay
		{
			get
			{
				return this._planetMissileLaunchDelay;
			}
		}

		public float PlanetBeamDelay
		{
			get
			{
				return this._planetBeamLaunchDelay;
			}
		}

		public float ForgeWorldImpMaxBonus
		{
			get
			{
				return this._forgeWorldImpMaxBonus;
			}
		}

		public float ForgeWorldIOBonus
		{
			get
			{
				return this._forgeWorldIOBonus;
			}
		}

		public float GemWorldCivMaxBonus
		{
			get
			{
				return this._gemWorldCivMaxBonus;
			}
		}

		public int SuperWorldSizeConstraint
		{
			get
			{
				return this._superWorldSizeConstraint;
			}
		}

		public float SuperWorldModifier
		{
			get
			{
				return this._superWorldModifier;
			}
		}

		public float MaxOverharvestRate
		{
			get
			{
				return this._maxOverharvestRate;
			}
		}

		public float RandomEncOddsPerOrbital
		{
			get
			{
				return this._randomEncOddsPerOrbital;
			}
		}

		public int SecurityPointCost
		{
			get
			{
				return this._securityPointCost;
			}
		}

		public int RequiredIntelPointsForMission
		{
			get
			{
				return this._requiredIntelPointsForMission;
			}
		}

		public int RequiredCounterIntelPointsForMission
		{
			get
			{
				return this._requiredCounterIntelPointsForMission;
			}
		}

		public int ColonyFleetSupportPoints
		{
			get
			{
				return this._colonyFleetSupportPoints;
			}
		}

		public int StationLvl1FleetSupportPoints
		{
			get
			{
				return this._stationLvl1FleetSupportPoints;
			}
		}

		public int StationLvl2FleetSupportPoints
		{
			get
			{
				return this._stationLvl2FleetSupportPoints;
			}
		}

		public int StationLvl3FleetSupportPoints
		{
			get
			{
				return this._stationLvl3FleetSupportPoints;
			}
		}

		public int StationLvl4FleetSupportPoints
		{
			get
			{
				return this._stationLvl4FleetSupportPoints;
			}
		}

		public int StationLvl5FleetSupportPoints
		{
			get
			{
				return this._stationLvl5FleetSupportPoints;
			}
		}

		public float MinSlaveDeathRate
		{
			get
			{
				return this._minSlaveDeathRate;
			}
		}

		public float MaxSlaveDeathRate
		{
			get
			{
				return this._maxSlaveDeathRate;
			}
		}

		public float ImperialProductionMultiplier
		{
			get
			{
				return this._imperialProductionMultiplier;
			}
		}

		public float SlaveProductionMultiplier
		{
			get
			{
				return this._slaveProductionMultiplier;
			}
		}

		public float CivilianProductionMultiplier
		{
			get
			{
				return this._civilianProductionMultiplier;
			}
		}

		public int MiningStationIOBonus
		{
			get
			{
				return this._miningStationIOBonus;
			}
		}

		public float FlockMaxBonus
		{
			get
			{
				return this._flockMaxBonus;
			}
		}

		public float FlockBRBonus
		{
			get
			{
				return this._flockBRBonus;
			}
		}

		public float FlockCRBonus
		{
			get
			{
				return this._flockCRBonus;
			}
		}

		public float FlockDNBonus
		{
			get
			{
				return this._flockDNBonus;
			}
		}

		public float FlockLVBonus
		{
			get
			{
				return this._flockLVBonus;
			}
		}

		public int FlockBRCountBonus
		{
			get
			{
				return this._flockBRCountBonus;
			}
		}

		public int FlockCRCountBonus
		{
			get
			{
				return this._flockCRCountBonus;
			}
		}

		public int FlockDNCountBonus
		{
			get
			{
				return this._flockDNCountBonus;
			}
		}

		public int FlockLVCountBonus
		{
			get
			{
				return this._flockLVCountBonus;
			}
		}

		public int DeclareWarPointCost
		{
			get
			{
				return this._declareWarPointCost;
			}
		}

		public int RequestResearchPointCost
		{
			get
			{
				return this._requestResearchPointCost;
			}
		}

		public int RequestMilitaryAssistancePointCost
		{
			get
			{
				return this._requestMilitaryAssistancePointCost;
			}
		}

		public int RequestGatePointCost
		{
			get
			{
				return this._requestGatePointCost;
			}
		}

		public int RequestEnclavePointCost
		{
			get
			{
				return this._requestEnclavePointCost;
			}
		}

		public int RequestSystemInfoPointCost
		{
			get
			{
				return this._requestSystemInfoPointCost;
			}
		}

		public int RequestSavingsPointCost
		{
			get
			{
				return this._requestSavingsPointCost;
			}
		}

		public int DemandSavingsPointCost
		{
			get
			{
				return this._demandSavingsPointCost;
			}
		}

		public int DemandResearchPointCost
		{
			get
			{
				return this._demandResearchPointCost;
			}
		}

		public int DemandSystemInfoPointCost
		{
			get
			{
				return this._demandSystemInfoPointCost;
			}
		}

		public int DemandSlavesPointCost
		{
			get
			{
				return this._demandSlavesPointCost;
			}
		}

		public int DemandSystemPointCost
		{
			get
			{
				return this._demandSystemPointCost;
			}
		}

		public int DemandProvincePointCost
		{
			get
			{
				return this._demandProvincePointCost;
			}
		}

		public int DemandEmpirePointCost
		{
			get
			{
				return this._demandEmpirePointCost;
			}
		}

		public int TreatyArmisticeWarNeutralPointCost
		{
			get
			{
				return this._treatyArmisticeWarNeutralPointCost;
			}
		}

		public int TreatyArmisticeNeutralCeasefirePointCost
		{
			get
			{
				return this._treatyArmisticeNeutralCeasefirePointCost;
			}
		}

		public int TreatyArmisticeNeutralNonAggroPointCost
		{
			get
			{
				return this._treatyArmisticeNeutralNonAggroPointCost;
			}
		}

		public int TreatyArmisticeCeaseFireNonAggroPointCost
		{
			get
			{
				return this._treatyArmisticeCeaseFireNonAggroPointCost;
			}
		}

		public int TreatyArmisticeCeaseFirePeacePointCost
		{
			get
			{
				return this._treatyArmisticeCeaseFirePeacePointCost;
			}
		}

		public int TreatyArmisticeNonAggroPeaceCost
		{
			get
			{
				return this._treatyArmisticeNonAggroPeaceCost;
			}
		}

		public int TreatyArmisticePeaceAllianceCost
		{
			get
			{
				return this._treatyArmisticePeaceAlliancePointCost;
			}
		}

		public int TreatyTradePointCost
		{
			get
			{
				return this._treatyTradePointCost;
			}
		}

		public int TreatyIncorporatePointCost
		{
			get
			{
				return this._treatyIncorporatePointCost;
			}
		}

		public int TreatyProtectoratePointCost
		{
			get
			{
				return this._treatyProtectoratePointCost;
			}
		}

		public int TreatyLimitationShipClassPointCost
		{
			get
			{
				return this._treatyLimitationShipClassPointCost;
			}
		}

		public int TreatyLimitationFleetsPointCost
		{
			get
			{
				return this._treatyLimitationFleetsPointCost;
			}
		}

		public int TreatyLimitationWeaponsPointCost
		{
			get
			{
				return this._treatyLimitationWeaponsPointCost;
			}
		}

		public int TreatyLimitationResearchTechPointCost
		{
			get
			{
				return this._treatyLimitationResearchTechPointCost;
			}
		}

		public int TreatyLimitationResearchTreePointCost
		{
			get
			{
				return this._treatyLimitationResearchTreePointCost;
			}
		}

		public int TreatyLimitationColoniesPointCost
		{
			get
			{
				return this._treatyLimitationColoniesPointCost;
			}
		}

		public int TreatyLimitationForgeGemWorldsPointCost
		{
			get
			{
				return this._treatyLimitationForgeGemWorldsPointCost;
			}
		}

		public int TreatyLimitationStationType
		{
			get
			{
				return this._treatyLimitationStationType;
			}
		}

		public int StimulusColonizationBonus
		{
			get
			{
				return this._stimulusColonizationBonus;
			}
		}

		public int StimulusMiningMin
		{
			get
			{
				return this._stimulusMiningMin;
			}
		}

		public int StimulusMiningMax
		{
			get
			{
				return this._stimulusMiningMax;
			}
		}

		public int StimulusColonizationMin
		{
			get
			{
				return this._stimulusColonizationMin;
			}
		}

		public int StimulusColonizationMax
		{
			get
			{
				return this._stimulusColonizationMax;
			}
		}

		public int StimulusTradeMin
		{
			get
			{
				return this._stimulusTradeMin;
			}
		}

		public int StimulusTradeMax
		{
			get
			{
				return this._stimulusTradeMax;
			}
		}

		public int StationsPerPop
		{
			get
			{
				return this._StationsPerPopulation;
			}
		}

		public int MinLoaCubesOnBuild
		{
			get
			{
				return this._minLoaCubesOnBuild;
			}
		}

		public int MaxLoaCubesOnBuild
		{
			get
			{
				return this._maxLoaCubesOnBuild;
			}
		}

		public int LoaCostPerCube
		{
			get
			{
				return this._LoaCostPerCube;
			}
		}

		public float LoaDistanceBetweenGates
		{
			get
			{
				return this._LoaDistanceBetweenGates;
			}
		}

		public int LoaBaseMaxMass
		{
			get
			{
				return this._LoaBaseMaxMass;
			}
		}

		public int LoaMassInductionProjectorsMaxMass
		{
			get
			{
				return this._LoaMassInductionProjectorsMaxMass;
			}
		}

		public int LoaMassStandingPulseWavesMaxMass
		{
			get
			{
				return this._LoaMassStandingPulseWavesMaxMass;
			}
		}

		public float LoaGateSystemMargin
		{
			get
			{
				return this._LoaGateSystemMargin;
			}
		}

		public float LoaTechModMod
		{
			get
			{
				return this._LoaTechModMod;
			}
		}

		public float HomeworldTaxBonusMod
		{
			get
			{
				return this._HomeworldTaxBonusMod;
			}
		}

		public Vector3 PieChartColourShipMaintenance
		{
			get
			{
				return this._pieChartColourShipMaintenance;
			}
		}

		public Vector3 PieChartColourPlanetaryDevelopment
		{
			get
			{
				return this._pieChartColourPlanetaryDevelopment;
			}
		}

		public Vector3 PieChartColourDebtInterest
		{
			get
			{
				return this._pieChartColourDebtInterest;
			}
		}

		public Vector3 PieChartColourResearch
		{
			get
			{
				return this._pieChartColourResearch;
			}
		}

		public Vector3 PieChartColourSecurity
		{
			get
			{
				return this._pieChartColourSecurity;
			}
		}

		public Vector3 PieChartColourStimulus
		{
			get
			{
				return this._pieChartColourStimulus;
			}
		}

		public Vector3 PieChartColourSavings
		{
			get
			{
				return this._pieChartColourSavings;
			}
		}

		public Vector3 PieChartColourCorruption
		{
			get
			{
				return this._pieChartColourCorruption;
			}
		}

		public int AccumulatedKnowledgeWeaponPerBattleMin
		{
			get
			{
				return this._accumulatedKnowledgeWeaponPerBattleMin;
			}
		}

		public int AccumulatedKnowledgeWeaponPerBattleMax
		{
			get
			{
				return this._accumulatedKnowledgeWeaponPerBattleMax;
			}
		}

		public int UpkeepBattleRider
		{
			get
			{
				return this._upkeepBattleRider;
			}
		}

		public int UpkeepCruiser
		{
			get
			{
				return this._upkeepCruiser;
			}
		}

		public int UpkeepDreadnaught
		{
			get
			{
				return this._upkeepDreadnaught;
			}
		}

		public int UpkeepLeviathan
		{
			get
			{
				return this._upkeepLeviathan;
			}
		}

		public int UpkeepDefensePlatform
		{
			get
			{
				return this._upkeepDefensePlatform;
			}
		}

		public int[] UpkeepScienceStation
		{
			get
			{
				return this._upkeepScienceStation;
			}
		}

		public int[] UpkeepNavalStation
		{
			get
			{
				return this._upkeepNavalStation;
			}
		}

		public int[] UpkeepDiplomaticStation
		{
			get
			{
				return this._upkeepDiplomaticStation;
			}
		}

		public int[] UpkeepCivilianStation
		{
			get
			{
				return this._upkeepCivilianStation;
			}
		}

		public int[] UpkeepGateStation
		{
			get
			{
				return this._upkeepGateStation;
			}
		}

		public float EliteUpkeepCostScale
		{
			get
			{
				return this._eliteUpkeepCostScale;
			}
		}

		public float StarSystemEntryPointRange
		{
			get
			{
				return this._starSystemEntryPointRange;
			}
		}

		public float TacStealthArmorBonus
		{
			get
			{
				return this._tacStealthArmorBonus;
			}
		}

		public float SlewModeMultiplier
		{
			get
			{
				return this._slewModeMultiplier;
			}
		}

		public float SlewModeDecelMultiplier
		{
			get
			{
				return this._slewModeDecelMultiplier;
			}
		}

		public float SlewModeExitRange
		{
			get
			{
				return this._slewModeExitRange;
			}
		}

		public float SlewModeEnterOffset
		{
			get
			{
				return this._slewModeEnterOffset;
			}
		}

		public MineFieldParams MineFieldParams
		{
			get
			{
				return this._mineFieldParams;
			}
		}

		public SpecialProjectData SpecialProjectData
		{
			get
			{
				return this._specialProjectData;
			}
		}

		public DefenseManagerSettings DefenseManagerSettings
		{
			get
			{
				return this._defenseManagerSettings;
			}
		}

		public AssetDatabase.CritHitChances[] CriticalHitChances
		{
			get
			{
				return this._critHitChances;
			}
		}

		public IntelMissionDescMap IntelMissions { get; private set; }

		public string GetLocalizedTechnologyName(string techId)
		{
			return AssetDatabase.CommonStrings.Localize("@TECH_NAME_" + techId);
		}

		public string GetLocalizedStationTypeName(StationType value, bool useZuulStations)
		{
			switch (value)
			{
				case StationType.INVALID_TYPE:
				case StationType.NUM_TYPES:
					throw new ArgumentOutOfRangeException(nameof(value));
				default:
					if (useZuulStations)
						return App.Localize("@STATION_TYPE_" + value.ToString().ToUpperInvariant() + "_ZUUL");
					return App.Localize("@STATION_TYPE_" + value.ToString().ToUpperInvariant());
			}
		}

		public IEnumerable<string> MaterialDictionaries
		{
			get
			{
				foreach (string materialDictionary in this._commonMaterialDictionaries)
					yield return materialDictionary;
				foreach (Faction faction in this._factions)
				{
					foreach (string materialDictionary in faction.MaterialDictionaries)
						yield return materialDictionary;
				}
			}
		}

		public Dictionary<StratModifiers, object> DefaultStratModifiers
		{
			get
			{
				return this._defaultStratModifiers;
			}
		}

		public Dictionary<RandomEncounter, int> RandomEncounterOdds
		{
			get
			{
				return this._randomEncounterOdds;
			}
		}

		public Dictionary<EasterEgg, int> EasterEggOdds
		{
			get
			{
				return this._easterEggOdds;
			}
		}

		public Dictionary<EasterEgg, int> GMOdds
		{
			get
			{
				return this._gmOdds;
			}
		}

		public IEnumerable<LogicalTurretHousing> TurretHousings
		{
			get
			{
				return (IEnumerable<LogicalTurretHousing>)this._turretHousings;
			}
		}

		public IEnumerable<LogicalWeapon> Weapons
		{
			get
			{
				return (IEnumerable<LogicalWeapon>)this._weapons;
			}
		}

		public IEnumerable<LogicalModule> Modules
		{
			get
			{
				return (IEnumerable<LogicalModule>)this._modules;
			}
		}

		public IEnumerable<LogicalModule> ModulesToAssignByDefault
		{
			get
			{
				return (IEnumerable<LogicalModule>)this._modulesToAssignByDefault;
			}
		}

		public IEnumerable<LogicalPsionic> Psionics
		{
			get
			{
				return (IEnumerable<LogicalPsionic>)this._psionics;
			}
		}

		public IEnumerable<SuulkaPsiBonus> SuulkaPsiBonuses
		{
			get
			{
				return (IEnumerable<SuulkaPsiBonus>)this._suulkaPsiBonuses;
			}
		}

		public IEnumerable<LogicalShield> Shields
		{
			get
			{
				return (IEnumerable<LogicalShield>)this._shields;
			}
		}

		public IEnumerable<LogicalShipSpark> ShipSparks
		{
			get
			{
				return (IEnumerable<LogicalShipSpark>)this._shipSparks;
			}
		}

		public LogicalEffect ShipEMPEffect
		{
			get
			{
				return this._shipEMPEffect;
			}
		}

		public IEnumerable<Faction> Factions
		{
			get
			{
				return (IEnumerable<Faction>)this._factions;
			}
		}

		public IEnumerable<Race> Races
		{
			get
			{
				return (IEnumerable<Race>)this._races;
			}
		}

		public string[] SplashScreenImageNames
		{
			get
			{
				return this._splashScreenImageNames;
			}
		}

		public IList<SkyDefinition> SkyDefinitions
		{
			get
			{
				return (IList<SkyDefinition>)this._skyDefinitions;
			}
		}

		public HashSet<ShipSectionAsset> ShipSections
		{
			get
			{
				return this._shipSections;
			}
		}

		public PlanetGraphicsRules PlanetGenerationRules
		{
			get
			{
				return this._planetgenrules;
			}
		}

		public TechTree MasterTechTree
		{
			get
			{
				return this._masterTechTree;
			}
		}

		public Kerberos.Sots.Data.TechnologyFramework.Tech[] MasterTechTreeRoots
		{
			get
			{
				return this._masterTechTreeRoots;
			}
		}

		public IEnumerable<string> TechTreeModels
		{
			get
			{
				return (IEnumerable<string>)this._techTreeModels;
			}
		}

		public IEnumerable<string> TechTreeRoots
		{
			get
			{
				return (IEnumerable<string>)this._techTreeRoots;
			}
		}

		public static CommonStrings CommonStrings
		{
			get
			{
				return AssetDatabase._commonStrings;
			}
		}

		public List<FleetTemplate> FleetTemplates
		{
			get
			{
				return this._fleetTemplates;
			}
		}

		public int GetDiplomaticRequestPointCost(RequestType rt)
		{
			switch (rt)
			{
				case RequestType.SavingsRequest:
					return this.RequestSavingsPointCost;
				case RequestType.SystemInfoRequest:
					return this.RequestSystemInfoPointCost;
				case RequestType.ResearchPointsRequest:
					return this.RequestResearchPointCost;
				case RequestType.MilitaryAssistanceRequest:
					return this.RequestMilitaryAssistancePointCost;
				case RequestType.GatePermissionRequest:
					return this.RequestGatePointCost;
				case RequestType.EstablishEnclaveRequest:
					return this.RequestEnclavePointCost;
				default:
					throw new ArgumentOutOfRangeException(nameof(rt));
			}
		}

		public int GetDiplomaticDemandPointCost(DemandType dt)
		{
			switch (dt)
			{
				case DemandType.SavingsDemand:
					return this.DemandSavingsPointCost;
				case DemandType.SystemInfoDemand:
					return this.DemandSystemInfoPointCost;
				case DemandType.ResearchPointsDemand:
					return this.DemandResearchPointCost;
				case DemandType.SlavesDemand:
					return this.DemandSlavesPointCost;
				case DemandType.WorldDemand:
					return this.DemandSystemPointCost;
				case DemandType.ProvinceDemand:
					return this.DemandProvincePointCost;
				case DemandType.SurrenderDemand:
					return this.DemandEmpirePointCost;
				default:
					throw new ArgumentOutOfRangeException(nameof(dt));
			}
		}

		public float GetPlagueInfectionRate(WeaponEnums.PlagueType pt)
		{
			switch (pt)
			{
				case WeaponEnums.PlagueType.BASIC:
					return 2f;
				case WeaponEnums.PlagueType.RETRO:
					return 4f;
				case WeaponEnums.PlagueType.BEAST:
					return 6f;
				case WeaponEnums.PlagueType.ASSIM:
					return 6f;
				case WeaponEnums.PlagueType.XOMBIE:
					return 8f;
				default:
					return 0.0f;
			}
		}

		public bool IsPotentialyHabitable(string planettype)
		{
			return planettype == "normal" || planettype == "pastoral" || (planettype == "volcanic" || planettype == "cavernous") || (planettype == "tempestuous" || planettype == "magnar" || planettype == "primordial");
		}

		public bool IsGasGiant(string planettype)
		{
			return planettype == "gaseous";
		}

		public bool IsMoon(string planettype)
		{
			return planettype == "barren";
		}

		public static string GetModuleFactionName(ModuleEnums.StationModuleType type)
		{
			string str = "";
			if (type.ToString().Contains("Morrigi"))
				str = "morrigi";
			if (type.ToString().Contains("Human"))
				str = "human";
			if (type.ToString().Contains("Tarkas"))
				str = "tarkas";
			if (type.ToString().Contains("Hiver"))
				str = "hiver";
			if (type.ToString().Contains("Liir"))
				str = "liir_zuul";
			if (type.ToString().Contains("Zuul"))
				str = "zuul";
			if (type.ToString().Contains("Loa"))
				str = "loa";
			return str;
		}

		public Faction GetFaction(string name)
		{
			foreach (Faction faction in this._factions)
			{
				if (faction.Name == name)
					return faction;
			}
			return (Faction)null;
		}

		public Faction GetFaction(int factionId)
		{
			foreach (Faction faction in this._factions)
			{
				if (faction.ID == factionId)
					return faction;
			}
			return (Faction)null;
		}

		public object GetFactionStratModifier(string name, string variable)
		{
			return this.GetFaction(name)?.GetStratModifier(variable);
		}

		public object GetFactionStratModifier(int factionId, string variable)
		{
			return this.GetFaction(factionId)?.GetStratModifier(variable);
		}

		public Race GetRace(string name)
		{
			foreach (Race race in this._races)
			{
				if (race.Name == name)
					return race;
			}
			return (Race)null;
		}

		public ShipSectionAsset GetShipSectionAsset(string filename)
		{
			ShipSectionAsset shipSectionAsset;
			if (this._shipSectionsByFilename.TryGetValue(filename, out shipSectionAsset))
				return shipSectionAsset;
			return (ShipSectionAsset)null;
		}

		public T GetGlobal<T>(string name)
		{
			object obj;
			if (!this._cachedGlobals.TryGetValue(name, out obj))
			{
				XmlElement global = this._globals[name];
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
				if (global == null || converter == null)
					throw new Exception("Unable to retreive global value " + name + " or unable to convert to specified type.");
				obj = converter.ConvertFromString((ITypeDescriptorContext)null, CultureInfo.InvariantCulture, global.InnerText);
				this._cachedGlobals[name] = obj;
			}
			return (T)obj;
		}

		private void LoadGlobalsAndStratModifiers(XmlDocument d)
		{
			XmlElement rootNode1 = d["CommonAssets"]["Globals"];
			this._globals = rootNode1;
			this._defaultTacSensorRange = XmlHelper.GetData<float>(rootNode1, "DefaultTacSensorRange");
			this._defaultBRTacSensorRange = XmlHelper.GetData<float>(rootNode1, "DefaultBRTacSensorRange");
			this._defaultPlanetTacSensorRange = XmlHelper.GetData<float>(rootNode1, "DefaultPlanetTacSensorRange");
			this._defaultStratSensorRange = XmlHelper.GetData<float>(rootNode1, "DefaultStratSensorRange");
			this._policePatrolRadius = XmlHelper.GetData<float>(rootNode1, "PolicePatrolRadius");
			this._grandMenaceMinTurn = XmlHelper.GetData<int>(rootNode1, "GrandMenaceMinTurn");
			this._grandMenaceChance = XmlHelper.GetData<int>(rootNode1, "GrandMenaceChance");
			this._randomEncMinTurns = XmlHelper.GetData<int>(rootNode1, "RandomEncounterMinTurns");
			this._randomEncTurnsToResetOdds = XmlHelper.GetData<int>(rootNode1, "RandomEncounterTurnsToResetOdds");
			this._randomEncMinOdds = XmlHelper.GetData<float>(rootNode1, "RandomEncounterMinOdds");
			this._randomEncMaxOdds = XmlHelper.GetData<float>(rootNode1, "RandomEncounterMaxOdds");
			this._randomEncBaseOdds = XmlHelper.GetData<float>(rootNode1, "RandomEncounterBaseOdds");
			this._randomEncDecOddsCombat = XmlHelper.GetData<float>(rootNode1, "RandomEncounterDecOddsCombat");
			this._randomEncIncOddsIdle = XmlHelper.GetData<float>(rootNode1, "RandomEncounterIncOddsIdle");
			this._randomEncIncOddsRounds = XmlHelper.GetData<float>(rootNode1, "RandomEncounterIncOddsRounds");
			this._randomEncTurnsToExclude = XmlHelper.GetData<int>(rootNode1, "RandomEncounterTurnsToExclude");
			this._randomEncSinglePlayerOdds = XmlHelper.GetData<float>(rootNode1, "RandomEncounterSinglePlayerOdds");
			this._largeCombatThreshold = XmlHelper.GetData<int>(rootNode1, "LargeCombatThreshold");
			this._infrastructureSupplyRatio = XmlHelper.GetData<float>(rootNode1, "InfrastructureSupplyRatio");
			this._populationNoise = XmlHelper.GetData<float>(rootNode1, "PopulationNoise");
			this._civilianPopulationGrowthRateMod = XmlHelper.GetData<float>(rootNode1, "CivilianPopulationGrowthRateMod");
			this._civilianPopulationTriggerAmount = XmlHelper.GetData<float>(rootNode1, "CivilianPopulationTriggerAmount");
			this._civilianPopulationStartAmount = XmlHelper.GetData<float>(rootNode1, "CivilianPopulationStartAmount");
			this._civilianPopulationStartMoral = XmlHelper.GetData<int>(rootNode1, "CivilianPopulationStartMoral");
			this._diplomacyPointsPerProvince = XmlHelper.GetData<int>(rootNode1, "DiplomacyPointsPerProvince");
			this._diplomacyPointsPerStation = new int[5];
			this._diplomacyPointsPerStation[0] = XmlHelper.GetData<int>(rootNode1, "DiplomacyPointsPerStationLevel1");
			this._diplomacyPointsPerStation[1] = XmlHelper.GetData<int>(rootNode1, "DiplomacyPointsPerStationLevel2");
			this._diplomacyPointsPerStation[2] = XmlHelper.GetData<int>(rootNode1, "DiplomacyPointsPerStationLevel3");
			this._diplomacyPointsPerStation[3] = XmlHelper.GetData<int>(rootNode1, "DiplomacyPointsPerStationLevel4");
			this._diplomacyPointsPerStation[4] = XmlHelper.GetData<int>(rootNode1, "DiplomacyPointsPerStationLevel5");
			this._globalProductionModifier = XmlHelper.GetData<float>(rootNode1, "GlobalProductionModifier");
			this._stationSupportRangeModifier = XmlHelper.GetData<float>(rootNode1, "StationSupportRangeModifier");
			this._colonySupportCostFactor = XmlHelper.GetData<float>(rootNode1, "ColonySupportCostFactor");
			this._baseCorruptionRate = XmlHelper.GetData<float>(rootNode1, "BaseCorruptionRate");
			this._evacCivPerCol = XmlHelper.GetData<int>(rootNode1, "EvacuationCivPerCol");
			this._maxGovernmentShift = XmlHelper.GetData<int>(rootNode1, "MaxGovernmentShift");
			XmlElement rootNode2 = rootNode1["SwarmerData"];
			this._globalSwarmerData = new SwarmerGlobalData();
			if (rootNode2 != null)
			{
				this._globalSwarmerData.GrowthRateLarvaSpawn = XmlHelper.GetData<int>(rootNode2, "GrowthRateLarvaSpawn");
				this._globalSwarmerData.GrowthRateQueenSpawn = XmlHelper.GetData<int>(rootNode2, "GrowthRateQueenSpawn");
				this._globalSwarmerData.NumHiveSwarmers = XmlHelper.GetData<int>(rootNode2, "NumHiveSwarmers");
				this._globalSwarmerData.NumHiveGuardians = XmlHelper.GetData<int>(rootNode2, "NumHiveGuardians");
				this._globalSwarmerData.NumQueenSwarmers = XmlHelper.GetData<int>(rootNode2, "NumQueenSwarmers");
				this._globalSwarmerData.NumQueenGuardians = XmlHelper.GetData<int>(rootNode2, "NumQueenGuardians");
			}
			XmlElement rootNode3 = rootNode1["MeteorShowerData"];
			this._globalMeteorShowerData = new MeteorShowerGlobalData();
			if (rootNode3 != null)
			{
				this._globalMeteorShowerData.LargeMeteorChance = XmlHelper.GetData<int>(rootNode3, "MeteorShowerLargeMeteorChance");
				this._globalMeteorShowerData.MinMeteors = XmlHelper.GetData<int>(rootNode3, "MeteorShowerMinMeteors");
				this._globalMeteorShowerData.MaxMeteors = XmlHelper.GetData<int>(rootNode3, "MeteorShowerMaxMeteors");
				this._globalMeteorShowerData.NumBreakoffMeteors = XmlHelper.GetData<int>(rootNode3, "NumBreakoffMeteors");
				for (int index = 0; index < 3; ++index)
					this._globalMeteorShowerData.Damage[index] = new CombatAIDamageData();
				this._globalMeteorShowerData.Damage[0].SetDataFromElement(rootNode3["SmallMeteorData"]);
				this._globalMeteorShowerData.Damage[1].SetDataFromElement(rootNode3["MediumMeteorData"]);
				this._globalMeteorShowerData.Damage[2].SetDataFromElement(rootNode3["LargeMeteorData"]);
				this._globalMeteorShowerData.ResourceBonuses[0] = rootNode3["SmallMeteorData"] != null ? int.Parse(rootNode3["SmallMeteorData"].GetAttribute("resources")) : 0;
				this._globalMeteorShowerData.ResourceBonuses[1] = rootNode3["MediumMeteorData"] != null ? int.Parse(rootNode3["MediumMeteorData"].GetAttribute("resources")) : 0;
				this._globalMeteorShowerData.ResourceBonuses[2] = rootNode3["LargeMeteorData"] != null ? int.Parse(rootNode3["LargeMeteorData"].GetAttribute("resources")) : 0;
			}
			XmlElement xmlElement1 = rootNode1["CometData"];
			this._globalCometData = new CometGlobalData();
			if (xmlElement1 != null)
				this._globalCometData.Damage.SetDataFromElement(xmlElement1["CometDamage"]);
			XmlElement rootNode4 = rootNode1["NeutronStarData"];
			this._globalNeutronStarData = new NeutronStarGlobalData();
			if (rootNode4 != null)
			{
				this._globalNeutronStarData.Speed = XmlHelper.GetData<float>(rootNode4, "StarSpeed");
				this._globalNeutronStarData.AffectRange = XmlHelper.GetData<float>(rootNode4, "GravityAffectRange");
				this._globalNeutronStarData.MeteorRatio = XmlHelper.GetData<int>(rootNode4, "MeteorRatio");
				this._globalNeutronStarData.CometRatio = Math.Max(100 - this._globalNeutronStarData.MeteorRatio, 0);
				this._globalNeutronStarData.MaxMeteorIntensity = Math.Max(XmlHelper.GetData<float>(rootNode4, "MaxMeteorIntensity"), 1f);
			}
			XmlElement rootNode5 = rootNode1["SuperNovaData"];
			this._globalSuperNovaData = new SuperNovaGlobalData();
			if (rootNode5 != null)
			{
				this._globalSuperNovaData.MinTurns = XmlHelper.GetData<int>(rootNode5, "MinTurns");
				this._globalSuperNovaData.Chance = XmlHelper.GetData<int>(rootNode5, "Chance");
				this._globalSuperNovaData.BlastRadius = XmlHelper.GetData<float>(rootNode5, "BlastRadius");
				this._globalSuperNovaData.MinExplodeTurns = XmlHelper.GetData<int>(rootNode5, "MinExplodeTurns");
				this._globalSuperNovaData.MaxExplodeTurns = XmlHelper.GetData<int>(rootNode5, "MaxExplodeTurns");
				this._globalSuperNovaData.SystemInRangeBioReduction = XmlHelper.GetData<int>(rootNode5, "SystemInRangeBioReduction");
				this._globalSuperNovaData.SystemInRangeMinHazard = XmlHelper.GetData<float>(rootNode5, "SystemInRangeMinHazard");
				this._globalSuperNovaData.SystemInRangeMaxHazard = XmlHelper.GetData<float>(rootNode5, "SystemInRangeMaxHazard");
			}
			XmlElement rootNode6 = rootNode1["SlaverData"];
			this._globalSlaverData = new SlaverGlobalData();
			if (rootNode6 != null)
			{
				this._globalSlaverData.MinAbductors = XmlHelper.GetData<int>(rootNode6, "SlaverMinAbductors");
				this._globalSlaverData.MaxAbductors = XmlHelper.GetData<int>(rootNode6, "SlaverMaxAbductors");
				this._globalSlaverData.MinScavengers = XmlHelper.GetData<int>(rootNode6, "SlaverMinScavengers");
				this._globalSlaverData.MaxScavengers = XmlHelper.GetData<int>(rootNode6, "SlaverMaxScavengers");
			}
			XmlElement rootNode7 = rootNode1["SpectreData"];
			this._globalSpectreData = new SpectreGlobalData();
			if (rootNode7 != null)
			{
				this._globalSpectreData.MinSpectres = XmlHelper.GetData<int>(rootNode7, "SpectreMinSpectres");
				this._globalSpectreData.MaxSpectres = XmlHelper.GetData<int>(rootNode7, "SpectreMaxSpectres");
				for (int index = 0; index < 3; ++index)
					this._globalSpectreData.Damage[index] = new CombatAIDamageData();
				this._globalSpectreData.Damage[0].SetDataFromElement(rootNode7["SmallSpectreDamage"]);
				this._globalSpectreData.Damage[1].SetDataFromElement(rootNode7["MediumSpectreDamage"]);
				this._globalSpectreData.Damage[2].SetDataFromElement(rootNode7["LargeSpectreDamage"]);
			}
			XmlElement rootNode8 = rootNode1["AsteroidMonitorData"];
			this._globalAsteroidMonitorData = new AsteroidMonitorGlobalData();
			if (rootNode8 != null)
				this._globalAsteroidMonitorData.NumMonitors = XmlHelper.GetData<int>(rootNode8, "AsteroidMonitorNumMonitors");
			this._globalMorrigiRelicData = new MorrigiRelicGlobalData();
			XmlElement rootNode9 = rootNode1["MorrigiRelicData"];
			if (rootNode9 != null)
			{
				this._globalMorrigiRelicData.NumFighters = XmlHelper.GetData<int>(rootNode9, "NumCrowFighters");
				this._globalMorrigiRelicData.NumTombs = XmlHelper.GetData<int>(rootNode9, "NumTombs");
				for (int index = 0; index < 2; ++index)
					this._globalMorrigiRelicData.ResearchBonus[index] = new ResearchBonusData();
				this._globalMorrigiRelicData.ResearchBonus[0].SetDataFromElement(rootNode9["CapturedResearchBonus"]);
				this._globalMorrigiRelicData.ResearchBonus[1].SetDataFromElement(rootNode9["DestroyedResearchBonus"]);
				XmlElement rootNode10 = rootNode9["Rewards"];
				if (rootNode10 != null)
				{
					for (int index = 0; index < 10; ++index)
						this._globalMorrigiRelicData.Rewards[index] = XmlHelper.GetData<int>(rootNode10, ((MorrigiRelicGlobalData.RelicType)index).ToString());
				}
			}
			XmlElement rootNode11 = rootNode1["GardenerData"];
			if (rootNode11 != null)
			{
				this._globalGardenerData = new GardenerGlobalData();
				this._globalGardenerData.MinPlanets = XmlHelper.GetData<int>(rootNode11, "ProteanMinPlanets");
				this._globalGardenerData.MinBiosphere = XmlHelper.GetData<int>(rootNode11, "ProteanMinBiosphere");
				this._globalGardenerData.MaxBiosphere = XmlHelper.GetData<int>(rootNode11, "ProteanMaxBiosphere");
				this._globalGardenerData.CatchUpDelay = XmlHelper.GetData<int>(rootNode11, "ProteanCatchUpDelay");
				this._globalGardenerData.ProteanMobMin = XmlHelper.GetData<int>(rootNode11, "ProteanMobMin");
				this._globalGardenerData.ProteanMobMax = XmlHelper.GetData<int>(rootNode11, "ProteanMobMax");
				this._globalGardenerData.Terrforming = XmlHelper.GetData<float>(rootNode11, "Terraform");
				this._globalGardenerData.BiosphereDamage = XmlHelper.GetData<float>(rootNode11, "BiosphereDamage");
			}
			XmlElement rootNode12 = rootNode1["VonNeumannData"];
			this._globalVonNeumannData = new VonNeumannGlobalData();
			if (rootNode12 != null)
			{
				this._globalVonNeumannData.StartingResources = XmlHelper.GetData<int>(rootNode12, "VonNeumannStartingResources");
				this._globalVonNeumannData.BuildRate = XmlHelper.GetData<int>(rootNode12, "VonNeumannBuildRate");
				this._globalVonNeumannData.SalvageCapacity = XmlHelper.GetData<int>(rootNode12, "VonNeumannSalvageCapacity");
				this._globalVonNeumannData.SalvageCycle = XmlHelper.GetData<int>(rootNode12, "VonNeumannSalvageCycle");
				this._globalVonNeumannData.TargetCycle = XmlHelper.GetData<int>(rootNode12, "VonNeumannTargetCycle");
				this._globalVonNeumannData.MomRUCost = XmlHelper.GetData<int>(rootNode12, "MomRUCost");
				this._globalVonNeumannData.BerserkerRUCost = XmlHelper.GetData<int>(rootNode12, "BerserkerRUCost");
				this._globalVonNeumannData.ChildRUCost = XmlHelper.GetData<int>(rootNode12, "ChildRUCost");
				this._globalVonNeumannData.ChildRUCarryCap = XmlHelper.GetData<int>(rootNode12, "ChildRUCarryCap");
				this._globalVonNeumannData.MinChildrenToMaintain = XmlHelper.GetData<int>(rootNode12, "MinChildrenToMaintain");
				this._globalVonNeumannData.NumSatelitesPerChild = XmlHelper.GetData<int>(rootNode12, "NumSatelitesPerChild");
				this._globalVonNeumannData.NumShipsPerChild = XmlHelper.GetData<int>(rootNode12, "NumShipsPerChild");
				this._globalVonNeumannData.ChildIntegrationTime = XmlHelper.GetData<float>(rootNode12, "ChildIntegrationTime");
				this._globalVonNeumannData.RUTransferRateShip = XmlHelper.GetData<float>(rootNode12, "RUTransferRateShip");
				this._globalVonNeumannData.RUTransferRatePlanet = XmlHelper.GetData<float>(rootNode12, "RUTransferRatePlanet");
			}
			XmlElement rootNode13 = rootNode1["Locust"];
			this._globalLocustData = new LocustGlobalData();
			if (rootNode13 != null)
			{
				this._globalLocustData.MaxDrones = XmlHelper.GetData<int>(rootNode13, "MaxDrones");
				this._globalLocustData.MaxCombatDrones = XmlHelper.GetData<int>(rootNode13, "MaxCombatDrones");
				this._globalLocustData.MaxMoonCombatDrones = XmlHelper.GetData<int>(rootNode13, "MaxMoonCombatDrones");
				this._globalLocustData.DroneCost = XmlHelper.GetData<int>(rootNode13, "DroneCost");
				this._globalLocustData.NumToLand = XmlHelper.GetData<int>(rootNode13, "NumToLand");
				this._globalLocustData.MinResourceSpawnAmount = XmlHelper.GetData<int>(rootNode13, "MinResourceSpawnAmount");
				this._globalLocustData.MaxSalvageRate = XmlHelper.GetData<int>(rootNode13, "MaxSalvageRate");
				this._globalLocustData.InitialLocustScouts = XmlHelper.GetData<int>(rootNode13, "InitialLocustScouts");
				this._globalLocustData.MinLocustScouts = XmlHelper.GetData<int>(rootNode13, "MinLocustScouts");
				this._globalLocustData.LocustScoutCost = XmlHelper.GetData<int>(rootNode13, "LocustScoutCost");
				this._globalLocustData.LocustMotherCost = XmlHelper.GetData<int>(rootNode13, "LocustMotherCost");
			}
			this._aiRebellionChance = XmlHelper.GetData<float>(rootNode1, "AIRebellionChance");
			this._aiRebellionColonyPercent = XmlHelper.GetData<float>(rootNode1, "AIRebellionColonyPercent");
			this._encounterMinStartOffset = XmlHelper.GetData<float>(rootNode1, "EncounterMinStartDistance");
			this._encounterMaxStartOffset = XmlHelper.GetData<float>(rootNode1, "EncounterMaxStartDistance");
			this._interceptThreshold = XmlHelper.GetData<float>(rootNode1, "InterceptThreshold");
			this._globalPiracyData = new PiracyGlobalData();
			XmlElement rootNode14 = rootNode1["PiracyBounties"];
			if (rootNode14 != null)
			{
				this._globalPiracyData.PiracyBaseOdds = XmlHelper.GetData<float>(rootNode14, "PiracyBaseOdds");
				this._globalPiracyData.PiracyModPolice = XmlHelper.GetData<float>(rootNode14, "PiracyModPolice");
				this._globalPiracyData.PiracyModNavalBase = XmlHelper.GetData<float>(rootNode14, "PiracyModNavalBase");
				this._globalPiracyData.PiracyModNoNavalBase = XmlHelper.GetData<float>(rootNode14, "PiracyModNoNavalBase");
				this._globalPiracyData.PiracyModZuulProximity = XmlHelper.GetData<float>(rootNode14, "PiracyModZuulProximity");
				this._globalPiracyData.PiracyMinZuulProximity = XmlHelper.GetData<float>(rootNode14, "PiracyMinZuulProximity");
				this._globalPiracyData.PiracyMinShips = XmlHelper.GetData<int>(rootNode14, "PiracyMinShips");
				this._globalPiracyData.PiracyMaxShips = XmlHelper.GetData<int>(rootNode14, "PiracyMaxShips");
				this._globalPiracyData.PiracyBaseMod = XmlHelper.GetData<float>(rootNode14, "PiracyBaseMod");
				this._globalPiracyData.PiracyMinBaseShips = XmlHelper.GetData<int>(rootNode14, "PiracyMinBaseShips");
				this._globalPiracyData.PiracyTotalMaxShips = XmlHelper.GetData<int>(rootNode14, "PiracyTotalMaxShips");
				this._globalPiracyData.PiracyBaseRange = XmlHelper.GetData<int>(rootNode14, "PiracyBaseRange");
				this._globalPiracyData.PiracyBaseShipBonus = XmlHelper.GetData<int>(rootNode14, "PiracyBaseShipBonus");
				this._globalPiracyData.PiracyBaseTurnShipBonus = XmlHelper.GetData<int>(rootNode14, "PiracyBaseTurnShipBonus");
				this._globalPiracyData.PiracyBaseTurnsPerUpdate = XmlHelper.GetData<int>(rootNode14, "PiracyBaseTurnsPerUpdate");
				this._globalPiracyData.Bounties[1] = XmlHelper.GetData<int>(rootNode14, "PirateShipDestroyed");
				this._globalPiracyData.Bounties[2] = XmlHelper.GetData<int>(rootNode14, "FreighterDestroyed");
				this._globalPiracyData.Bounties[3] = XmlHelper.GetData<int>(rootNode14, "FreighterCaptured");
				this._globalPiracyData.Bounties[0] = XmlHelper.GetData<int>(rootNode14, "PirateBaseDestroyed");
				XmlElement source = rootNode14["RelationBonusFromBase"];
				if (source != null)
				{
					foreach (XmlElement xmlElement2 in source.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Bonus")))
						this._globalPiracyData.ReactionBonuses.Add(xmlElement2.GetAttribute("faction"), int.Parse(xmlElement2.GetAttribute("value")));
				}
			}
			XmlElement source1 = rootNode1["BaseEmpireColors"];
			if (source1 != null)
			{
				List<Vector3> vector3List = new List<Vector3>();
				foreach (XmlElement xmlElement2 in source1.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Color")))
					vector3List.Add(Vector3.Parse(xmlElement2.GetAttribute("value")));
				if (vector3List.Count == 10)
				{
					Player.DefaultPrimaryPlayerColors.Clear();
					Player.DefaultPrimaryPlayerColors = vector3List;
				}
			}
			this._tradePointPerFreighterFleet = XmlHelper.GetData<float>(rootNode1, "TradePointPerFreightFleet");
			this._populationPerTradePoint = XmlHelper.GetData<float>(rootNode1, "PopulationPerTradePoint");
			this._incomePerInternationalTradePointMoved = XmlHelper.GetData<float>(rootNode1, "IncomePerInternationalTradePointMoved");
			this._incomePerProvincialTradePointMoved = XmlHelper.GetData<float>(rootNode1, "IncomePerProvincialTradePointMoved");
			this._incomePerGenericTradePointMoved = XmlHelper.GetData<float>(rootNode1, "IncomePerGenericTradePointMoved");
			this._taxDivider = XmlHelper.GetData<float>(rootNode1, "TaxDivider");
			this._maxDebtMultiplier = XmlHelper.GetData<float>(rootNode1, "MaxDebtMultiplier");
			this._bankruptcyTurns = XmlHelper.GetData<int>(rootNode1, "BankruptcyTurns");
			this._provinceTradeModifier = XmlHelper.GetData<float>(rootNode1, "ProvinceTradeModifier");
			this._empireTradeModifier = XmlHelper.GetData<float>(rootNode1, "EmpireTradeModifier");
			this._citizensPerImmigrationPoint = XmlHelper.GetData<float>(rootNode1, "CitizensPerImmigrationPoint");
			this._moralBonusPerPoliceShip = XmlHelper.GetData<int>(rootNode1, "MoralBonusPerPoliceShip");
			this._populationPerPlanetBeam = XmlHelper.GetData<float>(rootNode1, "PopulationPerPlanetBeam");
			this._populationPerPlanetMirv = XmlHelper.GetData<float>(rootNode1, "PopulationPerPlanetMirv");
			this._populationPerPlanetMissile = XmlHelper.GetData<float>(rootNode1, "PopulationPerPlanetMissile");
			this._populationPerHeavyPlanetMissile = XmlHelper.GetData<float>(rootNode1, "PopulationPerHeavyPlanetMissile");
			this._planetMissileLaunchDelay = XmlHelper.GetData<float>(rootNode1, "PlanetMissileLaunchDelay");
			this._planetBeamLaunchDelay = XmlHelper.GetData<float>(rootNode1, "PlanetBeamLaunchDelay");
			this._forgeWorldImpMaxBonus = XmlHelper.GetData<float>(rootNode1, "ForgeWorldImpMaxBonus");
			this._forgeWorldIOBonus = XmlHelper.GetData<float>(rootNode1, "ForgeWorldIOBonus");
			this._gemWorldCivMaxBonus = XmlHelper.GetData<float>(rootNode1, "GemWorldCivMaxBonus");
			this._superWorldSizeConstraint = XmlHelper.GetData<int>(rootNode1, "SuperWorldSizeConstraint");
			this._superWorldModifier = XmlHelper.GetData<float>(rootNode1, "SuperWorldModifier");
			this._maxOverharvestRate = XmlHelper.GetData<float>(rootNode1, "MaxOverharvestRate");
			this._randomEncOddsPerOrbital = XmlHelper.GetData<float>(rootNode1, "RandomEncOddsPerOrbital");
			this._securityPointCost = XmlHelper.GetData<int>(rootNode1, "SecurityPointCost");
			this._requiredIntelPointsForMission = XmlHelper.GetData<int>(rootNode1, "RequiredIntelPointsForMission");
			this._requiredCounterIntelPointsForMission = XmlHelper.GetData<int>(rootNode1, "RequiredCounterIntelPointsForMission");
			this._colonyFleetSupportPoints = XmlHelper.GetData<int>(rootNode1, "ColonyFleetSupportPoints");
			this._stationLvl1FleetSupportPoints = XmlHelper.GetData<int>(rootNode1, "StationLvl1FleetSupportPoints");
			this._stationLvl2FleetSupportPoints = XmlHelper.GetData<int>(rootNode1, "StationLvl2FleetSupportPoints");
			this._stationLvl3FleetSupportPoints = XmlHelper.GetData<int>(rootNode1, "StationLvl3FleetSupportPoints");
			this._stationLvl4FleetSupportPoints = XmlHelper.GetData<int>(rootNode1, "StationLvl4FleetSupportPoints");
			this._stationLvl5FleetSupportPoints = XmlHelper.GetData<int>(rootNode1, "StationLvl5FleetSupportPoints");
			this._minSlaveDeathRate = XmlHelper.GetData<float>(rootNode1, "MinSlaveDeathRate");
			this._maxSlaveDeathRate = XmlHelper.GetData<float>(rootNode1, "MaxSlaveDeathRate");
			this._imperialProductionMultiplier = XmlHelper.GetData<float>(rootNode1, "ImperialProductionMultiplier");
			this._slaveProductionMultiplier = XmlHelper.GetData<float>(rootNode1, "SlaveProductionMultiplier");
			this._civilianProductionMultiplier = XmlHelper.GetData<float>(rootNode1, "CivilianProductionMultiplier");
			this._miningStationIOBonus = XmlHelper.GetData<int>(rootNode1, "MiningStationIOBonus");
			this._flockMaxBonus = XmlHelper.GetData<float>(rootNode1, "FlockMaxBonus");
			this._flockBRBonus = XmlHelper.GetData<float>(rootNode1, "FlockBRBonus");
			this._flockCRBonus = XmlHelper.GetData<float>(rootNode1, "FlockCRBonus");
			this._flockDNBonus = XmlHelper.GetData<float>(rootNode1, "FlockDNBonus");
			this._flockLVBonus = XmlHelper.GetData<float>(rootNode1, "FlockLVBonus");
			this._flockBRCountBonus = XmlHelper.GetData<int>(rootNode1, "FlockBRCountBonus");
			this._flockCRCountBonus = XmlHelper.GetData<int>(rootNode1, "FlockCRCountBonus");
			this._flockDNCountBonus = XmlHelper.GetData<int>(rootNode1, "FlockDNCountBonus");
			this._flockLVCountBonus = XmlHelper.GetData<int>(rootNode1, "FlockLVCountBonus");
			this._declareWarPointCost = XmlHelper.GetData<int>(rootNode1, "DeclareWarPointCost");
			this._requestResearchPointCost = XmlHelper.GetData<int>(rootNode1, "RequestResearchPointCost");
			this._requestMilitaryAssistancePointCost = XmlHelper.GetData<int>(rootNode1, "RequestMilitaryAssistancePointCost");
			this._requestGatePointCost = XmlHelper.GetData<int>(rootNode1, "RequestGatePointCost");
			this._requestEnclavePointCost = XmlHelper.GetData<int>(rootNode1, "RequestEnclavePointCost");
			this._requestSystemInfoPointCost = XmlHelper.GetData<int>(rootNode1, "RequestSystemInfoPointCost");
			this._requestSavingsPointCost = XmlHelper.GetData<int>(rootNode1, "RequestSavingsPointCost");
			this._demandSavingsPointCost = XmlHelper.GetData<int>(rootNode1, "DemandSavingsPointCost");
			this._demandResearchPointCost = XmlHelper.GetData<int>(rootNode1, "DemandResearchPointCost");
			this._demandSystemInfoPointCost = XmlHelper.GetData<int>(rootNode1, "DemandSystemInfoPointCost");
			this._demandSlavesPointCost = XmlHelper.GetData<int>(rootNode1, "DemandSlavesPointCost");
			this._demandSystemPointCost = XmlHelper.GetData<int>(rootNode1, "DemandSystemPointCost");
			this._demandProvincePointCost = XmlHelper.GetData<int>(rootNode1, "DemandProvincePointCost");
			this._demandEmpirePointCost = XmlHelper.GetData<int>(rootNode1, "DemandEmpirePointCost");
			this._treatyArmisticeWarNeutralPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyArmisticeWarNeutralPointCost");
			this._treatyArmisticeNeutralCeasefirePointCost = XmlHelper.GetData<int>(rootNode1, "TreatyArmisticeNeutralCeasefirePointCost");
			this._treatyArmisticeNeutralNonAggroPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyArmisticeNeutralNonAggroPointCost");
			this._treatyArmisticeCeaseFireNonAggroPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyArmisticeCeaseFireNonAggroPointCost");
			this._treatyArmisticeCeaseFirePeacePointCost = XmlHelper.GetData<int>(rootNode1, "TreatyArmisticeCeaseFirePeacePointCost");
			this._treatyArmisticeNonAggroPeaceCost = XmlHelper.GetData<int>(rootNode1, "TreatyArmisticeNonAggroPeaceCost");
			this._treatyArmisticePeaceAlliancePointCost = XmlHelper.GetData<int>(rootNode1, "TreatyArmisticePeaceAlliancePointCost");
			this._treatyTradePointCost = XmlHelper.GetData<int>(rootNode1, "TreatyTradePointCost");
			this._treatyIncorporatePointCost = XmlHelper.GetData<int>(rootNode1, "TreatyIncorporatePointCost");
			this._treatyProtectoratePointCost = XmlHelper.GetData<int>(rootNode1, "TreatyProtectoratePointCost");
			this._treatyLimitationShipClassPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationShipClassPointCost");
			this._treatyLimitationFleetsPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationFleetsPointCost");
			this._treatyLimitationWeaponsPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationWeaponsPointCost");
			this._treatyLimitationResearchTechPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationResearchTechPointCost");
			this._treatyLimitationResearchTreePointCost = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationResearchTreePointCost");
			this._treatyLimitationColoniesPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationColoniesPointCost");
			this._treatyLimitationForgeGemWorldsPointCost = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationForgeGemWorldsPointCost");
			this._treatyLimitationStationType = XmlHelper.GetData<int>(rootNode1, "TreatyLimitationStationType");
			this._stimulusColonizationBonus = XmlHelper.GetData<int>(rootNode1, "StimulusColonizationBonus");
			this._stimulusMiningMin = XmlHelper.GetData<int>(rootNode1, "StimulusMiningMin");
			this._stimulusMiningMax = XmlHelper.GetData<int>(rootNode1, "StimulusMiningMax");
			this._stimulusColonizationMin = XmlHelper.GetData<int>(rootNode1, "StimulusColonizationMin");
			this._stimulusColonizationMax = XmlHelper.GetData<int>(rootNode1, "StimulusColonizationMax");
			this._stimulusTradeMin = XmlHelper.GetData<int>(rootNode1, "StimulusTradeMin");
			this._stimulusTradeMax = XmlHelper.GetData<int>(rootNode1, "StimulusTradeMax");
			this._StationsPerPopulation = XmlHelper.GetData<int>(rootNode1, "PopulationPerStation");
			this._maxLoaCubesOnBuild = XmlHelper.GetData<int>(rootNode1, "MaxLoaCubesOnBuild");
			this._minLoaCubesOnBuild = XmlHelper.GetData<int>(rootNode1, "MinLoaCubesOnBuild");
			this._LoaCostPerCube = XmlHelper.GetData<int>(rootNode1, "LoaCubeCostPer");
			this._LoaDistanceBetweenGates = XmlHelper.GetData<float>(rootNode1, "LoaDistanceBetweenGates");
			this._LoaBaseMaxMass = XmlHelper.GetData<int>(rootNode1, "LoaBaseMaxMass");
			this._LoaMassInductionProjectorsMaxMass = XmlHelper.GetData<int>(rootNode1, "LoaMassInductionProjectorsMaxMass");
			this._LoaMassStandingPulseWavesMaxMass = XmlHelper.GetData<int>(rootNode1, "LoaMassStandingPulseWavesMaxMass");
			this._LoaGateSystemMargin = XmlHelper.GetData<float>(rootNode1, "LoaGateSystemMargin");
			this._LoaTechModMod = XmlHelper.GetData<float>(rootNode1, "LoaTechModMod");
			this._HomeworldTaxBonusMod = XmlHelper.GetData<float>(rootNode1, "HomeworldTaxBonusMod");
			this._pieChartColourShipMaintenance = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourShipMaintenance"));
			this._pieChartColourPlanetaryDevelopment = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourPlanetaryDevelopment"));
			this._pieChartColourDebtInterest = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourDebtInterest"));
			this._pieChartColourResearch = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourResearch"));
			this._pieChartColourSecurity = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourSecurity"));
			this._pieChartColourStimulus = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourStimulus"));
			this._pieChartColourSavings = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourSavings"));
			this._pieChartColourCorruption = Vector3.Parse(XmlHelper.GetData<string>(rootNode1, "PieChartColourCorruption"));
			this._accumulatedKnowledgeWeaponPerBattleMin = XmlHelper.GetData<int>(rootNode1, "WeaponAccumulatedKnowledgePerBattleMin");
			this._accumulatedKnowledgeWeaponPerBattleMax = XmlHelper.GetData<int>(rootNode1, "WeaponAccumulatedKnowledgePerBattleMax");
			this._upkeepBattleRider = XmlHelper.GetData<int>(rootNode1, "UpkeepBattleRider");
			this._upkeepCruiser = XmlHelper.GetData<int>(rootNode1, "UpkeepCruiser");
			this._upkeepDreadnaught = XmlHelper.GetData<int>(rootNode1, "UpkeepDreadnaught");
			this._upkeepLeviathan = XmlHelper.GetData<int>(rootNode1, "UpkeepLeviathan");
			this._upkeepDefensePlatform = XmlHelper.GetData<int>(rootNode1, "UpkeepDefensePlatform");
			for (int index = 0; index < 5; ++index)
				this._upkeepScienceStation[index] = XmlHelper.GetData<int>(rootNode1, "UpkeepScience" + (object)(index + 1));
			for (int index = 0; index < 5; ++index)
				this._upkeepNavalStation[index] = XmlHelper.GetData<int>(rootNode1, "UpkeepNaval" + (object)(index + 1));
			for (int index = 0; index < 5; ++index)
				this._upkeepCivilianStation[index] = XmlHelper.GetData<int>(rootNode1, "UpkeepCivilian" + (object)(index + 1));
			for (int index = 0; index < 5; ++index)
				this._upkeepDiplomaticStation[index] = XmlHelper.GetData<int>(rootNode1, "UpkeepDiplomatic" + (object)(index + 1));
			for (int index = 0; index < 5; ++index)
				this._upkeepGateStation[index] = XmlHelper.GetData<int>(rootNode1, "UpkeepGate" + (object)(index + 1));
			this._starSystemEntryPointRange = XmlHelper.GetData<float>(rootNode1, "StarSystemEntryPointRange");
			this._tacStealthArmorBonus = XmlHelper.GetData<float>(rootNode1, "TacStealthArmorSignature");
			this._slewModeMultiplier = XmlHelper.GetData<float>(rootNode1, "SlewModeMultiplier");
			this._slewModeDecelMultiplier = XmlHelper.GetData<float>(rootNode1, "SlewModeDecelMultiplier");
			this._slewModeExitRange = XmlHelper.GetData<float>(rootNode1, "SlewModeExitRange");
			this._slewModeEnterOffset = XmlHelper.GetData<float>(rootNode1, "SlewModeEnterOffset");
			this._globalSpotterRanges = new GlobalSpotterRangeData();
			XmlElement xmlElement3 = rootNode1["SpotterRangeTable"];
			if (xmlElement3 != null)
			{
				this._globalSpotterRanges.SpotterValues[0] = float.Parse(xmlElement3.GetAttribute("br"));
				this._globalSpotterRanges.SpotterValues[1] = float.Parse(xmlElement3.GetAttribute("cr"));
				this._globalSpotterRanges.SpotterValues[2] = float.Parse(xmlElement3.GetAttribute("dn"));
				this._globalSpotterRanges.SpotterValues[3] = float.Parse(xmlElement3.GetAttribute("lv"));
				this._globalSpotterRanges.SpotterValues[4] = float.Parse(xmlElement3.GetAttribute("sn"));
				this._globalSpotterRanges.StationLVLOffset = float.Parse(xmlElement3.GetAttribute("snlvloffset"));
			}
			XmlElement rootNode15 = rootNode1["MineField"];
			if (rootNode15 != null)
			{
				this._mineFieldParams.Width = XmlHelper.GetData<float>(rootNode15, "Width");
				this._mineFieldParams.Length = XmlHelper.GetData<float>(rootNode15, "Length");
				this._mineFieldParams.Height = XmlHelper.GetData<float>(rootNode15, "Height");
				this._mineFieldParams.SpacingOffset = XmlHelper.GetData<float>(rootNode15, "SpacingOffset");
			}
			else
			{
				this._mineFieldParams.Width = 1000f;
				this._mineFieldParams.Length = 1000f;
				this._mineFieldParams.Height = 0.0f;
				this._mineFieldParams.SpacingOffset = 100f;
			}
			XmlElement rootNode16 = rootNode1["SpecialProjectData"];
			if (rootNode16 != null)
			{
				this._specialProjectData.MinimumIndyInvestigate = XmlHelper.GetData<int>(rootNode16, "MinimumIndyInvestigate");
				this._specialProjectData.MaximumIndyInvestigate = XmlHelper.GetData<int>(rootNode16, "MaximumIndyInvestigate");
				this._specialProjectData.MinimumAsteroidMonitorStudy = XmlHelper.GetData<int>(rootNode16, "MinimumAsteroidMonitorStudy");
				this._specialProjectData.MaximumAsteroidMonitorStudy = XmlHelper.GetData<int>(rootNode16, "MaximumAsteroidMonitorStudy");
				this._specialProjectData.MinimumRadiationShieldingStudy = XmlHelper.GetData<int>(rootNode16, "MinimumRadiationShieldingStudy");
				this._specialProjectData.MaximumRadiationShieldingStudy = XmlHelper.GetData<int>(rootNode16, "MaximumRadiationShieldingStudy");
				this._specialProjectData.MinimumNeutronStarStudy = XmlHelper.GetData<int>(rootNode16, "MaximumNeutronStarStudy");
				this._specialProjectData.MaximumNeutronStarStudy = XmlHelper.GetData<int>(rootNode16, "MaximumNeutronStarStudy");
				this._specialProjectData.MinimumGardenerStudy = XmlHelper.GetData<int>(rootNode16, "MaximumGardenerStudy");
				this._specialProjectData.MaximumGardenerStudy = XmlHelper.GetData<int>(rootNode16, "MaximumGardenerStudy");
			}
			else
			{
				this._specialProjectData.MinimumIndyInvestigate = 40000;
				this._specialProjectData.MaximumIndyInvestigate = 40000;
				this._specialProjectData.MinimumAsteroidMonitorStudy = 40000;
				this._specialProjectData.MaximumAsteroidMonitorStudy = 40000;
			}
			XmlElement rootNode17 = rootNode1["DefenseManagerSettings"];
			if (rootNode16 != null)
			{
				this._defenseManagerSettings.SDBCPCost = XmlHelper.GetData<int>(rootNode17, "SDBCPCost");
				this._defenseManagerSettings.MineLayerCPCost = XmlHelper.GetData<int>(rootNode17, "MineLayerCPCost");
				this._defenseManagerSettings.PoliceCPCost = XmlHelper.GetData<int>(rootNode17, "PoliceCPCost");
				this._defenseManagerSettings.ScanSatCPCost = XmlHelper.GetData<int>(rootNode17, "ScanSatCPCost");
				this._defenseManagerSettings.DroneSatCPCost = XmlHelper.GetData<int>(rootNode17, "DroneSatCPCost");
				this._defenseManagerSettings.TorpSatCPCost = XmlHelper.GetData<int>(rootNode17, "TorpSatCPCost");
				this._defenseManagerSettings.BRSatCPCost = XmlHelper.GetData<int>(rootNode17, "BRSatCPCost");
				this._defenseManagerSettings.MonitorSatCPCost = XmlHelper.GetData<int>(rootNode17, "MonitorSatCPCost");
				this._defenseManagerSettings.MissileSatCPCost = XmlHelper.GetData<int>(rootNode17, "MissileSatCPCost");
			}
			else
			{
				this._defenseManagerSettings.SDBCPCost = 0;
				this._defenseManagerSettings.MineLayerCPCost = 0;
				this._defenseManagerSettings.PoliceCPCost = 0;
				this._defenseManagerSettings.ScanSatCPCost = 0;
				this._defenseManagerSettings.DroneSatCPCost = 0;
				this._defenseManagerSettings.TorpSatCPCost = 0;
				this._defenseManagerSettings.BRSatCPCost = 0;
				this._defenseManagerSettings.MonitorSatCPCost = 0;
				this._defenseManagerSettings.MissileSatCPCost = 0;
			}
			XmlElement source2 = rootNode1["MiniShips"];
			if (source2 != null)
			{
				int num = 0;
				foreach (XmlElement xmlElement2 in source2.OfType<XmlElement>())
				{
					this._miniShipMap.Add(xmlElement2.GetAttribute("type"), new AssetDatabase.MiniMapData()
					{
						ID = num,
						Location = xmlElement2.GetAttribute("location")
					});
					++num;
				}
			}
			XmlElement source3 = rootNode1["CritHitPercentages"];
			if (source3 == null)
				return;
			for (int index = 0; index < 8; ++index)
				this._critHitChances[index].Chances = new int[25];
			foreach (XmlElement xmlElement2 in source3.OfType<XmlElement>())
			{
				int index = (int)Enum.Parse(typeof(CritHitType), xmlElement2.GetAttribute("type"));
				this._critHitChances[0].Chances[index] = int.Parse(xmlElement2.GetAttribute("cmd"));
				this._critHitChances[1].Chances[index] = int.Parse(xmlElement2.GetAttribute("mis"));
				this._critHitChances[2].Chances[index] = int.Parse(xmlElement2.GetAttribute("eng"));
				this._critHitChances[3].Chances[index] = int.Parse(xmlElement2.GetAttribute("sn"));
				this._critHitChances[4].Chances[index] = int.Parse(xmlElement2.GetAttribute("monster"));
				this._critHitChances[5].Chances[index] = int.Parse(xmlElement2.GetAttribute("loa_cmd"));
				this._critHitChances[6].Chances[index] = int.Parse(xmlElement2.GetAttribute("loa_mis"));
				this._critHitChances[7].Chances[index] = int.Parse(xmlElement2.GetAttribute("loa_eng"));
			}
		}

		private void LoadRandomEncounterOdds(XmlDocument d)
		{
			foreach (XmlElement childNode in d["CommonAssets"]["RandomEncounterOdds"].ChildNodes)
			{
				RandomEncounter result;
				if (Enum.TryParse<RandomEncounter>(childNode.Name, out result))
					this._randomEncounterOdds.Add(result, int.Parse(childNode.InnerText));
			}
		}

		private void LoadEasterEggOdds(XmlDocument d)
		{
			foreach (XmlElement childNode in d["CommonAssets"]["EasterEggOdds"].ChildNodes)
			{
				EasterEgg result;
				if (Enum.TryParse<EasterEgg>(childNode.Name, out result))
					this._easterEggOdds.Add(result, int.Parse(childNode.InnerText));
			}
		}

		private void LoadGMOdds(XmlDocument d)
		{
			foreach (XmlElement childNode in d["CommonAssets"]["GMOdds"].ChildNodes)
			{
				EasterEgg result;
				if (Enum.TryParse<EasterEgg>(childNode.Name, out result))
					this._gmOdds.Add(result, int.Parse(childNode.InnerText));
			}
		}

		private static XmlDocument LoadFile(string file)
		{
			string[] files = ScriptHost.FileSystem.FindFiles(file);
			XmlDocument document1 = new XmlDocument();
			document1.Load(ScriptHost.FileSystem, files[0]);
			for (int index = 1; index < files.Length; ++index)
			{
				XmlDocument document2 = new XmlDocument();
				document2.Load(ScriptHost.FileSystem, files[index]);
				foreach (XmlNode node in document2.DocumentElement.ChildNodes.OfType<XmlElement>().Cast<XmlElement>())
					document1.DocumentElement.AppendChild(document1.ImportNode(node, true));
			}
			return document1;
		}

		private void LoadFleetTemplates()
		{
			if (this._fleetTemplates != null)
				return;
			this._fleetTemplates = new List<FleetTemplate>();
			Stream stream = ScriptHost.FileSystem.CreateStream("factions\\fleet_templates.xml");
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(stream);
			int num = 1;
			foreach (XmlElement xmlElement1 in xmlDocument["FleetTemplates"].OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "FleetTemplate")))
			{
				FleetTemplate fleetTemplate = new FleetTemplate();
				fleetTemplate.TemplateID = num;
				++num;
				fleetTemplate.Name = xmlElement1.GetAttribute("name");
				fleetTemplate.FleetName = xmlElement1.GetAttribute("fleetName") ?? "Default Fleet";
				fleetTemplate.Initial = xmlElement1.HasAttribute("initial") && bool.Parse(xmlElement1.GetAttribute("initial"));
				foreach (XmlElement xmlElement2 in xmlElement1.ChildNodes.OfType<XmlElement>())
				{
					if (xmlElement2.Name == "StanceWeight")
						fleetTemplate.StanceWeights[(AIStance)Enum.Parse(typeof(AIStance), xmlElement2.GetAttribute("stance"))] = int.Parse(xmlElement2.InnerText);
					else if (xmlElement2.Name == "Ship")
					{
						ShipInclude shipInclude = new ShipInclude()
						{
							InclusionType = (ShipInclusionType)Enum.Parse(typeof(ShipInclusionType), xmlElement2.GetAttribute("inclusion"))
						};
						shipInclude.Amount = xmlElement2.HasAttribute("amount") ? int.Parse(xmlElement2.GetAttribute("amount")) : (shipInclude.InclusionType == ShipInclusionType.FILL ? 0 : 1);
						shipInclude.WeaponRole = xmlElement2.HasAttribute("weaponRole") ? new WeaponRole?((WeaponRole)Enum.Parse(typeof(WeaponRole), xmlElement2.GetAttribute("weaponRole"))) : new WeaponRole?();
						shipInclude.Faction = xmlElement2.HasAttribute("faction") ? xmlElement2.GetAttribute("faction") : string.Empty;
						shipInclude.FactionExclusion = xmlElement2.HasAttribute("nfaction") ? xmlElement2.GetAttribute("nfaction") : string.Empty;
						shipInclude.ShipRole = (ShipRole)Enum.Parse(typeof(ShipRole), xmlElement2.InnerText);
						fleetTemplate.ShipIncludes.Add(shipInclude);
					}
					else if (xmlElement2.Name == "MissionType")
					{
						MissionType missionType = (MissionType)Enum.Parse(typeof(MissionType), xmlElement2.InnerText);
						fleetTemplate.MissionTypes.Add(missionType);
					}
					else if (xmlElement2.Name == "MinToMaintain")
					{
						foreach (XmlElement xmlElement3 in xmlElement2.ChildNodes.OfType<XmlElement>())
						{
							if (xmlElement3.Name == "Amount")
								fleetTemplate.MinFleetsForStance[(AIStance)Enum.Parse(typeof(AIStance), xmlElement3.GetAttribute("stance"))] = int.Parse(xmlElement3.InnerText);
						}
					}
					else if (xmlElement2.Name == "AllowableFactions")
					{
						foreach (XmlElement xmlElement3 in xmlElement2.ChildNodes.OfType<XmlElement>())
						{
							if (xmlElement3.Name == "Faction")
								fleetTemplate.AllowableFactions.Add(xmlElement3.InnerText);
						}
					}
				}
				this._fleetTemplates.Add(fleetTemplate);
			}
			stream.Dispose();
		}

		private static IEnumerable<string> EnumerateCommonMaterialDictionaries(XmlDocument d)
		{
			foreach (string str in d["CommonAssets"].OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("MaterialDictionary", StringComparison.InvariantCulture))).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.InnerText)))
				yield return str;
		}

		private static IEnumerable<string> EnumerateSplashScreenImageNames(XmlElement node)
		{
			if (node != null)
			{
				foreach (XmlElement xmlElement in node.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(element => element.Name == "SplashScreen")))
				{
					string imageName = xmlElement.GetAttribute("image");
					if (!string.IsNullOrEmpty(imageName))
						yield return imageName;
				}
			}
		}

		private static string[] LoadSplashScreenImageNames(
		  XmlDocument commonAssetsDoc,
		  IEnumerable<Faction> factions)
		{
			List<string> stringList = new List<string>();
			stringList.AddRange(AssetDatabase.EnumerateSplashScreenImageNames(commonAssetsDoc["CommonAssets"]["SplashScreens"]));
			foreach (Faction faction in factions)
			{
				XmlDocument xmlDocument = Faction.LoadMergedXMLDocument(faction.FactionFileName);
				stringList.AddRange(AssetDatabase.EnumerateSplashScreenImageNames(xmlDocument["Faction"]["SplashScreens"]));
			}
			return stringList.ToArray();
		}

		public string GetRandomSplashScreenImageName()
		{
			return this._splashScreenImageNames[this._random.Next(this._splashScreenImageNames.Length)];
		}

		public static List<string> LoadTechTreeModels(XmlElement element)
		{
			List<string> stringList = new List<string>();
			if (element != null)
				stringList.AddRange(element.ChildNodes.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Model")).Select<XmlElement, string>((Func<XmlElement, string>)(y => y.InnerText)));
			return stringList;
		}

		public static List<string> LoadTechTreeRoots(XmlElement element)
		{
			List<string> stringList = new List<string>();
			if (element != null)
				stringList.AddRange(element.ChildNodes.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "RootNode")).Select<XmlElement, string>((Func<XmlElement, string>)(y => y.InnerText)));
			return stringList;
		}

		private static List<string> LoadTechTreeModelsFromCommonAssetsXml(
		  XmlDocument commonAssetsDoc)
		{
			return AssetDatabase.LoadTechTreeModels(commonAssetsDoc["CommonAssets"]["TechTree"]);
		}

		private static List<string> LoadTechTreeRootsFromCommonAssetsXml(XmlDocument commonAssetsDoc)
		{
			return AssetDatabase.LoadTechTreeRoots(commonAssetsDoc["CommonAssets"]["TechTree"]);
		}

		public AIResearchFramework AIResearchFramework { get; private set; }

		public AssetDatabase(App app)
		{
			XmlDocument xmlDocument = AssetDatabase.LoadFile("commonassets.xml");
			this._cachedGlobals = new Dictionary<string, object>();
			this.InitializeAIDifficultyBonuses();
			this.LoadFleetTemplates();
			this.LoadGlobalsAndStratModifiers(xmlDocument);
			this.LoadMoralEventModifiers(AssetDatabase.LoadFile("moralmodifiers.xml"));
			this.LoadDefaultStratModifiers(AssetDatabase.LoadFile("defaultstratmodifiers.xml"));
			this.LoadTechBonusValues(AssetDatabase.LoadFile("techbonuses.xml"));
			this.LoadGovernmentActionModifiers(AssetDatabase.LoadFile("govactionmodifiers.xml"));
			this.LoadRandomEncounterOdds(xmlDocument);
			this.LoadEasterEggOdds(xmlDocument);
			this.LoadGMOdds(xmlDocument);
			this._governmentEffects.LoadFromFile(AssetDatabase.LoadFile("goveffects.xml"));
			this._commonMaterialDictionaries = AssetDatabase.EnumerateCommonMaterialDictionaries(xmlDocument).ToArray<string>();
			this._turretHousings = TurretHousingLibrary.Enumerate().ToArray<LogicalTurretHousing>();
			this._weapons = WeaponLibrary.Enumerate(app).ToArray<LogicalWeapon>();
			this._modules = ModuleLibrary.Enumerate().ToArray<LogicalModule>();
			this._modulesToAssignByDefault = ((IEnumerable<LogicalModule>)this._modules).Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.AssignByDefault)).ToArray<LogicalModule>();
			this._psionics = PsionicLibrary.Enumerate(xmlDocument).ToArray<LogicalPsionic>();
			this._suulkaPsiBonuses = SuulkaPsiBonusLibrary.Enumerate(xmlDocument).ToArray<SuulkaPsiBonus>();
			this._shields = ShieldLibrary.Enumerate(xmlDocument).ToArray<LogicalShield>();
			this._shipSparks = ShipSparksLibrary.Enumerate(xmlDocument).ToArray<LogicalShipSpark>();
			this._shipEMPEffect = new LogicalEffect();
			this._shipEMPEffect.Name = XmlHelper.GetData<string>(xmlDocument["CommonAssets"], "ShipEMPSpark");
			this._factions = FactionLibrary.Enumerate().ToArray<Faction>();
			this._races = RaceLibrary.Enumerate().ToArray<Race>();
			this._shipSections = new HashSet<ShipSectionAsset>(SectionLibrary.Enumerate(this));
			this._shipSectionsByFilename = new Dictionary<string, ShipSectionAsset>();
			foreach (ShipSectionAsset shipSection in this._shipSections)
				this._shipSectionsByFilename[shipSection.FileName] = shipSection;
			this._planetgenrules = new PlanetGraphicsRules();
			this._skyDefinitions = Kerberos.Sots.GameObjects.SkyDefinitions.LoadFromXml();
			if (AssetDatabase._commonStrings != null)
				throw new InvalidOperationException("CommonStrings already created.");
			AssetDatabase._commonStrings = new CommonStrings(ScriptHost.TwoLetterISOLanguageName);
			this.IntelMissions = new IntelMissionDescMap();
			this._masterTechTree = new TechTree();
			TechnologyXmlUtility.LoadTechTreeFromXml("Tech\\techtree.techtree", ref this._masterTechTree);
			this._masterTechTreeRoots = this._masterTechTree.GetRoots();
			this._techTreeModels = AssetDatabase.LoadTechTreeModelsFromCommonAssetsXml(xmlDocument);
			this._techTreeRoots = AssetDatabase.LoadTechTreeRootsFromCommonAssetsXml(xmlDocument);
			this._splashScreenImageNames = AssetDatabase.LoadSplashScreenImageNames(xmlDocument, (IEnumerable<Faction>)this._factions);
			this.AIResearchFramework = new AIResearchFramework();
			this.DiplomacyStateChangeMap = new Dictionary<DiplomacyStateChange, int>()
	  {
		{
		  new DiplomacyStateChange()
		  {
			lower = DiplomacyState.WAR,
			upper = DiplomacyState.NEUTRAL
		  },
		  this.TreatyArmisticeWarNeutralPointCost
		},
		{
		  new DiplomacyStateChange()
		  {
			lower = DiplomacyState.NEUTRAL,
			upper = DiplomacyState.CEASE_FIRE
		  },
		  this.TreatyArmisticeNeutralCeasefirePointCost
		},
		{
		  new DiplomacyStateChange()
		  {
			lower = DiplomacyState.NEUTRAL,
			upper = DiplomacyState.NON_AGGRESSION
		  },
		  this.TreatyArmisticeNeutralNonAggroPointCost
		},
		{
		  new DiplomacyStateChange()
		  {
			lower = DiplomacyState.CEASE_FIRE,
			upper = DiplomacyState.NON_AGGRESSION
		  },
		  this.TreatyArmisticeCeaseFireNonAggroPointCost
		},
		{
		  new DiplomacyStateChange()
		  {
			lower = DiplomacyState.CEASE_FIRE,
			upper = DiplomacyState.PEACE
		  },
		  this.TreatyArmisticeCeaseFirePeacePointCost
		},
		{
		  new DiplomacyStateChange()
		  {
			lower = DiplomacyState.NON_AGGRESSION,
			upper = DiplomacyState.PEACE
		  },
		  this.TreatyArmisticeNonAggroPeaceCost
		},
		{
		  new DiplomacyStateChange()
		  {
			lower = DiplomacyState.PEACE,
			upper = DiplomacyState.ALLIED
		  },
		  this.TreatyArmisticePeaceAllianceCost
		}
	  };
		}

		public string GetStationModuleAsset(ModuleEnums.StationModuleType type, string factionName)
		{
			return string.Format(AssetDatabase.StationModuleTypeAssetMap[type], (object)factionName);
		}

		public string GetRandomBadgeTexture(string faction, Random rng)
		{
			string[] badgeTexturePaths = this.Factions.First<Faction>((Func<Faction, bool>)(x => x.Name.Equals(faction, StringComparison.InvariantCulture))).BadgeTexturePaths;
			if (badgeTexturePaths == null || badgeTexturePaths.Length == 0)
				return string.Empty;
			return badgeTexturePaths[rng.Next(badgeTexturePaths.Length)];
		}

		public string GetRandomAvatarTexture(string faction, Random rng)
		{
			string[] avatarTexturePaths = this.Factions.First<Faction>((Func<Faction, bool>)(x => x.Name.Equals(faction, StringComparison.InvariantCulture))).AvatarTexturePaths;
			if (avatarTexturePaths == null || avatarTexturePaths.Length == 0)
				return string.Empty;
			return avatarTexturePaths[rng.Next(avatarTexturePaths.Length)];
		}

		public void Dispose()
		{
			foreach (LogicalWeapon weapon in this._weapons)
				weapon.Dispose();
		}

		public enum MoraleModifierType
		{
			AllColonies,
			Province,
			System,
			Colony,
		}

		public struct MoralModifier
		{
			public AssetDatabase.MoraleModifierType type;
			public int value;
		}

		public struct CritHitChances
		{
			public int[] Chances;

			public enum CritHitLocationTypes
			{
				CommandSection,
				MissionSection,
				EngineSection,
				Station,
				Monsters,
				LoaCmd,
				LoaMis,
				LoaEng,
				MaxLocations,
			}
		}

		public class MiniMapData
		{
			public int ID = -1;
			public string Location = "";
		}
	}
}
