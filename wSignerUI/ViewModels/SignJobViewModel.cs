using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace wSignerUI
{
    public class SignJobViewModel:ViewModelBase
    {
        private static string[] SupportedExts = new[] {".pdf", ".docx", ".xlsx", ".pptx"};

        public SignJobViewModel(string inputFile)
        {
            var dir = Path.GetDirectoryName(inputFile);
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var ext = Path.GetExtension(inputFile);

            InputFile = inputFile;
            OutputFile = Path.Combine(dir, fileName + "-signed" + ext);
            FileType = Path.GetExtension(inputFile);

            if (SupportedExts.Any(validExt => validExt.Equals(FileType, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                State = SignJobState.Pending;
            }
            else
            {
                State = SignJobState.Invalid;
            }

            Icon = Utils.GetBitmapSourceIconFromFile(inputFile);

            Open = new RelayCommand(
                        o => Process.Start(new ProcessStartInfo { FileName = OutputFile, UseShellExecute = true }), 
                        o => IsSuccessful);
        }

        public SignJobState State { get; set; }

        public BitmapSource Icon { get; set; }

        public string InputFile { get; private set; }

        public string OutputFile { get; private set; }

        public string FileType { get; set; }

        public string Description { get; set; }

        public string Error { get; set; }

        public bool IsBusy { get { return State == SignJobState.Pending || State == SignJobState.Signing; } }

        public bool IsCompleted { get { return State == SignJobState.Signed || State == SignJobState.Failed; } }

        public bool IsSuccessful { get { return State == SignJobState.Signed; } }

        public bool IsFaulted { get { return State == SignJobState.Failed; } }

        public bool IsInvalid { get { return State == SignJobState.Invalid; } }

        public bool IsReady { get { return State == SignJobState.Pending; } }

        public RelayCommand Open { get; set; }

        public RelayCommand Sign { get; set; }

        public RelayCommand Close { get; set; }

        
    }
}