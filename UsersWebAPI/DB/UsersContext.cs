using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Options;
using UsersWebAPI.DatabaseModels;
using UsersWebAPI.Options;

namespace UsersWebAPI.DB;

public class UsersContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    private readonly string _connection;

    public UsersContext(IOptions<DatabaseSettings> settings)
    {
        _connection = settings.Value.DatabaseConnection;

        Database.EnsureDeleted();
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connection);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(x => x.Guid).HasDefaultValueSql("gen_random_uuid()");
        //modelBuilder.Entity<User>().HasKey(x => x.Guid);
        //modelBuilder.Entity<User>().Property(x => x.Guid).HasValueGenerator<GuidValueGenerator>();
        
        modelBuilder.Entity<User>().HasData(
            new User
            {
                //Guid = new Guid(),
                Login = "Admin",
                Password = "Admin",
                Name = "Admin",
                Admin = true,
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now.ToUniversalTime(),
                ModifiedOn = DateTime.Now.ToUniversalTime(),
                ModifiedBy = "Admin",
                Gender = 2
            }
        );
    }
}