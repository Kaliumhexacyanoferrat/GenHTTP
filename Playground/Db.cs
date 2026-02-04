namespace GenHTTP.Playground;

using Microsoft.EntityFrameworkCore;

public class Item
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
}

public class TestDbContext : DbContext
{
    public DbSet<Item> Items => Set<Item>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=test.db");
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(e =>
        {
            e.ToTable("Items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired();
        });
    }
    
}
