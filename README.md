# BootGen [![Build Status](https://github.com/BootGen/BootGen/workflows/Test/badge.svg?branch=master)](https://github.com/BootGen/BootGen/actions) [![Coverage Status](https://coveralls.io/repos/github/BootGen/BootGen/badge.svg?branch=master)](https://coveralls.io/github/BootGen/BootGen?branch=master)
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
Please visit the wiki to learn more!
