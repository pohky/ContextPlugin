using System;
using System.Linq;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ContextPlugin.Context; 

internal unsafe class ContextMenuHook : IDisposable {
    private delegate void OpenContextDelegate(AgentContext* agent, bool setOwner, bool closeExisting);

    private readonly Hook<OpenContextDelegate> m_OpenContextHook;
    private readonly CustomAtkEventInterface m_ContextEventInterface;
    private readonly InventoryContextMenuHook m_InventoryContextMenuHook;

    private ContextMenuOpenArgs? m_CurrentOpenArgs;
    private SubContextMenuOpenArgs? m_CurrentSubMenuOpenArgs;

    public event ContextMenuOpenEventDelegate? ContextMenuOpen;
    public event InventoryContextMenuOpenEventDelegate? InventoryContextMenuOpen;

    public ContextMenuHook() {
        m_ContextEventInterface = new CustomAtkEventInterface(ReceiveEvent);
        m_OpenContextHook = new Hook<OpenContextDelegate>((nint)AgentContext.fpOpenContextMenu, OpenContextDetour);
        m_OpenContextHook.Enable();
        m_InventoryContextMenuHook = new InventoryContextMenuHook();
    }
    
    public void Dispose() {
        m_InventoryContextMenuHook.Dispose();
        m_OpenContextHook.Dispose();
        m_ContextEventInterface.Dispose();
    }

    private void OpenContextDetour(AgentContext* agent, bool setowner, bool closeexisting) {
        // opening a submenu, just let it do it's thing and return
        if (agent->ContextMenuIndex > 0) {
            m_OpenContextHook.Original(agent, setowner, closeexisting);
            return;
        }
        
        var inv = AgentInventoryContext.Instance();
        var isInventoryContext = m_InventoryContextMenuHook.IsOpen && inv->TargetInventorySlot != null;
        
        if (isInventoryContext) {
            var item = inv->TargetInventorySlot;
            var itemId = item->ItemID;
            var inventoryId = (uint)item->Container;
            m_CurrentOpenArgs = new InventoryContextMenuOpenArgs(itemId, inventoryId);
            InventoryContextMenuOpen?.Invoke((InventoryContextMenuOpenArgs)m_CurrentOpenArgs);
        } else {
            m_CurrentOpenArgs = new ContextMenuOpenArgs();
            ContextMenuOpen?.Invoke(m_CurrentOpenArgs);
        }

        foreach (var item in m_CurrentOpenArgs.CustomMenuItems)
            agent->AddMenuItem($"[C] {item.Name}", m_ContextEventInterface.Pointer, item.Id, item.IsDisabled, item is CustomSubContextMenuItem);

        m_InventoryContextMenuHook.IsOpen = false;

        m_OpenContextHook.Original(agent, setowner, closeexisting);
    }

    private int* ReceiveEvent(void* self, int* a2, AtkValue* eventParams, nint a4, long param) {
        //check if a regular custom item was clicked
        var item = m_CurrentOpenArgs?.CustomMenuItems.FirstOrDefault(m => m.Id == param);
        if (item == null) {
            //check if a submenu custom item was clicked
            item = m_CurrentSubMenuOpenArgs?.CustomMenuItems.FirstOrDefault(m => m.Id == param);
            if (item is { } subItem) {
                //if the item is inside a custom submenu call it's handler and return
                subItem.Handler?.Invoke();
                *a2 = 2;
                return a2;
            }
        }

        if (item is CustomSubContextMenuItem subMenu) {
            //if the item is a submenu ask for items to add to it
            m_CurrentSubMenuOpenArgs = new SubContextMenuOpenArgs();
            subMenu.SubMenuHandler?.Invoke(m_CurrentSubMenuOpenArgs);

            //open the submenu and add the custom items
            var ctx = AgentContext.Instance();
            ctx->OpenSubMenu();
            foreach (var subItem in m_CurrentSubMenuOpenArgs.CustomMenuItems)
                ctx->AddMenuItem(subItem.Name, m_ContextEventInterface.Pointer, subItem.Id, subItem.IsDisabled);

            //set inventory context to open so it doesn't get lost when the submenu is opened and closed
            if (m_CurrentOpenArgs is InventoryContextMenuOpenArgs)
                m_InventoryContextMenuHook.IsOpen = true;

        } else {
            //if it's a regular custom item just call the click handler
            item?.Handler?.Invoke();
        }

        *a2 = 2;
        return a2;
    }
}