namespace JFA.Database.Sample.Data.Entities;

internal sealed class Ticket
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int? UserId { get; set; }
}