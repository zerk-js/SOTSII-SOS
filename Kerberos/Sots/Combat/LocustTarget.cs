// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.LocustTarget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.GameObjects;

namespace Kerberos.Sots.Combat
{
	internal class LocustTarget
	{
		private int m_NumFightersOnTarget;
		private Ship m_Target;

		public int FightersOnTarget
		{
			get
			{
				return this.m_NumFightersOnTarget;
			}
		}

		public LocustTarget()
		{
			this.m_NumFightersOnTarget = 0;
			this.m_Target = (Ship)null;
		}

		public Ship Target
		{
			get
			{
				return this.m_Target;
			}
			set
			{
				this.m_Target = value;
			}
		}

		public void IncFightersOnTarget()
		{
			++this.m_NumFightersOnTarget;
		}

		public void ClearNumTargets()
		{
			this.m_NumFightersOnTarget = 0;
		}
	}
}
