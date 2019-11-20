// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.PsionicLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.ShipFramework
{
	internal static class PsionicLibrary
	{
		public static IEnumerable<LogicalPsionic> Enumerate(XmlDocument doc)
		{
			foreach (XmlElement source in doc["CommonAssets"].OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("psionics", StringComparison.InvariantCulture))))
			{
				foreach (XmlElement xmlElement in source.OfType<XmlElement>())
				{
					LogicalPsionic logPsionic = new LogicalPsionic()
					{
						Name = xmlElement.GetAttribute("name")
					};
					logPsionic.Ability = (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), logPsionic.Name);
					logPsionic.PsionicTitle = xmlElement.GetAttribute("title");
					logPsionic.Description = xmlElement.GetAttribute("description");
					logPsionic.Icon = xmlElement.GetAttribute("icon");
					logPsionic.Model = xmlElement.GetAttribute("mesh");
					logPsionic.MinPower = int.Parse(xmlElement.GetAttribute("minPower"));
					logPsionic.MaxPower = int.Parse(xmlElement.GetAttribute("maxPower"));
					logPsionic.BaseCost = int.Parse(xmlElement.GetAttribute("cost"));
					logPsionic.Range = float.Parse(xmlElement.GetAttribute("range"));
					logPsionic.BaseDamage = float.Parse(xmlElement.GetAttribute("baseDamage"));
					logPsionic.CastorEffect = new LogicalEffect()
					{
						Name = xmlElement.GetAttribute("castorEffect") ?? string.Empty
					};
					logPsionic.CastEffect = new LogicalEffect()
					{
						Name = xmlElement.GetAttribute("castEffect") ?? string.Empty
					};
					logPsionic.ApplyEffect = new LogicalEffect()
					{
						Name = xmlElement.GetAttribute("applyEffect") ?? string.Empty
					};
					logPsionic.PsionicTitle = logPsionic.Name;
					logPsionic.RequiredTechID = xmlElement.GetAttribute("tech");
					logPsionic.RequiresSuulka = bool.Parse(xmlElement.GetAttribute("suulka_only"));
					yield return logPsionic;
				}
			}
		}
	}
}
