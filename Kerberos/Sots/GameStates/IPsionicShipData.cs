// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.IPsionicShipData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	internal interface IPsionicShipData
	{
		List<LogicalPsionic> Psionics { get; }

		List<int> Designs { get; }

		LogicalBank Bank { get; }

		LogicalPsionic SelectedPsionic { get; set; }

		int SelectedDesign { get; set; }

		bool RequiresDesign { get; set; }

		bool DesignIsSelectable { get; set; }

		int? FiringMode { get; set; }

		int? FilterMode { get; set; }
	}
}
