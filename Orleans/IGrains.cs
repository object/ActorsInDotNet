using Orleans;
using System.Threading.Tasks;

namespace OrleansS3Uploader
{
    public interface IFileGrain : IGrainWithStringKey
    {
        Task ProcessFile(string filePath);
    }

    public interface IS3Grain : IGrainWithIntegerKey
    {
        Task UploadFile(string filePath);
    }
}
