using Supabase;
using System.Reflection;

namespace MyShop.DataAccess;

public class SupabaseRepo<TData> : IRepo<TData> where TData : class
{
    protected readonly Client _client;
    protected readonly string _tableName;

    public SupabaseRepo(Client client, string tableName)
    {
        _client = client;
        _tableName = tableName;
    }

    public virtual async Task<List<TData>> GetAllAsync()
    {
        var response = await _client.From<TData>().Get();
        return response.Models;
    }

    public virtual async Task<TData?> GetByIdAsync(int id)
    {
        var response = await _client.From<TData>()
            .Where(x => GetIdProperty(x) == id)
            .Single();
        return response;
    }

    public virtual async Task<int> AddAsync(TData item)
    {
        var response = await _client.From<TData>().Insert(item);
        return GetIdProperty(response.Models.FirstOrDefault()!);
    }

    public virtual async Task UpdateAsync(TData item)
    {
        await _client.From<TData>().Update(item);
    }

    public virtual async Task DeleteAsync(int id)
    {
        await _client.From<TData>().Where(x => GetIdProperty(x) == id).Delete();
    }

    private int GetIdProperty(TData item)
    {
        var prop = typeof(TData).GetProperty("Id")
            ?? typeof(TData).GetProperty("Id".ToLower()) as PropertyInfo;
        return (int)(prop?.GetValue(item) ?? 0);
    }
}
