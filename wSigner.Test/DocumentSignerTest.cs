using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace wSigner.Test
{
    [TestFixture]
    public class DocumentSignerTest
    {
        private string _basePath, _serial;
        
        [SetUp]
        public void Setup()
        {
            //NOTE:this test is not meant to be fully automated, install the corresponding certificate and prepare the sample documents before running
            _basePath = Environment.CurrentDirectory;
            _serial = "54 03 4b e4 23 43 48 4a 50 c3 9a 00 d4 3c e1 cb";
            var outputDir = Path.Combine(_basePath, "output");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
        }

        [Test]
        public void CanSignPdf()
        {
            var input = Path.Combine(_basePath, "input", "input.pdf");
            var output = Path.Combine(_basePath, "output", "output.pdf");
            CanSignX(input, output, _serial);
        }

        [Test]
        public void CanSignXlsx()
        {
            var input = Path.Combine(_basePath, "input", "input.xlsx");
            var output = Path.Combine(_basePath, "output", "output.xlsx");
            CanSignX(input, output, _serial);
        }

        [Test]
        public void CanSignPptx()
        {
            var input = Path.Combine(_basePath, "input", "input.pptx");
            var output = Path.Combine(_basePath, "output", "output.pptx");
            CanSignX(input, output, _serial);
        }

        [Test]
        public void CanSignDocx()
        {
            var input = Path.Combine(_basePath, "input", "input.docx");
            var output = Path.Combine(_basePath, "output", "output.docx");
            CanSignX(input, output, _serial);
        }

        private static void CanSignX(string input, string output, string serial)
        {
            if (File.Exists(output))
            {
                File.Delete(output);
            }
            
            File.Exists(output).Should().BeFalse();
            var signer = DocumentSigner.For(input);
            signer.Sign(input, output, CertificateUtil.GetBySerial(serial, false));
            File.Exists(output).Should().BeTrue();
            signer.Verify(File.OpenRead(output), serial).Should().BeTrue();
        }
    }
}