using DotNetConverter.Data.Models;

namespace DotNetConverter.Data.Repositories;

public interface IRepo<T> where T : IStringIdEntity
{
    IEnumerable<T> GetAll();
    T? Get(string id);
    void Insert(T entity);
    void Update(T entity);
    void Delete(T entity);
    void Save();
    Task SaveAsync();
}