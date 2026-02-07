using UnityEngine;
using UnityEngine.UI;

namespace ShinyOwl.Common
{
    /// <summary>
    /// Dummy graphic script that allows raycasts to hit a ui without it having to
    /// generate any vertices
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class InvisibleRaycastTarget : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}