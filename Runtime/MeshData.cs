using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem {

    /// <summary>
    /// Contains the data of a mesh. Mesh can be casted to MeshData and vice-versa
    /// </summary>
    public struct MeshData {

        public SubMeshDescriptor[] subMeshes;
        public int[][] subMeshIndices;
        public string name;
        public Vector3[] vertices;
        public Vector2[] uv;
        public Vector2[] uv2;
        public Vector2[] uv3;
        public Vector2[] uv4;
        public Vector2[] uv5;
        public Vector2[] uv6;
        public Vector2[] uv7;
        public Vector2[] uv8;
        public Color32[] colors32;
        public Vector3[] normals;
        public Vector4[] tangents;
        public int[] triangles;
        public Bounds bounds;
        public GraphicsBuffer.Target indexBufferTarget;
        public IndexFormat indexFormat;
        public GraphicsBuffer.Target vertexBufferTarget;


        public static implicit operator MeshData (Mesh mesh) {
            return new MeshData(mesh);
        }


        public static implicit operator Mesh (MeshData meshData) {
            var mesh = new Mesh {
                name = meshData.name,
                vertices = meshData.vertices,
                uv = meshData.uv,
                uv2 = meshData.uv2,
                uv3 = meshData.uv3,
                uv4 = meshData.uv4,
                uv5 = meshData.uv5,
                uv6 = meshData.uv6,
                uv7 = meshData.uv7,
                uv8 = meshData.uv8,
                colors32 = meshData.colors32,
                normals = meshData.normals,
                tangents = meshData.tangents,
                triangles = meshData.triangles,
                bounds = meshData.bounds,
                indexBufferTarget = meshData.indexBufferTarget,
                indexFormat = meshData.indexFormat,
                vertexBufferTarget = meshData.vertexBufferTarget
            };

            for (var i = 0; i < meshData.subMeshes.Length; i++)
                mesh.SetIndices(meshData.subMeshIndices[i], meshData.subMeshes[i].topology, i);

            mesh.SetSubMeshes(meshData.subMeshes);
            return mesh;
        }


        private MeshData (Mesh mesh) {
            subMeshes = new SubMeshDescriptor[mesh.subMeshCount];
            subMeshIndices = new int[mesh.subMeshCount][];

            for (var i = 0; i < subMeshes.Length; i++) {
                subMeshes[i] = mesh.GetSubMesh(i);
                subMeshIndices[i] = mesh.GetIndices(i);
            }

            name = mesh.name;
            vertices = mesh.vertices;
            uv = mesh.uv;
            uv2 = mesh.uv2;
            uv3 = mesh.uv3;
            uv4 = mesh.uv4;
            uv5 = mesh.uv5;
            uv6 = mesh.uv6;
            uv7 = mesh.uv7;
            uv8 = mesh.uv8;
            colors32 = mesh.colors32;
            normals = mesh.normals;
            tangents = mesh.tangents;
            triangles = mesh.triangles;
            bounds = new Bounds {
                center = mesh.bounds.center,
                extents = mesh.bounds.extents,
                max = mesh.bounds.max,
                min = mesh.bounds.min,
                size = mesh.bounds.size
            };
            indexBufferTarget = mesh.indexBufferTarget;
            indexFormat = mesh.indexFormat;
            vertexBufferTarget = mesh.vertexBufferTarget;
        }

    }

}