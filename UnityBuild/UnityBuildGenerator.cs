using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UnityBuild
{
	class UnityBuildGenerator
	{
		public UnityBuildGenerator( CppProjectManager projManager, string targetPath, int chunkSize )
		{
			m_projManager = projManager;
			m_targetPath = targetPath;
			m_targetChunkSize = chunkSize;
		}

		public void InsertCppFile( FileInfo cppFile )
		{
			if( cppFile.Name.IndexOf( m_genFileName ) == 0 )
				return;

			m_projManager.UnSetCompile( cppFile );
			//여기서 cpp파일을 작성해야 함...

			++m_nowChunkSize;
			if( m_nowChunkSize >= m_targetChunkSize )
			{
				CreateFile();
			}
		}

		private void CreateFile()
		{
			string targetFullName = m_targetPath + m_genFileName + m_fileCnt + "cpp";
			System.IO.FileInfo file = new System.IO.FileInfo( targetFullName );

			if( file.Exists == false )
			{
				file.Directory.Create();
				m_projManager.AddFile( file );
			}
			System.IO.File.WriteAllText( file.FullName, m_fileText );

			++m_fileCnt;
		}

		private CppProjectManager m_projManager = null;
		private string m_targetPath = "";
		private int m_targetChunkSize = 0;
		private int m_nowChunkSize = 0;

		private int m_fileCnt = 1;
		private string m_fileText = "";

		private readonly string m_genFileName = "__UnityBuild";
	}
}
