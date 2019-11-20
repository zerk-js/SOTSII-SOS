// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.MoveOrderInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.Data
{
	internal class MoveOrderInfo : IIDProvider
	{
		public int FleetID;
		public int FromSystemID;
		public Vector3 FromCoords;
		public int ToSystemID;
		public Vector3 ToCoords;
		public float Progress;

		public int ID { get; set; }
	}
}
