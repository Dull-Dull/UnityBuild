﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UnityBuild
{
	class UnityBuildGenerator
	{
		public UnityBuildGenerator( CppProjectManager projManager, string projDirPath, int chunkSize )
		{
			m_projManager = projManager;
			m_projDirPath = projDirPath;
			m_genFolderPath = Path.Combine( m_projDirPath, m_genFileName );
			m_targetChunkSize = chunkSize;

			Directory.CreateDirectory( m_genFolderPath );
			m_projManager.MakeFilter( m_genFileName );

			initFile();
		}

		public void OnEnd()
		{
			if( m_nowChunkSize != 0 )
				createFile();

			m_projManager.Save();
		}

		public void InsertCppFile( FileInfo cppFile )
		{
			if( m_projManager.IsExist( cppFile ) == false )
				return;

			if( cppFile.Name.IndexOf( m_genFileName ) == 0 ||
				cppFile.Name.IndexOf( m_projManager.PreCompiledCppName ) == 0 )
				return;

			m_projManager.UnSetCompile( cppFile );
			m_fileTextLines.Add( $"#include \"..{cppFile.FullName.Substring( m_projDirPath.Length ) }\"" );

			++m_nowChunkSize;
			if( m_nowChunkSize >= m_targetChunkSize )
			{
				createFile();
			}
		}

		private void createFile()
		{
			string targetFullName = Path.Combine( m_genFolderPath, m_genFileName + "_" + m_fileCnt + ".cpp" );
			System.IO.FileInfo file = new System.IO.FileInfo( targetFullName );
			file.Directory.Create();

			if( m_projManager.IsExist( file ) == false )
				m_projManager.AddFile( file, m_genFileName );
			
			System.IO.File.WriteAllLines( file.FullName, m_fileTextLines.ToArray() );

			++m_fileCnt;
		}

		private void initFile()
		{
			m_fileTextLines.Clear();

			if( m_projManager.UsePreCompiled )
			{
				m_fileTextLines.Add( $"#include \"{m_projManager.PreCompiledHeaderName}\"" );
				m_fileTextLines.Add( "" );
			}				
		}

		private CppProjectManager m_projManager = null;
		private string m_projDirPath = "";
		private string m_genFolderPath = "";
		private int m_targetChunkSize = 0;
		private int m_nowChunkSize = 0;

		private int m_fileCnt = 1;
		private List<string> m_fileTextLines = new List<string>();

		private readonly string m_genFileName = "UnityBuild";
	}
}
