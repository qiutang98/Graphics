using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Used by DBuffer to copy depth into different texture.
    /// In future this should be replaced by proper depth copy logic, as current CopyDepthPass do not implement RecordRenderGraph.
    /// </summary>
    internal class DBufferCopyDepthPass : CopyDepthPass
    {
        public DBufferCopyDepthPass(RenderPassEvent evt, Material copyDepthMaterial, bool shouldClear = false, bool copyToDepth = false, bool copyResolvedDepth = false)
            : base(evt, copyDepthMaterial, shouldClear, copyToDepth, copyResolvedDepth)
        {
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            UniversalRenderer renderer = (UniversalRenderer)cameraData.renderer;

            TextureHandle cameraDepthTexture = resourceData.cameraDepthTexture;


            TextureHandle src, dest;
            if (renderer.renderingModeActual == RenderingMode.Deferred)
            {
                src = renderer.activeDepthTexture;
                dest = cameraDepthTexture;
            }
            else
            {
                var depthDesc = cameraData.cameraTargetDescriptor;
                depthDesc.graphicsFormat = GraphicsFormat.None; //Depth only rendering
                depthDesc.depthStencilFormat = cameraData.cameraTargetDescriptor.depthStencilFormat;
                depthDesc.msaaSamples = 1;
                var depthTarget = UniversalRenderer.CreateRenderGraphTexture(renderGraph, depthDesc, DBufferRenderPass.s_DBufferDepthName, true);
                resourceData.dBufferDepth = depthTarget;

                src = cameraDepthTexture;
                dest = cameraData.cameraTargetDescriptor.msaaSamples > 1 ? depthTarget : renderer.activeDepthTexture;
            }

            //TODO: bindAsCameraDepth should be investigated as without it DBufferDepth will not be bound correctly, though it should
            Render(renderGraph, dest, src, cameraData, resourceData, cameraData.cameraTargetDescriptor.msaaSamples > 1);
        }
    }
}
