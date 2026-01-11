using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShinyOwl.Common
{
    public class SquareLayoutElement : MonoBehaviour, ILayoutElement
    {
        [SerializeField] private RectTransform _rectTransform;
        
        public void CalculateLayoutInputHorizontal()
        { }

        public void CalculateLayoutInputVertical()
        { }

        public float minWidth => -1;
        public float preferredWidth
        {
            get
            {
                float height = LayoutUtility.GetPreferredHeight(_rectTransform);

                if (height <= 0)
                {
                    height = _rectTransform.rect.height;
                }

                return height;
            }
        }

        public float flexibleWidth => 0;

        public float minHeight => -1;

        public float preferredHeight => -1;

        public float flexibleHeight => -1;

        public int layoutPriority => 1;
    }
}