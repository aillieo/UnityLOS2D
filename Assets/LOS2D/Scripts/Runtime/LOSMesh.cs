using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils.LOS2D
{
    // [RequireComponent(typeof(MeshFilter))]
    public class LOSMesh : MonoBehaviour
    {
        public LOSSource associatedLOSSource;

        [Range(1, 1024)]
        public int resolution = 40;

        [SerializeField]
        private MeshFilter meshComp;

        public bool autoRegenerateMesh = true;

        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();

        private void Start()
        {
            if (meshComp == null)
            {
                meshComp = GetComponent<MeshFilter>();
                if (meshComp == null)
                {
                    meshComp = gameObject.AddComponent<MeshFilter>();
                }
            }

            if (associatedLOSSource == null)
            {
                associatedLOSSource = gameObject.GetComponentInParent<LOSSource>();
            }
        }

        private void Update()
        {
            if (!autoRegenerateMesh)
            {
                return;
            }

            RegenerateMesh();
        }

        [ContextMenu("RegenerateMesh")]
        public void RegenerateMesh()
        {
            if (meshComp == null)
            {
                return;
            }

            GenVerts(vertices);

            GenMesh(vertices);
        }

        private void GenVerts(List<Vector3> verts)
        {
            float fov;
            float maxDist;
            if (associatedLOSSource != null)
            {
                fov = associatedLOSSource.fov;
                maxDist = associatedLOSSource.maxDist;
            }
            else
            {
                fov = LOSManager.defaultFOV;
                maxDist = LOSManager.defaultMaxDist;
            }

            float halfFov = fov * 0.5f;
            float rotY = transform.eulerAngles.y;
            float angleStart = (rotY - halfFov) * Mathf.Deg2Rad;
            float angleEnd = (rotY + halfFov) * Mathf.Deg2Rad;

            float step = (angleEnd - angleStart) / resolution;

            verts.Clear();
            verts.Add(new Vector3(0, 0, 0));

            float angle = angleStart;
            for (int i = 0; i <= resolution; ++i)
            {
                var res = Cast(angle, maxDist);

                Vector3 hitPoint = res.point;
                hitPoint = transform.InverseTransformPoint(hitPoint);
                verts.Add(hitPoint);

                angle += step;
            }
        }

        private void GenMesh(List<Vector3> verts)
        {
            triangles.Clear();
            for (int i = 1; i + 1 < verts.Count; i ++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            Mesh losMesh;
            if (Application.isEditor && !Application.isPlaying)
            {
                losMesh = meshComp.sharedMesh;
                if (losMesh == null)
                {
                    losMesh = new Mesh();
                    meshComp.sharedMesh = losMesh;
                }
            }
            else
            {
                losMesh = meshComp.mesh;
            }

            losMesh.SetVertices(verts);
            losMesh.SetTriangles(triangles, 0);
        }

        private RaycastHit Cast(float angle, float maxDist)
        {
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            Ray ray = new Ray(transform.position, new Vector3(x, 0, y));
            if (!Physics.Raycast(ray, out RaycastHit hit, maxDist, 1 << LayerMask.NameToLayer("Default")))
            {
                hit.distance = maxDist;
                hit.point = ray.GetPoint(maxDist);
            }

            return hit;
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < vertices.Count; ++i)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, (float)i / vertices.Count);
                Gizmos.DrawWireCube(
                    transform.TransformPoint(vertices[i]),
                    Vector3.one * 0.1f);
            }
        }
    }
}
