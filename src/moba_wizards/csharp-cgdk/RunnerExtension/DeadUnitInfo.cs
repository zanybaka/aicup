using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public class DeadUnitInfo
    {
        public long TickIndex;
        public double X;
        public double Y;
        public double Angle;
        public bool IsUnion;
        public bool IsTargetTree;
        public ProjectileType ProjectileType;
        public BuildingType BuildingType;
        public MinionType MinionType;
        public Faction Faction;

        public DeadUnitInfo(World world, Building unit, Wizard me)
        {
            Faction = unit.Faction;
            Angle = unit.Angle;
            IsUnion = unit.IsUnion(me);
            TickIndex = world.TickIndex;
            X = unit.X;
            Y = unit.Y;
            BuildingType = unit.Type;
        }

        public DeadUnitInfo(World world, Wizard unit, Wizard me)
        {
            Faction = unit.Faction;
            Angle = unit.Angle;
            IsUnion = unit.IsUnion(me);
            TickIndex = world.TickIndex;
            X = unit.X;
            Y = unit.Y;
        }

        public DeadUnitInfo(World world, Minion unit, Wizard me)
        {
            Faction = unit.Faction;
            Angle = unit.Angle;
            IsUnion = unit.IsUnion(me);
            TickIndex = world.TickIndex;
            X = unit.X;
            Y = unit.Y;
            MinionType = unit.Type;
        }

        public DeadUnitInfo(World world, Projectile unit, Wizard me, bool isTargetTree)
        {
            Faction = unit.Faction;
            Angle = unit.Angle;
            IsUnion = unit.IsUnion(me);
            TickIndex = world.TickIndex;
            X = unit.X + unit.SpeedX;
            Y = unit.Y + unit.SpeedY;
            ProjectileType = unit.Type;
            IsTargetTree = isTargetTree;
        }
    }
}