using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using FinalProject;

namespace LogFileVisualizer
{
	class VisualizerWindow : GameWindow
	{
		readonly string mLogFilename;
		int mNumJoints;
		RelativeAction mAction;
		RelativeJointState mCurrState;
		
		float mScale;
		float mAngle;
		float mTime;
		
		public VisualizerWindow(string lfn)
			: base(640, 640, GraphicsMode.Default, "Log file visualizer")
		{
			mLogFilename = lfn;
			
			mScale = 1.0f;
			mAngle = 0.0f;
			mTime = 0.0f;
			
			//Keyboard.KeyUp += HandleKeyboardKeyUp;
		}
		
		IEnumerator<RawJointState> LoadEnumerator() {
			System.Console.WriteLine("Loading {0}...", mLogFilename);
			
			int numLoaded = 0;
			var file = new StreamReader(mLogFilename);
			while ( !file.EndOfStream ) {
				string line = file.ReadLine().Trim();
				if ( line.Length == 0 ) continue;
				if ( line[0] == '#' ) continue;
				if ( mNumJoints == -1 ) {
					// Count the number of joints
					mNumJoints = (line.Split().Length - 1) / 3;
					System.Console.WriteLine("Number joints: {0}", mNumJoints);
				}
				
				var j = RawJointState.FromInputLine(line);
				if ( j.Joints.Length != mNumJoints ) throw new Exception("Mismatch in number of joints!");
				yield return j;
				numLoaded++;
			}
			
			System.Console.WriteLine("Loaded {0} joint states from {1}", numLoaded, mLogFilename);
		}
		
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.PointSize(8.0f);
			GL.Enable(EnableCap.DepthTest);
			
			mNumJoints = -1;
			mAction = new RelativeAction(new EnumerableFromEnumerator<RawJointState>(LoadEnumerator()));
			mCurrState = RelativeJointState.CloneFrom(mAction.States[0]);
		}
		
		void Rescale() {
			//Matrix4 pm = Matrix4.CreateOrthographic(mScale, mScale, 1.0f, 50.0f);
			Matrix4 pm = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 2.0f, 1.0f, 1.0f, 50.0f);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref pm);
		}
		
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			
			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
			Rescale();
		}
		
		protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();
			
			
			float ft = (float)e.Time;
			
			float scaleSpeed = (float)Math.Pow(mScale, 1.5);
			if (Keyboard[Key.Z]) {
				mScale -= ft * scaleSpeed;
				if ( mScale <= 0.1f ) mScale = 0.1f;
				System.Console.WriteLine("Scale now {0}", mScale);
			}
			if (Keyboard[Key.A]) {
				mScale += ft * scaleSpeed;
				System.Console.WriteLine("Scale now {0}", mScale);
			}
			
			float turnSpeed = 2.0f;
			if (Keyboard[Key.Left]) {
				mAngle += ft * turnSpeed;
			}
			if (Keyboard[Key.Right]) {
				mAngle -= ft * turnSpeed;
			}
			
			float timeAdvanceSpeed = 1.0f;
			if (Keyboard[Key.Up]) {
				mTime += timeAdvanceSpeed * ft;
			}
			else if (Keyboard[Key.Down]) {
				mTime -= timeAdvanceSpeed * ft;
			}
			
			if (Keyboard[Key.R]) {
				mTime = 0.0f;
			}
			
			if ( mTime < 0.0f ) mTime = 0.0f;
			else if ( mTime > mAction.TotalTime ) mTime = mAction.TotalTime;
			
			mAction.InterpolateState(mTime, ref mCurrState);
        }
		
		void HandleKeyboardKeyUp (object sender, KeyboardKeyEventArgs e)
		{
			switch ( e.Key ) {
			case Key.Up:
				break;
			case Key.Down:
				break;
			}
		}
		
		protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			//Rescale();
			
			Vector3 camPos = new Vector3((float)Math.Cos(mAngle), 0.0f, (float)Math.Sin(mAngle));
			
            Matrix4 modelview = Matrix4.LookAt(camPos * 5.0f, Vector3.Zero, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			//GL.Translate(new Vector3(0.0f, 0.0f, -5.0f));
            GL.MultMatrix(ref modelview);
			GL.Scale(mScale, mScale, mScale);

            GL.Begin(BeginMode.Points);
			int index = 0;
			foreach ( var joint in mCurrState.RelativeJoints ) {
				GL.Color3(0.0f, (float)((index * 16) % 256) / 256.0f, 1.0f);
				GL.Vertex3(joint);
				index += 1;
			}
            GL.End();

            SwapBuffers();
        }
			
		
		
		public static void Main (string[] args)
		{
			string filename = "gestures/track_punch_00.log";
			if ( args.Length > 0 ) {
				filename = args[0];
			}
			
			using ( var vw = new VisualizerWindow(filename) ) {
				vw.Run(30.0);
			}
		}
	}
}

