namespace dotnet.LargeFileUploader.Services
{
    public interface IFileStorageService
    {
        public void FormFileHandling(string fileName, int sequence, int total, IFormFile file);
    }
}
