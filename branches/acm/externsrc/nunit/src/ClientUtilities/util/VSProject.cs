#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace NUnit.Util
{
	/// <summary>
	/// The ProjectType enumerator indicates the language of the project.
	/// </summary>
	public enum VSProjectType
	{
		CSharp,
		JSharp,
		VisualBasic,
		CPlusPlus
	}

	/// <summary>
	/// This class allows loading information about
	/// configurations and assemblies in a Visual
	/// Studio project file and inspecting them.
	/// Only the most common project types are
	/// supported and an exception is thrown if
	/// an attempt is made to load an invalid
	/// file or one of an unknown type.
	/// </summary>
	public class VSProject
	{
		#region Static and Instance Variables

		/// <summary>
		/// VS Project extentions
		/// </summary>
		private static readonly string[] validExtensions = { ".csproj", ".vbproj", ".vjsproj", ".vcproj" };
		
		/// <summary>
		/// VS Solution extension
		/// </summary>
		private static readonly string solutionExtension = ".sln";

		/// <summary>
		/// Path to the file storing this project
		/// </summary>
		private string projectPath;

		/// <summary>
		/// Collection of configs for the project
		/// </summary>
		private VSProjectConfigCollection configs;

		/// <summary>
		/// The current project type
		/// </summary>
		private VSProjectType projectType;

		/// <summary>
		/// Indicates whether the project is managed code. This is
		/// always true for C#, J# and VB projects but may vary in
		/// the case of C++ projects.
		/// </summary>
		private bool isManaged;

		#endregion

		#region Constructor

		public VSProject( string projectPath )
		{
			this.projectPath = Path.GetFullPath( projectPath );
			configs = new VSProjectConfigCollection();		

			Load();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the project.
		/// </summary>
		public string Name
		{
			get { return Path.GetFileNameWithoutExtension( projectPath ); }
		}

		/// <summary>
		/// The path to the project
		/// </summary>
		public string ProjectPath
		{
			get { return projectPath; }
		}

		/// <summary>
		/// Our collection of configurations
		/// </summary>
		public VSProjectConfigCollection Configs
		{
			get { return configs; }
		}

		/// <summary>
		/// The current project type
		/// </summary>
		public VSProjectType ProjectType
		{
			get { return projectType; }
		}

		/// <summary>
		/// Indicates whether the project is managed code. This is
		/// always true for C#, J# and VB projects but may vary in
		/// the case of C++ projects.
		/// </summary>
		public bool IsManaged
		{
			get { return isManaged; }
		}

		#endregion

		#region Static Methods

		public static bool IsProjectFile( string path )
		{
			if ( path.IndexOfAny( Path.InvalidPathChars ) >= 0 )
				return false;

			if ( path.ToLower().IndexOf( "http:" ) >= 0 )
				return false;
		
			string extension = Path.GetExtension( path );

			foreach( string validExtension in validExtensions )
				if ( extension == validExtension )
					return true;

			return false;
		}

		public static bool IsSolutionFile( string path )
		{
			return Path.GetExtension( path ) == solutionExtension;
		}

		#endregion

		#region Instance Methods

		private void Load()
		{
			if ( !IsProjectFile( projectPath ) ) 
				ThrowInvalidFileType( projectPath );

			string projectDirectory = Path.GetFullPath( Path.GetDirectoryName( projectPath ) );
			StreamReader rdr = new StreamReader( projectPath, System.Text.Encoding.UTF8 );
			
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( rdr );

				string extension = Path.GetExtension( projectPath );
				string assemblyName = null;

				switch ( extension )
				{
					case ".vcproj":
						this.projectType = VSProjectType.CPlusPlus;

						XmlNode topNode = doc.SelectSingleNode( "/VisualStudioProject" );
						string keyWord = topNode.Attributes["Keyword"].Value;
						this.isManaged = keyWord == "ManagedCProj";

						// TODO: This is all very hacked up... replace it.
						foreach ( XmlNode configNode in doc.SelectNodes( "/VisualStudioProject/Configurations/Configuration" ) )
						{
							string name = configNode.Attributes["Name"].Value;
							string dirName = name;
							int bar = dirName.IndexOf( '|' );
							if ( bar >= 0 )
								dirName = dirName.Substring( 0, bar );
							string outputPath = configNode.Attributes["OutputDirectory"].Value;
							outputPath = outputPath.Replace( "$(SolutionDir)", Path.GetFullPath( Path.GetDirectoryName( projectPath ) ) + @"\" );
							outputPath = outputPath.Replace( "$(ConfigurationName)", dirName );

							string outputDirectory = Path.Combine( projectDirectory, outputPath );
							XmlNode toolNode = configNode.SelectSingleNode( "Tool[@Name='VCLinkerTool']" );
							if ( toolNode != null )
							{
								assemblyName = Path.GetFileName( toolNode.Attributes["OutputFile"].Value );
							}
							else
							{
								toolNode = configNode.SelectSingleNode( "Tool[@Name='VCNMakeTool']" );
								if ( toolNode != null )
									assemblyName = Path.GetFileName( toolNode.Attributes["Output"].Value );
							}

							assemblyName = assemblyName.Replace( "$(OutDir)", outputPath );
							assemblyName = assemblyName.Replace( "$(ProjectName)", this.Name );

							VSProjectConfig config = new VSProjectConfig ( name );
							if ( assemblyName != null )
								config.Assemblies.Add( Path.Combine( outputDirectory, assemblyName ) );
							
							this.configs.Add( config );
						}
					
						break;

					case ".csproj":
						this.projectType = VSProjectType.CSharp;
						this.isManaged = true;
						LoadProject( projectDirectory, doc );
						break;

					case ".vbproj":
						this.projectType = VSProjectType.VisualBasic;
						this.isManaged = true;
						LoadProject( projectDirectory, doc );
						break;

					case ".vjsproj":
						this.projectType = VSProjectType.JSharp;
						this.isManaged = true;
						LoadProject( projectDirectory, doc );
						break;

					default:
						break;
				}
			}
			catch( FileNotFoundException )
			{
				throw;
			}
			catch( Exception e )
			{
				ThrowInvalidFormat( projectPath, e );
			}
			finally
			{
				rdr.Close();
			}
		}

		private bool LoadProject(string projectDirectory, XmlDocument doc)
		{
			bool loaded = LoadVS2003Project(projectDirectory, doc);
			if (loaded) return true;

			loaded = LoadMSBuildProject(projectDirectory, doc);
			if (loaded) return true;

			return false;
		}

		private bool LoadVS2003Project(string projectDirectory, XmlDocument doc)
		{
			XmlNode settingsNode = doc.SelectSingleNode("/VisualStudioProject/*/Build/Settings");
			if (settingsNode == null)
				return false;

			string assemblyName = settingsNode.Attributes["AssemblyName"].Value;
			string outputType = settingsNode.Attributes["OutputType"].Value;

			if (outputType == "Exe" || outputType == "WinExe")
				assemblyName = assemblyName + ".exe";
			else
				assemblyName = assemblyName + ".dll";

			XmlNodeList nodes = settingsNode.SelectNodes("Config");
			if (nodes != null)
				foreach (XmlNode configNode in nodes)
				{
					string name = configNode.Attributes["Name"].Value;
					string outputPath = configNode.Attributes["OutputPath"].Value;
					string outputDirectory = Path.Combine(projectDirectory, outputPath);
					string assemblyPath = Path.Combine(outputDirectory, assemblyName);

					VSProjectConfig config = new VSProjectConfig(name);
					config.Assemblies.Add(assemblyPath);

					configs.Add(config);
				}

			return true;
		}

		private bool LoadMSBuildProject(string projectDirectory, XmlDocument doc)
		{
			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
			namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

			XmlNodeList nodes = doc.SelectNodes("/msbuild:Project/msbuild:PropertyGroup", namespaceManager);
			if (nodes == null) return false;

			XmlElement assemblyNameElement = (XmlElement)doc.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:AssemblyName", namespaceManager);
			string assemblyName = assemblyNameElement.InnerText;

			XmlElement outputTypeElement = (XmlElement)doc.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:OutputType", namespaceManager);
			string outputType = outputTypeElement.InnerText;

			if (outputType == "Exe" || outputType == "WinExe")
				assemblyName = assemblyName + ".exe";
			else
				assemblyName = assemblyName + ".dll";

			foreach (XmlElement configNode in nodes)
			{
				XmlAttribute conditionAttribute = configNode.Attributes["Condition"];
				if (conditionAttribute == null) continue;

				string condition = conditionAttribute.Value;
				string configurationPrefix = " '$(Configuration)|$(Platform)' == '";
				string configurationPostfix = "|AnyCPU' ";
				if (!condition.StartsWith(configurationPrefix) || !condition.EndsWith(configurationPostfix))
					continue;

				string configurationName = condition.Substring(configurationPrefix.Length, condition.Length - configurationPrefix.Length - configurationPostfix.Length);

				XmlElement outputPathElement = (XmlElement)configNode.SelectSingleNode("msbuild:OutputPath", namespaceManager);
				string outputPath = outputPathElement.InnerText;

				string outputDirectory = Path.Combine(projectDirectory, outputPath);
				string assemblyPath = Path.Combine(outputDirectory, assemblyName);

				VSProjectConfig config = new VSProjectConfig(configurationName);
				config.Assemblies.Add(assemblyPath);

				configs.Add(config);
			}

			return true;
		}

		private void ThrowInvalidFileType(string projectPath)
		{
			throw new ArgumentException( 
				string.Format( "Invalid project file type: {0}", 
								Path.GetFileName( projectPath ) ) );
		}

		private void ThrowInvalidFormat( string projectPath, Exception e )
		{
			throw new ArgumentException( 
				string.Format( "Invalid project file format: {0}", 
								Path.GetFileName( projectPath ) ), e );
		}

		#endregion
	}
}