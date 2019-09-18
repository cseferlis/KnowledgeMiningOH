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

            DataSource ds = new DataSource()
            {
                Name = "travelblobs",
                Type = DataSourceType.AzureBlob,
                Container = new DataContainer("documents"),
                Credentials = new DataSourceCredentials("DefaultEndpointsProtocol=https;AccountName=margiesdocumentrepo;AccountKey=VzpvLxQAFzlQJqfogFA8b4INBZh6t9aExqwQrjTFYeHET+ydKaMTcIYp890JilEVG+oXPqruFsWNA9vsVo6d9g==;EndpointSuffix=core.windows.net")
            };
            serviceClient.DataSources.CreateOrUpdate(ds);

            Indexer travelBlobIndexer = new Indexer()
            {
                Name = "travelblobindexer",
                DataSourceName = "travelblobs",
                TargetIndexName = indexName,
                FieldMappings = new[]
                {
                    new FieldMapping("metadata_storage_name","FileName"),
                    new FieldMapping("metadata_storage_path","Url"),
                    new FieldMapping("metadata_storage_last_modified","LastModified"),
                    new FieldMapping("metadata_storage_size","Bytes")
                }
                ,
            };

            serviceClient.Indexers.CreateOrUpdate(travelBlobIndexer);

            var searchIndexClient = CreateSearchIndexClient(indexName, configuration);
            var parameters =
                new SearchParameters()
                {
                    SearchFields = new[] { "Content", "FileName", "Url" },
                    Select = new[] { "FileName", "Url", "LastModified", "Bytes"},

                };

            // Test Case 1 - the file name, URL, size, and last modified date of all documents that include "New York" (there should be 18)
            var docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>("\"New York\"", parameters);
            var count = docSearchResult.Results.Count;
            Assert.True(count == 18);

            // Test Case 2 - Document details based on multiple search terms - for example, details of all documents that include "London" and "Buckingham Palace" (there should be 2).
            docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>("London +\"Buckingham Palace\"", parameters);
            count = docSearchResult.Results.Count;
            Assert.True(count == 2);

            // Test Case 3 - Filtering based on specific fields - for example, all documents that contain the term "Las Vegas" that have "reviews" in their URL (there should be 13)
            parameters.Filter = "search.ismatch('reviews', 'Url')";
            docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>("\"Las Vegas\"", parameters);
            count = docSearchResult.Results.Count;
            Assert.True(count == 13);

            // Test Case 4 - and all documents containing the term "Las Vegas" that that do not have "reviews" in their URL (there should be 2).
            parameters.Filter = "not search.ismatch('reviews', 'Url')";
            docSearchResult = searchIndexClient.Documents.Search<TravelContractContent>("\"Las Vegas\"", parameters);
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
