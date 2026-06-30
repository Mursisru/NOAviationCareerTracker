using System;
using NOLoader.API;
using NOLoader.ModConfig;
using NOLoader.AviationCareer.ACT.Core;
using NOLoader.AviationCareer.ACT.UI.Contexts;
using UnityEngine;

namespace NOLoader.AviationCareer
{
    public sealed class AviationCareerMod : INOMod, INOModTickSlow
    {
        private const string DefaultIni = @"[AviationCareer]
Enabled=true
XpOverlayThreshold=250
AutoRecord=true
RecordFlushIntervalSec=2
TacViewDefaultSpeed=1
";

        public void OnLoad(ref NOModContext ctx)
        {
            try
            {
                ModIniConfig.EnsureDefault(ctx.ModRoot, DefaultIni);
                ACTConfigCache.Load(ModIniConfig.Load(ctx.ModRoot));
                ACTBootstrap.Initialize(ctx);
                TryMountMainMenuUi();
                Debug.Log($"[ACT] Aviation Career Tracker {AppVersion.DisplayVersion} loaded (stage={ctx.Stage}).");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ACT] OnLoad failed: " + ex.Message);
                Debug.LogException(ex);
                throw;
            }
        }

        private static void TryMountMainMenuUi()
        {
            var menu = UnityEngine.Object.FindObjectOfType<MainMenu>();
            if (menu != null)
                ACTContextHosts.MountMainMenu(menu);
        }

        public void OnUnload(ref NOModContext ctx)
        {
            ACTBootstrap.Shutdown();
            Debug.Log("[ACT] Unloaded.");
        }

        public void OnSlowUpdate(ref NOModContext ctx, float dt)
        {
            ACTBootstrap.OnSlowTick(dt);
        }
    }
}
