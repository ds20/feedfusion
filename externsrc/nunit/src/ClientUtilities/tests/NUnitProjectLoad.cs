#region Copyright (c) 2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
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

using System.IO;
using NUnit.Framework;
using NUnit.TestUtilities;

namespace NUnit.Util.Tests
{
	// TODO: Some of these tests are really tests of VSProject and should be moved there.

	[TestFixture]
	public class NUnitProjectLoad
	{
		static readonly string xmlfile = "test.nunit";
		static readonly string resourceDir = "resources";

		private NUnitProject project;

		[SetUp]
		public void SetUp()
		{
			project = NUnitProject.EmptyProject();
		}

		[TearDown]
		public void TearDown()
		{
			if ( File.Exists( xmlfile ) )
				File.Delete( xmlfile );
		}

		// Write a string out to our xml file and then load project from it
		private void LoadProject( string source )
		{
			StreamWriter writer = new StreamWriter( xmlfile );
			writer.Write( source );
			writer.Close();

			project.ProjectPath = Path.GetFullPath( xmlfile );
			project.Load();
		}

		[Test]
		public void LoadEmptyProject()
		{
			LoadProject( NUnitProjectXml.EmptyProject );
			Assert.AreEqual( 0, project.Configs.Count );
		}

		[Test]
		public void LoadEmptyConfigs()
		{
			LoadProject( NUnitProjectXml.EmptyConfigs );
			Assert.AreEqual( 2, project.Configs.Count );
			Assert.IsTrue( project.Configs.Contains( "Debug") );
			Assert.IsTrue( project.Configs.Contains( "Release") );
		}

		[Test]
		public void LoadNormalProject()
		{
			LoadProject( NUnitProjectXml.NormalProject );
			Assert.AreEqual( 2, project.Configs.Count );

			ProjectConfig config1 = project.Configs["Debug"];
			Assert.AreEqual( 2, config1.Assemblies.Count );
			Assert.AreEqual( Path.GetFullPath( @"bin" + Path.DirectorySeparatorChar + "debug" + Path.DirectorySeparatorChar + "assembly1.dll" ), config1.Assemblies[0].FullPath );
			Assert.AreEqual( Path.GetFullPath( @"bin" + Path.DirectorySeparatorChar + "debug" + Path.DirectorySeparatorChar + "assembly2.dll" ), config1.Assemblies[1].FullPath );

			ProjectConfig config2 = project.Configs["Release"];
			Assert.AreEqual( 2, config2.Assemblies.Count );
			Assert.AreEqual( Path.GetFullPath( @"bin" + Path.DirectorySeparatorChar + "release" + Path.DirectorySeparatorChar + "assembly1.dll" ), config2.Assemblies[0].FullPath );
			Assert.AreEqual( Path.GetFullPath( @"bin" + Path.DirectorySeparatorChar + "release" + Path.DirectorySeparatorChar + "assembly2.dll" ), config2.Assemblies[1].FullPath );
		}

		[Test]
		public void LoadProjectWithManualBinPath()
		{
			LoadProject( NUnitProjectXml.ManualBinPathProject );
			Assert.AreEqual( 1, project.Configs.Count );
			ProjectConfig config1 = project.Configs["Debug"];
			Assert.AreEqual( "bin_path_value", config1.PrivateBinPath );
		}

		[Test]
		public void FromAssembly()
		{
			NUnitProject project = NUnitProject.FromAssembly( "nunit.util.tests.dll" );
			Assert.AreEqual( "Default", project.ActiveConfigName );
			Assert.AreEqual( Path.GetFullPath( "nunit.util.tests.dll" ), project.ActiveConfig.Assemblies[0].FullPath );
			Assert.IsTrue( project.IsLoadable, "Not loadable" );
			Assert.IsTrue( project.IsAssemblyWrapper, "Not wrapper" );
			Assert.IsFalse( project.IsDirty, "Not dirty" );
		}

		[Test]
		public void SaveClearsAssemblyWrapper()
		{
			NUnitProject project = NUnitProject.FromAssembly( "nunit.util.tests.dll" );
			project.Save( xmlfile );
			Assert.IsFalse( project.IsAssemblyWrapper,
				"Changed project should no longer be wrapper");
		}

		private void AssertCanLoadVsProject( string resourceName )
		{
			string fileName = Path.GetFileNameWithoutExtension( resourceName );
			using( TempResourceFile file = new TempResourceFile( this.GetType(), resourceDir + "." + resourceName, resourceName ) )
			{
				NUnitProject project = NUnitProject.FromVSProject( file.Path );
				Assert.AreEqual( fileName, project.Name );
				Assert.AreEqual( project.Configs[0].Name, project.ActiveConfigName );
				Assert.AreEqual( fileName.ToLower(), Path.GetFileNameWithoutExtension( project.Configs[0].Assemblies[0].FullPath.ToLower() ) );
				Assert.IsTrue( project.IsLoadable, "Not loadable" );
				Assert.IsFalse( project.IsDirty, "Project should not be dirty" );
			}
		}

		[Test]
		public void FromCSharpProject()
		{
			AssertCanLoadVsProject( "csharp-sample.csproj" );
		}

		[Test]
		public void FromVBProject()
		{
			AssertCanLoadVsProject( "vb-sample.vbproj" );
		}

		[Test]
		public void FromJsharpProject()
		{
			AssertCanLoadVsProject( "jsharp.vjsproj" );
		}

		[Test]
		public void FromCppProject()
		{
			AssertCanLoadVsProject( "cpp-sample.vcproj" );
		}

		[Test]
		public void FromProjectWithHebrewFileIncluded()
		{
			AssertCanLoadVsProject( "HebrewFileProblem.csproj" );
		}

		[Test]
		public void FromVSSolution2003()
		{
			using(new TempResourceFile(this.GetType(), "resources.csharp-sample.csproj", @"csharp\csharp-sample.csproj"))
			using(new TempResourceFile(this.GetType(), "resources.jsharp.vjsproj", @"jsharp\jsharp.vjsproj"))
			using(new TempResourceFile(this.GetType(), "resources.vb-sample.vbproj", @"vb\vb-sample.vbproj"))
			using(new TempResourceFile(this.GetType(), "resources.cpp-sample.vcproj", @"cpp-sample\cpp-sample.vcproj"))
			using(TempResourceFile file = new TempResourceFile(this.GetType(), "resources.samples.sln", "samples.sln" ))
			{
				NUnitProject project = NUnitProject.FromVSSolution( file.Path );
				Assert.AreEqual( 4, project.Configs.Count );
				Assert.AreEqual( 3, project.Configs["Debug"].Assemblies.Count );
				Assert.AreEqual( 3, project.Configs["Release"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Debug|Win32"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Release|Win32"].Assemblies.Count );
				Assert.IsTrue( project.IsLoadable, "Not loadable" );
				Assert.IsFalse( project.IsDirty, "Project should not be dirty" );
			}
		}

		[Test]
		public void FromVSSolution2005()
		{
			using(new TempResourceFile(this.GetType(), "resources.csharp-sample_VS2005.csproj", @"csharp\csharp-sample_VS2005.csproj"))
			using(new TempResourceFile(this.GetType(), "resources.jsharp_VS2005.vjsproj", @"jsharp\jsharp_VS2005.vjsproj"))
			using(new TempResourceFile(this.GetType(), "resources.vb-sample_VS2005.vbproj", @"vb\vb-sample_VS2005.vbproj"))
			using(new TempResourceFile(this.GetType(), "resources.cpp-sample_VS2005.vcproj", @"cpp-sample\cpp-sample_VS2005.vcproj"))
			using(TempResourceFile file = new TempResourceFile(this.GetType(), "resources.samples_VS2005.sln", "samples_VS2005.sln"))
			{
				NUnitProject project = NUnitProject.FromVSSolution( file.Path );
				Assert.AreEqual( 4, project.Configs.Count );
				Assert.AreEqual( 3, project.Configs["Debug"].Assemblies.Count );
				Assert.AreEqual( 3, project.Configs["Release"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Debug|Win32"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Release|Win32"].Assemblies.Count );
				Assert.IsTrue( project.IsLoadable, "Not loadable" );
				Assert.IsFalse( project.IsDirty, "Project should not be dirty" );
			}
		}

		[Test]
		public void FromWebApplication()
		{
			using( new TempResourceFile(this.GetType(), "resources.ClassLibrary1.csproj", @"ClassLibrary1\ClassLibrary1.csproj" ) )
			using( TempResourceFile file = new TempResourceFile( this.GetType(), "resources.WebApplication1.sln", "WebApplication1.sln" ) )
			{
				NUnitProject project = NUnitProject.FromVSSolution( Path.GetFullPath( file.Path ) );
				Assert.AreEqual( 2, project.Configs.Count );
				Assert.AreEqual( 1, project.Configs["Debug"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Release"].Assemblies.Count );
			}
		}

		[Test]
		public void WithUnmanagedCpp()
		{
			using( new TempResourceFile( this.GetType(), "resources.ClassLibrary1.csproj", @"ClassLibrary1\ClassLibrary1.csproj" ) )
			using( new TempResourceFile( this.GetType(), "resources.Unmanaged.vcproj", @"Unmanaged\Unmanaged.vcproj" ) )
			using( TempResourceFile file = new TempResourceFile( this.GetType(), "resources.Solution1.sln", "Solution1.sln" ) ) 
			{
				NUnitProject project = NUnitProject.FromVSSolution( file.Path );
				Assert.AreEqual( 4, project.Configs.Count );
				Assert.AreEqual( 1, project.Configs["Debug"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Release"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Debug|Win32"].Assemblies.Count );
				Assert.AreEqual( 1, project.Configs["Release|Win32"].Assemblies.Count );
				Assert.IsTrue( project.Configs["Debug"].Assemblies[0].HasTests );
				Assert.IsTrue( project.Configs["Release"].Assemblies[0].HasTests );
				Assert.IsFalse( project.Configs["Debug|Win32"].Assemblies[0].HasTests );
				Assert.IsFalse( project.Configs["Release|Win32"].Assemblies[0].HasTests );
			}
		}

		[Test]
		public void FromMakefileProject()
		{
			using( TempResourceFile file = new TempResourceFile( this.GetType(), "resources.MakeFileProject.vcproj", "MakeFileProject.vcproj" ) )
			{
				NUnitProject project = NUnitProject.FromVSProject( file.Path );
				Assert.AreEqual( 2, project.Configs.Count );
				Assert.AreEqual( 1, project.Configs["Debug|Win32"].Assemblies.Count );
				Assert.IsFalse( project.Configs["Debug|Win32"].Assemblies[0].HasTests );
				Assert.AreEqual( 1, project.Configs["Release|Win32"].Assemblies.Count );
				Assert.IsFalse( project.Configs["Release|Win32"].Assemblies[0].HasTests );
			}
		}
	}
}
