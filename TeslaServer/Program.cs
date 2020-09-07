using System;

namespace TeslaServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TeslaServer server = new TeslaServer(8844);
            server.Run();
        }
    }
}
