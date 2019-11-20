// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.FormationPatternData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameObjects
{
	internal class FormationPatternData
	{
		private bool _isLead;
		private Ship _ship;
		private Vector3 _position;

		public bool IsLead
		{
			get
			{
				return this._isLead;
			}
			set
			{
				this._isLead = value;
			}
		}

		public Ship Ship
		{
			get
			{
				return this._ship;
			}
			set
			{
				this._ship = value;
			}
		}

		public Vector3 Position
		{
			get
			{
				return this._position;
			}
			set
			{
				this._position = value;
			}
		}
	}
}
