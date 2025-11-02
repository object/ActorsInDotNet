
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Proto;
using Proto.Router;

namespace ProtoS3Uploader
{
    // Define messages
    public record UploadFile(string FilePath);

    // S3Actor is responsible for uploading a single file to S3
    public class S3Actor : IActor
    {
        private readonly string _bucketName;
        private readonly RegionEndpoint _region;

        public S3Actor(string bucketName, RegionEndpoint region)
        {
            _bucketName = bucketName;
            _region = region;
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is UploadFile message)
            {
                await UploadToS3(message.FilePath);
            }
        }

        private async Task UploadToS3(string filePath)
        {
            try
            {
                using var s3Client = new AmazonS3Client(_region);
                var fileTransferUtility = new TransferUtility(s3Client);

                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    Console.WriteLine($"Skipping non-existent file: {filePath}");
                    return;
                }

                var key = fileInfo.Name;

                Console.WriteLine($"Uploading {filePath} to S3 bucket {_bucketName}...");
                await fileTransferUtility.UploadAsync(filePath, _bucketName, key);
                Console.WriteLine($"Successfully uploaded {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading {filePath}: {ex.Message}");
            }
        }
    }

    // FileActor is an intermediate actor that forwards the file reference to an S3 actor
    public class FileActor : IActor
    {
        private readonly PID _s3Router;

        public FileActor(PID s3Router)
        {
            _s3Router = s3Router;
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is UploadFile message)
            {
                context.Send(_s3Router, message);
            }
            return Task.CompletedTask;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ProtoS3Uploader <file_list_path>");
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

            var system = new ActorSystem();

            // Create a pool of S3 actors
            var s3Props = Props.FromProducer(() => new S3Actor(bucketName, region));
            var s3Router = system.Root.Spawn(system.Root.NewRoundRobinPool(s3Props, 10));

            var filePaths = await File.ReadAllLinesAsync(fileListPath);

            foreach (var filePath in filePaths)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                // For each file, create a FileActor and send it the path
                var fileActorProps = Props.FromProducer(() => new FileActor(s3Router));
                var fileActor = system.Root.Spawn(fileActorProps);
                system.Root.Send(fileActor, new UploadFile(filePath));
            }

            // Wait for user input to exit
            Console.WriteLine("All file upload jobs have been dispatched. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
