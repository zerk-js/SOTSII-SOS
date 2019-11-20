// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.SuulkaPsiBonusLibrary
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
	internal static class SuulkaPsiBonusLibrary
	{
		public static IEnumerable<SuulkaPsiBonus> Enumerate(XmlDocument doc)
		{
			foreach (XmlElement source in doc["CommonAssets"].OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("SuulkaPsiBonuses", StringComparison.InvariantCulture))))
			{
				foreach (XmlElement xmlElement1 in source.OfType<XmlElement>())
				{
					SuulkaPsiBonus psiBonus = new SuulkaPsiBonus();
					psiBonus.Name = xmlElement1.GetAttribute("name");
					string value = xmlElement1.GetAttribute("ability");
					if (!string.IsNullOrEmpty(value))
						psiBonus.Ability = (SuulkaPsiBonusAbilityType)Enum.Parse(typeof(SuulkaPsiBonusAbilityType), value);
					XmlElement psiEff = xmlElement1["PsiEfficiency"];
					if (psiEff != null)
					{
						foreach (XmlElement xmlElement2 in psiEff.OfType<XmlElement>())
						{
							value = xmlElement2.GetAttribute("psi");
							if (!string.IsNullOrEmpty(value))
							{
								SectionEnumerations.PsionicAbility psionicAbility = (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), value);
								value = xmlElement2.GetAttribute("rate");
								if (!string.IsNullOrEmpty(value))
									psiBonus.Rate[(int)psionicAbility] = float.Parse(value);
								value = xmlElement2.GetAttribute("efficiency");
								if (!string.IsNullOrEmpty(value))
									psiBonus.PsiEfficiency[(int)psionicAbility] = float.Parse(value);
							}
						}
					}
					value = xmlElement1.GetAttribute("psidrain");
					if (!string.IsNullOrEmpty(value))
						psiBonus.PsiDrainMultiplyer = 1f;
					value = xmlElement1.GetAttribute("lifedrain");
					if (!string.IsNullOrEmpty(value))
						psiBonus.LifeDrainMultiplyer = float.Parse(value);
					value = xmlElement1.GetAttribute("tkfist");
					if (!string.IsNullOrEmpty(value))
						psiBonus.TKFistMultiplyer = float.Parse(value);
					value = xmlElement1.GetAttribute("crush");
					if (!string.IsNullOrEmpty(value))
						psiBonus.CrushMultiplyer = float.Parse(value);
					value = xmlElement1.GetAttribute("fear");
					if (!string.IsNullOrEmpty(value))
						psiBonus.FearMultiplyer = float.Parse(value);
					value = xmlElement1.GetAttribute("control");
					if (!string.IsNullOrEmpty(value))
						psiBonus.ControlDuration = float.Parse(value);
					value = xmlElement1.GetAttribute("movement");
					if (!string.IsNullOrEmpty(value))
						psiBonus.MovementMultiplyer = float.Parse(value);
					value = xmlElement1.GetAttribute("biomissile");
					if (!string.IsNullOrEmpty(value))
						psiBonus.BioMissileMultiplyer = float.Parse(value);
					for (int index = 0; index <= 19; ++index)
					{
						if ((double)psiBonus.Rate[index] <= 0.0)
							psiBonus.Rate[index] = 1f;
						if ((double)psiBonus.PsiEfficiency[index] <= 0.0)
							psiBonus.PsiEfficiency[index] = 1f;
					}
					yield return psiBonus;
				}
			}
		}
	}
}
