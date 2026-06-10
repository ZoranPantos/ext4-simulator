using Ext4FileSystemSimulation;
using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using Ext4FileSystemSimulation.Strategies.ValidationStrategies;
using Microsoft.Extensions.DependencyInjection;
using System;

Console.Title = "ext4 simulator shell";

if (OperatingSystem.IsWindows() && !Console.IsOutputRedirected)
{
    try
    {
        Console.SetWindowSize(70, 40);
    }
    catch (Exception)
    {
        // Window resizing is cosmetic; ignore if the console refuses it.
    }
}

try
{
    var services = new ServiceCollection();

    services.AddSingleton<SystemStorage>();
    services.AddSingleton<ITerminalContext, TerminalContext>();

    services.AddSingleton(provider => new PathValidationStrategy(provider.GetRequiredService<ITerminalContext>(), "2"));
    services.AddSingleton<DirectoryCreationValidationStrategy>();
    services.AddSingleton<TwoPathValidationStrategy>();

    services.AddSingleton<ICommandStrategy, NoArgumentCommandStrategy>();
    services.AddSingleton<ICommandStrategy, OneArgumentCommandStrategy>();
    services.AddSingleton<ICommandStrategy, TwoArgumentCommandStrategy>();

    services.AddSingleton<Terminal>();

    using var provider = services.BuildServiceProvider();

    provider.GetRequiredService<Terminal>().Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
