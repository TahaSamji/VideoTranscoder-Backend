using System;
using System.IO;
using System.Threading.Tasks;

public class LocalCleanerService
{
    public async Task CleanDirectoryContentsAsync(string directoryPath)
    {

        Console.WriteLine($"DirectoryPAth given {directoryPath}");
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"⚠️ Directory does not exist: {directoryPath}");
                return;
            }

            // Delete all files
            // foreach (var filePath in Directory.GetFiles(directoryPath))
            // {
            //     try
            //     {
            //         File.Delete(filePath);
            //         Console.WriteLine($"🗑️ Deleted file: {filePath}");
            //     }
            //     catch (IOException ex)
            //     {
            //         Console.WriteLine($"❌ Could not delete file (locked?): {filePath}. {ex.Message}");
            //     }
            //     catch (UnauthorizedAccessException ex)
            //     {
            //         Console.WriteLine($"🚫 Permission denied for file: {filePath}. {ex.Message}");
            //     }
            // }

            try
            {
                Directory.Delete(directoryPath, recursive: true); // Will fail if contents still exist
                Console.WriteLine($"🗑️ Deleted root directory: {directoryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not delete root directory: {directoryPath}. {ex.Message}");
            }

            // Delete all subdirectories
            // foreach (var dirPath in Directory.GetDirectories(directoryPath))
            // {
            //     try
            //     {
            //         Directory.Delete(dirPath, recursive: true);
            //         Console.WriteLine($"🗑️ Deleted subdirectory: {dirPath}");
            //     }
            //     catch (IOException ex)
            //     {
            //         Console.WriteLine($"❌ Could not delete directory (locked?): {dirPath}. {ex.Message}");
            //     }
            //     catch (UnauthorizedAccessException ex)
            //     {
            //         Console.WriteLine($"🚫 Permission denied for directory: {dirPath}. {ex.Message}");
            //     }
            // }
            Console.WriteLine($"Cleaner Running ::");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ General error cleaning directory: {directoryPath}. {ex.Message}");
            throw;
        }
    }
}
