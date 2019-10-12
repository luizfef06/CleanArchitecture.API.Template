using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Api.Template.DbEFCore.Repositories
{
    public class EfCoreDbContextFactory : IDesignTimeDbContextFactory<EfCoreDbContext>
    {
        public EfCoreDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var context = new DbContextOptionsBuilder<EfCoreDbContext>();

            context.UseCosmosSql("https://vet-dev1.documents.azure.com:443/", "QORSHEFMH7p64PfmelgKDqh3MHuqI8we0OlQeuYcqafnUudoYyO0ZzUpoHbkrofNMOk0Jo9lFn0LF4eKtL3YNA==", "vet-dev1");

            return new EfCoreDbContext(context.Options);
        }
    }
}