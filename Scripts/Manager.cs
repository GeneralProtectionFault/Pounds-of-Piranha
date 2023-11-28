using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Manager : Node
{
    public static int LevelMoves { get; set; } = 0;
    public static int OverallMoves { get; set; }= 0;

    public static Dictionary<int,string> LevelDictionary = new Dictionary<int,string>();


    public Manager()
    {
        var ext = new List<string> {".tscn"};
        var LevelFiles = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "Scenes", "Levels"))
        .Where(s => ext.Contains(Path.GetExtension(s).TrimStart().ToLowerInvariant()) && !s.Contains("Test"));
        
        foreach(var LevelFile in LevelFiles)
        {
            // GD.Print(file);

            using(StreamReader file = new StreamReader(LevelFile)) 
            {
                string ln;

                while ((ln = file.ReadLine()) != null) 
                {
                    if (ln.Contains("metadata/Sequence"))
                    {
                        int SequenceIndex = ln.IndexOf('=') + 2;
                        var LevelSequence = Convert.ToInt32(ln.Substring(SequenceIndex));
                        // GD.Print(LevelSequence);

                        // Assign level file to dicitonary according the the sequence variable!
                        LevelDictionary[LevelSequence] = LevelFile;
                        break;
                    }
                }
                file.Close();
            }
        }

        // foreach(var level in LevelDictionary)
        // {
        //     GD.Print(level);
        // }
    }
}
