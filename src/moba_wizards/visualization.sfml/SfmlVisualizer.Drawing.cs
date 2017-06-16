using System;
using SFML.Graphics;
using SFML.System;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public partial class SfmlVisualizer
    {
        private void PrintInternal1(RenderTarget target, float x, float y, uint fontSize, string value, params object[] args)
        {
            Text text = new Text(string.Format(value, args), courier, fontSize);
            text.OutlineThickness = 1.5f;
            text.FillColor = new Color(0xFFFF11BB);
            text.OutlineColor = new Color(0xFF0000BB);
            text.Position = new Vector2f(x, y);
            target.Draw(text);
        }

        private void PrintInternal2(RenderTarget target, float x, float y, uint fontSize, Color color, bool bold, string value, params object[] args)
        {
            Text text = new Text(string.Format(value, args), courier, fontSize);
            text.FillColor = color;
            text.Position = new Vector2f(x, y);
            if (bold) text.Style = Text.Styles.Bold;
            target.Draw(text);
        }

        private void CircleInternal(RenderTarget target, float x, float y, float radius, uint color = 0, bool fill = false, uint pointCount = 0)
        {
            float xf = x - radius;
            float yf = y - radius;
            CircleShape shape = new CircleShape(radius);
            if (fill)
            {
                shape.FillColor = new Color(color);
            }
            else
            {
                shape.FillColor = Color.Transparent;
            }
            shape.OutlineThickness = 1.5f;
            shape.OutlineColor = new Color(color);
            shape.Position = new Vector2f(xf, yf);
            shape.SetPointCount(pointCount > 0 ? pointCount : shape.GetPointCount() * 2);
            target.Draw(shape);
        }

        private void LineInternal(RenderTarget target, float x1, float y1, float x2, float y2, uint color = 0)
        {
            VertexArray points = new VertexArray(PrimitiveType.LineStrip);
            var vertex = new Vertex(new Vector2f(x1, y1));
            vertex.Color = new Color(color);
            points.Append(vertex);
            var vertex1 = new Vertex(new Vector2f(x2, y2));
            vertex1.Color = new Color(color);
            points.Append(vertex1);
            target.Draw(points);
        }

        private void RectangleInternal(RenderTarget target, float x1, float y1, float x2, float y2, uint color = 0)
        {
            RectangleShape shape = new RectangleShape(new Vector2f(Math.Abs(x2 - x1), Math.Abs(y2 - y1)));
            shape.FillColor = Color.Transparent;
            shape.OutlineThickness = 1.5f;
            shape.OutlineColor = new Color(color);
            shape.Position = new Vector2f(Math.Min(x1, x2), Math.Min(y1, y2));
            target.Draw(shape);
        }
    }
}