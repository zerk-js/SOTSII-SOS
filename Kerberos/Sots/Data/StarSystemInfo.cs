// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarSystemInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class StarSystemInfo : StellarInfo
	{
		public bool IsOpen = true;
		public string Name;
		public string StellarClass;
		public int? ProvinceID;
		public bool IsVisible;
		public int? TerrainID;
		public List<int> ControlZones;

		public bool IsDeepSpace
		{
			get
			{
				return this.StellarClass == "Deepspace";
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("ID={0},Name={1}", (object)this.ID, (object)this.Name);
		}

		public static bool operator ==(StarSystemInfo s1, StarSystemInfo s2)
		{
			if ((object)s1 == null ^ (object)s2 == null)
				return false;
			if ((object)s1 == null && (object)s2 == null)
				return true;
			return s1.ID == s2.ID;
		}

		public static bool operator !=(StarSystemInfo s1, StarSystemInfo s2)
		{
			if ((object)s1 == null ^ (object)s2 == null)
				return true;
			if ((object)s1 == null && (object)s2 == null)
				return false;
			return s1.ID != s2.ID;
		}

		public override bool Equals(object obj)
		{
			return this.ID == ((StellarInfo)obj).ID;
		}
	}
}
