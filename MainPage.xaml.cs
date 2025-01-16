using Deep_Learning.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Deep_Learning
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }




        private async void OnUploadImageClicked(object sender, EventArgs e)
        {
            try
            {
                // Restrict file picker to image files
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select an image file",
                    FileTypes = FilePickerFileType.Images // Built-in support for images
                });

                if (result != null)
                {
                    // Retrieve file information
                    var fileName = result.FileName;
                    var filePath = result.FullPath;

                    // Ensure the selected file is an image by checking its extension
                    var validImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
                    if (!validImageExtensions.Contains(Path.GetExtension(fileName).ToLower()))
                    {
                        Console.WriteLine("The selected file is not a valid image.");
                        return;
                    }

                    // Read the image as a stream or byte array
                    var fileStream = await result.OpenReadAsync();
                    var byteArray = ReadStreamToByteArray(fileStream);

                    // Send the image to the Flask API using HttpClient
                    var prediction = await GetPredictionFromApi(byteArray);

                    if (prediction != null)
                    {
                        Console.WriteLine($"Prediction: {prediction}");
                        resultLabel.Text = prediction;
                    }
                    else
                    {
                        Console.WriteLine("Failed to get prediction from the API.");
                    }
                }
                else
                {
                    Console.WriteLine("No file was selected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting image: {ex.Message}");
            }
        }

        private byte[] ReadStreamToByteArray(Stream inputStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private async Task<String> GetPredictionFromApi(byte[] imageBytes)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = "http://127.0.0.1:5000/predict"; // Change this to your API URL
                    using (var content = new MultipartFormDataContent())
                    {
                        var fileContent = new ByteArrayContent(imageBytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Adjust according to the file type
                        content.Add(fileContent, "file", "image.jpg");

                        // Send the POST request
                        var response = await client.PostAsync(uri, content);
                        if (response.IsSuccessStatusCode)
                        {
                            // Read and parse the JSON response
                            var responseString = await response.Content.ReadAsStringAsync();
                            JObject jsonResponse = JObject.Parse(responseString);
                            // Extract the predicted class
                            string predictedClass = jsonResponse["predicted_class"].ToString();
                           
                            return predictedClass;
                        }
                        else
                        {
                            Console.WriteLine("Failed to get prediction from the API.");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending request to API: {ex.Message}");
                return null;
            }
        }



    }



}
