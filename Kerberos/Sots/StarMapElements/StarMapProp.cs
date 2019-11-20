// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapProp
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPPROP)]
	internal class StarMapProp : StarMapObject
	{
		public StarMapProp(
		  App game,
		  string modelName,
		  Vector3 position,
		  Vector3 eulerRotation,
		  float scale)
		{
			game.AddExistingObject((IGameObject)this, (object)modelName);
			this.PostSetPosition(position);
			this.PostSetRotation(Vector3.RadiansToDegrees(eulerRotation));
			this.PostSetScale(scale);
		}
	}
}
