// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.IRC.SotsIRC
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Meebey.SmartIrc4net;
using System;
using System.Collections;
using System.Text;

namespace Kerberos.Sots.IRC
{
	internal class SotsIRC
	{
		private static readonly string server = "kerberos-productions.com";
		private static readonly string channel = "#sots2";
		public IrcClient irc;
		private App App;
		private string _ircNick;
		private int nicknum;

		public SotsIRC(App app)
		{
			this.App = app;
			this.irc = new IrcClient();
			this._ircNick = "Connecting";
		}

		public void OnQueryMessage(object sender, IrcEventArgs e)
		{
		}

		public void OnError(object sender, ErrorEventArgs e)
		{
		}

		private void irc_OnErrorMessage(object sender, IrcEventArgs e)
		{
			if (e.Data.ReplyCode != ReplyCode.ErrorNicknameInUse)
				return;
			this.irc.RfcNick(this._ircNick + this.nicknum.ToString());
			++this.nicknum;
		}

		public void OnRawMessage(object sender, IrcEventArgs e)
		{
		}

		public void OnChannelMessage(object sender, IrcEventArgs e)
		{
			if (e.Data == null || e.Data.Message == null)
				return;
			ChannelUser channelUser = this.irc.GetChannelUser(SotsIRC.channel, e.Data.Nick);
			if (channelUser == null)
				return;
			string str = channelUser.IsOp ? "[b][" + e.Data.Nick + "]" : e.Data.Nick;
			this.App.Network.PostIRCChatMessage(string.IsNullOrEmpty(str) ? "?" : str, e.Data.Message);
		}

		public void OnNickChanged(object sender, IrcEventArgs e)
		{
			this._ircNick = this.irc.Nickname;
			this.App.Network.PostIRCNick(this.irc.Nickname);
		}

		public void SetupIRCClient(string name)
		{
			if (this.irc.IsConnected)
				this.SetNick(name);
			else
				this._SetupIRCClient(name);
		}

		private void _SetupIRCClient(string name)
		{
			if (!this.App.GameSettings.JoinGlobalChat)
			{
				this.App.Network.PostIRCChatMessage("", "Set Join Global Chat in Options to connect!");
			}
			else
			{
				this.irc.Encoding = Encoding.UTF8;
				this.irc.SendDelay = 200;
				this.irc.ActiveChannelSyncing = true;
				this.irc.OnQueryMessage += new IrcEventHandler(this.OnQueryMessage);
				this.irc.OnError += new ErrorEventHandler(this.OnError);
				this.irc.OnRawMessage += new IrcEventHandler(this.OnRawMessage);
				this.irc.OnChannelMessage += new IrcEventHandler(this.OnChannelMessage);
				this.irc.OnNickChange += new NickChangeEventHandler(this.OnNickChanged);
				this.irc.OnErrorMessage += new IrcEventHandler(this.irc_OnErrorMessage);
				this.irc.OnWho += new WhoEventHandler(this.irc_OnWho);
				this.irc.OnConnected += new EventHandler(this.irc_OnConnected);
				this.irc.OnConnecting += new EventHandler(this.irc_OnConnecting);
				this.irc.OnConnectionError += new EventHandler(this.irc_OnConnectionError);
				this.irc.OnMotd += new MotdEventHandler(this.irc_OnMotd);
				this.irc.OnTopic += new TopicEventHandler(this.irc_OnTopic);
				this.irc.OnDisconnected += new EventHandler(this.irc_OnDisconnected);
				string[] addresslist = new string[1]
				{
		  SotsIRC.server
				};
				int port = 6667;
				try
				{
					this.irc.Connect(addresslist, port);
				}
				catch (ConnectionException ex)
				{
					System.Console.WriteLine("couldn't connect! Reason: " + ex.Message);
				}
				if (this.irc.IsConnected)
				{
					this._ircNick = name.Replace(" ", "_");
					this.App.Network.PostIRCNick(this._ircNick);
					this.irc.Login(name, "Sots Client");
					this.irc.RfcJoin(SotsIRC.channel);
				}
				else
					this.irc_OnDisconnected((object)null, (EventArgs)null);
			}
		}

		private void irc_OnDisconnected(object sender, EventArgs e)
		{
			this.irc.OnQueryMessage -= new IrcEventHandler(this.OnQueryMessage);
			this.irc.OnError -= new ErrorEventHandler(this.OnError);
			this.irc.OnRawMessage -= new IrcEventHandler(this.OnRawMessage);
			this.irc.OnChannelMessage -= new IrcEventHandler(this.OnChannelMessage);
			this.irc.OnNickChange -= new NickChangeEventHandler(this.OnNickChanged);
			this.irc.OnErrorMessage -= new IrcEventHandler(this.irc_OnErrorMessage);
			this.irc.OnWho -= new WhoEventHandler(this.irc_OnWho);
			this.irc.OnConnected -= new EventHandler(this.irc_OnConnected);
			this.irc.OnConnecting -= new EventHandler(this.irc_OnConnecting);
			this.irc.OnConnectionError -= new EventHandler(this.irc_OnConnectionError);
			this.irc.OnMotd -= new MotdEventHandler(this.irc_OnMotd);
			this.irc.OnTopic -= new TopicEventHandler(this.irc_OnTopic);
			this.irc.OnDisconnected -= new EventHandler(this.irc_OnDisconnected);
		}

		private void irc_OnTopic(object sender, TopicEventArgs e)
		{
		}

		private void irc_OnMotd(object sender, MotdEventArgs e)
		{
			if (e.Data == null || e.Data.Message == null || this.irc.GetChannelUser(SotsIRC.channel, e.Data.Nick) == null)
				return;
			string str = "[b][MOTD:]";
			this.App.Network.PostIRCChatMessage(string.IsNullOrEmpty(str) ? "?" : str, "[b][" + e.Data.Message + "]");
		}

		private void irc_OnConnectionError(object sender, EventArgs e)
		{
			this.App.Network.PostIRCChatMessage("", "*** ERROR CONNECTING TO CHAT SERVER!");
			this.irc.Disconnect();
		}

		private void irc_OnConnecting(object sender, EventArgs e)
		{
			this.App.Network.PostIRCChatMessage("", "*** Connecting to chat server...");
		}

		private void irc_OnConnected(object sender, EventArgs e)
		{
			this.App.Network.PostIRCChatMessage("", "*** Connected to chat server!");
		}

		public void Update()
		{
			if (!this.irc.IsConnected)
				return;
			this.irc.Listen(false);
		}

		public void Disconnect()
		{
			if (!this.irc.IsConnected)
				return;
			this.irc.RfcQuit("Repensum est canicula");
		}

		private void irc_OnWho(object sender, WhoEventArgs e)
		{
		}

		public void SetNick(string nick)
		{
			if (!(this._ircNick != nick.Replace(" ", "_")))
				return;
			this._ircNick = nick.Replace(" ", "_");
			this.irc.RfcNick(nick);
		}

		public void SendChatMessage(string msg)
		{
			if (msg == "/who")
			{
				Channel channel = this.irc.GetChannel(SotsIRC.channel);
				string message = "";
				foreach (DictionaryEntry user in channel.Users)
				{
					string str = user.Key.ToString();
					message = !(message == "") ? message + ", " + str : channel.Users.Count.ToString() + " Connected Players. " + str;
				}
				this.App.Network.PostIRCChatMessage("", message);
			}
			else
			{
				if (!this.irc.IsConnected)
					return;
				this.irc.SendMessage(SendType.Message, SotsIRC.channel, msg);
			}
		}

		public void SendRawMessage(string cmd)
		{
			this.irc.WriteLine(cmd);
		}
	}
}
