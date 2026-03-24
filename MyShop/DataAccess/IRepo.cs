namespace MyShop.DataAccess;

public interface IRepo<TData> where TData : class
{
    Task<List<TData>> GetAllAsync();
    Task<TData?> GetByIdAsync(int id);
    Task<int> AddAsync(TData item);
    Task UpdateAsync(TData item);
    Task DeleteAsync(int id);
}
