
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class DateTimeUtils
{
	public static DateTime UnixUTCSecondsToLocalDateTime(long unixUtcSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixUtcSeconds).UtcDateTime.ToLocalTime();
	public static long LocalDateTimeToUnixUTCSeconds(DateTime localTime) => new DateTimeOffset(localTime).ToUnixTimeSeconds();
	public static long GetNowInUnixUTCSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}


public class RoundRecord
{
	public string LevelId;
	public bool Cleared;
	public bool Draw;
	public float Duration;
	public int NumPiecesLeft;
	public long EndTime;
}

public class PlayerRecord
{
	public string Name;
	public long Created;
	public long Modified;
	public List<RoundRecord> Rounds = new List<RoundRecord>();
	public string CurrentLevel;

	public bool IsLevelCleared(string id) => Rounds.Any(r => r.LevelId == id && r.Cleared);
}

public class SaveData
{
	public List<PlayerRecord> Players = new List<PlayerRecord>();
}

public static class Saver
{
	static readonly string _saveDir = Application.persistentDataPath;
	static readonly string _savePath = Path.Combine(_saveDir, _saveFile);
	const string _saveFile = "save.json";

	public static SaveData LoadFromDisk()
  {
		if(File.Exists(_savePath))
    {
			try
			{
				var contents = File.ReadAllText(_savePath);
				if (contents != null)
				{
					return JsonUtility.FromJson<SaveData>(contents);
				}
			}
			catch(Exception e)
      {
				Debug.LogException(e);
      }
    }

		Debug.Log("Save file hasn't been created yet, or was unreadable. Starting a new save file.");
		return new SaveData();
  }

	public static void SaveToDisk(SaveData data)
  {
		try
    {
			File.WriteAllText(_savePath, JsonUtility.ToJson(data));
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}
}
