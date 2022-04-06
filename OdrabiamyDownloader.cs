using Newtonsoft.Json.Linq;
using System.Text.Json;
using OdrabiamyD.Models;
using HtmlAgilityPack;

namespace OdrabiamyD
{
    /// <summary>
    /// Enum reprezentujący stan headerów w wewnętrzym obiekcie HttpClient obieku OdrabiamyDownloader
    /// </summary>
    public enum Headers
    {
        /// <summary>
        /// Reprezentuje headery non-premium wewnętrznego HttpClient
        /// </summary>
        NonPremium,
        /// <summary>
        /// Reprezentuje headery premium wewnętrznego HttpClient
        /// </summary>
        Premium
    }
    /// <summary>
    /// Pobiera strony i książki ze strony Odrabiamy.pl
    /// </summary>
    public class OdrabiamyDownloader
    {
        private readonly string? _apiAdress;
        private HttpClient _client = new HttpClient();
        /// <summary>
        /// Obecne headery wewnętrznego obiektu HttpClient
        /// </summary>
        public Headers Headers { get; private set; } = Headers.NonPremium;
        /// <summary>
        /// Event zgłaszający stan pobierania
        /// </summary>
        /// <remarks>
        /// Obsłuż aby uzyskiwać informacje o stanie pobierania
        /// </remarks>
        public event Action<string>? DownloadStatus;

        /// <summary>
        /// Konstruktor obiektu
        /// </summary>
        /// <remarks>
        /// Jeżeli w pliku .config nie ma ustawionego adresu endpointu api Odrabiamy.pl i nie 
        /// podasz go jako parametr konstruktora to zostanie zgłoszony wyjątek <c>ConfigurationErrorsException</c>. 
        /// Headery zawsze domyślnie ustawione są na <c>NonPremium</c>.
        /// </remarks>
        /// <param name="apiAdress">Opcjonalny adress endpointu api</param>
        /// <exception cref="System.Configuration. ConfigurationErrorsException">
        /// Zgłaszany gdy w pliku konfiguracyjnym nie ma podanego adressu endpoint api,
        /// a konstruktor nie otrzymał tego adresu.
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
            else
                _apiAdress = apiAdress;

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
        /// Zmienia headery wewnętrznego HttpClient
        /// </summary>
        /// <remarks>
        /// Żeby korzystać metod z "Premium" w nazwie, trzeba zmienić headery na <c>Headers.Premium</c> 
        /// </remarks>
        /// <param name="headers"></param>
        /// <param name="token">Opcjonalny token logowania potrzebny jeśli zamierzasz używać funkcji premium</param>
        public void ChangeHeaders(Headers headers, string? token = default)
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

                    _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(token ?? "");
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
        /// Dla podanego loginu i hasła, uzyskuje token potrzebny do pobierania premium
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        /// <exception cref="WrongHeadersException">Zgłaszany gdy obecnie ustawione headery są niepoprawne</exception>
        /// <exception cref="Exception">Zgłaszany gdy nie dało się pozyskać tokenu</exception>
        public async Task<string?> GetTokenAsync(string user, string password, 
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.NonPremium)
                throw new WrongHeadersException("Wrong headers!", Headers);
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
        /// Próbuje pobrać wszystkie strony w podanym przedziale i tworzy z nich książkę
        /// </summary>
        /// <param name="startpage">Pierwsza strona</param>
        /// <param name="lastpage">Ostatnia strona</param>
        /// <param name="bookid">ID Cionszki</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        /// <exception cref="WrongHeadersException">Zgłaszany gdy obecnie ustawione headery są niepoprawne
        /// </exception>
        public async Task<Book> DownloadBookAsync(int startpage, int lastpage, int bookid,
            CancellationToken ctoken = default)
        {
            DownloadStatus?.Invoke($"Started download of book {bookid}");
            var pages = new List<Page>();
            for (int pageN = startpage; pageN <= lastpage; pageN++)
            {
                try
                {
                    var page = await DownloadPageAsync(pageN, bookid, ctoken);
                    if (page is not null) pages.Add(page);
                }
                catch (WrongHeadersException)
                {
                    throw;
                }
                catch
                {
                    DownloadStatus?.Invoke($"Could not download page {pageN}!");
                    throw;
                }
            }
            DownloadStatus?.Invoke($"Finished download of book {bookid}");
            return new Book(bookid, pages.ToArray());
        }
        /// <summary>
        /// Próbuje pobrać wszystkie strony w podanym przedziale i tworzy z nich książkę,
        /// używając konta premium
        /// </summary>
        /// <param name="startpage">Strona startowa</param>
        /// /// <param name="lastpage">Ostatnia strona</param>
        /// <param name="bookid">ID Cionszki</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throw when there was no page to download for given number
        /// </exception>
        /// <exception cref="WrongHeadersException">Zgłaszany gdy obecnie ustawione headery są niepoprawne
        /// </exception>
        public async Task<Book> DownloadBookPremiumAsync(int startpage, int lastpage, int bookid,
            CancellationToken ctoken = default)
        {
            var pages = new List<Page>();
            DownloadStatus?.Invoke($"Started download of book {bookid}");
            for (int pageN = startpage; pageN <= lastpage; pageN++)
            {
                try
                {
                    var page = await DownloadPagePremiumAsync(pageN, bookid, ctoken);
                    if (page is not null) pages.Add(page);
                }
                catch(DailyLimitExceededException)
                {
                    DownloadStatus?.Invoke("Daily download limit reached!");
                    break;
                }
                catch (WrongHeadersException)
                {
                    throw;
                }
                catch 
                {
                    DownloadStatus?.Invoke($"Could not download page {pageN}!");
                    throw;
                }
            }
            DownloadStatus?.Invoke($"Finished download of book {bookid}");
            return new Book(bookid, pages.ToArray());
        }
        /// <summary>
        /// Pobiera stronę w wersji premium
        /// </summary>
        /// <param name="page">Numer strony</param>
        /// <param name="bookid">ID Cionszki</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        /// <exception cref="WrongHeadersException">Zgłaszany gdy obecnie ustawione headery są niepoprawne
        /// </exception>
        /// <exception cref="DailyLimitExceededException">Zgłaszany gdy dzienny limit pobrań został przekroczony</exception>
        public async Task<Page?> DownloadPagePremiumAsync(int page, int bookid, 
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.Premium)
                throw new WrongHeadersException("Wrong headers!", Headers);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                $@"https://odrabiamy.pl/api/v2/exercises/page/premium/{page}/{bookid}");
            DownloadStatus?.Invoke($"Started download of page {page}");
            var response = await _client.SendAsync(request, ctoken);

            var jsonr = await response.Content.ReadAsStringAsync(ctoken);
            JObject? content = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(jsonr);

            var error = content?["error"]?["message"]?["default"]?.Value<string>();
            if (error is not null)
                throw new DailyLimitExceededException(
                    "Daily premium limit exceeded!");
            
            var pagedata = content?["data"]?[0]?["solution"]?.Value<string>();
            DownloadStatus?.Invoke($"Finished download of page {page}");
            if (pagedata is null) return null;
            return new Page(page, pagedata);
        }
        /// <summary>
        /// Pobiera stronę
        /// </summary>
        /// <param name="page">Numer strony</param>
        /// <param name="bookid">ID Cionszki</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        /// <exception cref="WrongHeadersException">Zgłaszany gdy obecnie ustawione headery są niepoprawne
        /// </exception>
        public async Task<Page?> DownloadPageAsync(int page, int bookid,
            CancellationToken ctoken = default)
        {
            if (Headers != Headers.NonPremium)
                throw new WrongHeadersException("Wrong headers!", Headers);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                $@"https://odrabiamy.pl/api/v2/exercises/page/{page}/{bookid}");
            DownloadStatus?.Invoke($"Started download of page {page}");
            var response = await _client.SendAsync(request, ctoken);
            var jsonr = await response.Content.ReadAsStringAsync(ctoken);
            JObject? content = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(jsonr);

            var pagedata = content?["data"]?[0]?["solution"]?.Value<string>();
            DownloadStatus?.Invoke($"Finished download of page {page}");
            if(pagedata is null) return null;
            return new Page(page, pagedata);
        }
        /// <summary>
        /// Zapisuje stronę jako plik HTML
        /// </summary>
        /// <param name="page"></param>
        /// <param name="path">Ścieżka pliku</param>
        public void SavePageAsHTML(Page page, string path)
        {

            File.WriteAllText(Path.ChangeExtension(path, "html"), page.Content);
        }
        /// <summary>
        /// Zapisuje stronę jako plik HTML asynchronicznie
        /// </summary>
        /// <param name="page"></param>
        /// <param name="path">Ścieżka pliku</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        public async Task SavePageAsHTMLAsync(Page page, string path, 
            CancellationToken ctoken = default)
        {
            await File.WriteAllTextAsync(Path.ChangeExtension(path, "html"), page.Content, ctoken);
        }
        /// <summary>
        /// Tworzy folder i zapisuje w nim wszystkie strony książki jako pliki HTML
        /// </summary>
        /// <param name="book"></param>
        /// <param name="dirpath">Ścieżka folderu</param>
        public void SaveBookAsHTML(Book book, string dirpath)
        {
            Directory.CreateDirectory(dirpath);
            foreach(var page in book.Pages)
            {
                SavePageAsHTML(page, Path.Combine(dirpath, page.Number.ToString()));
            }
        }
        /// <summary>
        /// Tworzy folder i zapisuje w nim wszystkie strony książki jako pliki HTML asynchronicznie
        /// </summary>
        /// <param name="book"></param>
        /// <param name="dirpath">Ścieżka folderu</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
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
        /// Zapisuje kod HTML książki jako tekst w pliku <c>.txt</c> o podanej ścieżce
        /// </summary>
        /// <param name="page"></param>
        /// <param name="path">Ścieżka pliku</param>
        public void SavePageAsText(Page page, string path)
        {
            File.WriteAllText(Path.ChangeExtension(path, "txt"), page.Content);
        }
        /// <summary>
        /// Zapisuje kod HTML książki jako tekst w pliku <c>.txt</c> o podanej ścieżce asynchronicznie
        /// </summary>
        /// <param name="page"></param>
        /// <param name="path">Ścieżka pliku</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        public async Task SavePageAsTextAsync(Page page, string path,
            CancellationToken ctoken = default)
        {
            await File.WriteAllTextAsync(Path.ChangeExtension(path, "txt"), page.Content, ctoken);
        }
        /// <summary>
        /// Zapisuje wszystkie obrazki typu <c>.svg</c> o <c>.img</c> ze strony w podanym folderze
        /// </summary>
        /// <param name="page"></param>
        /// <param name="dir">Ścieżka folderu</param>
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
        /// Zapisuje wszystkie obrazki typu <c>.svg</c> o <c>.img</c> ze strony w podanym folderze asynchronicznie
        /// </summary>
        /// <param name="page"></param>
        /// <param name="dir">Ścieżka folderu</param>
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
                var imagestring = await _client.GetStringAsync(svgs.ElementAt(i).ImageString, ctoken);
                await File.WriteAllTextAsync(Path.Combine(dir, $"image-{i}{svgs.ElementAt(i).Extension}"),
                    imagestring, ctoken);
            }
        }
        /// <summary>
        /// Zapisuje w folderze obrazki ze wszystkich stron książki w poszczególnych podfolderach
        /// </summary>
        /// <param name="book"></param>
        /// <param name="dir">Ścieżka folderu</param>
        public void SaveAllBookImages(Book book, string dir)
        {
            Directory.CreateDirectory(dir);
            foreach (var page in book.Pages)
            {
                SaveAllPageImages(page, Path.Combine(dir, $"page_{page.Number}"));
            }
        }
        /// <summary>
        /// Zapisuje w folderze obrazki ze wszystkich stron książki w poszczególnych podfolderach asynchronicznie
        /// </summary>
        /// <param name="book"></param>
        /// <param name="dir">Ścieżka folderu</param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        public async Task SaveAllBookImagesAsync(Book book, string dir, 
            CancellationToken ctoken = default)
        {
            Directory.CreateDirectory(dir);
            foreach (var page in book.Pages)
            {
                await SaveAllPageImagesAsync(page, Path.Combine(dir, $"page_{page.Number}"), 
                    ctoken);
            }
        }
        /// <summary>
        /// WIP - NIE UŻYWAĆ
        /// </summary>
        /// <param name="startpage"></param>
        /// <param name="lastpage"></param>
        /// <param name="bookid"></param>
        /// <param name="maxlevelofparallelism"></param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        public Book? DownloadBookMultithread(int startpage, int lastpage, int bookid,
            int maxlevelofparallelism, CancellationToken ctoken = default )
        {
            var pages = new System.Collections.Concurrent.ConcurrentBag<Page>();
            DownloadStatus?.Invoke($"Started download of book {bookid}");

            var options = new ParallelOptions()
            {
                CancellationToken = ctoken,
                MaxDegreeOfParallelism = maxlevelofparallelism,
            };
            try
            {
                var exceptions = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
                Parallel.For(startpage, lastpage + 1, options, i =>
                {
                    try
                    {
                        var page = DownloadPageAsync(i, bookid, ctoken).Result;
                        if (page is not null) pages.Add(page);
                    }
                    catch (Exception ex)
                    {
                        if(ex.InnerException is WrongHeadersException) exceptions.Enqueue(ex);
                    }
                });
                Console.WriteLine(exceptions.Count);
                if (exceptions.Count > 0) throw new AggregateException(exceptions);
            }
            catch (AggregateException ae)
            {
                foreach(var ex in ae.Flatten().InnerExceptions)
                {
                    if (ex is WrongHeadersException) 
                        throw ex as WrongHeadersException;
                }
            }

            DownloadStatus?.Invoke($"Finished download of book {bookid}");
            return new Book(bookid, pages.OrderBy(p => p.Number).ToArray());
        }
        /// <summary>
        /// WIP - NIE UŻYWAĆ
        /// </summary>
        /// <param name="startpage"></param>
        /// <param name="lastpage"></param>
        /// <param name="bookid"></param>
        /// <param name="maxlevelofparallelism"></param>
        /// <param name="ctoken"></param>
        /// <returns></returns>
        public Book? DownloadBookPremiumMultithread(int startpage, int lastpage, int bookid,
           int maxlevelofparallelism, CancellationToken ctoken = default)
        {
            var pages = new System.Collections.Concurrent.ConcurrentBag<Page>();
            DownloadStatus?.Invoke($"Started download of book {bookid}");

            var options = new ParallelOptions()
            {
                CancellationToken = ctoken,
                MaxDegreeOfParallelism = maxlevelofparallelism,
            };
            try
            {
                var exceptions = new System.Collections.Concurrent.ConcurrentQueue<Exception>();
                Parallel.For(startpage, lastpage + 1, options, (i, state) =>
                {
                
                    try
                    {
                        var page = DownloadPagePremiumAsync(i, bookid, ctoken).Result;
                        if(page is not null) pages.Add(page);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                        state.Stop();
                    }
                });
                Console.WriteLine(exceptions.Count);
                if (exceptions.Count > 0) throw new AggregateException(exceptions);
            }
            catch(AggregateException ae)
            {
                foreach(var ex in ae.Flatten().InnerExceptions)
                {
                    if (ex is DailyLimitExceededException) throw ex as DailyLimitExceededException;
                    if (ex is WrongHeadersException) throw ex as WrongHeadersException;
                }
            }
            DownloadStatus?.Invoke($"Finished download of book {bookid}");
            return new Book(bookid, pages.OrderBy(p => p.Number).ToArray());
        }
    }
}