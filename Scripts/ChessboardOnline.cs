using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class ChessboardOnline : MonoBehaviour
{
	#region Variables

	#region Art

	[Header("Art")]
	[SerializeField] private Material tileMaterial;
	[SerializeField] private float tileSize = 1.0f;
	[SerializeField] private float yOffset = 0.2f;
	[SerializeField] private Vector3 boardCenter = Vector3.zero;
	[SerializeField] private float deathSize = 0.3f;
	[SerializeField] private float deathSpacing = 0.3f;
	[SerializeField] private float dragOffset = 1.25f;
	[SerializeField] private GameObject victoryScreen;
	[SerializeField] private GameObject promotionScreen;
	[SerializeField] private Transform rematchIndicator;
	[SerializeField] private Button rematchButton;

	[Header("Prefabs & Materials")]
	[SerializeField] private GameObject[] prefabs;
	[SerializeField] private int[] team;
	[SerializeField] private Material[] pieceMaterials;
	[SerializeField] private Mesh[] meshes;

	#endregion

	#region Logic

	private BasePiece[,] chessPieces;
	private BasePiece currentlyDragging;
	private List<Vector2Int> availableMoves = new List<Vector2Int>();
	private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
	private List<BasePiece> deadWhites = new List<BasePiece>();
	private List<BasePiece> deadBlacks = new List<BasePiece>();
	private const int TILE_COUNT_X = 8;
	private const int TILE_COUNT_Y = 8;
	private GameObject[,] tiles;
	private Camera currentCamera;
	private Vector2Int currentHover;
	private Vector3 bounds;
	private SpecialMove specialMove;
	private bool isWhiteTurn;

	#endregion

	#region Multi Logic

	PhotonView view;

	#endregion

	#endregion

	#region Start & Update

	private void Start()
	{
		isWhiteTurn = true;

		view = GetComponent<PhotonView>();
		GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
		SpawnAllPieces();
		PositionAllPieces();
		
		foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
		{
			if (player.Value == PhotonNetwork.LocalPlayer)
			{
				if ((int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] == 0)
				{
					ApplyLocalChanges(0);
				}
				else if ((int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] == 1)
				{
					ApplyLocalChanges(1);
				}
			}
		}
	}

	public void ApplyLocalChanges(int team)
	{
		if (team == 0)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
		}
		
		if (team == 1)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.blackTeam);
		}
		
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
					// Turns Mechanic
					if ((chessPieces[hitPos.x, hitPos.y].team == 0 && isWhiteTurn) || (chessPieces[hitPos.x, hitPos.y].team == 1 && !isWhiteTurn))
					{
						currentlyDragging = chessPieces[hitPos.x, hitPos.y];

						// Get available moves list and highlight tiles
						availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

						// Get special moves
						specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);

						PreventCheck();
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

					BasePiece cp = chessPieces[hitPos.x, hitPos.y];

					#region Notation

					#region Pawn

					// if (cp.type == ChessPieceType.Pawn)
					// {
					// 	#region a Notation
					// 	if (hitPos.x == 0 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("a1");
					// 	}
					// 	if (hitPos.x == 0 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("a2");
					// 	}
					// 	if (hitPos.x == 0 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("a3");
					// 	}
					// 	if (hitPos.x == 0 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("a4");
					// 	}
					// 	if (hitPos.x == 0 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("a5");
					// 	}
					// 	if (hitPos.x == 0 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("a6");
					// 	}
					// 	if (hitPos.x == 0 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("a7");
					// 	}
					// 	if (hitPos.x == 0 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("a8");
					// 	}
					// 	#endregion

					// 	#region b Notation
					// 	if (hitPos.x == 1 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("b1");
					// 	}
					// 	if (hitPos.x == 1 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("b2");
					// 	}
					// 	if (hitPos.x == 1 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("b3");
					// 	}
					// 	if (hitPos.x == 1 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("b4");
					// 	}
					// 	if (hitPos.x == 1 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("b5");
					// 	}
					// 	if (hitPos.x == 1 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("b6");
					// 	}
					// 	if (hitPos.x == 1 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("b7");
					// 	}
					// 	if (hitPos.x == 1 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("b8");
					// 	}
					// 	#endregion

					// 	#region c Notation
					// 	if (hitPos.x == 2 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("c1");
					// 	}
					// 	if (hitPos.x == 2 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("c2");
					// 	}
					// 	if (hitPos.x == 2 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("c3");
					// 	}
					// 	if (hitPos.x == 2 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("c4");
					// 	}
					// 	if (hitPos.x == 2 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("c5");
					// 	}
					// 	if (hitPos.x == 2 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("c6");
					// 	}
					// 	if (hitPos.x == 2 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("c7");
					// 	}
					// 	if (hitPos.x == 2 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("c8");
					// 	}
					// 	#endregion

					// 	#region d Notation
					// 	if (hitPos.x == 3 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("d1");
					// 	}
					// 	if (hitPos.x == 3 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("d2");
					// 	}
					// 	if (hitPos.x == 3 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("d3");
					// 	}
					// 	if (hitPos.x == 3 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("d4");
					// 	}
					// 	if (hitPos.x == 3 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("d5");
					// 	}
					// 	if (hitPos.x == 3 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("d6");
					// 	}
					// 	if (hitPos.x == 3 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("d7");
					// 	}
					// 	if (hitPos.x == 3 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("d8");
					// 	}
					// 	#endregion

					// 	#region e Notation
					// 	if (hitPos.x == 4 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("e1");
					// 	}
					// 	if (hitPos.x == 4 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("e2");
					// 	}
					// 	if (hitPos.x == 4 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("e3");
					// 	}
					// 	if (hitPos.x == 4 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("e4");
					// 	}
					// 	if (hitPos.x == 4 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("e5");
					// 	}
					// 	if (hitPos.x == 4 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("e6");
					// 	}
					// 	if (hitPos.x == 4 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("e7");
					// 	}
					// 	if (hitPos.x == 4 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("e8");
					// 	}
					// 	#endregion

					// 	#region f Notation
					// 	if (hitPos.x == 5 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("f1");
					// 	}
					// 	if (hitPos.x == 5 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("f2");
					// 	}
					// 	if (hitPos.x == 5 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("f3");
					// 	}
					// 	if (hitPos.x == 5 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("f4");
					// 	}
					// 	if (hitPos.x == 5 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("f5");
					// 	}
					// 	if (hitPos.x == 5 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("f6");
					// 	}
					// 	if (hitPos.x == 5 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("f7");
					// 	}
					// 	if (hitPos.x == 5 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("f8");
					// 	}
					// 	#endregion

					// 	#region g Notation
					// 	if (hitPos.x == 6 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("g1");
					// 	}
					// 	if (hitPos.x == 6 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("g2");
					// 	}
					// 	if (hitPos.x == 6 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("g3");
					// 	}
					// 	if (hitPos.x == 6 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("g4");
					// 	}
					// 	if (hitPos.x == 6 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("g5");
					// 	}
					// 	if (hitPos.x == 6 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("g6");
					// 	}
					// 	if (hitPos.x == 6 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("g7");
					// 	}
					// 	if (hitPos.x == 6 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("g8");
					// 	}
					// 	#endregion

					// 	#region h Notation
					// 	if (hitPos.x == 7 && hitPos.y == 0)
					// 	{
					// 		Debug.Log("h1");
					// 	}
					// 	if (hitPos.x == 7 && hitPos.y == 1)
					// 	{
					// 		Debug.Log("h2");
					// 	}
					// 	if (hitPos.x == 7 && hitPos.y == 2)
					// 	{
					// 		Debug.Log("h3");
					// 	}
					// 	if (hitPos.x == 7 && hitPos.y == 3)
					// 	{
					// 		Debug.Log("h4");
					// 	}
					// 	if (hitPos.x == 7 && hitPos.y == 4)
					// 	{
					// 		Debug.Log("h5");
					// 	}
					// 	if (hitPos.x == 7 && hitPos.y == 5)
					// 	{
					// 		Debug.Log("h6");
					// 	}
					// 	if (hitPos.x == 7 && hitPos.y == 6)
					// 	{
					// 		Debug.Log("h7");
					// 	}
					// 	if (hitPos.x == 7 && hitPos.y == 7)
					// 	{
					// 		Debug.Log("h8");
					// 	}
					// 	#endregion
					// }

					#endregion

					#endregion
				}
				currentlyDragging = null;
				RemoveHighlightTiles();
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
	}

	#endregion

	#region Board Generation

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

	#endregion

	#region Piece Spawning

	private void SpawnAllPieces()
	{
		chessPieces = new BasePiece[TILE_COUNT_X, TILE_COUNT_Y];

		// White Team
		chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, 0, 0, 2);
		chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, 0, 0, 4);
		chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, 0, 0, 6);
		chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, 0, 0, 8);
		chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, 0, 0, 10);
		chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, 0, 0, 2);
		chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, 0, 0, 4);
		chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, 0, 0, 6);
		for (int i = 0; i < TILE_COUNT_X; i++)
		{
			chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, 0, 0, 0);
		}

		// Black Team
		chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, 1, 1, 3);
		chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, 1, 1, 5);
		chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, 1, 1, 7);
		chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, 1, 1, 9);
		chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, 1, 1, 11);
		chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, 1, 1, 3);
		chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, 1, 1, 5);
		chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, 1, 1, 7);
		for (int i = 0; i < TILE_COUNT_X; i++)
		{
			chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, 1, 1, 1);
		}
	}

	private BasePiece SpawnSinglePiece(ChessPieceType type, int team, int pieceMaterial, int mesh)
	{
		BasePiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<BasePiece>();

		cp.type = type;
		cp.team = team;
		cp.GetComponent<MeshRenderer>().material = pieceMaterials[pieceMaterial];
		cp.GetComponent<MeshFilter>().mesh = meshes[mesh];
		return cp;
	}

	#endregion

	#region Positioning

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

	#endregion

	#region Tile Highlighting

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

	#endregion

	#region Check & CheckMate

	private void CheckMate(int team)
	{
		DisplayVictory(team);
	}

	private void DisplayVictory(int winningTeam)
	{
		victoryScreen.SetActive(true);
		victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
	}

	private void PreventCheck()
	{
		BasePiece targetKing = null;
		for (int x = 0; x < TILE_COUNT_X; x++)
		{
			for (int y = 0; y < TILE_COUNT_Y; y++)
			{
				if (chessPieces[x, y] != null)
				{
					if (chessPieces[x, y].type == ChessPieceType.King)
					{
						if (chessPieces[x, y].team == currentlyDragging.team)
						{
							targetKing = chessPieces[x, y];
						}
					}
				}
			}
		}

		// Deleting Moves that put us in check
		SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
	}

	private void SimulateMoveForSinglePiece(BasePiece cp, ref List<Vector2Int> moves, BasePiece targetKing)
	{
		// Save current values
		int actualX = cp.currentX;
		int actualY = cp.currentY;
		List<Vector2Int> movesToRemove = new List<Vector2Int>();

		// Simulate all moves to check if we're in check
		for (int i = 0; i < moves.Count; i++)
		{
			int simX = moves[i].x;
			int simY = moves[i].y;

			Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);

			// Did we simulate king's move
			if (cp.type == ChessPieceType.King)
			{
				kingPositionThisSim = new Vector2Int(simX, simY);
			}

			// Copy [,] but not the ref
			BasePiece[,] simulation = new BasePiece[TILE_COUNT_X, TILE_COUNT_Y];
			List<BasePiece> simAttackingPieces = new List<BasePiece>();

			for (int x = 0; x < TILE_COUNT_X; x++)
			{
				for (int y = 0; y < TILE_COUNT_Y; y++)
				{
					if (chessPieces[x, y] != null)
					{
						simulation[x, y] = chessPieces[x, y];

						if (simulation[x, y].team != cp.team)
						{
							simAttackingPieces.Add(simulation[x, y]);
						}
					}
				}
			}

			// Simulate move
			simulation[actualX, actualY] = null;
			cp.currentX = simX;
			cp.currentY = simY;
			simulation[simX, simY] = cp;

			// Did piece die during simulation
			var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
			if (deadPiece != null)
			{
				simAttackingPieces.Remove(deadPiece);
			}

			// Get all simulated attacking pieces moves
			List<Vector2Int> simMoves = new List<Vector2Int>();
			for (int a = 0; a < simAttackingPieces.Count; a++)
			{
				var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
				for (int b = 0; b < pieceMoves.Count; b++)
				{
					simMoves.Add(pieceMoves[b]);
				}
			}

			// If king in danger remove move
			if (ContainsValidMove(ref simMoves, kingPositionThisSim))
			{
				movesToRemove.Add(moves[i]);
			}

			// Restore actual cp data
			cp.currentX = actualX;
			cp.currentY = actualY;
		}

		// Remove danger moves
		for (int i = 0; i < movesToRemove.Count; i++)
		{
			moves.Remove(movesToRemove[i]);
		}
	}

	private bool CheckForCheckmate()
	{
		var lastMove = moveList[moveList.Count - 1];
		int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

		List<BasePiece> attackingPiece = new List<BasePiece>();
		List<BasePiece> defendingPiece = new List<BasePiece>();

		BasePiece targetKing = null;
		for (int x = 0; x < TILE_COUNT_X; x++)
		{
			for (int y = 0; y < TILE_COUNT_Y; y++)
			{
				if (chessPieces[x, y] != null)
				{
					if (chessPieces[x, y].team == targetTeam)
					{
						defendingPiece.Add(chessPieces[x, y]);

						if (chessPieces[x, y].type == ChessPieceType.King)
						{
							targetKing = chessPieces[x, y];
						}
					}
					else
					{
						attackingPiece.Add(chessPieces[x, y]);
					}
				}
			}
		}

		// Is king being attacked
		List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
		for (int i = 0; i < attackingPiece.Count; i++)
		{
			var pieceMoves = attackingPiece[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
			for (int b = 0; b < pieceMoves.Count; b++)
			{
				currentAvailableMoves.Add(pieceMoves[b]);
			}
		}

		// Are we in check
		if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
		{
			// Is king defendable
			for (int i = 0; i < defendingPiece.Count; i++)
			{
				List<Vector2Int> defendingMoves = defendingPiece[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

				// Delete all moves putting us in check
				SimulateMoveForSinglePiece(defendingPiece[i], ref defendingMoves, targetKing);

				if (defendingMoves.Count != 0)
				{
					return false;
				}
			}

			return true; // Checkmate Exit
		}

		return false;
	}

	#endregion

	#region UI Buttons

	public void OnMenuButton()
	{
		GameReset();
		GameUI.Instance.OnLeaveGame();

		Invoke("ShutdownRelay", 1.0f);
	}

	public void OnQueenButton()
	{
		Vector2Int[] lastMove = moveList[moveList.Count - 1];
		BasePiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

		OnQueenPromotion(lastMove, targetPawn);

		if (isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
		}
		else if (!isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.blackTeam);
		}
	}

	public void OnKnightButton()
	{
		Vector2Int[] lastMove = moveList[moveList.Count - 1];
		BasePiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

		OnKnightPromotion(lastMove, targetPawn);

		if (isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
		}
		else if (!isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.blackTeam);
		}
	}

	public void OnBishopButton()
	{
		Vector2Int[] lastMove = moveList[moveList.Count - 1];
		BasePiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

		OnBishopPromotion(lastMove, targetPawn);

		if (isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
		}
		else if (!isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.blackTeam);
		}

	}

	public void OnRookButton()
	{
		Vector2Int[] lastMove = moveList[moveList.Count - 1];
		BasePiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

		OnRookPromotion(lastMove, targetPawn);

		if (isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.whiteTeam);
		}
		else if (!isWhiteTurn)
		{
			GameUI.Instance.ChangeCamera(CameraAngle.blackTeam);
		}
	}

	#endregion

	#region Special Moves

	private void ProcessSpecialMove()
	{
		if (specialMove == SpecialMove.EnPassant)
		{
			var newMove = moveList[moveList.Count - 1];
			BasePiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
			var targetPawnPosition = moveList[moveList.Count - 2];
			BasePiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

			if (myPawn.currentX == enemyPawn.currentX)
			{
				if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
				{
					if (enemyPawn.team == 0)
					{
						deadWhites.Add(enemyPawn);
						enemyPawn.SetScale(Vector3.one * deathSize);
						enemyPawn.SetPosition
						(
							new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
							- bounds
							+ new Vector3(tileSize / 2, 0, tileSize / 2)
							+ (Vector3.back * deathSpacing) * deadWhites.Count
						);
					}
					else
					{
						deadBlacks.Add(enemyPawn);
						enemyPawn.SetScale(Vector3.one * deathSize);
						enemyPawn.SetPosition
						(
							new Vector3(8 * tileSize, yOffset, -1 * tileSize)
							- bounds
							+ new Vector3(tileSize / 2, 0, tileSize / 2)
							+ (Vector3.forward * deathSpacing) * deadBlacks.Count
						);
					}
					chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
				}
			}
		}

		if (specialMove == SpecialMove.Promotion)
		{
			DisplayPromotion();
		}

		if (specialMove == SpecialMove.Castling)
		{
			Vector2Int[] lastMove = moveList[moveList.Count - 1];

			// Left Rook
			if (lastMove[1].x == 2)
			{
				if (lastMove[1].y == 0) // White Team
				{
					BasePiece rook = chessPieces[0, 0];
					chessPieces[3, 0] = rook;
					PositionSinglePiece(3, 0);
					chessPieces[0, 0] = null;
				}
				else if (lastMove[1].y == 7) // Black Team
				{
					BasePiece rook = chessPieces[0, 7];
					chessPieces[3, 7] = rook;
					PositionSinglePiece(3, 7);
					chessPieces[0, 7] = null;
				}
			}
			else if (lastMove[1].x == 6)
			{
				if (lastMove[1].y == 0) // White Team
				{
					BasePiece rook = chessPieces[7, 0];
					chessPieces[5, 0] = rook;
					PositionSinglePiece(5, 0);
					chessPieces[7, 0] = null;
				}
				else if (lastMove[1].y == 7) // Black Team
				{
					BasePiece rook = chessPieces[7, 7];
					chessPieces[5, 7] = rook;
					PositionSinglePiece(5, 7);
					chessPieces[7, 7] = null;
				}
			}
		}

	}

	private void DisplayPromotion()
	{
		promotionScreen.SetActive(true);
	}

	private void OnQueenPromotion(Vector2Int[] lastMove, BasePiece targetPawn)
	{
		if (targetPawn.type == ChessPieceType.Pawn)
		{
			if (targetPawn.team == 0 && lastMove[1].y == 7)
			{
				BasePiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0, 0, 8);
				newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
			if (targetPawn.team == 1 && lastMove[1].y == 0)
			{
				BasePiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1, 1, 9);
				newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
		}

		promotionScreen.SetActive(false);
	}

	private void OnKnightPromotion(Vector2Int[] lastMove, BasePiece targetPawn)
	{
		if (targetPawn.type == ChessPieceType.Pawn)
		{
			if (targetPawn.team == 0 && lastMove[1].y == 7)
			{
				BasePiece newKnight = SpawnSinglePiece(ChessPieceType.Knight, 0, 0, 4);
				newKnight.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newKnight;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
			if (targetPawn.team == 1 && lastMove[1].y == 0)
			{
				BasePiece newKnight = SpawnSinglePiece(ChessPieceType.Knight, 1, 1, 5);
				newKnight.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newKnight;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
		}

		promotionScreen.SetActive(false);
	}

	private void OnBishopPromotion(Vector2Int[] lastMove, BasePiece targetPawn)
	{
		if (targetPawn.type == ChessPieceType.Pawn)
		{
			if (targetPawn.team == 0 && lastMove[1].y == 7)
			{
				BasePiece newBishop = SpawnSinglePiece(ChessPieceType.Bishop, 0, 0, 6);
				newBishop.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newBishop;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
			if (targetPawn.team == 1 && lastMove[1].y == 0)
			{
				BasePiece newBishop = SpawnSinglePiece(ChessPieceType.Bishop, 1, 1, 7);
				newBishop.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newBishop;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
		}

		promotionScreen.SetActive(false);
	}

	private void OnRookPromotion(Vector2Int[] lastMove, BasePiece targetPawn)
	{
		if (targetPawn.type == ChessPieceType.Pawn)
		{
			if (targetPawn.team == 0 && lastMove[1].y == 7)
			{
				BasePiece newRook = SpawnSinglePiece(ChessPieceType.Rook, 0, 0, 2);
				newRook.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newRook;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
			if (targetPawn.team == 1 && lastMove[1].y == 0)
			{
				BasePiece newRook = SpawnSinglePiece(ChessPieceType.Rook, 1, 1, 3);
				newRook.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
				Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
				chessPieces[lastMove[1].x, lastMove[1].y] = newRook;
				PositionSinglePiece(lastMove[1].x, lastMove[1].y);
			}
		}

		promotionScreen.SetActive(false);
	}

	#endregion

	#region Operations

	private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
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
		if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
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
				if (ocp.type == ChessPieceType.King)
				{
					CheckMate(1);
				}

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
				if (ocp.type == ChessPieceType.King)
				{
					CheckMate(0);
				}

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

		isWhiteTurn = !isWhiteTurn;

		moveList.Add(new Vector2Int[] { previousPos, new Vector2Int(x, y) });

		ProcessSpecialMove();

		if (currentlyDragging)
		{
			currentlyDragging = null;
		}

		if (CheckForCheckmate())
		{
			CheckMate(cp.team);
		}

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

	public void GameReset()
	{
		// UI
		rematchButton.interactable = true;

		rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
		rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

		victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
		victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
		victoryScreen.SetActive(false);

		promotionScreen.SetActive(false);

		// Fields Reset
		currentlyDragging = null;
		availableMoves.Clear();
		moveList.Clear();

		// Clean Up
		for (int x = 0; x < TILE_COUNT_X; x++)
		{
			for (int y = 0; y < TILE_COUNT_Y; y++)
			{
				if (chessPieces[x, y] != null)
				{
					Destroy(chessPieces[x, y].gameObject);
				}

				chessPieces[x, y] = null;
			}
		}

		for (int i = 0; i < deadWhites.Count; i++)
		{
			Destroy(deadWhites[i].gameObject);
		}
		for (int i = 0; i < deadBlacks.Count; i++)
		{
			Destroy(deadBlacks[i].gameObject);
		}

		deadWhites.Clear();
		deadBlacks.Clear();

		SpawnAllPieces();
		PositionAllPieces();
		isWhiteTurn = true;
	}

	#endregion
}