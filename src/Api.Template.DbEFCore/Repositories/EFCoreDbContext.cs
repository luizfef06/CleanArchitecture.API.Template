using Api.Template.DbEFCore.Mappings;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Api.Template.DbEFCore.Repositories
{
    public class EfCoreDbContext : DbContext
    {
        public EfCoreDbContext(DbContextOptions<EfCoreDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //domain
            modelBuilder.ApplyConfiguration(new PersonaMapConfig());
        }

        public async Task<bool> CreateTheDatabaseAsync()
        {
            var created = await this.Database.EnsureCreatedAsync();
            return created;
        }
    }
}