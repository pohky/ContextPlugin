using Dalamud.Configuration;

namespace ContextPlugin.Plugin; 

public class PluginConfig : IPluginConfiguration {
    public int Version { get; set; } = 0;
}