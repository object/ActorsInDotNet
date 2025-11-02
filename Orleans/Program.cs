using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

namespace OrleansS3Uploader
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: OrleansS3Uploader <file_list_path>");
                return 1;
            }

            string fileListPath = args[0];

            if (!File.Exists(fileListPath))
            {
                Console.WriteLine($"Error: File list not found at {fileListPath}");
                return 1;
            }

            try
            {
                var host = Host.CreateDefaultBuilder(args)
                    .UseOrleans(siloBuilder =>
                    {
                        siloBuilder.UseLocalhostClustering();
                        siloBuilder.AddMemoryGrainStorage("Default");
                    })
                    .ConfigureLogging(logging => logging.AddConsole())
                    .Build();
                await host.StartAsync();

                var client = host.Services.GetService(typeof(IGrainFactory)) as IGrainFactory;

                List<string> filesToUpload = (await File.ReadAllLinesAsync(fileListPath))
                                                .Where(f => !string.IsNullOrWhiteSpace(f))
                                                .ToList();
                Console.WriteLine($"Starting upload of {filesToUpload.Count} files using Orleans...");

                List<Task> uploadTasks = new List<Task>();
                foreach (string filePath in filesToUpload)
                {
                    // Each file gets its own FileGrain
                    IFileGrain fileGrain = client.GetGrain<IFileGrain>(filePath);
                    uploadTasks.Add(fileGrain.ProcessFile(filePath));
                }

                await Task.WhenAll(uploadTasks);

                Console.WriteLine("All files processed by FileGrains. Check logs for S3 upload status.");

                Console.WriteLine("OrleansS3Uploader finished. Press any key to exit...");
                Console.ReadKey();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError starting or running OrleansS3Uploader: {ex.Message}");
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }
   }
}