namespace ai;
using System.IO;

public class Mono {
  public Mono() {
    Restore();
		Nodes.CollectionChanged += (sender, e) => {
			Recorded = Restored = false;
		};
  }


	

	public string Pattern = "Pattern";

	// ObvserableCollection?
	public List<Node> Shelf = new() {
		new Node { Pos = new Vec(64, 100), Area = new Vec(100, 20), Color = "#57b373", Text = "0 | Zed\nLeaf Green", }
	};


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
					contents += $"\nText {Nodes[j].Text.Replace("\n", "\\n")}";
					contents += $"\nColor {Nodes[j].Color}";
					contents += $"\nPos {Nodes[j].Pos}";
					contents += $"\nArea {Nodes[j].Area}";
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
							Text  = lines[j].Replace("Text ",    "").Replace("\\n", "\n"),
							Color = lines[j+1].Replace("Color ", "").Trim(),
							Pos   = lines[j+2].Replace("Pos ",   "").ToVec(),
							Area  = lines[j+3].Replace("Area ",  "").ToVec()
						};
						j += 3;
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
	Libraries~ (too soon?)
	Boards~

	NOW
		One shared "board"
			secure (10%)
				input field
				hash function
		
	


	do everything with pixels and slap a scalar on top
	also turn this off?
		<meta name="viewport" content="width=device-width, initial-scale=1.0" />

	HASH FUNCTION *dont store peoples keys directly*

	Rewrite the token system to be persistent and time mapped
	totalTokens += result.Usage.TotalTokens;
	Console.WriteLine($"+{result.Usage.TotalTokens} | {totalTokens}");

*/