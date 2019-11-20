// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.TriggerCondition
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class TriggerCondition : IXmlLoadSave
	{
		internal bool isEventDriven;

		public virtual string XmlName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual void AttachToXmlNode(ref XmlElement node)
		{
		}

		public virtual void LoadFromXmlNode(XmlElement node)
		{
		}
	}
}
