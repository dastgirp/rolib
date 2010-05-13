using System;
using System.IO;
using Ragnarok.IO.Compression;

namespace GrfAddFilesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter a filename: ");
            string name = Console.ReadLine();
            Console.Write("Please enter a folder to add the files from: ");
            string folder = Console.ReadLine();

            using (GrfArchive grf = new GrfArchive(name))
            {
                DirectoryInfo dir = new DirectoryInfo(folder);
                AddFiles(grf, dir, dir);
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void AddFiles(GrfArchive grf, DirectoryInfo dir, DirectoryInfo root)
        {
            foreach (FileInfo file in dir.GetFiles())
            {
                Console.Write("Adding '{0}'... ", file.Name);
                grf.Items.AddFile(file.FullName.Substring(root.FullName.Length + 1), file.FullName);
                Console.WriteLine("added.");
            }

            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                AddFiles(grf, d, root);
            }
        }
    }
}
