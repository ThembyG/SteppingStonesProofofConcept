using Godot;
using System;


public partial class Grid : Node2D
{
    [Export] public int gridWidth = 5;  // Default width of the grid
    [Export] public int gridHeight = 5; // Default height of the grid
    [Export] public int cellSize = 64;  // Size of each grid cell in pixels
    [Export] public PackedScene tileScene;
    [Export] public PackedScene scoutScene;
    [Export] public Texture2D redPNG;
    [Export] public Texture2D bluePNG;
    [Export] public Texture2D redTile;
    [Export] public Texture2D blueTile;
	[Export] public String currColor{get; set;}
	[Export] public String currMode{get; set;}
    [Export] public bool multiScout {get; set;}
    private bool redScoutDown; 
    private bool blueScoutDown; 
    private Color gridColor = new Color(0.1f, 0.1f, 0.1f); // Dark grey for the grid lines
    private Color cellColor = new Color(1, 1, 1); // White color for the cell background
    private Vector2 gridOffset; // Offset for centering the grid
    private String[,] gridData; // Array to store the state of each grid cell (0 = empty, 1 = piece)
	private Tile[,] tiles;
    private Scout[,] scouts;
    private Vector2I selectedPieceLoc;
    private Piece selPiece;
    
    public override void _Ready()
    {
        // Initialize the grid data with zeros (empty)
        gridData = new String[gridWidth, gridHeight];
		for (int x = 0; x  < gridWidth; x++) {
			for(int y = 0; y < gridHeight; y++) {
				gridData[x,y] = "null";
			}
		}
        redScoutDown = false;
        blueScoutDown = false;
		tiles = new Tile[gridWidth, gridHeight];
		scouts = new Scout[gridWidth, gridHeight];

        // Calculate the grid offset to center the grid on the screen
        gridOffset = new Vector2((GetViewportRect().Size.X - gridWidth * cellSize) / 2, 
                                 (GetViewportRect().Size.Y - gridHeight * cellSize) / 2 + 40);
        //GD.Print($"Canvas has layer {this.GetVisibilityLayer()}");
        changeLabel();
    }

    // Drawing the grid, background, and placing pieces (sprites)
    public override void _Draw()
    {
        // Fill each grid cell with the background color (white)
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Calculate the position of the cell
                Rect2 cellRect = new Rect2(gridOffset + new Vector2(x * cellSize, y * cellSize), new Vector2(cellSize, cellSize));
                // Draw a white background for each cell
                DrawRect(cellRect, cellColor);
            }
        }

        // Draw grid lines over the background
        for (int i = 0; i <= gridWidth; i++)
        {
            Vector2 start = gridOffset + new Vector2(i * cellSize, 0);
            Vector2 end = gridOffset + new Vector2(i * cellSize, gridHeight * cellSize);
            DrawLine(start, end, gridColor);
        }

        for (int i = 0; i <= gridHeight; i++)
        {
            Vector2 start = gridOffset + new Vector2(0, i * cellSize);
            Vector2 end = gridOffset + new Vector2(gridWidth * cellSize, i * cellSize);
            DrawLine(start, end, gridColor);
        }

        // Place the pieces (sprites) in the grid based on the grid data

    }

	private void OnButtonPressed() {
        GD.Print("Button pressed");
		Button b = GetNode<Button>("Control/Button");
		if (currMode == "Movement") {
			currMode = "Placement";
		} else {
			currMode = "Movement";
		}
		b.Text = "Current Phase is: " + currMode;
	}

    private void OnSelectButtonPressed() {
        int x = selectedPieceLoc.X; int y = selectedPieceLoc.Y;
        if (selPiece != null) {
            if(scouts[selectedPieceLoc.X, selectedPieceLoc.Y] != null) {
                String type = (selPiece is Scout) ? "tile" : "scout";
                selPiece = (selPiece is Scout) ? tiles[x,y] : scouts[x,y];
                Label label = GetNode<Label>("Control/SelectedLabel");
                label.Text = type + " selected at: " + "("+ x + " ," +  y + ")";
            }
        }
    }

    private void OnResetButtonPressed() {
        resetSelectedPiece();

        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j< gridHeight; j++) {
                if (scouts[i,j] != null) {
                    Node2D temp = scouts[i,j];
                    scouts[i,j] = null;
                    temp.QueueFree();
                }
                if (tiles[i,j] != null) {
                    Node2D temp = tiles[i,j];
                    tiles[i,j] = null;
                    temp.QueueFree();
                }
            }
        }
        currMode = "Placement";
        currColor = "blue";
        Button b = GetNode<Button>("Control/Button");
        b.Text = "Current Phase is: " + currMode;
        Label l = GetNode<Label>("Control/Label");
        l.Text = "Current Turn is: " + currColor;
    }
    // Handle mouse input (click to toggle pieces)
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Vector2 mousePos = GetLocalMousePosition();
            OnGridClick(mousePos);
        }
    }

    // Process the mouse click to toggle the state of the clicked cell
    private void OnGridClick(Vector2 position)
    {
        // Convert the mouse position to grid coordinates
        int cellX = Mathf.FloorToInt((position.X - gridOffset.X) / cellSize);
        int cellY = Mathf.FloorToInt((position.Y - gridOffset.Y) / cellSize);
		//GD.Print($"Position is: ({position.X}, {position.Y})");
		//GD.Print($"offset is: ({gridOffset.X}, {gridOffset.Y})");
		GD.Print($"cell is: ({cellX}, {cellY})");
        if (cellX >= 0 && cellX < gridWidth && cellY >= 0 && cellY < gridHeight)
        {
          //  GD.Print($"Clicked on cell: ({cellX}, {cellY})");
            if (currMode == "Placement") {
                placePiece(cellX, cellY);
            } else {
                movePiece(cellX, cellY);
            }

        } else {
            if (currMode == "Movement" && selPiece != null) {
                movePiece(cellX, cellY);
            }
        }
    }
    public void changeLabel() {
        Label colorLabel = GetNode<Label>("Control/Label");
        colorLabel.Text = "Current Turn is: " + currColor;
    }
    public Vector3I getRelDirection(int cellX, int cellY) {
        int xDiff = selectedPieceLoc.X - cellX;
        int yDiff = selectedPieceLoc.Y - cellY;
        Vector3I v = new Vector3I(xDiff, yDiff, 1);
        if(Mathf.Abs(xDiff) + Mathf.Abs(yDiff) != 1) {
            v.Z = -1;
            GD.Print($"x diff: {xDiff}, y diff: {yDiff}");
            return v;
        }
        return v;
    }
    public void movePiece(int cellX, int cellY) {
        GD.Print("in move piece");
        if (selPiece == null) {
            GD.Print("in null");
            if (tiles[cellX, cellY] != null) {
                String type = (scouts[cellX, cellY] != null) ? "scout" : " tile";
                selPiece = (scouts[cellX, cellY] != null) ? scouts[cellX, cellY] : tiles[cellX, cellY];
                selectedPieceLoc = new Vector2I(cellX, cellY);
                Label label = GetNode<Label>("Control/SelectedLabel");
                label.Text = type + " selected at: " + "("+ cellX + " ," +  cellY + ")";

            }
            return;
        }
        Vector3I dir = getRelDirection(cellX, cellY);
        if (dir.Z == -1) {
            GD.Print("Bad dir");
            return;
        } 
        if (cellX >= gridWidth || cellX < 0 || cellY >= gridHeight || cellY < 0) {
            if (selPiece is Scout) {
                scouts[selectedPieceLoc.X, selectedPieceLoc.Y] = null;
            } else {
                tiles[selectedPieceLoc.X, selectedPieceLoc.Y] = null;
                if (scouts[selectedPieceLoc.X, selectedPieceLoc.Y] != null) {
                    Node2D other = scouts[selectedPieceLoc.X, selectedPieceLoc.Y];
                    scouts[selectedPieceLoc.X, selectedPieceLoc.Y] = null;
                    other.QueueFree();
                }
            }
            Node2D temp = ((Node2D)selPiece);
            temp.QueueFree();
            resetSelectedPiece();
            return;
        }
        if (scouts[cellX, cellY] == null && tiles[cellX, cellY] != null  && selPiece is Scout) {
            if (tiles[cellX, cellY].color == selPiece.color) {
                Scout temp = (Scout)selPiece;
                scouts[cellX, cellY] = temp;
                scouts[selectedPieceLoc.X, selectedPieceLoc.Y] = null;
                temp.Position = gridOffset + new Vector2(cellX * cellSize + cellSize / 2, cellY * cellSize + cellSize / 2 - 10);
                resetSelectedPiece();
                return;
            }
        }
        if (tiles[cellX, cellY] == null && selPiece is Tile) {
            Tile temp = (Tile)selPiece;
            GD.Print($"Hopefully moving tile from ({selectedPieceLoc.X}, {selectedPieceLoc.Y}) to ({cellX}, {cellY})");
            tiles[cellX, cellY] = temp;
            tiles[selectedPieceLoc.X, selectedPieceLoc.Y] = null;
            temp.Position = gridOffset + new Vector2(cellX * cellSize + cellSize / 2, cellY * cellSize + cellSize / 2);
            if (scouts[selectedPieceLoc.X, selectedPieceLoc.Y] != null) {
                Scout t = scouts[selectedPieceLoc.X, selectedPieceLoc.Y];
                scouts[cellX, cellY] = t;
                scouts[selectedPieceLoc.X, selectedPieceLoc.Y] = null;
                t.Position = gridOffset + new Vector2(cellX * cellSize + cellSize / 2, cellY * cellSize + cellSize / 2 - 10);
            }
            resetSelectedPiece();
        }
    }
    private void resetSelectedPiece() {
        selPiece = null;
        selectedPieceLoc = new Vector2I(-1, -1);
        Label label = GetNode<Label>("Control/SelectedLabel");
        label.Text = "No piece selected";
        return;
    }
    public void placePiece(int cellX, int cellY) {
        if (tiles[cellX, cellY] == null) // If This cell is empty
        {
            // Instantiate a new sprite and position it at the center of the cell
            Tile tile = tileScene.Instantiate<Tile>();
            tile.SetVisibilityLayer(2);
            //GD.Print($"Tile has layer {tile.GetVisibilityLayer()}");
            if (currColor == "red") {
                tile.texture = redTile;
                tile.updateColor(redTile, currColor);
                currColor = "blue";
            } else {
                currColor = "red";
            }
            changeLabel();
            tile.Position = gridOffset + new Vector2(cellX * cellSize + cellSize / 2, cellY * cellSize + cellSize / 2);
            tile.ApplyScale(new Vector2(2,2));
            tiles[cellX, cellY] = tile;
            AddChild(tile); // Add the sprite to the scene
        } else {
            if (scouts[cellX, cellY] == null) {
                if(currColor != tiles[cellX, cellY].color) {
                    return;
                }
                Scout scout = scoutScene.Instantiate<Scout>();
                scout.SetVisibilityLayer(3);
                if(currColor == "red") {
                    scout.texture = redPNG;
                    scout.color = currColor;
                    scout.updateColor(redPNG, currColor); //don't know if both are needed but no time to check
                    currColor = "blue";
                } else {
                    currColor = "red";
                }
                changeLabel();
                scout.Position = gridOffset + new Vector2(cellX * cellSize + cellSize / 2, cellY * cellSize + cellSize / 2 - 10);
                scout.ApplyScale(new Vector2(2, 2));
                scouts[cellX, cellY] = scout;
                AddChild(scout);
            } 
            
            }
            // GD.Print("in null");
            // Tile temp = tiles[cellX, cellY];
            // tiles[cellX, cellY] = null;
            // temp.QueueFree();
            }
    }

                // // Toggle the cell state (0 = empty, 1 = piece)
				// if (tiles[cellX, cellY] == null) // If This cell is empty
                // {
                //     // Instantiate a new sprite and position it at the center of the cell
                //     Tile tile = tileScene.Instantiate<Tile>();
                //     tile.SetVisibilityLayer(2);
				// 	//GD.Print($"Tile has layer {tile.GetVisibilityLayer()}");
                //     if (currColor == "red") {
				// 		tile.color = redTile;
                //         tile.updateColor(redTile);
                //         currColor = "blue";
                //     } else {
                //         currColor = "red";
                //     }
                //     changeLabel();
                //     tile.Position = gridOffset + new Vector2(cellX * cellSize + cellSize / 2, cellY * cellSize + cellSize / 2);
                //     tile.ApplyScale(new Vector2(2,2));
                //     tiles[cellX, cellY] = tile;
				// 	AddChild(tile); // Add the sprite to the scene
                // } else {
                //     if (currMode == "Placement") {
                //         if(scouts[cellX, cellY] == null) {
                //             Scout scout = scoutScene.Instantiate<Scout>();
                //             scout.SetVisibilityLayer(3);
                //             if(currColor == "red") {
                //                 scout.color = redPNG;
                //                 scout.updateColor(redPNG);
                //                 currColor = "blue";
                //             } else {
                //                 currColor = "red";
                //             }
                //             changeLabel();
                //             scout.Position = gridOffset + new Vector2(cellX * cellSize + cellSize / 2, cellY * cellSize + cellSize / 2);
                //             scout.ApplyScale(new Vector2(3, 3));
                //             scouts[cellX, cellY] = scout;
                //         } 
                        
                //     }
                //     GD.Print("in null");
                //     Tile temp = tiles[cellX, cellY];
                //     tiles[cellX, cellY] = null;
                //     temp.QueueFree();
				// 	}
				// // }

 