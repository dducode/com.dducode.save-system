using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.Security;
using SaveSystemPackage.Verification;
using File = SaveSystemPackage.Internal.File;

namespace SaveSystemPackage.Tests {

    public class VerificationManagerTests {

        private const string LoremIpsum =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis egestas cursus odio quis volutpat. Sed et nisl at quam dapibus faucibus at nec dui. Suspendisse rhoncus erat tincidunt risus dignissim, ut euismod odio vestibulum. Nunc vitae ante et justo bibendum eleifend quis eu ante. Vestibulum congue velit id imperdiet efficitur. Aenean interdum elit non neque pellentesque efficitur. Curabitur gravida tellus quis dapibus commodo. Suspendisse potenti. Vivamus nec blandit lorem, at tempus ante. Donec porttitor tellus elementum, tristique est eget, porta lorem. Mauris sit amet nibh imperdiet massa sodales sagittis. Sed sagittis magna sit amet tristique varius.\n\nNunc aliquam volutpat luctus. Quisque tempus, ex in ornare egestas, neque lectus vulputate nibh, vel volutpat lorem odio quis felis. Vestibulum in tincidunt eros. Integer eros massa, interdum in ante tincidunt, luctus hendrerit nisl. Donec egestas purus vitae venenatis venenatis. Praesent imperdiet leo mauris, vel aliquet erat pharetra sed. Nullam ut commodo massa. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse ac tempor lacus, ut pretium ex. Integer sagittis risus nulla, sed facilisis lectus pellentesque non.\n\nMorbi interdum velit at facilisis lacinia. Proin nec efficitur urna. Sed sed dolor semper, cursus justo et, malesuada mi. Donec egestas, mauris placerat finibus imperdiet, augue sapien tristique odio, eget tincidunt lorem nulla vel metus. Nam hendrerit, lorem non posuere congue, tellus mi vehicula arcu, id rutrum neque odio at lacus. Nullam tristique egestas hendrerit. Phasellus rhoncus nulla nunc. Maecenas lacinia vel ipsum venenatis malesuada. Curabitur commodo, lorem ut tincidunt bibendum, erat quam sodales diam, at volutpat nunc lorem in eros. Proin ligula urna, vulputate ac urna in, hendrerit porta lectus. Sed vulputate facilisis scelerisque. Sed sed est at sem iaculis luctus id sed elit. Suspendisse sit amet mauris in tellus luctus imperdiet sed nec orci. Vestibulum eget tincidunt felis, ac convallis nulla.\n\nIn hac habitasse platea dictumst. Mauris ut convallis ligula. Praesent mattis sollicitudin accumsan. Pellentesque interdum lobortis luctus. Mauris suscipit sem ut mauris scelerisque placerat. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Fusce vitae volutpat nibh. Sed sed neque sit amet ex varius porta ut ac lacus. Mauris tempor orci quis nibh feugiat vulputate. Ut vel accumsan felis, vel consequat diam. Aenean ultrices non ante ut imperdiet. Curabitur faucibus, velit nec tincidunt dictum, ex elit tempus lectus, accumsan ornare purus libero nec mauris. Fusce sollicitudin mauris urna, vitae rhoncus lorem blandit quis. Vivamus sollicitudin tellus eget erat dapibus facilisis. Nulla eu accumsan velit.\n\nNunc id neque nisi. Praesent ullamcorper justo vulputate dapibus accumsan. Duis scelerisque nec odio eget suscipit. Vivamus in massa scelerisque, finibus nulla sit amet, accumsan dolor. Vivamus sem lorem, placerat eget urna ut, sollicitudin luctus nulla. Praesent in felis iaculis, faucibus dolor nec, maximus metus. Proin elementum porttitor iaculis. Curabitur a mi blandit, dapibus enim non, lacinia turpis. Nam ullamcorper lacus ex, sed luctus eros molestie vel. Mauris porttitor quis ante ac tristique. Praesent elementum tellus at tincidunt aliquam. In erat purus, ornare quis lorem et, varius euismod erat. Morbi vitae faucibus turpis. Duis eget orci ut sem fermentum tempor ut quis mi.";

        private readonly File m_sourceFile = Storage.TestsDirectory.GetOrCreateFile("LoremIpsum", "txt");


        [SetUp]
        public void Start () {
            if (m_sourceFile.IsEmpty)
                m_sourceFile.WriteAllBytes(Encoding.Default.GetBytes(LoremIpsum));
        }


        [Test, Order(0)]
        public async Task SetLoremIpsumAuthHash () {
            var verificationManager = new VerificationManager(new DefaultHashStorage(), HashAlgorithmName.SHA1);
            await verificationManager.SetChecksum(m_sourceFile, await m_sourceFile.ReadAllBytesAsync());
        }


        [Test, Order(1)]
        public async Task AuthenticateLoremIpsumData () {
            var verificationManager = new VerificationManager(new DefaultHashStorage(), HashAlgorithmName.SHA1);
            await verificationManager.VerifyData(m_sourceFile, await m_sourceFile.ReadAllBytesAsync());
        }

    }

}