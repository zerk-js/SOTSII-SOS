// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.Psionic
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;

namespace Kerberos.Sots.ShipFramework
{
	[GameObjectType(InteropGameObjectType.IGOT_PSIONIC)]
	internal class Psionic : GameObject
	{
		private bool _isActive;
		private float _percentConsumed;

		public SectionEnumerations.PsionicAbility Type { get; set; }

		public bool IsActive
		{
			get
			{
				return this._isActive;
			}
		}

		public void Activate()
		{
			if (this._isActive)
				return;
			this.PostSetProp("ActivatePsionic");
		}

		public void Deactivate()
		{
			if (!this._isActive)
				return;
			this.PostSetProp("ReleasePsionic");
		}

		public float PercentConsumed
		{
			get
			{
				return this._percentConsumed;
			}
		}

		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				string str = message.ReadString();
				if (str == "Update")
				{
					this._percentConsumed = message.ReadSingle();
					return true;
				}
				if (str == "IsActive")
				{
					this._isActive = message.ReadBool();
					return true;
				}
			}
			return false;
		}
	}
}
