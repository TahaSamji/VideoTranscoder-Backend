using System;
using System.IO;
using System.Threading.Tasks;

public class LocalCleanerService
{
    public async Task CleanDirectoryContentsAsync(string directoryPath)
    {
         string currentDir = Directory.GetCurrentDirectory();
         directoryPath = Path.Combine(currentDir, directoryPath);
                
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"⚠️ Directory does not exist: {directoryPath}");
                return;
            }

            // Delete all files
            foreach (var filePath in Directory.GetFiles(directoryPath))
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine($"🗑️ Deleted file: {filePath}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"❌ Could not delete file (locked?): {filePath}. {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"🚫 Permission denied for file: {filePath}. {ex.Message}");
                }
            }

            // Delete all subdirectories
            foreach (var dirPath in Directory.GetDirectories(directoryPath))
            {
                try
                {
                    Directory.Delete(dirPath, recursive: true);
                    Console.WriteLine($"🗑️ Deleted subdirectory: {dirPath}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"❌ Could not delete directory (locked?): {dirPath}. {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"🚫 Permission denied for directory: {dirPath}. {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ General error cleaning directory: {directoryPath}. {ex.Message}");
            throw;
        }
    }
}
