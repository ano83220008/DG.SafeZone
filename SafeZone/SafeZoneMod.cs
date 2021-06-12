using DuckGame;
using Harmony;
using System;
using System.IO;
using System.Reflection;

namespace SafeZone
{
	public class SafeZoneMod : DuckGame.DisabledMod
	{

		public SafeZoneMod()
		{
#if DEBUG
			System.Diagnostics.Debugger.Launch();
#endif
			AppDomain.CurrentDomain.AssemblyResolve += ModResolve;
		}

		~SafeZoneMod()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= ModResolve;
		}

		protected override void OnPreInitialize()
		{
			HarmonyInstance.Create( GetType().Namespace ).PatchAll( Assembly.GetExecutingAssembly() );

			TeamSelect2.matchSettings.Add(new MatchSetting()
			{
				max = 999,
				min = 0,
				value = 30,
				name = "Time Limit",
				id = "timelimit",
			});

			TeamSelect2.matchSettings.Add(new MatchSetting()
			{
				max = 999,
				min = 0,
				value = 10,
				name = "Zone Start",
				id = "zonestart",
			});

			TeamSelect2.matchSettings.Add(new MatchSetting()
			{
				max = 9999,
				min = 0,
				value = 200,
				name = "Zone Size",
				id = "zonesize",
			});
		}

		private Assembly ModResolve( object sender , ResolveEventArgs args )
		{
			string dllFolder = Path.GetFileNameWithoutExtension( GetType().Assembly.ManifestModule?.ScopeName ?? GetType().Namespace );
			string cleanName = args.Name.Split( ',' ) [0];
			//now try to load the requested assembly


			string assemblyFolder = Path.Combine( configuration.directory , dllFolder , "Output" , "net471" );
			string assemblyPath = Path.GetFullPath( Path.Combine( assemblyFolder , cleanName + ".dll" ) );

			if( File.Exists( assemblyPath ) )
			{
				byte [] assemblyBytes = File.ReadAllBytes( assemblyPath );

				return Assembly.Load( assemblyBytes );
			}

			return null;
		}

	}
}
