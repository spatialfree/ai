namespace ai;
using System.IO;

public class Mono {
  public Mono() {
    Restore();
		Nodes.CollectionChanged += (sender, e) => {
			Recorded = Restored = false;
			// this only gets called when the collection is changed
			// not when the properties of the nodes are changed
		};
  }

	public bool Synced {
		get { return Recorded || Restored; }
	}
	

	public string Pattern = "Pattern";


	// ObvserableCollection?
	public ObservableCollection<Node> Nodes = new();

  string cd { get { return Directory.GetCurrentDirectory(); } }
  string dir { get { return $"{cd}/Records/"; } }

	public void InitRecords() {
		Directory.CreateDirectory(dir);
		for (int i = 0; i < 10; i++) {
			File.CreateText($"{dir}{i}");
		}
		File.Move($"{dir}0", $"{dir}0)");
	}

	public bool Recorded = false;
	public void Record() {
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
		
				string contents = $"{TimeStamp}\n___";
				for (int j = 0; j < Nodes.Count; j++) {
					contents += $"\nName  {Nodes[j].name.Trim()}";
					contents += $"\nText  {Nodes[j].text.Replace("\n", "\\n") }";
					contents += $"\nColor {Nodes[j].color}";
					contents += $"\nPos   {Nodes[j].pos.Stepped()}";
					contents += $"\nArea  {Nodes[j].area.Stepped()}";
					contents += $"\n";
				}

				File.WriteAllText(path, contents);
				// File.WriteAllTextAsync()
				File.Move(path, $"{path})");

				Recorded = true;
				break;
			}
		}
	}

	public bool Restored = false;
	public void Restore() {
		if (!Directory.Exists(dir))
			InitRecords();

		string[] files = Directory.GetFiles(dir);
		for (int i = 0; i < files.Length; i++) {
			string path = files[i];
			string name = Path.GetFileName(path);
			if (name.Contains(')')) {
				bool meta = true;
				Nodes.Clear();
				string[] lines = File.ReadAllLines(path);
				for (int j = 0; j < lines.Length; j++) {
					if (meta) {
						meta = !lines[j].Contains("___");
					} else {
						if (lines[j].Trim() == "") {
							continue;
						}

						Node node = new Node {
							name  = lines[j++].Replace("Name  ", "").Trim(),
							text  = lines[j++].Replace("Text  ", "").Replace("\\n", "\n"),
							color = lines[j++].Replace("Color ", "").Trim(),
							pos   = lines[j++].Replace("Pos   ", "").ToVec(),
							area  = lines[j++].Replace("Area  ", "").ToVec()
						};

						Nodes.Add(node);
					}
				}
				Restored = true;
			}
		}
	}
  
	public string TimeStamp { get {
		string date = DateTime.Now.ToShortDateString();
		string time = DateTime.Now.ToShortTimeString();
		return $"{date}\n{time}";
	}}
}


/*

NOTES
	TODO
		Housekeeping
		Token usage
		String of thought 
			(for tracking what the system is doing)
		Arrays?



	Libraries~ (too soon?)
	Boards~

	NOW
		One shared "board"
			secure (10%)
				input field
				hash function
		
	
	scale on edges and corners
		rather than just the bottom right corner

	"data areas"
		unique id/name
		text (1-4096 tokens)
		{ ref(.part) }

	"encapsulation"
		unique id/name
		contain "data areas"
		define input/output
		execution


	Directional
		to
			language model prompt patterning
				process
					branchial~
					sequenced
					programmatic
					modular
				memory
					on demand

		from
			conversational
				back and forth
					1 : 1
				memory
					limited

	Constraints
		Text
			That way the language model can interact with as much of it as possible
			Just text is bad tho?
		Mobile 1st
		

	{a}{b}
	{a} + {b}
	{a.top}
	{a.prompt}
	{a.out}
	{a.fresh}



	do everything with pixels and slap a scalar on top
	also turn this off?
		<meta name="viewport" content="width=device-width, initial-scale=1.0" />

	HASH FUNCTION *dont store peoples keys directly*

	Rewrite the token system to be persistent and time mapped
	totalTokens += result.Usage.TotalTokens;
	Console.WriteLine($"+{result.Usage.TotalTokens} | {totalTokens}");

*/