using Godot;
using System;

public interface Piece
{
	[Export]
	public Texture2D texture {get; set;}
	[Export]
	public String color {get; set;}

	// Called when the node enters the scene tree for the first time.
	public void updateColor(Texture2D texture, String color);
	// Called every frame. 'delta' is the elapsed time since the previous frame.
}
