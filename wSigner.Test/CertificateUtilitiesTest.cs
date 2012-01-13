using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace wSigner.Test
{
    [TestFixture]
    public class CertificateUtilitiesTest
    {
        private string _certPath        = Path.Combine(Environment.CurrentDirectory, "input", "test-cert.pfx"),
                       _certPassword    = "1234";
        
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanGetCertificates()
        {
            var result = CertUtil.GetAll(c => new {c.Subject, c.Issuer, Serial = c.GetSerialNumberString()});
            result.Should().NotBeNull();
        }

        [Test]
        public void Can_Read_Add_Find_Remove_And_FindNot()
        {
            //read
            var certFromFile = CertUtil.GetFromFile(_certPath, _certPassword);
            certFromFile.Should().NotBeNull("Failed to read");
            var serial = certFromFile.SerialNumber;

            //add
            CertUtil.AddCertificate(certFromFile);
            CertUtil.AddCertificate(certFromFile, System.Security.Cryptography.X509Certificates.StoreName.TrustedPublisher);
            
            //find
            var cert1 = CertUtil.GetBySerial(serial);
            cert1.Should().NotBeNull("Failed to add");
            cert1.SerialNumber.Should().BeEquivalentTo(serial);

            //remove
            CertUtil.RemoveCertificate(serial);
            CertUtil.RemoveCertificate(serial, System.Security.Cryptography.X509Certificates.StoreName.TrustedPublisher);

            //find-not
            var cert2 = CertUtil.GetBySerial(serial);
            cert2.Should().BeNull("Failed to remove");
        }

        [Test]
        public void CanNormalizeSerial()
        {
            var input = "  54 03 4b e4 23 43 48 4A 50 c3 9A   00 d4 3c e1 cb  ";
            var expected = "54034be42343484A50c39A00d43ce1cb";
            var actual = CertUtil.NormalizeSerialString(input);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ShouldNormalizedNullSerialToEmptyString()
        {
            string input = null;
            var expected = "";
            var actual = CertUtil.NormalizeSerialString(input);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}