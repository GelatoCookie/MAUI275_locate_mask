using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiRfidSample.MVVM.Models
{
    public class CertificateModel
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string CertificateType { get; set; }
        public string CertType { get; set; }

        public FileResult FileResult { get; set; }
    }
}
