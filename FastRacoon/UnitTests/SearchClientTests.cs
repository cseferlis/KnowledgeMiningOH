using Challenge2Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
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

             var indexName = configuration["SearchIndexName"];
            var searchIndex = serviceClient.Indexes.GetClient(indexName);

            var searchIndexClient = CreateSearchIndexClient(indexName, configuration);
            var parameters =
                new SearchParameters()
                {
                    SearchFields = new[] { "Content", "FileName", "Url" },
                    Select = new[] { "FileName", "Url", "LastModified"}
                };

            // Test Case 1 - the file name, URL, size, and last modified date of all documents that include "New York" (there should be 18)
            parameters.Filter = "\"New York\"";
            var docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>(indexName, parameters);
            var count = docSearchResult.Results.Count;
            Assert.True(count == 18);

            // Test Case 2 - Document details based on multiple search terms - for example, details of all documents that include "London" and "Buckingham Palace" (there should be 2).
            parameters.Filter = "London +\"Buckingham Palace\"";
            docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>(indexName, parameters);
            count = docSearchResult.Results.Count;
            Assert.True(count == 2);

            // Test Case 3 - Filtering based on specific fields - for example, all documents that contain the term "Las Vegas" that have "reviews" in their URL (there should be 13)
            parameters.Filter = "content:\"Las Vegas\" AND url:reviews";
            docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>(indexName, parameters);
            count = docSearchResult.Results.Count;
            Assert.True(count == 13);

            // Test Case 4 - and all documents containing the term "Las Vegas" that that do not have "reviews" in their URL (there should be 2).
            parameters.Filter = "content:\"Las Vegas\" NOT url:reviews";
            docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>(indexName, parameters);
            count = docSearchResult.Results.Count;
            Assert.True(count == 2);

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
