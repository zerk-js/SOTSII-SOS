// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.FormationDefinition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_FORMATION)]
	internal class FormationDefinition : GameObject
	{
		public static bool IsAdvancedFormationMovementEnabled;
		public bool HasReceivedAnUpdate;
		private ShipFormation _parentShipFormation;
		public Vector3 m_Facing;
		private Vector3 m_CurrentPosition;
		private List<FormationPatternData> m_formationPattern;

		public ShipFormation ParentShipFormation
		{
			get
			{
				return this._parentShipFormation;
			}
			set
			{
				this._parentShipFormation = value;
			}
		}

		public FormationDefinition()
		{
			this.m_CurrentPosition = new Vector3(0.0f, 0.0f, 0.0f);
			this.m_formationPattern = new List<FormationPatternData>();
			this._parentShipFormation = (ShipFormation)null;
		}

		protected override GameObjectStatus OnCheckStatus()
		{
			GameObjectStatus gameObjectStatus = base.OnCheckStatus();
			if (gameObjectStatus != GameObjectStatus.Ready)
				return gameObjectStatus;
			this.PostSetActive(true);
			return GameObjectStatus.Ready;
		}

		public void GetFormationDimensions(Vector3 vMin, Vector3 vMax)
		{
			vMin = Vector3.One * 1E+15f;
			vMax = -Vector3.One * 1E+15f;
			foreach (FormationPatternData formationPatternData in this.m_formationPattern)
			{
				vMin.X = Math.Min(formationPatternData.Position.X, vMin.X);
				vMin.Y = Math.Min(formationPatternData.Position.Y, vMin.Y);
				vMin.Z = Math.Min(formationPatternData.Position.Z, vMin.Z);
				vMax.X = Math.Max(formationPatternData.Position.X, vMax.X);
				vMax.Y = Math.Max(formationPatternData.Position.Y, vMax.Y);
				vMax.Z = Math.Max(formationPatternData.Position.Z, vMax.Z);
			}
			vMin -= Vector3.One * 200f;
			vMax += Vector3.One * 200f;
		}

		public Vector3 GetFormationPosition()
		{
			return this.m_CurrentPosition;
		}

		public override bool OnEngineMessage(InteropMessageID messageID, ScriptMessageReader message)
		{
			switch (messageID)
			{
				case InteropMessageID.IMID_SCRIPT_MANEUVER_INFO:
					this.m_CurrentPosition.X = message.ReadSingle();
					this.m_CurrentPosition.Y = message.ReadSingle();
					this.m_CurrentPosition.Z = message.ReadSingle();
					this.m_Facing.X = message.ReadSingle();
					this.m_Facing.Y = message.ReadSingle();
					this.m_Facing.Z = message.ReadSingle();
					this.HasReceivedAnUpdate = true;
					return true;
				case InteropMessageID.IMID_SCRIPT_FORMATION_REMOVE_SHIP:
					int shipID = message.ReadInteger();
					if (this._parentShipFormation != null)
						this._parentShipFormation.RemoveShip(shipID);
					return true;
				default:
					App.Log.Warn("Unhandled message (id=" + (object)messageID + ") in FormationDefinition.", "combat");
					return false;
			}
		}
	}
}
