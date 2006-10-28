namespace NUnit.Framework.Tests
{
	[TestFixture]
	public class StringAssertTests
	{
		[Test]
		public void Contains()
		{
			StringAssert.Contains( "abc", "abc" );
			StringAssert.Contains( "abc", "***abc" );
			StringAssert.Contains( "abc", "**abc**" );
		}

		[Test]
		public void ContainsFails()
		{
			ContainsAsserter asserter = 
				new ContainsAsserter( "abc", "abxcdxbc", null, null );
			Assert.AreEqual( false, asserter.Test() );
			Assert.AreEqual( System.Environment.NewLine
	+ "\t" + @"expected: String containing ""abc""" + System.Environment.NewLine
	+ "\t" + @" but was: <""abxcdxbc"">",
				asserter.Message );
		}

		[Test]
		public void StartsWith()
		{
			StringAssert.StartsWith( "abc", "abcdef" );
			StringAssert.StartsWith( "abc", "abc" );
		}

		[Test]
		public void StartsWithFails()
		{
			StartsWithAsserter asserter = 
				new StartsWithAsserter( "xyz", "abcxyz", null, null );
			Assert.AreEqual( false, asserter.Test() );
			Assert.AreEqual( System.Environment.NewLine
	+ "\t" + @"expected: String starting with ""xyz""" + System.Environment.NewLine
	+ "\t" + @" but was: <""abcxyz"">",
				asserter.Message );
		}
	
		[Test]
		public void EndsWith()
		{
			StringAssert.EndsWith( "abc", "abc" );
			StringAssert.EndsWith( "abc", "123abc" );
		}

		[Test]
		public void EndsWithFails()
		{
			EndsWithAsserter asserter =
				new EndsWithAsserter( "xyz", "abcdef", null, null );
			Assert.AreEqual( false, asserter.Test() );
			Assert.AreEqual( System.Environment.NewLine
	+ "\t" + @"expected: String ending with ""xyz""" + System.Environment.NewLine
	+ "\t" + @" but was: <""abcdef"">",
				asserter.Message );
		}

		[Test]
		public void CaseInsensitiveCompare()
		{
			StringAssert.AreEqualIgnoringCase( "name", "NAME" );
		}

		[Test]
		public void CaseInsensitiveCompareFails()
		{
			EqualIgnoringCaseAsserter asserter =
				new EqualIgnoringCaseAsserter( "Name", "NAMES", null, null );
			Assert.AreEqual( false, asserter.Test() );
			Assert.AreEqual( System.Environment.NewLine
	+ "\tString lengths differ.  Expected length=4, but was length=5." + System.Environment.NewLine
	+ "\tStrings differ at index 4." + System.Environment.NewLine
	+ "\t" + @"expected: <""Name"">" + System.Environment.NewLine
	+ "\t" + @" but was: <""NAMES"">" + System.Environment.NewLine
	+ "\t----------------^",
				asserter.Message );
		}
	}
}
