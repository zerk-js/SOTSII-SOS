// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.GameObject
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Engine
{
	internal abstract class GameObject : IGameObject
	{
		private GameObjectStatus _engineObjectStatus;

		internal void PromoteEngineObjectStatus(GameObjectStatus value)
		{
			if (value == GameObjectStatus.Pending)
				throw new ArgumentOutOfRangeException(nameof(value), "Can not revert to pending status. Create a new object instead.");
			this._engineObjectStatus = value;
		}

		protected virtual GameObjectStatus OnCheckStatus()
		{
			return GameObjectStatus.Ready;
		}

		private GameObjectStatus CheckStatus()
		{
			if (this._engineObjectStatus != GameObjectStatus.Ready)
				return this._engineObjectStatus;
			return this.OnCheckStatus();
		}

		public virtual bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			return false;
		}

		public int ObjectID { get; set; }

		public GameObjectStatus ObjectStatus
		{
			get
			{
				return this.CheckStatus();
			}
		}

		public App App { get; set; }
	}
}
