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

        public float preferredWidth => _rectTransform?.rect.height ?? -1;

        public float flexibleWidth => 0f;

        public float minHeight => -1f;

        public float preferredHeight => -1f;

        public float flexibleHeight => -1f;

        public int layoutPriority => 1;
    }
}