using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Challenge2Client
{
    public class SearchConfiguration
    {
        public string SearchServiceName { get; set; }
        public string SearchServiceAdminApiKey { get; set; }
        public string SearchIndexName { get; set; }
        public string DataSource { get; set; }
        public string CogServicesKey { get; set; }
    }
}
