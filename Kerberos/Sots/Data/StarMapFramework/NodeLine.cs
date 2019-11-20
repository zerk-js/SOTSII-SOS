// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.NodeLine
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class NodeLine : IXmlLoadSave
	{
		public bool isPermanent = true;
		internal const string XmlNodeLineName = "NodeLine";
		internal const string XmlSystemAName = "SystemA";
		internal const string XmlSystemBName = "SystemB";
		internal const string XmlIsPermanent = "isPermanent";
		public int SystemA;
		public int SystemB;

		public virtual string XmlName
		{
			get
			{
				return nameof(NodeLine);
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.SystemA, "SystemA", ref node);
			XmlHelper.AddNode((object)this.SystemB, "SystemB", ref node);
			XmlHelper.AddNode((object)this.isPermanent, "isPermanent", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.SystemA = XmlHelper.GetData<int>(node, "SystemA");
			this.SystemB = XmlHelper.GetData<int>(node, "SystemB");
			this.isPermanent = XmlHelper.GetData<bool>(node, "isPermanent");
		}
	}
}
