// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapObject
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.StarMapElements
{
	internal abstract class StarMapObject : AutoGameObject
	{
		public Vector3 Position;
		public float SensorRange;

		public void SetPosition(Vector3 value)
		{
			this.PostSetPosition(value);
			this.Position = value;
		}

		public void SetSensorRange(float value)
		{
			this.PostSetProp("SensorRange", value);
			this.SensorRange = value;
		}

		public void SetIsSelectable(bool value)
		{
			this.PostSetProp("Selectable", value);
		}
	}
}
