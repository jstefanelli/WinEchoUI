using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace WinEchoUI
{
	public struct ConfigEntry {
		public string CaptureName { get; set; }
		public string RenderName { get; set; }
	}

	public class Config
	{
		public List<ConfigEntry> Entries { get; set; } = new List<ConfigEntry>();

		public async Task Save(string path){
			string data = JsonSerializer.Serialize(this, new JsonSerializerOptions()
			{
				WriteIndented = true
			});
			await File.WriteAllTextAsync(path, data);
		}

		public static async Task<Config> Load(string path)
		{
			if (!File.Exists(path))
				return null;

			string data = await File.ReadAllTextAsync(path);
			try
			{
				return JsonSerializer.Deserialize<Config>(data);
			}
			catch (JsonException)
			{
				return null;
			}
		}
	}
}
