// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.TurretClass
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public class TurretClass : IXmlLoadSave
	{
		public string ActualTurretClass = "";
		public List<TurretClassSize> TurretClassSizes = new List<TurretClassSize>();
		private const string XmlTurretClassName = "TurretClass";
		private const string XmlTurretClassSizesName = "TurretClassSizes";

		private static string MakeModelName(string barrel)
		{
			if (string.IsNullOrWhiteSpace(barrel))
				return barrel;
			return barrel + ".scene";
		}

		public IEnumerable<LogicalTurretClass> GetLogicalTurretClasses(
		  bool ignoreInvalidData)
		{
			foreach (TurretClassSize turretClassSiz in this.TurretClassSizes)
			{
				WeaponEnums.TurretClasses turretClass;
				WeaponEnums.WeaponSizes turretSize;
				if (Enum.TryParse<WeaponEnums.TurretClasses>(this.ActualTurretClass, out turretClass) && Enum.TryParse<WeaponEnums.WeaponSizes>(turretClassSiz.TurretSize, out turretSize))
					yield return new LogicalTurretClass()
					{
						TurretClass = turretClass,
						TurretSize = turretSize,
						BarrelModelName = TurretClass.MakeModelName(turretClassSiz.Barrel),
						TurretModelName = TurretClass.MakeModelName(turretClassSiz.Turret),
						BaseModelName = TurretClass.MakeModelName(turretClassSiz.Base)
					};
			}
		}

		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.ActualTurretClass, nameof(TurretClass), ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.TurretClassSizes, "TurretClassSizes", "TurretClassSize", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.ActualTurretClass = XmlHelper.GetData<string>(node, nameof(TurretClass));
			this.TurretClassSizes = XmlHelper.GetDataObjectCollection<TurretClassSize>(node, "TurretClassSizes", "TurretClassSize");
		}
	}
}
