// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.BanInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Meebey.SmartIrc4net
{
	public class BanInfo
	{
		private string f_Channel;
		private string f_Mask;

		public string Channel
		{
			get
			{
				return this.f_Channel;
			}
		}

		public string Mask
		{
			get
			{
				return this.f_Mask;
			}
		}

		private BanInfo()
		{
		}

		public static BanInfo Parse(IrcMessageData data)
		{
			return new BanInfo()
			{
				f_Channel = data.RawMessageArray[3],
				f_Mask = data.RawMessageArray[4]
			};
		}
	}
}
