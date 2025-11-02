
using Akka.Actor;
using Akka.Routing;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AkkaS3Uploader
{
    // The message containing the file path
    public sealed class FilePath
    {
        public string Path { get; }

        public FilePath(string path)
        {
            Path = path;
        }
    }

    // Actor responsible for forwarding the file path to the S3 actor pool
    public class FileActor : ReceiveActor
    {
        private readonly IActorRef _s3ActorPool;

        public FileActor(IActorRef s3ActorPool)
        {
            _s3ActorPool = s3ActorPool;

            Receive<FilePath>(filePath =>
            {
                Console.WriteLine($"FileActor for {filePath.Path} received message.");
                _s3ActorPool.Tell(filePath);
                // Stop the actor after it has done its job
                Context.Stop(Self);
            });
        }
    }

    // Actor responsible for uploading the file to S3
    public class S3Actor : ReceiveActor
    {
        public S3Actor()
        {
            Receive<FilePath>(filePath =>
            {
                // TODO: Replace with your bucket name and region
                var bucketName = "your-s3-bucket-name";
                var region = RegionEndpoint.USEast1;

                using var s3Client = new AmazonS3Client(region);
                var fileTransferUtility = new TransferUtility(s3Client);

                try
                {
                    var fileInfo = new FileInfo(filePath.Path);
                    if (!fileInfo.Exists)
                    {
                        Console.WriteLine($"Skipping non-existent file: {filePath.Path}");
                        return;
                    }

                    var key = fileInfo.Name; // Use the file name as the S3 object key

                    Console.WriteLine($"Uploading {filePath.Path} to S3 bucket {bucketName}...");
                    fileTransferUtility.Upload(filePath.Path, bucketName, key);
                    Console.WriteLine($"Successfully uploaded {filePath.Path}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading {filePath.Path}: {ex.Message}");
                }
            });
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: AkkaS3Uploader <file_list_path>");
                return;
            }

            var fileListPath = args[0];
            if (!File.Exists(fileListPath))
            {
                Console.WriteLine($"Error: File not found: {fileListPath}");
                return;
            }

            // Create the actor system
            using var system = ActorSystem.Create("S3UploaderSystem");

            // Create the S3 actor pool
            var s3ActorPool = system.ActorOf(Props.Create<S3Actor>().WithRouter(new RoundRobinPool(10)), "s3-pool");

            var filePaths = await File.ReadAllLinesAsync(fileListPath);

            foreach (var filePath in filePaths)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                // Create a FileActor for each file and send it the file path
                var fileActor = system.ActorOf(Props.Create(() => new FileActor(s3ActorPool)));
                fileActor.Tell(new FilePath(filePath));
            }

            // Keep the application running until all actors have finished
            await system.WhenTerminated;
        }
    }
}
