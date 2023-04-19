using Unity.Collections;
using UnityEngine;

namespace SaveSystem {

    public struct NativeMesh {

        public readonly string name;
        public NativeArray<Vector3> vertices;
        public NativeArray<Vector2> uv;
        public NativeArray<Vector2> uv2;
        public NativeArray<Vector2> uv3;
        public NativeArray<Vector2> uv4;
        public NativeArray<Vector2> uv5;
        public NativeArray<Vector2> uv6;
        public NativeArray<Vector2> uv7;
        public NativeArray<Vector2> uv8;

        public Bounds bounds;

        public NativeArray<Color32> colors32;
        public NativeArray<Vector3> normals;
        public NativeArray<Vector4> tangents;
        public NativeArray<int> triangles;


        public NativeMesh (Mesh mesh) {
            name = new string(mesh.name);
            vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
            uv = new NativeArray<Vector2>(mesh.uv, Allocator.Persistent);
            uv2 = new NativeArray<Vector2>(mesh.uv2, Allocator.Persistent);
            uv3 = new NativeArray<Vector2>(mesh.uv3, Allocator.Persistent);
            uv4 = new NativeArray<Vector2>(mesh.uv4, Allocator.Persistent);
            uv5 = new NativeArray<Vector2>(mesh.uv5, Allocator.Persistent);
            uv6 = new NativeArray<Vector2>(mesh.uv6, Allocator.Persistent);
            uv7 = new NativeArray<Vector2>(mesh.uv7, Allocator.Persistent);
            uv8 = new NativeArray<Vector2>(mesh.uv8, Allocator.Persistent);

            bounds = mesh.bounds;

            colors32 = new NativeArray<Color32>(mesh.colors32, Allocator.Persistent);
            normals = new NativeArray<Vector3>(mesh.normals, Allocator.Persistent);
            tangents = new NativeArray<Vector4>(mesh.tangents, Allocator.Persistent);
            triangles = new NativeArray<int>(mesh.triangles, Allocator.Persistent);
        }


        public void Dispose () {
            vertices.Dispose();
            uv.Dispose();
            uv2.Dispose();
            uv3.Dispose();
            uv4.Dispose();
            uv5.Dispose();
            uv6.Dispose();
            uv7.Dispose();
            uv8.Dispose();
            colors32.Dispose();
            normals.Dispose();
            tangents.Dispose();
            triangles.Dispose();
        }

    }

}