namespace dotnet.LargeFileUploader.Events
{
    public class DownloadCompletedEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public DateTime TimeReached { get; set; }
        public int Total { get; set; }
        public int Sequence { get; set; }
    }
}
