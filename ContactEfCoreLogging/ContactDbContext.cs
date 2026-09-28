using Microsoft.EntityFrameworkCore;

class ContactDbContext : DbContext
{
    public ContactDbContext(DbContextOptions<ContactDbContext> options)
        : base(options) { }

    public DbSet<Contact> Contacts => Set<Contact>();
}