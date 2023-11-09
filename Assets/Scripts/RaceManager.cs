using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{

    public static RaceManager instance;
    // Start is called before the first frame update
    public Checkpoint[] allCheckpoints;

    public int totalLaps;

    public CarController playerCar;
    public List<CarController> allAICars = new List<CarController>();
    public int playerPosition;
    public float timeBetweenPosCheck=0.2f;
    private float posChkCounter;

    public float aiDefaultSpeed =30f, playerDefualtSpeed =30f, rubberBandSpeedMod =3.5f, rubBandAccel=.5f;

    public bool isStarting;
    public float timeBetweenStarCount=1f;
    private float startCounter;
    public int countdownCurrent=3;

    public int playerStartPosition, aiNumberToSpawn;
    public Transform[] startPoints;

    public List<CarController> carToSpawn = new List<CarController>();


    public bool raceCompleted;

    public string raceCompleteScene;

    private void Awake(){
        instance = this;
    }
    void Start()
    {
        totalLaps = RaceInfoManager.instance.noOfLaps;
        aiNumberToSpawn=RaceInfoManager.instance.noOfAI;

        for (int i = 0; i < allCheckpoints.Length; i++){
            allCheckpoints[i].chNumber =i;
        }
        isStarting=true;
        startCounter=timeBetweenStarCount;

        UIManager.instance.countDownText.text =countdownCurrent+"!";

        playerStartPosition =Random.Range(0,aiNumberToSpawn+1);

        playerCar = Instantiate(RaceInfoManager.instance.racerToUse, startPoints[playerStartPosition].position, startPoints[playerStartPosition].rotation);
        playerCar.isAI = false;
        playerCar.GetComponent<AudioListener>().enabled = true;

        CameraSwitcher.instance.SeTarget(playerCar);
        //playerCar.transform.position=startPoints[playerStartPosition].position;
        //playerCar.theRB.position=startPoints[playerStartPosition].position;

        for(int i = 0; i< aiNumberToSpawn+1;i++){
            if(i != playerStartPosition){
                int selectedCar = Random.Range(0,carToSpawn.Count);
                allAICars.Add(Instantiate(carToSpawn[selectedCar],startPoints[i].position,startPoints[i].rotation));
                if(carToSpawn.Count> aiNumberToSpawn - i){

                carToSpawn.RemoveAt(selectedCar);
                }
            }
        }

        UIManager.instance.positionText.text=( playerStartPosition +1) +"/" +(allAICars.Count+1);

    }

    // Update is called once per frame
    void Update()
    {
        if(isStarting){
            startCounter-=Time.deltaTime;
            if(startCounter<=0){
                countdownCurrent--;
                startCounter=timeBetweenStarCount;


                UIManager.instance.countDownText.text =countdownCurrent+"!";

                if(countdownCurrent==0){
                    isStarting=false;
                    UIManager.instance.countDownText.gameObject.SetActive(false);
                    UIManager.instance.goText.gameObject.SetActive(true);
                }
            }
        }
        posChkCounter-=Time.deltaTime;
        if(posChkCounter<=0){
        
            playerPosition=1;
            foreach(CarController aiCar in allAICars){
                if(aiCar.currentLap> playerCar.currentLap){
                    playerPosition++;
                }else if (aiCar.currentLap== playerCar.currentLap)
                {
                    if(aiCar.nextCheckpoint>playerCar.nextCheckpoint){
                        playerPosition++;
                    }else if(aiCar.nextCheckpoint == playerCar.nextCheckpoint){
                        if(Vector3.Distance(aiCar.transform.position,allCheckpoints[aiCar.nextCheckpoint].transform.position)<Vector3.Distance(playerCar.transform.position,allCheckpoints[aiCar.nextCheckpoint].transform.position)){
                            playerPosition++;
                        }
                    }
                }
            }
            posChkCounter = timeBetweenPosCheck;

            UIManager.instance.positionText.text=playerPosition+"/"+(allAICars.Count+1);
        }

        // manage rubber banding
        if(playerPosition==1){
            foreach(CarController aiCar in allAICars){
                aiCar.maxSpeed=Mathf.MoveTowards(aiCar.maxSpeed,aiDefaultSpeed+rubberBandSpeedMod,rubBandAccel*Time.deltaTime);
            }
            playerCar.maxSpeed =Mathf.MoveTowards(playerCar.maxSpeed,playerDefualtSpeed-rubberBandSpeedMod,rubBandAccel*Time.deltaTime);
        }
        else{
            foreach(CarController aiCar in allAICars){
                aiCar.maxSpeed=Mathf.MoveTowards(aiCar.maxSpeed,aiDefaultSpeed-(rubberBandSpeedMod*((float)playerPosition/((float)allAICars.Count+1))),rubBandAccel*Time.deltaTime);
            }
            playerCar.maxSpeed =Mathf.MoveTowards(playerCar.maxSpeed,playerDefualtSpeed+(rubberBandSpeedMod*((float)playerPosition/((float)allAICars.Count+1))),rubBandAccel*Time.deltaTime);
        }
    }
    public void FinishRace(){
        raceCompleted = true;

        switch(playerPosition){
            case 1:
                UIManager.instance.raceResultText.text= "You finished 1st";

                if(RaceInfoManager.instance.trackToUnlock !="")
                {
                    if(!PlayerPrefs.HasKey(RaceInfoManager.instance.trackToUnlock+"_unlocked"))
                    {
                        PlayerPrefs.SetInt(RaceInfoManager.instance.trackToUnlock+ "_unlocked",1);
                        UIManager.instance.trackUnlockedMessage.SetActive(true);
                    }
                    
                }
                break;
            case 2:
                UIManager.instance.raceResultText.text= "You finished 2nd";
                break;
            case 3:
                UIManager.instance.raceResultText.text= "You finished 3rd ";
                break;
            default:
                UIManager.instance.raceResultText.text= "You finished "+playerPosition + "th";
                break;
        }


        UIManager.instance.resultsScreen.SetActive(true);
    }
    public void ExitRace()
    {
        SceneManager.LoadScene(raceCompleteScene);
    }


}
