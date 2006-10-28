using System;

namespace NUnit.Framework.Tests
{
	/// <summary>
	/// Summary description for ArrayEqualTests.
	/// </summary>
	[TestFixture]
	public class ArrayEqualsFixture
	{
		[Test]
		public void SameArraysAreEqual()
		{
			string[] array = { "one", "two", "three" };
			Assert.AreSame( array, array );
			Assert.AreEqual( array, array );
		}

		[Test]
		public void DifferentArraysAreEqual()
		{
			string[] array1 = { "one", "two", "three" };
			string[] array2 = { "one", "two", "three" };
			Assert.IsFalse( array1 == array2 );
			Assert.AreEqual( array1, array2 );
		}

		[Test]
		public void DifferentLengthArrays()
		{
			string[] array1 = { "one", "two", "three" };
			string[] array2 = { "one", "two", "three", "four", "five" };

			EqualAsserter eq = new EqualAsserter( array1, array2, null, null );
			Assert.AreEqual( false, eq.Test() );
		}

		[Test]
		public void SameLengthDifferentContent()
		{
			string[] array1 = { "one", "two", "three" };
			string[] array2 = { "one", "two", "ten" };
			EqualAsserter eq = new EqualAsserter( array1, array2, null, null );
			Assert.AreEqual( false, eq.Test() );
		}

		[Test]
		public void DifferentArrayTypesButEqual()
		{
			string[] array1 = { "one", "two", "three" };
			object[] array2 = { "one", "two", "three" };
			Assert.AreEqual( array1, array2, "String[] not equal to Object[]" );
			Assert.AreEqual( array2, array1, "Object[] not equal to String[]" );	
		}

		[Test]
		public void MixedTypesAreEqual()
		{
			object[] array1 = { "one", 2, 3.0 };
			object[] array2 = { "one", 2.0, 3 };
			Assert.AreEqual( array1, array2 );
		}

		[Test]
		public void DifferentArrayTypesNotEqual()
		{
			string[] array1 = { "one", "two", "three" };
			object[] array2 = { "one", "three", "two" };
			EqualAsserter eq = new EqualAsserter( array1, array2, null, null );
			Assert.AreEqual( false, eq.Test() );
			Assert.AreEqual( "\r\nArray lengths are both 3.\r\nArrays differ at index 1.\r\n\tString lengths differ.  Expected length=3, but was length=5.\r\n\tStrings differ at index 1.\r\n\texpected: <\"two\">\r\n\t but was: <\"three\">\r\n\t-------------^",
				eq.Message );
		}

		[Test]
		public void ArraysOfInt()
		{
			int[] a = new int[] { 1, 2, 3 };
			int[] b = new int[] { 1, 2, 3 };
			Assert.AreEqual( a, b );
		}

		[Test]
		public void ArraysOfDouble()
		{
			double[] a = new double[] { 1.0, 2.0, 3.0 };
			double[] b = new double[] { 1.0, 2.0, 3.0 };
			Assert.AreEqual( a, b );
		}

		[Test]
		public void ArraysOfDecimal()
		{
			decimal[] a = new decimal[] { 1.0m, 2.0m, 3.0m };
			decimal[] b = new decimal[] { 1.0m, 2.0m, 3.0m };
			Assert.AreEqual( a, b );
		}

		[Test]
		public void ArrayOfIntVersusArrayOfDouble()
		{
			int[] a = new int[] { 1, 2, 3 };
			double[] b = new double[] { 1.0, 2.0, 3.0 };
			Assert.AreEqual( a, b );
			Assert.AreEqual( b, a );
		}

		[Test]
		public void RanksOfArraysMustMatch()
		{
			int[,] a = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
			int[] b = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			EqualAsserter eq = new EqualAsserter( a , b, null, null );
			Assert.AreEqual( false, eq.Test() );
			Assert.AreEqual( "\r\n\texpected: <System.Int32[,]>\r\n\t but was: <System.Int32[]>",
				eq.Message );
		}

		[Test, ExpectedException( typeof( ArgumentException ), "Multi-dimension array comparison is not supported" )]
		public void MultiDimensionedArraysNotSupported()
		{
			int[,] a = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
			int[,] b = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
			Assert.AreEqual( a, b );
		}

		[Test]
		public void ArraysPassedAsObjects()
		{
			object a = new int[] { 1, 2, 3 };
			object b = new double[] { 1.0, 2.0, 3.0 };
			Assert.AreEqual( a, b );
			Assert.AreEqual( b, a );
		}
	}
}
