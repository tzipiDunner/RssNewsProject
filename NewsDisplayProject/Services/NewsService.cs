using System.Net;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;
using NewsDisplay.Models;
using NewsDisplay.Services;
using Microsoft.Extensions.Options;

public class NewsService : INewsService
{
    private const string RssCacheKey = "RssFeed";
    private readonly IMemoryCache _memoryCache;
    private readonly string _rssFeedUrl;

    public NewsService(IMemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _rssFeedUrl = configuration["NewsSettings:RssFeedUrl"];
    }

    public async Task<List<NewsItem>> GetNewsItemsAsync()
    {
        try
        {
            if (!_memoryCache.TryGetValue(RssCacheKey, out XDocument cachedRssFeed))
            {
                var rssUrl = _rssFeedUrl;

                using (var webClient = new WebClient())
                {
                    var rssContent = await webClient.DownloadStringTaskAsync(rssUrl);
                    cachedRssFeed = XDocument.Parse(rssContent);
                    _memoryCache.Set(RssCacheKey, cachedRssFeed, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30)
                    });
                }
            }

            return cachedRssFeed.Descendants("item")
                .Select(item => new NewsItem
                {
                    Title = item.Element("title")?.Value,
                    Description = item.Element("description")?.Value,
                    Link = item.Element("link")?.Value
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetNewsItemsAsync: {ex.Message}");
            return new List<NewsItem>();
        }
    }

    public async Task<NewsItem> GetNewsItemByIdAsync(int id)
    {
        try
        {
            var rssFeed = _memoryCache.Get<XDocument>(RssCacheKey);

            if (rssFeed != null)
            {
                var item = rssFeed.Descendants("item").ElementAtOrDefault(id);

                if (item != null)
                {
                    return new NewsItem
                    {
                        Title = item.Element("title")?.Value,
                        Description = item.Element("description")?.Value,
                        Link = item.Element("link")?.Value
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetNewsItemByIdAsync: {ex.Message}");
        }

        return null;
    }
}

