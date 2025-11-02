# Generate code in C# for console application that reads a list of files and upload the files from the list to Amazon S3 storage using Microsoft Orleans library.

## Requirements

1. The code is written in C#.
2. Use code that works with Microsoft Orleans version 7.0.
3. The file S3Uploader.cs contains a working console application that uploads files to S3 storage but doesn't use actor model framework. The application has a single command line argument: a filename of a document that contains a list of file paths, Each of the items in that list represents a file that is uploaded to Amazon S3 storage.
4. Using S3Uploader as a reference, create a similar application that uploads files using Microsoft Orleans.
5. The application should upload files to S3 in two phases:
    - First, each file should be sent to its own File grain (one grain per file), the message consists of a file path. File grain is just an intermediate step that forwards the file reference to an S3 grain.
    - There are multiple S3 grains (their number can be configured), so files can be uploaded concurrently. Upon receiving a message that consists of a file path, S3 grain uploads the specified file to S3 storage.