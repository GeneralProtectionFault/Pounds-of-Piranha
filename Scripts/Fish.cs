using Godot;
using System;
using System.Linq;



public partial class Fish : Node2D
{
	public enum FishStatus {OnDeck, InPlay}
	public enum FishType {Bad, Good}
	
	[Export] public int RayCastLength = 225;
	[Export] public Vector2I FishFacingDirection = new Vector2I(1,0); // Default to the right
	[Export] public FishStatus Status = FishStatus.OnDeck;
	[Export] public FishType Type = FishType.Good;


	private Area2D FishCollider;
	private bool Moving = false;
	private RayCast2D NumberDetector;

	// Stores the navigation points to the target location
	private Godot.Collections.Array<Vector2I> IDPath = new Godot.Collections.Array<Vector2I>();
	

	public void FishLeavingScene()
	{
		Level.CommenceTurn -= MoveFish;
	}


	private void SetFacingDirection(Vector2I Direction)
	{
		FishFacingDirection = Direction;
		NumberDetector.TargetPosition = FishFacingDirection * RayCastLength;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Level.CommenceTurn += MoveFish;
		// ScaleObject.WeightChanged += MoveFish;

		NumberDetector = GetNode<RayCast2D>("NumberRayCast2D");
		FishCollider = GetNode<Area2D>("Area2D");
		SetFacingDirection(FishFacingDirection);
	}

	private void MoveFish(object sender, EventArgs e)
	{
		// TODO:  Do a raycast in the direction the fish is facing.
		// The raycast should be a chunk more than half the XResolution in order to reach into the next number, but not to the other side of it.
		if (IsInstanceValid(NumberDetector))
		{
			if (NumberDetector.IsColliding())
			{
				GD.Print("Fish cannot move, there is an impassable number in the way.");
			}
			else
			{
				// To determine the desired cell, first get the current one, then treat it like a normalized vector2
				Vector2I CurrentCell = Level.CellFromCoordinates(GlobalPosition);
				Vector2I DestinationCell = CurrentCell + FishFacingDirection;


				// If the fish can move, set the IDPath/destination, do the movement, repeat check, etc...
				// Need to get the cell by index-ey position, not coordinates, because that makes sense
				IDPath = Level.Grid.GetIdPath(CurrentCell, DestinationCell);
				// Removes the current position
				IDPath = IDPath.Slice(1);

			
				GD.Print(IDPath);

				Moving = true;
			}
		}
	}


	private void EatFish(Fish Consumer, Fish Consumee)
	{
		// TODO:  Play an animation FFS
		Consumee.QueueFree();
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _PhysicsProcess(double delta)
    {
		if (Moving)
		{
			// If there are more cells to navigate to...
			if (IDPath.Count > 0)
			{
				// This will be an x, y index, starting at the upper-left, indicating how many cells over & down the destination cell is...
				var TargetPosition = IDPath[0];
				// Ergo...it needs to be converted back to coordinates to move to that "position"
				GlobalPosition = GlobalPosition.MoveToward(Level.CoordinatesFromCell(TargetPosition), 5);

				// This means we've reached the next navigation point
				if (GlobalPosition == Level.CoordinatesFromCell(TargetPosition))
				{
					// Fish consumption check
					var OtherAreas = FishCollider.GetOverlappingAreas();

					if (OtherAreas.Count > 0)
					{
						Fish OtherFish = OtherAreas.FirstOrDefault().GetParent() as Fish;
						if (this.Type != OtherFish.Type)
						{
							Fish Bad;
							Fish Good;

							if (this.Type == FishType.Bad)
							{
								Bad = this;
								Good = OtherFish;
							}
							else
							{
								Bad = OtherFish;
								Good = this;
							}


							EatFish(Bad, Good);
						}
					}


					if (NumberDetector.IsColliding())
						Moving = false;
					
					
					IDPath = IDPath.Slice(1);


					if (!NumberDetector.IsColliding())
						MoveFish(this, EventArgs.Empty);

					// TODO:  Any changing in facing direction should happen here, followed by a check for collision before enabling movement again
					// If no obstructions, set Moving to true again
				}
			}
			// else
			// 	Moving = false;
		}
    }



    public override void _Input(InputEvent @event)
    {
		// ********* For testing navigation by clicking the mouse on the destination ************

		if (@event is InputEventMouseButton MouseEvent)
		{
			if (!MouseEvent.IsActionPressed("click"))
				return;

			// GD.Print($"Fish position: {GlobalPosition}");
			GD.Print($"Mouse clicked at: {MouseEvent.Position}");

			
			// Need to get the cell by index-ey position, not coordinates, because that makes sense
			// IDPath = Level.Grid.GetIdPath(Level.CellFromCoordinates(GlobalPosition), Level.CellFromCoordinates(MouseEvent.Position));
			// Removes the current position
			// IDPath = IDPath.Slice(1);

			

			// GD.Print(IDPath);
		}
    }


	
}
