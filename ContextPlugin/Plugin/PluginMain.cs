using ContextPlugin.Context;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace ContextPlugin.Plugin; 

public sealed class PluginMain : IDalamudPlugin {
    public string Name => "ContextPlugin";

    public readonly ContextMenu Context;

    public PluginMain() {
        Context = new ContextMenu();

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
        Context.Dispose();
    }
}