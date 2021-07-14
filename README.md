
## Technologies

* ASP.NET Core 5
* Entity Framework Core 5
* Angular 10
* MediatR
* FluentValidation
* NUnit, FluentAssertions, Moq & Respawn

## Getting Started

The easiest way to get started is as following: -

1. Install the latest [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
2. Navigate to `src/Web` and run `dotnet run` to launch the back end (ASP.NET Core Web API)

### First Time Running

In order to build and start the API, navigate to `src/Web` and run `dotnet build && dotnet run` or just `dotnet watch run`.
Then open http://localhost:5000 on your browser.

### Database Migrations

To use `dotnet-ef` for your migrations please add the following flags to your command (values assume you are executing from repository root)

* `--project src/Infrastructure`
* `--startup-project src/Web`
* `--output-dir Persistence/Migrations`

For example, to add a new migration from the root folder:

 `dotnet ef migrations add "SampleMigration" --project src\Infrastructure --startup-project src\Web --output-dir Persistence\Migrations`

## Overview

### Domain

This will contain all entities, enums, exceptions, interfaces, types and logic specific to the domain layer.

### Application

This layer contains all application logic. It is dependent on the domain layer, but has no dependencies on any other layer or project. This layer defines interfaces that are implemented by outside layers. For example, if the application need to access a notification service, a new interface would be added to application and an implementation would be created within infrastructure.

### Infrastructure

This layer contains classes for accessing external resources such as file systems, web services, smtp, and so on. These classes should be based on interfaces defined within the application layer.

### Web

This layer is a single page application based on Angular and ASP.NET Core. This layer depends on both the Application and Infrastructure layers, however, the dependency on Infrastructure is only to support dependency injection. Therefore only *Startup.cs* should reference Infrastructure.
