using System.IO;

namespace ai;
public static class Records {
  
	


	// files files files
	// bye bye shared instances?
	// than how do handle multiple users saving to the same file?


  // make this static
  // and pass in the reference to the pattern


  static string cd  => Directory.GetCurrentDirectory();
  static string dir => $"{cd}/Records/";

	public static void InitRecords() {
		Directory.CreateDirectory(dir);
		for (int i = 0; i < 10; i++) {
			File.CreateText($"{dir}{i}");
		}
		File.Move($"{dir}0", $"{dir}0)");
	}

	public static bool Record(this Pattern pattern) {
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
				for (int j = 0; j < pattern.scrolls.Count; j++) {
					Pattern.Scroll scroll = pattern.scrolls[j];
					contents += $"\ntemp  {scroll.temp}";
					contents += $"\nname  {scroll.name.Trim()}";
					contents += $"\ntext  {(scroll.temp ? "" : scroll.text.Replace("\n", "\\n"))}";
					contents += $"\npos   {scroll.pos.Stepped()}";
					contents += $"\narea  {scroll.area.Stepped()}";
					contents += $"\n";
				}

				File.WriteAllText(path, contents);
				// File.WriteAllTextAsync()
				File.Move(path, $"{path})");

				return true;
			}
		}
    return false;
	}

	
	public static bool Restore(this Pattern pattern) {
		if (!Directory.Exists(dir))
			InitRecords();

		string[] files = Directory.GetFiles(dir);
		for (int i = 0; i < files.Length; i++) {
			string path = files[i];
			string name = Path.GetFileName(path);
			if (name.Contains(')')) {
				bool meta = true;
				pattern.scrolls.Clear();
				string[] lines = File.ReadAllLines(path);
				for (int j = 0; j < lines.Length; j++) {
					if (meta) {
						meta = !lines[j].Contains("___");
					} else {
						if (lines[j].Trim() == "") {
							continue;
						}

						pattern.scrolls.Add(new Pattern.Scroll(pattern) {
							temp  = lines[j++].Replace("temp  ", "").Trim() == "True",
							name  = lines[j++].Replace("name  ", "").Trim(),
							text  = lines[j++].Replace("text  ", "").Replace("\\n", "\n"),
							pos   = lines[j++].Replace("pos   ", "").ToVec(),
							area  = lines[j++].Replace("area  ", "").ToVec()
						});
					}
				}
				return true;
			}
		}
    return false;
	}
  
	public static string TimeStamp
    => $"{DateTime.Now.ToShortDateString()}\n{DateTime.Now.ToShortTimeString()}";
}