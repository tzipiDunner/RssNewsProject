using NewsDisplay.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsDisplay.Services
{
    public interface INewsService
    {
        Task<List<NewsItem>> GetNewsItemsAsync();
        //Task<NewsItem> GetNewsItemByIdAsync(int id);
    }
}
