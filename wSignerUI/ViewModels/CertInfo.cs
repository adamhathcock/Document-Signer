using System;

namespace wSignerUI
{
    public class CertInfo
    {
        public string Title { get; set; }

        public string Subject {get;set;}

        public string Issuer {get;set;}

        public DateTime ValidBefore {get;set;}

        public DateTime ValidAfter {get; set; }

        public string Serial {get;set;}

        public bool IsValid
        {
            get
            {
                var now = DateTime.Now;
                return ValidAfter < now && now < ValidBefore;
            }
        }
    }
}