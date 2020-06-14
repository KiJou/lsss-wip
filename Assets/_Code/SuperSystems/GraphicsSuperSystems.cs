﻿using Latios;
using Unity.Transforms;

namespace Lsss.SuperSystems
{
    /// <summary>
    /// Updates transforms of graphics-exclusive items (Cameras, billboards, ect)
    /// </summary>
    public class GraphicsTransformsSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddSystem<CameraFollowPlayerSystem>();
            GetOrCreateAndAddSystem<FaceCameraSystem>();
            GetOrCreateAndAddSystem<LifetimeFadeSystem>();
            GetOrCreateAndAddSystem<SpawnPointAnimationSystem>();

            //Debug until playerloop fixed so camera doesn't jitter
            //GetOrCreateAndAddSystem<TransformSystemGroup>();
            //GetOrCreateAndAddSystem<CompanionGameObjectUpdateTransformSystem>();  //Todo: Namespace
        }
    }
}
