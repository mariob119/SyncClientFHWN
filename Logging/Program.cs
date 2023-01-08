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
                using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream("test-pipe"))
                {
                    byte[] buffer = new byte[1024];
                    namedPipeClient.Connect();
                    namedPipeClient.Read(buffer);
                    string str = Encoding.ASCII.GetString(buffer);
                    Console.WriteLine(str);
                }
            }
        }
    }
}