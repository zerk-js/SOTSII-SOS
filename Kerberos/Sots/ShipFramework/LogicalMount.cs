// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalMount
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalMount
	{
		public MinMax Yaw = new MinMax()
		{
			Min = -60f,
			Max = 60f
		};
		public MinMax Pitch = new MinMax()
		{
			Min = -5f,
			Max = 60f
		};
		public LogicalBank Bank;
		public string TurretOverload;
		public string BarrelOverload;
		public string BaseOverload;
		public string NodeName;
		public string FireAnimName;
		public string ReloadAnimName;

		public override string ToString()
		{
			return this.NodeName ?? string.Empty;
		}
	}
}
