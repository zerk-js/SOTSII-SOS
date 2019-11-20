// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.NonRfcChannel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections;

namespace Meebey.SmartIrc4net
{
	public class NonRfcChannel : Channel
	{
		private Hashtable _Halfops = Hashtable.Synchronized(new Hashtable((IHashCodeProvider)new CaseInsensitiveHashCodeProvider(), (IComparer)new CaseInsensitiveComparer()));

		internal NonRfcChannel(string name)
		  : base(name)
		{
		}

		public Hashtable Halfops
		{
			get
			{
				return (Hashtable)this._Halfops.Clone();
			}
		}

		internal Hashtable UnsafeHalfops
		{
			get
			{
				return this._Halfops;
			}
		}
	}
}
