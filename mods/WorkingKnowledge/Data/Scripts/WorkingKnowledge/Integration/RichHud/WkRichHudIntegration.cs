using System;
using RichHudFramework.Client;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using VRage.Utils;
using VRageMath;

namespace WkKn
{
    internal sealed class WkRichHudIntegration
    {
        private const long ApiStartupGraceTicks = 600;
        private const string ClientName = "Working Knowledge";

        private readonly WkProgressHudOverlay progressOverlay;
        private Action readyCallback;
        private Action resetCallback;
        private bool initialized;
        private bool dedicatedServer;
        private bool apiReady;
        private bool apiReadyLogged;
        private bool apiUnavailableLogged;

        internal WkRichHudIntegration(Color researchColor, Color proficiencyColor)
        {
            progressOverlay = new WkProgressHudOverlay(researchColor, proficiencyColor);
        }

        internal bool IsReady
        {
            get { return apiReady; }
        }

        internal void Initialize(Action onReady, Action onReset)
        {
            if (initialized || MyAPIGateway.Utilities == null)
                return;

            initialized = true;
            readyCallback = onReady;
            resetCallback = onReset;
            dedicatedServer = MyAPIGateway.Utilities.IsDedicated;
            if (dedicatedServer)
            {
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " dedicated server detected; Rich HUD rendering and settings are handled by connected clients.");
                return;
            }

            try
            {
                RichHudClient.Init(ClientName, OnApiReady, OnApiReset);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " Rich HUD client initialization failed: " + exception.Message);
            }
        }

        internal void Update(long currentTick, WkProgressHudSettings settings)
        {
            if (dedicatedServer)
                return;

            if (!apiReady)
            {
                if (!apiUnavailableLogged && currentTick >= ApiStartupGraceTicks)
                {
                    apiUnavailableLogged = true;
                    MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " Rich HUD Master is unavailable; the progress overlay and settings menu will remain hidden.");
                }

                return;
            }

            progressOverlay.Update(currentTick, settings);
        }

        internal void Clear()
        {
            progressOverlay.Clear();
        }

        internal void Close()
        {
            progressOverlay.Close();

            if (apiReady)
            {
                try
                {
                    RichHudClient.Reset();
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " Rich HUD client reset failed: " + exception.Message);
                }
            }

            apiReady = false;
            apiReadyLogged = false;
            apiUnavailableLogged = false;
            initialized = false;
            readyCallback = null;
            resetCallback = null;
        }

        internal void UpdateCombined(long identityId, string progressId, string displayName, double researchProgress, double proficiencyProgress, long currentTick)
        {
            progressOverlay.UpdateCombined(identityId, progressId, displayName, researchProgress, proficiencyProgress, currentTick);
        }

        private void OnApiReady()
        {
            apiReady = true;
            apiUnavailableLogged = false;
            progressOverlay.Attach(HudMain.HighDpiRoot);

            if (readyCallback != null)
                readyCallback();

            if (!apiReadyLogged)
            {
                apiReadyLogged = true;
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " Rich HUD Master connected; the progress overlay and settings menu are ready.");
            }
        }

        private void OnApiReset()
        {
            progressOverlay.Detach();
            apiReady = false;

            if (resetCallback != null)
                resetCallback();
        }
    }
}
