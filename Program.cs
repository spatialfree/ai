using System.Threading.RateLimiting;
using OpenAI;
using OpenAI.Models;
using OpenAI.Images;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRateLimiter(options => {
	options.AddPolicy("NotSoFast", httpContext => {
		return RateLimitPartition.GetFixedWindowLimiter(
			httpContext.Connection.Id,
			key => new FixedWindowRateLimiterOptions() {
				Window = TimeSpan.FromSeconds(3),
				PermitLimit = 3,
				QueueLimit = 1
			}
		);
	});
});
builder.Services.AddRazorPages();

var api = new OpenAIClient(
	new OpenAIAuthentication(builder.Configuration["OpenAI:ApiKey"])
);
// builder.Services.AddOpenAIService(settings => { 
// 	settings.ApiKey = builder.Configuration["OpenAI:ApiKey"];
// });
// ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
// var openAIService = serviceProvider.GetRequiredService<IOpenAIService>();


var app = builder.Build();
app.UseRateLimiter();
app.MapRazorPages();
app.UseStaticFiles();
// app.UseRouting();
// app.UseAuthorization();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Error");
}



app.MapGet("/mono", (string prompt) => {

	CompletionRequest request = new CompletionRequest(
		Model.Davinci,
		prompt, 
		max_tokens: 256, 
		temperature: 0.7,
		frequencyPenalty: 0.5,
		presencePenalty: 0.5,
		stopSequences: new string [] {".\n"} //\n\n
	);
	
	async IAsyncEnumerable<string>StreamCompletion() {
		await foreach (var token in api.CompletionsEndpoint.StreamCompletionEnumerableAsync(request)) {
			yield return token.Completions[0].Text;
			// await Task.Delay(500);
		}
	}
	
	return StreamCompletion();

	
	// var completionResult = await api.CompletionsEndpoint.CreateCompletionAsync(request);
	// var prompt = completionResult.Completions[0].Text;

	// var imageResult = await api.ImagesEndPoint.GenerateImageAsync(
	// 	prompt, 1, ImageSize.Small
	// );
	// var image = imageResult[0];

	// return new { prompt, image };
}); // .RequireRateLimiting("NotSoFast");

app.Run();