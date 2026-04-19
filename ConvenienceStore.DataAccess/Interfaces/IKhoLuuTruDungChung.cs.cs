using System.Linq.Expressions;

namespace ConvenienceStore.DataAccess.Interfaces
{
    public interface IKhoLuuTruDungChung<T> where T : class
    {
        Task<IEnumerable<T>> LayTatCaAsync();
        Task<T?> LayTheoIdAsync(object id);
        Task ThemAsync(T entity);
        void Sua(T entity);
        void Xoa(T entity);
        Task<IEnumerable<T>> TimAsync(Expression<Func<T, bool>> dieuKien);
    }
}