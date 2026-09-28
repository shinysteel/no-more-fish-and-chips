using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace NoMoreFishAndChips.Rendering
{
    public class OutlineRendererFeature : ScriptableRendererFeature
    {
        private Pass _pass;

        private class Pass : ScriptableRenderPass
        {
            private IReadOnlyList<Renderer> _renderers;
            private Material _maskMaterial;
            private Material _outlineMaterial;

            private const string MaskName = "Outline Mask";
            private const string OutlineName = "Outline";

            private class MaskData
            {
                private IReadOnlyList<Renderer> _renderers;
                private Material _material;

                public IReadOnlyList<Renderer> Renderers => _renderers;
                public Material Material => _material;

                public void Set(IReadOnlyList<Renderer> renderers, Material material)
                {
                    _renderers = renderers;
                    _material = material;
                }
            }

            private class OutlineData
            {
                private TextureHandle _handle;
                private Material _material;

                public TextureHandle Handle => _handle;
                public Material Material => _material;

                public void Set(TextureHandle handle, Material material)
                {
                    _handle = handle;
                    _material = material;
                }
            }

            public void Set(IReadOnlyList<Renderer> renderers, Material maskMaterial, Material outlineMaterial)
            {
                _renderers = renderers;
                _maskMaterial = maskMaterial;
                _outlineMaterial = outlineMaterial;
            }
            
            public override void RecordRenderGraph(RenderGraph graph, ContextContainer container)
            {
                if (_renderers.Count == 0)
                {
                    return;
                }

                UniversalCameraData cameraData = container.Get<UniversalCameraData>();
                UniversalResourceData resourceData = container.Get<UniversalResourceData>();

                RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;

                TextureHandle handle = graph.CreateTexture(new TextureDesc(descriptor)
                {
                    name = MaskName,
                    clearBuffer = true,
                    clearColor = Color.black
                });

                // Using declaration to use then dispose IDisposable variables once scope ends
                using (IUnsafeRenderGraphBuilder maskBuilder = graph.AddUnsafePass(MaskName, out MaskData maskData))
                {
                    maskData.Set(_renderers, _maskMaterial);

                    maskBuilder.SetRenderAttachment(handle, 0, AccessFlags.Write);
                    maskBuilder.SetRenderFunc<MaskData>(Mask);
                };

                RenderGraphUtils.BlitMaterialParameters parameters = new RenderGraphUtils.BlitMaterialParameters(handle, resourceData.activeColorTexture, _outlineMaterial, 0);
                graph.AddBlitPass(parameters, OutlineName);
            }

            private void Mask(MaskData data, UnsafeGraphContext context)
            {
                CommandBuffer buffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                foreach (Renderer renderer in data.Renderers)
                {
                    buffer.DrawRenderer(renderer, data.Material);
                }
            }
        }

        public override void Create()
        {   
            _pass = new Pass();
            _pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            RenderingManager renderingManager = GameManager.Instance.Get<RenderingManager>();
            
            _pass.Set(renderingManager.OutlineRenderers, renderingManager.Config.OutlineMaskMaterial, renderingManager.Config.OutlineMaterial);

            renderer.EnqueuePass(_pass);
        }
    }
}