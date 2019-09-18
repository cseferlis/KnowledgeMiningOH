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
        List<string> persons = new List<string>();
        List<string> urls = new List<string>();
        List<string> locations = new List<string>();
    }
}
