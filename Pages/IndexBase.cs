using System.Net.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;

using Microsoft.JSInterop;

using ai;
using ai.Shared;

using Blazored.LocalStorage;
using Blazored.LocalStorage.Serialization;

namespace ai;

public class IndexBase : ComponentBase {
	protected string Prompter = "";
	protected List<Scroll> Scrolls = new() {
		new Scroll {
			Label = "",
			Text = "",
		},
		new Scroll {
			Label = "",
			Text = "",
		},
	};

	protected string ChoiceLabel = "";
	protected int ChoiceIndex = 0;
	protected List<string> Choices = new() { "", "", "", };
	protected void Back() { ChoiceIndex = ChoiceIndex == 0 ? Choices.Count - 1 : ChoiceIndex - 1; }
	protected void Next() { ChoiceIndex = ChoiceIndex == Choices.Count - 1 ? 0 : ChoiceIndex + 1; }

	protected void Swap() {
		var txt = Scrolls[1].Text;
		Scrolls[1].Text = Choices[ChoiceIndex];
		Choices[ChoiceIndex] = txt;
	}

	protected int   MaxTokens   = 256;
	protected float Temperature = 1.0f;
	protected float Contrast    = 0.0f;
	protected float Repeat      = 0.0f;

	protected bool Loading = false;
	protected bool Error = false;
	protected async Task Complete() {
		if (!Loading) {
			Loading = true;

			try {
				Choices = Choices.Select(x => { x = ""; return x; }).ToList();

				CompletionRequest request = new CompletionRequest();
				request.Model = OpenAI.Models.Model.Davinci;
				request.Prompt = Scrolls[0].Full + Scrolls[1].Full + Tools.Formatted(ChoiceLabel,"\n");
				request.MaxTokens = MaxTokens;
				request.Temperature = Temperature;
				request.PresencePenalty = Contrast;
				request.FrequencyPenalty = -Repeat;
				request.NumChoicesPerPrompt = 3;

				var endpoint = API.CompletionsEndpoint;
				await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
					var index = token.Completions[0].Index;
					Choices[index] += token.Completions[0].Text;
					Choices[index] = Choices[index].TrimStart('\n');
					
					StateHasChanged();
				}

				Console.WriteLine($"{DateTime.Now.ToShortTimeString()} ++ {Prompter}");
			}
			catch {
				// openai btn flash red
				Error = true;
				await Task.Delay(2000);
				Error = false;

				Console.WriteLine($"{DateTime.Now.ToShortTimeString()} -- {Prompter}");
			}

			Loading = false;
		}
	}

	protected string ApiKey = "";
	OpenAIClient? API;
	protected bool Authorized = false;

	[Inject]
	ILocalStorageService localStorage { get; set; } = default!;
	// private IExampleService ExampleService { get; set; } = default!;

	IEnumerable<string> Keys { get; set; } = new List<string>();
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		Keys = await localStorage.KeysAsync();
		if (firstRender) {
			await LoadKey();
			StateHasChanged();
		}

		await SaveKey();
	}

	protected async Task LoadKey() {
		ApiKey = await localStorage.GetItemAsync<string>("apikey");
		if (string.IsNullOrEmpty(ApiKey)) { ApiKey = ""; }
	}

	string lastTry = "";
	protected async Task SaveKey() {
		if (ApiKey == lastTry)
			return;
		lastTry = ApiKey;

		string storageKey = await localStorage.GetItemAsync<string>("apikey");
		if (ApiKey == storageKey && Authorized)
			return;

		try {
			API = new OpenAIClient(new OpenAIAuthentication(ApiKey));
			var model = await API.ModelsEndpoint.GetModelDetailsAsync("text-davinci-003");
			Authorized = true;
		}
		catch {
			API = null; // not memory safe
			Authorized = false;
		}

		await localStorage.SetItemAsync("apikey", ApiKey);
		Keys = await localStorage.KeysAsync();
		StateHasChanged();
	}

/*

NOTES
	max_tokens  slider
	temperature slider
	Rewrite the token system to be persistent and time mapped
	totalTokens += result.Usage.TotalTokens;
	Console.WriteLine($"+{result.Usage.TotalTokens} | {totalTokens}");
	extensible localStorage based on key system


GRAVEYARD
	<PageTitle>Index</PageTitle>
	<h1>Index</h1>
	<h3>Todo (@todos.Count(todo => !todo.IsDone))</h3>

	<p>@completion</p>
	color: @Choices[ChoiceIndex].Color
	@bind:event="oninput" style="height: @(Rows(scroll.Text, 89))pc"
	localStorage.Changed += (_, e) => {
		Console.WriteLine($"  key: {e.Key} \n from: {e.OldValue} \n   to: {e.NewValue}");
	};
	async Task oninput(ChangeEventArgs e) {
		StateHasChanged();
	}

*/
}