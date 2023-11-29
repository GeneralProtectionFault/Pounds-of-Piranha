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
    public static AudioStreamPlayer PiranhaDance;
    public static AudioStreamPlayer Waves;
    public static AudioStreamPlayer AteFishSound;
    public static AudioStreamPlayer LevelUpSound;

    public Manager()
    {
        if (Instance is null)
            Instance = this;


        // This mess will parse the actual text of the scene files to get the metadata w/o loading them
        // We'll use the sequence to determine level order in case we want to muck about later without creating a file naming/referential mess.
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
            PiranhaDance = new AudioStreamPlayer();
            var PiranhaDanceResource = ResourceLoader.Load<AudioStream>("res://Audio/PiranhaDance.ogg");
            PiranhaDance.Stream = PiranhaDanceResource;
            PiranhaDance.VolumeDb = -10;
            GetTree().Root.CallDeferred("add_child", PiranhaDance);
            await ToSignal(GetTree(), "process_frame");
            PiranhaDance.Play();

            Waves = new AudioStreamPlayer();
            var WavesResource = ResourceLoader.Load<AudioStream>("res://Audio/mixkit-close-sea-waves-loop-1195.wav");
            Waves.Stream = WavesResource;
            Waves.VolumeDb = -22;
            GetTree().Root.CallDeferred("add_child", Waves);
            await ToSignal(GetTree(), "process_frame");
            Waves.Play();
            
            
            // Sound Effects
            AteFishSound = new AudioStreamPlayer();
            var AteFishSoundResource = ResourceLoader.Load<AudioStream>("res://Audio/350987__cabled_mess__lose_c_05.wav");
            AteFishSound.Stream = AteFishSoundResource;
            GetTree().Root.CallDeferred("add_child", AteFishSound);
            await ToSignal(GetTree(), "process_frame");

            LevelUpSound = new AudioStreamPlayer();
            var LevelUpSoundResource = ResourceLoader.Load<AudioStream>("res://Audio/682633__bastianhallo__level-up.ogg");
            LevelUpSound.Stream = LevelUpSoundResource;
            GetTree().Root.CallDeferred("add_child", LevelUpSound);
            await ToSignal(GetTree(), "process_frame");

            MusicStarted = true;
        }
    }
}
