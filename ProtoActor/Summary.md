Code Explanation

  The application consists of three main parts:

   1. `Program.cs` (in `ProtoS3Uploader.cs`): This is the main entry point of the application. It initializes the actor system, creates a pool of 
      S3Actor instances, reads the file list, and for each file, it creates a FileActor to handle the upload process.

   2. `FileActor`: This actor acts as an intermediary. It receives a message with a file path and forwards it to the S3Actor pool. This separation 
      of concerns makes the system more modular and easier to maintain.

   3. `S3Actor`: This actor is responsible for the actual file upload to S3. It receives a message with a file path, and then uses the AWS SDK to 
      upload the file to the specified S3 bucket. The application creates a pool of 10 S3Actor instances, allowing for up to 10 concurrent uploads.

  Actor Model Implementation

  The application uses the Proto.Actor framework to implement the actor model. Here's how it works:

   * Actor System: The ActorSystem is the container for all actors. It provides a way to create, manage, and communicate with actors.
   * Actors: Actors are the fundamental building blocks of the application. They are lightweight, concurrent entities that communicate with each 
     other by sending and receiving messages.
   * Messages: Messages are immutable data structures that are sent between actors. In this application, the UploadFile record is used as a 
     message to carry the file path.
   * PIDs (Process IDs): Each actor has a unique PID that is used to send messages to it.
   * Routers: The application uses a RoundRobinPool router to distribute the upload tasks evenly among the S3Actor instances. This allows for 
     concurrent file uploads and improves the overall performance of the application.

  This actor-based approach provides several benefits over the traditional, non-actor-based implementation:

   * Concurrency: The actor model makes it easy to write concurrent code without having to deal with the complexities of threads and locks.
   * Scalability: The application can be easily scaled by increasing the number of S3Actor instances in the pool.
   * Fault Tolerance: The actor model provides built-in fault tolerance mechanisms that can be used to handle errors and failures gracefully.

