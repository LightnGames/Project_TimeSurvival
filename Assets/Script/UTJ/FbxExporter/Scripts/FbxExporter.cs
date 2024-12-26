using System;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.FbxExporter
{
    public partial class FbxExporter
    {
        ExportOptions m_opt = ExportOptions.defaultValue;
        Context m_ctx;
        Dictionary<Transform, Node> m_nodes;

        public FbxExporter(ExportOptions opt)
        {
            m_opt = opt;
        }

        ~FbxExporter()
        {
            Release();
        }

        public void Release()
        {
            fbxeReleaseContext(m_ctx);
            m_ctx = Context.Null;
        }

        public bool CreateScene(string name)
        {
            Release();
            if (!m_ctx)
                m_ctx = fbxeCreateContext(ref m_opt);
            m_nodes = new Dictionary<Transform, Node>();
            return fbxeCreateScene(m_ctx, name);
        }

        public void AddNode(GameObject go)
        {
            if (go)
                FindOrCreateNodeTree(go.GetComponent<Transform>(), ProcessNode);
        }

        public bool WriteAsync(string path, Format format)
        {
            return fbxeWriteAsync(m_ctx, path, format);
        }

        public bool IsFinished()
        {
            return fbxeIsFinished(m_ctx);
        }


        #region impl
        void ProcessNode(Transform trans, Node node)
        {
            var mr = trans.GetComponent<MeshRenderer>();
            var smr = trans.GetComponent<SkinnedMeshRenderer>();
            var terrain = trans.GetComponent<Terrain>();

            if (terrain)
                AddTerrain(node, terrain);
            else if (smr)
                AddSkinnedMesh(node, smr);
            else if (mr)
                AddMesh(node, mr);
        }

        Node FindOrCreateNodeTree(Transform trans, Action<Transform, Node> act)
        {
            if (!trans) { return Node.Null; }

            if (m_nodes.ContainsKey(trans))
            {
                return m_nodes[trans];
            }
            else
            {
                var parent = !trans.parent ? fbxeGetRootNode(m_ctx) : FindOrCreateNodeTree(trans.parent, act);
                var node = fbxeCreateNode(m_ctx, parent, trans.name);
                if (m_opt.transform)
                    fbxeSetTRS(m_ctx, node, trans.localPosition, trans.localRotation, trans.localScale);
                m_nodes.Add(trans, node);

                if (act != null) { act.Invoke(trans, node); }
                return node;
            }
        }

        bool AddMesh(Node node, Mesh mesh)
        {
            if (!mesh || mesh.vertexCount == 0) { return false; }
            if (!mesh.isReadable)
            {
                Debug.LogWarning("Mesh " + mesh.name + " is not readable and be ignored.");
                return false;
            }

            Topology topology = Topology.Triangles;

            var indices = new PinnedArray<int>(mesh.triangles);
            var points = new PinnedArray<Vector3>(mesh.vertices);
            var normals = new PinnedArray<Vector3>(mesh.normals); if (normals.Length == 0) normals = null;
            var tangents = new PinnedArray<Vector4>(mesh.tangents); if (tangents.Length == 0) tangents = null;
            var uv = new PinnedArray<Vector2>(mesh.uv); if (uv.Length == 0) uv = null;
            var colors = new PinnedArray<Color>(mesh.colors); if (colors.Length == 0) colors = null;
            fbxeAddMesh(m_ctx, node, points.Length, points, normals, tangents, uv, colors);
            fbxeAddMeshSubmesh(m_ctx, node, topology, indices.Length, indices, -1);

            int blendshapeCount = mesh.blendShapeCount;
            if (blendshapeCount > 0)
            {
                var deltaVertices = new PinnedArray<Vector3>(mesh.vertexCount);
                var deltaNormals = new PinnedArray<Vector3>(mesh.vertexCount);
                var deltaTangents = new PinnedArray<Vector3>(mesh.vertexCount);
                for (int bi = 0; bi < blendshapeCount; ++bi)
                {
                    string name = mesh.GetBlendShapeName(bi);
                    int frameCount = mesh.GetBlendShapeFrameCount(bi);
                    for (int fi = 0; fi < frameCount; ++fi)
                    {
                        float weight = mesh.GetBlendShapeFrameWeight(bi, fi);
                        mesh.GetBlendShapeFrameVertices(bi, fi, deltaVertices, deltaNormals, deltaTangents);
                        fbxeAddMeshBlendShape(m_ctx, node, name, weight, deltaVertices, deltaNormals, deltaTangents);
                    }
                }
            }

            return true;
        }

        bool AddMesh(Node node, MeshRenderer mr)
        {
            var mf = mr.gameObject.GetComponent<MeshFilter>();
            if (!mf)
                return false;
            Mesh sourceMesh = mf.sharedMesh;
            Mesh mesh = new Mesh();
            mesh.vertices = sourceMesh.vertices;
            mesh.uv = sourceMesh.uv;
            mesh.uv2 = sourceMesh.uv2;
            mesh.uv3 = sourceMesh.uv3;
            mesh.uv4 = sourceMesh.uv4;
            mesh.triangles = sourceMesh.triangles;

            mesh.bindposes = sourceMesh.bindposes;
            mesh.boneWeights = sourceMesh.boneWeights;
            mesh.bounds = sourceMesh.bounds;
            mesh.colors = sourceMesh.colors;
            mesh.colors32 = sourceMesh.colors32;
            mesh.normals = sourceMesh.normals;
            mesh.subMeshCount = sourceMesh.subMeshCount;
            mesh.tangents = sourceMesh.tangents;

            if(mesh.uv2 != null)
            {
                mesh.uv = mesh.uv2;
                mesh.uv2 = null;
            }

            Vector2 uvScale = new Vector2(mr.lightmapScaleOffset.x, mr.lightmapScaleOffset.y);
            Vector2 uvOffset = new Vector2(mr.lightmapScaleOffset.z, mr.lightmapScaleOffset.w);
            Debug.Log("UvScale: " + uvScale.ToString() + " UvOffset: " + uvOffset.ToString());
            Vector2[] newUvs = new Vector2[mesh.uv.Length];
            for (int i = 0; i < mesh.uv.Length; ++i)
            {
                Vector2 newUv2 = mesh.uv[i];
                newUv2 = newUv2 * uvScale + uvOffset;
                Debug.Log("SourceUv: " + mesh.uv[i].ToString() + " NewUv: " + newUv2.ToString());
                newUvs[i] = newUv2;
            }
            mesh.uv = newUvs;
            Debug.Log("EN=P==================================================");

            return AddMesh(node, mesh);
        }

        bool AddSkinnedMesh(Node node, SkinnedMeshRenderer smr)
        {
            var mesh = smr.sharedMesh;
            if (!AddMesh(node, mesh))
                return false;

            var bones = smr.bones;
            var boneNodes = new PinnedArray<Node>(bones.Length);
            for (int bi = 0; bi < bones.Length; ++bi)
                boneNodes[bi] = FindOrCreateNodeTree(bones[bi], ProcessNode);
            var boneWeights = new PinnedArray<BoneWeight>(mesh.boneWeights);
            var bindposes = new PinnedArray<Matrix4x4>(mesh.bindposes);
            fbxeAddMeshSkin(m_ctx, node, boneWeights, boneNodes.Length, boneNodes, bindposes);
            return true;
        }

        bool AddTerrain(Node node, Terrain terrain)
        {
            var tdata = terrain.terrainData;
            var w = tdata.heightmapResolution;
            var h = tdata.heightmapResolution;
            var heightmap = tdata.GetHeights(0, 0, w, h);

            int vertexCount = w * h;
            int indexCount = (w - 1) * (h - 1) * 2 * 3;
            var vertices = new PinnedArray<Vector3>(vertexCount);
            var normals = new PinnedArray<Vector3>(vertexCount);
            var uv = new PinnedArray<Vector2>(vertexCount);
            var indices = new PinnedArray<int>(indexCount);
            fbxeGenerateTerrainMesh(heightmap, w, h, tdata.size,
                vertices, normals, uv, indices);

            Topology topology = Topology.Triangles;
            fbxeAddMesh(m_ctx, node, vertices.Length, vertices, normals, IntPtr.Zero, uv, IntPtr.Zero);
            fbxeAddMeshSubmesh(m_ctx, node, topology, indices.Length, indices, -1);

            return true;
        }
        #endregion
    }

}
