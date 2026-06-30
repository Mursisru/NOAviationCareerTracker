using System.Reflection;
using NOLoader.AviationCareer.ACT.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NOLoader.AviationCareer.ACT.UI.Contexts
{
    public static class ACTContextHosts
    {
        public static void MountMainMenu(MainMenu menu)
        {
            if (menu == null)
                return;
            if (ACTBootstrap.UIRegistry?.GetHost(ACTGameContextKind.MainMenu) != null)
                return;

            var menuButton = GetPrivateField<Button>(menu, "missionsButton");
            var workshopButton = menuButton != null
                ? ACTGameUiBridge.FindButtonByLabel(menuButton.transform.parent, "WORKSHOP")
                : null;
            ACTGameUiBridge.MenuButtonTemplate = workshopButton ?? menuButton;

            var canvas = ACTCanvasHost.Create("ACT_MainMenuHost", menuButton?.GetComponentInParent<Canvas>()?.rootCanvas);
            ACTShellController.Create(canvas.transform, new ACTMainMenuContext(), menuButton);
            ACTBootstrap.UIRegistry?.RegisterHost(ACTGameContextKind.MainMenu, canvas);
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.MainMenuReady, Time.time));
        }

        public static void MountBriefing(AircraftSelectionMenu menu)
        {
            if (menu == null)
                return;
            if (ACTBootstrap.UIRegistry?.GetHost(ACTGameContextKind.Briefing) != null)
                return;
            var canvas = ACTCanvasHost.Create("ACT_BriefingHost", menu.GetComponentInParent<Canvas>()?.rootCanvas);
            ACTShellController.Create(canvas.transform, new ACTBriefingContext());
            ACTBootstrap.UIRegistry?.RegisterHost(ACTGameContextKind.Briefing, canvas);
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.BriefingOpened, Time.time));
        }

        public static void MountPause(GameplayUI ui)
        {
            if (ui == null)
                return;
            if (ACTBootstrap.UIRegistry?.GetHost(ACTGameContextKind.Pause) != null)
                return;
            var canvas = ACTCanvasHost.Create("ACT_PauseHost", ui.GetComponentInParent<Canvas>()?.rootCanvas);
            ACTShellController.Create(canvas.transform, new ACTPauseContext());
            ACTBootstrap.UIRegistry?.RegisterHost(ACTGameContextKind.Pause, canvas);
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.GamePaused, Time.time));
        }

        public static void MountDebrief(Leaderboard board)
        {
            if (board == null)
                return;
            if (ACTBootstrap.UIRegistry?.GetHost(ACTGameContextKind.Debrief) != null)
                return;
            var canvas = ACTCanvasHost.Create("ACT_DebriefHost", board.GetComponentInParent<Canvas>()?.rootCanvas);
            ACTShellController.Create(canvas.transform, new ACTDebriefContext());
            ACTBootstrap.UIRegistry?.RegisterHost(ACTGameContextKind.Debrief, canvas);
            ACTEventBus.Publish(new ACTEvent(ACTEventKind.DebriefOpened, Time.time));
        }

        private static T? GetPrivateField<T>(object target, string fieldName) where T : class
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(target) as T;
        }
    }
}
