namespace ai;
using System.IO;

public class Mono {

  public Mono() {
    Restore();
  }

	public Vec Shared = new Vec(20, 20);
	public string SharedText = "text";

  string cd { get { return Directory.GetCurrentDirectory(); } }
  string dir { get { return $"{cd}/Records/"; } }


	public void InitRecords() {
		Directory.CreateDirectory(dir);
		for (int i = 0; i < 10; i++) {
			File.CreateText($"{dir}{i}");
		}
		File.Move($"{dir}0", $"{dir}0)");
	}

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
		
				string contents = $"{TimeStamp}\n___\n";
				contents += SharedText;
				File.WriteAllText(path, contents);
				// File.WriteAllTextAsync()

				File.Move(path, $"{path})");
			}
		}
	}

	public void Restore() {
		if (!Directory.Exists(dir))
			InitRecords();

		string[] files = Directory.GetFiles(dir);
		for (int i = 0; i < files.Length; i++) {
			string path = files[i];
			string name = Path.GetFileName(path);
			if (name.Contains(')')) {
				bool meta = true;
				string[] lines = File.ReadAllLines(path);
				for (int j = 0; j < lines.Length; j++) {
					if (meta) {
						meta = !lines[j].Contains("___");
					} else {
						Console.WriteLine(lines[j]);
						SharedText = lines[j];
					}
				}
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
	Library~ (too soon?)
	Board~

	NOW
		One shared "board"
			persistent (1hr)
			secure
		
	


	do everything with pixels and slap a scalar on top
	also turn this off?
		<meta name="viewport" content="width=device-width, initial-scale=1.0" />

	HASH FUNCTION *dont store peoples keys directly*

	Rewrite the token system to be persistent and time mapped
	totalTokens += result.Usage.TotalTokens;
	Console.WriteLine($"+{result.Usage.TotalTokens} | {totalTokens}");

	DATA DATA DATA
	much data so little time
		in memory
			what needs to be stored in the abstract?
			then build out the UI from that angle?
		auto back up array
			that can be used to restore debug and refactor












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