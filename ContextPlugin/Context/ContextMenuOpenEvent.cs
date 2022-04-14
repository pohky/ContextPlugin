using System;
using System.Collections.Generic;

namespace ContextPlugin.Context;

public delegate void ContextMenuOpenEventDelegate(ContextMenuOpenArgs args);

public class ContextMenuOpenArgs {
    private long m_InternalMenuItemId;

    public readonly List<CustomContextMenuItem> CustomMenuItems = new();

    internal ContextMenuOpenArgs(long baseId = 1000) {
        m_InternalMenuItemId = baseId;
    }

    public void AddItem(string name, Action handler, bool disabled = false) {
        var item = CustomContextMenuItem.Create(name, m_InternalMenuItemId++, handler, disabled);
        CustomMenuItems.Add(item);
    }

    public void InsertItem(int index, string name, Action handler, bool disabled = false) {
        var item = CustomContextMenuItem.Create(name, m_InternalMenuItemId++, handler, disabled);
        CustomMenuItems.Insert(index, item);
    }

    public void AddSubMenu(string name, Action<SubContextMenuOpenArgs> handler, bool disabled = false) {
        var item = CustomSubContextMenuItem.Create(name, m_InternalMenuItemId++, handler, disabled);
        CustomMenuItems.Add(item);
    }

    public void InsertSubMenu(int index, string name, Action<SubContextMenuOpenArgs> handler, bool disabled = false) {
        var item = CustomSubContextMenuItem.Create(name, m_InternalMenuItemId++, handler, disabled);
        CustomMenuItems.Insert(index, item);
    }
}

public class SubContextMenuOpenArgs {
    private long m_InternalMenuItemId;

    public readonly List<CustomContextMenuItem> CustomMenuItems = new();

    internal SubContextMenuOpenArgs(long baseId = 2000) {
        m_InternalMenuItemId = baseId;
    }

    public void AddItem(string name, Action handler, bool disabled = false) {
        var item = CustomContextMenuItem.Create(name, m_InternalMenuItemId++, handler, disabled);
        CustomMenuItems.Add(item);
    }

    public void InsertItem(int index, string name, Action handler, bool disabled = false) {
        var item = CustomContextMenuItem.Create(name, m_InternalMenuItemId++, handler, disabled);
        CustomMenuItems.Insert(index, item);
    }
}

public class CustomContextMenuItem {
    public string? Name { get; protected init; }
    public bool IsDisabled { get; protected init; }

    public long Id { get; protected init; }
    internal Action? Handler { get; private init; }

    protected CustomContextMenuItem() { }

    internal static CustomContextMenuItem Create(string name, long id, Action handler, bool disabled = false) {
        return new CustomContextMenuItem {
            Name = name,
            Id = id,
            Handler = handler,
            IsDisabled = disabled
        };
    }
}

public class CustomSubContextMenuItem : CustomContextMenuItem {
    internal Action<SubContextMenuOpenArgs>? SubMenuHandler { get; private init; }

    private CustomSubContextMenuItem() { }

    internal static CustomSubContextMenuItem Create(string name, long id, Action<SubContextMenuOpenArgs> handler, bool disabled = false) {
        return new CustomSubContextMenuItem {
            Name = name,
            Id = id,
            SubMenuHandler = handler,
            IsDisabled = disabled
        };
    }
}