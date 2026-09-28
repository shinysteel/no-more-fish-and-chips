using UnityEngine;
using System.Collections.Generic;

namespace NoMoreFishAndChips.Rendering
{
    public interface IRenderingManagerListener
    { }

    public class RenderingManager : GameSystem<IRenderingManagerListener>
    {
        private RenderingManagerConfig _config;
        public RenderingManagerConfig Config => _config;

        private List<Renderer> _outlineRenderers = new();
        public IReadOnlyList<Renderer> OutlineRenderers => _outlineRenderers;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _config = config.RenderingManagerConfig;

            base.InitialiseConfig(config);
        }

        public void AddOutline(Renderer renderer)
        {
            _outlineRenderers.Add(renderer);
        }

        public void RemoveOutline(Renderer renderer)
        {
            _outlineRenderers.Remove(renderer);
        }
    }
}