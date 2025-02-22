using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using C2Stats.Entities;
using C2Stats.Models;
using C2Stats.Notifications;
using MediatR;
using Microsoft.Extensions.Options;

namespace C2Stats.Services
{
	public interface IProfileFileStorage
	{
		ICollection<DbProfile> GetProfiles();
		
		bool TryGetProfile(int profileId, out DbProfile? profile);
		
		bool UpdatedProfile(DbProfile profile);
		
		void Persist();
	}

	public class ProfileFileStorage(
		ILogger<ProfileFileStorage> logger, IOptions<AppOptions> appOptions, IPublisher mediator) : IProfileFileStorage
	{
		private readonly Lock _lock = new();
		
		private IDictionary<int, DbProfile>? _profileMap;
		
		private readonly List<int> _added = [];
		private readonly List<int> _updated = [];
		
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
		};

		private IDictionary<int, DbProfile> ProfileMap
		{
			get
			{
				if (_profileMap == null)
				{
					lock (_lock)
					{
						_profileMap ??= Preload();
					}
				}
				
				return _profileMap;
			}
		}

		private ConcurrentDictionary<int, DbProfile> Preload()
		{
			var result = new ConcurrentDictionary<int, DbProfile>();

			var path = GetFilePath();

			if (File.Exists(path))
			{
				var currentJson = File.ReadAllText(path);

				var profiles = JsonSerializer.Deserialize<DbProfile[]>(currentJson, JsonOptions);

				if (profiles != null)
				{
					foreach (var profile in profiles)
					{
						result[profile.Id] = profile;
					}
				}
			}

			return result;
		}

		public ICollection<DbProfile> GetProfiles()
		{
			return ProfileMap.Values;
		}

		public bool TryGetProfile(int profileId, out DbProfile? profile)
		{
			return ProfileMap.TryGetValue(profileId, out profile);
		}

		public bool UpdatedProfile(DbProfile profile)
		{
			lock (_lock)
			{
				var map = ProfileMap;
				
				if (map.TryGetValue(profile.Id, out var existing))
				{
					if (profile.Name == existing.Name &&
					    profile.Country == existing.Country &&
					    profile.Sex == existing.Sex) return false;
					
					_updated.Add(profile.Id);
				}
				else
				{
					map[profile.Id] = profile;
					
					_added.Add(profile.Id);
				}
				
				return true;
			}
		}
		
		public void Persist()
		{
			lock (_lock)
			{
				if (_added.Count > 0 || _updated.Count > 0)
				{
					var path = GetFilePath();
			
					Directory.CreateDirectory(Path.GetDirectoryName(path)!);

					var map = ProfileMap;
					
					var profiles = map.Values.OrderBy(x => x.Id).ToArray();
			
					var json = JsonSerializer.Serialize(profiles, JsonOptions);

					File.WriteAllText(path, json);
			
					if (logger.IsEnabled(LogLevel.Information))
					{
						logger.LogInformation(
							"File {Path} saved, (total {TotalCount}, added {AddedCount}, updated {UpdatedCount})",
							path, profiles.Length, _added.Count, _updated.Count);
					}
					
					mediator.Publish(new ProfilesUpdated
					{
						Updated = _added.Union(_updated).Select(id => map[id]).ToList()
					});
					
					_added.Clear();
					_updated.Clear();
				}
			}
		}
		
		private string GetFilePath()
		{
			var options = appOptions.Value;
			
			return Path.Combine(options.ParseDirPath, "profiles.json");
		}
	}
}