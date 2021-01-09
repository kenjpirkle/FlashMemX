using System;
using System.IO;

namespace FlashMemX.Testing
{
    static class Program
    {
        private static void Main(string[] args)
        {
            string databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "testdb.db");

            Console.ReadKey();
        }
    }
}
