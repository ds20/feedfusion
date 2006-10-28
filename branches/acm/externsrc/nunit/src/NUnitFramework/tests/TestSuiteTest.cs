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

using System;
using NUnit.Framework;
using NUnit.Core.Builders;
using NUnit.Util;
using NUnit.Tests.Assemblies;
using System.Collections;

namespace NUnit.Core.Tests
{
	/// <summary>
	/// Summary description for TestSuiteTest.
	/// </summary>
	/// 
	[TestFixture]
	public class TestSuiteTest
	{
		TestSuite oneTestFixture;
		TestSuite mockTestFixture;
		TestSuite noTestSuite;

		[SetUp]
		public void SetUp()
		{
			oneTestFixture = TestFixtureBuilder.Make( typeof( OneTestCase ) );
			mockTestFixture = TestFixtureBuilder.Make( typeof( MockTestFixture ) );
			TestSuite noTestFixture = TestFixtureBuilder.Make( typeof( EmptyFixture ) );

			//EmptyFixture fixture = new EmptyFixture();
			noTestSuite = new TestSuite("No Tests");
			noTestSuite.Add( noTestFixture);
		}

		[Test]
		public void RunTestsInFixture()
		{
			TestResult result = mockTestFixture.Run( NullListener.NULL );
			ResultSummarizer summarizer = new ResultSummarizer( result );
			Assert.AreEqual( MockTestFixture.Tests - MockTestFixture.NotRun, summarizer.ResultCount );
			Assert.AreEqual( MockTestFixture.NotRun, summarizer.TestsNotRun );

			result = findResult( "ExplicitlyRunTest", result );
			Assert.IsNotNull( result, "Cannot find ExplicitlyRunTest result" );
			Assert.IsFalse( result.Executed, "ExplicitlyRunTest should not be executed" );
			Assert.AreEqual( "Explicit selection required", result.Message );
		}

		[Test]
		public void RunExplicitTestDirectly()
		{
			Test test = findTest( "ExplicitlyRunTest", mockTestFixture );
			Assert.IsNotNull( test, "Cannot find ExplicitlyRunTest" );
			Assert.IsTrue( test.IsExplicit, "Test not marked Explicit" );
			TestResult result = test.Run( NullListener.NULL );
			ResultSummarizer summarizer = new ResultSummarizer( result );
			Assert.AreEqual( 1, summarizer.ResultCount );
		}

		[Test]
		public void RunExplicitTestByName()
		{
			Test test = findTest( "ExplicitlyRunTest", mockTestFixture );
			Assert.IsNotNull( test, "Cannot find ExplicitlyRunTest" );
			Assert.IsTrue( test.IsExplicit, "Test not marked Explicit" );

			NameFilter filter = new NameFilter( test );
			TestResult result = mockTestFixture.Run( NullListener.NULL, filter );
			ResultSummarizer summarizer = new ResultSummarizer( result );
			Assert.AreEqual( 1, summarizer.ResultCount );
		}

		[Test]
		public void RunExplicitTestByCategory()
		{
			CategoryFilter filter = new CategoryFilter( "Special" );
			TestResult result = mockTestFixture.Run( NullListener.NULL, filter );
			ResultSummarizer summarizer = new ResultSummarizer( result );
			Assert.AreEqual( 1, summarizer.ResultCount );
		}

		[Test]
		public void ExcludingCategoryDoesNotRunExplicitTests()
		{
			CategoryFilter filter = new CategoryFilter( "MockCategory", true );
			TestResult result = mockTestFixture.Run( NullListener.NULL, filter );
			ResultSummarizer summarizer = new ResultSummarizer( result );
			Assert.AreEqual( 1, summarizer.ResultCount );
		}

		[Test]
		public void InheritedTestCount()
		{
			TestSuite suite = TestFixtureBuilder.Make( typeof( InheritedTestFixture ) );
			Assert.AreEqual(InheritedTestFixture.Tests, suite.CountTestCases());
		}

		[Test]
		public void SuiteRunInitialized()
		{
			Assert.IsTrue(mockTestFixture.ShouldRun, "default state is to run TestSuite");
		}

		[Test]
		public void SuiteWithNoTests()
		{
			ArrayList tests = noTestSuite.Tests;
			Assert.AreEqual(1, tests.Count);
			TestSuite testSuite = (TestSuite)tests[0];

			Assert.IsFalse(testSuite.ShouldRun,
				"ShouldRun should be false because there are no tests");
			Assert.AreEqual(testSuite.Name + " does not have any tests", testSuite.IgnoreReason);
		}

		[Test]
		public void RunNoTestSuite()
		{
			Assert.AreEqual(0, noTestSuite.CountTestCases());
			
			TestResult result = noTestSuite.Run(NullListener.NULL);

			ResultSummarizer summarizer = new ResultSummarizer(result);
			Assert.AreEqual(0, summarizer.ResultCount);
			Assert.AreEqual(0, summarizer.TestsNotRun);
			Assert.AreEqual(1, summarizer.SuitesNotRun);
		}

		[Test]
		public void RunTestByName() 
		{
			TestSuite testSuite = new TestSuite("Mock Test Suite");
			testSuite.Add(mockTestFixture);

			Assert.IsNull(testSuite.Parent);
			Test firstTest = (Test)testSuite.Tests[0];
			Assert.AreEqual(testSuite, firstTest.Parent);
			Assert.AreEqual("Mock Test Suite", testSuite.TestPath);
			Assert.AreEqual("Mock Test SuiteNUnit.Tests.Assemblies.MockTestFixture", firstTest.TestPath);
			Assert.IsTrue(firstTest.IsDescendant(testSuite), "test should be a descendant of suite");
			Test bottom = (Test)firstTest.Tests[2];
			Assert.IsTrue(bottom.IsDescendant(firstTest));
			Assert.IsTrue(bottom.IsDescendant(testSuite));
			
			RecordingListener listener = new RecordingListener();
			NameFilter filter = new NameFilter(bottom);
			testSuite.Run(listener, filter);
			Assert.AreEqual(1, listener.testStarted.Count);
			Assert.AreEqual("MockTest3", (string)listener.testStarted[0]);
		}

		[Test]
		public void RunSuiteByName() 
		{
			TestSuite testSuite = new TestSuite("Mock Test Suite");
			testSuite.Add(mockTestFixture);
			
			RecordingListener listener = new RecordingListener();
			testSuite.Run(listener);

			Assert.AreEqual(MockTestFixture.Tests, listener.testStarted.Count);
			Assert.AreEqual(2, listener.suiteStarted.Count);
		}

		[Test]
		public void CountTestCasesFilteredByName() 
		{
			TestSuite testSuite = new TestSuite("Mock Test Suite");
			testSuite.Add(mockTestFixture);
			Assert.AreEqual(MockTestFixture.Tests, testSuite.CountTestCases(EmptyFilter.Empty));
			
			NUnit.Core.TestCase mock3 = (NUnit.Core.TestCase) findTest("MockTest3", testSuite);
			NUnit.Core.TestCase mock1 = (NUnit.Core.TestCase) findTest("MockTest1", testSuite);
			NameFilter filter = new NameFilter(mock3);
			Assert.AreEqual(1, testSuite.CountTestCases(filter));

			ArrayList nodes = new ArrayList();
			nodes.Add(mock3);
			nodes.Add(mock1);
			filter = new NameFilter(nodes);

			Assert.AreEqual(2, testSuite.CountTestCases(filter));

			filter = new NameFilter(testSuite);

			Assert.AreEqual(MockTestFixture.Tests, testSuite.CountTestCases(filter));
		}

		[Test]
		public void RunTestByCategory() 
		{
			TestSuite testSuite = new TestSuite("Mock Test Suite");
			testSuite.Add( mockTestFixture );

			CategoryFilter filter = new CategoryFilter();
			filter.AddCategory("MockCategory");
			RecordingListener listener = new RecordingListener();
			testSuite.Run(listener, filter);
			Assert.AreEqual(2, listener.testStarted.Count);
			Assert.IsTrue(listener.testStarted.Contains("MockTest2"));
			Assert.IsTrue(listener.testStarted.Contains("MockTest3"));
		}

		[Test]
		public void RunSuiteByCategory() 
		{
			TestSuite testSuite = new TestSuite("Mock Test Suite");
			testSuite.Add(mockTestFixture);

			CategoryFilter filter = new CategoryFilter();
			filter.AddCategory("FixtureCategory");
			RecordingListener listener = new RecordingListener();
			testSuite.Run(listener, filter);
			Assert.AreEqual(MockTestFixture.Tests, listener.testStarted.Count);
		}

		[Test]
		public void RunSingleTest()
		{
			TestSuite fixture = TestFixtureBuilder.Make( typeof( NUnit.Tests.Assemblies.MockTestFixture ) );
			Test test = (Test) fixture.Tests[0];
			RecordingListener listener = new RecordingListener();
			test.Run(listener, null);
			Assert.IsFalse(listener.lastResult.IsFailure);
		}

		private Test findTest(string name, Test test) 
		{
			Test result = null;
			if (test.Name == name)
				result = test;
			else if (test.Tests != null)
			{
				foreach(Test t in test.Tests) 
				{
					result = findTest(name, t);
					if (result != null)
						break;
				}
			}

			return result;
		}

		private TestResult findResult(string name, TestResult result) 
		{
			if (result.Test.Name == name)
				return result;

			TestSuiteResult suiteResult = result as TestSuiteResult;
			if ( suiteResult != null )
			{
				foreach( TestResult r in suiteResult.Results ) 
				{
					TestResult myResult = findResult( name, r );
					if ( myResult != null )
						return myResult;
				}
			}

			return null;
		}
	}

	[Serializable]
	public class RecordingListener : EventListener
	{
		public ArrayList testStarted = new ArrayList();
		public ArrayList testFinished = new ArrayList();
		public ArrayList suiteStarted = new ArrayList();
		public ArrayList suiteFinished = new ArrayList();

		public TestResult lastResult = null;

		public void RunStarted(Test[] tests)
		{
		}

		public void RunFinished(NUnit.Core.TestResult[] results)
		{
		}

		public void RunFinished(Exception exception)
		{
		}

		public void TestStarted(NUnit.Core.TestCase testCase) 
		{
			testStarted.Add(testCase.Name);
		}
			
		public void TestFinished(TestCaseResult result)
		{
			testFinished.Add(result.Name);
			lastResult = result;
		}

		public void SuiteStarted(TestSuite suite)
		{
			suiteStarted.Add(suite.Name);
		}

		public void SuiteFinished(TestSuiteResult result)
		{
			suiteFinished.Add(result.Name);
		}

		public void UnhandledException(Exception exception )
		{
		}

		public void TestOutput(TestOutput testOutput)
		{
		}
	}
}
