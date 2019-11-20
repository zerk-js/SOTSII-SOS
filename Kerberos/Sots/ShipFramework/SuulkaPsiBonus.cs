// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.SuulkaPsiBonus
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.ShipFramework
{
	internal class SuulkaPsiBonus
	{
		public string Name = "";
		public float[] Rate = new float[20];
		public float[] PsiEfficiency = new float[20];
		public float PsiDrainMultiplyer = 1f;
		public float LifeDrainMultiplyer = 1f;
		public float TKFistMultiplyer = 1f;
		public float CrushMultiplyer = 1f;
		public float FearMultiplyer = 1f;
		public float MovementMultiplyer = 1f;
		public float BioMissileMultiplyer = 1f;
		public SuulkaPsiBonusAbilityType Ability;
		public float ControlDuration;

		public SuulkaPsiBonus()
		{
			for (int index = 0; index <= 19; ++index)
			{
				this.Rate[index] = 1f;
				this.PsiEfficiency[index] = 1f;
			}
		}
	}
}
