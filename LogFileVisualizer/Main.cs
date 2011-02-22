using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace LogFileVisualizer
{
	class VisualizerWindow : GameWindow
	{
		class JointState {
			public float time;
			public Vector3[] joints;
			
			public JointState(int num, ref string line) {
				string[] words = line.Split();
				if ( words.Length != num + 1 ) throw new Exception("Count mismatch!");
				
				joints = new Vector3[num];
				
				time = float.Parse(words[0]);
				for ( int i = 0; i < (words.Length-1) / 3; i++ ) {
					int ri = (i*3) + 1;
					
					joints[i].X = float.Parse(words[ri]);
					joints[i].Y = float.Parse(words[ri+1]);
					joints[i].Z = float.Parse(words[ri+2]);
				}
			}
			
			public void UpdateBB(ref Vector3 bb1, ref Vector3 bb2) {
				foreach ( var v in joints ) {
					if ( v.X < bb1.X ) bb1.X = v.X;
				}
			}
		}
		
		readonly string mLogFilename;
		int mNumJoints;
		List<JointState> mJointStates;
		
		float mAccum;
		
		float mScale;
		float mAngle;
		int mCurrJointState;
		
		OpenTK.Vector3 mBB1, mBB2;
		
		public VisualizerWindow(string lfn)
			: base(640, 640, GraphicsMode.Default, "Log file visualizer")
		{
			mLogFilename = lfn;
			mJointStates = new List<JointState>();
			
			mAccum = 0.0f;
			
			mScale = 1.0f;
			mAngle = 0.0f;
			mCurrJointState = 0;
			
			//Keyboard.KeyUp += HandleKeyboardKeyUp;
		}
		
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.PointSize(8.0f);
			GL.Enable(EnableCap.DepthTest);
			
			
			mNumJoints = -1;
			mBB1 = new Vector3(100.0f, 100.0f, 100.0f);
			mBB2 = new Vector3(-100.0f, -100.0f, -100.0f);
			
			System.Console.WriteLine("Loading {0}...", mLogFilename);
			var file = new StreamReader(mLogFilename);
			while ( !file.EndOfStream ) {
				string line = file.ReadLine().Trim();
				if ( line.Length == 0 ) continue;
				if ( line[0] == '#' ) continue;
				if ( mNumJoints == -1 ) {
					// Count the number of joints
					mNumJoints = line.Split().Length - 1;
					System.Console.WriteLine("Number joints: {0}", mNumJoints);
				}
				
				JointState j = new JointState(mNumJoints, ref line);
				j.UpdateBB(ref mBB1, ref mBB2);
				mJointStates.Add(j);
			}
			
			System.Console.WriteLine("Loaded {0} joint states from {1}", mJointStates.Count, mLogFilename);
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
			
			float turnSpeed = 1.0f;
			if (Keyboard[Key.Left]) {
				mAngle += ft * turnSpeed;
			}
			if (Keyboard[Key.Right]) {
				mAngle -= ft * turnSpeed;
			}
			
			float accumSpeed = 30.0f;
			if (Keyboard[Key.Up]) {
				mAccum += accumSpeed * ft;
			}
			else if (Keyboard[Key.Down]) {
				mAccum -= accumSpeed * ft;
			}
			else {
				mAccum = 0.0f;
			}
			
			if (Keyboard[Key.R]) {
				mCurrJointState = 0;
			}
			
			if ( mAccum > 1.0f ) {
				mCurrJointState += 1;
				if ( mCurrJointState >= mJointStates.Count ) {
					mCurrJointState = mJointStates.Count - 1;
				}
				mAccum = 0.0f;
				
				System.Console.WriteLine("Current state: {0}", mCurrJointState);
			}
			else if ( mAccum < -1.0f ) {
				mCurrJointState -= 1;
				if ( mCurrJointState < 0 ) {
					mCurrJointState = 0;
				}
				mAccum = 0.0f;
				
				System.Console.WriteLine("Current state: {0}", mCurrJointState);
			}
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
			foreach ( var joint in mJointStates[mCurrJointState].joints ) {
				GL.Color3(0.0f, (float)((index * 16) % 256) / 256.0f, 1.0f);
				GL.Vertex3(joint);
				index += 1;
			}
            GL.End();

            SwapBuffers();
        }
			
		
		
		public static void Main (string[] args)
		{
			using ( var vw = new VisualizerWindow("gestures/track_punch_00.log") ) {
				vw.Run(30.0);
			}
		}
	}
}

