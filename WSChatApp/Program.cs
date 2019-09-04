using System;

namespace WSChatApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Server wsServer = new Server("http://localhost:9000/");
            wsServer.Start();
        }
    }
}
