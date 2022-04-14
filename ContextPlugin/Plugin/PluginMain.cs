using ContextPlugin.Context;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs;
using SigScanner = Dalamud.Game.SigScanner;

namespace ContextPlugin.Plugin; 

public sealed class PluginMain : IDalamudPlugin {
    public string Name => "ContextPlugin";

    public readonly DalamudPluginInterface PluginInterface;
    public readonly Framework Framework;
    public readonly PluginConfig Config;
    public readonly ContextMenu Context;
    private readonly PluginGui m_Gui;

    public PluginMain(DalamudPluginInterface pluginInterface, Framework framework, SigScanner scanner) {
        Resolver.Initialize(scanner.SearchBase);
        PluginInterface = pluginInterface;
        Framework = framework;
        Config = pluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
        Context = new ContextMenu();
        m_Gui = new PluginGui(this);

        Context.MenuOpen += ContextOnMenuOpen;
        Context.InventoryMenuOpen += ContextOnInventoryMenuOpen;
    }

    private void ContextOnInventoryMenuOpen(ContextMenuOpenArgs args) {
        args.AddItem("Some Inventory Item", () => PluginLog.LogInformation("Whatever"));
        args.AddSubMenu("Some Submenu", openArgs => {
            openArgs.AddItem("Fart", () => PluginLog.LogInformation("Pfffft"));
        });
    }

    private void ContextOnMenuOpen(ContextMenuOpenArgs args) {
        args.AddItem("Random Test Item", () => PluginLog.LogInformation("Random Test Item Clicked"));
        args.AddSubMenu("Random Sub Menu", openArgs => {
            openArgs.AddItem("Sub 1", () => PluginLog.LogInformation("Sub 1 Click"));
            openArgs.AddItem("Sub 2", () => PluginLog.LogInformation("Sub 2 Click"));
        });
    }

    public void Dispose() {
        Context.InventoryMenuOpen -= ContextOnInventoryMenuOpen;
        Context.MenuOpen -= ContextOnMenuOpen;
        PluginInterface.SavePluginConfig(Config);
        m_Gui.Dispose();
        Context.Dispose();
    }
}