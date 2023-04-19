using System;
using System.IO;
using System.Threading.Tasks;
using Codice.Client.BaseCommands.Download;
using Unity.Collections;
using UnityEngine;

namespace SaveSystem {

    public class UnityAsyncWriter : IDisposable {

        private readonly BinaryWriter m_writer;


        public UnityAsyncWriter (BinaryWriter writer) {
            m_writer = writer;
        }


        public async Task Write (Vector2 vector2) {
            await Task.Run(() => {
                m_writer.Write(vector2.x);
                m_writer.Write(vector2.y);
            });
        }


        public async Task Write (Vector2[] vector2Array) {
            await Write(new NativeArray<Vector2>(vector2Array, Allocator.Temp));
        }


        public async Task Write (NativeArray<Vector2> vector2Array) {
            await Task.Run(async () => {
                m_writer.Write(vector2Array.Length);
                foreach (var vector2 in vector2Array)
                    await Write(vector2);
            });
        }


        public async Task Write (Vector3 vector3) {
            await Task.Run(() => {
                m_writer.Write(vector3.x);
                m_writer.Write(vector3.y);
                m_writer.Write(vector3.z);
            });
        }


        public async Task Write (Vector3[] vector3Array) {
            await Write(new NativeArray<Vector3>(vector3Array, Allocator.Temp));
        }


        public async Task Write (NativeArray<Vector3> vector3Array) {
            await Task.Run(async () => {
                m_writer.Write(vector3Array.Length);
                foreach (var vector3 in vector3Array)
                    await Write(vector3);
            });
        }


        public async Task Write (Vector4 vector4) {
            await Task.Run(() => {
                m_writer.Write(vector4.x);
                m_writer.Write(vector4.y);
                m_writer.Write(vector4.z);
                m_writer.Write(vector4.w);
            });
        }


        public async Task Write (Vector4[] vector4Array) {
            await Write(new NativeArray<Vector4>(vector4Array, Allocator.Temp));
        }


        public async Task Write (NativeArray<Vector4> vector4Array) {
            await Task.Run(async () => {
                m_writer.Write(vector4Array.Length);
                foreach (var vector4 in vector4Array)
                    await Write(vector4);
            });
        }


        public async Task Write (Color32 color32) {
            await Task.Run(() => {
                m_writer.Write(color32.r);
                m_writer.Write(color32.g);
                m_writer.Write(color32.b);
                m_writer.Write(color32.a);
            });
        }


        public async Task Write (Color32[] colors32) {
            await Write(new NativeArray<Color32>(colors32, Allocator.Temp));
        }


        public async Task Write (NativeArray<Color32> colors32) {
            await Task.Run(async () => {
                m_writer.Write(colors32.Length);
                foreach (var color32 in colors32)
                    await Write(color32);
            });
        }


        public async Task Write (Mesh mesh) {
            var nativeMesh = new NativeMesh(mesh);
            await Task.Run(async () => {
                m_writer.Write(nativeMesh.name);
                await Write(nativeMesh.vertices);
                await Write(nativeMesh.uv);
                await Write(nativeMesh.uv2);
                await Write(nativeMesh.uv3);
                await Write(nativeMesh.uv4);
                await Write(nativeMesh.uv5);
                await Write(nativeMesh.uv6);
                await Write(nativeMesh.uv7);
                await Write(nativeMesh.uv8);
                await Write(nativeMesh.bounds.center);
                await Write(nativeMesh.bounds.extents);
                await Write(nativeMesh.bounds.max);
                await Write(nativeMesh.bounds.min);
                await Write(nativeMesh.bounds.size);
                await Write(nativeMesh.colors32);
                await Write(nativeMesh.normals);
                await Write(nativeMesh.tangents);
                await Write(nativeMesh.triangles);
            });
            nativeMesh.Dispose();
        }


        public async Task Write (int[] intValues) {
            await Write(new NativeArray<int>(intValues, Allocator.Temp));
        }


        public async Task Write (NativeArray<int> intValues) {
            await Task.Run(() => {
                m_writer.Write(intValues.Length);
                foreach (var intValue in intValues)
                    m_writer.Write(intValue);
            });
        }


        public void Dispose () {
            m_writer?.Dispose();
        }

    }

}