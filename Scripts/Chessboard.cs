using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
	[Header("Art")]
	[SerializeField] private Material tileMaterial;
	[SerializeField] private float tileSize = 1.0f;
	[SerializeField] private float yOffset = 0.2f;
	[SerializeField] private Vector3 boardCenter = Vector3.zero;
	[SerializeField] private float deathSize = 0.3f;
	[SerializeField] private float deathSpacing = 0.3f;
	[SerializeField] private float dragOffset = 1.25f;

	[Header("Prefabs & Materials")]
	[SerializeField] private GameObject[] prefabs;
	[SerializeField] private int[] team;
	[SerializeField] private Material[] pieceMaterials;

	// LOGIC
	private BasePiece[,] chessPieces;
	private BasePiece currentlyDragging;
	private List<Vector2Int> availableMoves = new List<Vector2Int>();
	private List<BasePiece> deadWhites = new List<BasePiece>();
	private List<BasePiece> deadBlacks = new List<BasePiece>();
	private const int TILE_COUNT_X = 8;
	private const int TILE_COUNT_Y = 8;
	private GameObject[,] tiles;
	private Camera currentCamera;
	private Vector2Int currentHover;
	private Vector3 bounds;

	private void Awake()
	{
		GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
		SpawnAllPieces();
		PositionAllPieces();
	}

	private void Update()
	{
		if (!currentCamera)
		{
			currentCamera = Camera.main;
			return;
		}

		RaycastHit info;
		Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
		{
			// Get Tile Index
			Vector2Int hitPos = LookupTileIndex(info.transform.gameObject);

			// If first hover
			if (currentHover == -Vector2Int.one)
			{
				currentHover = hitPos;
				tiles[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
			}

			// If not first hover
			if (currentHover != hitPos)
			{
				tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
				currentHover = hitPos;
				tiles[hitPos.x, hitPos.y].layer = LayerMask.NameToLayer("Hover");
			}

			// If holding mouse button
			if (Input.GetMouseButtonDown(0))
			{
				if (chessPieces[hitPos.x, hitPos.y] != null)
				{
					if (true)
					{
						currentlyDragging = chessPieces[hitPos.x, hitPos.y];

						// Get available moves list and highlight tiles
						availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
						HighlightTiles();
					}
				}
			}

			// If released mouse button
			if (currentlyDragging != null && Input.GetMouseButtonUp(0))
			{
				Vector2Int previousPos = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
				bool validMove = MoveTo(currentlyDragging, hitPos.x, hitPos.y);

				if (!validMove)
				{
					currentlyDragging.SetPosition(GetTileCenter(previousPos.x, previousPos.y));
				}

				currentlyDragging = null;
				RemoveHighlightTiles();
			}
		}
		else
		{
			if (currentHover != -Vector2Int.one)
			{
				tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
				currentHover = -Vector2Int.one;
			}

			if (currentlyDragging && Input.GetMouseButtonUp(0))
			{
				currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
				currentlyDragging = null;
				RemoveHighlightTiles();
			}
		}

		// Piece dragging smoothing
		if (currentlyDragging)
		{
			Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
			float distance = 0.0f;

			if (horizontalPlane.Raycast(ray, out distance))
			{
				currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
			}
		}
	}

	// Board Generation
	private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
	{
		yOffset += transform.position.y;
		bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;
		tiles = new GameObject[tileCountX, tileCountY];
		for (int x = 0; x < tileCountX; x++)
		{
			for (int y = 0; y < tileCountY; y++)
			{
				tiles[x, y] = GenerateSingleTile(tileSize, x, y);
			}
		}
	}

	private GameObject GenerateSingleTile(float tileSize, int x, int y)
	{
		GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
		tileObject.transform.parent = transform;

		Mesh mesh = new Mesh();
		tileObject.AddComponent<MeshFilter>().mesh = mesh;
		tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

		Vector3[] vertices = new Vector3[4];
		vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
		vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
		vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
		vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

		int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

		mesh.vertices = vertices;
		mesh.triangles = tris;

		mesh.RecalculateNormals();

		tileObject.layer = LayerMask.NameToLayer("Tile");
		tileObject.AddComponent<BoxCollider>();

		return tileObject;
	}

	// Pieces Spawning
	private void SpawnAllPieces()
	{
		chessPieces = new BasePiece[TILE_COUNT_X, TILE_COUNT_Y];


		// White Team
		chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, 0, 2);
		chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, 0, 4);
		chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, 0, 6);
		chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, 0, 8);
		chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, 0, 10);
		chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, 0, 6);
		chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, 0, 4);
		chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, 0, 2);
		for (int i = 0; i < TILE_COUNT_X; i++)
		{
			chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, 0, 0);
		}

		// Black Team
		chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, 1, 3);
		chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, 1, 5);
		chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, 1, 7);
		chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, 1, 9);
		chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, 1, 11);
		chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, 1, 7);
		chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, 1, 5);
		chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, 1, 3);
		for (int i = 0; i < TILE_COUNT_X; i++)
		{
			chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, 1, 1);
		}
	}

	private BasePiece SpawnSinglePiece(ChessPieceType type, int team, int pieceMaterial)
	{
		BasePiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<BasePiece>();

		cp.type = type;
		cp.team = team;
		cp.GetComponent<MeshRenderer>().material = pieceMaterials[pieceMaterial];

		return cp;
	}

	// Positioning
	private void PositionAllPieces()
	{
		for (int x = 0; x < TILE_COUNT_X; x++)
		{
			for (int y = 0; y < TILE_COUNT_Y; y++)
			{
				if (chessPieces[x, y] != null)
				{
					PositionSinglePiece(x, y, true);
				}
			}
		}
	}

	private void PositionSinglePiece(int x, int y, bool force = false)
	{
		chessPieces[x, y].currentX = x;
		chessPieces[x, y].currentY = y;
		chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
	}

	private Vector3 GetTileCenter(int x, int y)
	{
		return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
	}

	private void HighlightTiles()
	{
		for (int i = 0; i < availableMoves.Count; i++)
		{
			tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
		}
	}

	private void RemoveHighlightTiles()
	{
		for (int i = 0; i < availableMoves.Count; i++)
		{
			tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
		}
		availableMoves.Clear();
	}

	// Operations
	private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
	{
		for (int i = 0; i < moves.Count; i++)
		{
			if (moves[i].x == pos.x && moves[i].y == pos.y)
			{
				return true;
			}
		}

		return false;
	}

	private bool MoveTo(BasePiece cp, int x, int y)
	{
		if (!ContainsValidMove(ref availableMoves, new Vector2(x, y)))
		{
			return false;
		}
		
		Vector2Int previousPos = new Vector2Int(cp.currentX, cp.currentY);

		// Is the tile occupied
		if (chessPieces[x, y] != null)
		{
			BasePiece ocp = chessPieces[x, y];

			if (cp.team == ocp.team)
			{
				return false;
			}

			// If enemy team
			if (ocp.team == 0)
			{
				deadWhites.Add(ocp);
				ocp.SetScale(Vector3.one * deathSize);
				ocp.SetPosition
				(
					new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
					- bounds
					+ new Vector3(tileSize / 2, 0, tileSize / 2)
					+ (Vector3.back * deathSpacing) * deadWhites.Count
				);
			}
			else
			{
				deadBlacks.Add(ocp);
				ocp.SetScale(Vector3.one * deathSize);
				ocp.SetPosition
				(
					new Vector3(8 * tileSize, yOffset, -1 * tileSize)
					- bounds
					+ new Vector3(tileSize / 2, 0, tileSize / 2)
					+ (Vector3.forward * deathSpacing) * deadBlacks.Count
				);
			}
		}

		chessPieces[x, y] = cp;
		chessPieces[previousPos.x, previousPos.y] = null;

		PositionSinglePiece(x, y);

		return true;
	}

	private Vector2Int LookupTileIndex(GameObject hitInfo)
	{
		for (int x = 0; x < TILE_COUNT_X; x++)
		{
			for (int y = 0; y < TILE_COUNT_Y; y++)
			{
				if (tiles[x, y] == hitInfo)
				{
					return new Vector2Int(x, y);
				}
			}
		}

		return -Vector2Int.one; // Invalid
	}
}