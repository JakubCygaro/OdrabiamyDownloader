using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using OdrabiamyD.Models;
using System.IO;
using HtmlAgilityPack;

namespace OdrabiamyD
{
    /// <summary>
    /// Represents the headers of the internal HttpClient
    /// </summary>
    public enum Headers
    {
        /// <summary>
        /// Represents the non-premium headers for the internal HttpClient
        /// </summary>
        NonPremium,
        /// <summary>
        /// Represents the premium headers for the internal HttpClient
        /// </summary>
        Premium
    }
    /// <summary>
    /// Downloads books and pages from Odrabiamy.pl
    /// </summary>
    public class OdrabiamyDownloader
    {
        private readonly string? _apiAdress;
        private HttpClient _client = new HttpClient();
        /// <summary>
        /// Current headers of the internal HttpClient
        /// </summary>
        public Headers Headers { get; private set; } = Headers.NonPremium;

        /// <summary>
        /// OdrabiamyDownloader constructor
        /// </summary>
        /// <remarks>
        /// If there is no valid api endpoint adress in the .config file, and you do not specify
        /// an adress at object creation, a ConfigurationErrorsException will be thrown. Headers are
        /// set to <c>NonPremium</c> by default.
        /// </remarks>
        /// <param name="apiAdress">Optional adress of the api endpoint</param>
        /// <exception cref="System.Configuration. ConfigurationErrorsException">
        /// Throw when there is no api endpoint adress in the .config file
        /// </exception>
        public OdrabiamyDownloader(string? apiAdress = default)
        {
            //https://odrabiamy.pl/api/v2/sessions <- adress api, domyślnie w pliku konfiguracyjnym
            //można podać przy tworzeniu obiektu adress endpointu api z którego będzie brany token
            //w przeciwnym wypadku zostanie on zczytany z configu
            //!!JAK TEGO NIE MA W KONFIGU ANI TEGO NIE PODASZ TO ŻYCZĘ POWODZENIA!!
            if (apiAdress is null)
                _apiAdress = System.Configuration.ConfigurationManager
                    .AppSettings["apiAdress"] ??
                    throw new System.Configuration.
                    ConfigurationErrorsException("No apiAdress specified in the config file!");


            //headery domyślnie są nonpremium
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
            _client.DefaultRequestHeaders.UserAgent.Add(
                new System.Net.Http.Headers.ProductInfoHeaderValue("Mozilla", "5.0"));
            _client.DefaultRequestHeaders.UserAgent.Add(
                new System.Net.Http.Headers.ProductInfoHeaderValue("AppleWebKit", "537.36"));
            _client.DefaultRequestHeaders.UserAgent.Add(
                new System.Net.Http.Headers.ProductInfoHeaderValue("Chrome", "99.0.4844.88"));
            _client.DefaultRequestHeaders.UserAgent.Add(
                new System.Net.Http.Headers.ProductInfoHeaderValue("Safari", "537.36"));
            _client.DefaultRequestHeaders.Connection.Add("keep-alive");

        }
        /// <summary>
        /// Changes the headers for the internal HttpClient
        /// </summary>
        /// <param name="headers"></param>
        public void ChangeHeaders(Headers headers)
        {
            _client.DefaultRequestHeaders.Clear();
            switch (headers)
            {
                case Headers.NonPremium:

                    
                    _client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                    _client.DefaultRequestHeaders.UserAgent.Add(
                        new System.Net.Http.Headers.ProductInfoHeaderValue("Mozilla", "5.0"));
                    _client.DefaultRequestHeaders.UserAgent.Add(
                        new System.Net.Http.Headers.ProductInfoHeaderValue("AppleWebKit", "537.36"));
                    _client.DefaultRequestHeaders.UserAgent.Add(
                        new System.Net.Http.Headers.ProductInfoHeaderValue("Chrome", "99.0.4844.88"));
                    _client.DefaultRequestHeaders.UserAgent.Add(
                        new System.Net.Http.Headers.ProductInfoHeaderValue("Safari", "537.36"));
                    _client.DefaultRequestHeaders.Connection.Add("keep-alive");
                    Headers = headers;
                    break;

                case Headers.Premium:

                    _client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                    _client.DefaultRequestHeaders.UserAgent.Add(
                        new System.Net.Http.Headers.ProductInfoHeaderValue(
                            new System.Net.Http.Headers.ProductHeaderValue("new_user_agent-huawei-142")));
                    _client.DefaultRequestHeaders.Connection.Add("keep-alive");
                    Headers = headers;
                    break;
            }
        }

        /// <summary>
        /// For given login and password, gets the token from the api endpoint adress
        /// </summary>
        /// <param name="user">User login</param>
        /// <param name="password">User password</param>
        /// <param name="ctoken">Cancellation token</param>
        /// <returns>The token as a <c>string</c> </returns>
        /// <exception cref="Exception">Throw when cannot obtain token</exception>
        public async Task<string?> GetTokenAsync(string user, string password, 
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.NonPremium)
                throw new Exception("Headers must be set to NonPremium!");
            string? token;
            var content = new
            {
                login = user,
                password = password,
            };
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                _apiAdress);
            request.Content = new StringContent(JsonSerializer.Serialize(content),
                System.Text.Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _client.SendAsync(request, ctoken);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Could not obtain token for given login and password, " +
                        $"{response.StatusCode}");

                var data = await response.Content.ReadAsStringAsync(ctoken);
                JObject? dataAsjson = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject(data);
                token = dataAsjson?["data"]?["token"]?.Value<string>();
                
            }
            catch (Exception)
            {
                throw;
            }
            return token;
        }
        /// <summary>
        /// Tries to download all pages of a book with given data
        /// </summary>
        /// <param name="token">Login token</param>
        /// <param name="startpage">Page from which start downloading</param>
        /// <param name="lastpage">The last page to download</param>
        /// <param name="bookid">Book ID</param>
        /// <param name="ctoken">Cancellation token</param>
        /// <returns>A <c>Page[]</c> containing downloaded pages</returns>
        /// <exception cref="ArgumentException">Throw when there was no page to download for given number
        /// </exception>
        /// <exception cref="Exception">Throw when there was no page to download for given number
        /// </exception>
        public async Task<Book> DownloadBookAsync(string token, int startpage, int lastpage, int bookid,
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.NonPremium)
                throw new Exception("Headers must be set to NonPremium!");
            var pages = new List<Page>();
            for (int pageN = startpage; pageN <= lastpage; pageN++)
            {
                try
                {
                    pages.Add(await DownloadPageAsync(token, pageN, bookid, ctoken) ??
                        throw new ArgumentException("No pages to download"));
                }
                catch { }
            }
            
            return new Book(bookid, pages.ToArray());
        }
        /// <summary>
        /// Tries to download all pages of a book with given data using a premium acoount
        /// </summary>
        /// <param name="token">Login token</param>
        /// <param name="startpage">Page from which start downloading</param>
        /// /// <param name="lastpage">The last page to download</param>
        /// <param name="bookid">Book ID</param>
        /// <param name="ctoken">Cancellation token</param>
        /// <returns>A <c>Page[]</c> containing downloaded pages</returns>
        /// <exception cref="ArgumentException">Throw when there was no page to download for given number
        /// </exception>
        /// <exception cref="Exception">Throw when there was no page to download for given number
        /// </exception>
        public async Task<Book> DownloadBookPremiumAsync(string token, int startpage, int lastpage, int bookid,
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.Premium)
                throw new Exception("Headers must be set to Premium!");

            var pages = new List<Page>();
            
            for(int pageN = startpage; pageN <= lastpage; pageN++)
            {
                try
                {
                    pages.Add(await DownloadPagePremiumAsync(token, pageN, bookid, ctoken) ??
                        throw new ArgumentException("No pages to download"));
                }
                catch(DailyLimitExceededException)
                {
                    break;
                }
                catch { }
            }
            
            return new Book(bookid, pages.ToArray());
        }
        /// <summary>
        /// Downloads a page with given data 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="bookid"></param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        public async Task<Page?> DownloadPagePremiumAsync(string token, int page, int bookid, 
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.Premium)
                throw new Exception("Headers must be set to Premium!");

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(token);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                $@"https://odrabiamy.pl/api/v2/exercises/page/premium/{page}/{bookid}");

            //https://odrabiamy.pl/api/v2/exercises/page/premium/{page}/{bookid}
            //http://localhost:8080/
            var response = await _client.SendAsync(request, ctoken);

            var jsonr = await response.Content.ReadAsStringAsync(ctoken);
            JObject? content = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(jsonr);

            var error = content?["error"]?["message"]?["default"]?.Value<string>();
            if (error is not null)
                throw new DailyLimitExceededException(
                    "Daily premium limit exceeded!");
            
            var pagedata = content?["data"]?[0]?["solution"]?.Value<string>();

            return new Page(page, pagedata ?? string.Empty);
        }
        /// <summary>
        /// Downloads a page with given data
        /// </summary>
        /// <param name="token"></param>
        /// <param name="page"></param>
        /// <param name="bookid"></param>
        /// <param name="ctoken"></param>
        /// <returns>HTML code of the page solution as <c>string</c> </returns>
        public async Task<Page?> DownloadPageAsync(string token, int page, int bookid,
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.NonPremium)
                throw new Exception("Headers must be set to NonPremium!");

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(token);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                $@"https://odrabiamy.pl/api/v2/exercises/page/{page}/{bookid}");

            var response = await _client.SendAsync(request, ctoken);
            var jsonr = await response.Content.ReadAsStringAsync(ctoken);
            JObject? content = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(jsonr);

            var pagedata = content?["data"]?[0]?["solution"]?.Value<string>();

            return new Page(page, pagedata ?? string.Empty);
        }
        /// <summary>
        /// Saves a page as HTML
        /// </summary>
        /// <param name="page">Page to save</param>
        /// <param name="path">Path</param>
        public void SavePageAsHTML(Page page, string path)
        {

            File.WriteAllText(Path.ChangeExtension(path, "html"), page.Content);
        }
        /// <summary>
        /// Saves a page as HTML asynchronously
        /// </summary>
        /// <param name="page">Page to save</param>
        /// <param name="path">Path</param>
        /// <param name="ctoken">Cancellation token</param>
        /// <returns>a task that represents the asynchronous save operation</returns>
        public async Task SavePageAsHTMLAsync(Page page, string path, 
            CancellationToken ctoken = default)
        {
            await File.WriteAllTextAsync(Path.ChangeExtension(path, "html"), page.Content, ctoken);
        }
        /// <summary>
        /// Creates a directory and saves all pages of the book provided
        /// </summary>
        /// <param name="book">Book to save</param>
        /// <param name="dirpath">Directory path</param>
        public void SaveBookAsHTML(Book book, string dirpath)
        {
            Directory.CreateDirectory(dirpath);
            foreach(var page in book.Pages)
            {
                SavePageAsHTML(page, Path.Combine(dirpath, page.Number.ToString()));
            }
        }
        /// <summary>
        /// Creates a directory and saves all pages of the book provided asynchronously
        /// </summary>
        /// <param name="book"></param>
        /// <param name="dirpath"></param>
        /// <param name="ctoken">Cancellation token</param>
        /// <returns>a task that represents the asynchronous save operation </returns>
        public async Task SaveBookAsHTMLAsync(Book book, string dirpath,
            CancellationToken ctoken = default)
        {
            Directory.CreateDirectory(dirpath);

            foreach (var page in book.Pages)
            {
                await SavePageAsHTMLAsync(page, Path.Combine(dirpath, page.Number.ToString()), ctoken);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="path"></param>
        public void SavePageAsText(Page page, string path)
        {
            File.WriteAllText(Path.ChangeExtension(path, "txt"), page.Content);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="path"></param>
        /// <param name="ctoken">Cancellation token</param>
        /// <returns></returns>
        public async Task SavePageAsTextAsync(Page page, string path,
            CancellationToken ctoken = default)
        {
            await File.WriteAllTextAsync(Path.ChangeExtension(path, "txt"), page.Content, ctoken);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="dir"></param>
        public void SaveAllPageImages(Page page, string dir)
        {
            var html = new HtmlDocument();
            html.LoadHtml(page.Content);
            var children = html.DocumentNode.ChildNodes;
            var svgs = from child in children.Nodes().AsEnumerable()
                       from attribute in child.Attributes
                       where attribute.Value.EndsWith("big.svg") || attribute.Value.EndsWith(".img")
                       select new
                       {
                           ImageString = attribute.Value,
                           Extension = Path.GetExtension(attribute.Value)
                       };
            Directory.CreateDirectory(dir);
            for(int i = 0; i < svgs.Count(); i++)
            {
                var imagestring = _client.GetStringAsync(svgs.ElementAt(i).ImageString).Result;
                File.WriteAllText(Path.Combine(dir, $"image-{i}{svgs.ElementAt(i).Extension}"), 
                    imagestring);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="dir"></param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        public async Task SaveAllPageImagesAsync(Page page, string dir, 
            CancellationToken ctoken = default)
        {
            var html = new HtmlDocument();
            html.LoadHtml(page.Content);
            var children = html.DocumentNode.ChildNodes;
            var svgs = from child in children.Nodes().AsEnumerable()
                                                       .AsParallel()
                                                       .WithCancellation(ctoken)
                       from attribute in child.Attributes
                       where attribute.Value.EndsWith("big.svg") || attribute.Value.EndsWith(".img")
                       select new
                       {
                           ImageString = attribute.Value,
                           Extension = Path.GetExtension(attribute.Value)
                       };
            Directory.CreateDirectory(dir);
            for (int i = 0; i < svgs.Count(); i++)
            {
                var imagestring = await _client.GetStringAsync(svgs.ElementAt(i).ImageString);
                await File.WriteAllTextAsync(Path.Combine(dir, $"image-{i}{svgs.ElementAt(i).Extension}"),
                    imagestring, ctoken);
            }
        }
    }
}