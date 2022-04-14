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
    public event ContextMenuOpenEventDelegate? InventoryContextMenuOpen;

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

        var isInventoryContext = agent->OwnerAddon == AgentInventoryContext.Instance()->OwnerAddonId;

        m_CurrentOpenArgs = new ContextMenuOpenArgs();
        if (isInventoryContext) 
            InventoryContextMenuOpen?.Invoke(m_CurrentOpenArgs);
        else 
            ContextMenuOpen?.Invoke(m_CurrentOpenArgs);

        foreach (var item in m_CurrentOpenArgs.CustomMenuItems)
            agent->AddMenuItem($"[C] {item.Name}", m_ContextEventInterface.Pointer, item.Id, item.IsDisabled, item is CustomSubContextMenuItem);

        m_OpenContextHook.Original(agent, setowner, closeexisting);
    }

    private int* ReceiveEvent(void* self, int* a2, AtkValue* eventParams, nint a4, long param) {
        *a2 = 2;
        
        //check if a regular custom item was clicked
        var item = m_CurrentOpenArgs?.CustomMenuItems.FirstOrDefault(m => m.Id == param);
        if (item == null) {
            //check if a submenu custom item was clicked
            item = m_CurrentSubMenuOpenArgs?.CustomMenuItems.FirstOrDefault(m => m.Id == param);
            if (item is { } subItem) {
                //if the item is inside a custom submenu call it's handler
                subItem.Handler?.Invoke();
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
        } else {
            //if it's a regular custom item just call the click handler
            item?.Handler?.Invoke();
        }

        return a2;
    }
}