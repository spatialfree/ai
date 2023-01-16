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
			Text = "Say this is a test",
		},
	};

	protected string OutputLabel = "";
	protected int OutputIndex = 0;
	protected List<string> Outputs = new() { "" };
	protected void Back() { OutputIndex--; }
	protected void Next() { OutputIndex++; }

	protected void Swap() {
		var txt = Scrolls[1].Text;
		Scrolls[1].Text = Outputs[OutputIndex];
		Outputs[OutputIndex] = txt;
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
				// 0 [1] 2 3
				// 0 [1] 
				// 0 [1] 2
				// 0  1 [2]
				if (Outputs[OutputIndex].Length > 0) {
					if (OutputIndex < Outputs.Count - 1)
						Outputs.RemoveRange(OutputIndex + 1, Outputs.Count - (OutputIndex + 1));
					
					Outputs.Add("");
					OutputIndex++;
				}

					// OutputIndex == Outputs.Count - 1

				// Outputs = Outputs.Select(x => { x = ""; return x; }).ToList();

				CompletionRequest request = new CompletionRequest();
				request.Model = OpenAI.Models.Model.Davinci;
				request.Prompt = Scrolls[0].Full + Scrolls[1].Full + Tools.Formatted(OutputLabel,"\n");
				request.MaxTokens = MaxTokens;
				request.Temperature = Temperature;
				request.PresencePenalty = Contrast;
				request.FrequencyPenalty = -Repeat;

				var endpoint = API.CompletionsEndpoint;
				await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
					// var index = token.Completions[0].Index;
					// var output = ;
					Outputs[OutputIndex] += token.Completions[0].Text;
					Outputs[OutputIndex] = Outputs[OutputIndex].TrimStart('\n');
					
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
	OpenAIClient API = default!;
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
			API = default!;
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
	color: @Outputs[OutputIndex].Color
	@bind:event="oninput" style="height: @(Rows(scroll.Text, 89))pc"
	localStorage.Changed += (_, e) => {
		Console.WriteLine($"  key: {e.Key} \n from: {e.OldValue} \n   to: {e.NewValue}");
	};
	async Task oninput(ChangeEventArgs e) {
		StateHasChanged();
	}

*/
}

public class Scroll {
	public string Label { get; set; } = default!;
	public string Text  { get; set; } = default!;
	public string Full => $"{Tools.Formatted(Label,"\n")}{Tools.Formatted(Text,"\n\n")}";
	public string Color { get; set; } = default!;
	// public bool IsDone { get; set; }
}