using System;
using System.Threading.Tasks;
using Orleans;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using Microsoft.Extensions.Logging;

namespace OrleansS3Uploader
{
    public class FileGrain : Grain, IFileGrain
    {
        private readonly ILogger<FileGrain> _logger;

        public FileGrain(ILogger<FileGrain> logger)
        {
            _logger = logger;
        }

        public async Task ProcessFile(string filePath)
        {
            _logger.LogInformation($"FileGrain {this.GetPrimaryKeyString()} received file: {filePath}");

            // Forward to an S3Grain. For simplicity, we'll use a random S3 grain.
            // In a real application, you might want a more sophisticated load balancing strategy.
            var s3GrainId = new Random().Next(0, Constants.NUMBER_OF_S3_GRAINS);
            var s3Grain = GrainFactory.GetGrain<IS3Grain>(s3GrainId);
            await s3Grain.UploadFile(filePath);

            _logger.LogInformation($"FileGrain {this.GetPrimaryKeyString()} forwarded {filePath} to S3Grain {s3GrainId}");
        }
    }

    public class S3Grain : Grain, IS3Grain
    {
        private readonly ILogger<S3Grain> _logger;
        private readonly IAmazonS3 _s3Client; // This will need to be injected or configured

        public S3Grain(ILogger<S3Grain> logger)
        {
            _logger = logger;
            // In a real application, _s3Client would be injected or configured securely.
            // For this example, we'll initialize it directly, assuming credentials are set up via environment variables or AWS config.
            _s3Client = new AmazonS3Client(); 
        }

        public async Task UploadFile(string filePath)
        {
            _logger.LogInformation($"S3Grain {this.GetPrimaryKeyString()} received file for upload: {filePath}");

            if (!File.Exists(filePath))
            {
                _logger.LogError($"File not found: {filePath}");
                return;
            }

            // Extract bucket name and key from the original S3Uploader.cs logic or assume defaults
            // For now, let's assume a default bucket and use the file name as the key.
            string bucketName = Constants.S3_BUCKET_NAME;
            string keyName = Path.GetFileName(filePath);

            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = filePath
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);
                _logger.LogInformation($"Successfully uploaded {filePath} to S3 bucket {bucketName} with key {keyName}. ETag: {response.ETag}");
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError($"Error encountered on server. Message:'{e.Message}' when writing an object");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unknown error encountered. Message:'{e.Message}' when writing an object");
            }
        }
    }
}
