using CounterStrikeSharp.API.Core;

namespace LupercaliaMGCore.modules.ExternalView.CSSharp
{
    internal interface ILocalizer
    {
        string LocalizeForPlayer(CCSPlayerController controller, string message, params object[] args);
    }
}
