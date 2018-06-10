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
			FileInfoIterator fileIter = new FileInfoIterator( fileInfo.DirectoryName );
			UnityBuildGenerator generator = new UnityBuildGenerator( 
					new CppProjectManager( fileInfo.FullName ),
					fileInfo.DirectoryName, int.Parse( args[1] ) );

			for( FileInfo item = fileIter.Next(); item != null; item = fileIter.Next() )
			{
				if( item.Extension == ".cpp" )
				{
					generator.InsertCppFile( item );
				}
			}

			generator.OnEnd();

			Console.WriteLine( "\n===== End Unity Build =====" );
		}
	}
}
