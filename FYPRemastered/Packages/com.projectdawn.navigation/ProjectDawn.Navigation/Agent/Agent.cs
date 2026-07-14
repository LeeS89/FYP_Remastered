using Unity.Entities;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Agent tag.
    /// </summary>
    public struct Agent : IComponentData, IEnableableComponent
    {
        public NavigationLayers Layers;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static Agent Default => new() { Layers = NavigationLayers.Default };
    }
}
