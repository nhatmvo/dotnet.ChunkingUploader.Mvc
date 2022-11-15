namespace dotnet.LargeFileUploader.Services
{
    public interface IChunkStorageService
    {
        public void ChunkDownloadHandler(string fileName, int sequence, int total, IFormFile file);
        public event EventHandler? DownloadCompleted;
    }
}
