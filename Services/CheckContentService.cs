using HtmlAgilityPack;

namespace AppVidaMinisterio.Services
{
    internal class CheckContentService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<bool> HasValidContentAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);
                var h1Node = doc.DocumentNode.SelectSingleNode("//h1");
                if (h1Node != null)
                {
                    return true;
                }
                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
