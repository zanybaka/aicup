using System;
using System.Linq;
using SFML.Graphics;
using SFML.System;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public partial class SfmlVisualizer
    {
        public void Init(int index)
        {
            this.index = index;
            InitFogOfWarMap();
        }

        public void BeginDrawing(double targetX, double targetY)
        {
            targetToObserve = new Vector2f((float)targetX, (float)targetY);
            window.DispatchEvents();
            HandleKeyboard();
            if (observeTarget)
            {
                defaultView.Center = targetToObserve;
            }
            window.Clear();
            if (index % FogOfWarFreq == 0)
            {
                visibleAreaTexture.Clear();
            }
            DrawWorld();
        }

        public void EndDrawing()
        {
            DrawFogOfWar();
            DrawInfoUnderMouseCursor();
            worldTexture.Display();
            var world = new Sprite(worldTexture.Texture);
            window.Draw(world);
            DrawMinimap(world);
            DrawDebugInfo();
            window.SetView(defaultView);
            window.Display();
        }

        public void PrintMid(double x, double y, string len, string value, params object[] args)
        {
            if (!showDebugInfo) return;
            float textWidth = GetTextWidth(string.Format(len, args), courier, 18);
            double xFixed = x - textWidth / 2;
            double yFixed = y;

            Print(xFixed, yFixed, value, args);
        }

        public void Print(double x, double y, string value, params object[] args)
        {
            if (!showDebugInfo) return;
            PrintInternal1(worldTexture, (float) x, (float) y, DefaultFontSize, value, args);
        }

        public void DrawCircle(double x, double y, double radius, uint color = 0, bool fill = false)
        {
            if (!showDebugInfo) return;
            CircleInternal(worldTexture, (float) x, (float) y, (float) radius, color, fill);
        }

        public void DrawLine(double x1, double y1, double x2, double y2, uint color = 0xFFFFFFFF)
        {
            if (!showDebugInfo) return;
            LineInternal(worldTexture, (float) x1, (float) y1, (float) x2, (float) y2, color);
        }

        public void DrawRectangle(double x1, double y1, double x2, double y2, uint color = 0)
        {
            if (!showDebugInfo) return;
            RectangleInternal(worldTexture, (float)x1, (float)y1, (float)x2, (float)y2, color);
        }

        public bool IsInVisibleArea(double x, double y)
        {
            float k = mapSize.X / 64;
            float xk = (float)(x / mapSize.X * k);
            float yk = (float)(y / mapSize.X * k);
            if (xk < 0 || yk < 0 || xk >= visibleArea.Length || yk >= visibleArea.Length) return false;
            return visibleArea[(int)xk][(int)yk] != 0;
        }

        public T GetNumberDependingOnIndex<T>(int speed = 2)
        {
            int numbers = Enum.GetValues(typeof(T)).Cast<T>().Count();
            return (T)Enum.ToObject(typeof(T), (index / speed) % numbers);
        }

        public void AddTree(double x, double y, double radius)
        {
            // r > 20, r < 50
            float k = mapSize.X / 64;
            int xk = (int)(x / mapSize.X * k);
            int yk = (int)(y / mapSize.X * k);
            if (xk < 0 || yk < 0 || xk >= visibleArea.Length || yk >= visibleArea.Length - 1) return;

            visibleArea[xk][yk] = 2;
            visibleArea[xk][yk + 1] = 2;
            if (radius > 32 && xk < visibleArea.Length - 1)
            {
                visibleArea[xk + 1][yk] = 2;
                visibleArea[xk + 1][yk + 1] = 2;
            }
            // CircleInternal(worldTexture, (float)x, (float)y, (float)radius, 0xFFFFFFFF, true, 10);
        }

        public void EnableTargetObservation()
        {
            observeTarget = true;
        }

        public void DrawTrees()
        {
            for (int i = 0; i < visibleArea.Length; i++)
            {
                for (int j = 0; j < visibleArea[i].Length; j++)
                {
                    if (visibleArea[i][j] == 2)
                    {
                        if (j == 0 || visibleArea[i][j - 1] != 2)
                        {
                            TreeHead(i*64, j*64);
                        }
                        else if (j == visibleArea[i].Length - 1 || visibleArea[i][j + 1] != 2)
                        {
                            TreeFooter(i*64, j*64);
                        }
                        else
                        {
                            TreeBody(i * 64, j * 64);
                        }
                    }
                }
            }
        }

        public void AddVisibleArea(double x, double y, double radius)
        {
            float k = mapSize.X/64;
            float xk = (float) (x / mapSize.X * k);
            float yk = (float) (y / mapSize.X * k);
            float rk = (float) (radius / mapSize.X * k);
            for (float i = xk-rk; i < xk+rk; i++)
            {
                for (float j = yk - rk; j < yk + rk; j++)
                {
                    // Fog of war as in W2 < rk + 0.5
                    // Fog of war with circles <= rk
                    // Fog of war with light <= rk
                    if (Math.Sqrt((xk - i)* (xk - i) + (yk - j)*(yk - j)) < rk + 0.5)
                    {
                        if (i < 0 || j < 0 || i >= visibleArea.Length || j >= visibleArea.Length) continue;
                        visibleArea[(int) i][(int) j] = 1;
                    }
                }
            }
            // Fog of war with circles
            //if (index%60 == 0)
            //{
            // CircleInternal(visibleAreaTexture, (float)x, (float)y, (float)radius, 0xFFFFFFFF, true);
            //}

            // Fog of war with light
            // DrawLight(visibleAreaTexture, x, y, radius);
        }

        private void InitFogOfWarMap()
        {
            visibleArea = new byte[63][];
            for (int i = 0; i < visibleArea.Length; i++)
            {
                visibleArea[i] = new byte[63];
            }
            // Fog of war with circles
            // Fog of war with light
            // visibleAreaTexture.Clear();
        }
    }
}