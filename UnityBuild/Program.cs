using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UnityBuild
{
	class Program
	{
		static void Main( string[] args )
		{
			if( args.Length < 2 )
			{
				Console.WriteLine( "[0] ProjectPath, [1] ChunkSize" );
				return;
			}

			Console.WriteLine( "===== Start Unity Build =====" );
			Console.WriteLine( "Target : " + args[0] );
			Console.WriteLine( "ChunkSize : " + args[1] );
			Console.WriteLine( "" );

			FileInfo fileInfo = new FileInfo( args[0] );

			CppProjectManager projManager = new CppProjectManager( fileInfo.FullName );
			UnityBuildGenerator generator = new UnityBuildGenerator(
					projManager, fileInfo.DirectoryName, int.Parse( args[1] ) );

			List<string> cppFileCon = projManager.GetCppFilePathCon();

			foreach( string cppFilePath in cppFileCon )
			{
				generator.InsertCppFile( cppFilePath );
			}

			generator.OnEnd();

			Console.WriteLine( "\n===== End Unity Build =====" );
		}
	}
}
