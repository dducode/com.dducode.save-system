using System.Text;

namespace SaveSystemPackage.Security {

    public sealed class EncodedString : EncodedObject<string> {

        public override string Value {
            get => Encoding.Default.GetString(Encode(bytes));
            set => bytes = Encode(Encoding.Default.GetBytes(value));
        }


        public EncodedString (int iterations = 1, int keyLength = 16) : base(iterations, keyLength) {
            Value = string.Empty;
        }

    }

}