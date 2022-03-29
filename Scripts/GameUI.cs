using UnityEngine;
using System;
using TMPro;

public enum CameraAngle
{
	menu = 0,
	whiteTeam = 1,
	blackTeam = 2,
	local = 3
}

public class GameUI : MonoBehaviour
{
	public static GameUI Instance { set; get; }
	[SerializeField] public Animator menuAnimator;
	[SerializeField] private Chessboard chessBoard;
	[SerializeField] private TMP_InputField addressInput;
	[SerializeField] private GameObject[] cameraAngles;

	public Action<bool> SetLocalGame;

	private void Awake()
	{
		Instance = this;
	}

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
}