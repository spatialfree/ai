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
			await endpoint.CreateEmbeddingAsync("TryKey");
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

	ObservableCollection<Node> nodes { get; set; } = new() {
		new Node { shelf = true, pos = new Vec(60, 100), area = new Vec(60, 20), text = "Text" },
		new Node { shelf = false, pos = new Vec(60, 100), area = new Vec(60, 20), text = "Text" },
		new Node { shelf = false, pos = new Vec(200, 100), area = new Vec(60, 20), text = "Text" },
	};
	public ObservableCollection<Node> Nodes {
		get { return Cloud ? mono.Nodes : nodes; }
	}



	protected bool Shelf = false;
	protected bool Menu = true;


	protected void PointerMove(PointerEventArgs e) {
		// Console.WriteLine($"PointerMove {e.PointerId}");
		Cursor = new Vec(e.ClientX, e.ClientY); // OffsetY

		Node node = Nodes[Nodes.Count - 1];

		if (held) { 
			Vec newPos = AutoCursor(node.shelf) + offset;
			node.pos = newPos.Stepped();
		} else if (pull) {
			Vec newArea = (AutoCursor(node.shelf) + offset) - node.pos;

			cull = newArea.x < 0 && newArea.y < 0;
			
			newArea.x = Math.Max(newArea.x.Stepped(), 60);
			newArea.y = Math.Max(newArea.y.Stepped(), 20);
			node.area = newArea;
		} else if (down) {
			Canvas = Cursor.Stepped() + canvasOffset;
		}
		StateHasChanged();
	}

	protected void PointerDown(PointerEventArgs e) {
		// Console.WriteLine($"PointerDown : {e.Button}");
		Cursor = new Vec(e.ClientX, e.ClientY);
		down = true;

		for (int i = Nodes.Count-1; i >= 0; i--) {
			Node node = Nodes[i];



			Vec localPos = AutoCursor(node.shelf) - node.pos;
			bool inXMin = localPos.x >= 0;
			bool inXMax = localPos.x < node.area.x + 20;
			// print 0 for inside both and - for outside min and + for outside max
			int x = (inXMin ? 0 : -1) + (inXMax ? 0 : 1);
			string xstr = x == 0 ? "0" : x < 0 ? "-" : "+";

			bool inYMin = localPos.y >= 0;
			bool inYMax = localPos.y < node.area.y + 40;
			int y = (inYMin ? 0 : -1) + (inYMax ? 0 : 1);
			string ystr = y == 0 ? "0" : y < 0 ? "-" : "+";

			// Console.WriteLine($"{xstr}{ystr} {(int)e.ClientX - node.pos.x}");
			if (inXMin && inXMax && inYMin && inYMax) {
				offset = node.pos - AutoCursor(node.shelf);
				oldPos = node.pos;

				// Lift
				if (Nodes.Count > 1) {
					Nodes.RemoveAt(i);
					Nodes.Add(node);
				}

				if ((AutoCursor(node.shelf) - node.pos).Mag < 10.0) {
					pull = true;
				} else {
					held = true;
				}


				StateHasChanged();
				return;
			}
		}

		canvasOffset = Canvas - Cursor.Stepped();
	}

	protected void PointerUp(PointerEventArgs e) {
		// Console.WriteLine($"PointerUp : {e.Button}");
		Cursor = new Vec(e.ClientX, e.ClientY);
		if (held && Shelf) { 
			Node node = Nodes[Nodes.Count - 1];
			if (node.shelf) {
				if (Cursor.y > 400) {
					Node newNode = new Node(node);
					newNode.shelf = false;
					newNode.pos = node.pos - Canvas;
					inst = held = true;
					Nodes.Add(newNode); // OOP

					// put shelf node back
					node.pos = oldPos;
					// Console.WriteLine("Instantiate Node");
				}
			} else {
				if (Cursor.y < 400) {
					node.shelf = true;
					node.pos += Canvas;
					// Console.WriteLine("Declare Node");
				}
			}
		}

		if (cull) {
			if (Nodes.Count(todo => !todo.shelf) <= 2) {
				Nodes[Nodes.Count - 1].name = "";
				Nodes[Nodes.Count - 1].text = "";
			} else {
				Nodes.RemoveAt(Nodes.Count - 1);
			}
			// Console.WriteLine("CULL");
		}
		down = held = pull = cull = inst = false;
		StateHasChanged();
	}

	Vec Cursor = new Vec(200, 300);
	protected Vec LocalCursor { get { return Cursor - Canvas; } }
	protected Vec AutoCursor(bool shelf) {
		return shelf ? Cursor : LocalCursor;
	}

	Vec offset = new Vec(0, 0);
	Vec canvasOffset = new Vec(0, 0);
	protected Vec Canvas = new Vec(0, 0);

	Vec oldPos = new Vec(0, 0); // vague variable name 
	
	protected bool down = false;
	protected bool held = false;
	protected bool pull = false;
	protected bool cull = false;
	protected bool inst = false;



	protected async Task Run() {
		Node node = Nodes[Nodes.Count - 2];

		string prep = "";

		bool read = false;
		string reference = "";
		for (int i = 0; i < node.text.Length; i++) {
			if (node.text[i] == '}') { read = false; 
				reference = reference.Trim();
				Node refNode = GetNodeByName(reference);
				prep += refNode.text;

				reference = "";
				continue;
			}
			if (read) { reference += node.text[i]; }
			if (node.text[i] == '{') { read = true; }

			if (!read) { prep += node.text[i]; }
		}

		// Console.WriteLine(prep);
		// Nodes[Nodes.Count - 1].text = completion;
		await Complete(prep);
		// Console.WriteLine(Nodes[Nodes.Count - 1].text);
	}

	Node GetNodeByName(string name) {
		foreach (Node node in Nodes) {
			if (node.name == name) {
				return node;
			}
		}
		Console.WriteLine($"Node {name} not found");
		return new Node();
	}

	protected bool Loading = false;
	protected bool Error = false;
	// string completion = "";
	protected async Task Complete(string prompt) {
		if (!Loading) {
			Loading = true;

			try {
				CompletionRequest request = new CompletionRequest();
				request.Model = OpenAI.Models.Model.Davinci;
				// request.Prompt = Nodes[0].Full + Nodes[1].Full + Tools.Formatted(OutputLabel,"\n");
				request.Prompt = prompt;
				request.MaxTokens = MaxTokens;
				request.Temperature = Temperature;
				request.PresencePenalty = Contrast;
				request.FrequencyPenalty = -Repeat;

				var endpoint = API.CompletionsEndpoint;
				await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
					Nodes[Nodes.Count - 1].text += token.Completions[0].Text;
					Nodes[Nodes.Count - 1].text = Nodes[Nodes.Count - 1].text.TrimStart('\n');
					
					StateHasChanged();
				}

				// Console.WriteLine($"{DateTime.Now.ToShortTimeString()} ++ {Prompter}");
			}
			catch (Exception ex) {
				Error = true;
				StateHasChanged();
				await Task.Delay(2000);
				Error = false;

				// Console.WriteLine($"{DateTime.Now.ToShortTimeString()} -- {Prompter} : {ex.Message}");
			}

			Loading = false;
		}
	}
	protected int   MaxTokens   = 256;
	protected float Temperature = 1.0f;
	protected float Contrast    = 0.0f;
	protected float Repeat      = 0.0f;
}

/*
	GRAVEYARD
	
	if (Outputs[OutputIndex].Length > 0) {
		if (OutputIndex < Outputs.Count - 1)
			Outputs.RemoveRange(OutputIndex + 1, Outputs.Count - (OutputIndex + 1));
		
		Outputs.Add("");
		OutputIndex++;
	}

	protected string OutputLabel = "";
	protected int OutputIndex = 0;
	protected List<string> Outputs = new() { "" };
	protected void Back() { OutputIndex--; }
	protected void Next() { OutputIndex++; }

	protected void Swap() {
		var txt = Nodes[1].text;
		Nodes[1].text = Outputs[OutputIndex];
		Outputs[OutputIndex] = txt;
	}

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
