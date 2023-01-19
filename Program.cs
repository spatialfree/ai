global using System;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;

global using Microsoft.JSInterop;

global using OpenAI;
global using OpenAI.Embeddings;

var builder = WebApplication.CreateBuilder(args);
	builder.Services.AddRazorPages();
	builder.Services.AddServerSideBlazor();
	// builder.Services.AddBlazoredLocalStorage();
	builder.Services.AddSingleton<Mono>();
	// builder.Services.AddScoped<IJSRuntime, JSRuntime>();

	// var apiKey = builder.Configuration["OpenAI:ApiKey"];
	// builder.Services.AddSingleton<OpenAIService>(sp => new OpenAIService(apiKey));


var app = builder.Build();
	if (!app.Environment.IsDevelopment()) { // Configure the HTTP request pipeline.
		app.UseExceptionHandler("/Error");
		app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	}
	app.UseHttpsRedirection();
	app.UseStaticFiles();
	app.UseRouting();
	app.MapBlazorHub();
	app.MapFallbackToPage("/_Host");
app.Run();


public class Mono {
	private Dictionary<string, int> data = new Dictionary<string, int>();

	public void Add(string key, int value) {
		data.Add(key, value);
	}

	public int Get(string key) {
		return data.GetValueOrDefault(key);
	}
}

public class Pattern {
	public Post[] posts = new Post[6]; // temp length
}

public class Post {
	public bool active = false;

	// does every post need a name
	// auto named button
	// user facing label vs packaged with the prompt for the ai
	public string name = "";
	public string prompt = "";

	// post's do not link to one another
	// rather we have threads (w/splitters and other control flow logic)
	// that exist on top

	// color?
	public string color = "";
}