using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace Challenge1Client
{
    public class Program
    {

        static void Main(string[] args)
        {

            // Represents the various elements used to create HTTP request URIs
            // for QnA Maker operations.
            // From Publish Page: HOST
            // Example: https://YOUR-RESOURCE-NAME.azurewebsites.net/qnamaker
            string host = "https://fastracoon21.azurewebsites.net/qnamaker";

            // Authorization endpoint key
            // From Publish Page
            string endpoint_key = "8deb66c2-72b8-4582-bebb-e0ff89dcaaad";

            // Management APIs postpend the version to the route
            // From Publish Page, value after POST
            // Example: /knowledgebases/ZZZ15f8c-d01b-4698-a2de-85b0dbf3358c/generateAnswer
            string route = "/knowledgebases/7bd1dc26-5388-4acb-b0c1-932399b5bf3e/generateAnswer";

            // JSON format for passing question to service
            string question = @"{'question': 'Is the QnA Maker Service free?','top': 3}";


            // Create http client
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // POST method
                request.Method = HttpMethod.Post;

                // Add host + service to get full URI
                request.RequestUri = new Uri(host + route);

                // set question
                request.Content = new StringContent(question, Encoding.UTF8, "application/json");

                // set authorization
                request.Headers.Add("Authorization", "EndpointKey " + endpoint_key);

                // Send request to Azure service, get response
                var response = client.SendAsync(request).Result;
                var jsonResponse = response.Content.ReadAsStringAsync().Result;

                dynamic simpleAnswer = JsonConvert.DeserializeObject(jsonResponse);

                /*
                 * {"answers":[{"questions":["Services We Provide"],
                 *  "answer":"**Services We Provide**\n\nMargie’s Travel can help arrange flights, accommodation, airport transfers, excursions, visas, travel insurance, and currency exchange.","score":44.61,"id":3,"source":"Margies Travel FAQ.docx","metadata":[],"context":{"isContextOnly":false,"prompts":[]}}],"debugInfo":null,"activeLearningEnabled":false}
                 *
                 */

                // Output JSON response
                Console.WriteLine(simpleAnswer.answers[0].answer);

                Console.WriteLine("Press any key to continue.");
                Console.ReadLine();
            }
        }
    }
}
