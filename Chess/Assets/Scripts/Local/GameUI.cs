using UnityEngine;
using System;

public enum CameraAngle
{
	menu = 0,
	lightTeam = 1,
	darkTeam = 2,
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
	[SerializeField] private GameObject menu;

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
		menu.SetActive(false);
	}
	
	public void OnComputerGameButton()
	{
		sceneTransition.LoadNextScene("Computer");
	}

	public void OnLeaveGame()
	{
		ChangeCamera(CameraAngle.menu);
		menuAnimator.SetTrigger("StartMenu");
		menu.SetActive(true);
	}

	public void OnOnlineGameButton()
	{
		sceneTransition.LoadNextScene("Init");
	}
}