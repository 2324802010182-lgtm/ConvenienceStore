using ConvenienceStore.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ConvenienceStore.DataAccess.Repositories
{
    public class KhoLuuTruDungChung<T> : IKhoLuuTruDungChung<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public KhoLuuTruDungChung(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> LayTatCaAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> LayTheoIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task ThemAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Sua(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Xoa(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> TimAsync(Expression<Func<T, bool>> dieuKien)
        {
            return await _dbSet.Where(dieuKien).ToListAsync();
        }
    }
}