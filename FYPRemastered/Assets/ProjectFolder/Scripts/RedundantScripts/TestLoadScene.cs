using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLoadScene : MonoBehaviour
{
    private AsyncOperation asyncLoad;

    private void Start()
    {
        StartCoroutine(LoadMainSceneAsync());
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Map_v1");
        }*/
    }

    private IEnumerator LoadMainSceneAsync()
    {
        // Start loading the main scene (replace "MainScene" with the actual name of your main scene)
        asyncLoad = SceneManager.LoadSceneAsync("Map_v1");

        // Don't allow the scene to activate until the loading is done
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous loading is done
        while (asyncLoad.progress < 0.9f)
        {
            yield return null; // Wait until the next frame
            // You can optionally show a loading bar here by using asyncLoad.progress
            // For example:
            // Debug.Log("Loading Progress: " + asyncLoad.progress * 100 + "%");
        }
        // If the scene is loaded, wait for Space key to be pressed before transitioning
        //if (asyncLoad.progress >= 0.9f)  // 0.9 means the scene is ready to activate
        //{
        for (int i = 0; i < 5; i++) yield return null;

        Scene newScene = SceneManager.GetSceneByName("Map_v1");
        if (newScene.IsValid())
        {
            yield return new WaitForSeconds(2f);
            GameObject[] sceneObjects = newScene.GetRootGameObjects();
            if (sceneObjects.Length == 0)
            {
                Debug.LogWarning("No root objects found in the new scene.");
            }
            foreach (GameObject obj in sceneObjects)
            {
                if (obj.CompareTag("Player"))
                {
                    GameManager.Instance.SetPlayer(obj);
                    Debug.LogWarning("Player found!!");
                    //newGameManager.SetPlayer(obj);
                }
                else
                {
                    Debug.LogWarning("Player not found!!");

                }

            }
            Debug.LogWarning("New Scene is Valid!!");
        }


        Debug.Log("Press Space to load the scene...");
        while (!Input.GetKeyDown(KeyCode.Space))  
        {
            yield return null; // Wait for the user to press Space
        }
       
        asyncLoad.allowSceneActivation = true;




    }
}
