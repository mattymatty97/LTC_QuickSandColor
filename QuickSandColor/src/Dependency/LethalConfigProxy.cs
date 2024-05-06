using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;


namespace QuickSandColor.Dependency
{
    public static class LethalConfigProxy
    {
        private static bool? _enabled;

        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig");
                return _enabled.Value;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddConfig(ConfigEntry<string> entry)
        {
            LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(entry, new TextInputFieldOptions()
            {
                RequiresRestart = false
            }));
        }
        
    }
}