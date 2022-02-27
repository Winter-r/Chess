using UnityEngine;

public class GameUI : MonoBehaviour
{
	public static GameUI Instance { get; set; }
	[SerializeField] private Animator menuAnimator;
	private void Awake()
	{
		Instance = this;
	}

	// Buttons
	public void OnLocalGameButton()
	{
		menuAnimator.SetTrigger("InGameMenu");
	}

	public void OnOnlineGameButton()
	{
		menuAnimator.SetTrigger("OnlineMenu");
	}
	
	public void OnOnlineHostButton()
	{
		menuAnimator.SetTrigger("HostMenu");
	}
	public void OnOnlineConnectButton()
	{

	}
	
	public void OnOnlineBackButton()
	{
		menuAnimator.SetTrigger("StartMenu");
	}
	
	public void OnHostBackButton()
	{
		menuAnimator.SetTrigger("OnlineMenu");
	}
}
