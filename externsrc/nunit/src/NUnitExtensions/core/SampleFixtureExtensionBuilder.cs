using System;
using NUnit.Core.Builders;

namespace NUnit.Core.Extensions
{
	/// <summary>
	/// MockFixtureExtensionBuilder knows how to build 
	/// a MockFixtureExtension.
	/// </summary>
	[SuiteBuilder]
	public class SampleFixtureExtensionBuilder : NUnitTestFixtureBuilder
	{	
		#region ISuiteBuilder Members

		// This builder delegates all the work to the constructor of the  
		// extension suite. Many builders will need to do more work, 
		// looking for other attributes, setting properties on the 
		// suite and locating methods for tests, setup and teardown.
		public override TestSuite BuildFrom(Type type, int assemblyKey)
		{
			if ( CanBuildFrom( type ) )
				return base.BuildFrom( type, assemblyKey );
			return null;
		}

		// The builder recognizes the types that it can use by the presense
		// of SampleFixtureExtensionAttribute. Note that an attribute does not
		// have to be used. You can use any arbitrary set of rules that can be 
		// implemented using reflection on the type.
		public override bool CanBuildFrom(Type type)
		{
			return type.IsDefined( typeof( SampleFixtureExtensionAttribute ), false );
		}
		#endregion
	}
}
