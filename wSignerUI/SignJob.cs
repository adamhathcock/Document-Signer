using System.ComponentModel;
using System.IO;
using System.Linq;

namespace wSignerUI
{
    public class SignJob : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChanged(string propName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }

        private string[] _docsToSign;
        public string[] DocsToSign
        {
            get { return _docsToSign; }
            set 
            { 
                _docsToSign = value;
                FirePropertyChanged("StatusText");
                FirePropertyChanged("HasNotSelected");
                FirePropertyChanged("HasSelected");
                FirePropertyChanged("IsPDF");
                FirePropertyChanged("IsOOXML");
                FirePropertyChanged("IsBoth");
                FirePropertyChanged("IsNeither");
                FirePropertyChanged("Count");
            }

        }

        public string StatusText
        {
            get
            {
                return HasSelected
                        ? IsNeither
                           ? "Document(s) not supported"
                           : Count > 1
                                ? "Sign " + Path.GetFileName(_docsToSign[0])
                                : "Sign " + _docsToSign.Length + "documents"
                        : "Drop your pdf, docx, xlsx, or pptx documents here to digitally sign it.";
            }
        }

        public bool IsPDF
        {
            get
            {
                return DocsToSign!=null && DocsToSign.Any(f => f != null && f.EndsWith(".pdf", System.StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public bool IsOOXML
        {
            get
            {
                return DocsToSign!=null &&  DocsToSign.Any(f => f != null && (
                    f.EndsWith(".xlsx", System.StringComparison.InvariantCultureIgnoreCase)
                    || f.EndsWith(".pptx", System.StringComparison.InvariantCultureIgnoreCase)
                    || f.EndsWith("docx", System.StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        public bool HasNotSelected
        {
            get { return DocsToSign == null; }
        }

        public bool HasSelected
        {
            get { return DocsToSign != null; }
        }

        public bool IsBoth
        {
            get { return HasSelected && (IsPDF && IsOOXML); }
        }

        public bool IsNeither { get { return HasSelected && !(IsPDF || IsOOXML); } }

        public int Count
        {
            get { return HasSelected ? DocsToSign.Length : 0; }
        }
    }
}