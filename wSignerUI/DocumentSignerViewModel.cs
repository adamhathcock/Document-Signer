using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using wSigner;

namespace wSignerUI
{
    public class DocumentSignerViewModel:ViewModelBase
    {
        public DocumentSignerViewModel()
        {
            ReloadCerts();
            Reload = new RelayCommand(x => ReloadCerts());
            Jobs = new ObservableCollection<SignJobViewModel>();
        }

        private X509Certificate2 _activeCert;

        public ObservableCollection<CertInfo> Certs { get; private set; }

        public X509Certificate2 ActiveCert
        {
            get
            {
                return _activeCert ??
                        (_activeCert = SelectedCertInfo != null
                                        && !String.IsNullOrEmpty(SelectedCertInfo.Serial)
                                        ? CertificateUtil.GetBySerial(SelectedCertInfo.Serial)
                                        : null);
            }
        }

        private CertInfo _selectedCertInfo;
        public CertInfo SelectedCertInfo
        {
            get
            {
                return _selectedCertInfo;
            }
            set
            {
                _selectedCertInfo = value;
                _activeCert = null;
                FirePropertyChanged(() => SelectedCertInfo);
                //TODO:update jobs' can execute commands
            }
        }

        public ICommand Reload { get; set; }

        public ObservableCollection<SignJobViewModel> Jobs { get; set; }

        public void ReloadCerts()
        {
            if (Certs == null)
            {
                Certs = new ObservableCollection<CertInfo>();
            }
            Certs.Clear();
            var allCerts = CertificateUtil.GetAll(xCert => new CertInfo
                                                            {
                                                                Title = xCert.FriendlyName,
                                                                Issuer = xCert.Issuer,
                                                                Serial = xCert.SerialNumber,
                                                                Subject = xCert.Subject,
                                                                ValidAfter = xCert.NotBefore,
                                                                ValidBefore = xCert.NotAfter
                                                            }).OrderBy(ci => ci.Title);
            foreach (var certInfo in allCerts)
            {
                Certs.Add(certInfo);
            }
            SelectedCertInfo = Certs.FirstOrDefault();
        }

        public void AddFiles(params string[] files)
        {
            foreach (var file in files)
            {
                var job = new SignJobViewModel(file);
                if (job.IsInvalid)
                {
                    continue;
                }

                job.Close = new RelayCommand(x => Jobs.Remove(job), x=>!job.IsBusy);
                job.Sign = new RelayCommand(x =>
                            {
                                Task.Factory
                                    .StartNew(() =>
                                    {
                                        Thread.Sleep(50);
                                        job.State = SignJobState.Signing;
                                        DocumentSigner.For(job.FileType).Sign(job.InputFile, job.OutputFile, ActiveCert);
                                    })
                                    .ContinueWith(task =>
                                    {
                                        job.Error = task.Exception.InnerException.Message;
                                        job.State = SignJobState.Failed;
                                    }, TaskContinuationOptions.OnlyOnFaulted)
                                    .ContinueWith(task => job.State = SignJobState.Signed, TaskContinuationOptions.OnlyOnRanToCompletion);
                            }, x => job.IsReady && ActiveCert != null);
                Jobs.Add(job);
                //if (job.IsReady)
                //{
                //    job.Sign.Execute(null);
                //}
            }
        }
    }
}