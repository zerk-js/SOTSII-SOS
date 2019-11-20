// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.ScriptHost
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Globalization;
using System.Threading;

namespace Kerberos.Sots.Engine
{
	public static class ScriptHost
	{
		private static object _host = new object();
		private static Thread _thread = ScriptHost.CreateThread(new ParameterizedThreadStart(ScriptHost.ThreadProc));
		private static readonly AutoResetEvent _kickUpdate = new AutoResetEvent(false);
		private static readonly ManualResetEvent _readyForNextUpdate = new ManualResetEvent(true);
		private static Log _log;
		private static bool _allowConsole;
		private static bool _quit;

		private static void ConditionThread(Thread value)
		{
			value.CurrentUICulture = value.CurrentCulture;
			value.CurrentCulture = CultureInfo.InvariantCulture;
		}

		internal static Thread CreateThread(ThreadStart start)
		{
			Thread thread = new Thread(start);
			ScriptHost.ConditionThread(thread);
			return thread;
		}

		internal static Thread CreateThread(ParameterizedThreadStart start)
		{
			Thread thread = new Thread(start);
			ScriptHost.ConditionThread(thread);
			return thread;
		}

		static ScriptHost()
		{
			ScriptHost.ConditionThread(Thread.CurrentThread);
		}

		internal static Log Log
		{
			get
			{
				return ScriptHost._log;
			}
		}

		public static bool AllowConsole
		{
			get
			{
				return ScriptHost._allowConsole;
			}
		}

		public static IFileSystem FileSystem { get; private set; }

		public static IEngine Engine { get; private set; }

		public static string TwoLetterISOLanguageName { get; private set; }

		private static void ThreadProc(object scriptHostParams)
		{
			ScriptHostParams scriptHostParams1 = (ScriptHostParams)scriptHostParams;
			ScriptHost.FileSystem = scriptHostParams1.FileSystem;
			ScriptHost.Engine = scriptHostParams1.Engine;
			ScriptHost.TwoLetterISOLanguageName = scriptHostParams1.TwoLetterISOLanguageName;
			ScriptHost._thread.Priority = ThreadPriority.Normal;
			ScriptHost._thread.Name = "SotsScript";
			App app = new App(scriptHostParams1);
			bool flag = false;
			while (!flag)
			{
				lock (ScriptHost._host)
				{
					if (ScriptHost._quit)
						flag = true;
				}
				ScriptHost._readyForNextUpdate.Set();
				if (!flag)
				{
					ScriptHost._kickUpdate.WaitOne();
					ScriptHost._readyForNextUpdate.Reset();
					app.Update();
				}
			}
			app.Exiting();
		}

		public static bool Localize(string text, out string localized)
		{
			return AssetDatabase.CommonStrings.Localize(text, out localized);
		}

		public static void Load(ScriptHostParams p)
		{
			ScriptHost._log = new Log(p.LogHost);
			ScriptHost._allowConsole = p.AllowConsole;
			if (ScriptHost._thread.IsAlive)
				throw new InvalidOperationException("Previous thread is still alive.");
			ScriptHost._thread.Start((object)p);
		}

		public static void Update(bool waitForCompletion)
		{
			ScriptHost._readyForNextUpdate.WaitOne();
			ScriptHost._kickUpdate.Set();
			if (!waitForCompletion)
				return;
			ScriptHost._readyForNextUpdate.WaitOne();
		}

		public static void Exit()
		{
			lock (ScriptHost._host)
				ScriptHost._quit = true;
			ScriptHost._readyForNextUpdate.WaitOne();
			ScriptHost._kickUpdate.Set();
		}

		public static bool Exited()
		{
			return !ScriptHost._thread.IsAlive;
		}
	}
}
