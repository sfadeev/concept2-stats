using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using C2Stats.Models;
using Microsoft.Extensions.Options;

namespace C2Stats.Services
{
	public interface IProfileCache
	{
		void Persist();
		
		bool TryGetProfile(int profileId, out Profile? profile);
		
		bool UpdatedProfile(Profile profile);
	}

	public class Profile
	{
		public int Id { get; set; }
		
		public string? Name { get; set; }
		
		public string? Country { get; set; }
		
		public string? Sex { get; set; }
	}

	public class FileProfileCache(ILogger<WodFileStorage> logger, IOptions<AppOptions> appOptions) : IProfileCache
	{
		private readonly object _lock = new();
		
		private IDictionary<int, Profile>? _profiles;
		
		private int _added;
		private int _updated;
		
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
		};

		private IDictionary<int, Profile> Profiles
		{
			get
			{
				Preload();
				
				return _profiles!;
			}
		}
		
		private void Preload()
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (_profiles == null)
			{
				lock (_lock)
				{
					if (_profiles == null)
					{
						_profiles = new ConcurrentDictionary<int, Profile>();
						
						var path = GetFilePath();
			
						if (File.Exists(path))
						{
							var currentJson = File.ReadAllText(path);
				
							var profiles = JsonSerializer.Deserialize<Profile[]>(currentJson, JsonOptions);

							if (profiles != null)
							{
								foreach (var profile in profiles)
								{
									_profiles[profile.Id] = profile;
								}
							}
						}
					}
				}
			}
		}

		public void Persist()
		{
			lock (_lock)
			{
				if (_added > 0 || _updated > 0)
				{
					var path = GetFilePath();
			
					Directory.CreateDirectory(Path.GetDirectoryName(path)!);

					var profiles = Profiles.Values.OrderBy(x => x.Id).ToArray();
			
					var json = JsonSerializer.Serialize(profiles, JsonOptions);

					File.WriteAllText(path, json);
			
					if (logger.IsEnabled(LogLevel.Information))
					{
						logger.LogInformation(
							"File {Path} saved, (total {TotalCount}, added {AddedCount}, updated {UpdatedCount})",
							path, profiles.Length, _added, _updated);
					}
				
					_added = 0;
					_updated = 0;
				}
			}
		}
		
		public bool TryGetProfile(int profileId, out Profile? profile)
		{
			Preload();
			
			return Profiles.TryGetValue(profileId, out profile);
		}

		public bool UpdatedProfile(Profile profile)
		{
			lock (_lock)
			{
				if (TryGetProfile(profile.Id, out var existing))
				{
					if (profile.Name == existing?.Name &&
					    profile.Country == existing?.Country &&
					    profile.Sex == existing?.Sex) return false;
					
					_updated++;
				}
				else
				{
					_added++;
				}
			
				Profiles[profile.Id] = profile;
			
				return true;
			}
		}
		
		private string GetFilePath()
		{
			var options = appOptions.Value;
			
			return Path.Combine(options.ParseDirPath, "profiles.json");
		}
	}
}