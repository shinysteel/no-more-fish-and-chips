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
            private Material _horizontalMaterial;
            private Material _verticalMaterial;

            private const string MaskName = "Outline Mask";
            private const string HorizontalName = "Outline Horizontal";
            private const string VerticalName = "Outline Vertical";

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

            public void Set(IReadOnlyList<Renderer> renderers, Material maskMaterial, Material horizontalMaterial, Material verticalMaterial)
            {
                _renderers = renderers;
                _maskMaterial = maskMaterial;
                _horizontalMaterial = horizontalMaterial;
                _verticalMaterial = verticalMaterial;
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

                TextureHandle maskHandle = graph.CreateTexture(new TextureDesc(descriptor)
                {
                    name = MaskName,
                    clearBuffer = true,
                    clearColor = Color.black
                });

                // Using declaration to use then dispose IDisposable variables once scope ends
                using (IUnsafeRenderGraphBuilder maskBuilder = graph.AddUnsafePass(MaskName, out MaskData maskData))
                {
                    maskData.Set(_renderers, _maskMaterial);

                    maskBuilder.SetRenderAttachment(maskHandle, 0, AccessFlags.Write);
                    maskBuilder.SetRenderFunc<MaskData>(Mask);
                };

                TextureHandle horizontalHandle = graph.CreateTexture(new TextureDesc(descriptor)
                {
                    name = HorizontalName
                });

                RenderGraphUtils.BlitMaterialParameters horizontalParameters = new RenderGraphUtils.BlitMaterialParameters(maskHandle, horizontalHandle, _horizontalMaterial, 0);
                graph.AddBlitPass(horizontalParameters, HorizontalName);

                RenderGraphUtils.BlitMaterialParameters verticalParameters = new RenderGraphUtils.BlitMaterialParameters(horizontalHandle, resourceData.activeColorTexture, _verticalMaterial, 0);
                graph.AddBlitPass(verticalParameters, VerticalName);
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
            
            _pass.Set(renderingManager.OutlineRenderers, renderingManager.Config.OutlineMaskMaterial, renderingManager.Config.OutlineHorizontalMaterial, renderingManager.Config.OutlineVerticalMaterial);

            renderer.EnqueuePass(_pass);
        }
    }
}