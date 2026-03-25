namespace MyShop.Repositories;

public interface IRepo<T>
{
    Task<List<T>> GetAllAsync();
}
