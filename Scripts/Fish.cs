using Godot;
using System;
using System.Linq;



public partial class Fish : AnimatedSprite2D
{
	public enum FishStatus {OnDeck, InPlay, Turning}
	public enum FishType {Bad, Good}
	
	[Export] public int RayCastLength = 225;
	[Export] public Vector2I FishFacingDirection = new Vector2I(1,0); // Default to the right
	[Export] public FishStatus Status = FishStatus.InPlay;
	[Export] public FishType Type = FishType.Good;


	private Area2D FishCollider;
	public bool Moving { get; set; } = false;
	private RayCast2D NumberDetector;

	// Stores the navigation points to the target location
	private Godot.Collections.Array<Vector2I> IDPath = new Godot.Collections.Array<Vector2I>();
	public Vector2I StartingGridCell { get; set; }
	public bool ReachedGoal = false;
	public bool PendingConsumption { get; set; } = false;

	public void FishLeavingScene()
	{
		Level.CommenceTurn -= MoveFish;
	}


	public void SetFacingDirection(Vector2I Direction)
	{
		FishFacingDirection = Direction;
		NumberDetector.TargetPosition = FishFacingDirection * RayCastLength;

		SetFishAnimation(Direction);
	}


	private void SetFishAnimation(Vector2I Direction)
	{
		string AnimationName_Idle = Direction switch
        {
            { X: -1 } and { Y: 0 } => "Idle_Left",
            { X: 1 } and { Y: 0 }  => "Idle_Right",
            { X: 0 } and { Y: 1 } => "Idle_Down",
            { X: 0 } and { Y: -1} => "Idle_Up",
            _ => "None"
        };

		string AnimationName_Moving = Direction switch
        {
            { X: -1 } and { Y: 0 } => "Swim_Left",
            { X: 1 } and { Y: 0 }  => "Swim_Right",
            { X: 0 } and { Y: 1 } => "Swim_Down",
            { X: 0 } and { Y: -1} => "Swim_Up",
            _ => "None"
        };

		if (Moving || ReachedGoal)
		{
			// GD.Print($"{Type} = Settomg swim animation");
			this.Stop();
			this.Play(AnimationName_Moving);
		}
		else
		{
			// GD.Print($"{Type} = Settomg idle animation");
			this.Stop();
			this.Play(AnimationName_Idle);
		}
	}



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Level.CommenceTurn += MoveFish;
		// ScaleObject.WeightChanged += MoveFish;

		NumberDetector = GetNode<RayCast2D>("NumberRayCast2D");
		FishCollider = GetNode<Area2D>("Area2D");
		SetFacingDirection(FishFacingDirection);

		StartingGridCell = Level.CellFromCoordinates(GlobalPosition);
	}


	private void MoveFish(object sender, EventArgs e)
	{
		// Resume from turning status here, only after the event to move fires off again.
		// This prevents the fish from moving immediately after changing direciton all in 1 turn
		if (Status == FishStatus.Turning)
			Status = FishStatus.InPlay;

		// TODO:  Do a raycast in the direction the fish is facing.
		// The raycast should be a chunk more than half the XResolution in order to reach into the next number, but not to the other side of it.
		if (IsInstanceValid(NumberDetector))
		{
			if (NumberDetector.IsColliding())
			{
				// GD.Print("Fish cannot move, there is an impassable number in the way.");
			}
			else if (Status == FishStatus.InPlay)
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
				
				//Level.CurrentLevelState = Level.LevelState.FishMoving;
			}
		}
	}


	private async void EatFish(Fish Consumer, Fish Consumee)
	{
		Consumee.PendingConsumption = true;
	
		if (Mathf.Sign(Consumer.FishFacingDirection.X) == -1 || Mathf.Sign(Consumer.FishFacingDirection.Y) == -1)
		{
			Consumer.Play("Eat_Left");
		}
		else if (Mathf.Sign(Consumer.FishFacingDirection.X) == 1 || Mathf.Sign(Consumer.FishFacingDirection.Y) == 1)
		{
			Consumer.Play("Eat_Right");
			
		}

		GD.Print("Awaiting loop..");
		await ToSignal(Consumer, "animation_looped");
		GD.Print("Consuming fish and should be setting idle animation...");
		Consumee.QueueFree();
		SetFacingDirection(FishFacingDirection);
		//Level.CurrentLevelState = Level.LevelState.Play;


		// A perfectly good fish has been consumed!  RESTART!
		Level.AteFishSound.Play();

		Tween AnotherDelayTween = GetTree().CreateTween();
		AnotherDelayTween.TweenCallback(Callable.From(() => {
				Level.LevelTemplateObject.RestartLevel();
			}
        )).SetDelay(1f);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _PhysicsProcess(double delta)
    {
		if (ReachedGoal)
		{
			GlobalPosition = GlobalPosition.MoveToward(GlobalPosition + FishFacingDirection * 225, 5);
		}

		// Fish consumption check
		if (Type == FishType.Bad)
		{
			var OtherAreas = FishCollider.GetOverlappingAreas();
			var GoodAreas = OtherAreas.Where(x => x.IsInGroup("GoodFishAreas"));

			if (GoodAreas is not null)
			{
				foreach(Area2D FishArea in GoodAreas)
				{
					if (FishArea is not null)
					{
						// Node OtherThing = OtherAreas.FirstOrDefault().GetParent();
						var OtherFish = FishArea.GetParent() as Fish;

						if (this.Type != OtherFish.Type && !OtherFish.PendingConsumption)
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
				}
			}
		}


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
				if (GlobalPosition.DistanceTo(Level.CoordinatesFromCell(TargetPosition)) < .1)
				{
					if (NumberDetector.IsColliding() || PendingConsumption)
					{
						Moving = false;
						SetFacingDirection(FishFacingDirection);

						if (//Level.CurrentLevelState == Level.LevelState.FishMoving && 
						!CheckMovingFish())
							Level.CurrentLevelState = Level.LevelState.Play;
					}
					
					
					IDPath = IDPath.Slice(1);


					if (!NumberDetector.IsColliding() && Status == FishStatus.InPlay && !PendingConsumption)
					{
						MoveFish(this, EventArgs.Empty);

						// TODO:  Any changing in facing direction should happen here, followed by a check for collision before enabling movement again
						// If no obstructions, set Moving to true again
					}
				}
			}
			else
			{
				Moving = false;
				SetFacingDirection(FishFacingDirection);

				var CurrentCell = Level.CellFromCoordinates(GlobalPosition);

				if (CurrentCell != StartingGridCell && Level.IsEdgeCell(Level.CellFromCoordinates(GlobalPosition)))
				{
					ReachedGoal = true;
					SetFacingDirection(FishFacingDirection);
				}

				if (//Level.CurrentLevelState == Level.LevelState.FishMoving && 
				!CheckMovingFish())
					Level.CurrentLevelState = Level.LevelState.Play;
			}
			
		}
    }



	public bool CheckMovingFish()
	{
		bool FishStillMoving = false;

		// 	Moving = false;
		foreach (var BadFish in GetTree().GetNodesInGroup("BadFish"))
		{
			if ((BadFish as Fish).Moving)
				FishStillMoving = true;
		}

		foreach(var GoodFish in GetTree().GetNodesInGroup("GoodFish"))
		{
			if ((GoodFish as Fish).Moving)
				FishStillMoving = true;
		}

		// if (FishStillMoving)
		// 	GD.Print("FISH STILL MOVING DAMMIT!");

		return FishStillMoving;
	}



    public override void _Input(InputEvent @event)
    {
		// ********* For testing navigation by clicking the mouse on the destination ************

		if (@event is InputEventMouseButton MouseEvent)
		{
			if (!MouseEvent.IsActionPressed("click"))
				return;

			// GD.Print($"Fish position: {GlobalPosition}");
			// GD.Print($"Mouse clicked at: {MouseEvent.Position}");

			
			// Need to get the cell by index-ey position, not coordinates, because that makes sense
			// IDPath = Level.Grid.GetIdPath(Level.CellFromCoordinates(GlobalPosition), Level.CellFromCoordinates(MouseEvent.Position));
			// Removes the current position
			// IDPath = IDPath.Slice(1);

			

			// GD.Print(IDPath);
		}
    }



	public void ExitedScreen()
	{
		GD.Print("Fish exited screen!");
		if (//Level.CurrentLevelState == Level.LevelState.FishMoving && 
		!CheckMovingFish())
			Level.CurrentLevelState = Level.LevelState.Play;
		

		// Check if there are any more good fish
		var GoodFish = GetTree().GetNodesInGroup("GoodFish");
		var GoodFishCount = 0;

		foreach(var Fish in GoodFish)
		{
			if (Fish == this)
				continue;
			
			GoodFishCount += 1;
		}

		// If there are no other good fish, go to the next level!
		if (GoodFishCount == 0)
		{
			var CurrentLevelSequence = (int)Level.LevelObject.GetMeta("Sequence");
			var CurrentLevelPar = (int)Level.LevelObject.GetMeta("Par");
			GD.Print($"SEQUENCE OF THIS LEVEL: {CurrentLevelSequence}");
			GD.Print($"PAR OF THIS LEVEL: {CurrentLevelPar}");

			// Load Next Level
			Level.LevelTemplateObject.ResetLevelVariables();
			GetTree().ChangeSceneToFile(Manager.LevelDictionary[CurrentLevelSequence + 1]);
		}

		QueueFree();
	}
	
}
