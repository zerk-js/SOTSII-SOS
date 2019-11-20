// Decompiled with JetBrains decompiler
// Type: Meebey.SmartIrc4net.IrcConnection
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Meebey.SmartIrc4net
{
	public class IrcConnection
	{
		private string[] _AddressList = new string[1]
		{
	  "localhost"
		};
		private Hashtable _SendBuffer = Hashtable.Synchronized(new Hashtable());
		private int _SendDelay = 200;
		private int _AutoRetryDelay = 30;
		private Encoding _Encoding = Encoding.Default;
		private int _SocketReceiveTimeout = 600;
		private int _SocketSendTimeout = 600;
		private int _IdleWorkerInterval = 60;
		private int _PingInterval = 60;
		private int _PingTimeout = 300;
		private string _VersionNumber;
		private string _VersionString;
		private int _CurrentAddress;
		private int _Port;
		private StreamReader _Reader;
		private StreamWriter _Writer;
		private IrcConnection.ReadThread _ReadThread;
		private IrcConnection.WriteThread _WriteThread;
		private IrcConnection.IdleWorkerThread _IdleWorkerThread;
		private IrcTcpClient _TcpClient;
		private bool _IsRegistered;
		private bool _IsConnected;
		private bool _IsConnectionError;
		private bool _IsDisconnecting;
		private int _ConnectTries;
		private bool _AutoRetry;
		private bool _AutoReconnect;
		private DateTime _LastPingSent;
		private DateTime _LastPongReceived;
		private TimeSpan _Lag;

		public event ReadLineEventHandler OnReadLine;

		public event WriteLineEventHandler OnWriteLine;

		public event EventHandler OnConnecting;

		public event EventHandler OnConnected;

		public event EventHandler OnDisconnecting;

		public event EventHandler OnDisconnected;

		public event EventHandler OnConnectionError;

		public event AutoConnectErrorEventHandler OnAutoConnectError;

		protected bool IsConnectionError
		{
			get
			{
				lock (this)
					return this._IsConnectionError;
			}
			set
			{
				lock (this)
					this._IsConnectionError = value;
			}
		}

		protected bool IsDisconnecting
		{
			get
			{
				lock (this)
					return this._IsDisconnecting;
			}
			set
			{
				lock (this)
					this._IsDisconnecting = value;
			}
		}

		public string Address
		{
			get
			{
				return this._AddressList[this._CurrentAddress];
			}
		}

		public string[] AddressList
		{
			get
			{
				return this._AddressList;
			}
		}

		public int Port
		{
			get
			{
				return this._Port;
			}
		}

		public bool AutoReconnect
		{
			get
			{
				return this._AutoReconnect;
			}
			set
			{
				this._AutoReconnect = value;
			}
		}

		public bool AutoRetry
		{
			get
			{
				return this._AutoRetry;
			}
			set
			{
				this._AutoRetry = value;
			}
		}

		public int AutoRetryDelay
		{
			get
			{
				return this._AutoRetryDelay;
			}
			set
			{
				this._AutoRetryDelay = value;
			}
		}

		public int SendDelay
		{
			get
			{
				return this._SendDelay;
			}
			set
			{
				this._SendDelay = value;
			}
		}

		public bool IsRegistered
		{
			get
			{
				return this._IsRegistered;
			}
		}

		public bool IsConnected
		{
			get
			{
				return this._IsConnected;
			}
		}

		public string VersionNumber
		{
			get
			{
				return this._VersionNumber;
			}
		}

		public string VersionString
		{
			get
			{
				return this._VersionString;
			}
		}

		public Encoding Encoding
		{
			get
			{
				return this._Encoding;
			}
			set
			{
				this._Encoding = value;
			}
		}

		public int SocketReceiveTimeout
		{
			get
			{
				return this._SocketReceiveTimeout;
			}
			set
			{
				this._SocketReceiveTimeout = value;
			}
		}

		public int SocketSendTimeout
		{
			get
			{
				return this._SocketSendTimeout;
			}
			set
			{
				this._SocketSendTimeout = value;
			}
		}

		public int IdleWorkerInterval
		{
			get
			{
				return this._IdleWorkerInterval;
			}
			set
			{
				this._IdleWorkerInterval = value;
			}
		}

		public int PingInterval
		{
			get
			{
				return this._PingInterval;
			}
			set
			{
				this._PingInterval = value;
			}
		}

		public int PingTimeout
		{
			get
			{
				return this._PingTimeout;
			}
			set
			{
				this._PingTimeout = value;
			}
		}

		public TimeSpan Lag
		{
			get
			{
				if (this._LastPingSent > this._LastPongReceived)
					return DateTime.Now - this._LastPingSent;
				return this._Lag;
			}
		}

		public IrcConnection()
		{
			this._SendBuffer[(object)Priority.High] = (object)System.Collections.Queue.Synchronized(new System.Collections.Queue());
			this._SendBuffer[(object)Priority.AboveMedium] = (object)System.Collections.Queue.Synchronized(new System.Collections.Queue());
			this._SendBuffer[(object)Priority.Medium] = (object)System.Collections.Queue.Synchronized(new System.Collections.Queue());
			this._SendBuffer[(object)Priority.BelowMedium] = (object)System.Collections.Queue.Synchronized(new System.Collections.Queue());
			this._SendBuffer[(object)Priority.Low] = (object)System.Collections.Queue.Synchronized(new System.Collections.Queue());
			this.OnReadLine += new ReadLineEventHandler(this._SimpleParser);
			this.OnConnectionError += new EventHandler(this._OnConnectionError);
			this._ReadThread = new IrcConnection.ReadThread(this);
			this._WriteThread = new IrcConnection.WriteThread(this);
			this._IdleWorkerThread = new IrcConnection.IdleWorkerThread(this);
			Assembly assembly = Assembly.GetAssembly(this.GetType());
			AssemblyName name = assembly.GetName(false);
			AssemblyProductAttribute customAttribute = (AssemblyProductAttribute)assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
			this._VersionNumber = name.Version.ToString();
			this._VersionString = customAttribute.Product + " " + this._VersionNumber;
		}

		public void Connect(string[] addresslist, int port)
		{
			if (this._IsConnected)
				throw new AlreadyConnectedException("Already connected to: " + this.Address + ":" + (object)this.Port);
			++this._ConnectTries;
			this._AddressList = (string[])addresslist.Clone();
			this._Port = port;
			if (this.OnConnecting != null)
				this.OnConnecting((object)this, EventArgs.Empty);
			try
			{
				IPAddress address = Dns.GetHostEntry(this.Address).AddressList[0];
				this._TcpClient = new IrcTcpClient();
				this._TcpClient.NoDelay = true;
				this._TcpClient.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
				this._TcpClient.ReceiveTimeout = this._SocketReceiveTimeout * 1000;
				this._TcpClient.SendTimeout = this._SocketSendTimeout * 1000;
				this._TcpClient.Connect(address, port);
				Stream stream = (Stream)this._TcpClient.GetStream();
				this._Reader = new StreamReader(stream, this._Encoding);
				this._Writer = new StreamWriter(stream, this._Encoding);
				if (this._Encoding.GetPreamble().Length > 0)
					this._Writer.WriteLine();
				this._ConnectTries = 0;
				this.IsConnectionError = false;
				this._IsConnected = true;
				this._ReadThread.Start();
				this._WriteThread.Start();
				this._IdleWorkerThread.Start();
				if (this.OnConnected == null)
					return;
				this.OnConnected((object)this, EventArgs.Empty);
			}
			catch (Exception ex1)
			{
				if (this._Reader != null)
				{
					try
					{
						this._Reader.Close();
					}
					catch (ObjectDisposedException ex2)
					{
					}
				}
				if (this._Writer != null)
				{
					try
					{
						this._Writer.Close();
					}
					catch (ObjectDisposedException ex2)
					{
					}
				}
				if (this._TcpClient != null)
					this._TcpClient.Close();
				this._IsConnected = false;
				this.IsConnectionError = true;
				if (this._AutoRetry && this._ConnectTries <= 3)
				{
					if (this.OnAutoConnectError != null)
						this.OnAutoConnectError((object)this, new AutoConnectErrorEventArgs(this.Address, this.Port, ex1));
					Thread.Sleep(this._AutoRetryDelay * 1000);
					this._NextAddress();
					this.Connect(this._AddressList, this._Port);
				}
				else
					throw new CouldNotConnectException("Could not connect to: " + this.Address + ":" + (object)this.Port + " " + ex1.Message, ex1);
			}
		}

		public void Connect(string address, int port)
		{
			this.Connect(new string[1] { address }, port);
		}

		public void Reconnect()
		{
			this.Disconnect();
			this.Connect(this._AddressList, this._Port);
		}

		public void Disconnect()
		{
			int num = this.IsConnected ? 1 : 0;
			if (this.OnDisconnecting != null)
				this.OnDisconnecting((object)this, EventArgs.Empty);
			this.IsDisconnecting = true;
			this._ReadThread.Stop();
			this._WriteThread.Stop();
			this._TcpClient.Close();
			this._IsConnected = false;
			this._IsRegistered = false;
			this.IsDisconnecting = false;
			if (this.OnDisconnected == null)
				return;
			this.OnDisconnected((object)this, EventArgs.Empty);
		}

		public void Listen(bool blocking)
		{
			if (blocking)
			{
				while (this.IsConnected)
					this.ReadLine(true);
			}
			else
			{
				do
					;
				while (this.ReadLine(false).Length > 0);
			}
		}

		public void Listen()
		{
			this.Listen(true);
		}

		public void ListenOnce(bool blocking)
		{
			this.ReadLine(blocking);
		}

		public void ListenOnce()
		{
			this.ListenOnce(true);
		}

		public string ReadLine(bool blocking)
		{
			string line = "";
			if (blocking)
			{
				while (this.IsConnected && !this.IsConnectionError && this._ReadThread.Queue.Count == 0)
					Thread.Sleep(10);
			}
			if (this.IsConnected && this._ReadThread.Queue.Count > 0)
				line = this._ReadThread.Queue.Dequeue() as string;
			if (line != null && line.Length > 0 && this.OnReadLine != null)
				this.OnReadLine((object)this, new ReadLineEventArgs(line));
			if (this.IsConnectionError && !this.IsDisconnecting && this.OnConnectionError != null)
				this.OnConnectionError((object)this, EventArgs.Empty);
			return line;
		}

		public void WriteLine(string data, Priority priority)
		{
			if (priority == Priority.Critical)
			{
				if (!this.IsConnected)
					throw new NotConnectedException();
				this._WriteLine(data);
			}
			else
				((System.Collections.Queue)this._SendBuffer[(object)priority]).Enqueue((object)data);
		}

		public void WriteLine(string data)
		{
			this.WriteLine(data, Priority.Medium);
		}

		private bool _WriteLine(string data)
		{
			if (!this.IsConnected)
				return false;
			try
			{
				this._Writer.Write(data + "\r\n");
				this._Writer.Flush();
			}
			catch (IOException ex)
			{
				this.IsConnectionError = true;
				return false;
			}
			catch (ObjectDisposedException ex)
			{
				this.IsConnectionError = true;
				return false;
			}
			if (this.OnWriteLine != null)
				this.OnWriteLine((object)this, new WriteLineEventArgs(data));
			return true;
		}

		private void _NextAddress()
		{
			++this._CurrentAddress;
			if (this._CurrentAddress < this._AddressList.Length)
				return;
			this._CurrentAddress = 0;
		}

		private void _SimpleParser(object sender, ReadLineEventArgs args)
		{
			string line = args.Line;
			string[] strArray = line.Split(' ');
			if (line[0] == ':')
			{
				string s = strArray[1];
				int result = 0;
				ReplyCode replyCode = ReplyCode.Null;
				if (int.TryParse(s, out result) && System.Enum.IsDefined(typeof(ReplyCode), (object)result))
					replyCode = (ReplyCode)result;
				switch (replyCode)
				{
					case ReplyCode.Null:
						switch (strArray[1])
						{
							case "PONG":
								DateTime now = DateTime.Now;
								this._LastPongReceived = now;
								this._Lag = now - this._LastPingSent;
								return;
							case null:
								return;
							default:
								return;
						}
					case ReplyCode.Welcome:
						this._IsRegistered = true;
						break;
				}
			}
			else
			{
				string str;
				if ((str = strArray[0]) == null)
					return;
				int num = str == "ERROR" ? 1 : 0;
			}
		}

		private void _OnConnectionError(object sender, EventArgs e)
		{
			try
			{
				if (this.AutoReconnect)
					this.Reconnect();
				else
					this.Disconnect();
			}
			catch (ConnectionException ex)
			{
			}
		}

		private class ReadThread
		{
			private System.Collections.Queue _Queue = System.Collections.Queue.Synchronized(new System.Collections.Queue());
			private IrcConnection _Connection;
			private Thread _Thread;

			public System.Collections.Queue Queue
			{
				get
				{
					return this._Queue;
				}
			}

			public ReadThread(IrcConnection connection)
			{
				this._Connection = connection;
			}

			public void Start()
			{
				this._Thread = new Thread(new ThreadStart(this._Worker));
				this._Thread.Name = "ReadThread (" + this._Connection.Address + ":" + (object)this._Connection.Port + ")";
				this._Thread.IsBackground = true;
				this._Thread.Start();
			}

			public void Stop()
			{
				try
				{
					this._Thread.Abort();
				}
				catch (ThreadAbortException ex)
				{
				}
				this._Thread.Join();
				try
				{
					this._Connection._Reader.Close();
				}
				catch (ObjectDisposedException ex)
				{
				}
			}

			private void _Worker()
			{
				try
				{
					try
					{
						string str;
						while (this._Connection.IsConnected && (str = this._Connection._Reader.ReadLine()) != null)
							this._Queue.Enqueue((object)str);
					}
					catch (IOException ex)
					{
					}
					finally
					{
						if (!this._Connection.IsDisconnecting)
							this._Connection.IsConnectionError = true;
					}
				}
				catch (ThreadAbortException ex)
				{
					Thread.ResetAbort();
				}
				catch (Exception ex)
				{
				}
			}
		}

		private class WriteThread
		{
			private int _AboveMediumThresholdCount = 4;
			private int _MediumThresholdCount = 2;
			private int _BelowMediumThresholdCount = 1;
			private IrcConnection _Connection;
			private Thread _Thread;
			private int _HighCount;
			private int _AboveMediumCount;
			private int _MediumCount;
			private int _BelowMediumCount;
			private int _LowCount;
			private int _AboveMediumSentCount;
			private int _MediumSentCount;
			private int _BelowMediumSentCount;
			private int _BurstCount;

			public WriteThread(IrcConnection connection)
			{
				this._Connection = connection;
			}

			public void Start()
			{
				this._Thread = new Thread(new ThreadStart(this._Worker));
				this._Thread.Name = "WriteThread (" + this._Connection.Address + ":" + (object)this._Connection.Port + ")";
				this._Thread.IsBackground = true;
				this._Thread.Start();
			}

			public void Stop()
			{
				try
				{
					this._Thread.Abort();
				}
				catch (ThreadAbortException ex)
				{
				}
				this._Thread.Join();
				try
				{
					this._Connection._Writer.Close();
				}
				catch (ObjectDisposedException ex)
				{
				}
			}

			private void _Worker()
			{
				try
				{
					try
					{
						while (this._Connection.IsConnected)
						{
							this._CheckBuffer();
							Thread.Sleep(this._Connection._SendDelay);
						}
					}
					catch (IOException ex)
					{
					}
					finally
					{
						if (!this._Connection.IsDisconnecting)
							this._Connection.IsConnectionError = true;
					}
				}
				catch (ThreadAbortException ex)
				{
					Thread.ResetAbort();
				}
				catch (Exception ex)
				{
				}
			}

			private void _CheckBuffer()
			{
				if (!this._Connection._IsRegistered)
					return;
				this._HighCount = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.High]).Count;
				this._AboveMediumCount = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.AboveMedium]).Count;
				this._MediumCount = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.Medium]).Count;
				this._BelowMediumCount = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.BelowMedium]).Count;
				this._LowCount = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.Low]).Count;
				if (this._CheckHighBuffer() && this._CheckAboveMediumBuffer() && (this._CheckMediumBuffer() && this._CheckBelowMediumBuffer()) && this._CheckLowBuffer())
				{
					this._AboveMediumSentCount = 0;
					this._MediumSentCount = 0;
					this._BelowMediumSentCount = 0;
					this._BurstCount = 0;
				}
				if (this._BurstCount >= 3)
					return;
				++this._BurstCount;
			}

			private bool _CheckHighBuffer()
			{
				if (this._HighCount > 0)
				{
					string data = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.High]).Dequeue() as string;
					if (!this._Connection._WriteLine(data))
						((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.High]).Enqueue((object)data);
					if (this._HighCount > 1)
						return false;
				}
				return true;
			}

			private bool _CheckAboveMediumBuffer()
			{
				if (this._AboveMediumCount > 0 && this._AboveMediumSentCount < this._AboveMediumThresholdCount)
				{
					string data = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.AboveMedium]).Dequeue() as string;
					if (!this._Connection._WriteLine(data))
						((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.AboveMedium]).Enqueue((object)data);
					++this._AboveMediumSentCount;
					if (this._AboveMediumSentCount < this._AboveMediumThresholdCount)
						return false;
				}
				return true;
			}

			private bool _CheckMediumBuffer()
			{
				if (this._MediumCount > 0 && this._MediumSentCount < this._MediumThresholdCount)
				{
					string data = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.Medium]).Dequeue() as string;
					if (!this._Connection._WriteLine(data))
						((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.Medium]).Enqueue((object)data);
					++this._MediumSentCount;
					if (this._MediumSentCount < this._MediumThresholdCount)
						return false;
				}
				return true;
			}

			private bool _CheckBelowMediumBuffer()
			{
				if (this._BelowMediumCount > 0 && this._BelowMediumSentCount < this._BelowMediumThresholdCount)
				{
					string data = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.BelowMedium]).Dequeue() as string;
					if (!this._Connection._WriteLine(data))
						((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.BelowMedium]).Enqueue((object)data);
					++this._BelowMediumSentCount;
					if (this._BelowMediumSentCount < this._BelowMediumThresholdCount)
						return false;
				}
				return true;
			}

			private bool _CheckLowBuffer()
			{
				if (this._LowCount > 0 && this._HighCount <= 0 && (this._AboveMediumCount <= 0 && this._MediumCount <= 0) && this._BelowMediumCount <= 0)
				{
					string data = ((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.Low]).Dequeue() as string;
					if (!this._Connection._WriteLine(data))
						((System.Collections.Queue)this._Connection._SendBuffer[(object)Priority.Low]).Enqueue((object)data);
					if (this._LowCount > 1)
						return false;
				}
				return true;
			}
		}

		private class IdleWorkerThread
		{
			private IrcConnection _Connection;
			private Thread _Thread;

			public IdleWorkerThread(IrcConnection connection)
			{
				this._Connection = connection;
			}

			public void Start()
			{
				DateTime now = DateTime.Now;
				this._Connection._LastPingSent = now;
				this._Connection._LastPongReceived = now;
				this._Thread = new Thread(new ThreadStart(this._Worker));
				this._Thread.Name = "IdleWorkerThread (" + this._Connection.Address + ":" + (object)this._Connection.Port + ")";
				this._Thread.IsBackground = true;
				this._Thread.Start();
			}

			public void Stop()
			{
				try
				{
					this._Thread.Abort();
				}
				catch (ThreadAbortException ex)
				{
				}
			}

			private void _Worker()
			{
				try
				{
					while (this._Connection.IsConnected)
					{
						Thread.Sleep(this._Connection._IdleWorkerInterval);
						if (this._Connection.IsRegistered)
						{
							DateTime now = DateTime.Now;
							int totalSeconds1 = (int)(now - this._Connection._LastPingSent).TotalSeconds;
							int totalSeconds2 = (int)(now - this._Connection._LastPongReceived).TotalSeconds;
							if (totalSeconds1 < this._Connection._PingTimeout)
							{
								if (!(this._Connection._LastPingSent > this._Connection._LastPongReceived) && totalSeconds2 > this._Connection._PingInterval)
								{
									this._Connection.WriteLine(Rfc2812.Ping(this._Connection.Address), Priority.Critical);
									this._Connection._LastPingSent = now;
								}
							}
							else
							{
								if (this._Connection.IsDisconnecting)
									break;
								this._Connection.IsConnectionError = true;
								break;
							}
						}
					}
				}
				catch (ThreadAbortException ex)
				{
					Thread.ResetAbort();
				}
				catch (Exception ex)
				{
				}
			}
		}
	}
}
