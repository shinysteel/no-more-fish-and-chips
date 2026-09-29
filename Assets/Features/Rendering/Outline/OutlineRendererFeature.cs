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
        private const string OcclusionTextureShaderPropertyName = "_OcclusionTexture";

        private Pass _pass;

        private class Pass : ScriptableRenderPass
        {
            private IReadOnlyDictionary<int, Outline> _outlines;
            private Material _maskMaterial;
            private Material _occlusionMaterial;
            private Material _combineMaterial;
            private Material _horizontalMaterial;
            private Material _verticalMaterial;

            private const string MaskName = "Outline Mask";
            private const string OcclusionName = "Outline Occlusion";
            private const string CombineName = "Outline Combine";
            private const string HorizontalName = "Outline Horizontal";
            private const string VerticalName = "Outline Vertical";

            private class MaskData
            {
                private IReadOnlyDictionary<int, Outline> _outlines;
                private Material _material;

                public IReadOnlyDictionary<int, Outline> Outlines => _outlines;
                public Material Material => _material;

                public void Set(IReadOnlyDictionary<int, Outline> outlines, Material material)
                {
                    _outlines = outlines;
                    _material = material;
                }
            }

            private class CombineData
            {
                private TextureHandle _maskHandle;
                private Material _material;

                public TextureHandle MaskHandle => _maskHandle;
                public Material Material => _material;

                public void Set(TextureHandle maskHandle, Material material)
                {
                    _maskHandle = maskHandle;
                    _material = material;
                }
            }

            public void Set(IReadOnlyDictionary<int, Outline> outlines, Material maskMaterial, Material occlusionMaterial, Material combineMaterial, Material horizontalMaterial, Material verticalMaterial)
            {
                _outlines = outlines;
                _maskMaterial = maskMaterial;
                _occlusionMaterial = occlusionMaterial;
                _combineMaterial = combineMaterial;
                _horizontalMaterial = horizontalMaterial;
                _verticalMaterial = verticalMaterial;
            }
            
            public override void RecordRenderGraph(RenderGraph graph, ContextContainer container)
            {
                if (_outlines.Count == 0)
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
                using (IUnsafeRenderGraphBuilder maskBuilder = graph.AddUnsafePass(MaskName, out MaskData data))
                {
                    data.Set(_outlines, _maskMaterial);

                    maskBuilder.SetRenderAttachment(maskHandle, 0, AccessFlags.Write);
                    maskBuilder.SetRenderFunc<MaskData>(Mask);
                };

                TextureHandle occlusionHandle = graph.CreateTexture(new TextureDesc(descriptor)
                {
                    name = OcclusionName,
                    clearBuffer = true,
                    clearColor = Color.black
                });

                using (IUnsafeRenderGraphBuilder occlusionBuilder = graph.AddUnsafePass(OcclusionName, out MaskData data))
                {
                    data.Set(_outlines, _occlusionMaterial);

                    occlusionBuilder.SetRenderAttachment(occlusionHandle, 0, AccessFlags.Write);
                    occlusionBuilder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);

                    occlusionBuilder.SetGlobalTextureAfterPass(occlusionHandle, Shader.PropertyToID(OcclusionTextureShaderPropertyName));

                    occlusionBuilder.SetRenderFunc<MaskData>(Mask);
                }

                TextureHandle combineHandle = graph.CreateTexture(new TextureDesc(descriptor)
                {
                    name = CombineName,
                    clearBuffer = true,
                    clearColor = Color.black
                });

                using (IRasterRenderGraphBuilder combineBuilder = graph.AddRasterRenderPass(CombineName, out CombineData data))
                {
                    data.Set(maskHandle, _combineMaterial);

                    combineBuilder.UseTexture(maskHandle, AccessFlags.Read);
                    combineBuilder.UseGlobalTexture(Shader.PropertyToID(OcclusionTextureShaderPropertyName), AccessFlags.Read);

                    combineBuilder.SetRenderAttachment(combineHandle, 0, AccessFlags.Write);

                    combineBuilder.SetRenderFunc<CombineData>(Combine);
                }

                TextureHandle horizontalHandle = graph.CreateTexture(new TextureDesc(descriptor)
                {
                    name = HorizontalName
                });

                RenderGraphUtils.BlitMaterialParameters horizontalParameters = new RenderGraphUtils.BlitMaterialParameters(combineHandle, horizontalHandle, _horizontalMaterial, 0);
                graph.AddBlitPass(horizontalParameters, HorizontalName);

                RenderGraphUtils.BlitMaterialParameters verticalParameters = new RenderGraphUtils.BlitMaterialParameters(horizontalHandle, resourceData.activeColorTexture, _verticalMaterial, 0);
                graph.AddBlitPass(verticalParameters, VerticalName);
            }

            private void Mask(MaskData data, UnsafeGraphContext context)
            {
                CommandBuffer buffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                foreach (Outline outline in data.Outlines.Values)
                {
                    foreach (Renderer renderer in outline.Renderers)
                    {
                        buffer.DrawRenderer(renderer, data.Material);
                    }
                }
            }

            private void Combine(CombineData data, RasterGraphContext context)
            {
                Blitter.BlitTexture(context.cmd, data.MaskHandle, Vector4.one, data.Material, 0);
            }
        }

        public override void Create()
        {   
            _pass = new Pass();
            _pass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            RenderingManager renderingManager = GameManager.Instance.Get<RenderingManager>();
            
            _pass.Set(renderingManager.Outlines, renderingManager.Config.OutlineConfig.MaskMaterial, renderingManager.Config.OutlineConfig.OcclusionMaterial, 
                renderingManager.Config.OutlineConfig.CombineMaterial, renderingManager.Config.OutlineConfig.HorizontalMaterial, renderingManager.Config.OutlineConfig.VerticalMaterial);

            renderer.EnqueuePass(_pass);
        }
    }
}