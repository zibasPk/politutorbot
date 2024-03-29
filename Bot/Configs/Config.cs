﻿using Newtonsoft.Json;
using Serilog;

namespace Bot.configs;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public abstract class Config
{
    [JsonIgnore]
    private protected string FilePath = "";

    /// <summary>
    /// Initializes the configuration class
    /// by loading the config data from json file. If no file exists it generates an empty one.
    /// </summary>
    public void Initialize()
    {
        try
        {
            var text = File.ReadAllText(FilePath);
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            JsonConvert.PopulateObject(text, this,settings);
        }
        catch (Exception e)
        {
            switch (e)
            {
                case FileNotFoundException:
                    Log.Warning("No configuration json file found, generating template file in " + FilePath);
                    GenerateEmptyConfig(FilePath);
                    break;
                case DirectoryNotFoundException:
                    Log.Warning("/config/ directory not found, couldn't generate template file");
                    break;
                default:
                    throw;
            }
        }
    }

    /// <summary>
    /// Generates an empty config json.
    /// </summary>
    private protected void GenerateEmptyConfig(string filePath)
    {
        var x = JsonConvert.SerializeObject(this);
        File.WriteAllText(filePath, x);
    }
}