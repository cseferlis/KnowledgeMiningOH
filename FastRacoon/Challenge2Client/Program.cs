﻿using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;

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
                SearchServiceName = "fastracoonsearch"
            };

            SearchServiceClient serviceClient = CreateSearchServiceClient(configuration);

            string indexName = configuration.SearchIndexName;

            Console.WriteLine("{0}", "Deleting index...\n");
            DeleteIndexIfExists(indexName, serviceClient);

            Console.WriteLine("{0}", "Creating index...\n");
            CreateIndex(indexName, serviceClient);

            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            //Console.WriteLine("{0}", "Uploading documents...\n");
            //UploadDocuments(indexClient);

            ISearchIndexClient indexClientForQueries = CreateSearchIndexClient(indexName, configuration);

            // ToDo: Create Indexer
            // here!!!!

            //RunQueries(indexClientForQueries);

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

        private static void DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }
        }

        private static void CreateIndex(string indexName, SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<TravelContractContent>()
            };

            serviceClient.Indexes.Create(definition);
        }

        //private static void RunQueries(ISearchIndexClient indexClient)
        //{
        //    SearchParameters parameters;
        //    DocumentSearchResult<Hotel> results;

        //    Console.WriteLine("Search the entire index for the term 'motel' and return only the HotelName field:\n");

        //    parameters =
        //        new SearchParameters()
        //        {
        //            Select = new[] { "HotelName" }
        //        };

        //    results = indexClient.Documents.Search<Hotel>("motel", parameters);

        //    WriteDocuments(results);

        //    Console.Write("Apply a filter to the index to find hotels with a room cheaper than $100 per night, ");
        //    Console.WriteLine("and return the hotelId and description:\n");

        //    parameters =
        //        new SearchParameters()
        //        {
        //            Filter = "Rooms/any(r: r/BaseRate lt 100)",
        //            Select = new[] { "HotelId", "Description" }
        //        };

        //    results = indexClient.Documents.Search<Hotel>("*", parameters);

        //    WriteDocuments(results);

        //    Console.Write("Search the entire index, order by a specific field (lastRenovationDate) ");
        //    Console.Write("in descending order, take the top two results, and show only hotelName and ");
        //    Console.WriteLine("lastRenovationDate:\n");

        //    parameters =
        //        new SearchParameters()
        //        {
        //            OrderBy = new[] { "LastRenovationDate desc" },
        //            Select = new[] { "HotelName", "LastRenovationDate" },
        //            Top = 2
        //        };

        //    results = indexClient.Documents.Search<Hotel>("*", parameters);

        //    WriteDocuments(results);

        //    Console.WriteLine("Search the entire index for the term 'hotel':\n");

        //    parameters = new SearchParameters();
        //    results = indexClient.Documents.Search<Hotel>("hotel", parameters);

        //    WriteDocuments(results);
        //}

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
