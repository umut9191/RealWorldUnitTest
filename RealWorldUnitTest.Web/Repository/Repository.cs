using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Models;

namespace RealWorldUnitTest.Web.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly UnitTestContext _context;
        private readonly DbSet<TEntity> _dbSet;
        public Repository(UnitTestContext context)
        {
            _context = context;
            _dbSet =_context.Set<TEntity>();
        }
        public async Task Create(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task Update(TEntity entity)
        {
            //Bu kullanım ile tablodaki değişen tek alan sadece değil, bütün alanları tekrar günceller.
            _context.Entry(entity).State = EntityState.Modified;
            //_dbSet.Update(entity); // bu ise sadece ilgili değişen alanı günceller.
            await _context.SaveChangesAsync();
        }
    }
}
