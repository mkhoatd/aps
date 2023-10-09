using System.Text.RegularExpressions;
using Sharprompt;

var userHomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var awsDir = Path.Combine(userHomeDir, ".aws");
var awsConfigFilePath = Path.Combine(awsDir, "config");

// Read the whole config file
var config = File.ReadAllText(awsConfigFilePath);

// Detect profiles
var profiles = Regex.Matches(config, @"\[(?<profile>[^\]]+)\]")
    .Select(m => m.Groups["profile"].Value.Trim().Replace("profile ", ""))
    .ToList();

var zshrcFilePath = Path.Combine(userHomeDir, ".zshrc");
var apsConfigFileDir = Path.Combine(userHomeDir, "aps");
var apsConfigFilePath = Path.Combine(apsConfigFileDir, ".aps_config");

// Check if aps dir exists
if (!Directory.Exists(apsConfigFileDir))
{
    Directory.CreateDirectory(apsConfigFileDir);
}

// Check if apsFile exists
if (!File.Exists(apsConfigFilePath))
{
    File.WriteAllText(apsConfigFilePath, "export AWS_PROFILE=default");
}

var apsConfigContent = File.ReadAllText(apsConfigFilePath).Split("\n");

if (!apsConfigContent.Length.Equals(1) || !apsConfigContent[0].Contains("AWS_PROFILE"))
{
    File.WriteAllText(apsConfigFilePath, "export AWS_PROFILE=default");
}

apsConfigContent = File.ReadAllText(apsConfigFilePath).Split("\n");
var zshrcContent = File.ReadAllText(zshrcFilePath);

var apsSpecificSetting = """
                        # >>> aps initialize >>>
                        # !! Contents within this block are managed by 'aps' !!
                        source ~/aps/.aps_config
                        alias aps="aps && source ~/aps/.aps_config"
                        # <<< aps initialize <<<
                        """;

if (!zshrcContent.Contains(apsSpecificSetting))
{
    var newZshrc = zshrcContent + apsSpecificSetting;
    var bakZshrcFilePath = Path.Combine(userHomeDir, ".zshrc.bak");
    File.Copy(zshrcFilePath, bakZshrcFilePath, true);
    File.WriteAllText(zshrcFilePath, newZshrc);
}

var profileLine = apsConfigContent[0]!;
var oldProfileName = profileLine.Split("=").LastOrDefault()?.Trim();
var newProfileName = Prompt.Select("Select new profile", profiles);

if (oldProfileName != null)
{
    var bakApsConfigFileePath = Path.Combine(apsConfigFileDir, ".aps_config.bak");
    File.Copy(apsConfigFilePath, bakApsConfigFileePath, true);
    var newApsConfigContent = profileLine.Replace(oldProfileName, newProfileName);
    File.WriteAllText(apsConfigFilePath, newApsConfigContent);
}
else
{
    var newApsConfigContent = new string[] { $"export AWS_PROFILE={newProfileName}" };
    File.WriteAllLines(apsConfigFilePath, newApsConfigContent);
}