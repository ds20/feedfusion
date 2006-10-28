using System;
using System.Reflection;
using System.Collections;
using NUnit.Framework;
using NUnit.Core.Builders;

namespace NUnit.Core.Tests
{
	// TODO: Review to see if we need these tests
	[TestFixture(Description = "Fixture Description")]
	internal class MockFixture
	{
		[Test(Description = "Test Description")]
		public void Method()
		{}

		[Test]
		public void NoDescriptionMethod()
		{}
	}

	internal class DescriptionVisitor : ResultVisitor
	{
		private string name; 
		private string description;

		public DescriptionVisitor(string name, string description)
		{
			this.name = name;
			this.description = description;
		}

		public void Visit(TestCaseResult caseResult)
		{
			if(caseResult.Name.Equals(name))
				Assert.AreEqual(description, caseResult.Description);
		}

		public void Visit(TestSuiteResult suiteResult)
		{
			Console.WriteLine(suiteResult.Name);
			if(suiteResult.Name.Equals(name))
				Assert.AreEqual(description, suiteResult.Description);

			foreach (TestResult result in suiteResult.Results)
			{
				result.Accept(this);
			}
		}
	}


	[TestFixture]
	public class TestAttributeFixture
	{
		static readonly Type FixtureType = typeof( MockFixture );

		[Test]
		public void ReflectionTest()
		{
			TestCase testCase = TestCaseBuilder.Make( FixtureType, "Method" );
			Assert.IsTrue(testCase.ShouldRun);
		}

		[Test]
		public void Description()
		{
			TestCase testCase = TestCaseBuilder.Make( FixtureType, "Method" );
			Assert.AreEqual("Test Description", testCase.Description);
		}

		[Test]
		public void DescriptionInResult()
		{
			TestSuite suite = new TestSuite("Mock Fixture");
			suite.Add( TestFixtureBuilder.Make( typeof( MockFixture ) ) );
			TestResult result = suite.Run(NullListener.NULL);

			DescriptionVisitor visitor = new DescriptionVisitor("NUnit.Tests.Attributes.MockFixture.Method", "Test Description");
			result.Accept(visitor);

			visitor = 
				new DescriptionVisitor("NUnit.Tests.Attributes.MockFixture.NoDescriptionMethod", null);
			result.Accept(visitor);
		}

		[Test]
		public void NoDescription()
		{
			TestCase testCase = TestCaseBuilder.Make( FixtureType, "NoDescriptionMethod" );
			Assert.IsNull(testCase.Description);
		}

		[Test]
		public void FixtureDescription()
		{
			TestSuite suite = new TestSuite("suite");
			suite.Add( TestFixtureBuilder.Make( typeof( MockFixture ) ) );

			ArrayList tests = suite.Tests;
			TestSuite mockFixtureSuite = (TestSuite)tests[0];

			Assert.AreEqual("Fixture Description", mockFixtureSuite.Description);
		}

		[Test]
		public void FixtureDescriptionInResult()
		{
			TestSuite suite = new TestSuite("Mock Fixture");
			suite.Add( TestFixtureBuilder.Make( typeof( MockFixture ) ) );
			TestResult result = suite.Run(NullListener.NULL);

			DescriptionVisitor visitor = new DescriptionVisitor("MockFixture", "Fixture Description");
			result.Accept(visitor);
		}
	}
}
