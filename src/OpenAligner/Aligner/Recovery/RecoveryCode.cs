using OpenAiAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AdvancedAligner.Recovery
{
    public class RecoveryCode
    {
        public static List<Dataset> Recover()
        {
            List<Dataset> result = new List<Dataset>();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "JSONL files (*.jsonl)|*.jsonl|All files (*.*)|*.*";
            openFileDialog1.Title = "Select the dataset file";
            DialogResult dialogResult = openFileDialog1.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(openFileDialog1.FileName);
                foreach (string line in lines)
                {
                    Dataset dataset = JsonSerializer.Deserialize<Dataset>(line);
                    result.Add(dataset);
                }
            }

            return result;
        }

        public static List<PromptResponse> RecoverX()
        {
            List<PromptResponse> promptResponses = new List<PromptResponse>();

            FolderBrowserDialog folderBrowserDialog = new();

            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                List<string> files = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.txt").ToList();
                files.Sort();

                // we process the txt files in pairs.
                // the files nnnnP.txt contain the prompt, and the files nnnnR.txt contain the response.
                // we assume that the files are named in a way that they can be sorted correctly, e.g., 0001P.txt, 0001R.txt, 0002P.txt, 0002R.txt, etc.
                for (int i = 0; i < files.Count; i += 2)
                {
                    string promptFile = files[i];
                    string responseFile = files[i + 1];
                    if (!promptFile.EndsWith("P.txt") || !responseFile.EndsWith("R.txt"))
                    {
                        throw new Exception($"Unexpected file naming: {promptFile}, {responseFile}");
                    }
                    string promptContent = File.ReadAllText(promptFile);
                    string responseContent = File.ReadAllText(responseFile);
                    var prompt = JsonSerializer.Deserialize<Prompt>(promptContent);
                    var response = JsonSerializer.Deserialize<ResponseX>(responseContent);
                    PromptResponse promptResponse = new PromptResponse
                    {
                        prompt = prompt,
                        response = response
                    };
                    promptResponses.Add(promptResponse);
                }


            }

            return promptResponses;
        }
    }
}
