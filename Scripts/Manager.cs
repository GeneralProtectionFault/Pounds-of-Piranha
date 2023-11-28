using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Manager : Node
{
    public static Manager Instance;
    public static int LevelMoves { get; set; } = 0;
    public static int OverallMoves { get; set; }= 0;

    public static Dictionary<int,string> LevelDictionary = new Dictionary<int,string>();
    // public static AudioStreamPlayer PiranhaDance;
    // public static AudioStreamPlayer Waves;

    public static bool MusicStarted = false;

    public Manager()
    {
        if (Instance is null)
            Instance = this;


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

        
    
    }

    public async void InitializeMusic()
    {
        // Load ze' music here so it persists across scenes instead of a most irritating restart on level advancement
        if (!MusicStarted)
        {
            var PiranhaDance = new AudioStreamPlayer();
            var PiranhaDanceResource = ResourceLoader.Load<AudioStream>("res://Audio/PiranhaDance.ogg");
            PiranhaDance.Stream = PiranhaDanceResource;
            GetTree().Root.CallDeferred("add_child", PiranhaDance);
            await ToSignal(GetTree(), "process_frame");
            PiranhaDance.Play();

            var Waves = new AudioStreamPlayer();
            var WavesResource = ResourceLoader.Load<AudioStream>("res://Audio/mixkit-close-sea-waves-loop-1195.wav");
            Waves.Stream = WavesResource;
            Waves.VolumeDb = -22;
            GetTree().Root.CallDeferred("add_child", Waves);
            await ToSignal(GetTree(), "process_frame");
            Waves.Play();
            
            
            MusicStarted = true;
        }
    }
}
