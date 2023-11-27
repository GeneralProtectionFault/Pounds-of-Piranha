using Godot;
using System;

public partial class Weight : RigidBody2D
{
	[Export] public int Pounds = 0;


	public bool MouseHeld { get; set; } = false;
	private bool MouseOverWeight = false;

	private Vector2 StartingPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// ScaleObject.WeightChanged += ReleaseWeight;

		var TheLabel = GetNode<Label>("Label");
		TheLabel.Text = Pounds.ToString();

		FreezeMode = FreezeModeEnum.Kinematic;
		StartingPosition = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (MouseHeld)
		{
			GlobalPosition = GetGlobalMousePosition();
		}
	}


	public override void _Input(InputEvent @event)
    {
		// Check if mouse is over the weight first
		if (@event is InputEventMouseButton MouseEvent)
		{
			if (MouseEvent.ButtonIndex == MouseButton.Left && MouseEvent.Pressed && MouseOverWeight
			// && Level.CurrentLevelState == Level.LevelState.Play
			)
			{
				GrabWeight(this, 0);
			}
			else if (MouseEvent.IsActionReleased("click"))
			{
				ReleaseWeight(this, 0);
			}
		}
	}


	private void GrabWeight(object sender, int dummy)
	{
		// SetDeferred("MouseHeld", true);
		// SetDeferred("Freeze", true);

		foreach (var Weight in GetTree().GetNodesInGroup("Weights"))
		{
			if (Weight == this)
				continue;
			else
			{
				// If any other weights have the mouse held, don't pick up this weight
				// if ((Weight as Weight).MouseHeld)
				// 	return;

				if ((Weight as RigidBody2D).LinearVelocity.DistanceTo(Vector2.Zero) > .02f)
					return;
			}
		}
		
		MouseHeld = true;
		Freeze = true;
	}

	private void ReleaseWeight(object sender, int dummy)
	{
		// SetDeferred("MouseHeld", false);
		// SetDeferred("Freeze", false);
		MouseHeld = false;
		Freeze = false;
	}



	public void SetMouseInWeight()
	{
		MouseOverWeight = true;
	}

	public void SetMouseOutOfWeight()
	{
		MouseOverWeight = false;
	}
}
