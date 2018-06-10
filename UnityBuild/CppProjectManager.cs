using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace UnityBuild
{
	class CppProjectManager
	{
		public CppProjectManager( string projPath )
		{
			m_projPath = projPath;
			m_projDirPath = new FileInfo( m_projPath ).Directory.FullName + @"\";

			m_projDoc.Load( m_projPath );
			m_filterDoc.Load( m_projPath + ".filters" );

			m_projNsMng = new XmlNamespaceManager( m_projDoc.NameTable );
			m_projNsMng.AddNamespace( "ns", m_projDoc.DocumentElement.NamespaceURI );

			m_filterNsMng = new XmlNamespaceManager( m_filterDoc.NameTable );
			m_filterNsMng.AddNamespace( "ns", m_filterDoc.DocumentElement.NamespaceURI );
			
			getSolutionTypes();
			getPreCompiled();
		}

		public void UnSetCompile( FileInfo cppFile )
		{
			string result = cppFile.FullName.Substring( m_projDirPath.Length );
			XmlNode clNode = m_projDoc.SelectSingleNode( $"/ns:Project/ns:ItemGroup/ns:ClCompile[@Include='{result}']", m_projNsMng );

			if( clNode == null )
				return;

			XmlNodeList exclNodeList = m_projDoc.SelectNodes( $"/ns:Project/ns:ItemGroup/ns:ClCompile[@Include='{result}']/ns:ExcludedFromBuild", m_projNsMng );
			if( exclNodeList.Count == m_solutionTypes.Count )
			{
				bool excluded = true;
				for( int i = 0; i < exclNodeList.Count; ++i )
				{
					if( exclNodeList[0].ChildNodes[0].Value != "false" )
					{
						excluded = false;
						break;
					}
				}
				if( excluded == false )
					return;
			}

			Console.WriteLine( $"UnSet Compile : {cppFile.Name}" );

			if( exclNodeList.Count != 0 )
			{
				for( int i = 0; i < exclNodeList.Count; ++i )
					clNode.RemoveChild( exclNodeList[i] );
			}

			foreach( string type in m_solutionTypes )
			{
				XmlElement exclElement = m_projDoc.CreateElement( "ExcludedFromBuild", m_projDoc.DocumentElement.NamespaceURI );
				clNode.AppendChild( exclElement );

				exclElement.SetAttribute( "Condition", $"'$(Configuration)|$(Platform)'=='{type}'" );

				XmlText exclText = m_projDoc.CreateTextNode( "true" );
				exclElement.AppendChild( exclText );
			}			
		}

		public bool IsExist( FileInfo cppFile )
		{
			string filePath = cppFile.FullName.Substring( m_projDirPath.Length );
			XmlNode clNode = m_projDoc.SelectSingleNode( $"/ns:Project/ns:ItemGroup/ns:ClCompile[@Include='{filePath}']", m_projNsMng );

			if( clNode == null )
				return false;

			return true;
		}

		public void AddFile( FileInfo cppFile, string filterName )
		{
			string filePath = cppFile.FullName.Substring( m_projDirPath.Length );
			//Append File To Proj
			XmlNode itemGroupNode = m_projDoc.SelectSingleNode( "/ns:Project/ns:ItemGroup[ns:ClCompile]", m_projNsMng );

			XmlElement clElement = m_projDoc.CreateElement( "ClCompile", m_projDoc.DocumentElement.NamespaceURI );
			clElement.SetAttribute( "Include", filePath );
			itemGroupNode.AppendChild( clElement );

			//Append File To Filter
			itemGroupNode = m_filterDoc.SelectSingleNode( "/ns:Project/ns:ItemGroup[ns:ClCompile]", m_filterNsMng );

			clElement = m_filterDoc.CreateElement( "ClCompile", m_filterDoc.DocumentElement.NamespaceURI );
			itemGroupNode.AppendChild( clElement );
			clElement.SetAttribute( "Include", filePath );

			XmlElement filterElement = m_filterDoc.CreateElement( "Filter", m_filterDoc.DocumentElement.NamespaceURI );
			clElement.AppendChild( filterElement );

			XmlText filterText = m_filterDoc.CreateTextNode( filterName );
			filterElement.AppendChild( filterText );
		}

		public void MakeFilter( string filterName )
		{
			XmlElement root = m_filterDoc.DocumentElement;
			XmlNode filterGroupNode = root.ChildNodes[1];

			var node = m_filterDoc.SelectSingleNode( $"/ns:Project/ns:ItemGroup/ns:Filter[@Include='{filterName}']", m_filterNsMng );
			if( node != null )
				return;

			XmlElement filterElem = m_filterDoc.CreateElement( "Filter", root.NamespaceURI );
			filterElem.SetAttribute( "Include", "UnityBuild" );
			filterGroupNode.AppendChild( filterElem );

			XmlElement uidElem = m_filterDoc.CreateElement( "UniqueIdentifier", root.NamespaceURI );
			filterElem.AppendChild( uidElem );

			XmlText uidText = m_filterDoc.CreateTextNode( "{" + Guid.NewGuid().ToString() + "}" );
			uidElem.AppendChild( uidText );
		}

		public void Save()
		{			
			m_filterDoc.Save( m_projPath + ".filters" );
			m_projDoc.Save( m_projPath );
		}

		public bool UsePreCompiled{
			get
			{
				return m_usePreCompiledHeader;
			}
		}

		public string PreCompiledHeaderName
		{
			get{ return m_preCompiledHeaderFileName; }
		}

		public string PreCompiledCppName
		{
			get { return m_preCompiledCppFileName; }
		}

		private void getSolutionTypes()
		{
			XmlNodeList itemGroupList = m_projDoc.SelectNodes( "/ns:Project/ns:ItemGroup/ns:ProjectConfiguration/@Include", m_projNsMng );

			for( int i = 0; i < itemGroupList.Count; ++i )
			{
				m_solutionTypes.Add( itemGroupList[i].Value );
			}
		}

		private void getPreCompiled()
		{
			XmlNodeList itemGroupNodeList = m_projDoc.SelectNodes( "/ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:PrecompiledHeader[text()='Use']", m_projNsMng );
			if( itemGroupNodeList.Count == 0 )
			{
				m_usePreCompiledHeader = false;
				return;
			}

			m_usePreCompiledHeader = true;

			XmlNodeList preCompiledFileNodeList = m_projDoc.SelectNodes( "/ns:Project/ns:ItemDefinitionGroup/ns:ClCompile/ns:PrecompiledHeaderFile/text()", m_projNsMng );
			if( preCompiledFileNodeList.Count == 0 )
			{
				m_preCompiledHeaderFileName = "StdAfx.h";
			}
			else
			{
				m_preCompiledHeaderFileName = preCompiledFileNodeList[0].Value;				
			}
			int extentionIndex = m_preCompiledHeaderFileName.IndexOf( ".h" );
			m_preCompiledCppFileName = m_preCompiledHeaderFileName.Substring( 0, extentionIndex ) + ".cpp";
		}

		private XmlDocument m_projDoc = new XmlDocument();
		private XmlNamespaceManager m_projNsMng = null;

		private XmlDocument m_filterDoc = new XmlDocument();
		private XmlNamespaceManager m_filterNsMng = null;

		private string m_projPath = "";
		private string m_projDirPath = "";
		private List<string> m_solutionTypes = new List<string>();

		private string m_preCompiledHeaderFileName = "";
		private string m_preCompiledCppFileName = "";
		private bool m_usePreCompiledHeader = false;
	}
}
