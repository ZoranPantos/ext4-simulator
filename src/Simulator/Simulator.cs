using System;

namespace OperatingSystemsProject2019;

public class Simulator
{
    public static void Main(string[] args)
    {
        Console.Title = "FS SIM Terminal Window";
        Console.SetWindowSize(70, 40);

        try
        {
            SystemStorage disk = new SystemStorage();
            Terminal terminal = new Terminal(disk);
            terminal.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
    }
}