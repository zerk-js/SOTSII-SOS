// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.TurretBase
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_TURRETBASE)]
	internal class TurretBase : AutoGameObject
	{
		private Section _section;
		private Module _module;

		public TurretBase(App game, string model, string damageModel, Section section, Module module)
		{
			game.AddExistingObject((IGameObject)this, (object)model, (object)damageModel);
			this._section = section;
			this._module = module;
		}

		public Section AttachedSection
		{
			get
			{
				return this._section;
			}
			set
			{
				this._section = value;
			}
		}

		public Module AttachedModule
		{
			get
			{
				return this._module;
			}
			set
			{
				this._module = value;
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			this._module = (Module)null;
			this._section = (Section)null;
		}
	}
}
