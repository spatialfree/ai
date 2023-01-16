global using System;
global using Microsoft.AspNetCore.Components;
global using Blazored.LocalStorage;
global using OpenAI;

var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddBlazoredLocalStorage();
    // builder.Services.Replace(ServiceDescriptor.Scoped<IJsonSerializer, NewtonSoftJsonSerializer>());

    // var apiKey = builder.Configuration["OpenAI:ApiKey"];
    // builder.Services.AddSingleton<OpenAIService>(sp => new OpenAIService(apiKey)); // singleton?


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
