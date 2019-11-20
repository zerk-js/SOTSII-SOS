// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.CreateShipParams
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;

namespace Kerberos.Sots.GameObjects
{
	internal class CreateShipParams
	{
		public IEnumerable<ShipSectionAsset> sections = (IEnumerable<ShipSectionAsset>)new ShipSectionAsset[0];
		public IEnumerable<SectionInstanceInfo> sectionInstances = (IEnumerable<SectionInstanceInfo>)new SectionInstanceInfo[0];
		public IEnumerable<LogicalTurretHousing> turretHousings = (IEnumerable<LogicalTurretHousing>)new LogicalTurretHousing[0];
		public IEnumerable<LogicalWeapon> weapons = (IEnumerable<LogicalWeapon>)new LogicalWeapon[0];
		public IEnumerable<LogicalWeapon> preferredWeapons = (IEnumerable<LogicalWeapon>)new LogicalWeapon[0];
		public IEnumerable<WeaponAssignment> assignedWeapons = (IEnumerable<WeaponAssignment>)new WeaponAssignment[0];
		public IEnumerable<LogicalModule> modules = (IEnumerable<LogicalModule>)new LogicalModule[0];
		public IEnumerable<LogicalModule> preferredModules = (IEnumerable<LogicalModule>)new LogicalModule[0];
		public IEnumerable<ModuleAssignment> assignedModules = (IEnumerable<ModuleAssignment>)new ModuleAssignment[0];
		public IEnumerable<LogicalPsionic> psionics = (IEnumerable<LogicalPsionic>)new LogicalPsionic[0];
		public List<Player> playersInCombat = new List<Player>();
		public AssignedSectionTechs[] assignedTechs = new AssignedSectionTechs[3];
		public Matrix? spawnMatrix = new Matrix?();
		public string shipName = "";
		public string shipDesignName = "";
		public string priorityWeapon = "";
		public bool isKillable = true;
		public bool enableAI = true;
		public bool addPsionics = true;
		public int riderindex = -1;
		public bool AutoAddDrawable = true;
		public Player player;
		public Faction faction;
		public int serialNumber;
		public int parentID;
		public int inputID;
		public ShipRole role;
		public WeaponRole wpnRole;
		public int databaseId;
		public int designId;
		public bool isInDeepSpace;
		public int parentDBID;
		public int curPsiPower;
		public bool defenceBoatIsActive;
		public int defenceBoatOrbitalID;
		public double obtainedSlaves;

		public CreateShipParams()
		{
			for (int index = 0; index <= 2; ++index)
				this.assignedTechs[index] = new AssignedSectionTechs();
		}
	}
}
