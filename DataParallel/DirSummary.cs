using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class DirSummary
{
    public static void Main()
    {
        String[] args = Environment.GetCommandLineArgs();
        if (args.Length == 1)
        {
            Console.WriteLine("There are no command line arguments.");
            return;
        }
        if (!Directory.Exists(args[1]))
        {
            Console.WriteLine("The directory does not exist.");
            return;
        }

        Console.WriteLine("Directory '{0}':", args[1]);
        var results = CalculateDirectoryRecursive(args[1]);
        Console.WriteLine("{0:N0} files, {1:N0} bytes", results.Item1, results.Item2);
    }

    public static Tuple<int, long> CalculateDirectory(string dir)
    {
        // {{## BEGIN parallel-for ##}}
        long totalSize = 0;
        String[] files = Directory.GetFiles(dir);
        Parallel.For(0, files.Length,
                     index => {
                         FileInfo fi = new FileInfo(files[index]);
                         long size = fi.Length;
                         Interlocked.Add(ref totalSize, size);
                     });
        return new Tuple<int, long>(files.Length, totalSize);
        // {{## END parallel-for ##}}
    }
    public static Tuple<int,long> CalculateDirectoryRecursive(string dir)
    {
        // {{## BEGIN parallel-for-recursive ##}}
        long totalSize = 0;

        String[] dirs = Directory.GetDirectories(dir);
        Parallel.For(0, dirs.Length, 
                index =>
                {
                    DirectoryInfo di = new DirectoryInfo(dirs[index]);
                    var result = CalculateDirectoryRecursive(dirs[index]);
                    Interlocked.Add(ref totalSize, result.Item2);
                });

        String[] files = Directory.GetFiles(dir);
        Parallel.For(0, files.Length,
                     index => {
                         FileInfo fi = new FileInfo(files[index]);
                         long size = fi.Length;
                         Interlocked.Add(ref totalSize, size);
                     });
        return new Tuple<int, long>(files.Length, totalSize);
        // {{## END parallel-for-recursive ##}}
    }
}