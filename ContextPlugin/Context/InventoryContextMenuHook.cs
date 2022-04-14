using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ContextPlugin.Context; 

internal unsafe class InventoryContextMenuHook : IDisposable {
    private delegate void OpenInventoryContextDelegate(AgentInventoryContext* agent, uint inventoryId, int slot, int a4, uint addonId);

    private readonly CustomAtkEventInterface m_InventoryEventInterface;
    private readonly Hook<OpenInventoryContextDelegate> m_OpenInventoryContextHook;

    public InventoryContextMenuHook() {
        m_InventoryEventInterface = new CustomAtkEventInterface(InventoryReceiveEvent);
        m_OpenInventoryContextHook = new Hook<OpenInventoryContextDelegate>((nint)AgentInventoryContext.fpOpenForItemSlot, OpenInventoryContextDetour);
        m_OpenInventoryContextHook.Enable();
    }

    public void Dispose() {
        m_OpenInventoryContextHook.Dispose();
        m_InventoryEventInterface.Dispose();
    }

    private void OpenInventoryContextDetour(AgentInventoryContext* agent, uint inventoryId, int slot, int a4, uint addonId) {
        m_OpenInventoryContextHook.Original(agent, inventoryId, slot, a4, addonId);

        var ctx = AgentContext.Instance();
        ctx->ClearMenu();

        // clone AgentInventoryContext items with their original index as param
        for (var i = agent->ContexItemStartIndex; i < agent->ContexItemStartIndex + agent->ContextItemCount; i++) {
            var param = agent->EventParamsSpan[i];
            ctx->AddMenuItem(param.String, m_InventoryEventInterface.Pointer, i - agent->ContexItemStartIndex);
        }

        //update owner id and open regular context menu
        agent->OwnerAddonId = addonId;
        ctx->OpenContextMenuForAddon(addonId);
    }

    private int* InventoryReceiveEvent(void* self, int* a2, AtkValue* eventparams, nint a4, long param) {
        // pass inventorycontext events to the the original handler with the original index
        var agent = AgentInventoryContext.Instance();
        var handler = (delegate* unmanaged<void*, int*, void*, nint, long, int*>)agent->AgentInterface.AtkEventInterface.vtbl[0];
        eventparams[1].Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int;
        eventparams[1].Int = (int)param;
        return handler(agent, a2, eventparams, a4, 66);
    }
}