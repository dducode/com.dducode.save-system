using UnityEngine;

namespace SaveSystem {

    internal struct MeshData {

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


        public MeshData (Mesh mesh) {
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
        }

    }

}