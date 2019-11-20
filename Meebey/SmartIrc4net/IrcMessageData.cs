// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.IrcMessageData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class IrcMessageData
	{
		private IrcClient _Irc;
		private string _From;
		private string _Nick;
		private string _Ident;
		private string _Host;
		private string _Channel;
		private string _Message;
		private string[] _MessageArray;
		private string _RawMessage;
		private string[] _RawMessageArray;
		private ReceiveType _Type;
		private ReplyCode _ReplyCode;

		public IrcClient Irc
		{
			get
			{
				return this._Irc;
			}
		}

		public string From
		{
			get
			{
				return this._From;
			}
		}

		public string Nick
		{
			get
			{
				return this._Nick;
			}
		}

		public string Ident
		{
			get
			{
				return this._Ident;
			}
		}

		public string Host
		{
			get
			{
				return this._Host;
			}
		}

		public string Channel
		{
			get
			{
				return this._Channel;
			}
		}

		public string Message
		{
			get
			{
				return this._Message;
			}
		}

		public string[] MessageArray
		{
			get
			{
				return this._MessageArray;
			}
		}

		public string RawMessage
		{
			get
			{
				return this._RawMessage;
			}
		}

		public string[] RawMessageArray
		{
			get
			{
				return this._RawMessageArray;
			}
		}

		public ReceiveType Type
		{
			get
			{
				return this._Type;
			}
		}

		public ReplyCode ReplyCode
		{
			get
			{
				return this._ReplyCode;
			}
		}

		public IrcMessageData(
		  IrcClient ircclient,
		  string from,
		  string nick,
		  string ident,
		  string host,
		  string channel,
		  string message,
		  string rawmessage,
		  ReceiveType type,
		  ReplyCode replycode)
		{
			this._Irc = ircclient;
			this._RawMessage = rawmessage;
			this._RawMessageArray = rawmessage.Split(' ');
			this._Type = type;
			this._ReplyCode = replycode;
			this._From = from;
			this._Nick = nick;
			this._Ident = ident;
			this._Host = host;
			this._Channel = channel;
			if (message == null)
				return;
			this._Message = message;
			this._MessageArray = message.Split(' ');
		}
	}
}
