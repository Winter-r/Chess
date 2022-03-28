using System.Threading;
using System.Net;
using UnityEngine;
using System;
using TMPro;

public enum CameraAngle
{
	menu = 0,
	whiteTeam = 1,
	blackTeam = 2
}

public class GameUI : MonoBehaviour
{
	public static GameUI Instance { set; get; }
	[SerializeField] public Animator menuAnimator;
	public Chessboard chessBoard;
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
		chessBoard.drawLocalButton.gameObject.SetActive(true);
	}

	public void OnOnlineGameButton()
	{
		menuAnimator.SetTrigger("OnlineMenu");
	}

	public void OnOnlineHostButton()
	{
		SetLocalGame?.Invoke(false);
		menuAnimator.SetTrigger("HostMenu");
	}

	public void OnOnlineConnectButton()
	{
		SetLocalGame?.Invoke(false);
	}

	public void OnOnlineBackButton()
	{
		menuAnimator.SetTrigger("StartMenu");
	}

	public void OnHostBackButton()
	{
		menuAnimator.SetTrigger("OnlineMenu");
	}

	public void OnLeaveGame()
	{
		ChangeCamera(CameraAngle.menu);
		menuAnimator.SetTrigger("StartMenu");
	}
}
