using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace LupercaliaMGCore.modules.ExternalView
{
    /// <summary>
    /// Fixes the issue with attack and use position while using custom view entity.
    ///
    /// Uses CounterStrikeSharp's built-in OnServerPreEntityThink/OnServerPostEntityThink events
    /// instead of requiring a separate C++ metamod plugin.
    ///
    /// This fixer enforces all players will be treated as if they are not using
    /// custom view entity during server's ENTITY_THINK.
    /// So, all attack and use actions will be performed from the correct location
    /// (i.e., the front of the player's pawn)
    /// </summary>
    internal class AttackAndUsePositionFixer : IDisposable
    {
        public delegate IEnumerable<CCSPlayerController?> GetPlayersFn();

        private ILogger Logger;
        private GetPlayersFn _GetPlayers;
        private BasePlugin _Plugin;

        private bool _IsHooked = false;

        private record FixedPlayer(
            CCSPlayerController Controller,
            uint LastViewEntityHandleRaw
        );

        private List<FixedPlayer> _FixedPlayers = new();

        public AttackAndUsePositionFixer(ILogger logger, BasePlugin plugin, GetPlayersFn getPlayers)
        {
            Logger = logger;
            _Plugin = plugin;
            _GetPlayers = getPlayers;

            SetupHooks();
        }

        void IDisposable.Dispose()
        {
            Unload();
        }

        public void Unload()
        {
            if (_IsHooked)
            {
                _Plugin.RemoveListener<Listeners.OnServerPreEntityThink>(OnPreEntityThink);
                _Plugin.RemoveListener<Listeners.OnServerPostEntityThink>(OnPostEntityThink);
                _IsHooked = false;
            }
        }

        private void SetupHooks()
        {
            if (_IsHooked)
                return;

            _Plugin.RegisterListener<Listeners.OnServerPreEntityThink>(OnPreEntityThink);
            _Plugin.RegisterListener<Listeners.OnServerPostEntityThink>(OnPostEntityThink);
            _IsHooked = true;

            Logger.LogInformation("[ExternalView] AttackAndUsePositionFixer initialized using CounterStrikeSharp events.");
        }

        private void OnPreEntityThink()
        {
            foreach (CCSPlayerController? controller in _GetPlayers())
            {
                if (controller == null)
                    continue;

                var pawn = controller.PlayerPawn.Value;
                var cam = pawn?.CameraServices;
                var viewEntityHandle = cam?.ViewEntity;
                var viewEntityHandleRaw = viewEntityHandle?.Raw;
                if (!viewEntityHandleRaw.HasValue)
                    continue;

                if (viewEntityHandleRaw.Value == uint.MaxValue)
                    continue;

                _FixedPlayers.Add(new FixedPlayer(controller, viewEntityHandleRaw.Value));

                if (viewEntityHandle != null)
                {
                    viewEntityHandle.Raw = uint.MaxValue;
                }
            }
        }

        private void OnPostEntityThink()
        {
            foreach (var player in _FixedPlayers)
            {
                if (!player.Controller.IsValid)
                    continue;

                var pawn = player.Controller.PlayerPawn.Value;
                var cam = pawn?.CameraServices;
                var viewEntityHandle = cam?.ViewEntity;
                if (viewEntityHandle == null)
                    continue;

                viewEntityHandle.Raw = player.LastViewEntityHandleRaw;
            }
            _FixedPlayers.Clear();
        }
    }
}
