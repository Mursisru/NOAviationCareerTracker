using System;
using System.Collections.Generic;
using System.IO;
using NOLoader.AviationCareer.ACT.Core.Analytics;
using NOLoader.AviationCareer.ACT.Core.Data;
using NOLoader.AviationCareer.ACT.Core.Progression;
using NOLoader.AviationCareer.ACT.Recording;
using NOLoader.AviationCareer.ACT.UI;
using NOLoader.AviationCareer.ACT.UI.Tabs;

namespace NOLoader.AviationCareer.ACT.Core
{
    public static class ACTBootstrap
    {
        public static ACTProfileStore Profile { get; private set; } = null!;
        public static ACTSessionRuntime Session { get; private set; } = null!;
        public static ACTUIRegistry UIRegistry { get; private set; } = null!;
        public static ACTXPEngine XPEngine { get; private set; } = null!;
        public static ACTDeltaBuffer DeltaBuffer { get; private set; } = null!;
        public static ACTNorepWriter NorepWriter { get; private set; } = null!;
        public static string ModRoot { get; private set; } = string.Empty;

        private static readonly List<IDisposable> Disposables = new List<IDisposable>();

        public static void Initialize(NOLoader.API.NOModContext ctx)
        {
            ModRoot = ctx.ModRoot;
            UnitEconomyDatabase.Initialize();

            var dataRoot = Path.Combine(ctx.ModRoot, "Data");
            Directory.CreateDirectory(dataRoot);
            Directory.CreateDirectory(Path.Combine(dataRoot, "Recordings"));

            Profile = new ACTProfileStore(Path.Combine(dataRoot, "profile.act"));
            Profile.Load();

            Session = new ACTSessionRuntime();
            UIRegistry = new ACTUIRegistry();
            XPEngine = new ACTXPEngine(Profile);
            DeltaBuffer = new ACTDeltaBuffer();
            NorepWriter = new ACTNorepWriter(DeltaBuffer, Path.Combine(dataRoot, "Recordings"));

            WireServices();
            WireEventHandlers();

            ACTEventBus.Subscribe(OnGlobalEvent);
        }

        public static void Shutdown()
        {
            ACTEventBus.Unsubscribe(OnGlobalEvent);
            NorepWriter?.StopAndFinalize();
            Profile?.Save();
            ACTEventBus.Clear();

            for (int i = Disposables.Count - 1; i >= 0; i--)
                Disposables[i].Dispose();
            Disposables.Clear();
        }

        public static void OnSlowTick(float dt)
        {
            NorepWriter?.TickFlush(ACTConfigCache.Instance.RecordFlushIntervalSec);
        }

        private static void WireServices()
        {
            var ledger = new FactionLedgerService(Session);
            var weapons = new WeaponsAuditService(Session);
            var flight = new FlightPerformanceService(Session);
            var patterns = new TacticalPatternService(Session);
            var blackBox = new BlackBoxService(Session);
            var recorder = new RecorderService(Session, NorepWriter);

            Session.RegisterService(ledger);
            Session.RegisterService(weapons);
            Session.RegisterService(flight);
            Session.RegisterService(patterns);
            Session.RegisterService(blackBox);
            Session.RegisterService(recorder);

            TabServiceLocator.Register(ledger);
            TabServiceLocator.Register(weapons);
            TabServiceLocator.Register(flight);
            TabServiceLocator.Register(patterns);
            TabServiceLocator.Register(blackBox);
            TabServiceLocator.Register(recorder);
        }

        private static void WireEventHandlers()
        {
            ACTEventBus.Subscribe(evt =>
            {
                Session.Dispatch(evt);
                ReplayAdapter.HandleEvent(evt, DeltaBuffer, Session);
            });
        }

        private static void OnGlobalEvent(ACTEvent evt)
        {
            ACTCombatBridge.Handle(evt);
            if (evt.Kind == ACTEventKind.SessionStarted && ACTConfigCache.Instance.AutoRecord)
                NorepWriter.StartSession(evt.Payload);
            else if (evt.Kind == ACTEventKind.SessionEnded)
            {
                NorepWriter.StopAndFinalize();
                Profile?.Save();
            }
        }
    }
}
