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
			m_projDir = new FileInfo( m_projPath ).Directory.FullName + @"\";

			m_projDoc.Load( m_projPath );
			m_filterDoc.Load( m_projPath + ".filters" );

			m_projNsMng = new XmlNamespaceManager( m_projDoc.NameTable );
			m_projNsMng.AddNamespace( "ns", m_projDoc.DocumentElement.NamespaceURI );

			m_filterNsMng = new XmlNamespaceManager( m_filterDoc.NameTable );
			m_filterNsMng.AddNamespace( "ns", m_filterDoc.DocumentElement.NamespaceURI );

			makeUnityBuildFolder();
			getSolutionTypes();
		}

		public void UnSetCompile( FileInfo cppFile )
		{
			string result = cppFile.FullName.Substring( m_projDir.Length );
			XmlNode clNode = m_projDoc.SelectSingleNode( $"/ns:Project/ns:ItemGroup[2]/ns:ClCompile[@Include='{result}']", m_projNsMng );

			if( clNode == null )
				return;

			XmlNodeList exclNodeList = m_projDoc.SelectNodes( $"/ns:Project/ns:ItemGroup[2]/ns:ClCompile[@Include='{result}']/ns:ExcludedFromBuild", m_projNsMng );
			if( exclNodeList.Count == m_solutionTypes.Count )
				return;

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

		public void AddFile( FileInfo cppFile )
		{

		}

		public void Save()
		{			
			m_filterDoc.Save( m_projPath + ".filters" );
			m_projDoc.Save( m_projPath );
		}


		private void makeUnityBuildFolder()
		{
			XmlElement root = m_filterDoc.DocumentElement;
			XmlNode filterGroupNode = root.ChildNodes[1];

			var node = m_filterDoc.SelectSingleNode( "/ns:Project/ns:ItemGroup[2]/ns:Filter[@Include='UnityBuild']", m_filterNsMng );
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

		private void getSolutionTypes()
		{
			XmlElement root = m_projDoc.DocumentElement;
			XmlNodeList itemGroupList = m_projDoc.SelectNodes( "/ns:Project/ns:ItemGroup/ns:ProjectConfiguration/@Include", m_projNsMng );

			Console.WriteLine( itemGroupList.Count );

			for( int i = 0; i < itemGroupList.Count; ++i )
			{
				m_solutionTypes.Add( itemGroupList[i].Value );
			}
		}

		private XmlDocument m_projDoc = new XmlDocument();
		private XmlNamespaceManager m_projNsMng = null;

		private XmlDocument m_filterDoc = new XmlDocument();
		private XmlNamespaceManager m_filterNsMng = null;

		private string m_projPath = "";
		private string m_projDir = "";
		private List<string> m_solutionTypes = new List<string>();
	}
}
