using Godot;
using System;

public partial class Weight : RigidBody2D
{
	[Export] public int Pounds = 0;


	private bool MouseHeld = false;
	private bool MouseOverWeight = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// ScaleObject.WeightChanged += ReleaseWeight;

		var TheLabel = GetNode<Label>("Label");
		TheLabel.Text = Pounds.ToString();

		FreezeMode = FreezeModeEnum.Kinematic;
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
			&& Level.CurrentLevelState == Level.LevelState.Play)
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
