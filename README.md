# Identity

Boa Identity extends ASP.NET Core Identity allowing:

* the addition of new reset password methods by implementing the `IResetPasswordService` interface.
* the use of a [telegram bot](https://telegram.org/) to interact with users.

## Description

The following contains a description of each sub-directory.

* `EntityFrameworkTelegram`: Contains extensions to EntityFrameworkCore to store users telegram data.
* `Extensions.Core`: Contains the extensions to Core Identity for reset password service.
* `Extensions.Telegram`: Contains abstractions, types, and implementations for telegram bot.
* `Test`: Contains a webapp used for functional testing.
* `UI`: Contains compiled Razor UI components for use in ASP.NET Core Identity.

## Development Setup

### Build

To build this specific project from source, is needed the nuget package generated from my other project [TelegramBotService](https://github.com/boajr/TelegramBotService).

The fasted way to allow Visual Studio to find the package, is to create a local NuGet Repository following this step:
1. Copy the NuGet package generated in <project-root>\\TelegramBotService\\Bin\\Release\\*.nupkg into a folder that will be the local NuGet repository (eg. M:\\nuget.local).
2. Add the new folder to Visual Studio NuGet Package Sources: open the options dialog (Tools -> NuGet Packages Manager -> Package Manager Setting) and in Package Sources tab add a new source.