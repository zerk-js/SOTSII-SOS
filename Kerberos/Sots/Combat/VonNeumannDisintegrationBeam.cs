// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannDisintegrationBeam
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.Combat
{
	[GameObjectType(InteropGameObjectType.IGOT_VONDISBEAM)]
	internal class VonNeumannDisintegrationBeam : GameObject, IActive
	{
		private bool m_Finished;
		private bool m_Succeeded;
		private int m_Resources;
		private bool _active;

		public bool Finished
		{
			get
			{
				return this.m_Finished;
			}
		}

		public bool Succeeded
		{
			get
			{
				return this.m_Succeeded;
			}
		}

		public int Resources
		{
			get
			{
				return this.m_Resources;
			}
		}

		public VonNeumannDisintegrationBeam()
		{
			this.m_Finished = false;
			this.m_Succeeded = false;
			this.m_Resources = 0;
		}

		protected override GameObjectStatus OnCheckStatus()
		{
			GameObjectStatus gameObjectStatus = base.OnCheckStatus();
			if (gameObjectStatus != GameObjectStatus.Ready)
				return gameObjectStatus;
			return GameObjectStatus.Ready;
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
					return;
				this._active = value;
				this.PostSetActive(this._active);
			}
		}

		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			InteropMessageID interopMessageId = messageId;
			if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				if (message.ReadString() == "BeamFinished")
				{
					this.m_Finished = true;
					this.m_Succeeded = message.ReadInteger() == 1;
					this.m_Resources = message.ReadInteger();
				}
				return true;
			}
			App.Log.Warn("Unhandled message (id=" + (object)interopMessageId + ").", "combat");
			return base.OnEngineMessage(messageId, message);
		}
	}
}
