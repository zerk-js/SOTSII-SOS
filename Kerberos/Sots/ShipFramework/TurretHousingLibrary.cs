// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.TurretHousingLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.ShipFramework
{
	internal static class TurretHousingLibrary
	{
		public static IEnumerable<LogicalTurretHousing> Enumerate()
		{
			XmlDocument d = new XmlDocument();
			d.Load(ScriptHost.FileSystem, "weapons\\turrets.xml");
			foreach (XmlElement xmlElement in d["Turrets"].OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Turret")))
				yield return new LogicalTurretHousing()
				{
					MountSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), xmlElement.GetAttribute("MountSize")),
					WeaponSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), xmlElement.GetAttribute("WeaponSize")),
					Class = (WeaponEnums.TurretClasses)Enum.Parse(typeof(WeaponEnums.TurretClasses), xmlElement.GetAttribute("Class")),
					TrackSpeed = float.Parse(xmlElement.GetAttribute("TrackSpeed")),
					ModelName = xmlElement.GetAttribute("Model"),
					BaseModelName = xmlElement.GetAttribute("BaseModel")
				};
		}
	}
}
