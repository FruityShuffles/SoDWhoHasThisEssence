using UnityEngine;

namespace SoDWhoHasThisEssence
{
    // Entry point. The game's mod loader adds every ModBehaviour in our assembly to an inactive container GameObject,
    // sets `instance`, loads the config, then activates it (Awake runs). Mods can be unloaded and live-reloaded, so
    // OnDestroy undoes everything: patches and every UI object we added.
    public class WhoHasThisEssenceMod : ModBehaviour
    {
        public EssenceConfig config = new EssenceConfig();

        internal static WhoHasThisEssenceMod Instance { get; private set; }

        // The mod manager copies edited values into `config` in place, so reading it on every update applies a change
        // without a restart.
        internal static bool ShowOtherDuplicates => Instance != null && Instance.config.showOtherDuplicates;

        private void Awake()
        {
            Instance = this;
            harmony.PatchAll(typeof(WhoHasThisEssenceMod).Assembly);
            Log.Info($"Loaded {mod.metadata.id} {mod.metadata.modVer}; running game {Application.version}");
        }

        private void OnDestroy()
        {
            harmony.UnpatchAll(harmony.Id);
            AddedUi.DestroyAll();
            if (Instance == this) Instance = null;
            Log.Info("Unloaded");
        }
    }

    internal static class Log
    {
        public static void Info(string msg) => Debug.Log("[WHTE] " + msg);
        public static void Warn(string msg) => Debug.LogWarning("[WHTE] " + msg);
    }
}
