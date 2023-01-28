namespace ai;

public class IndexBase : ComponentBase {
	[Inject] protected Mono mono { get; set; } = default!;

	OpenAIClient API = default!;
	protected bool ValidKey = false;
	string apikey = "";
	protected string Prompter = "";
	protected string ApiKey { 
		get { return apikey; }
		set { apikey = value; TryKey(); }
	}

	string lastTry = "";
	protected async Task TryKey() {
		if (ApiKey == lastTry)
			return;
		lastTry = ApiKey;

		try {
			API = new OpenAIClient(new OpenAIAuthentication(ApiKey));
			var endpoint = API.EmbeddingsEndpoint;
			await endpoint.CreateEmbeddingAsync("key");
			ValidKey = true;
		}
		catch {
			API = default!;
			ValidKey = false;
		}

		StateHasChanged();
	}


	string pattern = "";
	protected string Pattern {
		get { return pattern; }
		set { 
			pattern = value;
			Cloud = pattern == mono.Pattern;
		}
	}
	protected bool Cloud = false;

	ObservableCollection<Node> nodes = new() {
		new Node { Pos = new Vec(64, 100), Area = new Vec(100, 20), Color = "#57b373", Text = "0 | Zed\nLeaf Green", }
	};
	protected ObservableCollection<Node> Nodes {
		get { return Cloud ? mono.Nodes : nodes; }
	}




	protected bool Menu = true;
	protected void MenuToggle() {
		Menu = !Menu;
	}


	protected string OutputLabel = "";
	protected int OutputIndex = 0;
	protected List<string> Outputs = new() { "" };
	protected void Back() { OutputIndex--; }
	protected void Next() { OutputIndex++; }

	protected void Swap() {
		var txt = Nodes[1].Text;
		Nodes[1].Text = Outputs[OutputIndex];
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
				// request.Prompt = Nodes[0].Full + Nodes[1].Full + Tools.Formatted(OutputLabel,"\n");
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

	
	protected void MouseMove(MouseEventArgs e) {
		SetCursor(e.ClientX, e.ClientY);
		Move();
	}

	protected void MouseDown(MouseEventArgs e) {
		SetCursor(e.ClientX, e.ClientY);
		down = true;
		Grab();
	}

	protected void MouseUp(MouseEventArgs e) {
		down = held = pull = false;
		StateHasChanged();
	}


	protected void TouchMove(TouchEventArgs e) {
		SetCursor(e.Touches[0].ClientX, e.Touches[0].ClientY);
		Move();
	}

	protected void TouchStart(TouchEventArgs e) {
		SetCursor(e.Touches[0].ClientX, e.Touches[0].ClientY);
		down = true;
		Grab();
	}

	protected void TouchEnd(TouchEventArgs e) {
		down = held = pull = false;
		StateHasChanged();
	}

	void SetCursor(double x, double y) {
		Cursor = new Vec(x, y); // +/- canvas
	}


	protected void Grab() {
		for (int i = 0; i < Nodes.Count; i++) {
			Vec pos = Nodes[i].Pos;

			Vec localPos = (LocalCursor - pos);
			bool inX = localPos.x <= -2 && localPos.x >= -41;
			bool inY = localPos.y <= 25 && localPos.y >= -5;
			// Console.WriteLine($"{i} : {localPos} : {inX} : {inY}");
			if (inX && inY) {
				offset = pos - LocalCursor;

				Lift(i);
				held = true;

				StateHasChanged();
				return;
			}

			Vec area = Nodes[i].Area;
			localPos = (LocalCursor - (pos + area));
			localPos.x -= 12; // ~padding + border
			localPos.y += 2; // ~border
			inX = localPos.x <= -3 && localPos.x >= -23; 
			inY = localPos.y <= 23 && localPos.y >=   4;
			// Console.WriteLine($"{i} : {localPos} : {inX} : {inY}");
			if (inX && inY) {
				offset = (pos + area) - LocalCursor;
				
				Lift(i);
				pull = true;

				StateHasChanged();
				return;
			}
		}

		canvasOffset = Canvas - Cursor;
	}

	void Move() {
		if (Nodes.Count == 0) return;
		Node node = Nodes[Nodes.Count - 1];
		if (held) { 
			Vec newPos = LocalCursor + offset;
			node.Pos = newPos;
		} else if (pull) {
			Vec newArea = (LocalCursor + offset) - node.Pos;
			newArea.x = Math.Max(newArea.x, 100);
			newArea.y = Math.Max(newArea.y, 20);
			node.Area = newArea;
		} else if (down) {
			Canvas = Cursor + canvasOffset;
		}
		StateHasChanged();
	}

	protected Vec Cursor = new Vec(200, 300);
	protected Vec LocalCursor { get { return Cursor - Canvas; } }

	Vec offset = new Vec(0, 0);
	Vec canvasOffset = new Vec(0, 0);
	protected Vec Canvas = new Vec(0, 0);
	
	protected bool down = false;
	protected bool held = false;
	protected bool pull = false;

	void Lift(int index) {
		Node node = Nodes[index];
		Nodes.RemoveAt(index);
		Nodes.Add(node);
	}
}

/*
	GRAVEYARD
		<h3>Todo (@todos.Count(todo => !todo.IsDone))</h3>

	[Inject] ILocalStorageService localStorage { get; set; } = default!;
	[Inject] IJSRuntime ijsruntime { get; set; } = default!;
	private IExampleService ExampleService { get; set; } = default!;

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

		// double[] w2g = Tools.Direction(vWhite, vGrey);
		// double[] g2b = Tools.Direction(vGrey, vBlack);
		// Console.WriteLine($"{Tools.DotProduct(w2g, g2b)}");
	}
	double[] GetVector(EmbeddingsResponse er) { return er.Data[0].Embedding.ToArray(); }


*/
