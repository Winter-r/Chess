using UnityEngine;

public class PickTeam : MonoBehaviour
{
	#region Singleton Implementation

	public static PickTeam Instance { set; get; }

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
	public void OnWhiteTeamButton()
	{
		menuAnimator.SetTrigger("InGameMenu");
		ChangeCamera(CameraAngle.lightTeam);
		menu.SetActive(false);
	}
	
	public void OnBlackTeamButton()
	{
		menuAnimator.SetTrigger("InGameMenu");
		ChangeCamera(CameraAngle.darkTeam);
		menu.SetActive(false);
	}
	
	public void OnRandomTeamButton()
	{
		menuAnimator.SetTrigger("InGameMenu");
		// randomly choose either light or dark team but not menu
		CameraAngle randomCamera = (CameraAngle)Random.Range(1, 3);
		ChangeCamera(randomCamera);
		Debug.Log("Randomly picked camera: " + randomCamera);
		menu.SetActive(false);
	}
}