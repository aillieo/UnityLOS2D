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
        public bool fillUV = false;

        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles1 = new List<int>();
        private List<int> triangles2 = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();

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

            GenVerts();

            GenMesh();
        }

        private void GenVerts()
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

            vertices.Clear();

            if (fillUV)
            {
                uvs.Clear();
            }
            else
            {
                vertices.Add(new Vector3(0, 0, 0));
            }

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
                    if (fillUV)
                    {
                        vertices.Add(new Vector3(0, 0, 0));
                    }

                    vertices.Add(endPointLocal);

                    if (fillUV)
                    {
                        float theta = (float)i / resolution;
                        uvs.Add(new Vector2(0, theta));
                        uvs.Add(new Vector2(1, theta));
                    }
                }
                else
                {
                    bool hit = Cast(dir, maxDist, mask, out Vector3 point1, out Vector3 point2, out float dist);
                    point1 = transform.InverseTransformPoint(point1);
                    if (hit)
                    {
                        point2 = transform.InverseTransformPoint(point2);
                    }
                    else
                    {
                        point2 = point1;
                    }

                    if (fillUV)
                    {
                        vertices.Add(new Vector3(0, 0, 0));
                    }

                    vertices.Add(point1);
                    vertices.Add(point2);

                    if (fillUV)
                    {
                        float theta = (float)i / resolution;
                        uvs.Add(new Vector2(0, theta));
                        uvs.Add(new Vector2(dist / maxDist, theta));
                        uvs.Add(new Vector2(1, theta));
                    }
                }

                angle += step;
            }
        }

        private void GenMesh()
        {
            // 内侧mesh
            triangles1.Clear();
            triangles2.Clear();

            if (drawSimpleSector)
            {
                if (fillUV)
                {
                    for (int i = 0; i + 3 < vertices.Count; i += 2)
                    {
                        triangles1.Add(i);
                        triangles1.Add(i + 1);
                        triangles1.Add(i + 3);
                    }
                }
                else
                {
                    for (int i = 1; i + 1 < vertices.Count; i++)
                    {
                        triangles1.Add(0);
                        triangles1.Add(i);
                        triangles1.Add(i + 1);
                    }
                }
            }
            else
            {
                if (drawSight)
                {
                    if (fillUV)
                    {
                        for (int i = 0; i + 4 < vertices.Count; i += 3)
                        {
                            triangles1.Add(i);
                            triangles1.Add(i + 1);
                            triangles1.Add(i + 4);
                        }
                    }
                    else
                    {
                        for (int i = 1; i + 2 < vertices.Count; i += 2)
                        {
                            triangles1.Add(0);
                            triangles1.Add(i);
                            triangles1.Add(i + 2);
                        }
                    }
                }

                if (drawHidden)
                {
                    if (fillUV)
                    {
                        for (int i = 0; i + 3 < vertices.Count; i += 3)
                        {
                            triangles2.Add(i + 1);
                            triangles2.Add(i + 2);
                            triangles2.Add(i + 4);

                            triangles2.Add(i + 2);
                            triangles2.Add(i + 5);
                            triangles2.Add(i + 4);
                        }
                    }
                    else
                    {
                        for (int i = 1; i + 2 < vertices.Count; i += 2)
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
                losMesh.SetVertices(vertices);
                losMesh.SetTriangles(triangles1, 0);
                if (fillUV)
                {
                    losMesh.SetUVs(0, uvs);
                }
            }
            else
            {
                losMesh.subMeshCount = 2;
                losMesh.SetVertices(vertices);
                losMesh.SetTriangles(triangles1, 0);
                losMesh.SetTriangles(triangles2, 1);
                if (fillUV)
                {
                    losMesh.SetUVs(0, uvs);
                }
            }
        }

        private bool Cast(Vector3 direction, float maxDist, LayerMask mask, out Vector3 point1, out Vector3 point2, out float dist)
        {
            Ray ray = new Ray(transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDist, mask))
            {
                point1 = hit.point;
                point2 = ray.GetPoint(maxDist);
                dist = hit.distance;
                return true;
            }
            else
            {
                point1 = ray.GetPoint(maxDist);
                point2 = point1;
                dist = maxDist;
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
