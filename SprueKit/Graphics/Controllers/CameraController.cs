using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using System.Windows.Input;
using SprueKit.Graphics.SprueModel;

namespace SprueKit.Graphics.Controllers
{
    public enum CameraMode
    {
        ArcBall,
        Flight,
    }

    public class CameraController
    {
        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();
        IOCDependency<Settings.ViewportSettings> viewportSettings = new IOCDependency<Settings.ViewportSettings>();

        bool[] mouseDown_ = new bool[3];
        System.Windows.Point? lastMouse_ = null;
        BaseScene elem_;
        ViewportDelegate viewport_;
        public Camera camera;

        public bool PrimaryIsOrbit = true;
        public float AnimationDistanceMultipler = 1.0f;
        float accelerationCount = 0.0f;
        const float accelerationSteps = 64.0f;

        public Vector3 OrbitOrigin = Vector3.Zero;

        public CameraController(BaseScene elem, ViewportDelegate viewport, Camera camera)
        {
            viewport_ = viewport;
            this.camera = camera;
            elem_ = elem;
            elem.PreviewMouseDown += Elem_MouseState;
            elem.PreviewMouseUp += Elem_MouseState;
            elem.PreviewMouseMove += Elem_MouseMove;
            elem.PreviewKeyUp += Elem_KeyUp;
            elem.MouseWheel += Elem_MouseWheel;
        }

        public void Update(float td)
        {
            if (!viewport_.IsActive)
                return;

            if (elem_.IsFocused && elem_.IsMouseDirectlyOver)
            {
                bool shiftDown = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);

                float speedFactor = shiftDown ? UserData.inst().ViewportSettings.FastMovementSpeed : UserData.inst().ViewportSettings.BaseMovementSpeed;

                if (Keyboard.IsKeyDown(viewportSettings.Object.Forward))
                    camera.Position += camera.Forward * 0.1f * speedFactor;
                if (Keyboard.IsKeyDown(viewportSettings.Object.Backward))
                    camera.Position -= camera.Forward * 0.1f * speedFactor;
                if (Keyboard.IsKeyDown(viewportSettings.Object.PanLeft))
                    camera.Position -= camera.Right * 0.1f * speedFactor;
                if (Keyboard.IsKeyDown(viewportSettings.Object.PanRight))
                    camera.Position += camera.Right * 0.1f * speedFactor;
                if (Keyboard.IsKeyDown(viewportSettings.Object.PanUp))
                    camera.Position += camera.UpDir * 0.1f * speedFactor;
                if (Keyboard.IsKeyDown(viewportSettings.Object.PanDown))
                    camera.Position -= camera.UpDir * 0.1f * speedFactor;
            }
        }

        private void Elem_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!viewport_.IsActive)
                return;

            //if (elem_.IsMouseDirectlyOver)
            {
                if (e.Delta > 0)
                {
                    camera.Position += camera.Forward;
                    camera.LookAtDir(camera.Forward);
                }
                else if (e.Delta < 0)
                {
                    camera.Position -= camera.Forward;
                    camera.LookAtDir(camera.Forward);
                }
            }
        }

        float CalculateFitmentDistance(out Vector3 centroid)
        {
            centroid = new Vector3();
            if (documentManager.Object.ActiveDocument is SprueKit.Data.SprueModelDocument)
            {
                Vector3 pos = new Vector3();
                float ct = 0.0f;
                Vector3 min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
                Vector3 max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
                ((SprueKit.Data.SprueModelDocument)documentManager.Object.ActiveDocument).DataRoot.VisitAll((p) =>
                {
                    ct += 1.0f;
                    pos = pos + p.Position;
                    min = Vector3.Min(min, p.Position);
                    max = Vector3.Max(max, p.Position);
                });

                if (ct == 0.0f) // should always have 'something' but just in case
                    return 1.0f;

                pos.X = pos.X / ct;
                pos.Y = pos.Y / ct;
                pos.Z = pos.Z / ct;
                centroid = pos;

                float radius = (max - min).Length() / 2;
                double camDistance = (radius * 2.0) / Math.Tan((45 * Mathf.DEGTORAD) / 2.0);

                return (float)(camDistance / 3.5);
            }
            return 1.0f;
        }

        public void Focus()
        {
            Vector3 centroid = new Vector3();
            float dist = CalculateFitmentDistance(out centroid);
            Vector3 zoomDir = new Vector3(1, 1, 1) * dist;
            camera.ActiveAnimation = new VizAnim.CameraAnimation(zoomDir, centroid, 0.3f);
        }

        private void Elem_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!viewport_.IsActive)
                return;

            // Camera control commands
            // Home = reset to default view
            // End = Look at selection, or scene center
            // Numpad 8 look at front-side
            // Numpad 2 look at back-side
            // Numpad 4 look at left-side
            // Numpad 6 look at right-side
            // Numpad 5 look at top

            if (!elem_.IsFocused)
                return;

            float pos64 = 64.0f * AnimationDistanceMultipler;
            float neg64 = -64.0f * AnimationDistanceMultipler;
            float pos3 = 3.0f * AnimationDistanceMultipler;
            float neg3 = -3.0f * AnimationDistanceMultipler;

            if (e.Key == viewportSettings.Object.ResetView)
            {
                if (documentManager.Object.ActiveDocument != null)
                {
                    // Set ourself to fit everything into view
                    if (documentManager.Object.ActiveDocument is SprueKit.Data.SprueModelDocument)
                    {
                        Vector3 centroid = new Vector3();
                        float dist = CalculateFitmentDistance(out centroid);
                        Vector3 zoomDir = new Vector3(1, 1, 1) * dist;
                        camera.ActiveAnimation = new VizAnim.CameraAnimation(zoomDir, centroid, 0.3f);
                        return;
                    }
                }

                // Fallback, just set ourself to near the center
                camera.ActiveAnimation = new VizAnim.CameraAnimation(new Vector3(5, 5, 5), Vector3.Zero, 0.3f);
            }
            else if (e.Key == viewportSettings.Object.LookAtSelection)
            {
                Vector3? selPos = SelectedPosition();
                if (selPos.HasValue)
                    camera.ActiveAnimation = new VizAnim.CameraLookAtAnimation(selPos.Value, 0.2f);
                else
                    camera.ActiveAnimation = new VizAnim.CameraLookAtAnimation(Vector3.Zero, 0.2f);
            }
            else if (e.Key == System.Windows.Input.Key.NumPad4) //look to right, from left
                camera.ActiveAnimation = new VizAnim.CameraAnimation(new Vector3(neg64, pos3, 0), new Vector3(0, pos3, 0), 0.3f);
            else if (e.Key == System.Windows.Input.Key.NumPad2) //look to front, from back
                camera.ActiveAnimation = new VizAnim.CameraAnimation(new Vector3(0, pos3, neg64), new Vector3(0, pos3, 0), 0.3f);
            else if (e.Key == System.Windows.Input.Key.NumPad8) //look to back, from front
                camera.ActiveAnimation = new VizAnim.CameraAnimation(new Vector3(0, pos3, pos64), new Vector3(0, pos3, 0), 0.3f);
            else if (e.Key == System.Windows.Input.Key.NumPad6) //look to left, from right
                camera.ActiveAnimation = new VizAnim.CameraAnimation(new Vector3(pos64, pos3, 0), new Vector3(0, pos3, 0), 0.3f);
            else if (e.Key == System.Windows.Input.Key.NumPad5) //down from top
                camera.ActiveAnimation = new VizAnim.CameraAnimation(new Vector3(0, pos64, 0.01f), Vector3.Zero, 0.3f);
        }

        private void Elem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!viewport_.IsActive)
                return;

            System.Windows.Point newMouse = e.GetPosition(elem_);
            if (!lastMouse_.HasValue)
            {
                lastMouse_ = newMouse;
                return;
            }

            if (!elem_.IsMouseDirectlyOver && !elem_.IsVisible)
                return;

            if (mouseDown_[0] || mouseDown_[1] || mouseDown_[2])
            {
                float dx = (float)(newMouse.X - lastMouse_.Value.X);
                float dy = (float)(newMouse.Y - lastMouse_.Value.Y);

                Vector2 vec;// FramePool<Vector2>.obtain();
                vec.X = dx;
                vec.Y = dy;
                OnMouseMove(vec);
                e.Handled = true;
            }
            lastMouse_ = newMouse;
        }

        private void Elem_MouseState(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mouseDown_[0] = e.LeftButton == System.Windows.Input.MouseButtonState.Pressed;
            mouseDown_[1] = e.RightButton == System.Windows.Input.MouseButtonState.Pressed;
            mouseDown_[2] = e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed;

            if (!elem_.IsMouseDirectlyOver && !elem_.IsVisible)
                return;

            if (!viewport_.IsActive)
                return;
            elem_.Focus();

            if (mouseDown_[1] && e.ChangedButton == MouseButton.Right)
            {
                Mouse.Capture(elem_);
                Mouse.OverrideCursor = Cursors.SizeAll;
                e.Handled = true;
            }
            else if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                elem_.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                accelerationCount = 0.0f;
                e.Handled = true;
            }

            if (mouseDown_[2] && e.ChangedButton == MouseButton.Middle)
            {
                Mouse.Capture(elem_);
                Mouse.OverrideCursor = Cursors.SizeNS;
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {
                elem_.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                accelerationCount = 0.0f;
                e.Handled = true;
            }
        }

        protected virtual void OnMouseMove(Vector2 delta)
        {
            if (!viewport_.IsActive)
                return;

            float accelerationFactor = Math.Min(accelerationCount / accelerationSteps, 1.0f);
            float multiplier = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? viewportSettings.Object.FastMovementSpeed : viewportSettings.Object.BaseMovementSpeed;

            // Angular control
            if (mouseDown_[1])
            {
                const float pitchFactor = 1.0f;
                const float yawFactor = 1.0f;
                bool anyChanges = false;
                float xx = 0.0f;
                float yy = 0.0f;

                // flip the Y axis
                if (viewportSettings.Object.InvertYAxis)
                    delta.Y = -delta.Y;

                bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                // Turn in place
                if ((shiftDown && PrimaryIsOrbit) || (!shiftDown && !PrimaryIsOrbit))
                {
                    if (Math.Abs(delta.Y) > 0)
                    {
                        camera.Pitch(delta.Y * 0.2f);// > 0 ? pitchFactor : -pitchFactor) * 0.4f);
                        anyChanges = true;
                    }
                    if (Math.Abs(delta.X) > 0)
                    {
                        camera.Yaw(delta.X * 0.2f);// > 0 ? yawFactor : -yawFactor) * 0.4f);
                        anyChanges = true;
                    }
                }
            // Orbit around point
                else
                {
                    if (Math.Abs(delta.Y) > 0)
                        yy = (delta.Y > 0 ? 1.0f : -1.0f);
                    if (Math.Abs(delta.X) > 0)
                        xx = (delta.X > 0 ? 1.0f : -1.0f);

                    if (xx != 0 || yy != 0)
                    {
                        anyChanges = true;

                        Vector3? selPos = SelectedPosition();
                        camera.OrbitAround(selPos.HasValue ? selPos.Value : OrbitOrigin, delta.X * accelerationFactor, delta.Y * accelerationFactor);
                    }
                }

                if (!anyChanges)
                    accelerationCount = 0.0f;
                accelerationCount += 1.0f;
            }
            
        // Dolly
            if (mouseDown_[2])
            {
                if (Math.Abs(delta.Y) > 0)
                {
                    camera.Position += camera.Forward * ((delta.Y > 0 ? 0.5f : -0.5f) * accelerationFactor) * multiplier;
                    accelerationCount += 1.0f;
                }
                else
                    accelerationCount = 0;
            }

        // Picking
            if (mouseDown_[0])
            {

            }
        }

        private Vector3? SelectedPosition()
        {
            if (documentManager.Object.ActiveDocument != null)
            {
                object sel = documentManager.Object.ActiveDocument.Selection.MostRecentlySelected;
                if (sel != null)
                {
                    SprueKit.Data.SpruePiece piece = sel as SprueKit.Data.SpruePiece;
                    if (piece != null)
                        return piece.Position;
                }
            }
            return null;
        }
    }

    public class OrthographicCameraController
    {
        IOCDependency<Settings.ViewportSettings> viewportSettings = new IOCDependency<Settings.ViewportSettings>();
        Camera camera_;
        ViewportDelegate viewport_;
        BaseScene elem_;
        System.Windows.Point? lastMouse_;
        bool useLeftToDrag_ = true;
        bool[] mouseDown_ = new bool[3];

        public bool UseLeftToDrag { get { return useLeftToDrag_; } set { useLeftToDrag_ = value; } }

        public OrthographicCameraController(BaseScene elem, ViewportDelegate viewport, Camera camera)
        {
            camera_ = camera;
            viewport_ = viewport;
            elem_ = elem;
            elem.PreviewMouseDown += Elem_MouseState;
            elem.PreviewMouseUp += Elem_MouseState;
            elem.PreviewMouseMove += Elem_MouseMove;
            elem.PreviewKeyUp += Elem_KeyUp;
            elem.MouseWheel += Elem_MouseWheel;
        }

        public void Update(float td)
        {
            if (!viewport_.IsActive)
                return;

            if (elem_.IsFocused && elem_.IsMouseDirectlyOver)
            {
                bool shiftDown = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);

                float speedFactor = shiftDown ? UserData.inst().ViewportSettings.FastMovementSpeed : UserData.inst().ViewportSettings.BaseMovementSpeed;
            }
        }

        private void Elem_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!viewport_.IsActive)
                return;

            int delta = e.Delta;
            if (delta > 0)
                camera_.OrthoScaling *= 0.9f;
            else if (delta < 0)
                camera_.OrthoScaling *= 1.1f;
        }

        private void Elem_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!viewport_.IsActive)
                return;

            if (elem_.IsFocused)
            {
                if (e.Key == Key.Home || e.Key == Key.End)
                    camera_.ActiveAnimation = new VizAnim.CameraPositionAnimation(new Vector3(0.5f, 0.5f, 1), 0.3f);
            }
        }

        private void Elem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!viewport_.IsActive)
                return;

            System.Windows.Point newMouse = e.GetPosition(elem_);
            if (!lastMouse_.HasValue)
            {
                lastMouse_ = newMouse;
                return;
            }

            if ((UseLeftToDrag && mouseDown_[0]) || mouseDown_[1] || mouseDown_[2])
            {
                float dx = (float)(newMouse.X - lastMouse_.Value.X);
                float dy = (float)(newMouse.Y - lastMouse_.Value.Y);

                Vector2 vec;// FramePool<Vector2>.obtain();
                vec.X = dx;
                vec.Y = dy;
                OnMouseMove(vec);
                e.Handled = true;
            }
            lastMouse_ = newMouse;
        }

        private void Elem_MouseState(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!viewport_.IsActive)
                return;

            if (elem_.IsMouseDirectlyOver && elem_.IsVisible)
            {
                mouseDown_[0] = e.LeftButton == System.Windows.Input.MouseButtonState.Pressed;
                mouseDown_[1] = e.RightButton == System.Windows.Input.MouseButtonState.Pressed;
                mouseDown_[2] = e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed;

                elem_.Focus();

                if (mouseDown_[1] && e.ChangedButton == MouseButton.Right)
                {
                    Mouse.Capture(elem_);
                    Mouse.OverrideCursor = Cursors.SizeAll;
                    e.Handled = true;
                }
                else if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
                {
                    elem_.ReleaseMouseCapture();
                    Mouse.OverrideCursor = null;
                    e.Handled = true;
                    //accelerationCount = 0.0f;
                }

                if (mouseDown_[2] && e.ChangedButton == MouseButton.Middle)
                {
                    Mouse.Capture(elem_);
                    Mouse.OverrideCursor = Cursors.SizeNS;
                    e.Handled = true;
                }
                else if (e.ChangedButton == MouseButton.Middle)
                {
                    elem_.ReleaseMouseCapture();
                    Mouse.OverrideCursor = null;
                    //accelerationCount = 0.0f;
                }
            }
        }

        protected virtual void OnMouseMove(Vector2 delta)
        {
            if (!viewport_.IsActive)
                return;

            if (mouseDown_[1] || mouseDown_[0])
            {
                camera_.Position += new Vector3(-delta.X * 0.0025f, delta.Y * 0.0025f, 0) * camera_.OrthoScaling.X;
            }
        }
    }
}
