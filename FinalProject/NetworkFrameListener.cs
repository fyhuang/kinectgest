using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace FinalProject
{
	public class NetworkFrameListener
	{
		IPAddress mAddress;
		TcpListener mListener;
		Queue<RawJointState> mQueue;
		bool mRunning;
		
		public NetworkFrameListener (int port)
		{
			mAddress = IPAddress.Parse("0.0.0.0");
			mListener = new TcpListener(mAddress, port);
			mQueue = new Queue<RawJointState>();
		}
		
		public void Start()
		{
			mRunning = true;
			var thread = new Thread(new ThreadStart(this.ThreadRun));
			thread.Start();
		}
		
		public void Stop()
		{
			mRunning = false;
			Console.WriteLine("Stopping");
		}
		
		void ThreadRun()
		{
			try {
				mListener.Start(1);
				Console.WriteLine("Listening on {0}", mListener.LocalEndpoint);
				using ( var client = mListener.AcceptTcpClient() ) { // Synchronous?
					Console.WriteLine("Accepted connection from {0}", client.Client.RemoteEndPoint);
					var rawstream = client.GetStream();
					var stream = new StreamReader(rawstream);
					
					while ( mRunning ) {
						try {
							var line = stream.ReadLine();
							if ( line == null ) break;
							var rjs = RawJointState.FromInputLine(line);
							lock ( mQueue ) {
								mQueue.Enqueue(rjs);
							}
						}
						catch ( IOException e ) {
							Console.WriteLine("IO exception while receiving network data:\n{0}\n{1}", e, e.StackTrace);
							mRunning = false;
							break;
						}
					}
					
					client.Close();
				}
			}
			finally {
				mListener.Stop();
			}
		}
		
		public RawJointState GetState() {
			lock ( mQueue ) {
				if ( mQueue.Count == 0 ) return null;
				else {
					return mQueue.Dequeue();
				}
			}
		}
	}
}

