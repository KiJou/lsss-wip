﻿using Latios;
using Latios.PhysicsEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Lsss
{
    public class PopulateOrbitalSpawnersSystem : SubSystem
    {
        struct NewSpawnerTag : IComponentData { }

        EntityQuery m_populatorQuery;
        EntityQuery m_newSpawnerQuery;

        protected override void OnUpdate()
        {
            float arenaRadius = sceneGlobalEntity.GetComponentData<ArenaRadius>().radius;

            Entities.WithStoreEntityQueryInField(ref m_populatorQuery).ForEach((ref OrbitalSpawnPointPopulator populator) =>
            {
                Collider collider = new SphereCollider(0f, populator.colliderRadius);

                var entity = EntityManager.CreateEntity();
                EntityManager.AddComponent<Translation>( entity);
                EntityManager.AddComponent<Rotation>(    entity);
                EntityManager.AddComponent<LocalToWorld>(entity);
                EntityManager.AddComponentData(entity, collider);
                EntityManager.AddComponentData(entity, new SpawnPoint
                {
                    spawnGraphicPrefab = populator.spawnGraphicPrefab,
                    maxTimeUntilSpawn  = populator.maxTimeUntilSpawn,
                    maxPauseTime       = populator.maxPauseTime
                });
                EntityManager.AddComponentData(entity, new SpawnPayload { disabledShip = Entity.Null });
                EntityManager.AddComponent<SpawnPointOrbitalPath>(entity);
                EntityManager.AddComponent<SafeToSpawn>(          entity);
                EntityManager.AddComponent<SpawnTimes>(           entity);
                EntityManager.AddComponent<SpawnPointTag>(        entity);
                EntityManager.AddComponent<NewSpawnerTag>(        entity);

                EntityManager.Instantiate(entity, populator.spawnerCount - 1, Allocator.Temp);
                Randomize(populator, arenaRadius);
                EntityManager.RemoveComponent<NewSpawnerTag>(m_newSpawnerQuery);
            }).WithStructuralChanges().Run();
            EntityManager.DestroyEntity(m_populatorQuery);
        }

        void Randomize(OrbitalSpawnPointPopulator populator, float radius)
        {
            Random random = new Random(populator.randomSeed);

            Entities.WithAll<NewSpawnerTag>().ForEach((ref Translation trans, ref Rotation rot, ref SpawnPointOrbitalPath path) =>
            {
                float orbitalRadius   = random.NextFloat(0f, 1f);
                path.radius           = orbitalRadius * orbitalRadius * (radius - populator.colliderRadius);
                path.center           = random.NextFloat3(0f, radius - orbitalRadius - populator.colliderRadius);
                path.orbitPlaneNormal = random.NextFloat3Direction();
                path.orbitSpeed       = random.NextFloat(populator.minMaxOrbitSpeed.x, populator.minMaxOrbitSpeed.y);

                rot.Value = quaternion.identity;

                trans.Value = math.forward(quaternion.AxisAngle(path.orbitPlaneNormal, random.NextFloat(-math.PI, math.PI))) * path.radius;
            }).WithStoreEntityQueryInField(ref m_newSpawnerQuery).Run();
        }
    }
}
