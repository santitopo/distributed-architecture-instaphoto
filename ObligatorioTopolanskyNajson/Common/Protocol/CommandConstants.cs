namespace ProtocolLibrary
{
    public static class CommandConstants
    {
        public const int Login =  1;
        public const int Register =  2;
        public const int ListUsers = 3;
        public const int UploadPicture = 4;
        public const int ListPhotos = 5;
        public const int GetComments = 6;
        public const int AddComment = 9;
        
        public const int Exit = 7;
        public const int Error = 99;
        public const int OK = 0;
    }

    public static class LogConstants
    {
        public const string Info = "INFO";
        public const string Warning = "WARNING";
        public const string Error = "ERROR";
    }
}