// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.OrbitPainter
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_ORBITPAINTER)]
	internal class OrbitPainter : GameObject, IActive
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

		public void Add(Matrix orbitTransform)
		{
			this.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_ADD_ORBIT, (object)this.ObjectID, (object)orbitTransform.M11, (object)orbitTransform.M12, (object)orbitTransform.M13, (object)orbitTransform.M14, (object)orbitTransform.M21, (object)orbitTransform.M22, (object)orbitTransform.M23, (object)orbitTransform.M24, (object)orbitTransform.M31, (object)orbitTransform.M32, (object)orbitTransform.M33, (object)orbitTransform.M34, (object)orbitTransform.M41, (object)orbitTransform.M42, (object)orbitTransform.M43, (object)orbitTransform.M44);
		}
	}
}
