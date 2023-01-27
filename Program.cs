global using System;
global using System.Collections.ObjectModel;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;

global using Microsoft.JSInterop;

global using OpenAI;
global using OpenAI.Embeddings;

using ai;

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
