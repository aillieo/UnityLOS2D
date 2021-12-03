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
        [Range(0, 180)]
        public float defaultFOV = 90f;
        [Range(0, 1000)]
        public float defaultMaxDist = 10f;

        [SerializeField]
        private MeshFilter meshComp;

        public bool autoRegenerateMesh = true;
        public bool drawHidden = true;
        public bool drawSight = true;
        public bool drawSimpleSector = false;

        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles1 = new List<int>();
        private List<int> triangles2 = new List<int>();

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
            if (!drawSimpleSector && !drawSight && !drawHidden)
            {
                return;
            }

            float fov;
            float maxDist;
            LayerMask mask;
            if (associatedLOSSource != null)
            {
                fov = associatedLOSSource.fov;
                maxDist = associatedLOSSource.maxDist;
                mask = associatedLOSSource.maskForRender;
            }
            else
            {
                fov = defaultFOV;
                maxDist = defaultMaxDist;
                mask = LOSManager.defaultMaskForRender;
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
                float x = Mathf.Sin(angle);
                float y = Mathf.Cos(angle);
                Vector3 dir = new Vector3(x, 0, y);

                if (drawSimpleSector)
                {
                    Vector3 endPoint = transform.position + dir * maxDist;
                    Vector3 endPointLocal = transform.InverseTransformPoint(endPoint);
                    verts.Add(endPointLocal);
                }
                else
                {
                    bool hit = Cast(dir, maxDist, mask, out Vector3 point1, out Vector3 point2);
                    point1 = transform.InverseTransformPoint(point1);
                    if (hit)
                    {
                        point2 = transform.InverseTransformPoint(point2);
                    }
                    else
                    {
                        point2 = point1;
                    }

                    verts.Add(point1);
                    verts.Add(point2);
                }

                angle += step;
            }
        }

        private void GenMesh(List<Vector3> verts)
        {
            // 内侧mesh
            triangles1.Clear();
            triangles2.Clear();

            if (drawSimpleSector)
            {
                for (int i = 1; i + 1 < verts.Count; i ++)
                {
                    triangles1.Add(0);
                    triangles1.Add(i);
                    triangles1.Add(i + 1);
                }
            }
            else
            {
                if (drawSight)
                {
                    for (int i = 1; i + 2 < verts.Count; i += 2)
                    {
                        triangles1.Add(0);
                        triangles1.Add(i);
                        triangles1.Add(i + 2);
                    }
                }

                if (drawHidden)
                {
                    for (int i = 1; i + 2 < verts.Count; i += 2)
                    {
                        triangles2.Add(i);
                        triangles2.Add(i + 1);
                        triangles2.Add(i + 2);

                        triangles2.Add(i + 1);
                        triangles2.Add(i + 3);
                        triangles2.Add(i + 2);
                    }
                }
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

            losMesh.Clear();
            if (drawSimpleSector || !drawHidden)
            {
                losMesh.subMeshCount = 1;
                losMesh.SetVertices(verts);
                losMesh.SetTriangles(triangles1, 0);
            }
            else
            {
                losMesh.subMeshCount = 2;
                losMesh.SetVertices(verts);
                losMesh.SetTriangles(triangles1, 0);
                losMesh.SetTriangles(triangles2, 1);
            }
        }

        private bool Cast(Vector3 direction, float maxDist, LayerMask mask, out Vector3 point1, out Vector3 point2)
        {
            Ray ray = new Ray(transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDist, mask))
            {
                point1 = hit.point;
                point2 = ray.GetPoint(maxDist);
                return true;
            }
            else
            {
                point1 = ray.GetPoint(maxDist);
                point2 = point1;
                return false;
            }
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
