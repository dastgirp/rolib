using System;
using Ragnarok.IO.Compression;

namespace GrfListFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter a filename: ");
            string name = Console.ReadLine();

            using (GrfArchive grf = GrfArchive.Open(name, true))
            {
                foreach (GrfItem item in grf.Items)
                {
                    Console.WriteLine(item.FullName);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
