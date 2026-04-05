// COMPLETE LLM-BASED HEBREW ALIGNMENT PIPELINE
// NOW WITH RAG (Automatic Example Selector)

// Includes:
// 1. API call (Responses API)
// 2. JSON schema enforcement
// 3. Retry + validation
// 4. Batch processing
// 5. Ready for interlinear export

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace AdvancedAligner.Examples
{
    using System.Diagnostics;
    // ================= USAGE WITH DATABASE =================
    // Example usage:
    // var db = new ExampleDatabase("examples.json");
    // var selector = new ExampleSelector(db.Examples);
    // var result = await service.AlignVerseAsync(verse, selector);
    // db.AddExample(new ExampleAlignment { ... });

    // ================= PERSISTENT EXAMPLE DATABASE (FILE) =================
    using System.IO;
    using System.Text.Encodings.Web;
    using System.Text.Unicode;

    public class ExamplesDatabase
    {
        private readonly string saveFolder = "ExamplesDB";
        private readonly string backupFolderName = "Backup";
        private readonly string backupFolderPath = string.Empty;
        private readonly string fileName = "Examples";
        private readonly string fileExtension = "json";
        private readonly string dbFilePath = string.Empty;

        public List<ExampleAlignment> Examples { get; private set; } = new();

        public ExamplesDatabase()
        {
            if(!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);
            
            backupFolderPath = Path.Combine(saveFolder, backupFolderName);
            if (!Directory.Exists(backupFolderPath))
                Directory.CreateDirectory(backupFolderPath);

            dbFilePath = Path.Combine(saveFolder, $"{fileName}.{fileExtension}");  

            Load();
        }

        public void Load()
        {
            if (!File.Exists(dbFilePath))
            {
                Examples = new();
                return;
            }

            var json = File.ReadAllText(dbFilePath);
            Examples = JsonSerializer.Deserialize<List<ExampleAlignment>>(json) ?? new List<ExampleAlignment>();


            Dirty = false;
        }

        public bool Dirty {  get; set; }
        public void Clear()
        {
            Examples.Clear();
            Dirty = true;
        }

        public int Delete(string reference)
        { 
            int result = Examples.RemoveAll(x => x.Reference == reference);
            if (result > 0)
                Dirty = true; 
            
            return result;
        }
        public void Save()
        {
            if (Dirty)
            {
                var json = JsonSerializer.Serialize(Examples, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true
                });

                if (File.Exists(dbFilePath))
                {
                    // backUp the current file by renaming it
                    // the new name will be the fileName affixed by a timestamp 
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(":","-");
                    string destFileName = $"{fileName}-{timestamp}.{fileExtension}";
                    string destPath = Path.Combine(backupFolderPath, destFileName);
                    if (File.Exists(destPath))
                    {
                        // if a file with the same name already exists in the backup folder, append a guid to the filename
                        string guid = Guid.NewGuid().ToString();
                        destFileName = $"{fileName}-{timestamp}-{guid}.{fileExtension}";
                        destPath = Path.Combine(backupFolderPath, destFileName);
                    }
                    File.Move(dbFilePath, destPath);
                }
                    
                File.WriteAllText(dbFilePath, json);
                Dirty = false;
            }
        }

        public void AddExample(ExampleAlignment example)
        {
            // Prevent duplicates by reference
            if (Examples.Any(e => e.Reference == example.Reference))
                return;

            Examples.Add(example);
            Dirty = true;
            //Save();
        }
    }


}
