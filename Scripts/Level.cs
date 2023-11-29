using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;



public partial class Level : Node2D
{
	[Export] public bool ShowGridLines = false;
	[Export(PropertyHint.Range, "1,4")] public int Digits = 1;


	public enum LevelState {Play, FishMoving}
	public static LevelState CurrentLevelState = LevelState.Play;

	public static Node2D LevelObject;
	public static Level LevelTemplateObject;

	public static event EventHandler CommenceTurn;

	public static Label LevelLabel;
	public static Label TotalLabel;
	public static AStarGrid2D Grid;

	public static int TopLeftNumber_X = -1;
	public static int TopLeftNumber_Y = -1;
	public static int BottomRightNumber_X = -1;
	public static int BottomRightNumber_Y = -1;



	public static Vector2I GridTopLeft = new Vector2I(0,0);
	public static Vector2I GridBottomRight = new Vector2I(0,0);


	// Size of a single number in pixels
	public const int XResolution = 245;
	public const int YResolution = 175;



	// private static List<Node2D> NumberSpawnNodes = new List<Node2D>();
	// private static List<Node2D> NumberNodes = new List<Node2D>();
	private static Node2D NegativeSymbol;


	// private static List<Line2D> GridDebugLines = new List<Line2D>();
	public static bool LinesPopulated = false;

	public Area2D LevelBody;

	// public static AudioStreamPlayer AteFishSound;
	// public static AudioStreamPlayer LevelUpSound;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Gets the level, NOT the level template
		LevelObject = this.GetParent() as Node2D;
		LevelTemplateObject = this;
		ScaleObject.WeightChanged += SpawnNumber;

		NegativeSymbol = GetNode<Node2D>("NumberSpawns/Negative");
		LevelBody = GetNode<Area2D>("LevelBody");
		LevelLabel = GetNode<Label>("Label_Level/Label_Level_Number");
		TotalLabel = GetNode<Label>("Label_Total/Label_Total_Number");
		// AteFishSound = GetNode<AudioStreamPlayer>("AteFishSound");
		// LevelUpSound = GetNode<AudioStreamPlayer>("LevelUpSound");

		LevelLabel.Text = Manager.LevelMoves.ToString();
		TotalLabel.Text = Manager.OverallMoves.ToString();

		// This gets the nodes that are the spawn locations
		// foreach(Node2D SpawnNode in GetTree().GetNodesInGroup("NumberSpawns"))
		// {
		// 	NumberSpawnNodes.Add(SpawnNode);
		// }

		SpawnNumber(this, 0);

		Manager.Instance.InitializeMusic();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// QueueRedraw();
	}


	public void RestartLevel()
	{
		// GetTree().ChangeSceneToFile(GetTree().CurrentScene.SceneFilePath);
		ResetLevelVariables();
	
		var ReloadResult = GetTree().ReloadCurrentScene();
		GD.Print($"Reloading scene.\nReload Result: {ReloadResult}");
		ScaleObject.WeightChanged -= SpawnNumber;
		Level.CurrentLevelState = Level.LevelState.Play;
	}


	public void ResetLevelVariables()
	{
		Grid = null;

		TopLeftNumber_X = -1;
		TopLeftNumber_Y = -1;
		BottomRightNumber_X = -1;
		BottomRightNumber_Y = -1;
		GridTopLeft = new Vector2I(0,0);
		GridBottomRight = new Vector2I(0,0);
		// NumberSpawnNodes = new List<Node2D>();
		// NumberNodes = new List<Node2D>();
		LinesPopulated = false;

		ScaleObject.ResetLevelScore();
	}



	public void TreeExit()
	{
		// GD.Print("Level exiting tree...");
		ScaleObject.WeightChanged -= SpawnNumber;
	}


    public void TestButtonPressed()
	{
		CommenceTurn?.Invoke(this, EventArgs.Empty);
	}

	public void NumberSetButtonPressed()
	{
		LinesPopulated = false;

		var NumberOverride = GetNode<TextEdit>("NumberOverride");
		try 
		{
			var Number = Convert.ToInt32(NumberOverride.Text);
			SpawnNumber(this, Number);
		}
		catch
		{
			GD.Print("Converting override to an integer failure.  Don't be a jerk, but in an integer!");
		}
	}

	public void TestNumberChanged()
	{
		QueueRedraw();
	}


	public override void _Draw()
	{
		// GD.Print("Drawing!");

		if (ShowGridLines && !LinesPopulated && Grid is not null)
			DrawAstarGrid();
	}

	
	private void DrawAstarGrid()
	{
		// Show AstarGrid2D lines
		// End.X & End.Y are the number of cells in the grid
		for (int i = 0; i < Grid.Region.End.X; i++)
		{
			for (int n = 0; n < Grid.Region.End.Y; n++)
			{
				// Top-left corner of cell
				var XCoord1 = GridTopLeft.X + (XResolution * i);
				var YCoord1 = GridTopLeft.Y + (YResolution * n);
				// Top-right corner of cell
				var XCoord2 = XCoord1 + XResolution;
				var YCoord2 = YCoord1;
				// Bottom-left corner of cell
				var XCoord3 = XCoord1;
				var YCoord3 = YCoord1 + YResolution;
				// Bottom-right corner of cell
				var XCoord4 = XCoord2;
				var YCoord4 = YCoord3;

				DrawLine(new Vector2(XCoord1, YCoord1), new Vector2(XCoord2, YCoord2), Colors.Red, 1.0f);
				DrawLine(new Vector2(XCoord1, YCoord1), new Vector2(XCoord3, YCoord3), Colors.Red, 1.0f);
				DrawLine(new Vector2(XCoord2, YCoord2), new Vector2(XCoord4, YCoord4), Colors.Red, 1.0f);
				DrawLine(new Vector2(XCoord3, YCoord3), new Vector2(XCoord4, YCoord4), Colors.Red, 1.0f);
			}
		}

		LinesPopulated = true;
	}




	/// <summary>
	/// Determine if a cell in the AstarGrid2D is on the "edge," as in outiside the numbers.
	/// In the event this is the case and not the cell the fish started in, it can be considered the "goal"
	/// </summary>
	public static bool IsEdgeCell(Vector2I Cell)
	{
		var MaxX = Grid.Region.Size.X - 1;
		var MaxY = Grid.Region.Size.Y - 1;

		// GD.Print($"MaxX: {MaxX}");
		// GD.Print($"MaxY: {MaxY}");
		// GD.Print($"CellX: {Cell.X}");
		// GD.Print($"CellY: {Cell.Y}");

		if (Cell.X == 0 || Cell.X == MaxX ||
		Cell.Y == 0 || Cell.Y == MaxY)
			return true;
		else
			return false;
	}



	private async void SpawnNumber(object sender, int number)
	{
		// First, clear 'em
		// if (NumberNodes.Count > 0)
		// {
		// 	foreach(var Number in NumberNodes.ToList())
		// 	{
		// 		Number.QueueFree();
		// 		await ToSignal(Number, "tree_exited");
		// 	}
		// }

		foreach(Node2D Number in GetTree().GetNodesInGroup("NumberScenes"))
		{
			if (IsInstanceValid(Number))
			{
				Number.CallDeferred("free");
				// await ToSignal(Number, "tree_exited");
			}
		}

		// NumberNodes.Clear();
		
		//ParameterizedThreadStart NumbersStart = new ParameterizedThreadStart(RepopulateNumbers);

		// var Start = new ParameterizedThreadStart(RepopulateNumbers);
		// NewNumbersThread = new Thread(Start);
		// NewNumbersThread.Start(number);
		
		
		RepopulateNumbers(number);
		// CallDeferred("RepopulateNumbers", number);
	}


	

	private async void RepopulateNumbers(int number)
	{
		GD.Print("Repopulating Numbers");

		if (number < 0)
		{
			NegativeSymbol.Visible = true;
		}
		else
		{
			NegativeSymbol.Visible = false;
		}

		var NumberAsString = Mathf.Abs(number).ToString();

		// Populate the desired leading 0s
		var WeightDigits = NumberAsString.Length;
		var ExtraDigits = Digits - WeightDigits;

		// GD.Print($"Weight Digits: {WeightDigits}");
		// GD.Print($"Extra Digits: {ExtraDigits}");

		for (int i = 0; i < ExtraDigits; i++)
		{
			//var ZeroScene = ResourceLoader.Load<PackedScene>($"res://Scenes/Numbers/0.tscn").Instantiate();
			//NumberNodes.Add((Node2D)ZeroScene);
			PopulateSingleNumber(number, '0', i);
		}


		for (int i = 0; i < WeightDigits; i++)
		{
			var Digit = NumberAsString[i];
			// PopulateSingleNumber(number, Digit, i + ExtraDigits);
			CallDeferred("PopulateSingleNumber", number, Digit, i + ExtraDigits);
		}


		Tween DumbDelayTween1 = GetTree().CreateTween();
		DumbDelayTween1.TweenCallback(Callable.From(() => {
				// await Task.Run(() => {  });
				CreateGrid();
				SnapAllFishToGrid();
			}
        )).SetDelay(.15f);
		
		// await Task.Run(() => {  }); 
		// CreateGrid().WaitToFinish();
				
		// CallDeferred("FireTurnEvent");
		Tween DumbDelayTween2 = GetTree().CreateTween();
		DumbDelayTween2.TweenCallback(Callable.From(() => 
            FireTurnEvent()
        )).SetDelay(.15f);
	}

	private async void PopulateSingleNumber(int number, char Digit, int NumberPosition)
	{
		var NumberScene = ResourceLoader.Load<PackedScene>($"res://Scenes/Numbers/{Digit}.tscn").Instantiate();
		// NumberNodes.Add((Node2D)NumberScene);

		// NumberSpawnNodes[NumberPosition].AddChild(NumberScene);
		// NumberSpawnNodes[NumberPosition].CallDeferred("add_child", NumberScene);
		var SpawnNode = GetNode<Node2D>($"NumberSpawns/Digit_{NumberPosition + 1}");
		SpawnNode.CallDeferred("add_child", NumberScene);

		// CallDeferred("add_child", NumberSpawnNodes[i]);

		var Sprite = NumberScene.GetNode<Sprite2D>($"{NumberScene.Name}");
		if (number < 0)
		{
			Sprite.Modulate = new Color(0xd41e48ff);
		}
		else
		{
			Sprite.Modulate = new Color(0xffffffff);
		}

		// await ToSignal(GetTree(), "process_frame");
	}



	private async void FireTurnEvent()
	{
		// var Result = NewNumbersThread.WaitToFinish();

		// Fire to automatically move after weight change
		CommenceTurn?.Invoke(this, EventArgs.Empty);
	}



	private async void CreateGrid()
	{
		Grid = new AStarGrid2D();
		Node2D TopLeftNumber = new Node2D();

		TopLeftNumber_X = -1;
		TopLeftNumber_Y = -1;
		BottomRightNumber_X = -1;
		BottomRightNumber_Y = -1;


		// Get the top-left number on the screen
		foreach (var Number in GetTree().GetNodesInGroup("NumberScenes"))
		{
			// GD.Print($"Number in NumberScenes that is in tree: {Number}");

			var NumberPosition = (Number as Node2D).GlobalPosition;
			// GD.Print($"{Number.Name} (Global) Position: {NumberPosition}");

			if (TopLeftNumber_X == -1 || NumberPosition.X < TopLeftNumber_X)
			{
				TopLeftNumber_X = (int)NumberPosition.X;
				TopLeftNumber = Number as Node2D;
			}

			if (TopLeftNumber_Y == -1 || NumberPosition.Y < TopLeftNumber_Y)		
			{
				TopLeftNumber_Y = (int)NumberPosition.Y;
				TopLeftNumber = Number as Node2D;
			}

			if (BottomRightNumber_X == -1 || NumberPosition.X > BottomRightNumber_X)
			{
				BottomRightNumber_X = (int)NumberPosition.X;
			}

			if (BottomRightNumber_Y == -1 || NumberPosition.Y > BottomRightNumber_Y)
			{
				BottomRightNumber_Y = (int)NumberPosition.Y;
			}
		}


		// GD.Print($"Top Left Number Scene: {TopLeftNumber.Name}");
		// GD.Print($"Top Left Number X Position: {TopLeftNumber_X}");
		// GD.Print($"Top Left Number Y Position: {TopLeftNumber_Y}");
		// GD.Print($"Bottom Right Number X Position: {BottomRightNumber_X}");
		// GD.Print($"Bottom Right Number Y Position: {BottomRightNumber_Y}");

		GridTopLeft = new Vector2I(TopLeftNumber_X - XResolution, TopLeftNumber_Y - YResolution);
		// The "*2" is because the origin of the image/scene is the upper left, so this will effectively
		// set the point to an extra "blank" cell to the right & down of the actual bottom-right number
		// The "*3" on the Y is because a number is broken into two vertical cells
		GridBottomRight = new Vector2I(BottomRightNumber_X + (XResolution*2), BottomRightNumber_Y + (YResolution*3));

		//Grid.Region = new Rect2I(GridTopLeft, GridBottomRight);
		Grid.Region = new Rect2I(CellFromCoordinates(GridTopLeft), CellFromCoordinates(GridBottomRight));
		
		Grid.CellSize = new Vector2I(XResolution, YResolution);
		Grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		
		Grid.Update();

		// GD.Print($"Grid Region: {Grid.Region}");
		// GD.Print($"Grid Offset: {Grid.Offset}");
		// GD.Print($"Grid Cell Size: {Grid.CellSize}");
		// GD.Print($"Grid Top Left: {GridTopLeft}");
		// GD.Print($"Grid Bottom Right: {GridBottomRight}");	
		
		QueueRedraw();
	}


	



    public static Vector2I CellFromCoordinates(Vector2 Coordinates)
	{
		// The +1 here is because rounding can yield us a pixel short, and casting to an int might get us the wrong number
		var XOffset = Coordinates.X - GridTopLeft.X + 1;
		var YOffset = Coordinates.Y - GridTopLeft.Y + 1;
		
		// var XCellCount = (GridBottomRight.X - GridTopLeft.X + 2) / XResolution;
		// GD.Print($"X Cell Cnt: {XCellCount}");

		var XCellPosition = (int)Mathf.Floor(XOffset / XResolution);
		var YCellPosition = (int)Mathf.Floor(YOffset / YResolution);


		var Cell = new Vector2I(XCellPosition, YCellPosition);
		// GD.Print($"Cell from Coordinates - {Coordinates} - {Cell}");

		return Cell;
	}


	

	public static Vector2 CoordinatesFromCell(Vector2I Cell)
	{
		// GD.Print($"Passed in Cell: {Cell}");

		var XTopLeft = GridTopLeft.X + (Cell.X * XResolution);
		var YTopLeft = GridTopLeft.Y + (Cell.Y * YResolution);

		// GD.Print($"Grid Top Left: {GridTopLeft}");
		
		// Get the CENTER of the cell
		var XPostion = XTopLeft + (XResolution / 2);
		var YPosition = YTopLeft + (YResolution / 2);

		var Target = new Vector2(XPostion, YPosition);
		//GD.Print($"Converted Target Position: {Target}");
		return Target;
	}


	private void SnapAllFishToGrid()
	{
		foreach (Fish fish in GetTree().GetNodesInGroup("GoodFish"))
			SnapFishToGrid(fish);
		foreach (Fish fish in GetTree().GetNodesInGroup("BadFish"))
			SnapFishToGrid(fish);
	}

	private void SnapFishToGrid(Fish fish)
	{
		// Will return the cell regardless of if in the exact center
		var Cell = CellFromCoordinates(fish.GlobalPosition);
		// Set to exact center :)
		fish.GlobalPosition = CoordinatesFromCell(Cell);
	}

}
