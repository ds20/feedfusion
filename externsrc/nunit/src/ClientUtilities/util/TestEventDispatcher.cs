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
using System.Collections;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Helper class used to dispatch test events
	/// </summary>
	public class TestEventDispatcher : ITestEvents
	{
		#region Events

		// Project loading events
		public event TestEventHandler ProjectLoading;
		public event TestEventHandler ProjectLoaded;
		public event TestEventHandler ProjectLoadFailed;
		public event TestEventHandler ProjectUnloading;
		public event TestEventHandler ProjectUnloaded;
		public event TestEventHandler ProjectUnloadFailed;

		// Test loading events
		public event TestEventHandler TestLoading;	
		public event TestEventHandler TestLoaded;	
		public event TestEventHandler TestLoadFailed;

		public event TestEventHandler TestReloading;
		public event TestEventHandler TestReloaded;
		public event TestEventHandler TestReloadFailed;

		public event TestEventHandler TestUnloading;
		public event TestEventHandler TestUnloaded;
		public event TestEventHandler TestUnloadFailed;

		// Test running events
		public event TestEventHandler RunStarting;	
		public event TestEventHandler RunFinished;
		
		public event TestEventHandler SuiteStarting;
		public event TestEventHandler SuiteFinished;

		public event TestEventHandler TestStarting;
		public event TestEventHandler TestFinished;

		public event TestEventHandler TestException;
		public event TestEventHandler TestOutput;

		#endregion

		#region Methods for Firing Events
		
		protected virtual void Fire( TestEventHandler handler, TestEventArgs e )
		{
			if ( handler != null )
				handler( this, e );
		}

		public void FireProjectLoading( string fileName )
		{
			Fire(
				ProjectLoading,
				new TestEventArgs( TestAction.ProjectLoading, fileName ) );
		}

		public void FireProjectLoaded( string fileName )
		{
			Fire( 
				ProjectLoaded,
				new TestEventArgs( TestAction.ProjectLoaded, fileName ) );
		}

		public void FireProjectLoadFailed( string fileName, Exception exception )
		{
			Fire( 
				ProjectLoadFailed,
				new TestEventArgs( TestAction.ProjectLoadFailed, fileName, exception ) );
		}

		public void FireProjectUnloading( string fileName )
		{
			Fire( 
				ProjectUnloading,
				new TestEventArgs( TestAction.ProjectUnloading, fileName ) );
		}

		public void FireProjectUnloaded( string fileName )
		{
			Fire( 
				ProjectUnloaded,
				new TestEventArgs( TestAction.ProjectUnloaded, fileName ) );
		}

		public void FireProjectUnloadFailed( string fileName, Exception exception )
		{
			Fire( 
				ProjectUnloadFailed,
				new TestEventArgs( TestAction.ProjectUnloadFailed, fileName, exception ) );
		}

		public void FireTestLoading( string fileName )
		{
			Fire( 
				TestLoading,
				new TestEventArgs( TestAction.TestLoading, fileName ) );
		}

		public void FireTestLoaded( string fileName, Test test )
		{
			Fire( 
				TestLoaded,
				new TestEventArgs( TestAction.TestLoaded, fileName, test ) );
		}

		public void FireTestLoadFailed( string fileName, Exception exception )
		{
			Fire(
				TestLoadFailed,
				new TestEventArgs( TestAction.TestLoadFailed, fileName, exception ) );
		}

		public void FireTestUnloading( string fileName, Test test )
		{
			Fire(
				TestUnloading,
				new TestEventArgs( TestAction.TestUnloading, fileName, test ) );
		}

		public void FireTestUnloaded( string fileName, Test test )
		{
			Fire(
				TestUnloaded,
				new TestEventArgs( TestAction.TestUnloaded, fileName, test ) );
		}

		public void FireTestUnloadFailed( string fileName, Exception exception )
		{
			Fire(
				TestUnloadFailed, 
				new TestEventArgs( TestAction.TestUnloadFailed, fileName, exception ) );
		}

		public void FireTestReloading( string fileName, Test test )
		{
			Fire(
				TestReloading,
				new TestEventArgs( TestAction.TestReloading, fileName, test ) );
		}

		public void FireTestReloaded( string fileName, Test test )
		{
			Fire(
				TestReloaded,
				new TestEventArgs( TestAction.TestReloaded, fileName, test ) );
		}

		public void FireTestReloadFailed( string fileName, Exception exception )
		{
			Fire(
				TestReloadFailed, 
				new TestEventArgs( TestAction.TestReloadFailed, fileName, exception ) );
		}

		public void FireRunStarting( Test[] tests, int count )
		{
			Fire(
				RunStarting,
				new TestEventArgs( TestAction.RunStarting, tests, count ) );
		}

		public void FireRunFinished( TestResult[] results )
		{	
			Fire(
				RunFinished,
				new TestEventArgs( TestAction.RunFinished, results ) );
		}

		public void FireRunFinished( Exception exception )
		{
			Fire(
				RunFinished,
				new TestEventArgs( TestAction.RunFinished, exception ) );
		}

		public void FireTestStarting( Test test )
		{
			Fire(
				TestStarting,
				new TestEventArgs( TestAction.TestStarting, test ) );
		}

		public void FireTestFinished( TestResult result )
		{	
			Fire(
				TestFinished,
				new TestEventArgs( TestAction.TestFinished, result ) );
		}

		public void FireSuiteStarting( Test test )
		{
			Fire(
				SuiteStarting,
				new TestEventArgs( TestAction.SuiteStarting, test ) );
		}

		public void FireSuiteFinished( TestResult result )
		{	
			Fire(
				SuiteFinished,
				new TestEventArgs( TestAction.SuiteFinished, result ) );
		}

		public void FireTestException( Exception exception )
		{
			Fire(
				TestException,
				new TestEventArgs( TestAction.TestException, exception ) );
		}

		public void FireTestOutput( TestOutput testOutput )
		{
			Fire(
				TestOutput,
				new TestEventArgs( TestAction.TestOutput, testOutput ) );
		}

		#endregion
	}
}
