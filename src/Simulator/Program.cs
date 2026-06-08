using System;
using Ext4FileSystemSimulation;

Console.Title = "FS SIM Terminal Window";
Console.SetWindowSize(70, 40);

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