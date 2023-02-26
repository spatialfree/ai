using System.IO;
using System.Reflection;

namespace ai;
public static class Records {  
  static string cd  => Directory.GetCurrentDirectory();
  static string dir => $"{cd}/Records/";

	public static bool Record(this Pattern pattern) {
		Console.WriteLine($"Recording {pattern.name}");
		List<string> lines = new List<string>();
		lines.Add(TimeStamp);
		lines.Add("___");
		for (int i = 0; i < pattern.scrolls.Count; i++) {
			Pattern.Scroll scroll = pattern.sorted[i];
			lines.Add($"temp  {scroll.temp}");
			lines.Add($"name  {scroll.name.Trim()}");
			lines.Add($"text  {(scroll.temp ? "" : scroll.text.Replace("\n", "\\n"))}");
			lines.Add($"pos   {scroll.pos.Stepped()}");
			lines.Add($"area  {scroll.area.Stepped()}");
			lines.Add($"");
		}
		lines.Add("___");
		string[] style = pattern.style.Split("\n");
		// lines.Add(pattern.style.Replace("\n", "\\n"));
		for (int i = 0; i < style.Length; i++) {
			lines.Add(style[i]);
		}
		lines.Add($""); // mark end

		// File.WriteAllLinesAsync() ???
		File.WriteAllLines($"{dir}{pattern.name}.txt", lines); // (!)catch exceptions

		return true; 
	}

	public static bool Restore(this Pattern pattern) {
		Console.WriteLine($"Restoring {pattern.name}");
		string[] lines = new string[0];
		try {
			lines = File.ReadAllLines($"{dir}{pattern.name}.txt");
		} catch (Exception e) {
			Console.WriteLine($"Restore failed: {e.Message}");
			return false;
		}

		int section = 0;
		pattern.style = "";
		pattern.scrolls.Clear();
		
		Pattern.Scroll scroll = new Pattern.Scroll(pattern);
		System.Reflection.PropertyInfo[] properties = typeof(Pattern.Scroll).GetProperties();
		int count = 0;
		for (int j = 0; j < lines.Length; j++) {
			if (lines[j].Contains("___")) {
				section++;
				continue;
			}

			if (section == 0)   // meta
				continue;
			
			if (section == 2) { // style
				pattern.style += lines[j] + "\n";
				continue;
			}
				


			

			if (lines[j].Trim() == "") {
				pattern.scrolls.Add(scroll);
				count++;
				scroll = new Pattern.Scroll(pattern);
				continue;
			}

			// try to find matches for each scroll parameter using reflection
			// that way we can add new parameters without having to update this code
			// and we can add new parameters without having to update the file format
			foreach (System.Reflection.PropertyInfo property in properties) {
				if (lines[j].StartsWith(property.Name)) {
					string value = lines[j].Replace(property.Name, "").Trim();
					if (property.PropertyType == typeof(bool)) {
						property.SetValue(scroll, bool.Parse(value));
					} else if (property.PropertyType == typeof(string)) {
						property.SetValue(scroll, value.Replace("\\n", "\n"));
					} else if (property.PropertyType == typeof(Vec)) {
						property.SetValue(scroll, value.ToVec());
					}
				}
			}
		}
		// Console.WriteLine($"lines {lines.Length} | count {pattern.scrolls.Count}/{count}");
		
		return true;
	}
  
	public static string TimeStamp
    => $"{DateTime.Now.ToShortDateString()}\n{DateTime.Now.ToShortTimeString()}";
}