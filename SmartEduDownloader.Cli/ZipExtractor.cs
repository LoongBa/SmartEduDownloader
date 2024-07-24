using System.IO.Compression;

namespace SmartEduDownloader.Cli;

public class ZipExtractor
{
    public static void ExtractZipFileIgnoringFirstDirectory(string zipFilePath, string extractPath)
    {
        try
        {
            // Ensure the target directory exists
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            using (var archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Skip directories
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    // Get the relative path ignoring the first directory
                    var relativePath = GetRelativePathIgnoringFirstDirectory(entry.FullName);

                    // Combine the extract path with the relative path
                    var destinationPath = Path.Combine(extractPath, relativePath);

                    // Ensure the directory for the file exists
                    var destinationDir = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    // Extract the file
                    entry.ExtractToFile(destinationPath, overwrite: true);
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occurred while extracting the zip file: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static string GetRelativePathIgnoringFirstDirectory(string fullPath)
    {
        // Split the path into parts
        string[] parts = fullPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        // Join the parts ignoring the first directory
        return string.Join(Path.DirectorySeparatorChar.ToString(), parts, 1, parts.Length - 1);
    }
}