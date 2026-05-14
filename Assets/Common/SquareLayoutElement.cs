using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ShinyOwl.Common
{
    public class SquareLayoutElement : MonoBehaviour, ILayoutElement
    {
        [SerializeField] private RectTransform _rectTransform;

        private void OnEnable()
        {
            _ = RebuildAsync();
        }

        // Without a rebuild, this element's width will be 0
        private async Task RebuildAsync()
        {
            await Task.Yield();
            await Task.Yield();

            if (isActiveAndEnabled)
            {
                LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
            }
        }

        public void CalculateLayoutInputHorizontal()
        { }

        public void CalculateLayoutInputVertical()
        { }

        public float minWidth => -1;

        public float preferredWidth => _rectTransform?.rect.height ?? -1;

        public float flexibleWidth => 0f;

        public float minHeight => -1f;

        public float preferredHeight => -1f;

        public float flexibleHeight => -1f;

        public int layoutPriority => 1;
    }
}