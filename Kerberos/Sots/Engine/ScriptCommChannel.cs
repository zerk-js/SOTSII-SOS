// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.ScriptCommChannel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Kerberos.Sots.Engine
{
	internal class ScriptCommChannel
	{
		private UnicodeEncoding _stringcoder = new UnicodeEncoding();
		private ScriptMessageWriter _scriptMessageWriter = new ScriptMessageWriter();
		public static bool LogEnable;
		private IMessageQueue _messageQueue;
		private byte[] _messageBuffer;
		private uint _recvMessageCount;
		private uint _sendMessageCount;

		public ScriptCommChannel(IMessageQueue messageQueue)
		{
			this._messageQueue = messageQueue;
			this._messageBuffer = new byte[messageQueue.IncomingCapacity];
		}

		public void SendMessage(IEnumerable elements)
		{
			this._scriptMessageWriter.Clear();
			this._scriptMessageWriter.Write(elements);
			++this._sendMessageCount;
			this._messageQueue.PutMessage(this._scriptMessageWriter.GetBuffer(), (int)this._scriptMessageWriter.GetSize());
		}

		public IEnumerable<ScriptMessageReader> PumpMessages()
		{
			this._messageQueue.Update();
			while (true)
			{
				int size = this._messageQueue.GetNextMessageSize();
				if (size != 0)
				{
					ScriptMessageReader reader = new ScriptMessageReader();
					reader.SetSize((long)size);
					this._messageQueue.GetNextMessageData(reader.GetBuffer(), size);
					++this._recvMessageCount;
					yield return reader;
				}
				else
					break;
			}
		}
	}
}
