// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.IMessageQueue
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Engine
{
	public interface IMessageQueue
	{
		int IncomingCapacity { get; }

		int OutgoingCapacity { get; }

		int IncomingSize { get; }

		int OutgoingSize { get; }

		void PrepareIncoming();

		void PrepareOutgoing();

		void Update();

		int GetNextMessage(byte[] data);

		int GetNextMessageSize();

		int GetNextMessageData(byte[] data, int size);

		void PutMessage(byte[] data, int count);
	}
}
