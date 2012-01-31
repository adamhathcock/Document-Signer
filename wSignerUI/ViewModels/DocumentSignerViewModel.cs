using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wSigner;

namespace wSignerUI
{
    public class DocumentSignerViewModel:ViewModelBase
    {
        public DocumentSignerViewModel()
        {
            Jobs = new ObservableCollection<SignJobViewModel>();
            ActiveCert = !String.IsNullOrEmpty(LastCertSerial)
                            ? CertUtil.GetBySerial(LastCertSerial) ?? CertUtil.GetAll(x => x, requirePrivateKey:true).FirstOrDefault()
                            : CertUtil.GetAll(x => x, requirePrivateKey: true).FirstOrDefault();
            ShowCertsDialog = new RelayCommand(x => AskUserForCert());
            ShowActiveCert = new RelayCommand(x => DisplayActiveCert(), x => ActiveCert != null);
        }

        //TODO: add LoadCertFromFile
        public ICommand ShowCertsDialog { get; set; }

        public ICommand ShowActiveCert { get; set; }

        internal X509Certificate2 ActiveCert { get; private set; }

        public ObservableCollection<SignJobViewModel> Jobs { get; set; }

        //TODO:remember the serial of the cert used last time (registry, maybe?)
        public string LastCertSerial { get; set; }

        public bool HasNoCert
        {
            get
            {
                return ActiveCert == null;
            }
        }

        public bool HasCert
        {
            get
            {
                return ActiveCert != null;
            }
        }

        public string CertSubject
        {
            get
            {
                var cert = ActiveCert;
                return cert != null 
                        ? String.IsNullOrWhiteSpace(cert.FriendlyName)
                            ? cert.GetNameInfo(X509NameType.SimpleName, false)
                            : cert.FriendlyName
                        : String.Empty;
            }
        }

        public string CertExpiryDate
        {
            get
            {
                return ActiveCert != null ? ActiveCert.NotAfter.ToShortDateString() : String.Empty;
            }
        }

        public bool CertHasExpired
        {
            get
            {
                return HasCert && DateTime.Now > ActiveCert.NotAfter;
            }
        }

        public bool CertHasError
        {
            get
            {
                return !String.IsNullOrEmpty(CertError);
            }
        }

        private string _certError;
        public string CertError
        {
            get
            {
                var cert = ActiveCert;
                if (_certError == null && cert != null)
                {
                    try
                    {
                        if (cert.Verify())
                        {
                            _certError = string.Empty;
                        }
                        else
                        {
                            _certError = "Invalid for unknown reason";
                        }
                    }
                    catch (CryptographicException cryptoEx)
                    {
                        _certError = cryptoEx.Message;
                    }
                }
                else
                {
                    _certError = string.Empty;
                }
                return _certError;
            }
            set { _certError = value; }
        }

        public void DisplayActiveCert()
        {
            var cert = ActiveCert;
            if (cert != null)
            {
                X509Certificate2UI.DisplayCertificate(cert);
            }
        }

        public void AskUserForCert()
        {
            var newCert = CertUtil.GetByDialog(requirePrivateKey: true);
            if (newCert != null)
            {
                ActiveCert = newCert;
                CertError = null;
                FirePropertyChanged(() => HasCert);
                FirePropertyChanged(() => HasNoCert);
                FirePropertyChanged(() => CertError);
                FirePropertyChanged(() => CertHasExpired);
                FirePropertyChanged(() => CertExpiryDate);
                FirePropertyChanged(() => CertSubject);
            }
        }

        public void AddFiles(params string[] files)
        {
            foreach (var file in files)
            {
                var job = new SignJobViewModel(file);
                if (job.IsInvalid)
                {
                    //TODO:explain why, don't just fail so silently
                    continue;
                }
                job.Close = new RelayCommand(x => Jobs.Remove(job));
                job.Sign = new RelayCommand(x =>
                {
                    var cert = ActiveCert;
                    if (cert != null)
                    {
                        var signTask = Task.Factory.StartNew(() =>
                        {
                            job.State = SignJobState.Signing;
                            DocumentSigner.For(job.FileType)
                                .Sign(job.InputFile, job.OutputFile, cert);
                        });

                        signTask.ContinueWith(task =>
                        {
                            job.Error = task.Exception.InnerException.Message;
                            job.State = SignJobState.Failed;
                        }, TaskContinuationOptions.OnlyOnFaulted);

                        signTask.ContinueWith(task =>
                        {
                            job.State = SignJobState.Signed;
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    }
                });
                
                Jobs.Add(job);
                if (job.IsReady)
                {
                    job.Sign.Execute(null);
                }
            }
        }

        public static void SignFile(string file)
        {
            var signer = DocumentSigner.For(file);
            if (signer != null)
            {
                var cert = CertUtil.GetByDialog(requirePrivateKey: true);
                if (cert != null)
                {
                    var ext = Path.GetExtension(file);
                    var bareFileName = Path.GetFileNameWithoutExtension(file);
                    signer.Sign(file, bareFileName + "-signed" + ext, cert);
                }
            }
            else
            {
                MessageBox.Show("Only documents of type pdf, docx, xlsx or pptx can be digitally signed");
            }
        }
    }
}