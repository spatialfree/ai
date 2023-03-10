global using System;
global using System.Collections.ObjectModel;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.HttpOverrides;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;

global using MetaMask.Blazor;

global using OpenAI;
global using OpenAI.Completions;
global using Microsoft.JSInterop;
using ai;

var builder = WebApplication.CreateBuilder(args);
	builder.Services.AddRazorPages();
	builder.Services.AddServerSideBlazor();
	builder.Services.AddMetaMaskBlazor();
	// get environment variable
	builder.Services.AddSingleton<Mono>(
		new Mono(builder.Configuration["OpenAI:ApiKey"], Environment.GetEnvironmentVariable("OpenAIApiKey"))
	);
	// builder.Services.AddScoped<IJSRuntime, JSRuntime>();

var app = builder.Build();
	
	app.UseForwardedHeaders(new ForwardedHeadersOptions {
		ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
	});

	if (!app.Environment.IsDevelopment()) { // Configure the HTTP request pipeline.
		app.UseExceptionHandler("/Error");
		// app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
		Console.WriteLine("Running in production mode");
	}

	app.UseStaticFiles();
	// app.UseHttpsRedirection();

	app.UseRouting();
	app.MapBlazorHub();
	app.MapFallbackToPage("/_Host");
	app.Run();
