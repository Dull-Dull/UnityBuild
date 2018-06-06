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
			if( args.Length < 3 )
			{
				Console.WriteLine( "[0] ProjectPath, [1] TargetPath, [2] ChunkSize" );
				return;
			}
				

			FileInfo fileInfo = new FileInfo( args[0] );
			FileInfoIterator fileIter = new FileInfoIterator( fileInfo.Directory.FullName );
			UnityBuildGenerator generator = new UnityBuildGenerator( 
					new CppProjectManager( fileInfo.FullName ),
					args[1], int.Parse( args[2] ) ); 

			for( FileInfo item = fileIter.Next(); item != null;  item = fileIter.Next() )
			{
				if( item.Extension == "cpp" )
				{
					generator.InsertCppFile( item );
				}
			}
		}
	}
}
