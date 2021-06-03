using Harmony;
using System;
using System.IO;
using System.Reflection;

namespace Template
{
	public class TemplateMod : DuckGame.DisabledMod
	{

		public TemplateMod()
		{
#if DEBUG
			System.Diagnostics.Debugger.Launch();
#endif
			AppDomain.CurrentDomain.AssemblyResolve += ModResolve;
		}

		~TemplateMod()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= ModResolve;
		}

		protected override void OnPreInitialize()
		{
			HarmonyInstance.Create( GetType().Namespace ).PatchAll( Assembly.GetExecutingAssembly() );
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
