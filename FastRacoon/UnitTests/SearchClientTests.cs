using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace UnitTests
{
    public class SearchClientTests
    {
        [Fact]
        public void BasicSearch()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json");

            IConfigurationRoot configuration = builder.Build();

            SearchServiceClient serviceClient = CreateSearchServiceClient(configuration);

            // Test Case 1 - the file name, URL, size, and last modified date of all documents that include "New York" (there should be 18)
            var indexName = configuration["SearchIndexName"];
            var searchIndex = serviceClient.Indexes.GetClient(indexName);

            var searchIndexClient = CreateSearchIndexClient(indexName, configuration);
            SearchParameters parameters =
                new SearchParameters()
                {
                    SearchFields = new[] { "content", "file_name" },
                    Select = new[] { "file_name", "url", "last_modified"}
                };

           // results = searchIndexClient.Documents.Search<Hotel>(indexName, parameters);

        }
        private static SearchServiceClient CreateSearchServiceClient(
            IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            return serviceClient;
        }

        private static SearchIndexClient CreateSearchIndexClient(string indexName, IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string queryApiKey = configuration["SearchServiceQueryApiKey"];

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            return indexClient;
        }
    }
}
