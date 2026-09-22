
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public static class ContactsEndpointMapper
{
    private class ContactsEndpoint { };

    public static void Map(WebApplication app)
    {
        var contacts = app.MapGroup("/contacts").WithTags("Contacts").RequireAuthorization();

        contacts.MapGet("/list", List).WithName("Contacts.List").WithSummary("").AllowAnonymous();
        contacts.MapGet("/get/{id}", Get).WithName("Contacts.Get").WithSummary("").AllowAnonymous();
        contacts.MapPost("/post", Post).WithName("Contacts.Post").WithSummary("");
        contacts.MapPut("/put/{id}", Put).WithName("Contacts.Put").WithSummary("");
        contacts.MapPatch("/patch/{id}", Patch).WithName("Contacts.Patch").WithSummary("");
        contacts.MapDelete("/delete/{id}", Delete).WithName("Contacts.Delete").WithSummary("");
    }

    static async Task<Ok<ContactResource[]>> List(ContactDbContext db,
                                                  CancellationToken token,
                                                  ILogger<ContactsEndpoint> logger)
    {
        return TypedResults.Ok(await db.Contacts
                                        .Select(contact => new ContactResource(contact.Id,
                                                                                contact.FirstName,
                                                                                contact.LastName,
                                                                                contact.Email))
                                        .ToArrayAsync(token));
    }

    static async Task<Results<Ok<ContactResource>, NotFound>> Get(int id,
                                                                    ContactDbContext db,
                                                                    CancellationToken token,
                                                                    ILogger<ContactsEndpoint> logger)
    {
        var contact = await db.Contacts.FindAsync(id, token);

        return contact is not null
                ? TypedResults.Ok(new ContactResource(contact.Id, contact.FirstName, contact.LastName, contact.Email))
                : TypedResults.NotFound();
    }

    static async Task<CreatedAtRoute<ContactResource>> Post(CreateContactRequestData contactData,
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

        return TypedResults.CreatedAtRoute(contactResource, "Contacts.Get", new { id = contactResource.Id });
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

    static async Task<Results<NotFound, NoContent>> Patch(int id,
                                                            PatchContactRequestData contactData,
                                                            ContactDbContext db,
                                                            CancellationToken token,
                                                            ILogger<ContactsEndpoint> logger)
    {
        var contact = await db.Contacts.FindAsync(id, token);

        if (contact is null)
        {
            return TypedResults.NotFound();
        }

        if (contactData.FirstName is not null) contact.FirstName = contactData.FirstName;
        if (contactData.LastName is not null) contact.LastName = contactData.LastName;
        if (contactData.Email is not null) contact.Email = contactData.Email;

        await db.SaveChangesAsync(token);

        return TypedResults.NoContent();
    }

    static async Task<Results<NotFound, NoContent>> Delete(int id,
                                                            ContactDbContext db,
                                                            CancellationToken token,
                                                            ILogger<ContactsEndpoint> logger)
    {
        if (await db.Contacts.FindAsync(id, token) is Contact contact)
        {
            db.Contacts.Remove(contact);

            await db.SaveChangesAsync(token);

            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

}