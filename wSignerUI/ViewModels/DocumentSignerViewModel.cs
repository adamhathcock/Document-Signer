using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using wSigner;

namespace wSignerUI
{
    public class DocumentSignerViewModel:ViewModelBase
    {
        private X509Certificate2 _activeCert;
        private ObservableCollection<CertInfo> _certInfos;

        public ObservableCollection<CertInfo> Certs
        {
            get
            {
                return _certInfos ?? (_certInfos = new ObservableCollection<CertInfo>(LoadCerts()));
            }
        }

        public IEnumerable<CertInfo> LoadCerts()
        {
            return CertificateUtil.GetAll(xCert => new CertInfo
            {
                Issuer = xCert.Issuer,
                Serial = xCert.SerialNumber,
                Subject = xCert.Subject,
                ValidAfter = xCert.NotBefore,
                ValidBefore = xCert.NotAfter
            });
        }

        public void ReloadCerts()
        {
            _certInfos.Clear();
            foreach(var certInfo in LoadCerts())
            {
                _certInfos.Add(certInfo);
            }
        }

        public X509Certificate2 ActiveCert
        {
            get
            {
                return _activeCert ?? 
                        (_activeCert = SelectedCert != null 
                                        && !String.IsNullOrEmpty(SelectedCert.Serial)
                                        ? CertificateUtil.GetBySerial(SelectedCert.Serial)
                                        : null);
            }
        }

        private CertInfo _certInfo;
        public CertInfo SelectedCert
        {
            get { return _certInfo; }
            set
            {
                if (_certInfo != value)
                {
                    _certInfo = value;
                    _activeCert = null;
                    //TODO:update jobs' can execute commands
                }
            }
        }

        public ObservableCollection<SignJobViewModel> Jobs { get; set; }

        public void AddFiles(params string[] files)
        {
            foreach (var file in files)
            {
                var job = new SignJobViewModel(file);
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
                
                if (job.IsReady)
                {
                    job.Sign.Execute(null);
                }
            }
        }
    }
}