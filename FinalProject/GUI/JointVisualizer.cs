using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace FinalProject
{
	// TODO: merge this with LogFileVisualizer
	public class JointVisualizer : GameWindow
	{
		JointState _mCurrState;
		public JointState CurrState {
			get {
				lock ( this ) {
					return _mCurrState;
				}
			}
			set {
				lock ( this ) {
					_mCurrState = value;
				}
			}
		}
		
		public JointVisualizer()
			: base(960, 480, GraphicsMode.Default, "Joint visual window")
		{
		}
		
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.PointSize(8.0f);
			GL.Enable(EnableCap.DepthTest);
			
			CurrState = null;
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
        }
		
		void RenderJoints() {
			if ( CurrState != null ) {
	            GL.Begin(BeginMode.Points);
				int index = 0;
				foreach ( var joint in CurrState.RelativeJoints ) {
					GL.Color3(0.0f, (float)((index * 16) % 256) / 256.0f, 1.0f);
					GL.Vertex3(joint);
					index += 1;
				}
	            GL.End();
			}
		}
		
		protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			float camDistance = 3.0f;
			float scale = 2.0f;
			Vector3 centerPos = new Vector3(0.0f, -1.0f, 0.0f);
			
			// Front
			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width / 2, ClientRectangle.Height);
			Rescale();
            Matrix4 modelview = Matrix4.LookAt(new Vector3(0.0f, -1.0f, camDistance), centerPos, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
            GL.MultMatrix(ref modelview);
			GL.Scale(scale, scale, scale);
			
			RenderJoints();
			
			// Side
			GL.Viewport(ClientRectangle.X + ClientRectangle.Width / 2, ClientRectangle.Y, ClientRectangle.Width / 2, ClientRectangle.Height);
			Rescale();
            modelview = Matrix4.LookAt(new Vector3(camDistance, -1.0f, 0.0f), centerPos, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
            GL.MultMatrix(ref modelview);
			GL.Scale(scale, scale, scale);
			
			RenderJoints();
			
            SwapBuffers();
        }
	}
}

