using System;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using visualization.sfml;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public partial class SfmlVisualizer
    {
        public void PaintWayPoints(Vector2f[] wayPoints)
        {
            if (!showWayPoints) return;
            // LineShape - http://en.sfml-dev.org/forums/index.php?topic=12825.0
            // https://github.com/SFML/SFML/wiki/Source:-line-with-thickness
            VertexArray array = GenerateTrianglesStrip(wayPoints.Select(x => new Vector2f(x.X, x.Y)).ToList(), new Color(0xffffff11), 4, true);
            //for (int i = 0; i < wayPoints.Length - 1; i++)
            //{
            //    var vertex = new Vertex(new Vector2f(wayPoints[i].X, wayPoints[i].Y), new Color(0xffffff11));
            //    array.Append(vertex);
            //    array.Append(new Vertex(new Vector2f(wayPoints[i + 1].X, wayPoints[i + 1].Y), new Color(0xffffff11)));
            //}
            worldTexture.Draw(array);
            for (int i = 0; i < wayPoints.Length; i++)
            {
                var sprite = Sprites.Waypoint_Union();
                DrawGameUnitSprite(sprite, wayPoints[i].X, wayPoints[i].Y, 102);
            }
        }

        public void BuildingShoot(double x, double y, BuildingShoortSprite number)
        {
            var sprite = Sprites.BuildingShoot(number);
            DrawGameUnitSprite(sprite, x, y);
        }

        public void DrawBonus(Vector2f[] points, Vector2f[] visibleBonuses)
        {
            foreach (var point in points)
            {
                DrawGameUnitSprite(Sprites.BonusPoint(), point.X, point.Y);
            }
            foreach (var visibleBonus in visibleBonuses)
            {
                DrawGameUnitSprite(Sprites.Bonus(GetNumberDependingOnIndex<BonusSprite>(4)), visibleBonus.X, visibleBonus.Y);
            }
        }

        public void DrawMinionRespawn(Vector2f[] points, bool isUnion)
        {
            WayPointInfo[][] respawnWayPoints;
            if (isUnion)
            {
                if (respawnUnionPoints == null)
                {
                    respawnUnionPoints = points;
                    respawnWayPoints = respawnUnionWayPoints = GenerateWayPoints(points, 100);
                } else respawnWayPoints = respawnUnionWayPoints;
            }
            else if (respawnEnemyPoints == null)
            {
                respawnEnemyPoints = points;
                respawnWayPoints = respawnEnemyWayPoints = GenerateWayPoints(points, 100);
            } else respawnWayPoints = respawnEnemyWayPoints;

            for (int i = 0; i < respawnWayPoints.Length; i++)
            {
                var point = points[i];
                var isVisiblePoint = visibleArea[(int) (point.X/visibleArea.Length)][(int) (point.Y/visibleArea.Length)] != 0;
                if (!isVisiblePoint) continue;
                int n = (index/1)%respawnWayPoints[i].Length;
                var sprite = isUnion
                    ? Sprites.Critter_Union(respawnWayPoints[i][n].Angle)
                    : Sprites.Critter_Enemy(respawnWayPoints[i][n].Angle);
                DrawGameUnitSprite(sprite, respawnWayPoints[i][n].Point.X, respawnWayPoints[i][n].Point.Y);
            }
        }

        [Obsolete]
        public void Tree(double x, double y)
        {
            TreeHead(x, y - 32);
            TreeFooter(x, y + 32);
        }

        public void TreeHead(double x, double y)
        {
            DrawGameUnitSprite(Sprites.Tree_Union(9, 7), x, y);
        }

        public void TreeBody(double x, double y)
        {
            DrawGameUnitSprite(Sprites.Tree_Union(10, 7), x, y);
        }

        public void TreeFooter(double x, double y)
        {
            DrawGameUnitSprite(Sprites.Tree_Union(11, 7), x, y);
        }

        public void DrawLight(RenderTexture renderTexture, double x, double y, double radius)
        {
            Sprite sprite;
            if (radius < 401)
            {
                sprite = Sprites.Light(LightSprite.Light200x200);
                sprite.Scale = new Vector2f((float) (radius / 200f), (float) (radius / 200f));
            } else if (radius < 601)
            {
                sprite = Sprites.Light(LightSprite.Light300x300);
                sprite.Scale = new Vector2f((float) (radius / 300f), (float) (radius / 300f));
            }
            else
            {
                sprite = Sprites.Light(LightSprite.Light400x400);
                sprite.Scale = new Vector2f((float) (radius / 400f), (float) (radius / 400f));
            }
            DrawGameUnitSprite(sprite, x, y, null, renderTexture, sprite.Scale, new RenderStates(BlendMode.Add));
        }

        public void DrawBaseUnion(double x, double y, double radius, double life, double maxLife, int cooldownTicks, int remainingActionCooldownTicks)
        {
            var sprite = Sprites.Base_Union();
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, true);
            DrawBuildingFlame(x, y, life, maxLife, sprite, 200);
            DrawCooldownTicks(x, y, cooldownTicks, remainingActionCooldownTicks, sprite);
        }

        public void DrawBaseEnemy(double x, double y, double radius, double life, double maxLife, int cooldownTicks, int remainingActionCooldownTicks)
        {
            var sprite = Sprites.Base_Enemy();
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, true);
            DrawBuildingFlame(x, y, life, maxLife, sprite, 200);
            DrawCooldownTicks(x, y, cooldownTicks, remainingActionCooldownTicks, sprite);
        }

        public void DrawTowerUnion(double x, double y, double radius, double life, double maxLife, int cooldownTicks, int remainingActionCooldownTicks)
        {
            var sprite = Sprites.Tower_Union();
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, true);
            DrawBuildingFlame(x, y, life, maxLife, sprite, 200);
            DrawCooldownTicks(x, y, cooldownTicks, remainingActionCooldownTicks, sprite);
        }

        public void DrawTowerEnemy(double x, double y, double radius, double life, double maxLife, int cooldownTicks, int remainingActionCooldownTicks)
        {
            var sprite = Sprites.Tower_Enemy();
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, true);
            DrawBuildingFlame(x, y, life, maxLife, sprite, 200);
            DrawCooldownTicks(x, y, cooldownTicks, remainingActionCooldownTicks, sprite);
        }

        public void WizardUnion(double x, double y, double angle, double radius, double life, double maxLife, int xp, int level, UnitStateSprite stateSprite, bool isBurning, bool isFrozen, bool isMe)
        {
            Sprite sprite;
            if (isMe)
            {
                sprite = Sprites.Wizard_Union_Me(angle, stateSprite);
                if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
                DrawGameUnitSprite(sprite, x, y);
            }
            else
            {
                sprite = Sprites.Wizard_Union(angle, stateSprite);
                if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
                DrawGameUnitSprite(sprite, x, y);
            }
            DrawHealth(sprite, x, y, life, maxLife, true);
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
            DrawXp(sprite, x, y, xp, level);
        }

        public void WizardEnemy(double x, double y, double angle, double radius, double life, double maxLife, int xp, int level, UnitStateSprite stateSprite, bool isBurning, bool isFrozen)
        {
            var sprite = Sprites.Wizard_Enemy(angle, stateSprite);
            if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, true);
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
            DrawXp(sprite, x, y, xp, level);
        }

        public void DrawFetishBlowdartUnion(double x, double y, double angle, double radius, double life, double maxLife, UnitStateSprite stateSprite, bool isBurning, bool isFrozen)
        {
            var sprite = Sprites.FetishBlowdart_Union(angle, stateSprite);
            if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, false);
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
        }

        public void DrawFetishBlowdartNeutral(double x, double y, double angle, double radius, double life, double maxLife, UnitStateSprite stateSprite, bool isBurning, bool isFrozen, bool isNeutralAttacking)
        {
            var sprite = Sprites.FetishBlowdart_Neutral(angle, stateSprite);
            if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
            DrawGameUnitSprite(sprite, x, y);
            if (isNeutralAttacking)
            {
                DrawHealth(sprite, x, y, life, maxLife, false);
            }
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
        }

        public void DrawFetishBlowdartEnemy(double x, double y, double angle, double radius, double life, double maxLife, UnitStateSprite stateSprite, bool isBurning, bool isFrozen)
        {
            var sprite = Sprites.FetishBlowdart_Enemy(angle, stateSprite);
            if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, false);
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
        }

        public void DrawOrcWoodcutterUnion(double x, double y, double angle, double radius, double life, double maxLife, UnitStateSprite stateSprite, bool isBurning, bool isFrozen)
        {
            var sprite = Sprites.OrcWoodcutter_Union(angle, stateSprite);
            if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, false);
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
        }

        public void DrawOrcWoodcutterNeutral(double x, double y, double angle, double radius, double life, double maxLife, UnitStateSprite stateSprite, bool isBurning, bool isFrozen, bool isNeutralAttacking)
        {
            var sprite = Sprites.OrcWoodcutter_Neutral(angle, stateSprite);
            if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
            DrawGameUnitSprite(sprite, x, y);
            if (isNeutralAttacking)
            {
                DrawHealth(sprite, x, y, life, maxLife, false);
            }
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
        }

        public void DrawOrcWoodcutterEnemy(double x, double y, double angle, double radius, double life, double maxLife, UnitStateSprite stateSprite, bool isBurning, bool isFrozen)
        {
            var sprite = Sprites.OrcWoodcutter_Enemy(angle, stateSprite);
            if (isFrozen) sprite.Color = new Color(0, 255, 255, 200);
            DrawGameUnitSprite(sprite, x, y);
            DrawHealth(sprite, x, y, life, maxLife, false);
            if (isBurning) DrawSmallFlame(x, y, isFrozen ? (byte)160 : (byte)200);
        }

        public void DrawMagicMissle_Union(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.MagicMissle_Union(angle), x, y, 200);
        }

        public void DrawMagicMissleAtTarget_Union(double x, double y, MagicMissleSprite number)
        {
            DrawGameUnitSprite(Sprites.MagicMissleTargeted_Union(number), x, y, 200);
        }

        public void DrawDart_Union(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.Axe_Union(angle), x, y);
        }

        public void DrawFireball_Union(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.Fireball(angle), x, y, 200);
        }

        public void DrawFrostBolt_Union(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.Frost(angle), x, y, 200);
        }

        public void DrawMagicMissle_Enemy(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.MagicMissle_Enemy(angle), x, y, 200);
        }

        public void DrawMagicMissleAtTarget_Enemy(double x, double y, MagicMissleSprite number)
        {
            DrawGameUnitSprite(Sprites.MagicMissleTargeted_Enemy(number), x, y, 200);
        }

        public void DrawDart_Enemy(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.Axe_Enemy(angle), x, y);
        }

        public void DrawFireball_Enemy(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.Fireball(angle), x, y, 200);
        }

        public void DrawFrostBolt_Enemy(double x, double y, double angle)
        {
            DrawGameUnitSprite(Sprites.Frost(angle), x, y, 200);
        }

        public void SetStaticData(int maxLevelXp, int maxLevel)
        {
            this.maxLevelXp = maxLevelXp;
            this.maxLevel = maxLevel;
        }

        public void DrawDeadWizardUnion(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            } else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.Wizard_Union(angle, state1.Value), x, y);
            }
        }

        public void DrawDeadWizardEnemy(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            }
            else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.Wizard_Enemy(angle, state1.Value), x, y);
            }
        }

        public void DrawDeadFetishBlowdartUnion(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            }
            else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.FetishBlowdart_Union(angle, state1.Value), x, y);
            }
        }

        public void DrawDeadFetishBlowdartNeutral(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            }
            else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.FetishBlowdart_Neutral(angle, state1.Value), x, y);
            }
        }

        public void DrawDeadFetishBlowdartEnemy(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            }
            else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.FetishBlowdart_Enemy(angle, state1.Value), x, y);
            }
        }

        public void DrawDeadOrcWoodcutterUnion(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            }
            else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.OrcWoodcutter_Union(angle, state1.Value), x, y);
            }
        }

        public void DrawDeadOrcWoodcutterNeutral(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            }
            else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.OrcWoodcutter_Neutral(angle, state1.Value), x, y);
            }
        }

        public void DrawDeadOrcWoodcutterEnemy(double x, double y, double angle, UnitStateSprite? state1 = null, CorpseSprite? state2 = null)
        {
            if (state2.HasValue)
            {
                DrawGameUnitSprite(Sprites.Corpses_Neutral(angle, state2.Value), x, y);
            }
            else if (state1.HasValue)
            {
                DrawGameUnitSprite(Sprites.OrcWoodcutter_Enemy(angle, state1.Value), x, y);
            }
        }

        public void DrawDestroyedBaseUnion(double x, double y, int? state = null)
        {
            DrawGameUnitSprite(Sprites.BaseDestroyed_Union(), x, y);
            if (!state.HasValue)
            {
            }
            else
            {
                DrawGameUnitSprite(Sprites.Explosion(state.Value), x, y);
            }
        }

        public void DrawDestroyedBaseEnemy(double x, double y, int? state = null)
        {
            DrawGameUnitSprite(Sprites.BaseDestroyed_Enemy(), x, y);
            if (!state.HasValue)
            {
            }
            else
            {
                DrawGameUnitSprite(Sprites.Explosion(state.Value), x, y);
            }
        }

        public void DrawDestroyedTowerUnion(double x, double y, int? state = null)
        {
            DrawGameUnitSprite(Sprites.TowerDestroyed_Union(), x, y);
            if (!state.HasValue)
            {
            }
            else
            {
                DrawGameUnitSprite(Sprites.Explosion(state.Value), x, y);
            }
        }

        public void DrawDestroyedTowerEnemy(double x, double y, int? state = null)
        {
            DrawGameUnitSprite(Sprites.TowerDestroyed_Enemy(), x, y);
            if (!state.HasValue)
            {
            }
            else
            {
                DrawGameUnitSprite(Sprites.Explosion(state.Value), x, y);
            }
        }

        public void Test()
        {
            DrawGameUnitSprite(Sprites.Light(LightSprite.Light200x200), 400, 3600, null, visibleAreaTexture, null, new RenderStates(BlendMode.Add));
            DrawGameUnitSprite(Sprites.Light(LightSprite.Light200x200), 600, 3600, null, visibleAreaTexture, null, new RenderStates(BlendMode.Add));
        }
    }
}