// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.CombatGrid
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_COMBATGRID)]
	internal class CombatGrid : GameObject, IActive
	{
		private float _gridSize = 10f;
		private float _cellSize = 1f;
		private bool _active;

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

		public float GridSize
		{
			get
			{
				return this._gridSize;
			}
			set
			{
				if ((double)value == (double)this._gridSize)
					return;
				this._gridSize = value;
				this.PostSetProp(nameof(GridSize), this._gridSize);
			}
		}

		public float CellSize
		{
			get
			{
				return this._cellSize;
			}
			set
			{
				if ((double)value == (double)this._cellSize)
					return;
				this._cellSize = value;
				this.PostSetProp(nameof(CellSize), this._cellSize);
			}
		}
	}
}
