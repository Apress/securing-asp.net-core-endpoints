using Grpc.Core;
using Grpc.Net.Client;
using GrpcServerApp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesApp.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
        Message = string.Empty;
    }

    public string Message { get; set; }

    public async Task OnGet()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        var metadata = new Metadata
        {
            { "Authorization", $"Bearer {accessToken}" }
        };

        using var channel = GrpcChannel.ForAddress("https://localhost:7203");
        var client = new Greeter.GreeterClient(channel);

        var request = new HelloRequest
        {
            Name = "User"
        };

        var response = await client.SayHelloAsync(request, metadata);

        Message = response.Message;
    }
}