using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace wSignerUI
{
    public class SignJobViewModel:ViewModelBase
    {
        private static string[] SupportedExts = new[] { ".pdf", ".docx", ".xlsx", ".pptx" };

        public SignJobViewModel(string inputFile)
        {
            if(String.IsNullOrEmpty(inputFile) 
                || !File.Exists(inputFile) 
                || !SupportedExts.Any(validExt=>inputFile.EndsWith(validExt,StringComparison.InvariantCultureIgnoreCase)))
            {
                State = SignJobState.Invalid;
                return;
            }

            InputFile = inputFile;
            InputFileName = Path.GetFileName(inputFile);

            var dir = Path.GetDirectoryName(inputFile);
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var ext = Path.GetExtension(inputFile);
            OutputFile = Path.Combine(dir, fileName + "-signed" + ext);
            FileType = ext;
            
            //TODO: may have to try catch this, and use a backup icon on failure
            Icon = Utils.GetBitmapSourceIconFromFile(inputFile);
            OpenOutput = new RelayCommand(
                    o => Process.Start(new ProcessStartInfo { FileName = OutputFile, UseShellExecute = true }));
            OpenInput = new RelayCommand(o => Process.Start(new ProcessStartInfo { FileName = InputFile, UseShellExecute = true }));

            State = SignJobState.Pending;
        }

        private SignJobState _state;
        public SignJobState State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    FirePropertyChanged(() => State);
                    FirePropertyChanged(() => Description);
                    FirePropertyChanged(() => Error);
                    FirePropertyChanged(() => IsBusy);
                    FirePropertyChanged(() => IsFree);
                    FirePropertyChanged(() => IsCompleted);
                    FirePropertyChanged(() => IsSuccessful);
                    FirePropertyChanged(() => IsFaulted);
                    FirePropertyChanged(() => IsReady);
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public BitmapSource Icon { get; set; }

        public string InputFile { get; set; }

        public string InputFileName { get; set; }

        public string OutputFile { get; set; }

        public string FileType { get; set; }

        public string Description { get; set; }

        public string Error { get; set; }

        public bool IsBusy { get { return State == SignJobState.Signing; } }

        public bool IsCompleted { get { return State == SignJobState.Signed || State == SignJobState.Failed; } }

        public bool IsSuccessful { get { return State == SignJobState.Signed; } }

        public bool IsFaulted { get { return State == SignJobState.Failed; } }

        public bool IsInvalid { get { return State == SignJobState.Invalid; } }

        public bool IsReady { get { return State == SignJobState.Pending; } }

        public bool IsFree { get { return State != SignJobState.Signing; } }

        public RelayCommand OpenOutput { get; set; }

        public RelayCommand OpenInput { get; set; }

        public RelayCommand Sign { get; set; }

        public RelayCommand Close { get; set; }
    }
}