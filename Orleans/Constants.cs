namespace OrleansS3Uploader
{
    public static class Constants
    {
        public const string S3_BUCKET_NAME = "your-orleans-s3-upload-bucket"; // TODO: Change to your S3 bucket name
        public const int NUMBER_OF_S3_GRAINS = 10; // Configurable number of S3 grains for concurrent uploads
    }
}
