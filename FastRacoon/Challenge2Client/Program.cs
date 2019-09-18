using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Rest.Azure;
using System;
using System.Net;

namespace Challenge2Client
{
    class Program
    {
        // This sample shows how to delete, create, upload documents and query an index
        static void Main(string[] args)
        {
            SearchConfiguration configuration = new SearchConfiguration
            {
                SearchIndexName = "travelcontractindex",
                SearchServiceAdminApiKey = "C7748B5DE38F1579AB6778141A44DADD",
                SearchServiceName = "fastracoonsearch",
                DataSource = "travelblobs"
            };

            SearchServiceClient serviceClient = CreateSearchServiceClient(configuration);

            string indexName = configuration.SearchIndexName;

            DataSource ds = new DataSource()
            {
                Name = "travelblobs",
                Type = DataSourceType.AzureBlob,
                Container = new DataContainer("documents"),
                Credentials = new DataSourceCredentials("DefaultEndpointsProtocol=https;AccountName=margiesdocumentrepo;AccountKey=VzpvLxQAFzlQJqfogFA8b4INBZh6t9aExqwQrjTFYeHET+ydKaMTcIYp890JilEVG+oXPqruFsWNA9vsVo6d9g==;EndpointSuffix=core.windows.net")
            };
            serviceClient.DataSources.CreateOrUpdate(ds);

            Console.WriteLine("{0}", "Creating index...\n");
            CreateIndex(indexName, serviceClient);

            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            //Console.WriteLine("{0}", "Uploading documents...\n");
            //UploadDocuments(indexClient);

            ISearchIndexClient indexClientForQueries = CreateSearchIndexClient(indexName, configuration);

            // ToDo: Create Indexer
            Indexer indexer = new Indexer(
                 name: "travel-contract-indexer",
                 dataSourceName: configuration.DataSource,
                 targetIndexName: configuration.SearchIndexName,
                 schedule: new IndexingSchedule(TimeSpan.FromDays(1)),
                 fieldMappings: new[]
                 {
                    new FieldMapping("metadata_storage_name","FileName"),
                    new FieldMapping("metadata_storage_path","Url"),
                    new FieldMapping("metadata_storage_last_modified","LastModified"),
                    new FieldMapping("metadata_storage_size","Bytes"),
                 });

            // Indexers contain metadata about how much they have already indexed 
            // If we already ran the sample, the indexer will remember that it already 
            // indexed the sample data and not run again 
            // To avoid this, reset the indexer if it exists 
            bool exists = serviceClient.Indexers.Exists(indexer.Name);

            if (exists)
            {
                serviceClient.Indexers.Reset(indexer.Name);
            }

            serviceClient.Indexers.CreateOrUpdate(indexer);
            var parameters =
                new SearchParameters()
                {
                    SearchFields = new[] { "Content", "FileName", "Url" },
                    Select = new[] { "FileName", "Url", "LastModified", "Bytes" },

                };

            // Test Case 1 - the file name, URL, size, and last modified date of all documents that include "New York" (there should be 18)
            var docSearchResult = indexClientForQueries.Documents.Search<TravelContractContent>("\"New York\"", parameters);
            var count = docSearchResult.Results.Count;

            // Test Case 2 - Document details based on multiple search terms - for example, details of all documents that include "London" and "Buckingham Palace" (there should be 2).
            docSearchResult = indexClientForQueries.Documents.Search<TravelContractContent>("London +\"Buckingham Palace\"", parameters);
            count = docSearchResult.Results.Count;

            // Test Case 3 - Filtering based on specific fields - for example, all documents that contain the term "Las Vegas" that have "reviews" in their URL (there should be 13)
            docSearchResult = indexClientForQueries.Documents.Search<TravelContractContent>("content:\"Las Vegas\" AND url:reviews", parameters);
            count = docSearchResult.Results.Count;

            // Test Case 4 - and all documents containing the term "Las Vegas" that that do not have "reviews" in their URL (there should be 2).
            docSearchResult = indexClientForQueries.Documents.Search<TravelContractContent>("content:\"Las Vegas\" NOT url:reviews", parameters);
            count = docSearchResult.Results.Count;

            Console.WriteLine("{0}", "Complete.  Press any key to end application...\n");
            Console.ReadKey();
        }

        private static SearchServiceClient CreateSearchServiceClient(SearchConfiguration configuration)
        {
            SearchServiceClient serviceClient = new SearchServiceClient(configuration.SearchServiceName, new SearchCredentials(configuration.SearchServiceAdminApiKey));
            return serviceClient;
        }

        private static SearchIndexClient CreateSearchIndexClient(string indexName, SearchConfiguration configuration)
        {
            SearchIndexClient indexClient = new SearchIndexClient(configuration.SearchServiceName, indexName, new SearchCredentials(configuration.SearchServiceAdminApiKey));
            return indexClient;
        }

        private static void CreateIndex(string indexName, SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<TravelContractContent>()
            };

            serviceClient.Indexes.CreateOrUpdate(definition);
        }

        private static void WriteDocuments(DocumentSearchResult<TravelContractContent> searchResults)
        {
            foreach (SearchResult<TravelContractContent> result in searchResults.Results)
            {
                Console.WriteLine(result.Document);
            }

            Console.WriteLine();
        }
    }
}
