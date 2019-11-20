// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.WeaponRangeTable
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;

namespace Kerberos.Sots.ShipFramework
{
	internal class WeaponRangeTable
	{
		public WeaponRangeTableItem PointBlank;
		public WeaponRangeTableItem Effective;
		public WeaponRangeTableItem Maximum;
		public float PlanetRange;

		public WeaponRangeTableItem this[WeaponRanges range]
		{
			get
			{
				switch (range)
				{
					case WeaponRanges.PointBlank:
						return this.PointBlank;
					case WeaponRanges.Effective:
						return this.Effective;
					case WeaponRanges.Maximum:
						return this.Maximum;
					default:
						throw new ArgumentOutOfRangeException(nameof(range));
				}
			}
		}

		public IEnumerable<object> EnumerateScriptMessageParams()
		{
			yield return (object)this.PointBlank.Range;
			yield return (object)this.PointBlank.Deviation;
			yield return (object)this.PointBlank.Damage;
			yield return (object)this.Effective.Range;
			yield return (object)this.Effective.Deviation;
			yield return (object)this.Effective.Damage;
			yield return (object)this.Maximum.Range;
			yield return (object)this.Maximum.Deviation;
			yield return (object)this.Maximum.Damage;
			yield return (object)this.PlanetRange;
		}
	}
}
