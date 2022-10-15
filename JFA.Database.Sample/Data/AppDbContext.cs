using JFA.Database.Sample.Data.Entities;
using JFA.Database.Sqlite;

namespace JFA.Database.Sample.Data;

internal class AppDbContext : DbContext
{
    public AppDbContext(string connectionString) : base(connectionString)
    {
        Users = Configure<User>();
        Tickets = Configure<Ticket>();
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Ticket> Tickets { get; set; }

    public override void Save()
    {
        SaveTable(Users);
        SaveTable(Tickets);

        Users = Configure<User>();
        Tickets = Configure<Ticket>();
    }
}