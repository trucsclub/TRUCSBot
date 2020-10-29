using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using DSharpPlus;

using Newtonsoft.Json;

namespace TRUCSBot
{
    public class MessageScanner
    {
        public List<string> FlagWords { get; private set; } = null;

        private string _filename;

        public MessageScanner(string flagWordFile)
        {
            _filename = flagWordFile;
            if (File.Exists(flagWordFile))
            {
                Console.WriteLine("Found flag words config file. Reading...");
                FlagWords = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(flagWordFile));
            }
            if (FlagWords == null)
                FlagWords = new List<string>();

            Console.WriteLine($"Found {FlagWords.Count} flagged words.");

        }

        public bool ScanMessage(string message)
        {
            var lower = message.ToLower();
            foreach (var f in FlagWords)
            {
                if (lower.Contains(f.ToLower()))
                    return true;
            }
            return false;
        }

        public void AddFlaggedWord(string word)
        {
            FlagWords.Add(word.ToLower());
            Save();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(_filename, JsonConvert.SerializeObject(FlagWords));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving flag words: " + ex.Message, LogLevel.Error);
            }
        }
    }
}
