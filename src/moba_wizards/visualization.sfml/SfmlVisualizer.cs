using System;
using System.IO;
using System.Windows.Forms;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Properties;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using visualization.sfml;
using View = SFML.Graphics.View;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public partial class SfmlVisualizer
    {
        public static bool ForceSmooth;
        public event EventHandler<bool> ToggleAudio;
        public event EventHandler<bool> ToggleTargetObserving;
        public event EventHandler<Vector2i> LeftMousePressed;
        private uint WindowSizeW;
        private uint WindowSizeH;
        private readonly Sprite background;
        private readonly Clock clock;
        private readonly Font courier;
        private readonly View defaultView;
        private readonly View minimapView;
        private readonly RenderWindow window;
        private readonly RenderTexture worldTexture;
        private readonly RenderTexture tmpTexture;
        private readonly RenderTexture visibleAreaTexture;
        private readonly View worldView;
        private float lastTime;
        private Vector2f mapSize = new Vector2f(4000, 4000);
        private RenderTexture minimapTexture;
        private Vector2f targetToObserve;
        private Vector2f mouseWindowPosition;
        private Vector2f mouseMapPosition;
        private bool observeTarget;
        private bool showDebugInfo;
        private bool showHealth;
        private bool showWayPoints;
        private bool showMinimap;
        private const uint DefaultFontSize = 18;
        private const uint FogOfWarFreq = 90;
        private Random rnd = new Random();
        // 0 - invisible
        // 1 - visible
        // 2 - tree
        private byte[][] visibleArea;
        private Vector2f[] respawnUnionPoints;
        private Vector2f[] respawnEnemyPoints;
        private WayPointInfo[][] respawnUnionWayPoints;
        private WayPointInfo[][] respawnEnemyWayPoints;
        private int index;
        private int maxLevelXp;
        private int maxLevel;

        public SfmlVisualizer()
        {
            // init sprites
            background = Sprites.Background_Union();

            // init fonts
            var courierFontFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "cour.ttf");
            if (!File.Exists(courierFontFileName))
            {
                courierFontFileName = Path.Combine(Settings.Default.ResourcesDir, "cour.ttf");
                if (!File.Exists(courierFontFileName))
                {
                    MessageBox.Show($"cour.ttf could not be found, please copy it into the folder '{Settings.Default.ResourcesDir}'");
                }
            }
            courier = new Font(courierFontFileName);

            // init window
            var contextSettings = new ContextSettings();
            WindowSizeW = Settings.Default.WindowSizeW;
            WindowSizeH = Settings.Default.WindowSizeH;
            window = new RenderWindow(new VideoMode(WindowSizeW, WindowSizeH), "AI Cup 2016 Visualizer", Styles.Resize | Styles.Close, contextSettings);
            window.SetVerticalSyncEnabled(false);
            window.Closed += OnClose;
            // window.Resized += OnResized;
            window.MouseWheelScrolled += WindowOnMouseWheelScrolled;
            window.KeyPressed += WindowOnKeyPressed;
            window.MouseButtonReleased += WindowOnMouseButtonReleased;
            window.MouseMoved += WindowOnMouseMoved;

            // generate background random tiles
            tmpTexture = new RenderTexture((uint)mapSize.X, (uint)mapSize.Y);
            tmpTexture.Draw(background);
            DrawGroundRandomTiles(tmpTexture);
            tmpTexture.Display();
            background = new Sprite(tmpTexture.Texture);
            // background.Texture.CopyToImage().SaveToFile(Path.Combine(Settings.Default.ResourcesDir, "1.bmp"));

            // init views
            defaultView = window.DefaultView;
            worldView = new View(new FloatRect(0, 0, mapSize.X, mapSize.Y));
            worldTexture = new RenderTexture((uint) mapSize.X, (uint) mapSize.Y);
            visibleAreaTexture = new RenderTexture((uint) mapSize.X, (uint) mapSize.Y);
            minimapTexture = new RenderTexture((uint) mapSize.X, (uint) mapSize.Y);
            minimapView = new View(new FloatRect(0, 0, mapSize.X, mapSize.Y));

            // init other stuff
            clock = new Clock();
            observeTarget = Settings.Default.ObserveTarget;
            showDebugInfo = false;
            showHealth = false;
            showMinimap = Settings.Default.ShowMinimap;
            ForceSmooth = false;
            showWayPoints = false;
            Audios.EnabledAudio = Settings.Default.EnableAudio;
        }

        private void DrawGroundRandomTiles(RenderTexture texture)
        {
            for (int i = 0; i < 200; i++)
            {
                var sprite = Sprites.Ground_Union(i % 12 + GroundSprite.Sprite1);
                float x = rnd.Next(0, (int)(mapSize.X / sprite.TextureRect.Width / 2)) * sprite.TextureRect.Width * 2;
                float y = rnd.Next(0, (int)(mapSize.Y / sprite.TextureRect.Height / 2)) * sprite.TextureRect.Height * 2;
                sprite.Scale = new Vector2f(2, 2);
                sprite.Origin = new Vector2f(0, -16);
                sprite.Position = new Vector2f(x, y);
                texture.Draw(sprite);
            }
        }

        private void DrawInfoUnderMouseCursor()
        {
            if (mouseMapPosition.GetHashCode() > 0)
            {
                string text = $"({mouseMapPosition.X:####};{mouseMapPosition.Y:####})";
                PrintMid(mouseMapPosition.X, mouseMapPosition.Y, text, text);
            }
        }

        private void DrawWorld()
        {
            worldTexture.Clear();
            {
                worldTexture.SetView(worldView);

                //background = Sprites.Ground_Union();
                //var texture = new RenderTexture((uint) background.TextureRect.Width, (uint) background.TextureRect.Height);
                //texture.Draw(background);
                //texture.Texture.Repeated = true;
                //texture.Display();
                //background = new Sprite(texture.Texture, new IntRect(0, 0, 4000, 4000));
                var stepX = background.TextureRect.Width;
                var stepY = background.TextureRect.Height;
                for (double x = 0; x < mapSize.X; x += stepX)
                {
                    for (double y = 0; y < mapSize.Y; y += stepY)
                    {
                        background.Position = new Vector2f((float)x, (float)y);
                        worldTexture.Draw(background);
                    }
                }
            }
        }

        private void DrawFogOfWar()
        {
            if (index % FogOfWarFreq == 0)
            {
                for (int i = 0; i < visibleArea.Length; i++)
                {
                    for (int j = 0; j < visibleArea[i].Length; j++)
                    {
                        if (visibleArea[i][j] != 0)
                        {
                            Sprite sprite = Sprites.White();
                            //if (i == 0 || (i == visibleArea.Length - 1) || j == 0 || (j == visibleArea[i].Length - 1))
                            {
                                // border
                            }
                            // else
                            {
                                var isLeftBlack = i > 0 && visibleArea[i - 1][j] == 0;
                                var isRightBlack = (i < visibleArea.Length - 1) && visibleArea[i + 1][j] == 0;
                                var isTopBlack = j > 0 && visibleArea[i][j - 1] == 0;
                                var isBottomBlack = (j < visibleArea[i].Length - 1) && visibleArea[i][j + 1] == 0;
                                //var isLeftBlack = i == 0 || visibleArea[i - 1][j] == 0;
                                //var isRightBlack = (i == visibleArea.Length - 1) || visibleArea[i + 1][j] == 0;
                                //var isTopBlack = j == 0 || visibleArea[i][j - 1] == 0;
                                //var isBottomBlack = (j == visibleArea[i].Length - 1) || visibleArea[i][j + 1] == 0;
                                if (isLeftBlack)
                                {
                                    if (isTopBlack) sprite = Sprites.FogTopLeft();
                                    else if (isBottomBlack) sprite = Sprites.FogBottomLeft();
                                    else /*if (i != 0) */sprite = Sprites.FogLeft();
                                }
                                else if (isTopBlack)
                                {
                                    if (isRightBlack) sprite = Sprites.FogTopRight();
                                    else if (isBottomBlack) sprite = Sprites.FogMidVertical();
                                    else /*if (j != 0) */sprite = Sprites.FogTop();
                                }
                                else if (isRightBlack)
                                {
                                    if (isBottomBlack) sprite = Sprites.FogBottomRight();
                                    else /*if (i != visibleArea.Length - 1) */sprite = Sprites.FogRight();
                                }
                                else if (isBottomBlack/* && (j != visibleArea[i].Length - 1)*/)
                                {
                                    sprite = Sprites.FogBottom();
                                }
                                else
                                {
                                    var isLeftTopBlack = i > 0 && j > 0 && visibleArea[i - 1][j - 1] == 0;
                                    var isRightTopBlack = (i < visibleArea.Length - 1) && j > 0 &&
                                                          visibleArea[i + 1][j - 1] == 0;
                                    var isRightBottomBlack = (i < visibleArea.Length - 1) &&
                                                             (j < visibleArea[i].Length - 1) &&
                                                             visibleArea[i + 1][j + 1] == 0;
                                    var isLeftBottomBlack = i > 0 && (j < visibleArea[i].Length - 1) &&
                                                            visibleArea[i - 1][j + 1] == 0;
                                    //var isLeftTopBlack = i == 0 || j == 0 || visibleArea[i - 1][j - 1] == 0;
                                    //var isRightTopBlack = (i == visibleArea.Length - 1) || j == 0 ||
                                    //                      visibleArea[i + 1][j - 1] == 0;
                                    //var isRightBottomBlack = (i == visibleArea.Length - 1) ||
                                    //                         (j == visibleArea[i].Length - 1) ||
                                    //                         visibleArea[i + 1][j + 1] == 0;
                                    //var isLeftBottomBlack = i == 0 || (j == visibleArea[i].Length - 1) ||
                                    //                        visibleArea[i - 1][j + 1] == 0;
                                    if (isLeftTopBlack && isRightBottomBlack)
                                        sprite = Sprites.FogTopLeftAndBottomRightSingle();
                                    else if (isRightTopBlack && isLeftBottomBlack)
                                        sprite = Sprites.FogBottomLeftAndTopRightSingle();
                                    else if (isLeftTopBlack) sprite = Sprites.FogTopLeftSingle();
                                    else if (isRightTopBlack) sprite = Sprites.FogTopRightSingle();
                                    else if (isRightBottomBlack) sprite = Sprites.FogBottomRightSingle();
                                    else if (isLeftBottomBlack) sprite = Sprites.FogBottomLeftSingle();
                                }
                            }
                            sprite.Position = new Vector2f(i*64, j*64);
                            sprite.Scale = new Vector2f(2, 2);
                            visibleAreaTexture.Draw(sprite, new RenderStates(BlendMode.Add));
                        }
                    }
                }
            }
            visibleAreaTexture.Display();
            var texture = visibleAreaTexture.Texture;
            var fogMap = new Sprite(texture);
            worldTexture.Draw(fogMap, new RenderStates(BlendMode.Multiply));
        }

        private void DrawMinimap(Sprite world)
        {
            if (!showMinimap) return;
            window.SetView(window.DefaultView);
            var sprite = Sprites.Minimap();
            sprite.Position = new Vector2f(window.Size.X * 0.75f - 2, window.Size.X * 0.75f - 2);
            // sprite.Scale = new Vector2f(1.875f, 1.875f);
            sprite.Scale = new Vector2f(1.69f, 1.69f);
            window.Draw(sprite); // minimap border

            minimapView.Viewport = new FloatRect(0.75f, 0.75f, 0.2f, 0.2f);
            window.SetView(minimapView);
            window.Draw(world);
            float borderSize = mapSize.X * 0.01f;
            var center = GetSize(window.Size.X, window.Size.Y)[0];
            float x = borderSize + defaultView.Center.X - center.X;
            float y = borderSize + defaultView.Center.Y - center.Y;
            var w = Math.Min(defaultView.Size.X, worldView.Size.X) - borderSize*2;
            var h = Math.Min(defaultView.Size.Y, worldView.Size.Y) - borderSize*2;
            var rectangle = new RectangleShape(new Vector2f(w, h));
            // rectangle.OutlineColor = new Color(0xFF001199);
            rectangle.OutlineColor = new Color(0xFFFFFF99);
            rectangle.FillColor = Color.Transparent;
            rectangle.OutlineThickness = borderSize;
            rectangle.Position = new Vector2f(x, y);
            window.Draw(rectangle); // current view border on minimap
        }

        private void DrawDebugInfo()
        {
            uint fontSize = 12u;
            string helpText;
            window.SetView(window.DefaultView);
            if (!showDebugInfo)
            {
                helpText = "D            - Show/Hide debug info";
                PrintInternal1(window, fontSize, window.Size.Y - GetTextHeight(helpText, courier, fontSize) - fontSize, fontSize, helpText);
                return;
            }
            
            PrintInternal1(window, 0, 0, DefaultFontSize, GetFPS());
            helpText = "Keys:\n" +
                           "D            - Show/Hide debug info\n" +
                           "A            - Audio on/off\n" +
                           "C            - Center\n" +
                           "F            - Show/Hide all info\n" +
                           "M            - Show/Hide map\n" +
                           "H            - Show/Hide health/xp\n" +
                           "W            - Show/Hide way points\n" +
                           "S            - On/Off force smooth\n" +
                           "left arrow   - Move view left\n" +
                           "right arrow  - Move view right\n" +
                           "up arrow     - Move view up\n" +
                           "down arrow   - Move view down\n" +
                           "ctrl+F12     - Exit\n" +
                           "mouse scroll - Zoom";
            PrintInternal1(window, fontSize, window.Size.Y - GetTextHeight(helpText, courier, fontSize) - fontSize, fontSize, helpText);
            /*var s1 = Sprites.MagicMissle_Union(0);
            s1.Position = new Vector2f(16, 16);
            window.Draw(s1);
            var s2 = Sprites.MagicMissle_Union(0.86);
            s2.Position = new Vector2f(16+100, 16);
            window.Draw(s2);
            var s3 = Sprites.MagicMissle_Union(1.57);
            s3.Position = new Vector2f(16+200, 16);
            window.Draw(s3);
            var s4 = Sprites.MagicMissle_Union(1.57 + 0.86);
            s4.Position = new Vector2f(16+300, 16);
            window.Draw(s4);
            var s5 = Sprites.MagicMissle_Union(3.14);
            s5.Position = new Vector2f(16+400, 16);
            window.Draw(s5);
            var s6 = Sprites.MagicMissle_Union(-3.14 + 0.86);
            s6.Position = new Vector2f(16+500, 16);
            window.Draw(s6);
            var s7 = Sprites.MagicMissle_Union(-3.14 + 1.57);
            s7.Position = new Vector2f(16+600, 16);
            window.Draw(s7);
            var s8 = Sprites.MagicMissle_Union(-1.57 + 0.86);
            s8.Position = new Vector2f(16+700, 16);
            window.Draw(s8);*/

            /*var s1 = Sprites.OrcWoodcutter_Enemy(0, UnitState.Die1);
            s1.Position = new Vector2f(16, 16);
            window.Draw(s1);
            var s2 = Sprites.OrcWoodcutter_Enemy(0, UnitState.Die2);
            s2.Position = new Vector2f(16 + 100, 16);
            window.Draw(s2);
            var s3 = Sprites.OrcWoodcutter_Enemy(0, UnitState.Die3);
            s3.Position = new Vector2f(16 + 200, 16);
            window.Draw(s3);*/
        }
    }
}