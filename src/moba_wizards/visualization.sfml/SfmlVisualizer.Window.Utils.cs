using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using visualization.sfml;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public partial class SfmlVisualizer
    {
        private string GetFPS()
        {
            float currentTime = clock.Restart().AsSeconds();
            float smooth;
            if (Math.Abs(currentTime - lastTime) < 0.001 && lastTime > 0)
            {
                smooth = lastTime;
            }
            else
            {
                smooth = currentTime;
            }
            float fps = 1.0f / smooth;
            lastTime = smooth;

            return fps.ToString("000 fps");
        }

        private float GetTextWidth(string text, Font font, uint fontSize)
        {
            var localBounds = new Text(text, font, fontSize).GetLocalBounds();
            return localBounds.Width + localBounds.Left;
        }

        private float GetTextHeight(string text, Font font, uint fontSize)
        {
            var localBounds = new Text(text, font, fontSize).GetLocalBounds();
            return localBounds.Height + localBounds.Top;
        }

        public bool IsInWindow(double x, double y)
        {
            var hR = defaultView.Size.X / 2;
            var vR = defaultView.Size.Y / 2;
            if (x < defaultView.Center.X - hR) return false;
            if (x > defaultView.Center.X + hR) return false;
            if (y < defaultView.Center.Y - vR) return false;
            if (y > defaultView.Center.Y + vR) return false;
            return true;
        }

        private Vector2f[] GetSize(float width, float height)
        {
            if (worldTexture.Size.X < width && worldTexture.Size.Y < height)
            {
                // full window
                // defaultView.Reset(new FloatRect(0, 0, renderTexture.Size.X, renderTexture.Size.Y));

                // full texture size
                var center = new Vector2f(worldTexture.Size.X / 2f, worldTexture.Size.Y / 2f);
                var size = new Vector2f(width, height);
                return new[] { center, size };
            }
            if (worldTexture.Size.X >= width && worldTexture.Size.Y >= height)
            {
                var center = new Vector2f(width / 2f, height / 2f);
                var size = new Vector2f(width, height);
                return new[] { center, size };
            }
            return null;
        }

        VertexArray GenerateTrianglesStrip(List<Vector2f> points, Color color, float thickness, bool open)
        {
            var array = new VertexArray(PrimitiveType.TrianglesStrip);
            for (int i = 1; i < points.Count + 1 + (open ? 0 : 1); i++)
            {
                Vector2f v0 = points[(i - 1) % points.Count];
                Vector2f v1 = points[i % points.Count];
                Vector2f v2 = points[(i + 1) % points.Count];
                Vector2f v01 = (v1 - v0).Normalized();
                Vector2f v12 = (v2 - v1).Normalized();
                Vector2f d = (v01 + v12).GetNormal();
                float dot = d.Dot(v01.GetNormal());
                d *= thickness / 2f / dot; //< TODO: Add flat miter joint in extreme cases // d *= thickness / 2f / (float)Math.Max(.8, dot);
                if (points.Count == i)
                {
                    array.Append(new Vertex(v0 - d / 4, color));
                    array.Append(new Vertex(v0 + d / 4, color));
                }
                else
                {
                    array.Append(new Vertex(v0 + d, color));
                    array.Append(new Vertex(v0 - d, color));
                }
            }
            return array;
        }

        private void DrawGameUnitSprite(Sprite sprite, double x, double y, byte? alpha = null, RenderTexture renderTexture = null, Vector2f? scale = null, RenderStates? renderStates = null)
        {
            DrawGameUnitSprite(sprite, (float)x, (float)y, alpha, renderTexture, scale, renderStates);
        }

        private void DrawGameUnitSprite(Sprite sprite, float x, float y, byte? alpha = null, RenderTexture renderTexture = null, Vector2f? scale = null, RenderStates? renderStates = null)
        {
            // k = 1
            // sprite.Scale = new Vector2f(Math.Sign(sprite.Scale.X), Math.Sign(sprite.Scale.Y));
            // sprite.Position = new Vector2f(x - sprite.TextureRect.Width / 2f, y - sprite.TextureRect.Height / 2f);

            // k = 2
            sprite.Scale = scale ?? new Vector2f(Math.Sign(sprite.Scale.X) * 2, Math.Sign(sprite.Scale.Y) * 2);
            sprite.Position = new Vector2f(x - sprite.TextureRect.Width, y - sprite.TextureRect.Height);
            if (alpha.HasValue)
            {
                sprite.Color = new Color(255, 255, 255, alpha.Value);
            }
            if (renderTexture != null)
            {
                renderTexture.Draw(sprite, renderStates ?? RenderStates.Default);
            }
            else
            {
                worldTexture.Draw(sprite, renderStates ?? RenderStates.Default);
            }
        }

        private void DrawCooldownTicks(double x, double y, int cooldownTicks, int remainingActionCooldownTicks, Sprite sprite)
        {
            if (remainingActionCooldownTicks != 0)
            {
                var text = remainingActionCooldownTicks.ToString();
                PrintMid(x, y + sprite.TextureRect.Height, text, text);
            }
        }

        private void DrawBuildingFlame(double x, double y, double life, double maxLife, Sprite sprite, byte? alpha = null)
        {
            var healthK = life / maxLife;
            if (healthK < 0.51)
            {
                int numbers = Enum.GetValues(typeof(BigFlameSprite)).Cast<BigFlameSprite>().Count();
                sprite = Sprites.BigFlame((BigFlameSprite)((index / 2) % numbers));
            }
            else if (healthK < 0.75)
            {
                int numbers = Enum.GetValues(typeof(SmallFlameSprite)).Cast<SmallFlameSprite>().Count();
                sprite = Sprites.SmallFlame((SmallFlameSprite)((index / 2) % numbers));
            }
            if (alpha.HasValue)
            {
                sprite.Color = new Color(255, 255, 255, alpha.Value);
            }
            DrawGameUnitSprite(sprite, x, y);
        }

        private void DrawSmallFlame(double x, double y, byte? alpha = null)
        {
            var sprite = Sprites.SmallFlame(GetNumberDependingOnIndex<SmallFlameSprite>());
            if (alpha.HasValue)
            {
                sprite.Color = new Color(255, 255, 255, alpha.Value);
            }
            DrawGameUnitSprite(sprite, x, y);
        }

        private void DrawHealth(Sprite sprite, double x, double y, double life, double maxLife, bool fullSize)
        {
            if (!showHealth) return;
            const float height = 10f;
            const float borderThickness = 1f;
            const float yShift = -height * 2.5f;
            var sX = sprite.TextureRect.Width * Math.Abs(sprite.Scale.X);
            var sY = sprite.TextureRect.Height * Math.Abs(sprite.Scale.Y);
            if (!fullSize)
            {
                sX = sX*3/5;
            }
            RectangleShape shape = new RectangleShape(new Vector2f(sX, height));
            shape.Position = new Vector2f((float)(x - sX / 2f), (float)(y - sY / 2f + yShift));
            shape.FillColor = Color.Black;
            worldTexture.Draw(shape);
            shape = new RectangleShape(new Vector2f((float) ((sX - borderThickness * 2) / maxLife * life), height - borderThickness * 2));
            if (life > 0.75 * maxLife)
            {
                shape.FillColor = new Color(49, 113, 32);
            }
            else if (life > 0.51 * maxLife)
            {
                shape.FillColor = new Color(246, 253, 76);
            }
            else if (life > 0.25 * maxLife)
            {
                shape.FillColor = new Color(255, 198, 0);
            }
            else
            {
                shape.FillColor = new Color(253, 44, 31);
            }
            shape.Position = new Vector2f((float)(x - sX / 2f) + borderThickness, (float)(y - sY / 2f + yShift) + borderThickness);
            worldTexture.Draw(shape);
        }

        private void DrawXp(Sprite sprite, double x, double y, double xp, double level)
        {
            if (!showHealth) return;
            const float height = 10f;
            const float borderThickness = 1f;
            const float yShift = -height * 1f;
            const int fontSize = 11;
            var sX = sprite.TextureRect.Width * Math.Abs(sprite.Scale.X);
            var sY = sprite.TextureRect.Height * Math.Abs(sprite.Scale.Y);
            RectangleShape shape = new RectangleShape(new Vector2f(sX, height));
            shape.Position = new Vector2f((float)(x - sX / 2f), (float)(y - sY / 2f + yShift));
            shape.FillColor = Color.Black;
            worldTexture.Draw(shape);
            shape = new RectangleShape(new Vector2f((float)((sX - borderThickness * 2) / maxLevelXp * xp), height - borderThickness * 2));
            shape.FillColor = new Color(31, 44, (byte) (253 - 0.5 * (100 * level / maxLevel)));
            shape.Position = new Vector2f((float)(x - sX / 2f) + borderThickness, (float)(y - sY / 2f + yShift) + borderThickness);
            string text = string.Format("{0}/{1} ({2})", xp, maxLevelXp, level);
            PrintInternal2(worldTexture, shape.Position.X + sX / 2 - GetTextWidth(text, courier, fontSize) / 2, shape.Position.Y - 3, fontSize, Color.White, true, text);
            worldTexture.Draw(shape);
        }
    }
}