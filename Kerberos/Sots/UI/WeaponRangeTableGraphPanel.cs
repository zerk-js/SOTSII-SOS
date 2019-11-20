// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.WeaponRangeTableGraphPanel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal abstract class WeaponRangeTableGraphPanel : GraphPanel
	{
		public WeaponRangeTable RangeTable { get; private set; }

		public void SetRangeTables(WeaponRangeTable primary, WeaponRangeTable comparative)
		{
			if (primary == null)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)nameof(SetRangeTables));
			objectList.Add((object)this.ID);
			int num = comparative == null ? 1 : 2;
			objectList.Add((object)num);
			objectList.AddRange(primary.EnumerateScriptMessageParams());
			if (comparative != null)
				objectList.AddRange(comparative.EnumerateScriptMessageParams());
			this.UI.Send(objectList.ToArray());
		}

		public WeaponRangeTableGraphPanel(UICommChannel ui, string id)
		  : base(ui, id)
		{
		}
	}
}
