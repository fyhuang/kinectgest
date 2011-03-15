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
	class GestureJointVisualizer : GameWindow
	{
		readonly string mLogFilename;
		InputGesture mGesture;
		JointState mCurrState;
		
		float mScale;
		float mAngle;
		float mTime;
		
		public GestureJointVisualizer(string lfn)
			: base(640, 640, GraphicsMode.Default, "Gesture visualizer (by joints)")
		{
			mLogFilename = lfn;
			
			mScale = 1.0f;
			mAngle = 0.0f;
			mTime = 0.0f;
		}
		
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.PointSize(8.0f);
			GL.Enable(EnableCap.DepthTest);
			
			mGesture = new InputGesture(new LogFileLoader(mLogFilename));
			mCurrState = JointState.CloneFrom(mGesture.States[0]);
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
			float timeAdvanceSpeed = 1.0f;
			if (Keyboard[Key.LShift]) {
				turnSpeed = 0.2f;
				timeAdvanceSpeed = 0.1f;
			}
			
			if (Keyboard[Key.Left]) {
				mAngle += ft * turnSpeed;
			}
			if (Keyboard[Key.Right]) {
				mAngle -= ft * turnSpeed;
			}
			
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
			else if ( mTime > mGesture.TotalTime ) mTime = mGesture.TotalTime;
			
			mGesture.InterpolateState(mTime, ref mCurrState);
			
			if (Keyboard[Key.S]) {
				// Output the current frame
				var rjs = mCurrState.ToRawJointState();
				Console.WriteLine(rjs.ToString());
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
	}
}
