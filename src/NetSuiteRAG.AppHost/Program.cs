var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL with pgAdmin
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var netsuitedb = postgres.AddDatabase("netsuitedb");

// Add Redis
var redis = builder.AddRedis("redis");

// Use existing local Qdrant instance (localhost:6333)
// Note: Qdrant is already running locally, so we just configure the connection

// Add the API project
var api = builder.AddProject<Projects.NetSuiteRAG_Api>("api")
    .WithReference(netsuitedb)
    .WithReference(redis)
    .WithEnvironment("Qdrant__Endpoint", "http://localhost:6333");

builder.Build().Run();
