using UnityEngine;
using System;

public enum CameraAngle
{
	menu = 0,
	whiteTeam = 1,
	blackTeam = 2,
	local = 3
}

public class GameUI : MonoBehaviour
{
	#region Singleton Implementation

	public static GameUI Instance { set; get; }

	private void Awake()
	{
		Instance = this;
	}

	#endregion

	[SerializeField] private Animator menuAnimator;
	[SerializeField] private Chessboard chessBoard;
	[SerializeField] private GameObject[] cameraAngles;
	[SerializeField] private SceneTransition sceneTransition;

	public Action<bool> SetLocalGame;

	// Cameras
	public void ChangeCamera(CameraAngle index)
	{
		for (int i = 0; i < cameraAngles.Length; i++)
		{
			cameraAngles[i].SetActive(false);
		}

		cameraAngles[(int)index].SetActive(true);
	}

	// Buttons
	public void OnLocalGameButton()
	{
		menuAnimator.SetTrigger("InGameMenu");
		ChangeCamera(CameraAngle.local);
	}

	public void OnLeaveGame()
	{
		ChangeCamera(CameraAngle.menu);
		menuAnimator.SetTrigger("StartMenu");
	}

	public void OnOnlineGameButton()
	{
		sceneTransition.LoadNextScene("Init");
	}
}