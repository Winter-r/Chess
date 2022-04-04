using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
	[SerializeField] private Animator transitioner;
	[SerializeField] private float transitionTime = 1f;
	
	public void LoadNextScene(string sceneName)
	{
		StartCoroutine(LoadScene(sceneName));
	}
	
	IEnumerator LoadScene(string sceneName)
	{
		// Play Animation
		transitioner.SetTrigger("Start");
		
		// Wait
		yield return new WaitForSeconds(transitionTime);
		
		// Load Scene
		SceneManager.LoadScene(sceneName);
	}
}