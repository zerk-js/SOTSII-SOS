// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SwarmerTarget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.Combat
{
	internal class SwarmerTarget
	{
		private int m_NumSwarmersOnTarget;
		private int m_NumGuardiansOnTarget;
		private IGameObject m_Target;

		public int SwarmersOnTarget
		{
			get
			{
				return this.m_NumSwarmersOnTarget;
			}
		}

		public int GuardiansOnTarget
		{
			get
			{
				return this.m_NumGuardiansOnTarget;
			}
		}

		public SwarmerTarget()
		{
			this.m_NumSwarmersOnTarget = 0;
			this.m_NumGuardiansOnTarget = 0;
			this.m_Target = (IGameObject)null;
		}

		public IGameObject Target
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

		public void IncSwarmersOnTarget()
		{
			++this.m_NumSwarmersOnTarget;
		}

		public void IncGuardiansOnTarget()
		{
			++this.m_NumGuardiansOnTarget;
		}

		public void ClearNumTargets()
		{
			this.m_NumSwarmersOnTarget = 0;
			this.m_NumGuardiansOnTarget = 0;
		}
	}
}
