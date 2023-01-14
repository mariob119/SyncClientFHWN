using System;
using System.IO.Pipes;
using System.Text;

namespace Logging
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream("LoggingPipe"))
                {
                    byte[] buffer = new byte[10000];
                    namedPipeClient.Connect();
                    namedPipeClient.Read(buffer);
                    string str = Encoding.ASCII.GetString(buffer);
                    namedPipeClient.Flush();
                    namedPipeClient.Close();
                    Console.Clear();
                    Console.WriteLine(str);
                }
            }
        }
    }
}