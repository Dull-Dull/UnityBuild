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

			parseIgnorList();
			deleteUnityBuildFiles();

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

		public void InsertCppFile( string cppFile )
		{
			FileInfo cppFileInfo = new FileInfo( Path.Combine( m_projDirPath, cppFile ) );
			if( cppFileInfo.Name.ToLower().IndexOf( m_genFileName.ToLower() ) == 0 ||
				cppFileInfo.Name.ToLower().IndexOf( m_projManager.PreCompiledCppName.ToLower() ) == 0 ||
				m_ignorFileCon.Contains( cppFile.ToLower() ) )
				return;

			modifyCppFile( cppFileInfo );

			Console.WriteLine( $"Process : {cppFileInfo.Name}" );
			m_projManager.UnSetCompile( cppFile );
			m_fileTextLines.Add( $"#include \"..\\{cppFile}\"" );

			++m_nowChunkSize;
			if( m_nowChunkSize >= m_targetChunkSize )
			{
				createFile();
			}
		}

		private void createFile()
		{
			string targetFileName = m_genFileName + "_" + m_fileCnt + ".cpp";
			string targetFullPath = Path.Combine( m_genFolderPath, targetFileName );
			System.IO.FileInfo file = new System.IO.FileInfo( targetFullPath );
			file.Directory.Create();

			Console.WriteLine( $"Create UnityBuild File : {targetFileName}" );

			if( m_projManager.IsExist( file ) == false )
			{
				m_projManager.AddFile( file, m_genFileName );
				Console.WriteLine( $"Add File To Project : {m_genFileName}" );
			}
			
			System.IO.File.WriteAllLines( file.FullName, m_fileTextLines.ToArray() );
			Console.WriteLine( $"Write File : {targetFileName}" );

			++m_fileCnt;

			initFile();
			m_nowChunkSize = 0;
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

		private void parseIgnorList()
		{
			const string ignoreFile = @".\UnityIgnore.conf";

			if( File.Exists( ignoreFile ) )
			{
				var lines = File.ReadAllLines( ignoreFile );
				foreach( string line in lines )
				{
					m_ignorFileCon.Add( line.ToLower() );
				}				
			}
		}

		private void deleteUnityBuildFiles()
		{
			DirectoryInfo unityBuildFolder = new DirectoryInfo( m_genFolderPath );
			if( unityBuildFolder.Exists )
			{
				FileInfo[] files = unityBuildFolder.GetFiles();

				foreach( FileInfo info in files )
				{
					m_projManager.DeleteFile( info );
					info.Delete();
				}
			}
		}

		private void modifyCppFile( FileInfo cppFile )
		{
			List<string> cppTextLines = new List<string>( File.ReadAllLines( cppFile.FullName, Encoding.GetEncoding( 51949 ) ) );

			//erase include precompile
			if( m_projManager.UsePreCompiled )
			{
				cppTextLines.RemoveAll( ( line ) => ( line.ToLower().IndexOf( "#include" ) != -1 &&
				line.ToLower().IndexOf( m_projManager.PreCompiledHeaderName.ToLower() ) != -1 ) );
			}

			//erase comment
			cppTextLines.RemoveAll( ( line ) => ( line.ToLower().IndexOf( $"//{m_genFileName}_" ) != -1 ) );

			//write comment
			cppTextLines.Insert( 0, $"//{m_genFileName}_{m_fileCnt}.cpp" );

			File.WriteAllLines( cppFile.FullName, cppTextLines, Encoding.GetEncoding( 51949 ) );
		}

		private CppProjectManager m_projManager = null;
		private string m_projDirPath = "";
		private string m_genFolderPath = "";
		private int m_targetChunkSize = 0;
		private int m_nowChunkSize = 0;

		private int m_fileCnt = 1;
		private List<string> m_fileTextLines = new List<string>();

		private readonly string m_genFileName = "UnityBuild";

		private HashSet<string> m_ignorFileCon = new HashSet<string>();
	}
}
