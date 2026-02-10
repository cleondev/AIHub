var builder = DistributedApplication.CreateBuilder(args);

var chatbotApi = builder.AddProject<Projects.AIHub_Api>("chatbot-api");
var productApi = builder.AddProject<Projects.AIHub_ProductApi>("product-api");

chatbotApi.WithReference(productApi);

builder.Build().Run();
