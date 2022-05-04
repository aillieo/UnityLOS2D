using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace AillieoUtils.LOS2D
{
    [DefaultExecutionOrder(-100)]
    public class RaycastTaskScheduler : MonoBehaviour
    {
        private static RaycastTaskScheduler instance;
        private static bool quitFlag = false;

        internal static RaycastTaskScheduler Instance
        {
            get
            {
                EnsureInstance();
                return instance;
            }
        }

        private static void EnsureInstance()
        {
            if (instance == null && !quitFlag)
            {
                GameObject go = new GameObject($"[{nameof(RaycastTaskScheduler)}]");
                instance = go.AddComponent<RaycastTaskScheduler>();
                GameObject.DontDestroyOnLoad(go);
            }
        }

        private void OnApplicationQuit()
        {
            quitFlag = true;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            else
            {
                instance = this;
                GameObject.DontDestroyOnLoad(gameObject);
            }

            raycastRequests = new NativeList<RaycastRequestData>(0, Allocator.Persistent);
            raycastCommands = new NativeList<RaycastCommand>(0, Allocator.Persistent);
            raycastResults = new NativeList<RaycastHit>(0, Allocator.Persistent);
        }

        private NativeList<RaycastRequestData> raycastRequests;
        private NativeList<RaycastCommand> raycastCommands;
        private NativeList<RaycastHit> raycastResults;
        private readonly Dictionary<LOSMesh, int> losToIndexMap = new Dictionary<LOSMesh, int>();
        private JobHandle handle;

        private struct RaycastRequestData
        {
            [ReadOnly]
            public float3 origin;
            [ReadOnly]
            public float3 angleStart;
            [ReadOnly]
            public float angleStep;
            [ReadOnly]
            public int total;
        }

        [BurstCompile]
        private struct RaycastRequestJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeList<RaycastRequestData> raycastRequests;
            [WriteOnly]
            // public NativeList<RaycastCommand> raycastCommands;
            public NativeArray<RaycastCommand> raycastCommands;

            public void Execute(int index)
            {
                int count = 0;
                RaycastRequestData requestData = default;
                int offset = 0;

                for (int i = 0; i < raycastRequests.Length; i++)
                {
                    requestData = raycastRequests[i];
                    if (count + index < requestData.total)
                    {
                        offset = index - count;
                        break;
                    }
                }

                raycastCommands[index] = new RaycastCommand(
                    requestData.origin,
                    requestData.angleStart + new float3(0, requestData.angleStep * offset, 0),
                    1000,
                    1,
                    1);
            }
        }

        public void Register(LOSMesh losMesh)
        {
            losToIndexMap.Add(losMesh, 0);
        }

        public void Unregister(LOSMesh losMesh)
        {
            losToIndexMap.Remove(losMesh);
        }

        public RaycastHit GetResult(LOSMesh losMesh, int offset)
        {
            if (!handle.IsCompleted)
            {
                Debug.LogError("job executing 2");
            }

            int start = losToIndexMap[losMesh];
            return raycastResults[start + offset];
        }

        private List<LOSMesh> keys = new List<LOSMesh>();

        private void Update()
        {
            raycastRequests.Clear();
            raycastCommands.Clear();
            raycastResults.Clear();

            keys.Clear();
            foreach (var pair in losToIndexMap)
            {
                keys.Add(pair.Key);
            }

            int indexStart = 0;
            foreach (var key in keys)
            {
                losToIndexMap[key] = indexStart;
                indexStart += key.resolution;
            }

            keys.Clear();

            foreach (var pair in losToIndexMap)
            {
                raycastRequests.Add(new RaycastRequestData()
                {
                    origin = pair.Key.transform.position,
                    angleStart = pair.Key.transform.forward,
                    angleStep = pair.Key.defaultFOV / pair.Key.resolution,
                    total = pair.Key.resolution,
                });
            }

            NativeArray<RaycastCommand> array = new NativeArray<RaycastCommand>(indexStart, Allocator.Temp);
            RaycastRequestJob request = new RaycastRequestJob
            {
                raycastRequests = this.raycastRequests,
                raycastCommands = array,
            };

            handle = request.Schedule(raycastCommands.Length, 5);
            handle.Complete();

            raycastCommands.Clear();
            raycastCommands.AddRange(array);

            raycastResults.Length = raycastCommands.Length;
            handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 20, handle);
            UnityEngine.Debug.Log($"ScheduleBatch {raycastCommands.Length}");
        }

        private void LateUpdate()
        {
            handle.Complete();
            UnityEngine.Debug.Log($"Complete {raycastResults.Length}");
            handle = default;
        }

        private void OnDestroy()
        {
            handle.Complete();
            handle = default;

            raycastCommands.Dispose();
            raycastResults.Dispose();
        }
    }
}
