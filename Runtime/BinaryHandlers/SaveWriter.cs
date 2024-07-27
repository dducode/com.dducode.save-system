using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SaveSystemPackage.Attributes;
using UnityEngine.Rendering;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage.BinaryHandlers {

    public class SaveWriter : IDisposable, IAsyncDisposable {

        public readonly Stream input;


        public SaveWriter (Stream input) {
            this.input = input;
        }


        public void Write<TValue> (TValue value) where TValue : unmanaged {
            ReadOnlySpan<TValue> span = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
            input.Write(MemoryMarshal.AsBytes(span));
        }


        public void Write (object graph) {
            Type type = graph.GetType();
            Write(type.IsPrimitive);

            if (!type.IsPrimitive) {
                bool isString = type == typeof(string);
                Write(isString);

                if (isString) {
                    var str = (string)graph;
                    Write(str.ToCharArray());
                    return;
                }

                Write(type.IsArray);

                if (type.IsArray) {
                    var array = (Array)graph;
                    Write(array.Length);
                    Type elementType = type.GetElementType() ?? throw new InvalidOperationException();
                    Write(elementType.AssemblyQualifiedName);
                    foreach (object element in array)
                        Write(element);
                    return;
                }

                FieldInfo[] fields = type
                   .GetFields()
                   .Concat(type
                       .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                       .Where(field => field.IsDefined(typeof(RuntimeSerializedFieldAttribute))))
                   .ToArray();
                Write(fields.Length);

                foreach (FieldInfo field in fields) {
                    Write(field.Name);
                    Write(field.FieldType.AssemblyQualifiedName);
                    Write(field.GetValue(graph));
                }

                PropertyInfo[] properties = type
                   .GetProperties()
                   .Concat(type
                       .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                       .Where(property => property.IsDefined(typeof(RuntimeSerializedPropertyAttribute))))
                   .ToArray();
                Write(properties.Length);

                foreach (PropertyInfo property in properties) {
                    Write(property.Name);
                    Write(property.PropertyType.AssemblyQualifiedName);
                    Write(property.GetValue(graph));
                }

                return;
            }

            int size = Marshal.SizeOf(graph);
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(graph, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            Write(bytes.Length);
            input.Write(bytes);
        }


        public void Write<TValue> ([NotNull] TValue[] array) where TValue : unmanaged {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Write((ReadOnlySpan<TValue>)array);
        }


        public void Write<TValue> (ReadOnlySpan<TValue> span) where TValue : unmanaged {
            Write(span.Length);
            input.Write(MemoryMarshal.AsBytes(span));
        }


        public void Write (Version version) {
            Write(version.Major);
            Write(version.Minor);
            Write(version.Build);
            Write(version.Revision);
        }


        public void Write (MeshData mesh) {
            Write(mesh.subMeshes.Length);

            for (var i = 0; i < mesh.subMeshes.Length; i++) {
                SubMeshDescriptor subMesh = mesh.subMeshes[i];
                Write(subMesh.baseVertex);
                Write(subMesh.bounds.center);
                Write(subMesh.bounds.extents);
                Write(subMesh.bounds.max);
                Write(subMesh.bounds.min);
                Write(subMesh.bounds.size);
                Write(subMesh.firstVertex);
                Write(subMesh.indexCount);
                Write(subMesh.indexStart);
                Write(subMesh.vertexCount);
                Write(subMesh.topology);
                Write(mesh.subMeshIndices[i]);
            }

            Write(mesh.name);
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
            Write(mesh.indexBufferTarget);
            Write(mesh.indexFormat);
            Write(mesh.vertexBufferTarget);
        }


        public void Write ([NotNull] MeshData[] array) {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Write(array.Length);
            foreach (MeshData meshData in array)
                Write(meshData);
        }


        public void Write (ReadOnlySpan<MeshData> span) {
            Write(span.Length);
            foreach (MeshData meshData in span)
                Write(meshData);
        }


        public void Write (string value) {
            Write((ReadOnlySpan<char>)value);
        }


        public void Write ([NotNull] string[] array) {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Write(array.Length);
            foreach (string value in array)
                Write(value);
        }


        public void Write (DataBuffer dataBuffer) {
            dataBuffer.WriteData(this);
        }


        public void Dispose () {
            if (input != null) {
                input.SetLength(input.Position);
                input.Dispose();
            }
        }


        public async ValueTask DisposeAsync () {
            if (input != null)
                await input.DisposeAsync();
        }

    }

}