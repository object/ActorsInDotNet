using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace S3Uploader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: S3Uploader <file_list_path>");
                return;
            }

            var fileListPath = args[0];
            if (!File.Exists(fileListPath))
            {
                Console.WriteLine($"Error: File not found: {fileListPath}");
                return;
            }

            // TODO: Replace with your bucket name and region
            var bucketName = "your-s3-bucket-name";
            var region = RegionEndpoint.USEast1; 

            using var s3Client = new AmazonS3Client(region);
            var fileTransferUtility = new TransferUtility(s3Client);

            var filePaths = await File.ReadAllLinesAsync(fileListPath);

            foreach (var filePath in filePaths)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                try
                {
                    var fileInfo = new FileInfo(filePath);
                    if (!fileInfo.Exists)
                    {
                        Console.WriteLine($"Skipping non-existent file: {filePath}");
                        continue;
                    }

                    var key = fileInfo.Name; // Use the file name as the S3 object key

                    Console.WriteLine($"Uploading {filePath} to S3 bucket {bucketName}...");
                    await fileTransferUtility.UploadAsync(filePath, bucketName, key);
                    Console.WriteLine($"Successfully uploaded {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading {filePath}: {ex.Message}");
                }
            }
        }
    }
}