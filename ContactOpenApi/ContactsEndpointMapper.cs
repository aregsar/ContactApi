
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public record ContactResource(int Id, string FirstName, string? LastName, string Email);
public record CreateContactRequestData(string FirstName, string? LastName, string Email);
public record UpdateContactRequestData(string FirstName, string? LastName, string Email);


public static class ContactsEndpointMapper
{
    private class ContactsEndpoint { };

    public static void Map(WebApplication app)
    {
        var contacts = app.MapGroup("/contacts").WithTags("Contacts");

        contacts.MapGet("/list", List).WithName("Contacts.List");
        contacts.MapGet("/get/{id}", Get).WithName("Contacts.Get");
        contacts.MapPost("/post", Post).WithName("Contacts.Post");
        contacts.MapPut("/put/{id}", Put).WithName("Contacts.Put");
        contacts.MapPatch("/patch/{id}", Patch).WithName("Contacts.Patch");
        contacts.MapDelete("/delete/{id}", Delete).WithName("Contacts.Delete");
    }

    static async Task<Ok<ContactResource[]>> List(ContactDbContext db,
                                                  CancellationToken token,
                                                  ILogger<ContactsEndpoint> logger)
    {
        return TypedResults.Ok(await db.Contacts.Select(contact => new ContactResource(contact.Id,
                                                                    contact.FirstName,
                                                                    contact.LastName,
                                                                    contact.Email)).ToArrayAsync(token));
    }

    static async Task<Results<Ok<ContactResource>, NotFound>> Get(int id, ContactDbContext db,
                                                    CancellationToken token,
                                                    ILogger<ContactsEndpoint> logger)
    {
        return await db.Contacts.FindAsync(id, token)
            is Contact contact
                ? TypedResults.Ok(new ContactResource(contact.Id, contact.FirstName, contact.LastName, contact.Email))
                : TypedResults.NotFound();
    }

    static async Task<IResult> Post(CreateContactRequestData contactData,
                                    ContactDbContext db,
                                    CancellationToken token,
                                    ILogger<ContactsEndpoint> logger)
    {
        var contact = new Contact
        {
            FirstName = contactData.FirstName,
            LastName = contactData.LastName,
            Email = contactData.Email,
        };

        db.Contacts.Add(contact);
        await db.SaveChangesAsync(token);

        var contactResource = new ContactResource(contact.Id, contact.FirstName, contact.LastName, contact.Email);

        return TypedResults.Created($"/contacts/{contactResource.Id}", contactResource);
    }

    static async Task<Results<NotFound, NoContent>> Put(int id,
                                                        UpdateContactRequestData contactData,
                                                        ContactDbContext db,
                                                        CancellationToken token,
                                                        ILogger<ContactsEndpoint> logger)
    {
        var contact = await db.Contacts.FindAsync(id);

        if (contact is null)
        {
            return TypedResults.NotFound();
        }

        contact.FirstName = contactData.FirstName;
        contact.LastName = contactData.LastName;
        contact.Email = contactData.Email;

        await db.SaveChangesAsync(token);

        return TypedResults.NoContent();
    }

    static async Task<IResult> Patch(int id, TodoPatchDto inputTodo, ContactDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return TypedResults.NotFound();

        if (inputTodo.Name is not null) todo.Name = inputTodo.Name;
        if (inputTodo.IsComplete is not null) todo.IsComplete = inputTodo.IsComplete.Value;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> Delete(int id, ContactDbContext db)
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

}