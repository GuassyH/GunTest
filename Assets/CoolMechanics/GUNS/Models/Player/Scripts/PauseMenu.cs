using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    public GameObject pauseScreen;
    public GameObject loadScreen;

    public Slider loadingSlider;

    [HideInInspector] public bool isPaused;

    public CameraLook cameraLook;

    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;
        Unpause();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(!isPaused){
                Pause(); 
            }
            else{
                Unpause();  
            }
        }
    }

    void Pause(){
        cameraLook.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
        Time.timeScale = 0f;
        pauseScreen.SetActive(true);
        loadScreen.SetActive(false);

        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>())
        {
            audioSource.pitch = 0;
        }

    }
    public void Unpause(){
        cameraLook.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
        loadScreen.SetActive(false);

        
        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>())
        {
            audioSource.pitch = 1;
        }
    }

    public void QuitToMenu(int SceneToLoad){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        pauseScreen.SetActive(false);
        loadScreen.SetActive(true);

        StartCoroutine(LoadLevelASync(SceneToLoad));
    }    

    IEnumerator LoadLevelASync(int SceneToLoad){
        AsyncOperation loadOperatioon = SceneManager.LoadSceneAsync(SceneToLoad);
        
        while(!loadOperatioon.isDone){
            float progress = Mathf.Clamp01(loadOperatioon.progress / 0.9f);
            loadingSlider.value = progress;
            yield return null;
        }
    }
}

