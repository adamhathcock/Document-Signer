using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace wSigner.Test
{
    /// <summary>
    /// NOTE: these tests depends on CertUtil.GetFromFile() runs correctly
    /// </summary>
    [TestFixture]
    public class DocumentSignerTest
    {
        private string  _certPath       = Path.Combine(Environment.CurrentDirectory, "input", "test-cert.pfx"), 
                        _certPassword   = "1234",
                        _inputPath      = Path.Combine(Environment.CurrentDirectory, "input"),
                        _outputPath     = Path.Combine(Environment.CurrentDirectory, "output");
        
        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
        }

        [Test]
        [TestCase("signed.pdf", "74 fd 4c 4b c0 90 a3 0e d4 f0")]
        [TestCase("signed.xlsx", "74 fd 4c 4b c0 90 a3 0e d4 f0")]
        [TestCase("signed.docx", "74 fd 4c 4b c0 90 a3 0e d4 f0")]
        [TestCase("signed.pptx", "74 fd 4c 4b c0 90 a3 0e d4 f0")]
        public void CanVerifyX(string file, string serial)
        {
            file = Path.Combine(_inputPath, file);
            var signer = DocumentSigner.For(file);
            signer.Verify(File.OpenRead(file), serial).Should().BeTrue();
        }

        [Test]
        [TestCase("input.docx", "output.docx")]
        [TestCase("input.xlsx", "output.xlsx")]
        [TestCase("input.pptx", "output.pptx")]
        [TestCase("input.pdf", "output.pdf")]
        [TestCase("signed.pdf", "signed2.pdf")]
        [TestCase("signed.xlsx", "signed2.xlsx")]
        [TestCase("signed.docx", "signed2.docx")]
        [TestCase("signed.pptx", "signed2.pptx")]
        public void CanSignX(string inputName, string outputName)
        {
            var input = Path.Combine(_inputPath, inputName);
            var output = Path.Combine(_outputPath, outputName);

            if (File.Exists(output))
            {
                File.Delete(output);
            }

            File.Exists(output).Should().BeFalse();
            var signer = DocumentSigner.For(input);
            var cert = CertUtil.GetFromFile(_certPath, _certPassword);
            var serial = cert.SerialNumber;
            signer.Sign(input, output, cert);
            File.Exists(output).Should().BeTrue();
            CanVerifyX(output, serial);
        }
    }
}