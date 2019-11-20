// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.LoaFleetComposition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class LoaFleetComposition : IIDProvider
	{
		public int PlayerID;
		public string Name;
		public List<LoaFleetShipDef> designs;

		public int ID { get; set; }
	}
}
