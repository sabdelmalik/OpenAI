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
        private readonly string _filePath;
        public List<ExampleAlignment> Examples { get; private set; } = new();

        public ExamplesDatabase()
        {
            string folder = "ExamplesDB";
            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            _filePath = $@"{folder}\Examples.json";
            Load();
        }

        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                Examples = new();
                return;
            }

            var json = File.ReadAllText(_filePath);
            Examples = JsonSerializer.Deserialize<List<ExampleAlignment>>(json) ?? new List<ExampleAlignment>();
        }

        public void Clear()
        {
            Examples.Clear();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Examples, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }

        public void AddExample(ExampleAlignment example)
        {
            // Prevent duplicates by reference
            if (Examples.Any(e => e.Reference == example.Reference))
                return;

            Examples.Add(example);
            Save();
        }
    }


}
