using ii.dc.Model;
using ii.SimpleZip;
using Marvin.JsonPatch.Dynamic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.IO.Compression;
using System.Reflection;

/*
var xxfileTextIn = File.ReadAllText(@$"C:\Users\user\Desktop\WL7.are");
var xxfileObject = JsonConvert.DeserializeObject<ExpandoObject>(xxfileTextIn);
var xxpatchDocument = new JsonPatchDocument();

var newCreature = @"{
      ""Descriptor"":{
        ""Id"":""wl_forest_kobold"",
        ""Type"":""BGModel.Reference.CreatureDescRef""
      },
      ""GroupID"":"""",
      ""IdleAnimGroupOverride"":"""",
      ""InstanceItems"":[
        {
          ""Amount"":300,
          ""Area"":{
            ""Id"":""dummy"",
            ""Type"":""BGModel.Reference.AreaDescRef""
          },
          ""CurrentContainerSlotType"":{
            ""Id"":""backpack"",
            ""Type"":""BGModel.Reference.InventorySlotTypeDescRef""
          },
          ""DroppedByCreatureId"":"""",
          ""ItemDescriptor"":{
            ""Id"":""gold"",
            ""Type"":""BGModel.Reference.ItemDescRef""
          },
          ""OwnerId"":"""",
          ""Stealable"":true,
          ""Type"":""BGModel.State.ItemState""
        }
      ],
      ""LocalCreatureGroup"":"""",
      ""PosMarker"":{
        ""Id"":""CreatureSpawn_TEST"",
        ""Type"":""BGModel.Reference.PosMarkerRef""
      },
      ""Type"":""BGModel.Descriptors.Area.CreatureSpawn""
    }";
xxpatchDocument.Add("/InitialCreatureSpawns/-", newCreature);

var newSpawn = @"{
      ""K"":""CreatureSpawn_TEST"",
      ""V"":{
        ""RY"":126.6574,
        ""Type"":""BGModel.Descriptors.Area.V3PositionMarker"",
        ""X"":-20.09332,
        ""Y"":4.768372E-07,
        ""Z"":41.30723
      }
    }";
xxpatchDocument.Add("/PositionMarkers/-", newSpawn);

xxpatchDocument.ApplyTo(xxfileObject);

var xxfileTextOut = JsonConvert.SerializeObject(xxfileObject, Formatting.Indented);


xxfileTextOut = xxfileTextOut.Replace("\\\"", "\"");
xxfileTextOut = xxfileTextOut.Replace("\"{", "{");
xxfileTextOut = xxfileTextOut.Replace("}\"", "}");
xxfileTextOut = xxfileTextOut.Replace("\\r\\n", System.Environment.NewLine);
File.WriteAllText(@$"C:\Users\user\Desktop\WL7.out", xxfileTextOut);
*/


//Loop through all nodes in a JSON file
//var xxfileTextIn = File.ReadAllText(@$"C:\Users\user\Desktop\WL7.are");
//var xxfileObject = JsonConvert.DeserializeObject<ExpandoObject>(xxfileTextIn);
//DisplayProperties(xxfileObject, -1);


//args = new string[3];
//args[0] = "uninstall";
//args[1] = @"D:\data\bg";
////args[2] = @"D:\data\mymod\tweaks.json";
//args[2] = @"D:\data\typofix\typofix.json";

if (args.Length < 3 || args.Length > 4)
{
    Console.WriteLine("Expected usage");
    Console.WriteLine("  dc extract path_to_bgd.data output_directory archived_path/archived_file.ext");
    Console.WriteLine("  dc extractall path_to_bgd.data output_directory");
    Console.WriteLine("  dc install path_to_bgd.data path_to_mod_file.json");
    Console.WriteLine("  dc create path_to_bgd.data input_directory");
    return;
}

if (args[0].ToLower() == "create")
{
    var inputDirectory = args[2];
    var outputFile = args[1];
    var sz = new SimpleZipFile();

    sz.Create(inputDirectory, outputFile);
}

if (args[0].ToLower() == "extractall")
{
    var bdgPath = args[1];
    var outputDirectory = args[2];
    ZipFile.ExtractToDirectory(bdgPath, outputDirectory);
}

if (args[0].ToLower() == "extract")
{
    var bdgPath = args[1];
    var outputDirectory = args[2];
    var targetFile = args[3];

    outputDirectory = EnsurePathSeparator(outputDirectory);

    using var zf = ZipFile.Open(bdgPath, ZipArchiveMode.Read);
    foreach (var entry in zf.Entries)
    {
        if (entry.FullName.ToLower() == targetFile)
        {
            var outputFile = outputDirectory + targetFile;
            var directory = Path.GetDirectoryName(outputFile);
            Directory.CreateDirectory(directory);
            entry.ExtractToFile(outputFile);
            break;
        }
    }
}

if (args[0].ToLower() == "install")
{
    var modFile = args[2];
    var modFolder = Path.GetDirectoryName(modFile);
    modFolder = EnsurePathSeparator(modFolder);
    var backupFolder = Path.GetDirectoryName(modFile);
    backupFolder = $@"{EnsurePathSeparator(backupFolder)}backup\";
    var unpackedFolder = args[1];
    unpackedFolder = EnsurePathSeparator(unpackedFolder);

    Directory.CreateDirectory(backupFolder);

    var metadata = new Metadata();
    var mod = new Mod();
    var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(modFile, optional: false, reloadOnChange: false);
    var configuration = builder.Build();
    ConfigurationBinder.Bind(configuration.GetSection("Metadata"), metadata);
    ConfigurationBinder.Bind(configuration.GetSection("Mod"), mod);

    var thisInstallerVersion = Assembly.GetExecutingAssembly().GetName().Version.Major;
    if (metadata.RequiredInstallerVersion > thisInstallerVersion)
    {
        Console.WriteLine($"This is installer version {thisInstallerVersion}. Mod requires installer version {metadata.RequiredInstallerVersion}. Aborting.");
        return;
    }

    foreach (var action in mod.Actions)
    {
        if (action.Operation == "copy")
        {
            File.Copy($"{modFolder}{action.Source}", $"{unpackedFolder}{action.Destination}", true);
            LogFileAdded(backupFolder, $"{unpackedFolder}{action.Destination}");
        }

        if (action.Operation == "jpatch")
        {
            var fileTextIn = File.ReadAllText(@$"{unpackedFolder}{action.File}");
            var fileText = JsonConvert.DeserializeObject<dynamic>(fileTextIn);

            var n = fileText.SelectToken(action.Specifier);
            n[action.Additional] = action.Change;

            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            var jsonOutput = JsonConvert.SerializeObject(fileText, Formatting.Indented, settings);
            File.WriteAllText(@$"{unpackedFolder}{action.File}", jsonOutput);

            // Create a backup of the changed file (only once per mod)
            if (fileTextIn != jsonOutput)
            {
                var backupFileLocation = $@"{backupFolder}{action.File}";
                if (!File.Exists(backupFileLocation))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFileLocation));
                }
                File.WriteAllText(backupFileLocation, fileTextIn);
                LogFileChanged(backupFolder, backupFileLocation);
            }
        }

        if (action.Operation == "patch")
        {
            var fileTextIn = File.ReadAllText(@$"{unpackedFolder}{action.File}");
            var fileObject = JsonConvert.DeserializeObject<ExpandoObject>(fileTextIn);
            var patchDocument = new JsonPatchDocument();

            if (action.Data.Op == "add")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "replace")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "remove")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "copy")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            if (action.Data.Op == "move")
            {
                patchDocument.Add(action.Data.Path, action.Data.Value);
            }
            patchDocument.ApplyTo(fileObject);
            var fileTextOut = JsonConvert.SerializeObject(fileObject, Formatting.Indented);
            File.WriteAllText($@"{unpackedFolder}{action.File}", fileTextOut);

            // Create a backup of the changed file (only once per mod)
            if (fileTextIn != fileTextOut)
            {
                var backupFileLocation = $@"{backupFolder}{action.File}";
                if (!File.Exists(backupFileLocation))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFileLocation));
                }
                File.WriteAllText(backupFileLocation, fileTextIn);
                LogFileChanged(backupFolder, backupFileLocation);
            }
        }

        LogModInstallation("", $"{metadata.Name}");
    }
}

if (args[0].ToLower() == "uninstall")
{
    var modFile = args[2];
    var modFolder = Path.GetDirectoryName(modFile);
    modFolder = EnsurePathSeparator(modFolder);
    var backupFolder = Path.GetDirectoryName(modFile);
    backupFolder = $@"{EnsurePathSeparator(backupFolder)}backup\";
    var unpackedFolder = args[1];
    unpackedFolder = EnsurePathSeparator(unpackedFolder);

    var metadata = new Metadata();
    var mod = new Mod();
    var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(modFile, optional: false, reloadOnChange: false);
    var configuration = builder.Build();
    ConfigurationBinder.Bind(configuration.GetSection("Metadata"), metadata);

    var mostRecentModInstalled = GetMostRecentModInstalled("");
    if (mostRecentModInstalled != $"{metadata.Name}")
    {
        Console.WriteLine($"{metadata.Name} is not the most recently installed mod, aborting.");
        return;
    }

    ConfigurationBinder.Bind(configuration.GetSection("Mod"), mod);

    if (File.Exists($"{backupFolder}fileschanged.log"))
    {
        var lines = File.ReadAllLines($"{backupFolder}fileschanged.log");
        foreach (var line in lines)
        {
            var source = line;
            var thisLine = line.Replace(backupFolder, "");
            var destination = $"{unpackedFolder}{thisLine}";
            File.Copy(source, destination, true);
        }
    }

    if (File.Exists($"{backupFolder}filesadded.log"))
    {
        var lines = File.ReadAllLines($"{backupFolder}filesadded.log");
        foreach (var line in lines.Where(w => File.Exists(w)))
        {
            File.Delete(line);
        }
    }

    File.Delete($"{backupFolder}filesadded.log");
    File.Delete($"{backupFolder}fileschanged.log");
    Directory.Delete(backupFolder, true);

    LogModUninstallation("", metadata.Name);
}


Console.WriteLine("Process complete");
Console.WriteLine("- press any key -");
Console.ReadKey();

string EnsurePathSeparator(string s)
{
    if (!s.EndsWith(Path.DirectorySeparatorChar.ToString()))
    {
        s += Path.DirectorySeparatorChar;
    }
    return s;
}

void LogFileAdded(string backupfolder, string content)
{
    content += System.Environment.NewLine;
    File.AppendAllText($"{backupfolder}filesadded.log", content);
}

void LogFileChanged(string backupfolder, string content)
{
    content += System.Environment.NewLine;
    File.AppendAllText($"{backupfolder}fileschanged.log", content);
}

void LogModInstallation(string folder, string content)
{
    content += System.Environment.NewLine;
    File.AppendAllText($"{folder}mods.log", content);
}

void LogModUninstallation(string folder, string content)
{
    var lines = File.ReadAllLines($"{folder}mods.log");
    if ((lines?.Last() ?? String.Empty) == content)
    {
        var newLines = lines.Take(lines.Count() - 1).ToArray();
        File.WriteAllLines(folder, lines);
    }
}

string GetMostRecentModInstalled(string folder)
{
    if (File.Exists($"{folder}mods.log"))
    {
        var lines = File.ReadAllLines($"{folder}mods.log");
        return lines?.Last() ?? String.Empty;
    }
    return string.Empty;
}


void DisplayProperties(ExpandoObject expando, int indent)
{
    indent++;
    foreach (var property in (IDictionary<string, object>)expando)
    {
        var leftPad = string.Concat(Enumerable.Repeat(" ", indent*2));

        var isExpando = property.Value is ExpandoObject;
        var isList = property.Value is List<object>;

        var val = isExpando ? "" : " " + property.Value;
        if (isList)
        {
            Console.WriteLine(property.Key);

            foreach (var x in property.Value as List<object>)
            {
                if (x is ExpandoObject)
                {
                    DisplayProperties(x as ExpandoObject, indent);
                }
            }
        }
        else
        {
            Console.WriteLine(leftPad + property.Key + val);
            if (isExpando)
            {
                DisplayProperties((ExpandoObject)property.Value, indent);
            }
        }
    }
}