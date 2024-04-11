namespace Repository.Repositories {
    public interface IRepository<T> {
        Task<IEnumerable<T>> GetAsync();
        Task<T> GetAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}