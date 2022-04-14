using System;
using ContextPlugin.Context;
using ContextPlugin.Plugin;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace ContextPlugin;

public unsafe class PluginGui : IDisposable {
    private readonly PluginMain m_Plugin;

    public PluginGui(PluginMain plugin) {
        m_Plugin = plugin;
        m_Plugin.PluginInterface.UiBuilder.Draw += OnDraw;
        //m_Plugin.Context.MenuOpen += ContextOnMenuOpen;
        //m_Plugin.Context.SubMenuOpen += ContextOnSubMenuOpen;
        //m_Plugin.Context.InventoryMenuOpen += ContextOnInventoryMenuOpen;
    }

    public void Dispose() {
        m_Plugin.PluginInterface.UiBuilder.Draw -= OnDraw;
        //m_Plugin.Context.MenuOpen -= ContextOnMenuOpen;
        //m_Plugin.Context.SubMenuOpen -= ContextOnSubMenuOpen;
        //m_Plugin.Context.InventoryMenuOpen -= ContextOnInventoryMenuOpen;
    }

    private void OnDraw() {
        //if (ImGui.Begin("ContextPlugin Window")) {
            
        //}
        //ImGui.End();
    }

    //private void ContextOnInventoryMenuOpen(InventoryContextMenuOpenArgs obj) {
    //    obj.AddMenuItem("InvItem 1", _ => { PluginLog.LogInformation("InvItem 1"); });
    //    obj.AddMenuItem("InvItem 2", _ => { PluginLog.LogInformation("InvItem 2"); });
    //    obj.AddSubMenu("InvSub Item 1", args => {
    //        PluginLog.LogInformation("InvSub Item 1 Open");
    //        args.AddMenuItem("InvSub 1", _ => { PluginLog.LogInformation("InvSub 1"); });
    //        args.AddMenuItem("InvSub 2", _ => { PluginLog.LogInformation("InvSub 2"); });
    //    });
    //}

    //private void ContextOnMenuOpen(ContextMenuOpenArgs obj) {
    //    obj.AddMenuItem("Item 1", _ => { PluginLog.LogInformation("Item 1"); });
    //    obj.AddMenuItem("Item 2", _ => { PluginLog.LogInformation("Item 2"); });
    //    obj.AddSubMenu("Sub Item 1", args => {
    //        PluginLog.LogInformation("Sub Item 1 Open");
    //        args.AddMenuItem("Sub 1", _ => { PluginLog.LogInformation("Sub 1"); });
    //        args.AddMenuItem("Sub 2", _ => { PluginLog.LogInformation("Sub 2"); });
    //    });
    //}

    //private void ContextOnSubMenuOpen(ContextSubMenuOpenArgs obj) {
    //    obj.AddMenuItem("Random Ass SubItem 1", _ => { PluginLog.LogInformation("Lol"); });
    //}
}