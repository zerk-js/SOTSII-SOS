// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarModel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMODEL)]
	internal class StarModel : AutoGameObject, IActive
	{
		private bool _active;

		public int StarSystemDatabaseID { get; set; }

		public float Radius { get; private set; }

		public Vector3 Position { get; private set; }

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

		public StarModel(App app, string modelName, Vector3 position, float radius, bool isInCombat)
		  : this(app, modelName, position, radius, isInCombat, string.Empty, Vector2.Zero, Vector2.Zero, Vector3.Zero, false, string.Empty)
		{
		}

		public StarModel(
		  App app,
		  string modelName,
		  Vector3 position,
		  float radius,
		  bool isInCombat,
		  string impostorMaterialName,
		  Vector2 impostorSpriteScale,
		  Vector2 impostorRange,
		  Vector3 impostorVertexColor,
		  bool impostorEnabled,
		  string name)
		{
			this.Radius = radius;
			this.Position = position;
			app.AddExistingObject((IGameObject)this, (object)modelName, (object)position, (object)radius, (object)isInCombat, (object)impostorMaterialName, (object)impostorSpriteScale, (object)impostorRange, (object)impostorVertexColor, (object)impostorEnabled, (object)name);
		}
	}
}
