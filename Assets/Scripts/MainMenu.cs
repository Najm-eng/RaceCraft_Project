using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;//make it accessible from other C# files it has to have Awake function

    public GameObject raceSetupPanel, trackSelectPanel, raceSelectPanel;

    public Image trackSelectImage, racerSelectImage;

    private void Awake(){
        instance = this;
    }
   

    // Start is called before the first frame update
    void Start()
    {
        if(RaceInfoManager.instance.enteredRace){
            trackSelectImage.sprite= RaceInfoManager.instance.trackSprite;
            racerSelectImage.sprite= RaceInfoManager.instance.racerSprite;

            openRaceSetup();
        }

        PlayerPrefs.SetInt(RaceInfoManager.instance.trackToLoad + "_unlocked", 1);
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("key deleted");
            

        }
#endif
    }
    public void StartGame()
    {
        RaceInfoManager.instance.enteredRace = true;
        SceneManager.LoadScene(RaceInfoManager.instance.trackToLoad);

    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

    public void openRaceSetup()
    {
        raceSetupPanel.SetActive(true);
    }

    public void CloseRaceSetup()
    {
        raceSetupPanel.SetActive(false);
    }

    public void OpenTrackSelect()
    {
        trackSelectPanel.SetActive(true);
        CloseRaceSetup();
    }

    public void CloseTrackSelect()
    {
        trackSelectPanel.SetActive(false);
        openRaceSetup();
    }

    public void OpenRacerSelect()
    {
        raceSelectPanel.SetActive(true);
        CloseRaceSetup();
    }

    public void CloseRacerSelect()
    {
        raceSelectPanel.SetActive(false);
        openRaceSetup();
    }











}
