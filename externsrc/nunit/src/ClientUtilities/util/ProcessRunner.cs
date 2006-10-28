using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for ProcessRunner.
	/// </summary>
	public class ProcessRunner : ProxyTestRunner
	{
		private Process process;

		public ProcessRunner() { }

		public void Start()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo( "nunit-server.exe", "TestServer" );
			startInfo.CreateNoWindow = true;
			this.process = Process.Start( startInfo );
			System.Threading.Thread.Sleep( 1000 );
			Object obj = Activator.GetObject( typeof( TestRunner ), "tcp://localhost:9000/TestServer" );
			this.testRunner = (TestRunner) obj;
		}

		public void Stop()
		{
			//RealProxy proxy = RemotingServices.GetRealProxy( this.testRunner );
			process.Kill();
		}

		public Process Process
		{
			get { return process; }
		}
	}
}
