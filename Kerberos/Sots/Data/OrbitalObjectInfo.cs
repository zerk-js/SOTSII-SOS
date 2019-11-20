// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.OrbitalObjectInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class OrbitalObjectInfo : IIDProvider
	{
		public int? ParentID;
		public int StarSystemID;
		public OrbitalPath OrbitalPath;
		public string Name;

		public int ID { get; set; }
	}
}
