using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

	[SerializeField] public Animator menuAnimator;
	[SerializeField] private Chessboard chessBoard;
	[SerializeField] private GameObject[] cameraAngles;

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
		SceneManager.LoadScene("Init");
	}
}