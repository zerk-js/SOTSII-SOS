// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.IIDProviderComparer
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class IIDProviderComparer : Comparer<IIDProvider>, IEqualityComparer<IIDProvider>
	{
		public override int Compare(IIDProvider x, IIDProvider y)
		{
			if (x.ID < y.ID)
				return -1;
			return x.ID > y.ID ? 1 : 0;
		}

		public bool Equals(IIDProvider x, IIDProvider y)
		{
			return x.ID == y.ID;
		}

		public int GetHashCode(IIDProvider obj)
		{
			return obj.ID;
		}
	}
}
