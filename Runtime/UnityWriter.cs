using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem {

    /// <summary>
    /// Adapter to class <see cref="BinaryWriter"/> for simplify writing data
    /// </summary>
    public sealed class UnityWriter : IDisposable {

        private readonly BinaryWriter m_writer;
        public readonly string localPath;


        public UnityWriter (BinaryWriter writer, string localPath) {
            m_writer = writer;
            this.localPath = localPath;
        }


        public void Write (Vector2 vector2) {
            m_writer.Write(vector2.x);
            m_writer.Write(vector2.y);
        }


        public void Write (Vector2[] vector2Array) {
            m_writer.Write(vector2Array.Length);
            foreach (var vector2 in vector2Array)
                Write(vector2);
        }


        public async UniTask WriteAsync (Vector2[] vector2Array, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(vector2Array); });
                return;
            }

            await UniTask.NextFrame();
            Write(vector2Array);
        }


        public void Write (Vector3 vector3) {
            m_writer.Write(vector3.x);
            m_writer.Write(vector3.y);
            m_writer.Write(vector3.z);
        }


        public void Write (Vector3[] vector3Array) {
            m_writer.Write(vector3Array.Length);
            foreach (var vector3 in vector3Array)
                Write(vector3);
        }


        public async UniTask WriteAsync (Vector3[] vector3Array, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(vector3Array); });
                return;
            }

            await UniTask.NextFrame();
            Write(vector3Array);
        }


        public void Write (Vector4 vector4) {
            m_writer.Write(vector4.x);
            m_writer.Write(vector4.y);
            m_writer.Write(vector4.z);
            m_writer.Write(vector4.w);
        }


        public void Write (Vector4[] vector4Array) {
            m_writer.Write(vector4Array.Length);
            foreach (var vector4 in vector4Array)
                Write(vector4);
        }


        public async UniTask WriteAsync (Vector4[] vector4Array, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(vector4Array); });
                return;
            }

            await UniTask.NextFrame();
            Write(vector4Array);
        }


        public void Write (Quaternion rotation) {
            m_writer.Write(rotation.x);
            m_writer.Write(rotation.y);
            m_writer.Write(rotation.z);
            m_writer.Write(rotation.w);
        }


        public void Write (Color color) {
            m_writer.Write(color.r);
            m_writer.Write(color.g);
            m_writer.Write(color.b);
            m_writer.Write(color.a);
        }


        public void Write (Color[] colors) {
            m_writer.Write(colors.Length);
            foreach (var color in colors)
                Write(color);
        }


        public async UniTask WriteAsync (Color[] colors, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(colors); });
                return;
            }

            await UniTask.NextFrame();
            Write(colors);
        }


        public void Write (Color32 color32) {
            m_writer.Write(color32.r);
            m_writer.Write(color32.g);
            m_writer.Write(color32.b);
            m_writer.Write(color32.a);
        }


        public void Write (Color32[] colors32) {
            m_writer.Write(colors32.Length);
            foreach (var color32 in colors32)
                Write(color32);
        }


        public async UniTask WriteAsync (Color32[] colors32, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(colors32); });
                return;
            }

            await UniTask.NextFrame();
            Write(colors32);
        }


        public void Write (Matrix4x4 matrix) {
            for (var i = 0; i < 16; i++)
                m_writer.Write(matrix[i]);
        }


        public void Write (Matrix4x4[] matrices) {
            m_writer.Write(matrices.Length);
            foreach (var matrix in matrices)
                Write(matrix);
        }


        public async UniTask WriteAsync (Matrix4x4[] matrices, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(matrices); });
                return;
            }

            await UniTask.NextFrame();
            Write(matrices);
        }



        #region WritingMeshes

        public void Write (Mesh mesh) {
            var subMeshes = new SubMeshDescriptor[mesh.subMeshCount];
            for (var i = 0; i < subMeshes.Length; i++)
                subMeshes[i] = mesh.GetSubMesh(i);

            m_writer.Write(subMeshes.Length);

            foreach (var subMesh in subMeshes) {
                m_writer.Write(subMesh.baseVertex);
                Write(subMesh.bounds.center);
                Write(subMesh.bounds.extents);
                Write(subMesh.bounds.max);
                Write(subMesh.bounds.min);
                Write(subMesh.bounds.size);
                m_writer.Write(subMesh.firstVertex);
                m_writer.Write(subMesh.indexCount);
                m_writer.Write(subMesh.indexStart);
                m_writer.Write((int) subMesh.topology);
                m_writer.Write(subMesh.vertexCount);
            }

            m_writer.Write(mesh.name);
            Write(mesh.vertices);
            Write(mesh.uv);
            Write(mesh.uv2);
            Write(mesh.uv3);
            Write(mesh.uv4);
            Write(mesh.uv5);
            Write(mesh.uv6);
            Write(mesh.uv7);
            Write(mesh.uv8);
            Write(mesh.colors32);
            Write(mesh.normals);
            Write(mesh.tangents);
            Write(mesh.triangles);
            Write(mesh.bounds.center);
            Write(mesh.bounds.extents);
            Write(mesh.bounds.max);
            Write(mesh.bounds.min);
            Write(mesh.bounds.size);
            m_writer.Write((int) mesh.indexBufferTarget);
            m_writer.Write((int) mesh.indexFormat);
            m_writer.Write((int) mesh.vertexBufferTarget);
        }


        public async UniTask WriteAsync (Mesh mesh, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            var subMeshes = new SubMeshDescriptor[mesh.subMeshCount];
            for (var i = 0; i < subMeshes.Length; i++)
                subMeshes[i] = mesh.GetSubMesh(i);

            if (asyncMode == AsyncMode.OnThreadPool)
                await UniTask.RunOnThreadPool(WriteSubMeshes);
            else {
                await UniTask.NextFrame();
                WriteSubMeshes();
            }

            var meshData = new MeshData(mesh);
            if (asyncMode == AsyncMode.OnThreadPool)
                await UniTask.SwitchToThreadPool();
            await WriteAsync(meshData.vertices, asyncMode);
            await WriteAsync(meshData.uv, asyncMode);
            await WriteAsync(meshData.uv2, asyncMode);
            await WriteAsync(meshData.uv3, asyncMode);
            await WriteAsync(meshData.uv4, asyncMode);
            await WriteAsync(meshData.uv5, asyncMode);
            await WriteAsync(meshData.uv6, asyncMode);
            await WriteAsync(meshData.uv7, asyncMode);
            await WriteAsync(meshData.uv8, asyncMode);
            await WriteAsync(meshData.colors32, asyncMode);
            await WriteAsync(meshData.normals, asyncMode);
            await WriteAsync(meshData.tangents, asyncMode);
            await WriteAsync(meshData.triangles, asyncMode);
            if (asyncMode == AsyncMode.OnThreadPool)
                await UniTask.SwitchToMainThread();
            m_writer.Write(mesh.name);
            Write(mesh.bounds.center);
            Write(mesh.bounds.extents);
            Write(mesh.bounds.max);
            Write(mesh.bounds.min);
            Write(mesh.bounds.size);
            m_writer.Write((int) mesh.indexBufferTarget);
            m_writer.Write((int) mesh.indexFormat);
            m_writer.Write((int) mesh.vertexBufferTarget);

            void WriteSubMeshes () {
                m_writer.Write(subMeshes.Length);

                foreach (var subMesh in subMeshes) {
                    m_writer.Write(subMesh.baseVertex);
                    Write(subMesh.bounds.center);
                    Write(subMesh.bounds.extents);
                    Write(subMesh.bounds.max);
                    Write(subMesh.bounds.min);
                    Write(subMesh.bounds.size);
                    m_writer.Write(subMesh.firstVertex);
                    m_writer.Write(subMesh.indexCount);
                    m_writer.Write(subMesh.indexStart);
                    m_writer.Write((int) subMesh.topology);
                    m_writer.Write(subMesh.vertexCount);
                }
            }
        }


        public void Write (Mesh[] meshes) {
            m_writer.Write(meshes.Length);
            foreach (var mesh in meshes)
                Write(mesh);
        }


        public async UniTask WriteAsync (Mesh[] meshes, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            m_writer.Write(meshes.Length);
            foreach (var mesh in meshes)
                await WriteAsync(mesh, asyncMode);
        }

        #endregion



        #region WritingObjects

        public void Write<T> (T obj) {
            m_writer.Write(JsonUtility.ToJson(obj));
        }


        public void Write<T> (List<T> listObjects) {
            m_writer.Write(listObjects.Count);
            foreach (var obj in listObjects)
                Write(obj);
        }


        public async UniTask WriteAsync<T> (List<T> listObjects, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(listObjects); });
                return;
            }

            await UniTask.NextFrame();
            Write(listObjects);
        }


        public void Write<T> (T[] arrayObjects) {
            m_writer.Write(arrayObjects.Length);
            foreach (var obj in arrayObjects)
                Write(obj);
        }


        public async UniTask WriteAsync<T> (T[] arrayObjects, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(arrayObjects); });
                return;
            }

            await UniTask.NextFrame();
            Write(arrayObjects);
        }

        #endregion



        public void Write (byte byteValue) {
            m_writer.Write(byteValue);
        }


        public void Write (byte[] bytes) {
            m_writer.Write(bytes.Length);
            foreach (var byteValue in bytes)
                m_writer.Write(byteValue);
        }


        public async UniTask WriteAsync (byte[] bytes, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(bytes); });
                return;
            }

            await UniTask.NextFrame();
            Write(bytes);
        }


        public void Write (short shortValue) {
            m_writer.Write(shortValue);
        }


        public void Write (short[] shorts) {
            m_writer.Write(shorts.Length);
            foreach (var shortValue in shorts)
                m_writer.Write(shortValue);
        }


        public void Write (int intValue) {
            m_writer.Write(intValue);
        }


        public void Write (int[] ints) {
            m_writer.Write(ints.Length);
            foreach (var intValue in ints)
                m_writer.Write(intValue);
        }


        public async UniTask WriteAsync (int[] intValues, AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool) {
                await UniTask.RunOnThreadPool(() => { Write(intValues); });
                return;
            }

            await UniTask.NextFrame();
            Write(intValues);
        }


        public void Write (long longValue) {
            m_writer.Write(longValue);
        }


        public void Write (long[] longs) {
            m_writer.Write(longs.Length);
            foreach (var longValue in longs)
                m_writer.Write(longValue);
        }


        public void Write (char charValue) {
            m_writer.Write(charValue);
        }


        public void Write (char[] chars) {
            m_writer.Write(chars.Length);
            foreach (var charValue in chars)
                m_writer.Write(charValue);
        }


        public void Write (string stringValue) {
            m_writer.Write(stringValue);
        }


        public void Write (string[] strings) {
            m_writer.Write(strings.Length);
            foreach (var stringValue in strings)
                m_writer.Write(stringValue);
        }


        public void Write (float floatValue) {
            m_writer.Write(floatValue);
        }


        public void Write (float[] floats) {
            m_writer.Write(floats.Length);
            foreach (var floatValue in floats)
                m_writer.Write(floatValue);
        }


        public void Write (double doubleValue) {
            m_writer.Write(doubleValue);
        }


        public void Write (double[] doubles) {
            m_writer.Write(doubles.Length);
            foreach (var doubleValue in doubles)
                m_writer.Write(doubleValue);
        }


        public void Write (bool boolValue) {
            m_writer.Write(boolValue);
        }


        public void Dispose () {
            m_writer?.Dispose();
        }

    }

}