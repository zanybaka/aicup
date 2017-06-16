using System;
using System.Collections.Generic;
using System.IO;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Properties;
using SFML.Graphics;
using SFML.System;

namespace visualization.sfml
{
    public static class Sprites
    {
        // ----- Misc -----

        public static Sprite Light(LightSprite size)
        {
            if (size == LightSprite.Light200x200)
            {
                return GetSprite(@"Light200x200x2.png");
            }
            if (size == LightSprite.Light300x300)
            {
                return GetSprite(@"Light300x300x2.png");
            }
            if (size == LightSprite.Light400x400)
            {
                return GetSprite(@"Light400x400x2.png");
            }
            throw new NotSupportedException();
        }

        public static Sprite Black()
        {
            return GetSprite(@"tilesets\swamp\terrain\swamp.png", new IntRect(0 * 32, 0 * 32, 32, 32));
        }

        public static Sprite Minimap()
        {
            return GetSprite(@"minimap130x130.png", new IntRect(0, 0, 130, 130));
        }

        public static Sprite FogTopLeft()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(1 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogTop()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(2 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogTopRight()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(3 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogLeft()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(4 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogMidVertical()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(5 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogRight()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(6 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogBottomLeft()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(7 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogBottom()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(8 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogBottomRight()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(9 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogTopLeftSingle()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(10 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogTopRightSingle()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(11 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogBottomLeftSingle()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(12 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogBottomRightSingle()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(13 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogTopLeftAndBottomRightSingle()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(14 * 32, 0 * 32, 32, 32));
        }

        public static Sprite FogBottomLeftAndTopRightSingle()
        {
            return GetSprite(@"swamp_no_transparency.png", new IntRect(15 * 32, 0 * 32, 32, 32));
        }

        public static Sprite White()
        {
            return GetSprite(@"white.png", new IntRect(0, 0, 32, 32));
        }

        public static Sprite BuildingShoot(BuildingShoortSprite number)
        {
            return GetSprite(@"missiles\cannon-tower_explosion.png", new IntRect(0, 32 * (int) number, 32, 32));
        }

        public static Sprite BonusPoint()
        {
            return GetSprite(@"neutral\buildings\circle_of_power.png");
        }

        public static Sprite Bonus(BonusSprite number)
        {
            // 2 rows
            // 5 columns
            // 1 tile in the second row
            int x = (int)number % 5;
            int y = (int)number / 5;
            var sprite = GetSprite(@"missiles\normal_spell2.png", new IntRect(x * 32, y * 32, 32, 32));
            return sprite;
        }

        public static Sprite SmallFlame(SmallFlameSprite number)
        {
            // 2 rows
            // 5 columns
            // 1 tile in the second row
            int x = (int)number % 5;
            int y = (int)number / 5;
            return GetSprite(@"missiles\flame_shield.png", new IntRect(x * 32, y * 48, 32, 48));
        }

        public static Sprite BigFlame(BigFlameSprite number)
        {
            // 2 rows
            // 5 columns
            int x = (int)number % 5;
            int y = (int)number / 5;
            return GetSprite(@"missiles\big_fire.png", new IntRect(x * 48, y * 48, 48, 48));
        }

        public static Sprite Explosion(int number)
        {
            // 64, 5. 5. 5. 1
            int x = number%5;
            int y = number/5;
            return GetSprite(@"missiles\explosion.png", new IntRect(64 * x, 64 * y, 64, 64));
        }

        public static Sprite Fireball(double angle)
        {
            return GetRotatedW2Sprite(angle, @"missiles\fireball.png", 32);
        }

        public static Sprite Frost(double angle)
        {
            string file = "frost.png";
            int size = 32;
            Sprite sprite;
            if (angle >= -Math.PI * 1 / 8 && angle < Math.PI * 1 / 8)
            {
                sprite = GetSprite(file, new IntRect(3 * size, 0, size, size));
            }
            else if (angle >= Math.PI * 1 / 8 && angle < Math.PI * 3 / 8)
            {
                sprite = GetSprite(file, new IntRect(4 * size, 0, size, size));
            }
            else if (angle >= Math.PI * 3 / 8 && angle < Math.PI * 5 / 8)
            {
                sprite = GetSprite(file, new IntRect(5 * size, 0, size, size));
            }
            else if (angle >= Math.PI * 5 / 8 && angle < Math.PI * 7 / 8)
            {
                sprite = GetSprite(file, new IntRect(6 * size, 0, size, size));
            }
            else if (angle >= Math.PI * 7 / 8 || angle < -Math.PI * 7 / 8)
            {
                sprite = GetSprite(file, new IntRect(7 * size, 0, size, size));
            }
            else if (angle >= -Math.PI * 7 / 8 && angle < -Math.PI * 5 / 8)
            {
                sprite = GetSprite(file, new IntRect(0 * size, 0, size, size));
            }
            else if (angle >= -Math.PI * 5 / 8 && angle < -Math.PI * 3 / 8)
            {
                sprite = GetSprite(file, new IntRect(1 * size, 0, size, size));
            }
            else// if (angle >= -Math.PI * 3 / 8 && angle < -Math.PI * 1 / 8)
            {
                sprite = GetSprite(file, new IntRect(2 * size, 0, size, size));
            }
            return sprite;
        }

        // ----- Neutral -----

        public static Sprite FetishBlowdart_Neutral(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"human\units\peasant.png", 72, (int)stateSprite);
        }

        public static Sprite OrcWoodcutter_Neutral(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"orc\units\peon.png", 72, (int)stateSprite);
        }

        public static Sprite Corpses_Neutral(double angle, CorpseSprite sprite)
        {
            return GetRotatedW2Sprite(angle, @"neutral\units\corpses.png", 72, (int)sprite);
        }

        // ----- Union -----

        public static Sprite Axe_Union(double angle)
        {
            return GetSprite(@"missiles\axe.png", new IntRect(0, 0, 32, 32));
        }

        public static Sprite MagicMissle_Union(double angle)
        {
            // var sprite = GetSprite(@"missiles\touch_of_death.png", new IntRect(0, 0, 32, 32));
            // sprite.Rotation = (float) (angle/Math.PI*180 + 90);
            return GetRotatedW2Sprite(angle, @"missiles\touch_of_death.png", 32);
        }

        public static Sprite MagicMissleTargeted_Union(MagicMissleSprite number)
        {
            return GetSprite(@"missiles\touch_of_death.png", new IntRect(0, 32 + 32 * (int)number, 32, 32));
        }

        public static Sprite Ground_Union(GroundSprite number)
        {
            // 13x21
            int x = (int)number % 16;
            int y = (int)number / 16;
            return GetSprite(@"tilesets\swamp\terrain\swamp.png", new IntRect(32 * x, 32 * y, 32, 32));
        }

        public static Sprite Background_Union()
        {
            return GetSprite(@"tilesets\swamp\terrain\swamp4096x4096.png");
            // return GetSprite(@"tilesets\swamp\terrain\swamp2048x2048.png");
        }

        public static Sprite Tree_Union(int i, int j)
        {
            return GetSprite(@"tilesets\swamp\terrain\swamp.png", new IntRect(32 * i, 32 * j, 32, 32));
        }

        public static Sprite Critter_Union(double angle)
        {
            return GetRotatedW2Sprite(angle, @"tilesets\swamp\neutral\units\critter.png", 32);
        }

        public static Sprite Base_Union()
        {
            return GetSprite(@"tilesets\summer\orc\buildings\barracks.png", new IntRect(0, 0, 96, 96));
        }

        public static Sprite BaseUnderAttack_Union()
        {
            return GetSprite(@"tilesets\summer\orc\buildings\barracks.png", new IntRect(0, 96, 96, 96));
        }

        public static Sprite BaseDestroyed_Union()
        {
            return GetSprite(@"tilesets\swamp\neutral\buildings\destroyed_site.png", new IntRect(0, 0, 64, 64));
        }

        public static Sprite Tower_Union()
        {
            return GetSprite(@"tilesets\summer\orc\buildings\cannon_tower.png", new IntRect(0, 0, 64, 64));
        }

        public static Sprite TowerUnderAttack_Union()
        {
            return GetSprite(@"tilesets\summer\orc\buildings\cannon_tower.png", new IntRect(0, 64, 64, 64));
        }

        public static Sprite TowerDestroyed_Union()
        {
            return GetSprite(@"tilesets\swamp\neutral\buildings\destroyed_site.png", new IntRect(0, 64, 64, 64));
        }

        public static Sprite Wizard_Union(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"orc\units\death_knight.png", 72, (int)stateSprite);
        }

        public static Sprite Wizard_Union_Me(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"orc\units\death_knight_me.png", 72, (int)stateSprite);
        }

        public static Sprite OrcWoodcutter_Union(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"orc\units\grunt.png", 72, (int)stateSprite);
        }

        public static Sprite FetishBlowdart_Union(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"orc\units\troll_axethrower.png", 72, (int)stateSprite);
        }

        public static Sprite Waypoint_Union()
        {
            return GetSprite(@"orc\o_startpoint.png");
        }

        // ----- Enemy -----

        public static Sprite MagicMissle_Enemy(double angle)
        {
            return GetRotatedW2Sprite(angle, @"missiles\lightning.png", 32);
        }

        public static Sprite MagicMissleTargeted_Enemy(MagicMissleSprite number)
        {
            return GetSprite(@"missiles\lightning.png", new IntRect(0, 32 + 32 * (int)number, 32, 32));
        }

        public static Sprite Axe_Enemy(double angle)
        {
            return GetRotatedW2Sprite(angle, @"missiles\arrow.png", 40);
        }

        public static Sprite Ground_Enemy()
        {
            return GetSprite(@"tilesets\summer\terrain\summer.png", new IntRect(32 * 4, 23 * 32, 32, 32));
        }

        public static Sprite Tree_Enemy()
        {
            return GetSprite(@"tilesets\summer\terrain\summer.png", new IntRect(32 * 0, 6 * 32, 32, 32));
        }

        public static Sprite Critter_Enemy(double angle)
        {
            return GetRotatedW2Sprite(angle, @"tilesets\swamp\neutral\units\critter.png", 32);
        }

        public static Sprite Base_Enemy()
        {
            return GetSprite(@"tilesets\summer\human\buildings\barracks.png", new IntRect(0, 0, 96, 96));
        }

        public static Sprite BaseUnderAttack_Enemy()
        {
            return GetSprite(@"tilesets\summer\human\buildings\barracks.png", new IntRect(0, 96, 96, 96));
        }

        public static Sprite BaseDestroyed_Enemy()
        {
            return GetSprite(@"tilesets\swamp\neutral\buildings\destroyed_site.png", new IntRect(0, 0, 64, 64));
        }

        public static Sprite Tower_Enemy()
        {
            return GetSprite(@"tilesets\summer\human\buildings\cannon_tower.png", new IntRect(0, 0, 64, 64));
        }

        public static Sprite TowerUnderAttack_Enemy()
        {
            return GetSprite(@"tilesets\summer\human\buildings\cannon_tower.png", new IntRect(0, 64, 64, 64));
        }

        public static Sprite TowerDestroyed_Enemy()
        {
            return GetSprite(@"tilesets\swamp\neutral\buildings\destroyed_site.png", new IntRect(0, 64, 64, 64));
        }

        public static Sprite Wizard_Enemy(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"human\units\mage.png", 72, (int)stateSprite);
        }

        public static Sprite OrcWoodcutter_Enemy(double angle, UnitStateSprite stateSprite)
        {
            return GetRotatedW2Sprite(angle, @"human\units\footman.png", 72, (int)stateSprite);
        }

        public static Sprite FetishBlowdart_Enemy(double angle, UnitStateSprite stateSprite)
        {
            if (stateSprite == UnitStateSprite.Attack2) stateSprite = UnitStateSprite.Attack1; //archer has only two sprites for attack while other units has four sprites
            else if (stateSprite == UnitStateSprite.Attack3 || stateSprite == UnitStateSprite.Attack4) stateSprite = UnitStateSprite.Attack2; //archer has only two sprites for attack while other units has four sprites
            else if (stateSprite >= UnitStateSprite.Die1) stateSprite -= 2; //archer has only two sprites for attack while other units has four sprites
            return GetRotatedW2Sprite(angle, @"human\units\elven_archer.png", 72, (int)stateSprite);
        }

        public static Sprite Waypoint_Enemy()
        {
            return GetSprite(@"human\x_startpoint.png");
        }

        private static Sprite GetSprite(string file, IntRect? rect = null, bool smooth = false)
        {
            Texture texture;
            if (!Cache.TryGetValue(file, out texture))
            {
                texture = new Texture(Path.Combine(Settings.Default.ResourcesDir, "WarCraft 2 Sprites", file));
                Cache.Add(file, texture);
            }
            var sprite = rect.HasValue ? new Sprite(texture, rect.Value) : new Sprite(texture);
            sprite.Texture.Smooth =  SfmlVisualizer.ForceSmooth || smooth;
            return sprite;
        }

        private static Sprite GetRotatedW2Sprite(double angle, string file, int size, int number = 0)
        {
            Sprite sprite;
            if (angle >= -Math.PI * 1 / 8 && angle < Math.PI * 1 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 2, number * size, size, size));
            }
            else if (angle >= Math.PI * 1 / 8 && angle < Math.PI * 3 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 3, number * size, size, size));
            }
            else if (angle >= Math.PI * 3 / 8 && angle < Math.PI * 5 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 4, number * size, size, size));
            }
            else if (angle >= Math.PI * 5 / 8 && angle < Math.PI * 7 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 3, number * size, size, size));
                sprite.Scale = new Vector2f(-1f, 1);
                sprite.Origin = new Vector2f(size, 0);
            }
            else if (angle >= Math.PI * 7 / 8 || angle < -Math.PI * 7 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 2, number * size, size, size));
                sprite.Scale = new Vector2f(-1f, 1);
                sprite.Origin = new Vector2f(size, 0);
            }
            else if (angle >= -Math.PI * 7 / 8 && angle < -Math.PI * 5 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 1, number * size, size, size));
                sprite.Scale = new Vector2f(-1f, 1);
                sprite.Origin = new Vector2f(size, 0);
            }
            else if (angle >= -Math.PI * 5 / 8 && angle < -Math.PI * 3 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 0, number * size, size, size));
            }
            else// if (angle >= -Math.PI * 3 / 8 && angle < -Math.PI * 1 / 8)
            {
                sprite = GetSprite(file, new IntRect(size * 1, number * size, size, size));
            }
            return sprite;
        }

        private static readonly Dictionary<string, Texture> Cache = new Dictionary<string, Texture>();
    }
}
