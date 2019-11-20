// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ShipManeuveringInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.ShipFramework
{
	internal class ShipManeuveringInfo
	{
		public float LinearAccel = 20f;
		public Vector3 RotAccel = new Vector3(10f, 10f, 10f);
		public float LinearSpeed = 500f;
		public float RotationSpeed = 500f;
		public float Deacceleration;
	}
}
