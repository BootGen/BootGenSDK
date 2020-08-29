# BootGen
<img align="right" width="200px" height="85px" src="img/BootGenLogo.png">
BootGen is a Library that helps you to create model based and template driven application code generators. With BootGen you can create code generators for ASP.NET or ASP.NET Core applications that use REST APIs.

Generators based on BootGen can generate:
* Server side entity classes
* Client side entity classes
* ORM Layer
* Database Seeds
* Controllers
* API Client
* Client side state management
* Rest API documentation (Open API Specification / Swagger)

The models that BootGen uses for code generation are simple C# classis, such as:

```csharp
class Ticket
{
    public int Id { get; set; }
    public DateTime Deadline { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}
```

To read more about creating models, please visit the [[Models|models]] page.

Models can be used as [[REST resources|resources]].

BootGen provides a very convenient way to create database [[seeds|seeding]].
