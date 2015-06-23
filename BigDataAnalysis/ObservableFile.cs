using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BigDataAnalysis
{
    class ObservableFile
    {
        public event EventHandler<WordsChangedEventArgs> WordsChanged;
        public List<string> Words { get; set; }
        public ObservableFile(string fileName)
        {
            Words = new List<string>();

            var watcher = new FileSystemWatcher(fileName);
            watcher.Changed += Updated;
            watcher.Created += Updated;
            watcher.Deleted+=Deleted;
        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            Words.Clear();
            OnWordsChanged(new WordsChangedEventArgs{TypeOfChange = ChangeType.Cleared});
        }

        private void Updated(object sender, FileSystemEventArgs e)
        {
            // Get all the word in the file
            var allWords = AllWords(e.FullPath);

            // Remove any word in Words that are not in allWords
            Words.RemoveAll(word => !allWords.Contains(word));

            // Finally add all the word in allWords that are not in words.
            allWords.RemoveAll(word => Words.Contains(word));
            Words.AddRange(allWords);
            OnWordsChanged(new WordsChangedEventArgs { TypeOfChange = ChangeType.Updated });
        }

        private static List<string> AllWords(string file)
        {
            var lines = File.ReadAllLines(file);
            var allWords = new List<string>();
            foreach (var words in lines.Select(line => line.Split(',')))
            {
                allWords.AddRange(words);
            }
            return allWords;
        }

        protected virtual void OnWordsChanged(WordsChangedEventArgs e)
        {
            var handler = WordsChanged;
            if (handler != null) handler(this, e);
        }
    }

    public class WordsChangedEventArgs:EventArgs
    {
        public ChangeType TypeOfChange { get; set; }
    }

    public enum ChangeType
    {
        Updated,
        Cleared
    }
}
