#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright � 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright � 2000-2003 Philip A. Craig
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
' Portions Copyright � 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright � 2000-2003 Philip A. Craig
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
using System.Collections;
using System.Reflection;

namespace NUnit.Core.Builders
{
	/// <summary>
	/// Class that builds a TestAssembly suite from an assembly
	/// </summary>
	public class TestAssemblyBuilder
	{
		#region Instance Fields

		/// <summary>
		/// The file path of the assembly this builder is building from
		/// </summary>
		string assemblyName;

		/// <summary>
		/// The internal key for this assembly 
		/// </summary>
		int assemblyKey;

		/// <summary>
		/// The loaded assembly
		/// </summary>
		Assembly assembly;

		/// <summary>
		/// The TestFramework used by the loaded assembly
		/// </summary>
		ITestFramework testFramework = null;

		/// <summary>
		/// Our LegacySuite builder, which is only used when a 
		/// fixture has been passed by name on the command line.
		/// </summary>
		ISuiteBuilder legacySuiteBuilder;

		/// <summary>
		/// Set to true to build automatic namespace suites,
		/// false to build a single suite with the fixtures.
		/// </summary>
		bool autoNamespaceSuites = true;

		#endregion

		#region Properties

		public ITestFramework Framework
		{
			get { return testFramework; }
		}

		public bool AutoNamespaceSuites
		{
			get { return autoNamespaceSuites; }
			set { autoNamespaceSuites = value; }
		}

		#endregion

		#region Constructor

		public TestAssemblyBuilder( string assemblyName, int assemblyKey, bool autoNamespaceSuites )
		{
			this.assemblyName = assemblyName;
			this.assemblyKey = assemblyKey;
			this.autoNamespaceSuites = autoNamespaceSuites;

			// TODO: Keeping this separate till we can make
			//it work in all situations.
			legacySuiteBuilder = new NUnit.Core.Builders.LegacySuiteBuilder();
		}

		#endregion

		#region ISuiteBuilder Members

		public bool CanBuildFrom(Type type)
		{
			return Addins.CanBuildFrom( type ) 
				|| Builtins.CanBuildFrom( type );
		}

		public TestSuite BuildFrom(Type type, int assemblyKey)
		{
			TestSuite suite = Addins.BuildFrom( type, assemblyKey );
			if ( suite == null )
				suite = Builtins.BuildFrom( type, assemblyKey );

			return suite;
		}

		#endregion

		#region Other Public Methods

		public TestSuite Build( string testName )
		{
			if ( testName == null || testName == string.Empty )
				return Build();

			this.assembly = Load( this.assemblyName );
			if ( assembly == null ) return null;

			// If provided test name is actually a fixture,
			// just build and return that!
			Type testType = assembly.GetType(testName);
			if( testType != null )
				return BuildSingleFixture( testType );
		
			// Assume that testName is a namespace and get all fixtures in it
			IList fixtures = GetFixtures( assembly, testName, assemblyKey );
			if ( fixtures.Count > 0 ) 
				return BuildTestAssembly( fixtures );
			return null;
		}

		public TestSuite Build()
		{
			this.assembly = Load( this.assemblyName );
			if ( this.assembly == null ) return null;

			IList fixtures = GetFixtures( assembly, null, assemblyKey );
			return BuildTestAssembly( fixtures );
		}

		private TestAssembly BuildTestAssembly( IList fixtures )
		{
			TestAssembly testAssembly = CreateTestAssembly(this.assemblyName, this.assemblyKey);

			if ( autoNamespaceSuites )
			{
				NamespaceTreeBuilder treeBuilder = 
					new NamespaceTreeBuilder( testAssembly );
				treeBuilder.Add( fixtures );
			}
			else 
			foreach( TestSuite fixture in fixtures )
			{
				testAssembly.Add( fixture );
			}

			if ( fixtures.Count == 0 )
			{
				testAssembly.ShouldRun = false;
				testAssembly.IgnoreReason = "Has no TestFixtures";
			}
			
			return testAssembly;
		}

		#endregion

		#region Factory Methods
		/// <summary>
		/// Factory method for constructing TestAssembly instance.
		/// </summary>
		protected virtual TestAssembly CreateTestAssembly(string assemblyName, int assemblyKey)
		{
			return new TestAssembly(assemblyName, assemblyKey);
		} 
		#endregion

		#region Nested TypeFilter Class

//		private class TypeFilter
//		{
//			private string rootNamespace;
//
//			TypeFilter( string rootNamespace ) 
//			{
//				this.rootNamespace = rootNamespace;
//			}
//
//			public bool Include( Type type )
//			{
//				if ( type.Namespace == rootNamespace )
//					return true;
//
//				return type.Namespace.StartsWith( rootNamespace + '.' );
//			}
//		}

		#endregion

		#region Helper Methods

		private Assembly Load(string assemblyName)
		{
			// Change currentDirectory in case assembly references unmanaged dlls
			using( new DirectorySwapper( Path.GetDirectoryName( assemblyName ) ) )
			{
				Assembly assembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assemblyName));
				
				if ( assembly != null )
				{
					this.testFramework = TestFramework.FromAssembly( assembly );
					Addins.Register( assembly );
				}

				return assembly;
			}
		}

		private IList GetFixtures( Assembly assembly, string ns, int assemblyKey )
		{
			ArrayList fixtures = new ArrayList();
			if ( testFramework != null )
			{
				IList testTypes = GetCandidateFixtureTypes( assembly, ns );
				foreach(Type testType in testTypes)
				{
					if( CanBuildFrom( testType ) )
						fixtures.Add( BuildFrom( testType, assemblyKey ) );
				}
			}
			return fixtures;
		}

		private TestSuite BuildSingleFixture( Type testType )
		{
			// The only place we currently allow legacy suites
			if ( legacySuiteBuilder.CanBuildFrom( testType ) )
				return legacySuiteBuilder.BuildFrom( testType, 0 );

			return BuildFrom( testType, 0 );
		}
		
		private IList GetCandidateFixtureTypes( Assembly assembly, string ns )
		{
			IList types = testFramework.AllowPrivateTests
				? assembly.GetTypes()
				: assembly.GetExportedTypes();
				
			if ( ns == null || ns == string.Empty || types.Count == 0 ) 
				return types;

			string prefix = ns + "." ;
			
			ArrayList result = new ArrayList();
			foreach( Type type in types )
				if ( type.FullName.StartsWith( prefix ) )
					result.Add( type );

			return result;
		}

		#endregion
	}
}
