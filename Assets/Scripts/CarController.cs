using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB;


    public float maxSpeed;

    public float forwardAccel = 8f, reverseAccel = 4f;
    private float speedInput;

    public float turnStrength = 180f;
    private float turnInput;

    private bool grounded;

    public Transform groundRayPoint,groundRayPoint2;
    public LayerMask whatIsGround;
    public float groundRayLength = 0.75f;

    private float dragOnGround;
    public float gravityMod = 10f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn =25f;
    public ParticleSystem[] dustTrail;
    public float maxEmission =25f, emissionFedeSpeed =20f;
    private float emissionRate;

    public AudioSource engineSound, skidSound;
    public float skidFadeSpeed;


    public int nextCheckpoint;
    public int currentLap;

    public float lapTime, bestLapTime;

    public float resetCooldown = 2f;
    private float resetCounter;

    public bool isAI;

    public int currentTarget;
    private Vector3 targetPoint;
    public float aiAccelerateSpeed =1f ,aiTurnSpeed = .8f,aiReachPointRange=5f,aiPointVariance=3f,aiMaxturn=15f;
    private float aiSpeedInput, aiSpeedMod;



 

    // Start is called before the first frame update
    void Start()
    {
        theRB.transform.parent = null;

        dragOnGround = theRB.drag;

        if(isAI){
            targetPoint=RaceManager.instance.allCheckpoints[currentTarget].transform.position;
            RandomiseAITarget();
            aiSpeedMod = Random.Range(.8f,1.1f);
        }

        UIManager.instance.lapCounterText.text = currentLap + "/"+ RaceManager.instance.totalLaps;

        resetCounter = resetCooldown;

        

    }

    // Update is called once per frame
    void Update()
    {
        if(!RaceManager.instance.isStarting){

        

        lapTime +=Time.deltaTime;

        if(!isAI){
        var ts= System.TimeSpan.FromSeconds(lapTime);
        UIManager.instance.currentLapTimeText.text= string.Format("{0:00}:{1:00}.{2:000}s",ts.Minutes,ts.Seconds,ts.Milliseconds);
        speedInput =0f;
        if(Input.GetAxis("Vertical")>0){
            speedInput = Input.GetAxis("Vertical") * forwardAccel;
        }
        else if(Input.GetAxis("Vertical")<0){
            speedInput = Input.GetAxis("Vertical") * reverseAccel;
        }

        turnInput = Input.GetAxis("Horizontal");
        /*if(grounded && Input.GetAxis("Vertical") != 0){
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f,turnInput * turnStrength * Time.deltaTime* Mathf.Sign(speedInput)*(theRB.velocity.magnitude/maxSpeed),0f));
        }*/
        

        if( resetCounter>0){
            resetCounter-=Time.deltaTime;
        }
        
        if(Input.GetKeyDown(KeyCode.R) && resetCounter <=0){
            RestToTrack();
        }
        

        }
        else{
            targetPoint.y=transform.position.y;
            if(Vector3.Distance(transform.position,targetPoint) < aiReachPointRange){
                SetNextAITarget();
            };
            Vector3 targetDire=targetPoint-transform.position;
            float angle = Vector3.Angle(targetDire,transform.forward);
            Vector3 locaPos= transform.InverseTransformPoint(targetPoint);
            if(locaPos.x <0f){
                angle = -angle;
            }
            turnInput=Mathf.Clamp(angle/aiMaxturn,-1f,1f);
            if(Mathf.Abs(angle)< aiMaxturn){
                aiSpeedInput=Mathf.MoveTowards(aiSpeedInput,1f,aiAccelerateSpeed);
            }else{
                aiSpeedInput=Mathf.MoveTowards(aiSpeedInput,aiTurnSpeed,aiAccelerateSpeed);
            }

            aiSpeedInput=1f;
            speedInput=aiSpeedInput*forwardAccel*aiSpeedMod;
        }

        // turning the wheels
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x,(turnInput*maxWheelTurn) -180,leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x,(turnInput*maxWheelTurn) ,rightFrontWheel.localRotation.eulerAngles.z);

        

        //transform.position = theRB.position;
        
        // control paricle emissions dust effect 
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFedeSpeed * Time.deltaTime);

        if(grounded && (Mathf.Abs(turnInput) > 0.5f || (theRB.velocity.magnitude < maxSpeed * 0.5f && theRB.velocity.magnitude !=0))){
            emissionRate = maxEmission;
        }

        // to stop dust in stoping  
        if(theRB.velocity.magnitude <= 1f){
            emissionRate = 0;
        }

        for(int i = 0; i <dustTrail.Length; i++){
            var emissionModule = dustTrail[i].emission;
            emissionModule.rateOverTime = emissionRate;
        }
        
        if(engineSound != null){
            engineSound.pitch = 1f + ((theRB.velocity.magnitude/ maxSpeed )* 2.3f);
        }
        
        if(skidSound != null){
            if(grounded && (Mathf.Abs(turnInput) > 0.5f )){
                skidSound.volume = 1f;
            }else{
                skidSound.volume = Mathf.MoveTowards(skidSound.volume,0f,skidFadeSpeed * Time.deltaTime);
            }
            if(theRB.velocity.magnitude <= 1f){
            skidSound.volume = 0;
        }
        }
        }
    }

    private void FixedUpdate(){

        grounded  = false;
        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if(Physics.Raycast(groundRayPoint.position, -transform.up, out hit,groundRayLength,whatIsGround)){
            grounded = true;

            normalTarget = hit.normal;

        }

        if(Physics.Raycast(groundRayPoint2.position, -transform.up, out hit,groundRayLength,whatIsGround))
        {
            grounded = true;

            normalTarget = (normalTarget + hit.normal)/ 2f;
        }

        if(grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up , normalTarget)*transform.rotation;
        }

        // accelerates the car
        if(grounded )
        {
            theRB.drag = dragOnGround;
            theRB.AddForce(transform.forward * speedInput * 1000f);
        }else
        {
            theRB.drag = 0.1f;

            theRB.AddForce(-Vector3.up * gravityMod * 100f);
        }

        if(theRB.velocity.magnitude > maxSpeed){
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }

        //Debug.Log(theRB.velocity.magnitude);

        transform.position = theRB.position;
        // defult turning wheels 
        /*if(grounded && Input.GetAxis("Vertical") != 0){
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f,turnInput * turnStrength * Time.deltaTime* Mathf.Sign(speedInput)*(theRB.velocity.magnitude/maxSpeed),0f));
        }*/

        // fixing the issue relate to turning wheels before been drug or completly stopped
        if (grounded && theRB.velocity.magnitude > 0.1f)// speedInput  !=0 his correction 
        {       
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
        }

        
        

    }


    public void CheckpointHit(int chNumber){
        //Debug.Log(chNumber);
        if(chNumber== nextCheckpoint){
            nextCheckpoint++;

            if(nextCheckpoint == RaceManager.instance.allCheckpoints.Length){
                nextCheckpoint = 0;
                LapCompleted();
                
            }
        }

        if(isAI){
            if(chNumber==currentTarget){
                SetNextAITarget();
            }
        }

    }
    public void SetNextAITarget(){
        currentTarget++;
        if(currentTarget>=RaceManager.instance.allCheckpoints.Length){
            currentTarget=0;
        }

        targetPoint=RaceManager.instance.allCheckpoints[currentTarget].transform.position;
        RandomiseAITarget();
    }

    public void LapCompleted(){
        currentLap++;
        if(lapTime<bestLapTime || bestLapTime ==0){
            bestLapTime = lapTime;
        }

        if(currentLap<= RaceManager.instance.totalLaps){

        


            lapTime=0f;
            if(!isAI){
                var ts= System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.instance.bestLapTimeText.text= string.Format("{0:00}:{1:00}.{2:000}s",ts.Minutes,ts.Seconds,ts.Milliseconds);
                UIManager.instance.lapCounterText.text = currentLap + "/"+ RaceManager.instance.totalLaps;
            }

        }else{
            if(!isAI){
                isAI = true;
                aiSpeedMod =1f;

                targetPoint=RaceManager.instance.allCheckpoints[currentTarget].transform.position;
                RandomiseAITarget();

                var ts= System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.instance.bestLapTimeText.text= string.Format("{0:00}:{1:00}.{2:000}s",ts.Minutes,ts.Seconds,ts.Milliseconds);

                RaceManager.instance.FinishRace();
            }
        }
    }

    public void RandomiseAITarget(){
        targetPoint+=new Vector3(Random.Range(-aiPointVariance,aiPointVariance),0f,Random.Range(-aiPointVariance,aiPointVariance));
    }


    void RestToTrack(){
        int pointToGoTo = nextCheckpoint-1;
        if(pointToGoTo<0){
            pointToGoTo=RaceManager.instance.allCheckpoints.Length-1;
        }
        transform.position=RaceManager.instance.allCheckpoints[pointToGoTo].transform.position;
        //transform.rotation=RaceManager.instance.allCheckpoints[pointToGoTo].transform.rotation; // Match checkpoint rotation
        theRB.position = transform.position;
        //theRB.rotation = transform.rotation; // Match checkpoint rotation
        theRB.velocity = Vector3.zero; // Stop any movement immediately
        //theRB.angularVelocity = Vector3.zero; // Stop any rotation immediately

        resetCounter = resetCooldown;
    }
    

}
