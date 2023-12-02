using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        string url = "https://blammed.pro/";

        Uri uri = new Uri(url);
        string websiteName = uri.Host;

        string folderPath = $"seemo grabber - {websiteName}";

        Directory.CreateDirectory(folderPath);

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            try
            {
                string html = await client.GetStringAsync(url);
                File.WriteAllText(Path.Combine(folderPath, "index.html"), html);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                ExtractAndDownloadAssets(doc, "link", "href", "css", folderPath, uri);
                ExtractAndDownloadAssets(doc, "script", "src", "js", folderPath, uri);
                ExtractAndSaveInlineStyles(doc, "style", folderPath);

                DownloadFile("https://blammed.pro/static/scripts/index.js", Path.Combine(folderPath, "index.js"));
                DownloadFile("https://blammed.pro/static/styles/index.css", Path.Combine(folderPath, "index.css"));

                AdjustAssetDirectories(Path.Combine(folderPath, "index.html"), folderPath, uri);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Great success! Website source code, CSS, and JavaScript files downloaded successfully to folder: {folderPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Uh-oh! An error occurred: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    static void ExtractAndDownloadAssets(HtmlDocument doc, string tag, string attribute, string extension, string folderPath, Uri baseUri)
    {
        var nodes = doc.DocumentNode.SelectNodes($"//{tag}[@{attribute}]");

        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var src = node.Attributes[attribute].Value;
                if (src.StartsWith("/"))
                {
                    var linkDirectories = src.TrimStart('/').Split('/');
                    var currentPath = folderPath;

                    foreach (var directory in linkDirectories.Take(linkDirectories.Length - 1))
                    {
                        currentPath = Path.Combine(currentPath, directory);
                        Directory.CreateDirectory(currentPath);
                    }

                    var fileName = Path.GetFileName(src);

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                        var content = client.GetStringAsync(new Uri(baseUri, src)).Result;
                        var filePath = Path.Combine(currentPath, fileName);
                        File.WriteAllText(filePath, content);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"{extension.ToUpper()} file '{fileName}' magically appeared in folder: {folderPath}");
                        Console.ResetColor();

                        node.Attributes[attribute].Value = filePath.Replace(folderPath, string.Empty).Replace("\\", "/");
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }

    static void ExtractAndSaveInlineStyles(HtmlDocument doc, string tag, string folderPath)
    {
        var styleNodes = doc.DocumentNode.SelectNodes($"//{tag}");

        if (styleNodes != null)
        {
            foreach (var styleNode in styleNodes)
            {
                var content = styleNode.InnerHtml;
                var fileName = $"{Guid.NewGuid()}.css";
                File.WriteAllText(Path.Combine(folderPath, fileName), content);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"A mystical CSS file '{fileName}' has been extracted and saved to the sacred folder: {folderPath}");
                Console.ResetColor();

                styleNode.InnerHtml = $"{folderPath}/{fileName}";
                Thread.Sleep(1000);
            }
        }
    }

    static void DownloadFile(string url, string filePath)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            var content = client.GetStringAsync(url).Result;
            File.WriteAllText(filePath, content);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"A mystical file '{Path.GetFileName(filePath)}' has been summoned to the sacred folder: {Path.GetDirectoryName(filePath)}");
            Console.ResetColor();

            Thread.Sleep(1000);
        }
    }

    static void AdjustAssetDirectories(string htmlFilePath, string folderPath, Uri baseUri)
    {
        string htmlContent = File.ReadAllText(htmlFilePath);

        htmlContent = htmlContent.Replace("/static/styles/", $"{folderPath}/static/styles/");
        htmlContent = htmlContent.Replace("/static/scripts/", $"{folderPath}/static/scripts/");

        File.WriteAllText(htmlFilePath, htmlContent);

        Thread.Sleep(3000);
        htmlContent = htmlContent.Replace("seemo grabber - blammed.pro", baseUri.ToString());

        File.WriteAllText(htmlFilePath, htmlContent);

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("The ancient HTML file has been updated with correct asset directories.");
        Console.ResetColor();
    }
}
