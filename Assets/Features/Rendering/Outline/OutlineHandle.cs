using UnityEngine;

namespace NoMoreFishAndChips.Rendering
{
    public class OutlineHandle
    {
        private RenderingManager _renderingManager;

        private int _id;

        public OutlineHandle(int id)
        {
            _renderingManager = GameManager.Instance.Get<RenderingManager>();

            _id = id;
        }

        public void Remove()
        {
            _renderingManager.RemoveOutline(_id);
        }
    }
}