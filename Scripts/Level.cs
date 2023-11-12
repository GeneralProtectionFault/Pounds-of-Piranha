using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node2D
{
	[Export] public bool ShowGridLines = false;


	public static event EventHandler CommenceTurn;


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



	private static List<Node2D> NumberSpawnNodes = new List<Node2D>();
	private static List<Node2D> NumberNodes = new List<Node2D>();


	private static List<Line2D> GridDebugLines = new List<Line2D>();
	public static bool LinesPopulated = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScaleObject.WeightChanged += SpawnNumber;

		// This gets the nodes that are the spawn locations
		foreach(Node2D SpawnNode in GetTree().GetNodesInGroup("NumberSpawns"))
		{
			NumberSpawnNodes.Add(SpawnNode);
		}

		SpawnNumber(this, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// QueueRedraw();

		if (Input.IsActionJustPressed("reset"))
		{
			GetTree().ReloadCurrentScene();
		}
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

		if (ShowGridLines && !LinesPopulated)
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


	
	private async void SpawnNumber(object sender, int number)
	{
		// First, clear 'em
		foreach(var Number in NumberNodes.ToList())
		{
			Number.QueueFree();
			await ToSignal(Number, "tree_exited");
		}

		
		NumberNodes.Clear();
		
		var NumberAsString = number.ToString();

		for (int i = 0; i < NumberAsString.Length; i++)
		{
			var Digit = NumberAsString[i];
			var NumberScene = ResourceLoader.Load<PackedScene>($"res://Scenes/Numbers/{Digit}.tscn").Instantiate();
			NumberNodes.Add((Node2D)NumberScene);
			NumberSpawnNodes[i].AddChild(NumberScene);
			
		}

		CreateGrid();
		QueueRedraw();
	}



	private void CreateGrid()
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
			GD.Print($"Number in NumberScenes that is in tree: {Number}");

			var NumberPosition = (Number as Node2D).GlobalPosition;
			GD.Print($"{Number.Name} (Global) Position: {NumberPosition}");

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


		GD.Print($"Top Left Number Scene: {TopLeftNumber.Name}");
		GD.Print($"Top Left Number X Position: {TopLeftNumber_X}");
		GD.Print($"Top Left Number Y Position: {TopLeftNumber_Y}");
		GD.Print($"Bottom Right Number X Position: {BottomRightNumber_X}");
		GD.Print($"Bottom Right Number Y Position: {BottomRightNumber_Y}");

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

		GD.Print($"Grid Region: {Grid.Region}");
		GD.Print($"Grid Offset: {Grid.Offset}");
		GD.Print($"Grid Cell Size: {Grid.CellSize}");
		GD.Print($"Grid Top Left: {GridTopLeft}");
		GD.Print($"Grid Bottom Right: {GridBottomRight}");	
		
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
		GD.Print($"Cell from Coordinates - {Coordinates} - {Cell}");

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
}
