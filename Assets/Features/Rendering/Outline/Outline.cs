using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace NoMoreFishAndChips.Rendering
{
    public class Outline
    {
        private IEnumerable<Renderer> _renderers;

        public IEnumerable<Renderer> Renderers => _renderers;

        public Outline(IEnumerable<Renderer> renderers)
        {
            _renderers = renderers;
        }
    }
}