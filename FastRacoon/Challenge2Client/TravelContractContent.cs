using System;
using System.Collections.Generic;
using System.Text;

namespace Challenge2Client
{
    class TravelContractContent
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
        public int Bytes { get; set; }
        public DateTime LastModified { get; set; }
    }
}
