using dotnet.LargeFileUploader.Events;
using System.Collections.Concurrent;

namespace dotnet.LargeFileUploader.Services
{
    public class FileStorageService : IFileStorageService
    {
        // temporary buffer storage used to know which file contains which chunks, will do cleanup when all chunks are merged into file.
        // Concurrentcy Dictionary is used because of request send from client using ajax is asynchronous by default
        // just a memory storage solution and it will be cleaned up each time application starts or stops
        // should consider persistence storage solution to save this kind of data
        private readonly ConcurrentDictionary<string, Dictionary<int, bool>> bufferFilesChunks = new ConcurrentDictionary<string, Dictionary<int, bool>>();
        private readonly ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();
        private readonly IChunkStorageService _chunkStorageService;

        public FileStorageService(IChunkStorageService chunkStorageService)
        {
            _chunkStorageService = chunkStorageService;
            _chunkStorageService.DownloadCompleted += c_DownloadChunkCompletedEventHandler;
        }

        public void FormFileHandling(string fileName, int sequence, int total, IFormFile file)
        {
            bufferFilesChunks.AddOrUpdate(fileName, new Dictionary<int, bool>() { { sequence, false } }, (k, v) =>
            {
                v.Add(sequence, false);
                return v;
            });
            InitialLockForFile(fileName);
            _chunkStorageService.ChunkDownloadHandler(fileName, sequence, total, file);
        }

        private void c_DownloadChunkCompletedEventHandler(object? sender, EventArgs evt)
        {
            var completedEvent = evt as DownloadCompletedEventArgs;
            if (completedEvent == null) return;

            var currentChunksStatus = new Dictionary<int, bool>();
            var canAccessCurrentChunksStatus = bufferFilesChunks.TryGetValue(completedEvent.FileName, out currentChunksStatus);
            if (!canAccessCurrentChunksStatus || currentChunksStatus == null) return;

            var updatingChunksStatus = currentChunksStatus;
            updatingChunksStatus[completedEvent.Sequence] = true;

            bufferFilesChunks.AddOrUpdate(completedEvent.FileName, new Dictionary<int, bool>() { { completedEvent.Sequence, true } }, (k, v) =>
            {
                v[completedEvent.Sequence] = true;
                return v;
            });
            if (!IsMergeProcessAvailable(completedEvent.FileName, completedEvent.Total)) return;

            lock (locks[completedEvent.FileName])
            {
                if (bufferFilesChunks.ContainsKey(completedEvent.FileName))
                {
                    ExecuteMergeProcess(completedEvent.FileName);
                }
            }
        }

        private void InitialLockForFile(string fileName)
        {
            locks.TryAdd(fileName, new object());
        }

        private void ExecuteMergeProcess(string fileName)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string tempPath = Path.Combine("Downloads", fileNameWithoutExt + "_temp");
            string mergePath = Path.Combine("Downloads", fileName);
            int total = bufferFilesChunks[fileName].Count;

            if (!File.Exists(mergePath))
                File.Create(mergePath).Dispose();
            try
            {
                for (int i = 0; i < total; i++)
                {
                    var chunkFilename = Path.Combine(tempPath, fileNameWithoutExt + "_" + i.ToString());
                    var chunkBytes = File.ReadAllBytes(chunkFilename);
                    using (var stream = new FileStream(mergePath, FileMode.Append))
                    {
                        stream.Write(chunkBytes, 0, chunkBytes.Length);
                    }
                }
            }
            finally
            {
                CleanUp(fileName);
            }
        }
        private void CleanUp(string fileName)
        {
            var valueToBeRemoved = new Dictionary<int, bool>();
            // Clear data from buffer as well as temp folder
            bufferFilesChunks.TryRemove(fileName, out valueToBeRemoved);
            //Directory.Delete(Path.Combine("Downloads", Path.GetFileNameWithoutExtension(fileName) + "_temp"));
        }

        private bool IsMergeProcessAvailable(string fileName, int totalChunks)
        {
            var chunksStatus = bufferFilesChunks.Where(c => c.Key.Equals(fileName)).FirstOrDefault();
            return chunksStatus.Value.Where(c => c.Value).Count() == totalChunks;
        }
    }
}
