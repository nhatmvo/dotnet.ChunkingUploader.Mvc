using dotnet.LargeFileUploader.Events;

namespace dotnet.LargeFileUploader.Services
{
    public class ChunkStorageService : IChunkStorageService
    {
        public event EventHandler? DownloadCompleted;

        protected virtual void OnDownloadCompleted(EventArgs evt)
        {
            DownloadCompleted?.Invoke(this, evt);
        }

        public void ChunkDownloadHandler(string fileName, int sequence, int total, IFormFile file)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var tempFolder = CreateTempoFolder(fileNameWithoutExt);
            var chunkFileNameBySequence = fileNameWithoutExt + "_" + sequence.ToString();
            
            using (FileStream stream = new FileStream(Path.Combine(tempFolder, chunkFileNameBySequence), FileMode.Create))
            {
                file.CopyTo(stream);
            }
            OnDownloadCompleted(new DownloadCompletedEventArgs()
            {
                FileName = fileName,
                Sequence = sequence,
                TimeReached = DateTime.UtcNow,
                Total = total
            });
        }

        private string CreateTempoFolder(string fileNameWithoutExt)
        {
            string path = Path.Combine("Downloads", fileNameWithoutExt + "_temp");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return Path.GetFullPath(path);
        }
    }
}
