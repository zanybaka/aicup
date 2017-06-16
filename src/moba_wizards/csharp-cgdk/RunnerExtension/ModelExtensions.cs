using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public static class ModelExtensions
    {
        public static bool IsUnion(this CircularUnit unit, Wizard me)
        {
            return unit.Faction == me.Faction;
        }
    }
}