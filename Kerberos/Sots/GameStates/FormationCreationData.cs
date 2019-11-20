// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.FormationCreationData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.GameStates
{
	internal struct FormationCreationData
	{
		public int ShipID;
		public int DesignID;
		public ShipRole ShipRole;
		public ShipClass ShipClass;
		public Vector3 FormationPosition;
	}
}
