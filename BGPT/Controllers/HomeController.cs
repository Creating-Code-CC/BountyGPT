using BGPT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using static System.Environment;
using System.IO;
using Azure.AI.OpenAI;
using Azure;
using System.Threading.Tasks;
using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BGPT.Helpers;


namespace BGPT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string connectionString = "blob storage"; // Store this securely!
        public string extractedText;
        public Uri endpoint;
        public AzureKeyCredential credentials;
        public OpenAIClient openAIClient;
        public ChatCompletionsOptions completionOptions;
        public ChatRequestSystemMessage systemMessage;
        public ChatRequestUserMessage userMessage;


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> IntelligenceGathering(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Ensures only the file name is used, not any path information
                var fileName = Path.GetFileName(file.FileName);

                //// Combines the target directory with the file name
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                //// Ensure the directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                try
                {
                    // Create the file or overwrite it if it already exists
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    extractedText = PdfTextExtractorHelper.ExtractTextFromPdf(file.OpenReadStream());
                    TempData["MyValue"] = extractedText;
                    //await UploadFileToBlobAsync("fileupload-bounty-gpt", fileName, filePath);
                }
                catch (Exception ex)
                {
                    // Log the exception or return an error message
                    return BadRequest("An error occurred while uploading the file.");
                }

                return RedirectToAction("SubjectProfile", "Home");
            }

            // Handle the case where the file is null or empty
            return BadRequest("No file was uploaded.");
        }
        private async Task UploadFileToBlobAsync(string containerName, string blobName, string filePath)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            using (var uploadFileStream = System.IO.File.OpenRead(filePath))
            {
                await blobClient.UploadAsync(uploadFileStream, new BlobHttpHeaders { ContentType = "application/octet-stream" });
                uploadFileStream.Close();
            }
        }

        public IActionResult SubjectProfile()
        {
            ViewBag.Response = TempData["Response"];
            ViewBag.Query = TempData["Query"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Query(string myValue, string chatInput)
        {
            // Now you can use the userName in your server-side logic
            //if (string.IsNullOrEmpty(myValue))
            //{
            //    return RedirectToAction("SubjectProfile");
            //}
            if (!string.IsNullOrEmpty(chatInput))
            {
                endpoint = new Uri(GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"));
                credentials = new AzureKeyCredential(GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
                openAIClient = new OpenAIClient(endpoint, credentials);

                completionOptions = new ChatCompletionsOptions
                {
                    MaxTokens = 4090,
                    Temperature = 1f,
                    FrequencyPenalty = 0.0f,
                    PresencePenalty = 0.0f,
                    NucleusSamplingFactor = 0.95f,
                    DeploymentName = "malik-gpt-35-turbo-trial-1"
                };

                string prompt = "Your goal is to respond to user text usering information from the file: " + myValue;

                systemMessage = new(prompt);
                completionOptions.Messages.Add(systemMessage);

                // Modify the text data
                //var modifiedText = $"Modified: {chatInput} {myValue}";

                //// Pass the modified text back to the view
                //ViewBag.ModifiedText = modifiedText;

                userMessage = new(chatInput);

                completionOptions.Messages.Add(userMessage);

                ChatCompletions response = await openAIClient.GetChatCompletionsAsync(completionOptions);

                ChatResponseMessage assistantResponse = response.Choices[0].Message;
                //ViewBag.Response = assistantResponse.Content;
                TempData["Query"] = chatInput;
                TempData["Response"] = assistantResponse.Content;
            }
            
            return RedirectToAction("SubjectProfile");
        }
        public IActionResult PhysicalDescriptions()
        {
            return View();
        }        
        public IActionResult LegalInformation()
        {
            return View();
        }        
        public IActionResult RecentActivity()
        {
            return View();
        }        
        public IActionResult RiskAssessment()
        {
            return View();
        }        
        public IActionResult BackgroundInfoSkills()
        {
            return View();
        }        
        public IActionResult Contacts()
        {
            return View();
        }        
        public IActionResult Addresses()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
