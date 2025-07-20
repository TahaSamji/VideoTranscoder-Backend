using System;
using System.IO;
using System.Threading.Tasks;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    /// <summary>
    /// Service responsible for cleaning up local directories after processing.
    /// </summary>
    public class LocalCleanerService
    {
        /// <summary>
        /// Deletes the given directory and its contents recursively.
        /// </summary>
        /// <param name="directoryPath">Absolute path to the directory to be cleaned</param>
        public async Task CleanDirectoryContentsAsync(string directoryPath)
        {


            try
            {
                // Check if directory exists before attempting deletion
                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine($"⚠️ Directory does not exist: {directoryPath}");
                    return; // Exit if the directory is not found
                }

                try
                {
                    // Attempt to delete the entire directory and all subdirectories/files
                    Directory.Delete(directoryPath, recursive: true);
                    Console.WriteLine($"🗑️ Deleted root directory: {directoryPath}");
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during deletion
                    Console.WriteLine($"⚠️ Could not delete root directory: {directoryPath}. {ex.Message}");
                }

               
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions and rethrow after logging
                Console.WriteLine($"❌ General error cleaning directory: {directoryPath}. {ex.Message}");
                throw;
            }
        }
    }
}
