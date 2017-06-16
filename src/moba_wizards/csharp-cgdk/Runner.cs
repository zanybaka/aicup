using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.RunnerExtension;
using NLog;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    public sealed class Runner {
        private readonly RemoteProcessClient remoteProcessClient;
        private readonly string token;
        private static int hotKey;

        public static void Main(string[] args) {
            if (Debugger.IsAttached || args.Length == 0)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(@"local-runner.bat");
                process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory +
                    (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "local-runner-ru-master")) 
                        ? "local-runner-ru-master"
                        : @"..\..\..\local-runner-ru-master\");
                process.Start();
                Thread.Sleep(3000);
            }
            hotKey = HotKeyManager.RegisterHotKey(Keys.F12, KeyModifiers.Control);
            HotKeyManager.HotKeyPressed += OnHotKeyHandler;

            AppDomain.CurrentDomain.DomainUnload += (sender, eventArgs) => HotKeyManager.UnregisterHotKey(hotKey);
            var i = args.Length > 0 ? int.Parse(args[0]) : 0;
            var runner = new Runner(args.Length == 3 ? args : new[] {"127.0.0.1", (31001 + i).ToString(), "0000000000000000"});
            runner.Run();
        }

        private static void OnHotKeyHandler(object sender, HotKeyEventArgs e)
        {
            HotKeyManager.UnregisterHotKey(hotKey);
            Process kill = new Process();
            kill.StartInfo = new ProcessStartInfo("taskkill.exe", "/F /IM javaw.exe /T");
            kill.StartInfo.CreateNoWindow = true;
            kill.Start();
            Environment.Exit(0);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.GetLogger("Runner").Error(e.ExceptionObject.ToString);
        }

        private Runner(string[] args) {
            remoteProcessClient = new RemoteProcessClient(args[0], int.Parse(args[1]));
            token = args[2];
        }

        public void Run() {
            try {
                remoteProcessClient.WriteTokenMessage(token);
                remoteProcessClient.WriteProtocolVersionMessage();
                int teamSize = remoteProcessClient.ReadTeamSizeMessage();
                Game game = remoteProcessClient.ReadGameContextMessage();
                ExtendedRunnerModel extendedRunner = new ExtendedRunnerModel(game);
                VisualizerHost host = new VisualizerHost(extendedRunner);
                host.Start();

                MyStrategy[] strategies = new MyStrategy[teamSize];
                for (int strategyIndex = 0; strategyIndex < teamSize; ++strategyIndex)
                {
                    strategies[strategyIndex] = new MyStrategy(host.Visualizer);
                }

                PlayerContext playerContext;
                while ((playerContext = remoteProcessClient.ReadPlayerContextMessage()) != null)
                {
                    Wizard[] playerWizards = playerContext.Wizards;
                    if (playerWizards == null || playerWizards.Length != teamSize)
                    {
                        break;
                    }
                    extendedRunner.UpdatePlayerContext(playerContext);
                    extendedRunner.OnBeforeMove();
                    Move[] moves = new Move[teamSize];
                    for (int wizardIndex = 0; wizardIndex < teamSize; ++wizardIndex)
                    {
                        Wizard playerWizard = playerWizards[wizardIndex];
                        Move move = new Move();
                        moves[wizardIndex] = move;
                        strategies[wizardIndex].Move(playerWizard, playerContext.World, game, move);
                    }
                    remoteProcessClient.WriteMovesMessage(moves);
                    extendedRunner.OnAfterMove();
                }
                host.Stop();

            } finally {
                remoteProcessClient.Close();
            }
        }
    }
}