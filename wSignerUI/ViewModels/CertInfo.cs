using System;

namespace wSignerUI
{
    public class CertInfo
    {
        public string Subject {get;set;}

        public string Issuer {get;set;}

        public DateTime ValidBefore {get;set;}

        public DateTime ValidAfter {get; set; }

        public string Serial {get;set;}
    }
}