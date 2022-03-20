using HarmonyLib;
namespace PerformanceImprovements.Patches
{
    [HarmonyPatch(typeof(MenuControllerHandler),"Update")]
    [HarmonyPriority(Priority.First)]
    class MenuControllerHandler_Patch_Update
    {
        static bool Prefix()
        {
            return !PerformanceImprovements.FixControllerLag || !PerformanceImprovements.GameInProgress || EscapeMenuHandler.isEscMenu;
        }
    }
}
