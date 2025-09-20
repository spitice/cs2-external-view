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

    public override string ModuleVersion => "3.1.0";

    public override string ModuleAuthor => "Spitice, uru";

    public override string ModuleDescription => "A standalone plugin of Lupercalia MG ExternalView - now C# only!";

    public override string BaseCfgDirectoryPath => Path.Combine(Server.GameDirectory, "csgo/cfg/extv/");

    public override string ConVarConfigPath => Path.Combine(BaseCfgDirectoryPath, "extv.cfg");

    protected override void TncssOnPluginLoad(bool hotReload)
    {
        RegisterModule<ExternalView>();
    }
}
