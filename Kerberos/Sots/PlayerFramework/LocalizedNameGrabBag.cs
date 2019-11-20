// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.LocalizedNameGrabBag
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Xml;

namespace Kerberos.Sots.PlayerFramework
{
	internal class LocalizedNameGrabBag
	{
		private int _next;

		public string Prefix { get; private set; }

		public int Count { get; private set; }

		private void Construct(string prefix, int count, Random random)
		{
			this.Prefix = prefix;
			this.Count = count;
			if (this.Count <= 0)
				return;
			this._next = random.Next(this.Count);
		}

		public LocalizedNameGrabBag(string prefix, int count, Random random)
		{
			this.Construct(prefix, count, random);
		}

		public LocalizedNameGrabBag(XmlElement element, Random random)
		{
			if (element == null)
				this.Construct(null, 0, (Random)null);
			else
				this.Construct(element.GetAttribute("prefix"), int.Parse(element.GetAttribute("count")), random);
		}

		public string GetNextStringID()
		{
			if (this.Count == 0)
				return null;
			this._next %= this.Count;
			string str = this.Prefix + (this._next + 1).ToString();
			++this._next;
			return str;
		}
	}
}
