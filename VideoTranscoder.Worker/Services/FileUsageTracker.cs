// using System.Collections.Concurrent;

// namespace VideoTranscoder.VideoTranscoder.Worker.Services
// {
//     public static class FileUsageTracker
//     {
//         private class FileUsageEntry
//         {
//             public int Count;
//             public readonly object Lock = new object();
//         }

//         private static readonly ConcurrentDictionary<string, FileUsageEntry> _usage = new();

//         public static void Increment(string filePath)
//         {
//             var entry = _usage.GetOrAdd(filePath, _ => new FileUsageEntry());

//             lock (entry.Lock)
//             {
//                 entry.Count++;
//                 Console.WriteLine($"📈 Incremented usage for: {filePath} | Count: {entry.Count}");
//             }
//         }

//         public static void Decrement(string filePath)
//         {
//             if (!_usage.TryGetValue(filePath, out var entry))
//             {
//                 Console.WriteLine($"⚠️ Attempted to decrement usage for unknown file: {filePath}");
//                 return;
//             }

//             lock (entry.Lock)
//             {
//                 entry.Count--;
//                 Console.WriteLine($"📉 Decremented usage for: {filePath} | Count: {entry.Count}");

//                 if (entry.Count <= 0)
//                 {
//                     _usage.TryRemove(filePath, out _);

//                     if (File.Exists(filePath))
//                     {
//                         try
//                         {
//                             File.Delete(filePath);
//                             Console.WriteLine($"🗑️ File deleted after last usage: {filePath}");
//                         }
//                         catch (Exception ex)
//                         {
//                             Console.WriteLine($"❌ Failed to delete file: {filePath}. {ex.Message}");
//                         }
//                     }
//                     else
//                     {
//                         Console.WriteLine($"⚠️ Tried to delete file that doesn't exist: {filePath}");
//                     }
//                 }
//             }
//         }
//     }
// }
