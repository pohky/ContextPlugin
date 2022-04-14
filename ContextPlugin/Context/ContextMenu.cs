using System;

namespace ContextPlugin.Context;

public sealed class ContextMenu : IDisposable {
    private readonly ContextMenuHook m_ContextMenuHook;
    
    public event ContextMenuOpenEventDelegate? MenuOpen;
    public event ContextMenuOpenEventDelegate? InventoryMenuOpen;

    public ContextMenu() {
        m_ContextMenuHook = new ContextMenuHook();
        m_ContextMenuHook.ContextMenuOpen += OnMenuOpen;
        m_ContextMenuHook.InventoryContextMenuOpen += OnInventoryMenuOpen;
    }

    private void OnInventoryMenuOpen(ContextMenuOpenArgs args) {
        InventoryMenuOpen?.Invoke(args);
    }

    private void OnMenuOpen(ContextMenuOpenArgs args) {
        MenuOpen?.Invoke(args);
    }

    public void Dispose() {
        m_ContextMenuHook.InventoryContextMenuOpen -= OnInventoryMenuOpen;
        m_ContextMenuHook.ContextMenuOpen -= OnMenuOpen;
        m_ContextMenuHook.Dispose();
    }
}