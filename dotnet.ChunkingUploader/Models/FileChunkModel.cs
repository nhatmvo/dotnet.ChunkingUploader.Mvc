namespace dotnet.LargeFileUploader.Models
{
    public class FileChunkModel
    {
        public string FileName { get; set; }
        public int Sequence { get; set; }
        public int Total { get; set; }
        public IFormFile File { get; set; }
    }
}
