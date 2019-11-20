// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.ShipTargetComparision
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Framework;
using System.Collections.Generic;

namespace Kerberos.Sots.GameObjects
{
	internal class ShipTargetComparision : IComparer<Ship>
	{
		public CombatAI _ai;
		public Vector3 _formationPosition;

		public ShipTargetComparision(CombatAI ai, Vector3 formPos)
		{
			this._ai = ai;
			this._formationPosition = formPos;
		}

		public int Compare(Ship alpha, Ship beta)
		{
			int targetShipScore1 = this._ai.GetTargetShipScore(alpha);
			int targetShipScore2 = this._ai.GetTargetShipScore(beta);
			if (targetShipScore1 != targetShipScore2)
				return targetShipScore2 < targetShipScore1 ? -1 : 1;
			return (double)(alpha.Maneuvering.Position - this._formationPosition).Length < (double)(beta.Maneuvering.Position - this._formationPosition).Length ? -1 : 1;
		}
	}
}
