﻿using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.SceneManagement;

namespace Latios.Systems
{
    [AlwaysUpdateSystem]
    public class SceneManagerSystem : SubSystem
    {
        private EntityQuery m_rlsQuery;

        protected override void OnCreate()
        {
            CurrentScene curr = new CurrentScene
            {
                currentScene      = new FixedString128(),
                previousScene     = new FixedString128(),
                isSceneFirstFrame = false
            };
            worldGlobalEntity.AddOrSetComponentData(curr);

            m_rlsQuery = GetEntityQuery(typeof(RequestLoadScene));
        }

        protected override void OnUpdate()
        {
            if (m_rlsQuery.CalculateChunkCount() > 0)
            {
                FixedString128 targetScene = new FixedString128();
                bool           isInvalid   = false;

                Entities.ForEach((ref RequestLoadScene rls) =>
                {
                    if (rls.newScene.UTF8LengthInBytes == 0)
                        return;
                    if (targetScene.Length == 0)
                        targetScene = rls.newScene;
                    else if (rls.newScene != targetScene)
                        isInvalid = true;
                }).Run();

                if (targetScene.Length > 0)
                {
                    if (isInvalid)
                    {
                        UnityEngine.Debug.LogError("Multiple scenes were requested to load during the previous frame.");
                    }
                    else
                    {
                        var curr           = worldGlobalEntity.GetComponentData<CurrentScene>();
                        curr.previousScene = curr.currentScene;
                        UnityEngine.Debug.Log("Loading scene: " + targetScene);
                        SceneManager.LoadScene(targetScene.ToString());
                        latiosWorld.pauseForSceneLoad = true;
                        curr.currentScene             = targetScene;
                        curr.isSceneFirstFrame        = true;
                        worldGlobalEntity.SetComponentData(curr);
                        return;
                    }
                }
            }

            //Handle case where initial scene loads or set firstFrame to false
            var currentScene = worldGlobalEntity.GetComponentData<CurrentScene>();
            if (currentScene.currentScene.UTF8LengthInBytes == 0)
            {
                currentScene.currentScene      = SceneManager.GetActiveScene().name;
                currentScene.isSceneFirstFrame = true;
            }
            else
                currentScene.isSceneFirstFrame = false;
            worldGlobalEntity.SetComponentData(currentScene);
        }
    }
}

