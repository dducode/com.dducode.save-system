using System.Runtime.InteropServices;

namespace SaveSystemPackage.Security {

    public sealed class EncodedValue<TValue> : EncodedObject<TValue> where TValue : unmanaged {

        public override TValue Value {
            get => MemoryMarshal.Read<TValue>(Encode(bytes));
            set => bytes = Encode(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray());
        }


        public EncodedValue (int iterations = 1, int keyLength = 16) : base(iterations, keyLength) {
            Value = default;
        }

    }

}