# JFA.Database.Sqlite
Simple object mapping tool for Sqlite

# Steps

> Install package
```PM
NuGet\Install-Package JFA.Database.Sqlite -Version <VERSION>
```
#
> Create DbContext
```C#
using JFA.Database.Sqlite;

internal class AppDbContext : DbContext
{
    public AppDbContext(string connectionString) : base(connectionString)
    {
        Users = Configure<User>();
    }

    public DbSet<User> Users { get; set; }
}
```
# 
> Insert data and read
```C#
var dbContext = new AppDbContext("Data Source = users.db");

// add new rows
dbContext.Insert(new User { Name = "Csharp", Phone = "123456789" });

// read
var _user = dbContext.Users.First(u => u.Id == 1);

// update
_user.Name = "DotNet";
_user.Phone = "987654321";
dbContext.Save();

```
