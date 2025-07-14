using System.Collections.Concurrent;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    public static class FileUsageTracker
    {
        private static readonly ConcurrentDictionary<string, int> _usageCount = new();

        public static void Increment(string filePath)
        {
            var count = _usageCount.AddOrUpdate(filePath, 1, (_, current) => current + 1);
            Console.WriteLine($"📈 Incremented usage for: {filePath} | Count: {count}");
        }

        public static void Decrement(string filePath)
        {
            if (_usageCount.TryGetValue(filePath, out var count))
            {
                if (count <= 1)
                {
                    _usageCount.TryRemove(filePath, out _);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Console.WriteLine($"🗑️ File deleted after last usage: {filePath}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Tried to delete file that doesn't exist: {filePath}");
                    }
                }
                else
                {
                    var newCount = _usageCount.AddOrUpdate(filePath, 0, (_, current) => current - 1);
                    Console.WriteLine($"📉 Decremented usage for: {filePath} | Count: {newCount}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Attempted to decrement usage for unknown file: {filePath}");
            }
        }
    }
}
