using LitJson;
using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;

namespace ONS.MontadorDessem.Build.Utilities
{
    public class UpdateJson : Task
    {
        public string InputFileName { get; set; }
        public string OutputFileName { get; set; }
        public string ParametersFile { get; set; }

        public override bool Execute()
        {
            var settings = JsonMapper.ToObject(File.ReadAllText(InputFileName));
            var parameters = JsonMapper.ToObject(File.ReadAllText(ParametersFile));

            Load(settings, parameters);

            File.WriteAllText(OutputFileName, settings.ToJson());

            return true;
        }

        private void Load(JsonData inputSettings, JsonData parameters, JsonData parentSettings = null, string path = "")
        {
            var key = path?.Split('.')?.LastOrDefault();

            if (inputSettings.IsString && parameters.ContainsKey(inputSettings.ToString()))
            {
                parentSettings[key] = parameters[inputSettings.ToString()];
            }
            if (parameters.ContainsKey(path))
            {
                parentSettings[key] = parameters[path];
            }
            else
            {
                if (inputSettings.IsObject)
                {
                    path = string.IsNullOrEmpty(path) ? "" : $"{path}.";

                    foreach (var childKey in inputSettings.Keys.ToList())
                    {
                        var child = inputSettings[childKey];

                        Load(child, parameters, inputSettings, $"{path}{childKey}");
                    }
                }
                else if (inputSettings.IsArray)
                {
                    for (int i = 0; i < inputSettings.Count; ++i)
                    {
                        Load(inputSettings[i], parameters, inputSettings, $"{path}[{i}]");
                    }
                }
            }
        }
    }
}