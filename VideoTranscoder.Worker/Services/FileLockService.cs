using System.Collections.Concurrent;
using VideoTranscoder.VideoTranscoder.Application.enums;

public class FileLockService
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, FileStatus> _statuses = new();

    public FileStatus GetStatus(string filePath)
    {
        return _statuses.TryGetValue(filePath, out var status) ? status : FileStatus.NotExists;
    }

    public async Task WaitUntilReadyAsync(string filePath)
    {
        while (true)
        {
            var status = GetStatus(filePath);
            
            if (status == FileStatus.Ready)
            {
                return; // File is ready
            }
            
            if (status == FileStatus.Downloading)
            {
                // Wait a bit and check again
                await Task.Delay(100);
                continue;
            }
            
            // If NotExists, break out - caller should handle download
            break;
        }
    }

    public async Task<bool> TryAcquireDownloadLockAsync(string filePath)
    {
        var semaphore = _locks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync();
        
        try
        {
            // Check if another thread already downloaded it
            if (_statuses.TryGetValue(filePath, out var status) && status == FileStatus.Ready)
            {
                return false; // Already ready, no need to download
            }
            
            if (status == FileStatus.Downloading)
            {
                return false; // Already downloading
            }
            
            // Mark as downloading
            _statuses[filePath] = FileStatus.Downloading;
            return true; // Acquired lock for download
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void MarkReady(string filePath)
    {
        _statuses[filePath] = FileStatus.Ready;
    }

    public void Reset(string filePath)
    {
        _statuses[filePath] = FileStatus.NotExists;
    }

    // Method to release resources
    public void Cleanup(string filePath)
    {
        if (_locks.TryRemove(filePath, out var semaphore))
        {
            semaphore.Dispose();
        }
        _statuses.TryRemove(filePath, out _);
    }

}
