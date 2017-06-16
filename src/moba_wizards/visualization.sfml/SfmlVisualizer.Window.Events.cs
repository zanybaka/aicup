using System;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Properties;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public partial class SfmlVisualizer
    {
        private void OnClose(object sender, EventArgs e)
        {
            var window = (RenderWindow)sender;
            window.Close();
        }

        private void OnResized(object sender, SizeEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            Vector2f[] size;
            if (e.Width > e.Height)
            {
                size = GetSize(e.Height, e.Height / mapSize.Y*mapSize.X);
            }
            else
            {
                size = GetSize(e.Width, e.Width / mapSize.X * mapSize.Y);
            }
            defaultView.Center = size[0];
            defaultView.Size = size[1];
        }

        
        private void WindowOnMouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            if (e.Wheel != Mouse.Wheel.VerticalWheel)
            {
                return;
            }

            var window = (RenderWindow) sender;
            var view = defaultView;
            var zoom = e.Delta > 0 ? 1/1.1f : 1*1.1f;
            view.Zoom(zoom);
        }

        private void HandleKeyboard()
        {
            var shift = mapSize.X/Settings.Default.ScreenMoveK;
            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                observeTarget = false;
                defaultView.Move(new Vector2f(shift, 0));
                ToggleTargetObserving?.Invoke(this, false);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                observeTarget = false;
                defaultView.Move(new Vector2f(-shift, 0));
                ToggleTargetObserving?.Invoke(this, false);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                observeTarget = false;
                defaultView.Move(new Vector2f(0, -shift));
                ToggleTargetObserving?.Invoke(this, false);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                observeTarget = false;
                defaultView.Move(new Vector2f(0, shift));
                ToggleTargetObserving?.Invoke(this, false);
            }
        }

        private void WindowOnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            mouseWindowPosition = new Vector2f(e.X, e.Y);
            int x = e.X - (int)(defaultView.Size.X / 2 - defaultView.Center.X);
            int y = e.Y - (int)(defaultView.Size.Y / 2 - defaultView.Center.Y);
            mouseMapPosition = new Vector2f(x, y);
        }

        private void WindowOnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Left)
            {
                int x = e.X - (int) (defaultView.Size.X/2 - defaultView.Center.X);
                int y = e.Y - (int) (defaultView.Size.Y/2 - defaultView.Center.Y);
                LeftMousePressed?.Invoke(this, new Vector2i(x, y));
            }
        }

        private void WindowOnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.C)
            {
                observeTarget = true;
                Audios.Wizard_Union_Selected();
                ToggleTargetObserving?.Invoke(this, true);
            }

            if (e.Code == Keyboard.Key.D)
            {
                showDebugInfo = !showDebugInfo;
            }

            if (e.Code == Keyboard.Key.F)
            {
                showDebugInfo = !showDebugInfo;
                showHealth = showDebugInfo;
                showMinimap = showDebugInfo;
                showWayPoints = showDebugInfo;
                // ForceSmooth = showDebugInfo;
            }

            if (e.Code == Keyboard.Key.H)
            {
                showHealth = !showHealth;
            }

            if (e.Code == Keyboard.Key.A)
            {
                if (Audios.EnabledAudio)
                {
                    Audios.DisableAudio();
                }
                Audios.EnabledAudio = !Audios.EnabledAudio;
                ToggleAudio?.Invoke(this, Audios.EnabledAudio);
            }

            if (e.Code == Keyboard.Key.M)
            {
                showMinimap = !showMinimap;
            }

            if (e.Code == Keyboard.Key.W)
            {
                showWayPoints = !showWayPoints;
            }

            if (e.Code == Keyboard.Key.S)
            {
                ForceSmooth = !ForceSmooth;
                if (ForceSmooth)
                {
                    Audios.EnableSmooth();
                }
            }
        }
    }
}