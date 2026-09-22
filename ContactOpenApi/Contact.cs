
public record ContactResource(int Id, string FirstName, string? LastName, string Email);
public record CreateContactRequestData(string FirstName, string? LastName, string Email);
public record UpdateContactRequestData(string FirstName, string? LastName, string Email);
public record PatchContactRequestData(string? FirstName, string? LastName, string? Email);
public class Contact
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
}