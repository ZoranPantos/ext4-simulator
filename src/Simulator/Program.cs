using System;
using Ext4FileSystemSimulation;

Console.Title = "FS SIM Terminal Window";

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
    var disk = new SystemStorage();
    var terminal = new Terminal(disk);

    terminal.Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}