using Godot;
using System;
using System.Collections.Generic;

public partial class Level : Node
{
	public static event EventHandler WeightMoved;



	public static AStarGrid2D Grid;

	public static int TopLeftXPosition = -1;
	public static int TopLeftYPosition = -1;
	public static int BottomRightXPosition = -1;
	public static int BottomRightYPosition = -1;



	public static Vector2I GridTopLeft = new Vector2I(0,0);
	public static Vector2I GridBottomRight = new Vector2I(0,0);


	// Size of a single number in pixels
	public const int XResolution = 350;
	public const int YResolution = 250;



	private static List<Node2D> NumberSpawnNodes = new List<Node2D>();
	private static List<Node2D> NumberNodes = new List<Node2D>();



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// This gets the nodes that are the spawn locations
		foreach(Node2D SpawnNode in GetTree().GetNodesInGroup("NumberSpawns"))
		{
			NumberSpawnNodes.Add(SpawnNode);
		}

		SpawnNumber(0);
		CreateGrid();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}



	public void TestButtonPressed()
	{
		WeightMoved?.Invoke(this, EventArgs.Empty);
	}


	
	private void SpawnNumber(int number)
	{
		// First, clear 'em
		foreach(var Number in NumberNodes)
		{
			Number.QueueFree();
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

	}



	private void CreateGrid()
	{
		Grid = new AStarGrid2D();
		

		Node2D TopLeftNumber = new Node2D();

		// Get the top-left number on the screen
		foreach (var Number in GetTree().GetNodesInGroup("NumberScenes"))
		{
			var NumberPosition = (Number as Node2D).GlobalPosition;
			GD.Print($"{Number.Name}: {NumberPosition}");

			if (TopLeftXPosition == -1f || NumberPosition.X < TopLeftXPosition)
			{
				TopLeftXPosition = (int)NumberPosition.X;
				TopLeftNumber = Number as Node2D;
			}

			if (TopLeftYPosition == -1f || NumberPosition.Y < TopLeftYPosition)		
			{
				TopLeftYPosition = (int)NumberPosition.Y;
				TopLeftNumber = Number as Node2D;
			}

			if (BottomRightXPosition == -1f || NumberPosition.X > BottomRightXPosition)
			{
				BottomRightXPosition = (int)NumberPosition.X;
			}

			if (BottomRightYPosition == -1f || NumberPosition.Y > BottomRightYPosition)
			{
				BottomRightYPosition = (int)NumberPosition.Y;
			}
		}


		GD.Print($"Top Left Number Scene: {TopLeftNumber.Name}");
		GD.Print($"Top Left X: {TopLeftXPosition}");
		GD.Print($"Top Left Y: {TopLeftYPosition}");
		GD.Print($"Bottom Right X: {BottomRightXPosition}");
		GD.Print($"Bottom Right Y: {BottomRightYPosition}");

		GridTopLeft = new Vector2I(TopLeftXPosition - XResolution, TopLeftYPosition - YResolution);
		// The "*2" is because the origin of the image/scene is the upper left, so this will effectively
		// set the point to an extra "blank" cell to the right & down of the actual bottom-right number
		// The "*3" on the Y is because a number is broken into two vertical cells
		GridBottomRight = new Vector2I(BottomRightXPosition + (XResolution*2), BottomRightYPosition + (YResolution*3));

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



		
	}


	



    public static Vector2I CellFromCoordinates(Vector2 Coordinates)
	{
		var XOffset = Coordinates.X - GridTopLeft.X ;
		var YOffset = Coordinates.Y - GridTopLeft.Y;
		// The +1 here is because rounding can yield us a pixel short, and casting to an int might get us the wrong number (1 too few)
		var XCellCount = (GridBottomRight.X - GridTopLeft.X + 1) / XResolution;
		// GD.Print($"X Cell Cnt: {XCellCount}");

		var XCellPosition = (int)Mathf.Floor(XOffset / XResolution);
		var YCellPosition = (int)Mathf.Floor(YOffset / YResolution);


		var Cell = new Vector2I(XCellPosition, YCellPosition);
		GD.Print($"Cell: {Cell}");

		return Cell;
	}


	


	public static Vector2 CoordinatesFromCell(Vector2I Cell)
	{
		GD.Print($"Passed in Cell: {Cell}");

		var XTopLeft = GridTopLeft.X + (Cell.X * XResolution);
		var YTopLeft = GridTopLeft.Y + (Cell.Y * YResolution);

		GD.Print($"Grid Top Left: {GridTopLeft}");
		

		var XPostion = XTopLeft + (XResolution / 2);
		var YPosition = YTopLeft + (YResolution / 2);

		var Target = new Vector2(XPostion, YPosition);
		//GD.Print($"Converted Target Position: {Target}");
		return Target;
	}
}
