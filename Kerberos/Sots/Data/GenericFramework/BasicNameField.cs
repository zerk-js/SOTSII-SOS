// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.GenericFramework.BasicNameField
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.GenericFramework
{
	public class BasicNameField : IXmlLoadSave
	{
		public string Name = "";
		private const string XmlNameName = "Name";

		public override string ToString()
		{
			return this.Name ?? string.Empty;
		}

		public string XmlName
		{
			get
			{
				return "Name";
			}
		}

		public virtual void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Name, "Name", ref node);
		}

		public virtual void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			this.Name = XmlHelper.GetData<string>(node, "Name");
		}
	}
}
