// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.WhoInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Meebey.SmartIrc4net
{
	public class WhoInfo
	{
		private string f_Channel;
		private string f_Ident;
		private string f_Host;
		private string f_Server;
		private string f_Nick;
		private int f_HopCount;
		private string f_Realname;
		private bool f_IsAway;
		private bool f_IsOp;
		private bool f_IsVoice;
		private bool f_IsIrcOp;

		public string Channel
		{
			get
			{
				return this.f_Channel;
			}
		}

		public string Ident
		{
			get
			{
				return this.f_Ident;
			}
		}

		public string Host
		{
			get
			{
				return this.f_Host;
			}
		}

		public string Server
		{
			get
			{
				return this.f_Server;
			}
		}

		public string Nick
		{
			get
			{
				return this.f_Nick;
			}
		}

		public int HopCount
		{
			get
			{
				return this.f_HopCount;
			}
		}

		public string Realname
		{
			get
			{
				return this.f_Realname;
			}
		}

		public bool IsAway
		{
			get
			{
				return this.f_IsAway;
			}
		}

		public bool IsOp
		{
			get
			{
				return this.f_IsOp;
			}
		}

		public bool IsVoice
		{
			get
			{
				return this.f_IsVoice;
			}
		}

		public bool IsIrcOp
		{
			get
			{
				return this.f_IsIrcOp;
			}
		}

		private WhoInfo()
		{
		}

		public static WhoInfo Parse(IrcMessageData data)
		{
			WhoInfo whoInfo = new WhoInfo();
			whoInfo.f_Channel = data.RawMessageArray[3];
			whoInfo.f_Ident = data.RawMessageArray[4];
			whoInfo.f_Host = data.RawMessageArray[5];
			whoInfo.f_Server = data.RawMessageArray[6];
			whoInfo.f_Nick = data.RawMessageArray[7];
			whoInfo.f_Realname = string.Join(" ", data.MessageArray, 1, data.MessageArray.Length - 1);
			string message = data.MessageArray[0];
			try
			{
				int.Parse(message);
			}
			catch (FormatException ex)
			{
			}
			string rawMessage = data.RawMessageArray[8];
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			int length = rawMessage.Length;
			for (int index = 0; index < length; ++index)
			{
				switch (rawMessage[index])
				{
					case '*':
						flag3 = true;
						break;
					case '+':
						flag2 = true;
						break;
					case '@':
						flag1 = true;
						break;
					case 'G':
						flag4 = true;
						break;
					case 'H':
						flag4 = false;
						break;
				}
			}
			whoInfo.f_IsAway = flag4;
			whoInfo.f_IsOp = flag1;
			whoInfo.f_IsVoice = flag2;
			whoInfo.f_IsIrcOp = flag3;
			return whoInfo;
		}
	}
}
