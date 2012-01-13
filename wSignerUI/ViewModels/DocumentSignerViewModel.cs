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

        public ObservableCollection<CertInfo> Certs { get; private set; }

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
                FirePropertyChanged(() => SelectedCertInfo);
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
            var allCerts = CertUtil.GetAll(xCert => new CertInfo
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

        private X509Certificate2 _activeCert;

        internal X509Certificate2 ActiveCert
        {
            get
            {
                var selection = SelectedCertInfo;
                if(_activeCert == null || (selection != null && _activeCert.SerialNumber != selection.Serial))
                {
                    _activeCert = CertUtil.GetBySerial(selection.Serial);
                }
                return _activeCert;
            }
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
                job.Close = new RelayCommand(x => Jobs.Remove(job));
                job.Sign = new RelayCommand(x =>
                {
                    var signTask = Task.Factory
                    .StartNew(() =>
                    {
                        
                        Thread.Sleep(100);
                        job.State = SignJobState.Signing;
                        DocumentSigner.For(job.FileType)
                                        .Sign(job.InputFile, job.OutputFile, ActiveCert);
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
                });
                Jobs.Add(job);
                if (job.IsReady)
                {
                    job.Sign.Execute(null);
                }
            }
        }
    }
}