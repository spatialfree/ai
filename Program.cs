global using System;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;

global using Microsoft.JSInterop;

global using OpenAI;
global using OpenAI.Embeddings;



using ai;
using System.IO;



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

	public Vec Shared = new Vec(20, 20);
	public string SharedText = "text";

	public void Add(string key, int value) {
		data.Add(key, value);
	}

	public int Get(string key) {
		return data.GetValueOrDefault(key);
	}

	// Manual Setup
	public void InitRecords() {
		string cd    = Directory.GetCurrentDirectory();
		string dir   = $"{cd}/Records/";

		Directory.CreateDirectory(dir);
		for (int i = 0; i < 10; i++) {
			File.CreateText($"{dir}{i}");
		}
		File.Move($"{dir}0", $"{dir}0)");
	}

	public void Record() {
		string cd  = Directory.GetCurrentDirectory();
		string dir = $"{cd}/Records/";

		if (!Directory.Exists(dir))
			InitRecords();

		string[] files = Directory.GetFiles(dir);
		for (int i = 0; i < files.Length; i++) {
			string path = files[i];
			string name = Path.GetFileName(path);
			if (name.Contains(')')) {
				int index = int.Parse(name.Remove(1));
				File.Move(path, $"{dir}{index}");
				
				index = Tools.RollOver(index, 1, files.Length);
				path = $"{dir}{index}";
		
				string contents = $"{TimeStamp}\n_\n";
				contents += SharedText;
				File.WriteAllText(path, contents);
				// File.WriteAllTextAsync()

				File.Move(path, $"{path})");
			}
		}
	}

	public string TimeStamp { get {
		string date = DateTime.Now.ToShortDateString();
		string time = DateTime.Now.ToShortTimeString();
		return $"{date}\n{time}";
	}}

	public void Restore() {
		string cd  = Directory.GetCurrentDirectory();
		string dir = $"{cd}/Records/";

		if (!Directory.Exists(dir))
			InitRecords();

		string[] files = Directory.GetFiles(dir);
		for (int i = 0; i < files.Length; i++) {
			string path = files[i];
			string name = Path.GetFileName(path);
			if (name.Contains(')')) {
				string contents = File.ReadAllText(path);
				Console.WriteLine(contents);

				// int index = int.Parse(name.Remove(1));
				// File.Move(path, $"{dir}{index}");
				
				// index = Tools.RollOver(index, 1, files.Length);
				// path = $"{dir}{index}";
		
				// string contents = $"{TimeStamp}\n_\n";
				// contents += SharedText;
				// File.WriteAllText(path, contents);
				// // File.WriteAllTextAsync()

				// File.Move(path, $"{path})");
			}
		}
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



/*

I need to be able to save thing's in a file format that is human readable
that way I can recover and refactor data as needed

Label
Prompt
Color
Pos
Area

Label
Prompt
Color
Pos
Area

*/