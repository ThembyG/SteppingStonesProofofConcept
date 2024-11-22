using Godot;
using System;

public partial class Tile : Node2D, Piece
{
	[Export]
	public Texture2D texture {get; set;}
	[Export]
	public String color {get; set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetVisibilityLayer(2);
		updateColor(texture, color);
	}
	public void updateColor(Texture2D texture, String color) {
		Sprite2D sprite = (Sprite2D)FindChild("Sprite2D", false, false);
		sprite.SetTexture(texture);
		this.color = color;
		QueueRedraw();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
