using JFA.Database.Sample.Data;
using JFA.Database.Sample.Data.Entities;

var dbContext = new AppDbContext("Data Source = users.db");

// add new rows

dbContext.Insert(new Ticket { IsActive = true, UserId = 2, Name = "ticket" });
dbContext.Users.Add(new User { Name = "javlonbek", Phone = "432422423" });
dbContext.Save();


// update entity

var _user = dbContext.Users.First(u => u.Id == 1);
_user.Name = "Javlon Fattoev";
_user.Phone = "121314";
dbContext.Save();


// read data

foreach (var user in dbContext.Users)
    Console.WriteLine($"{user.Id}, {user.Name}, {user.Phone}");

foreach (var ticket in dbContext.Tickets)
    Console.WriteLine($"{ticket.Id}, {ticket.Name}, {ticket.UserId}, {ticket.IsDeleted}");