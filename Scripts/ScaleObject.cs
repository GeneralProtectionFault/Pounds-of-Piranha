using Godot;
using System;

public partial class ScaleObject : RigidBody2D
{
	public static event EventHandler<int> WeightChanged;



	public static int Pounds = 0;
	private static Area2D WeightArea;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		WeightArea = GetNode<Area2D>("Weight_Area2D");
		WeightArea.BodyEntered += AddWeight;
		WeightArea.BodyExited += RemoveWeight;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void AddWeight(Node2D EnteringBody)
	{
		if (EnteringBody.IsInGroup("Weights"))
		{
			var Amount = Convert.ToInt32((EnteringBody as Weight).Pounds);
			Pounds += Amount;
			GD.Print($"Adding Weight: {Amount}");

			WeightChanged?.Invoke(this, Pounds);
		}
	}


	private void RemoveWeight(Node2D LeavingBody)
	{
		if (LeavingBody.IsInGroup("Weights"))
		{
			var Amount = Convert.ToInt32((LeavingBody as Weight).Pounds);
			Pounds -= Amount;
			GD.Print($"Removing Weight: {Amount}");

			WeightChanged?.Invoke(this, Pounds);
		}
	}




}
