using System;
using System.Collections.Generic;
using System.Linq;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.RunnerExtension;
using NAudio.Wave;
using SFML.System;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public class VisualizerHost
    {
        private const int TargetSelectionRadiusK = 2;
        private const int NumberOfTicksForSpriteAnimation = 5;

        private readonly ExtendedRunnerModel extendedRunnerModel;
        private bool playedBaseAttack1;
        private bool playedBaseAttack2;
        private List<AnimatedUnitInfo> buildingShoots = new List<AnimatedUnitInfo>();
        private long? targetUnitId;
        private LivingUnit targetUnit;

        public SfmlVisualizer Visualizer { get; private set; }
        public IWavePlayer BackgroundMusic { get; private set; }

        public VisualizerHost(ExtendedRunnerModel extendedRunnerModel)
        {
            this.extendedRunnerModel = extendedRunnerModel;
            this.extendedRunnerModel.WorldUpdated += OnWorldUpdated;
            this.extendedRunnerModel.BeforeMove += BeforeMove;
            this.extendedRunnerModel.AfterMove += AfterMove;
        }

        public void Start()
        {
            if (Visualizer != null) throw new InvalidOperationException("Visualizer is already initialized.");

            Visualizer = new SfmlVisualizer();
            Visualizer.SetStaticData(this.extendedRunnerModel.Game.LevelUpXpValues.Sum(), this.extendedRunnerModel.Game.LevelUpXpValues.Length);

            Visualizer.ToggleAudio += (sender, enableAudio) =>
            {
                if (enableAudio && BackgroundMusic == null)
                {
                    BackgroundMusic = Audios.PlayBackgroundMusic();
                }
                else if (!enableAudio && BackgroundMusic != null)
                {
                    BackgroundMusic.Stop();
                    BackgroundMusic.Dispose();
                    BackgroundMusic = null;
                }
            };

            Visualizer.ToggleTargetObserving += (sender, observe) =>
            {
                targetUnitId = null;
                targetUnit = null;
            };

            string debugText = "";
            Visualizer.LeftMousePressed += (sender, position) =>
            {
                debugText = string.Format("{0};{1}", position.X, position.Y);
                Unit nearestUnit = extendedRunnerModel.AllWizards.Values
                    .Cast<LivingUnit>()
                    .Union(extendedRunnerModel.AllBuildings.Values)
                    .Union(extendedRunnerModel.AllMinions.Values)
                    .Select(
                        x =>
                        {
                            var distanceTo = x.GetDistanceTo(position.X, position.Y);
                            return new
                            {
                                Distance = distanceTo > x.Radius * TargetSelectionRadiusK ? (double?)null : distanceTo,
                                Unit = x
                            };
                        })
                    .Where(x => x.Distance.HasValue)
                    .OrderBy(x => x.Distance.Value)
                    .Select(x => x.Unit)
                    .FirstOrDefault();
                if (nearestUnit != null)
                {
                    bool canObserve = false;
                    if (nearestUnit is Building)
                    {
                        if (nearestUnit.Faction == Faction.Academy)
                        {
                            if (((Building)nearestUnit).Type == BuildingType.FactionBase)
                            {
                                Audios.Union_Base_Selected();
                            }
                            else
                            {
                                Audios.Union_Tower_Selected();
                            }
                        }
                        else if (nearestUnit.Faction == Faction.Renegades)
                        {
                            if (((Building)nearestUnit).Type == BuildingType.FactionBase)
                            {
                                Audios.Enemy_Base_Selected();
                            }
                            else
                            {
                                Audios.Enemy_Tower_Selected();
                            }
                        }
                    }
                    else if (nearestUnit is Wizard)
                    {
                        if (nearestUnit.Faction == Faction.Academy)
                        {
                            Audios.Wizard_Selected_Union();
                            canObserve = true;
                        }
                        else if (nearestUnit.Faction == Faction.Renegades)
                        {
                            Audios.Wizard_Selected_Enemy();
                            canObserve = true;
                        }
                    }
                    else if (nearestUnit is Minion)
                    {
                        if (nearestUnit.Faction == Faction.Academy)
                        {
                            Audios.Minion_Selected_Union(((Minion)nearestUnit).Type == MinionType.FetishBlowdart);
                            canObserve = true;
                        }
                        else if (nearestUnit.Faction == Faction.Renegades)
                        {
                            Audios.Minion_Selected_Enemy(((Minion)nearestUnit).Type == MinionType.FetishBlowdart);
                            canObserve = true;
                        }
                    }
                    if (canObserve)
                    {
                        targetUnitId = nearestUnit.Id;
                        targetUnit = null;
                        Visualizer.EnableTargetObservation();
                    }
                }
            };
        }

        private void OnWorldUpdated(object sender, EventArgs e)
        {
            Visualizer.Init(extendedRunnerModel.World.TickIndex);

            // prepare visual part before drawing
            foreach (var unit in extendedRunnerModel.CurrentBuildings)
            {
                if (unit.Value.IsUnion(extendedRunnerModel.Me))
                {
                    Visualizer.AddVisibleArea(unit.Value.X, unit.Value.Y, unit.Value.VisionRange);
                }
            }
            foreach (var unit in extendedRunnerModel.CurrentWizards)
            {
                if (targetUnitId == unit.Value.Id)
                {
                    targetUnit = unit.Value;
                }
                if (unit.Value.IsUnion(extendedRunnerModel.Me))
                {
                    Visualizer.AddVisibleArea(unit.Value.X, unit.Value.Y, unit.Value.VisionRange);
                }
            }
            foreach (var unit in extendedRunnerModel.CurrentMinions)
            {
                if (targetUnitId == unit.Value.Id)
                {
                    targetUnit = unit.Value;
                }
                if (unit.Value.IsUnion(extendedRunnerModel.Me))
                {
                    Visualizer.AddVisibleArea(unit.Value.X, unit.Value.Y, unit.Value.VisionRange);
                }
            }

            foreach (var unit in extendedRunnerModel.World.Trees)
            {
                Visualizer.AddTree(unit.X, unit.Y, unit.Radius);
            }
        }

        private void BeforeMove(object sender, EventArgs e)
        {
            if (extendedRunnerModel.World.TickIndex == 35)
            {
                BackgroundMusic = Audios.PlayBackgroundMusic();
                Audios.Wizard_Union_Start();
            }

            if (targetUnit != null)
            {
                Visualizer.BeginDrawing(targetUnit.X, targetUnit.Y);
            }
            else
            {
                Visualizer.BeginDrawing(extendedRunnerModel.Me.X, extendedRunnerModel.Me.Y);
            }

            Visualizer.DrawBonus(new[] { new Vector2f(1200, 1200), new Vector2f(2800, 2800) }, extendedRunnerModel.World.Bonuses.Select(x => new Vector2f((float)x.X, (float)x.Y)).ToArray());
            Visualizer.DrawMinionRespawn(new[] { new Vector2f(1000, 3750), new Vector2f(850, 3150), new Vector2f(250, 3000) }, true);
            Visualizer.DrawMinionRespawn(new[] { new Vector2f(3000, 250), new Vector2f(3150, 850), new Vector2f(3750, 1000) }, false);

            foreach (var deadUnit in extendedRunnerModel.DeadWizards)
            {
                var unit = deadUnit.Value;
                if (unit.IsUnion)
                {
                    UnitStateSprite? state1 = GetKilledWizardUnionState(unit, extendedRunnerModel.World.TickIndex);
                    if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillWizard_Union();
                    // CorpseState? state2 = state1.HasValue ? null : GetCorpseState(unit, extendedRunnerModel.World.TickIndex);
                    // TODO remove from dictionary if state2 is null
                    Visualizer.DrawDeadWizardUnion(unit.X, unit.Y, unit.Angle, state1, null);
                }
                else
                {
                    UnitStateSprite? state1 = GetKilledWizardEnemyState(unit, extendedRunnerModel.World.TickIndex);
                    if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillWizard_Enemy();
                    // CorpseState? state2 = state1.HasValue ? null : GetCorpseState(unit, extendedRunnerModel.World.TickIndex);
                    // TODO remove from dictionary if state2 is null
                    Visualizer.DrawDeadWizardEnemy(unit.X, unit.Y, unit.Angle, state1, null);
                }
            }
            foreach (var deadUnit in extendedRunnerModel.DeadMinions)
            {
                var minion = deadUnit.Value;
                UnitStateSprite? state1 = GetKilledMinionState(minion, extendedRunnerModel.World.TickIndex);
                CorpseSprite? state2 = state1.HasValue ? null : GetCorpseState(minion, extendedRunnerModel.World.TickIndex);
                if (minion.MinionType == MinionType.FetishBlowdart)
                {
                    if (minion.IsUnion)
                    {
                        if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillMinion_Union();
                        Visualizer.DrawDeadFetishBlowdartUnion(minion.X, minion.Y, minion.Angle, state1, state2);
                    }
                    else if (minion.Faction == Faction.Neutral)
                    {
                        if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillMinion_Union();
                        Visualizer.DrawDeadFetishBlowdartNeutral(minion.X, minion.Y, minion.Angle, state1, state2);
                    }
                    else
                    {
                        if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillMinion_Enemy();
                        Visualizer.DrawDeadFetishBlowdartEnemy(minion.X, minion.Y, minion.Angle, state1, state2);
                    }
                }
                else
                {
                    if (minion.IsUnion)
                    {
                        if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillMinion_Union();
                        Visualizer.DrawDeadOrcWoodcutterUnion(minion.X, minion.Y, minion.Angle, state1, state2);
                    }
                    else if (minion.Faction == Faction.Neutral)
                    {
                        if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillMinion_Union();
                        Visualizer.DrawDeadOrcWoodcutterNeutral(minion.X, minion.Y, minion.Angle, state1, state2);
                    }
                    else
                    {
                        if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && extendedRunnerModel.World.TickIndex == deadUnit.Value.TickIndex) Audios.KillMinion_Enemy();
                        Visualizer.DrawDeadOrcWoodcutterEnemy(minion.X, minion.Y, minion.Angle, state1, state2);
                    }
                }
            }

            Visualizer.DrawTrees();

            foreach (var deadUnit in extendedRunnerModel.DeadBuildings)
            {
                var building = deadUnit.Value;
                int? state = GetDestroyedUnitState(building, extendedRunnerModel.World.TickIndex);
                if (Visualizer.IsInWindow(building.X, building.Y) && extendedRunnerModel.World.TickIndex == building.TickIndex) Audios.DestroyBuilding();

                if (building.BuildingType == BuildingType.FactionBase)
                {
                    if (building.IsUnion)
                    {
                        Visualizer.DrawDestroyedBaseUnion(building.X, building.Y, state);
                    }
                    else
                    {
                        Visualizer.DrawDestroyedBaseEnemy(building.X, building.Y, state);
                    }
                }
                else
                {
                    if (building.IsUnion)
                    {
                        Visualizer.DrawDestroyedTowerUnion(building.X, building.Y, state);
                    }
                    else
                    {
                        Visualizer.DrawDestroyedTowerEnemy(building.X, building.Y, state);
                    }
                }
            }
            foreach (var building in extendedRunnerModel.World.Buildings)
            {
                if (building.Type == BuildingType.FactionBase)
                {
                    if (Visualizer.IsInWindow(building.X, building.Y) && building.RemainingActionCooldownTicks >= extendedRunnerModel.Game.FactionBaseCooldownTicks - 1) Audios.BaseHit();
                    if (building.IsUnion(extendedRunnerModel.Me))
                    {
                        if (!playedBaseAttack1 && building.Life < building.MaxLife) { playedBaseAttack1 = true; Audios.BaseAttacked_Union(); }
                        Visualizer.DrawBaseUnion(building.X, building.Y, building.Radius, building.Life, building.MaxLife, building.CooldownTicks, building.RemainingActionCooldownTicks);
                    }
                    else
                    {
                        Visualizer.DrawBaseEnemy(building.X, building.Y, building.Radius, building.Life, building.MaxLife, building.CooldownTicks, building.RemainingActionCooldownTicks);
                    }
                }
                else
                {
                    if (Visualizer.IsInWindow(building.X, building.Y) && building.RemainingActionCooldownTicks >= extendedRunnerModel.Game.FactionBaseCooldownTicks - 1) Audios.TowerHit();
                    if (building.IsUnion(extendedRunnerModel.Me))
                    {
                        if (!playedBaseAttack2 && building.Life < building.MaxLife) { playedBaseAttack2 = true; Audios.TowerAttacked_Union(); }
                        Visualizer.DrawTowerUnion(building.X, building.Y, building.Radius, building.Life, building.MaxLife, building.CooldownTicks, building.RemainingActionCooldownTicks);
                    }
                    else
                    {
                        Visualizer.DrawTowerEnemy(building.X, building.Y, building.Radius, building.Life, building.MaxLife, building.CooldownTicks, building.RemainingActionCooldownTicks);
                    }
                }
            }
            foreach (var wizard in extendedRunnerModel.World.Wizards)
            {
                UnitStateSprite stateSprite = GetUnitState(wizard, extendedRunnerModel.World.TickIndex, wizard.RemainingActionCooldownTicks);
                var isBurning = wizard.Statuses.Any(x => x.Type == StatusType.Burning);
                var isFrozen = wizard.Statuses.Any(x => x.Type == StatusType.Frozen);
                if (wizard.IsUnion(extendedRunnerModel.Me))
                {
                    Visualizer.WizardUnion(wizard.X, wizard.Y, wizard.Angle, wizard.Radius, wizard.Life, wizard.MaxLife, wizard.Xp, wizard.Level, stateSprite, isBurning, isFrozen, wizard.IsMe);
                }
                else
                {
                    Visualizer.WizardEnemy(wizard.X, wizard.Y, wizard.Angle, wizard.Radius, wizard.Life, wizard.MaxLife, wizard.Xp, wizard.Level, stateSprite, isBurning, isFrozen);
                }
                if (Visualizer.IsInWindow(wizard.X, wizard.Y))
                {
                    if (wizard.RemainingCooldownTicksByAction[(int)ActionType.Staff] >= extendedRunnerModel.Game.StaffCooldownTicks - 1) Audios.Tree();
                    if (wizard.RemainingCooldownTicksByAction[(int)ActionType.Fireball] >= extendedRunnerModel.Game.FireballCooldownTicks - 1) Audios.Fireball();
                    if (wizard.RemainingCooldownTicksByAction[(int)ActionType.FrostBolt] >= extendedRunnerModel.Game.FrostBoltCooldownTicks - 1) Audios.FrostBolt();
                    if (wizard.RemainingCooldownTicksByAction[(int)ActionType.Haste] >= extendedRunnerModel.Game.HasteCooldownTicks - 1) Audios.Haste();
                    if (wizard.RemainingCooldownTicksByAction[(int)ActionType.Shield] >= extendedRunnerModel.Game.ShieldCooldownTicks - 1) Audios.Shield();
                    if (wizard.RemainingCooldownTicksByAction[(int)ActionType.MagicMissile] >= extendedRunnerModel.Game.MagicMissileCooldownTicks - 1)
                    {
                        if (wizard.IsUnion(extendedRunnerModel.Me)) Audios.MagicMissile_Union(); else Audios.MagicMissile_Enemy();
                    }
                }
            }

            foreach (var minion in extendedRunnerModel.World.Minions)
            {
                var isBurning = minion.Statuses.Any(x => x.Type == StatusType.Burning);
                var isFrozen = minion.Statuses.Any(x => x.Type == StatusType.Frozen);
                UnitStateSprite stateSprite = GetUnitState(minion, extendedRunnerModel.World.TickIndex, minion.RemainingActionCooldownTicks);
                if (minion.Type == MinionType.FetishBlowdart)
                {
                    if (minion.IsUnion(extendedRunnerModel.Me))
                    {
                        if (Visualizer.IsInWindow(minion.X, minion.Y) && minion.RemainingActionCooldownTicks >= extendedRunnerModel.Game.FetishBlowdartActionCooldownTicks - 1) Audios.Axe();
                        Visualizer.DrawFetishBlowdartUnion(minion.X, minion.Y, minion.Angle, minion.Radius, minion.Life, minion.MaxLife, stateSprite, isBurning, isFrozen);
                    }
                    else if (minion.Faction == Faction.Neutral)
                    {
                        if (Visualizer.IsInWindow(minion.X, minion.Y) && minion.RemainingActionCooldownTicks >= extendedRunnerModel.Game.FetishBlowdartActionCooldownTicks - 1) Audios.Axe();
                        Visualizer.DrawFetishBlowdartNeutral(minion.X, minion.Y, minion.Angle, minion.Radius, minion.Life, minion.MaxLife, stateSprite, isBurning, isFrozen, IsNeutralAttacking(minion));
                    }
                    else
                    {
                        if (Visualizer.IsInWindow(minion.X, minion.Y) && minion.RemainingActionCooldownTicks >= extendedRunnerModel.Game.FetishBlowdartActionCooldownTicks - 1) Audios.Arrow();
                        Visualizer.DrawFetishBlowdartEnemy(minion.X, minion.Y, minion.Angle, minion.Radius, minion.Life, minion.MaxLife, stateSprite, isBurning, isFrozen);
                    }
                }
                else
                {
                    if (minion.IsUnion(extendedRunnerModel.Me))
                    {
                        if (Visualizer.IsInWindow(minion.X, minion.Y) && minion.RemainingActionCooldownTicks >= extendedRunnerModel.Game.OrcWoodcutterActionCooldownTicks - 1) Audios.OrcHit();
                        Visualizer.DrawOrcWoodcutterUnion(minion.X, minion.Y, minion.Angle, minion.Radius, minion.Life, minion.MaxLife, stateSprite, isBurning, isFrozen);
                    }
                    else if (minion.Faction == Faction.Neutral)
                    {
                        if (Visualizer.IsInWindow(minion.X, minion.Y) && minion.RemainingActionCooldownTicks >= extendedRunnerModel.Game.OrcWoodcutterActionCooldownTicks - 1) Audios.OrcHit();
                        Visualizer.DrawOrcWoodcutterNeutral(minion.X, minion.Y, minion.Angle, minion.Radius, minion.Life, minion.MaxLife, stateSprite, isBurning, isFrozen, IsNeutralAttacking(minion));
                    }
                    else
                    {
                        if (Visualizer.IsInWindow(minion.X, minion.Y) && minion.RemainingActionCooldownTicks >= extendedRunnerModel.Game.OrcWoodcutterActionCooldownTicks - 1) Audios.SwordHit();
                        Visualizer.DrawOrcWoodcutterEnemy(minion.X, minion.Y, minion.Angle, minion.Radius, minion.Life, minion.MaxLife, stateSprite, isBurning, isFrozen);
                    }
                }
            }
            foreach (var projectile in extendedRunnerModel.World.Projectiles)
            {
                if (projectile.IsUnion(extendedRunnerModel.Me))
                {
                    if (projectile.Type == ProjectileType.MagicMissile)
                    {
                        Visualizer.DrawMagicMissle_Union(projectile.X, projectile.Y, projectile.Angle);
                    }
                    else if (projectile.Type == ProjectileType.Dart)
                    {
                        Visualizer.DrawDart_Union(projectile.X, projectile.Y, projectile.Angle);
                    }
                    else if (projectile.Type == ProjectileType.Fireball)
                    {
                        Visualizer.DrawFireball_Union(projectile.X, projectile.Y, projectile.Angle);
                    }
                    else if (projectile.Type == ProjectileType.FrostBolt)
                    {
                        Visualizer.DrawFrostBolt_Union(projectile.X, projectile.Y, projectile.Angle);
                    }
                }
                else // TODO neutral
                {
                    if (projectile.Type == ProjectileType.MagicMissile)
                    {
                        Visualizer.DrawMagicMissle_Enemy(projectile.X, projectile.Y, projectile.Angle);
                    }
                    else if (projectile.Type == ProjectileType.Dart)
                    {
                        Visualizer.DrawDart_Enemy(projectile.X, projectile.Y, projectile.Angle);
                    }
                    else if (projectile.Type == ProjectileType.Fireball)
                    {
                        Visualizer.DrawFireball_Enemy(projectile.X, projectile.Y, projectile.Angle);
                    }
                    else if (projectile.Type == ProjectileType.FrostBolt)
                    {
                        Visualizer.DrawFrostBolt_Enemy(projectile.X, projectile.Y, projectile.Angle);
                    }
                }
            }

            var deadProjectilesInVisibleArea = extendedRunnerModel.DeadProjectiles.Where(x => Visualizer.IsInVisibleArea(x.Value.X, x.Value.Y)).ToDictionary(x => x.Key, x => x.Value);
            foreach (var deadUnit in deadProjectilesInVisibleArea)
            {
                var projectile = deadUnit.Value;
                if (projectile.ProjectileType == ProjectileType.MagicMissile)
                {
                    if (projectile.IsUnion)
                    {
                        // if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && deadUnit.Value.IsTargetTree && extendedRunnerModel.World.TickIndex <= deadUnit.Value.TickIndex) Audios.Tree();
                        Visualizer.DrawMagicMissleAtTarget_Union(deadUnit.Value.X, deadUnit.Value.Y, Visualizer.GetNumberDependingOnIndex<MagicMissleSprite>(4));
                    }
                    else
                    {
                        // if (Visualizer.IsInWindow(deadUnit.Value.X, deadUnit.Value.Y) && deadUnit.Value.IsTargetTree && extendedRunnerModel.World.TickIndex <= deadUnit.Value.TickIndex) Audios.Tree();
                        Visualizer.DrawMagicMissleAtTarget_Enemy(deadUnit.Value.X, deadUnit.Value.Y, Visualizer.GetNumberDependingOnIndex<MagicMissleSprite>(4));
                    }
                }
            }

            foreach (var pair in extendedRunnerModel.CurrentBuildingTargets)
            {
                var buildingUnit = extendedRunnerModel.CurrentBuildings[pair.Key];
                var buildingUnitTargets = pair.Value;
                foreach (var unit in buildingUnitTargets)
                {
                    Visualizer.DrawLine(buildingUnit.X, buildingUnit.Y, unit.X, unit.Y, 0xFFFFFF44);
                    if (buildingUnit.RemainingActionCooldownTicks != 0)
                    {
                        if (buildingUnit.RemainingActionCooldownTicks == 1)
                        {
                            buildingShoots.Add(new AnimatedUnitInfo(unit.X, unit.Y,
                                extendedRunnerModel.World.TickIndex));
                        }
                        else
                        {
                            const double radius = 10;
                            var max = buildingUnit.CooldownTicks;
                            var i = (max - buildingUnit.RemainingActionCooldownTicks);
                            var xC = buildingUnit.X - (buildingUnit.X - unit.X) * i / max;
                            var yC = buildingUnit.Y - (buildingUnit.Y - unit.Y) * i / max;
                            // Visualizer.Circle(xC, yC, radius, 0xFFFFFF00 + (byte) (200* buildingUnit.RemainingActionCooldownTicks / max), true);
                            Visualizer.DrawCircle(xC, yC, radius, 0x000000AA, false);
                            Visualizer.DrawLine(xC, yC - radius, xC, yC + radius, 0x000000AA);
                            Visualizer.DrawLine(xC - radius, yC, xC + radius, yC, 0x000000AA);
                        }
                    }
                }
            }

            foreach (var info in buildingShoots)
            {
                Visualizer.BuildingShoot(info.X, info.Y, Visualizer.GetNumberDependingOnIndex<BuildingShoortSprite>(4));
            }

            buildingShoots = buildingShoots
                .Where(x => extendedRunnerModel.World.TickIndex < x.TickIndex + 5 * 4)
                .ToList();

            if (targetUnit != null)
            {
                Visualizer.DrawCircle(targetUnit.X, targetUnit.Y, targetUnit.Radius * TargetSelectionRadiusK, 0xFFFFFFFF);
            }
        }

        private void AfterMove(object sender, EventArgs eventArgs)
        {
            Visualizer.EndDrawing();
        }

        public void Stop()
        {
            if (BackgroundMusic != null)
            {
                BackgroundMusic.Stop();
                BackgroundMusic.Dispose();
                BackgroundMusic = null;
            }
        }

        private int? GetDestroyedUnitState(DeadUnitInfo unit, long tickIndex)
        {
            var diff = tickIndex - unit.TickIndex;
            if (diff < NumberOfTicksForSpriteAnimation) return 0;
            else if (diff < NumberOfTicksForSpriteAnimation * 2) return 1;
            else if (diff < NumberOfTicksForSpriteAnimation * 3) return 2;
            else if (diff < NumberOfTicksForSpriteAnimation * 4) return 3;
            else if (diff < NumberOfTicksForSpriteAnimation * 5) return 4;
            else if (diff < NumberOfTicksForSpriteAnimation * 6) return 5;
            else if (diff < NumberOfTicksForSpriteAnimation * 7) return 6;
            else if (diff < NumberOfTicksForSpriteAnimation * 8) return 7;
            else if (diff < NumberOfTicksForSpriteAnimation * 9) return 8;
            else if (diff < NumberOfTicksForSpriteAnimation * 10) return 9;
            else if (diff < NumberOfTicksForSpriteAnimation * 11) return 10;
            else if (diff < NumberOfTicksForSpriteAnimation * 12) return 11;
            else if (diff < NumberOfTicksForSpriteAnimation * 13) return 12;
            else if (diff < NumberOfTicksForSpriteAnimation * 14) return 13;
            else if (diff < NumberOfTicksForSpriteAnimation * 15) return 14;
            else if (diff < NumberOfTicksForSpriteAnimation * 16) return 15;
            else return null;
        }

        private UnitStateSprite? GetKilledMinionState(DeadUnitInfo unit, long tickIndex)
        {
            var diff = tickIndex - unit.TickIndex;
            if (diff < NumberOfTicksForSpriteAnimation) return UnitStateSprite.Die1;
            else if (diff < NumberOfTicksForSpriteAnimation * 2) return UnitStateSprite.Die2;
            else if (diff < 500) return UnitStateSprite.Die3;
            else return null;
        }

        private UnitStateSprite? GetKilledWizardUnionState(DeadUnitInfo unit, long tickIndex)
        {
            var diff = tickIndex - unit.TickIndex;
            if (diff < NumberOfTicksForSpriteAnimation) return UnitStateSprite.Die1;
            else if (diff < NumberOfTicksForSpriteAnimation * 2) return UnitStateSprite.Die2;
            else if (diff < NumberOfTicksForSpriteAnimation * 3) return UnitStateSprite.Die3;
            else if (diff < 1000) return UnitStateSprite.Die4;
            else return null;
        }

        private UnitStateSprite? GetKilledWizardEnemyState(DeadUnitInfo unit, long tickIndex)
        {
            var diff = tickIndex - unit.TickIndex;
            if (diff < NumberOfTicksForSpriteAnimation) return UnitStateSprite.Die1;
            else if (diff < NumberOfTicksForSpriteAnimation * 2) return UnitStateSprite.Die2;
            else if (diff < NumberOfTicksForSpriteAnimation * 3) return UnitStateSprite.Die3;
            else if (diff < NumberOfTicksForSpriteAnimation * 4) return UnitStateSprite.Die4;
            else if (diff < NumberOfTicksForSpriteAnimation * 5) return UnitStateSprite.Die5;
            else if (diff < NumberOfTicksForSpriteAnimation * 6) return UnitStateSprite.Die6;
            else if (diff < 1000) return UnitStateSprite.Die7;
            else return null;
        }

        private CorpseSprite? GetCorpseState(DeadUnitInfo unit, long tickIndex)
        {
            var diff = tickIndex - unit.TickIndex;
            if (diff < 1000) return CorpseSprite.Corpse1;
            else if (diff < 2000) return CorpseSprite.Corpse2;
            else if (diff < 3000) return CorpseSprite.Corpse3;
            else if (diff < 4000) return CorpseSprite.Corpse4;
            else if (diff < 5000) return CorpseSprite.Corpse5;
            else return null;
        }

        private UnitStateSprite GetUnitState(Unit unit, long tickIndex, int remainingActionCooldownTicks)
        {
            bool attack = remainingActionCooldownTicks != 0;
            bool run = unit.SpeedX != 0 || unit.SpeedY != 0;
            bool stay = !attack && !run;
            UnitStateSprite stateSprite;
            if (stay)
            {
                stateSprite = UnitStateSprite.Stay;
            }
            else
            {
                const int maxTicks = 30;
                if (attack)
                {
                    if (remainingActionCooldownTicks % maxTicks < maxTicks / 4)
                    {
                        stateSprite = UnitStateSprite.Attack1;
                    }
                    else if (remainingActionCooldownTicks % maxTicks < maxTicks / 2)
                    {
                        stateSprite = UnitStateSprite.Attack2;
                    }
                    else if (remainingActionCooldownTicks % maxTicks < maxTicks * 3 / 4)
                    {
                        stateSprite = UnitStateSprite.Attack3;
                    }
                    else stateSprite = UnitStateSprite.Attack4;
                }
                else if (run)
                {
                    if (tickIndex % maxTicks < maxTicks / 5)
                    {
                        stateSprite = UnitStateSprite.Run1;
                    }
                    else if (tickIndex % maxTicks < maxTicks * 2 / 5)
                    {
                        stateSprite = UnitStateSprite.Run2;
                    }
                    else if (tickIndex % maxTicks < maxTicks * 3 / 5)
                    {
                        stateSprite = UnitStateSprite.Run3;
                    }
                    else if (tickIndex % maxTicks < maxTicks * 4 / 5)
                    {
                        stateSprite = UnitStateSprite.Run4;
                    }
                    else stateSprite = UnitStateSprite.Run5;
                }
                else throw new Exception("Unknown unit state");
            }
            return stateSprite;
        }

        private bool IsNeutralAttacking(Minion unit)
        {
            return ((unit.Life < unit.MaxLife) || (unit.SpeedX != 0) || (unit.SpeedY != 0) || (((Minion)unit).RemainingActionCooldownTicks > 0));
        }
    }
}