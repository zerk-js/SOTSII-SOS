// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.Sky
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SKY)]
	internal class Sky : AutoGameObject, IActive
	{
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

		public Sky(App game, SkyUsage usage, int instance)
		{
			instance = Math.Abs(instance);
			SkyDefinition[] array = game.AssetDatabase.SkyDefinitions.Where<SkyDefinition>((Func<SkyDefinition, bool>)(def => def.Usage == usage)).ToArray<SkyDefinition>();
			string str = nameof(Sky);
			if (array.Length > 0)
				str = array[instance % array.Length].MaterialName;
			game.AddExistingObject((IGameObject)this, (object)str);
		}
	}
}
