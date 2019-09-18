using Challenge2Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace UnitTests
{
    public class SearchClientTests
    {

        [Fact]
        public void CreateDataSource()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json").Build();

            using (var serviceClient = CreateSearchServiceClient(configuration))
            {
                InternalCreateDataSource(configuration, serviceClient);
            }
        }

        [Fact]
        public void CreateIndexer()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json").Build();

            using (var serviceClient = CreateSearchServiceClient(configuration))
            {
                CreateBlobIndexer(serviceClient, configuration["SearchIndexName"]);
            }
        }

        [Fact]
        public void BasicSearch()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json").Build();

            var searchIndexClient = CreateSearchIndexClient(
                    configuration["SearchIndexName"], configuration);
            var parameters =
                new SearchParameters()
                {
                    SearchFields = new[] { "Content", "FileName", "Url" },
                    Select = new[] { "FileName", "Url", "LastModified", "Bytes" },

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

        [Fact]
        public void CreateSkillset()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                                                .AddJsonFile("appsettings.json").Build();

            var inputMappings = new List<InputFieldMappingEntry>
            {
                new InputFieldMappingEntry(
                name: "text",
                source: "/document/Content")
            };

            var outputMappings = new List<OutputFieldMappingEntry>
            {
                new OutputFieldMappingEntry(name: "persons", "Persons"),
                new OutputFieldMappingEntry(name: "locations", "Locations"),
                new OutputFieldMappingEntry(name: "urls", "Urls"),
            };

            var entityCategory = new List<EntityCategory>()
                { EntityCategory.Location, EntityCategory.Person, EntityCategory.Url };

            var entityRecognitionSkill = new EntityRecognitionSkill(
                description: "Recognize organizations",
                context: "/document",
                inputs: inputMappings,
                outputs: outputMappings,
                categories: entityCategory,
                defaultLanguageCode: EntityRecognitionSkillLanguage.En);

            var keyPhraseSkill = new KeyPhraseExtractionSkill(
                name: "keyphraseextractionskill",
                description: "Key Phrase Extraction Skill",
                context: "/document",
                inputs: new[] { new InputFieldMappingEntry("text", "/document/Content") },
                outputs: new[] { new OutputFieldMappingEntry("keyPhrases", "KeyPhrases") }
                );

            var imageSkill = new ImageAnalysisSkill(
                name: "imageanalysisskill",
                context: "/document/normalized_images/*",
                inputs: new[] { new InputFieldMappingEntry("image", "/document/normalized_images/*") },
                outputs: new[] {
                        new OutputFieldMappingEntry("/document/normalized_images/*/tags/*","Tags"),
                        new OutputFieldMappingEntry("/document/normalized_images/*/description","Description")
                    }
                );

            var ocrSkill = new OcrSkill(
                name: "ocrskill",
                context: "/document/normalized_images/*",
                inputs: new[] { new InputFieldMappingEntry("image", "/document/normalized_images/*") },
                outputs: new[] { new OutputFieldMappingEntry("text", "OcrText") }
                );

            var mergeTextSkill = new MergeSkill(
                name: "mergeTextSkill",
                context: "/document",
                inputs: new[] {
                    new InputFieldMappingEntry("text", "/document/Content"),
                    new InputFieldMappingEntry("itemsToInsert", "/document/normalized_images/*/OcrText")
                },
                outputs: new[] {
                    new OutputFieldMappingEntry("mergedText", "MergedText")
                }
                );

            /*Microsoft.Rest.Azure.CloudException : One or more skills are invalid. Details: Skill 'mergeTextSkill' is not allowed to have recursively defined inputs
            */

            //public SentimentSkill(IList<InputFieldMappingEntry> inputs, IList<OutputFieldMappingEntry> outputs, string name = null, string description = null, string context = null, SentimentSkillLanguage? defaultLanguageCode = null);
            var sentimentskill = new SentimentSkill(
                name: "sentimentskill",
                description: "Our favorite Sentiment Skill",
                context: "/document",
                defaultLanguageCode: SentimentSkillLanguage.En,
                inputs: new[] { new InputFieldMappingEntry("text", "/document/Content") },
                outputs: new[] { new OutputFieldMappingEntry("score", "Sentiment") }
                );

            var ss = new Skillset("fastracoontravelskillset", "self describing",
                skills: new List<Skill>() { entityRecognitionSkill, keyPhraseSkill, sentimentskill,
                    ocrSkill, mergeTextSkill},
                cognitiveServices: new CognitiveServicesByKey(configuration["CogServicesKey"])
                );

            using (var serviceClient = CreateSearchServiceClient(configuration))
            {
                serviceClient.Skillsets.CreateOrUpdate(ss);
            }
        }

        [Fact]
        public void CreateIndex()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            using (var serviceClient = CreateSearchServiceClient(configuration))
            {
                Challenge2Client.Program.CreateIndex(
                    indexName: configuration["SearchIndexName"],
                    serviceClient: serviceClient);
            }
        }

        private static void CreateBlobIndexer(SearchServiceClient serviceClient, string indexName)
        {
            Indexer travelBlobIndexer = new Indexer()
            {
                Name = "travelblobindexer",
                DataSourceName = "travelblobs",
                TargetIndexName = indexName,
                FieldMappings = new[]
                {
                    new FieldMapping("metadata_storage_name", "FileName"),
                    new FieldMapping("metadata_storage_path", "Url"),
                    new FieldMapping("metadata_storage_last_modified", "LastModified"),
                    new FieldMapping("metadata_storage_size", "Bytes"),
                },
                OutputFieldMappings = new[]
                {
                    new FieldMapping("/document/Persons", "Persons"),
                    new FieldMapping("/document/Locations", "Locations"),
                    new FieldMapping("/document/Urls", "Urls"),
                    new FieldMapping("/document/KeyPhrases", "KeyPhrases"),
                    new FieldMapping("/document/Sentiment","Sentiment"),
                    new FieldMapping("/document/normalized_images/*/OcrText", "OcrText"),
                    new FieldMapping("/document/MergedText", "MergedText")
                }
                ,
                SkillsetName = "fastracoontravelskillset",
                Parameters = new IndexingParameters
                {
                    Configuration = new Dictionary<string, object>()
                    {
                        { "dataToExtract", "contentAndMetadata" },
                        { "imageAction", "generateNormalizedImages" }
                    }
                }
            };

            serviceClient.Indexers.CreateOrUpdate(travelBlobIndexer);
        }

        private static void InternalCreateDataSource(IConfigurationRoot configuration, SearchServiceClient serviceClient)
        {
            DataSource ds = new DataSource()
            {
                Name = "travelblobs",
                Type = DataSourceType.AzureBlob,
                Container = new DataContainer("documents"),
                Credentials = new DataSourceCredentials(configuration["storageconnection"])
            };
            serviceClient.DataSources.CreateOrUpdate(ds);
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
