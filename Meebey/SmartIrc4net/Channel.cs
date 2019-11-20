// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.Channel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
	public class Channel
	{
		private string _Key = string.Empty;
		private Hashtable _Users = Hashtable.Synchronized(new Hashtable((IHashCodeProvider)new CaseInsensitiveHashCodeProvider(), (IComparer)new CaseInsensitiveComparer()));
		private Hashtable _Ops = Hashtable.Synchronized(new Hashtable((IHashCodeProvider)new CaseInsensitiveHashCodeProvider(), (IComparer)new CaseInsensitiveComparer()));
		private Hashtable _Voices = Hashtable.Synchronized(new Hashtable((IHashCodeProvider)new CaseInsensitiveHashCodeProvider(), (IComparer)new CaseInsensitiveComparer()));
		private StringCollection _Bans = new StringCollection();
		private string _Topic = string.Empty;
		private string _Mode = string.Empty;
		private string _Name;
		private int _UserLimit;
		private DateTime _ActiveSyncStart;
		private DateTime _ActiveSyncStop;
		private TimeSpan _ActiveSyncTime;
		private bool _IsSycned;

		internal Channel(string name)
		{
			this._Name = name;
			this._ActiveSyncStart = DateTime.Now;
		}

		public string Name
		{
			get
			{
				return this._Name;
			}
		}

		public string Key
		{
			get
			{
				return this._Key;
			}
			set
			{
				this._Key = value;
			}
		}

		public Hashtable Users
		{
			get
			{
				return (Hashtable)this._Users.Clone();
			}
		}

		internal Hashtable UnsafeUsers
		{
			get
			{
				return this._Users;
			}
		}

		public Hashtable Ops
		{
			get
			{
				return (Hashtable)this._Ops.Clone();
			}
		}

		internal Hashtable UnsafeOps
		{
			get
			{
				return this._Ops;
			}
		}

		public Hashtable Voices
		{
			get
			{
				return (Hashtable)this._Voices.Clone();
			}
		}

		internal Hashtable UnsafeVoices
		{
			get
			{
				return this._Voices;
			}
		}

		public StringCollection Bans
		{
			get
			{
				return this._Bans;
			}
		}

		public string Topic
		{
			get
			{
				return this._Topic;
			}
			set
			{
				this._Topic = value;
			}
		}

		public int UserLimit
		{
			get
			{
				return this._UserLimit;
			}
			set
			{
				this._UserLimit = value;
			}
		}

		public string Mode
		{
			get
			{
				return this._Mode;
			}
			set
			{
				this._Mode = value;
			}
		}

		public DateTime ActiveSyncStart
		{
			get
			{
				return this._ActiveSyncStart;
			}
		}

		public DateTime ActiveSyncStop
		{
			get
			{
				return this._ActiveSyncStop;
			}
			set
			{
				this._ActiveSyncStop = value;
				this._ActiveSyncTime = this._ActiveSyncStop.Subtract(this._ActiveSyncStart);
			}
		}

		public TimeSpan ActiveSyncTime
		{
			get
			{
				return this._ActiveSyncTime;
			}
		}

		public bool IsSycned
		{
			get
			{
				return this._IsSycned;
			}
			set
			{
				this._IsSycned = value;
			}
		}
	}
}
