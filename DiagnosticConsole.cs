using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        await DiagnosticRunner.RunDiagnosticsAsync();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
