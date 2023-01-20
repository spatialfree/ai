namespace ai;
// using Blazored.LocalStorage;
// using ChangeEventArgs = Microsoft.AspNetCore.Components.ChangeEventArgs;

public class IndexBase : ComponentBase {
	protected string Prompter = "";
	protected List<Scroll> Scrolls = new() {
		new Scroll { Pos = new Vec(64, 128), Area = new Vec(200, 40), Label = "0", Text = "", },
		new Scroll { Pos = new Vec(64, 256), Area = new Vec(200, 40), Label = "1", Text = "Say this is a test", },
		new Scroll { Pos = new Vec(64, 384), Area = new Vec(200, 40), Label = "2", Text = "my way", },
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
				if (Outputs[OutputIndex].Length > 0) {
					if (OutputIndex < Outputs.Count - 1)
						Outputs.RemoveRange(OutputIndex + 1, Outputs.Count - (OutputIndex + 1));
					
					Outputs.Add("");
					OutputIndex++;
				}

				CompletionRequest request = new CompletionRequest();
				request.Model = OpenAI.Models.Model.Davinci;
				request.Prompt = Scrolls[0].Full + Scrolls[1].Full + Tools.Formatted(OutputLabel,"\n");
				request.MaxTokens = MaxTokens;
				request.Temperature = Temperature;
				request.PresencePenalty = Contrast;
				request.FrequencyPenalty = -Repeat;

				var endpoint = API.CompletionsEndpoint;
				await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
					Outputs[OutputIndex] += token.Completions[0].Text;
					Outputs[OutputIndex] = Outputs[OutputIndex].TrimStart('\n');
					
					StateHasChanged();
				}

				Console.WriteLine($"{DateTime.Now.ToShortTimeString()} ++ {Prompter}");
			}
			catch (Exception ex) {
				Error = true;
				StateHasChanged();
				await Task.Delay(2000);
				Error = false;

				Console.WriteLine($"{DateTime.Now.ToShortTimeString()} -- {Prompter} : {ex.Message}");
			}

			Loading = false;
		}
	}

	protected async Task Embed() {
		var endpoint = API.EmbeddingsEndpoint;
		// (+), 0, (-)
		EmbeddingsResponse white = await endpoint.CreateEmbeddingAsync("north");
		EmbeddingsResponse grey = await endpoint.CreateEmbeddingAsync("south");
		EmbeddingsResponse black = await endpoint.CreateEmbeddingAsync("west");
		double[] vWhite = GetVector(white);
		double[] vGrey  = GetVector(grey);
		double[] vBlack = GetVector(black);


		// double w2g = Similarity(vWhite, vGrey);
		// double g2b = Similarity(vGrey, vBlack);
		// Console.WriteLine($"{w2g} : {g2b}");

		double[] w2g = Tools.Direction(vWhite, vGrey);
		double[] g2b = Tools.Direction(vGrey, vBlack);
		Console.WriteLine($"{Tools.DotProduct(w2g, g2b)}");
	}

	double[] GetVector(EmbeddingsResponse er) { return er.Data[0].Embedding.ToArray(); }



	protected string ApiKey = "";
	OpenAIClient API = default!;
	protected bool Authorized = false;

	// [Inject] ILocalStorageService localStorage { get; set; } = default!;
	[Inject] IJSRuntime ijsruntime { get; set; } = default!;
	[Inject] Mono mono { get; set; } = default!;
	// private IExampleService ExampleService { get; set; } = default!;

	IEnumerable<string> Keys { get; set; } = new List<string>();

	protected override async Task OnAfterRenderAsync(bool firstRender) {
		// Keys = await localStorage.KeysAsync();
		if (firstRender) {
			
			// await LoadKey();
			// StateHasChanged();
		}

		// await SaveKey();
	}

	protected async Task LoadKey() {
		// ApiKey = await localStorage.GetItemAsync<string>("apikey");
		if (string.IsNullOrEmpty(ApiKey)) { ApiKey = ""; }
	}

	string lastTry = "";
	protected async Task SaveKey() {
		if (ApiKey == lastTry)
			return;
		lastTry = ApiKey;

		// string storageKey = await localStorage.GetItemAsync<string>("apikey");
		// if (ApiKey == storageKey && Authorized)
		// 	return;

		try {
			API = new OpenAIClient(new OpenAIAuthentication(ApiKey));
			var endpoint = API.EmbeddingsEndpoint;
			await endpoint.CreateEmbeddingAsync("key");

			Authorized = true;
		}
		catch {
			API = default!;
			Authorized = false;
		}

		// await localStorage.SetItemAsync("apikey", ApiKey);
		// Keys = await localStorage.KeysAsync();
		StateHasChanged();
	}

	protected void SetData() {
		int value = 1;
		mono.Add(ApiKey, value);
		Console.WriteLine($"SET: {value}");
	}

	protected void GetData() {
		int value = mono.Get(ApiKey);
		Console.WriteLine($"GET: {value}");
	}



	
	protected void MouseMove(MouseEventArgs e) {
		cursor = new Vec(e.ClientX, e.ClientY); 
		Move();
	}

	protected void MouseDown(MouseEventArgs e) {
		cursor = new Vec(e.ClientX, e.ClientY);
		Grab();
	}

	protected void MouseUp(MouseEventArgs e) {
		held = false;
		StateHasChanged();
	}


	protected void TouchMove(TouchEventArgs e) {
		cursor = new Vec(e.Touches[0].ClientX, e.Touches[0].ClientY);
		Move();
	}

	protected void TouchStart(TouchEventArgs e) {
		cursor = new Vec(e.Touches[0].ClientX, e.Touches[0].ClientY);
		Grab();
	}

	protected void TouchEnd(TouchEventArgs e) {
		held = false;
		StateHasChanged();
	}


	protected void Grab() {
		for (int i = 0; i < Scrolls.Count; i++) {
			Vec pos = Scrolls[i].Pos;
			Vec localPos = (cursor - pos);
			bool inX = localPos.x <  0 && localPos.x > -40;
			bool inY = localPos.y < 20 && localPos.y >   0;
			if (inX && inY) {
				offset = pos - cursor;

				Console.WriteLine(i);
				Scroll item = Scrolls[i];
				Scrolls.RemoveAt(i);
				Scrolls.Add(item);

				held = true;

				StateHasChanged();
				return;
			}
		}
	}

	void Move() {
		if (held) { 
			Vec newPos = cursor + offset;
			Scrolls[Scrolls.Count - 1].Pos = newPos;
		}
		StateHasChanged();
	}

	Vec offset = new Vec(0, 0);
	protected bool held = false;
	protected Vec cursor = new Vec(200, 300);

/*

NOTES
	do everything with pixels and slap a scalar on top
	also turn this off?
		<meta name="viewport" content="width=device-width, initial-scale=1.0" />

	HASH FUNCTION *dont store peoples keys directly*

	Rewrite the token system to be persistent and time mapped
	totalTokens += result.Usage.TotalTokens;
	Console.WriteLine($"+{result.Usage.TotalTokens} | {totalTokens}");


GRAVEYARD
	<PageTitle>Index</PageTitle>
	<h1>Index</h1>
	<h3>Todo (@todos.Count(todo => !todo.IsDone))</h3>

	<p>@completion</p>
	color: @Outputs[OutputIndex].Color
	@bind:event="oninput" style="height: @(Rows(scroll.Text, 89))pc"
	// localStorage.Changed += (_, e) => {
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
	public Vec Pos { get; set; } = default!;
	public Vec Area { get; set; } = default!;
	// public bool IsDone { get; set; }
}


public class Vec {
	public double x { get; set; }
	public double y { get; set; }

	public Vec(double x, double y) {
		this.x = x;
		this.y = y;
	}

	public static Vec operator +(Vec a, Vec b) 
		=> new Vec(a.x + b.x, a.y + b.y);

	public static Vec operator -(Vec a, Vec b) 
		=> new Vec(a.x - b.x, a.y - b.y);


  public double Mag
		=> Math.Sqrt(x * x + y * y);

	


  public override string ToString()
    => string.Format("[{0:0.##}, {1:0.##}]", x, y);
}