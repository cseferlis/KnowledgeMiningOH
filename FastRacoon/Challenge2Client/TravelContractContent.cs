using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace Challenge2Client
{
    class TravelContractContent
    {
        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterableAttribute]
        public string Id { get; set; }
        [IsSortable, IsSearchable, IsFilterable]
        public string Url { get; set; }
        [IsSortable, IsSearchable, IsFilterable]
        public string FileName { get; set; }
        [IsSearchable]
        public string Content { get; set; }
        public int Bytes { get; set; }
        public DateTime LastModified { get; set; }

        public List<string> Persons { get; set; } = new List<string>();
        public List<string> Urls { get; set; } = new List<string>();
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> KeyPhrases { get; set; } = new List<string>();
        public double Sentiment { get; set; }
    }
}
