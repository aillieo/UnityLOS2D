using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AillieoUtils.LOS2D
{
    public class LOSManager : MonoBehaviour
    {
        private struct Pair
        {
            public LOSSource s;
            public LOSTarget t;

            internal Pair(LOSSource s, LOSTarget t)
            {
                this.s = s;
                this.t = t;
            }

            public class EqualityComparer : IEqualityComparer<Pair>
            {
                public bool Equals(Pair x, Pair y)
                {
                    return x.s == y.s && x.t == y.t;
                }

                public int GetHashCode(Pair obj)
                {
                    return obj.s.GetHashCode() ^ obj.t.GetHashCode() << 2;
                }
            }
        }

        private static LOSManager instance;

        internal static LOSManager Instance
        {
            get
            {
                EnsureInstance();
                return instance;
            }
        }

        private static void EnsureInstance()
        {
            if (instance == null)
            {
                GameObject go = new GameObject($"[{nameof(LOSManager)}]");
                instance = go.AddComponent<LOSManager>();
                GameObject.DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        //[Range(0, 180)]
        public static float defaultFOV = 90f;
        //[Range(0, 1000)]
        public static float defaultMaxDist = 10f;
        public static LayerMask defaultMaskForRender = 1;

        private static readonly HashSet<Pair> pairsInSight = new HashSet<Pair>(new Pair.EqualityComparer());
        private static readonly HashSet<LOSSource> sources = new HashSet<LOSSource>();
        private static readonly HashSet<LOSTarget> targets = new HashSet<LOSTarget>();
        private List<LOSSource> sourceBuffer = new List<LOSSource>();
        private List<LOSTarget> targetBuffer = new List<LOSTarget>();

        public static event Action<LOSSource, LOSTarget> OnEnter;

        public static event Action<LOSSource, LOSTarget> OnExit;

        public static bool IsInSight(LOSSource source, LOSTarget target)
        {
            if (source == null || target == null)
            {
                return false;
            }
            EnsureInstance();
            return pairsInSight.Contains(new Pair(source, target));
        }

        internal static bool Register(LOSSource source)
        {
            EnsureInstance();
            return sources.Add(source);
        }

        internal static bool Unregister(LOSSource source)
        {
            if (sources.Remove(source))
            {
                pairsInSight.RemoveWhere(p => p.s == source);
            }

            return false;
        }

        internal static bool Register(LOSTarget target)
        {
            EnsureInstance();
            return targets.Add(target);
        }

        internal static bool Unregister(LOSTarget target)
        {
            if (targets.Remove(target))
            {
                pairsInSight.RemoveWhere(p => p.t == target);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckAndFireEvent(LOSSource s, LOSTarget t, bool inSight)
        {
            Pair pair = new Pair(s, t);
            bool original = pairsInSight.Contains(pair);
            if (original == inSight)
            {
                return;
            }

            if (inSight)
            {
                pairsInSight.Add(pair);
                OnEnter?.Invoke(s, t);
            }
            else
            {
                pairsInSight.Remove(pair);
                OnExit?.Invoke(s, t);
            }
        }

        private void Update()
        {
            sourceBuffer.AddRange(sources);
            targetBuffer.AddRange(targets);
            foreach (var t in targetBuffer)
            {
                foreach (var s in sourceBuffer)
                {
                    bool inSight = false;
                    if (InSector(t, s))
                    {
                        Vector3 sPos = s.transform.position;
                        Vector3 tPos = t.transform.position;
                        Ray ray = new Ray(sPos, tPos - sPos);
                        if (Physics.Raycast(ray, out RaycastHit hit, s.maxDist, s.maskForEvent))
                        {
                            if (hit.transform.IsChildOf(t.transform))
                            {
                                inSight = true;
                            }
                        }
                    }

                    CheckAndFireEvent(s, t, inSight);
                }
            }

            sourceBuffer.Clear();
            targetBuffer.Clear();
        }

        private static bool InSector(LOSTarget target, LOSSource source, float heightTolerance = 1)
        {
            Vector3 point3 = target.transform.position;
            Vector3 center3 = source.transform.position;

            if (Mathf.Abs(point3.y - center3.y) > heightTolerance)
            {
                return false;
            }

            Vector2 point2 = new Vector2(point3.x, point3.z);
            Vector2 center2 = new Vector2(center3.x, center3.z);
            float radius = source.maxDist;
            if (Vector2.SqrMagnitude(point2 - center2) > radius * radius)
            {
                return false;
            }

            Vector2 dir = point2 - center2;
            Vector3 forward3 = source.transform.forward;
            Vector2 forward2 = new Vector2(forward3.x, forward3.z);
            float fov = source.fov;
            if (Vector2.Angle(dir, forward2) > fov * 0.5f)
            {
                return false;
            }

            return true;
        }
    }
}
