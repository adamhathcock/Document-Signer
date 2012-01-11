using FluentAssertions;
using NUnit.Framework;

namespace wSigner.Test
{
    [TestFixture]
    public class CertificateUtilitiesTest
    {
        [Test]
        public void CanGetCertificates()
        {
            var result = CertificateUtil.GetAll(c => new {c.Subject, c.Issuer, Serial = c.GetSerialNumberString()});
            result.Should().NotBeNull();
        }

        [Test]
        public void CanGetCertificateBySerial()
        {
            //NOTE:this test is not meant to be automated, install the corresponding certificate before running
            var serial = "54 03 4b e4 23 43 48 4a 50 c3 9a 00 d4 3c e1 cb";
            var cert = CertificateUtil.GetBySerial(serial, false);
            cert.Should().NotBeNull();
        }

        [Test]
        public void CanNormalizeSerial()
        {
            var input = "  54 03 4b e4 23 43 48 4A 50 c3 9A   00 d4 3c e1 cb  ";
            var expected = "54034be42343484A50c39A00d43ce1cb";
            var actual = CertificateUtil.NormalizeSerialString(input);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNormalizedNullSerialToEmptyString()
        {
            string input = null;
            var expected = "";
            var actual = CertificateUtil.NormalizeSerialString(input);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}