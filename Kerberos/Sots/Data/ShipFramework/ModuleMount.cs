// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.ModuleMount
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class ModuleMount : IXmlLoadSave
	{
		internal static readonly string XmlNameModule = "Module";
		private static readonly string XmlIdName = nameof(Id);
		private static readonly string XmlAssignedModuleName = "AssignedModule";
		private static readonly string XmlNodeNameName = nameof(NodeName);
		private static readonly string XmlSizeName = nameof(Size);
		private static readonly string XmlTypeName = nameof(Type);
		private static readonly string XmlFrameXName = nameof(FrameX);
		private static readonly string XmlFrameYName = nameof(FrameY);
		public string Id = string.Empty;
		public string Size = "Cruiser";
		public string Type = "";
		public string AssignedModuleName;
		public string NodeName;
		public int FrameX;
		public int FrameY;

		public ModuleMount()
		{
			if (!ToolEnvironment.IsRunningInTool)
				return;
			this.Id = Guid.NewGuid().ToString();
		}

		public string XmlName
		{
			get
			{
				return ModuleMount.XmlNameModule;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.Id, ModuleMount.XmlIdName, ref node);
			XmlHelper.AddNode((object)this.AssignedModuleName, ModuleMount.XmlAssignedModuleName, ref node);
			XmlHelper.AddNode((object)this.NodeName, ModuleMount.XmlNodeNameName, ref node);
			XmlHelper.AddNode((object)this.Size, ModuleMount.XmlSizeName, ref node);
			XmlHelper.AddNode((object)this.Type, ModuleMount.XmlTypeName, ref node);
			XmlHelper.AddNode((object)this.FrameX, ModuleMount.XmlFrameXName, ref node);
			XmlHelper.AddNode((object)this.FrameY, ModuleMount.XmlFrameYName, ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.Id = XmlHelper.GetData<string>(node, ModuleMount.XmlIdName);
			this.AssignedModuleName = XmlHelper.GetData<string>(node, ModuleMount.XmlAssignedModuleName);
			this.NodeName = XmlHelper.GetData<string>(node, ModuleMount.XmlNodeNameName);
			this.Size = XmlHelper.GetData<string>(node, ModuleMount.XmlSizeName);
			this.Type = XmlHelper.GetData<string>(node, ModuleMount.XmlTypeName);
			this.FrameX = XmlHelper.GetData<int>(node, ModuleMount.XmlFrameXName);
			this.FrameY = XmlHelper.GetData<int>(node, ModuleMount.XmlFrameYName);
		}
	}
}
