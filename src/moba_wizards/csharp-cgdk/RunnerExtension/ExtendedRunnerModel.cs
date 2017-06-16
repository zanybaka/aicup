using System;
using System.Collections.Generic;
using System.Linq;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.RunnerExtension
{
    public class ExtendedRunnerModel
    {
        public Dictionary<long, Building> AllBuildings { get; private set; } = new Dictionary<long, Building>();
        public Dictionary<long, Wizard> AllWizards { get; private set; } = new Dictionary<long, Wizard>();
        public Dictionary<long, Minion> AllMinions { get; private set; } = new Dictionary<long, Minion>();
        public Dictionary<long, Projectile> AllProjectiles { get; private set; } = new Dictionary<long, Projectile>();

        public Dictionary<long, List<LivingUnit>> CurrentBuildingTargets { get; private set; } = new Dictionary<long, List<LivingUnit>>();
        public Dictionary<long, Building> CurrentBuildings { get; private set; } = new Dictionary<long, Building>();
        public Dictionary<long, Wizard> CurrentWizards { get; private set; } = new Dictionary<long, Wizard>();
        public Dictionary<long, Minion> CurrentMinions { get; private set; } = new Dictionary<long, Minion>();
        public Dictionary<long, Projectile> CurrentProjectiles { get; set; } = new Dictionary<long, Projectile>();

        public Dictionary<long, DeadUnitInfo> DeadBuildings { get; } = new Dictionary<long, DeadUnitInfo>();
        public Dictionary<long, DeadUnitInfo> DeadWizards { get; } = new Dictionary<long, DeadUnitInfo>();
        public Dictionary<long, DeadUnitInfo> DeadMinions { get; } = new Dictionary<long, DeadUnitInfo>();
        public Dictionary<long, DeadUnitInfo> DeadProjectiles { get; private set; } = new Dictionary<long, DeadUnitInfo>();

        public event EventHandler WorldUpdated;
        public event EventHandler BeforeMove;
        public event EventHandler AfterMove;

        public Game Game { get; }
        public World World { get; private set; }
        public Wizard Me { get; private set; }

        public ExtendedRunnerModel(Game game)
        {
            Game = game;
        }

        public void UpdatePlayerContext(PlayerContext context)
        {
            World = context.World;
            Me = context.Wizards.Single(x => x.IsMe);
            CurrentBuildings = new Dictionary<long, Building>();
            CurrentWizards = new Dictionary<long, Wizard>();
            CurrentMinions = new Dictionary<long, Minion>();
            CurrentProjectiles = new Dictionary<long, Projectile>();

            foreach (var unit in context.World.Buildings)
            {
                DeadBuildings.Remove(unit.Id);
                AllBuildings[unit.Id] = unit;
                CurrentBuildings[unit.Id] = unit;
            }
            foreach (var unit in context.World.Wizards)
            {
                DeadWizards.Remove(unit.Id);
                AllWizards[unit.Id] = unit;
                CurrentWizards[unit.Id] = unit;
            }
            foreach (var unit in context.World.Minions)
            {
                DeadMinions.Remove(unit.Id);
                AllMinions[unit.Id] = unit;
                CurrentMinions[unit.Id] = unit;
            }

            foreach (var unit in context.World.Projectiles)
            {
                DeadProjectiles.Remove(unit.Id);
                AllProjectiles[unit.Id] = unit;
                CurrentProjectiles[unit.Id] = unit;
            }

            foreach (var diedUnit in AllBuildings.Except(CurrentBuildings).Where(x => x.Value.IsUnion(Me) || x.Value.Life <= 36))
            {
                DeadBuildings[diedUnit.Key] = new DeadUnitInfo(context.World, diedUnit.Value, Me);
            }

            foreach (var diedUnit in AllWizards.Except(CurrentWizards).Where(x => x.Value.IsUnion(Me) || x.Value.Life <= 36))
            {
                DeadWizards[diedUnit.Key] = new DeadUnitInfo(context.World, diedUnit.Value, Me);
            }

            foreach (var diedUnit in AllMinions.Except(CurrentMinions).Where(x => x.Value.IsUnion(Me) || x.Value.Life <= 36))
            {
                DeadMinions[diedUnit.Key] = new DeadUnitInfo(context.World, diedUnit.Value, Me);
            }

            foreach (var diedUnit in AllProjectiles.Except(CurrentProjectiles))
            {
                bool isTargetTree = context.World.Trees.Any(x => x.GetDistanceTo(diedUnit.Value) < 220);
                DeadProjectiles[diedUnit.Key] = new DeadUnitInfo(context.World, diedUnit.Value, Me, isTargetTree);
            }

            AllBuildings = CurrentBuildings;
            AllWizards = CurrentWizards;
            AllMinions = CurrentMinions;
            AllProjectiles = CurrentProjectiles;

            CurrentBuildingTargets = new Dictionary<long, List<LivingUnit>>();
            foreach (var building in CurrentBuildings)
            {
                var buildingUnit = building.Value;
                var attackRange = buildingUnit.Type == BuildingType.GuardianTower ? Game.GuardianTowerAttackRange : Game.FactionBaseAttackRange;
                var maxDamage = buildingUnit.Type == BuildingType.GuardianTower ? Game.GuardianTowerDamage : Game.FactionBaseDamage;
                List<LivingUnit> p1Units = new List<LivingUnit>();
                List<LivingUnit> p2Units = new List<LivingUnit>();
                foreach (var minion in CurrentMinions)
                {
                    FindBuildingTarget(minion.Value, buildingUnit, attackRange, maxDamage, p1Units, p2Units);
                }
                foreach (var wizard in CurrentWizards)
                {
                    FindBuildingTarget(wizard.Value, buildingUnit, attackRange, maxDamage, p1Units, p2Units);
                }
                var buildingTargets = p1Units.Count > 0 ? p1Units : p2Units;
                CurrentBuildingTargets[building.Key] = buildingTargets;
            }

            DeadProjectiles = DeadProjectiles
                .Where(x => x.Value.ProjectileType == ProjectileType.MagicMissile
                         && context.World.TickIndex < x.Value.TickIndex + 5 * 4)
                .ToDictionary(x => x.Key, y => y.Value);

            WorldUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void OnBeforeMove()
        {
            BeforeMove?.Invoke(this, EventArgs.Empty);
        }

        public void OnAfterMove()
        {
            AfterMove?.Invoke(this, EventArgs.Empty);
        }

        private void FindBuildingTarget(LivingUnit unit, Building buildingUnit, double attackRange, int maxDamage,
            List<LivingUnit> p1Units, List<LivingUnit> p2Units)
        {
            if ((unit.Faction == Faction.Academy || unit.Faction == Faction.Renegades)
                && unit.Faction != buildingUnit.Faction && unit.GetDistanceTo(buildingUnit) <= attackRange)
            {
                if (unit.Life >= maxDamage)
                {
                    if (p1Units.Count == 0 || unit.Life < p1Units[0].Life)
                    {
                        p1Units.Clear();
                        p1Units.Add(unit);
                    }
                    else if (unit.Life == p1Units[0].Life)
                    {
                        p1Units.Add(unit);
                    }
                }
                else
                {
                    if (p2Units.Count == 0 || unit.Life < p2Units[0].Life)
                    {
                        p2Units.Clear();
                        p2Units.Add(unit);
                    }
                    else if (unit.Life == p2Units[0].Life)
                    {
                        p2Units.Add(unit);
                    }
                }
            }
        }
    }
}
