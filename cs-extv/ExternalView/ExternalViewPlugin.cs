using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.modules.ExternalView;
using TNCSSPluginFoundation;

namespace LupercaliaMGCore;

public sealed class ExternalViewPlugin : TncssPluginBase
{
    public override string PluginPrefix =>
        $" [{ChatColors.Orange}ExternalView{ChatColors.Default}]";

    public override bool UseTranslationKeyInPluginPrefix => false;

    public override string ModuleName => "External View";

    public override string ModuleVersion => "2.0.0";

    public override string ModuleAuthor => "Spitice";

    public override string ModuleDescription => "A standalone plugin of Lupercalia MG ExternalView";

    public override string BaseCfgDirectoryPath => Path.Combine(Server.GameDirectory, "csgo/cfg/extv/");

    public override string ConVarConfigPath => Path.Combine(BaseCfgDirectoryPath, "extv.cfg");

    protected override void TncssOnPluginLoad(bool hotReload)
    {
        RegisterModule<ExternalView>();
    }
}
