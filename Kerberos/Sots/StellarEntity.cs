﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StellarEntity
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots
{
	internal class StellarEntity : IStellarEntity
	{
		public Kerberos.Sots.Data.StarMapFramework.Orbit Params { get; set; }

		public int ID { get; set; }

		public Orbit Orbit { get; set; }
	}
}
