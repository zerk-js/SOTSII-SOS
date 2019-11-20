// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.ConsoleResources
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Kerberos.Sots.Console
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class ConsoleResources
	{
		private static ResourceManager resourceMan;
		private static CultureInfo resourceCulture;

		internal ConsoleResources()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals((object)ConsoleResources.resourceMan, (object)null))
					ConsoleResources.resourceMan = new ResourceManager("Kerberos.Sots.Console.ConsoleResources", typeof(ConsoleResources).Assembly);
				return ConsoleResources.resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return ConsoleResources.resourceCulture;
			}
			set
			{
				ConsoleResources.resourceCulture = value;
			}
		}

		internal static string load_tac_targeting_config
		{
			get
			{
				return ConsoleResources.ResourceManager.GetString(nameof(load_tac_targeting_config), ConsoleResources.resourceCulture);
			}
		}
	}
}
