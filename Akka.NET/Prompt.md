# Generate code in C# for console application that reads a list of files and upload the files from the list to Amazon S3 storage using Akka.NET actor model library.

## Requirements

1. The code is written in C#.
2. The file S3Uploader.cs contains a working console application that uploads files to S3 storage but doesn't use actor model framework. The application has a single command line argument: a filename of a document that contains a list of file paths, Each of the items in that list represents a file that is uploaded to Amazon S3 storage.
3. Using S3Uploader as a reference, create a similar application that uploads files using Akka.NET.
4. The application should upload files to S3 in two phases:
    - First, each file should be sent to its own File actor, the message consists of a file path. File actor is just an intermediate actor that forwards the file reference to an S3 actor.
    - S3 actors form an actor pool using Pool router. The pool size is 10. Upon receiving a message that consists of a file path, S3 actor uploads the specified file to S3 storage.