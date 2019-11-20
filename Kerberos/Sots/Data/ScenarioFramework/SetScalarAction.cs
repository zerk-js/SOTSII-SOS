// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ScenarioFramework.SetScalarAction
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.ScenarioFramework
{
	public class SetScalarAction : TriggerAction
	{
		public string Scalar = "";
		internal const string XmlSetScalarAction = "SetScalar";
		private const string XmlScalarName = "Scalar";
		private const string XmlValueName = "Value";
		public float Value;

		public override string XmlName
		{
			get
			{
				return "SetScalar";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Scalar, "Scalar", ref node);
			XmlHelper.AddNode((object)this.Value, "Value", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			this.Scalar = XmlHelper.GetData<string>(node, "Scalar");
			this.Value = XmlHelper.GetData<float>(node, "Value");
		}
	}
}
