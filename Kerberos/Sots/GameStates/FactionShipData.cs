// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.FactionShipData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.PlayerFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	internal class FactionShipData
	{
		public readonly List<ClassShipData> Classes = new List<ClassShipData>();
		public Faction Faction;
		private ClassShipData _selectedClass;

		public ClassShipData SelectedClass
		{
			get
			{
				ClassShipData selectedClass = this._selectedClass;
				if (selectedClass != null)
					return selectedClass;
				if (this.Classes.Count <= 0)
					return (ClassShipData)null;
				return this.Classes[0];
			}
			set
			{
				this._selectedClass = value;
			}
		}
	}
}
