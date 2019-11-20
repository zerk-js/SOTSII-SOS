// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.BidirMap`2
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections.Generic;

namespace Kerberos.Sots.Framework
{
	internal class BidirMap<TForwardKey, TReverseKey>
	{
		public readonly Dictionary<TForwardKey, TReverseKey> Forward = new Dictionary<TForwardKey, TReverseKey>();
		public readonly Dictionary<TReverseKey, TForwardKey> Reverse = new Dictionary<TReverseKey, TForwardKey>();

		public void Insert(TForwardKey f, TReverseKey r)
		{
			this.Forward[f] = r;
			this.Reverse[r] = f;
		}

		public void Remove(TForwardKey f, TReverseKey r)
		{
			this.Forward.Remove(f);
			this.Reverse.Remove(r);
		}

		public void Clear()
		{
			this.Forward.Clear();
			this.Reverse.Clear();
		}
	}
}
