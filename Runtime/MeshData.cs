using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem {

    /// <summary>
    /// Contains the data of a mesh. Mesh can be casted to MeshData and vice-versa
    /// </summary>
    public struct MeshData {

        internal SubMeshDescriptor[] subMeshes;
        internal int[][] subMeshIndices;
        internal string name;
        internal Vector3[] vertices;
        internal Vector2[] uv;
        internal Vector2[] uv2;
        internal Vector2[] uv3;
        internal Vector2[] uv4;
        internal Vector2[] uv5;
        internal Vector2[] uv6;
        internal Vector2[] uv7;
        internal Vector2[] uv8;
        internal Color32[] colors32;
        internal Vector3[] normals;
        internal Vector4[] tangents;
        internal int[] triangles;
        internal Bounds bounds;
        internal GraphicsBuffer.Target indexBufferTarget;
        internal IndexFormat indexFormat;
        internal GraphicsBuffer.Target vertexBufferTarget;


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


        public static bool operator == (MeshData meshData1, MeshData meshData2) {
            return meshData1.bounds == meshData2.bounds &&
                   meshData1.colors32 == meshData2.colors32 &&
                   meshData1.name == meshData2.name &&
                   meshData1.normals == meshData2.normals &&
                   meshData1.tangents == meshData2.tangents &&
                   meshData1.triangles == meshData2.triangles &&
                   meshData1.uv == meshData2.uv &&
                   meshData1.uv2 == meshData2.uv2 &&
                   meshData1.uv3 == meshData2.uv3 &&
                   meshData1.uv4 == meshData2.uv4 &&
                   meshData1.uv5 == meshData2.uv5 &&
                   meshData1.uv6 == meshData2.uv6 &&
                   meshData1.uv7 == meshData2.uv7 &&
                   meshData1.uv8 == meshData2.uv8 &&
                   meshData1.vertices == meshData2.vertices &&
                   meshData1.indexFormat == meshData2.indexFormat &&
                   meshData1.subMeshes == meshData2.subMeshes &&
                   meshData1.indexBufferTarget == meshData2.indexBufferTarget &&
                   meshData1.subMeshIndices == meshData2.subMeshIndices &&
                   meshData1.vertexBufferTarget == meshData2.vertexBufferTarget;
        }


        public static bool operator != (MeshData meshData1, MeshData meshData2) {
            return !(meshData1 == meshData2);
        }


        public override bool Equals (object obj) {
            return obj is MeshData other && Equals(other);
        }


        public override int GetHashCode () {
            var hashCode = new HashCode();
            hashCode.Add(subMeshes);
            hashCode.Add(subMeshIndices);
            hashCode.Add(name);
            hashCode.Add(vertices);
            hashCode.Add(uv);
            hashCode.Add(uv2);
            hashCode.Add(uv3);
            hashCode.Add(uv4);
            hashCode.Add(uv5);
            hashCode.Add(uv6);
            hashCode.Add(uv7);
            hashCode.Add(uv8);
            hashCode.Add(colors32);
            hashCode.Add(normals);
            hashCode.Add(tangents);
            hashCode.Add(triangles);
            hashCode.Add(bounds);
            hashCode.Add((int)indexBufferTarget);
            hashCode.Add((int)indexFormat);
            hashCode.Add((int)vertexBufferTarget);
            return hashCode.ToHashCode();
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


        private bool Equals (MeshData other) {
            return Equals(subMeshes, other.subMeshes) && Equals(subMeshIndices, other.subMeshIndices) &&
                   name == other.name && Equals(vertices, other.vertices) && Equals(uv, other.uv) &&
                   Equals(uv2, other.uv2) && Equals(uv3, other.uv3) && Equals(uv4, other.uv4) &&
                   Equals(uv5, other.uv5) && Equals(uv6, other.uv6) && Equals(uv7, other.uv7) &&
                   Equals(uv8, other.uv8) && Equals(colors32, other.colors32) && Equals(normals, other.normals) &&
                   Equals(tangents, other.tangents) && Equals(triangles, other.triangles) &&
                   bounds.Equals(other.bounds) && indexBufferTarget == other.indexBufferTarget &&
                   indexFormat == other.indexFormat && vertexBufferTarget == other.vertexBufferTarget;
        }

    }

}