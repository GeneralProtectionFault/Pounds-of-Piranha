using Godot;
using System;
using System.Collections.Generic;

public partial class Turner : Node2D
{
	private Vector2 InitialRightVector = new Vector2(1,0);
	[Export] public Vector2 ArrowDirection;
	[Export] public Fish.FishType AffectedFishType { get; set; }

	private Sprite2D ArrowSprite;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ArrowSprite = GetNode<Sprite2D>("Sprite2D");
		RotateSpriteToDireciton();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ArrowCollided(Area2D OtherArea)
	{
		Fish CollidingFish = (Fish)OtherArea.GetParent();
		
		if (CollidingFish.Type == AffectedFishType)
		{		
			CollidingFish.SetFacingDirection((Vector2I)ArrowDirection);
			CollidingFish.Status = Fish.FishStatus.Turning;
		}
	}



	private void RotateSpriteToDireciton()
	{
		var DotProduct = InitialRightVector.Dot(ArrowDirection);
		var CosineTheta = DotProduct / (InitialRightVector.Length() * ArrowDirection.Length());
		var Theta = Mathf.Acos(CosineTheta);
		Theta *= Mathf.Sign(ArrowDirection.Y);

		if (Mathf.Sign(ArrowDirection.X) == -1 && Theta == 0)
			Theta = 3.141593f;

		GD.Print($"Angle: {Theta}");
		ArrowSprite.Rotate(Theta);
	}

}
