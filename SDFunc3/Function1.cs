using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SDFunc3
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([QueueTrigger("scraperdata", Connection = "myScraperDataQueueConnection")]string myQueueItem, ILogger log)
        {

            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            PostToBrightData(myQueueItem);

        }
        public async Task PostToBrightData(string queueItem)
        {

            try
            {
                string requestUri = "";
                ProcessingDirective processingDirective = JsonConvert.DeserializeObject<ProcessingDirective>(queueItem);

                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", "22f606a9-94ec-4aeb-a624-151a97519057"); //My note: during the video blur this out
                                                                                                     // new AuthenticationHeaderValue("Bearer", "Your Bright Data Token Goes Here"); //Use secret to store this information //My note: this line goes into GitHub instead of the line above

                client.DefaultRequestHeaders
                     .Accept
                     .Add(new MediaTypeWithQualityHeaderValue("application/json"));


                if (processingDirective.Stage == "1")
                {
                    //Call TIOBE on BrightData - This code must be put into secret or vault and read at runtime
                    requestUri = "https://api.brightdata.com/dca/trigger?collector=c_lu0xnqikjas5rhmhj&queue_next=1";
                    //In GitHub the line below will be included instead of the line above
                    // requestUri = "URI used to trigger appropriate Scraper Task goes here";

                    var response = await client.PostAsync(requestUri, null);

                    var statusCode = response.StatusCode;

                    Console.WriteLine($"Stage {processingDirective.Stage}:  Status Code: {statusCode} - returned after triggering BrightData scraper");

                }
                else if (processingDirective.Stage == "2")
                {
                    var input = processingDirective.Input;
                    //Call Amazon Scraper on BrightData - This code must be put into secret or vault and read at runtime
                    requestUri = "https://api.brightdata.com/dca/trigger?collector=c_lty5ig4yjkcqtf2en&queue_next=1";
                    //In GitHub the line below will be included instead of the line above
                    // requestUri = "URI used to trigger appropriate Scraper Task goes here";

                    //Call Amazon on BrightData
                    var response = await client.PostAsJsonAsync(requestUri, new { search = input });

                    var statusCode = response.StatusCode;

                    Console.WriteLine($"Stage {processingDirective.Stage}:  Status Code: {statusCode} - returned after triggering BrightData scraper");


                }
                else if (processingDirective.Stage == "3")
                {
                    //JSON format for stage 3 input - "{'stage':'3','input':{'query':'Java','keywords':[code,'programming']}}"
                    var input = processingDirective.Input; // this will be an array of objects {'query':'Java','keywords':'code,programming'}
                    requestUri = "https://api.brightdata.com/dca/trigger?collector=c_ltygmbmm1405s7q7le&queue_next=1";

                    //In GitHub the line below will be included instead of the line above
                    // requestUri = "URI used to trigger appropriate Scraper Task goes here";

                    // YouTubeQuery youTubeQuery = JsonConvert.DeserializeObject<YouTubeQuery>(input); 

                    //Call YOuTube on BrightData
                    var response = await client.PostAsJsonAsync(requestUri, new { query = input.query, keywords = string.Join(",", input.keywords) });

                    var statusCode = response.StatusCode;

                    Console.WriteLine($"Stage {processingDirective.Stage}:  Status Code: {statusCode} - returned after triggering BrightData scraper");

                }
                else
                {
                    Console.WriteLine($"Invalid stage number {processingDirective.Stage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

    }

    public class ProcessingDirective
    {
        public string Stage { get; set; }
        public dynamic Input { get; set; }
    }

}
//{ "Stage":"3","Input":{ "query":"Java","keywords":["code", "programming"]} }