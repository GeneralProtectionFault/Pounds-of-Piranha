using Godot;
using System;

public partial class Weight : Node2D
{
	[Export] public int Pounds = 0;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var TheLabel = GetNode<Label>("Label");
		TheLabel.Text = Pounds.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
