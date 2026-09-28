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

        private Dictionary<int, Outline> _outlines = new();
        private int _outlineIdCounter;

        public IReadOnlyDictionary<int, Outline> Outlines => _outlines;

        public override void InitialiseConfig(GameManagerConfig config)
        {
            _config = config.RenderingManagerConfig;

            base.InitialiseConfig(config);
        }

        public OutlineHandle CreateOutline(IEnumerable<Renderer> renderers)
        {
            int id = _outlineIdCounter++;
            Outline outline = new Outline(renderers);

            _outlines.Add(id, outline);

            OutlineHandle handle = new OutlineHandle(id);
            return handle;
        }

        public void RemoveOutline(int id)
        {
            _outlines.Remove(id);
        }
    }
}