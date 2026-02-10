var builder = DistributedApplication.CreateBuilder(args);

var chatbotApi = builder.AddProject("chatbot-api", "../AIHub.Api/AIHub.Api.csproj");
var productApi = builder.AddProject("product-api", "../AIHub.ProductApi/AIHub.ProductApi.csproj");

chatbotApi.WithReference(productApi);

builder.Build().Run();
