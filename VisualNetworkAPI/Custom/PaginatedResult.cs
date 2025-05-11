using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VisualNetworkAPI.Paginated
{
  public class PaginatedResult<T>
  {
      public int PageIndex { get; set; }
      public int PageSize { get; set; }
      public int TotalCount { get; set; }
      public int TotalPages { get; set; }
      public bool HasPreviousPage => PageIndex > 1;
      public bool HasNextPage => PageIndex < TotalPages;
      public List<T> Items { get; set; }

      public PaginatedResult(List<T> items, int count, int pageIndex, int pageSize)
      {
          PageIndex = pageIndex;
          PageSize = pageSize;
          TotalCount = count;
          TotalPages = (int)Math.Ceiling(count / (double)pageSize);
          Items = items;
      }
  }

  public static class QueryableExtensions
    {
        public static async Task<PaginatedResult<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> source, 
            int pageIndex, 
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            
            var count = await source.CountAsync(cancellationToken);
            
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            
            return new PaginatedResult<T>(items, count, pageIndex, pageSize);
        }
    }
}