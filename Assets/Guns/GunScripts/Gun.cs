using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Gun : MonoBehaviour
{

    enum FireMode { Semi, Burst, Full };
    FireMode fireMode;


    [Header("Firing")]
    [SerializeField, Tooltip("What modes of firing can you choose between")]        private List<FireMode> firemodes = new List<FireMode>() { FireMode.Semi };
    [SerializeField, Tooltip("The Force the bullet fires with"), Min(0f)]           private float FiringForce = 35f;
    [SerializeField, Tooltip("When you fire does the gun auto-chamber")]            private bool AutomaticChamber = true; 
    [SerializeField, Tooltip("How fast the gun auto-chambers"), Min(0f)]            private float ChamberTime = 0.05f; 
    

    [Header("Handling")]
    [SerializeField, Tooltip("How much the gun rotates vertically (up/down)")]      private float VerticalRecoil = 4f;
    [SerializeField, Tooltip("How much the gun rotates horizontally (left/right)")] private float HorizontalRecoil = 1f;
    [SerializeField, Tooltip("How much MORE the gun recoils if you're hipfiring")]  private float HipfireRecoilMultiplier = 2f;
    [SerializeField, Tooltip("How much the gun kicks downwards")]                   private float Kickback = 0.5f;
    [SerializeField, Tooltip("How much the gun kicks upwards")]                     private float Kickup = 0.5f;
    [Space]
    [SerializeField, Tooltip("How quickly you chamber your gun"), Min(0f)]          private float ManualChamberTime = 1f; 
    [SerializeField, Tooltip("The object Rack/Chamber that moves back")]            private GameObject Rack;
    [SerializeField, Tooltip("How far back the chamber moves when unchambering")]   private float UnchamberedRackDistance;
    [Space]
    [SerializeField, Tooltip("How fast you recover from shooting"), Min(0f)]        private float RecoverSpeed = 12;
    [SerializeField, Tooltip("How much time it takes to reload"), Min(0f)]          private float ReloadTime = 2f;
    [Space]
    [SerializeField, Tooltip("How much the gun sways when not aiming")]             private float HipSwayAmount = 60f;
    [SerializeField, Tooltip("How much the gun sways when aiming")]                 private float AimSwayAmount = 20f;


    [Header("Mag")]
    [SerializeField, Tooltip("What magazine prefab to load into the gun")]          private GameObject MagazinePrefab; 
    [SerializeField, Tooltip("Where the magazine should centre on")]                private Transform MagazinePosition;
    [Space]
    [SerializeField, Tooltip("The current magazine inserted in the gun")]           private GameObject CurrentMagazine;
    [SerializeField, Tooltip("Gets the inserted magazines info, such as bullets")]  private Magazine CurrentMagazineInfo;


    [Header("Bullet")]
    [SerializeField, Tooltip("Damage the bullet does to the object"), Min(0f)]      private float BulletDamage = 1;
    [SerializeField, Tooltip("How many bullets (0 or 1) are in the chamber")]       private int BulletsChambered;
    [SerializeField, Tooltip("Where the barrel is (Where the bullets leave)")]      private Transform Barrel;
    [SerializeField, Tooltip("What bullet prefab is being shot out")]               private GameObject Bullet;
    [SerializeField, Tooltip("Where the casings exit (align transforms up)")]       private Transform CasingExitPos;


    [Header("Aiming")]
    [SerializeField, Tooltip("Where the camera will move to when aiming")]          private Transform AimPos;
    [SerializeField, Tooltip("Where the gun is when hipfiring")]                    public Vector3 HipfirePos;
    [SerializeField, Tooltip("How fast you go to aim"), Min(0f)]                    private float AimDownSpeed = 8f;
    [SerializeField, Tooltip("Camera FOV when aiming"), Min(0f)]                    public float AimFOV = 75;


    [Header("Sounds")]
    [SerializeField] private AudioClip FiringSound;
    [SerializeField] private AudioClip TriggerPullSound;
    [SerializeField] private AudioClip MagInsertSound;
    [SerializeField] private AudioClip ChamberSound;
    [SerializeField] private AudioClip CasingExitSound;





    [HideInInspector] public bool IsAiming;
    [HideInInspector] public bool isPickedUp;
    [HideInInspector] public Transform Holder;
    [HideInInspector] public Rigidbody rb;
    

    float CasingsInChamber;
    float holdRtimer;
    int selectedFireModeIndex;

    bool isChambering;
    bool isReloading;

    bool holdingR;

    Camera cam;
    Camerainteract camerainteract;
    CameraLook cameraLook;
    AudioManager audioManager;

    Vector3 WantedGunPos;



    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        cam = Camera.main;
        selectedFireModeIndex = 0;
        fireMode = firemodes[selectedFireModeIndex];

        audioManager = FindObjectOfType<AudioManager>();
        camerainteract = FindObjectOfType<Camerainteract>();
        cameraLook = FindObjectOfType<CameraLook>();

        isChambering = false;
        isReloading = false;
    }


    void Update()
    {

        if(transform.parent) { if(transform.parent.name == "Holder"){  isPickedUp = true; Holder = transform.parent;  } else {    isPickedUp = false; Holder = null; } } else{  isPickedUp = false; }


        if(!isPickedUp){    return;    }

        if(Input.GetKeyDown(KeyCode.Mouse0)){   audioManager.PlayAudio(TriggerPullSound, this.transform.position);   StartCoroutine(Shoot());    }
        
        
        if(Input.GetKey(KeyCode.Mouse1)){   IsAiming = true;    }
        else{   IsAiming = false;   }
        
        Aim();
        MoveGun();
        RotateGun();
        LookSway();

        if(Input.GetKeyDown(KeyCode.R)){   holdingR = true;   }
        if(holdingR){
            holdRtimer += Time.deltaTime;
            if(holdRtimer > 0.3f){  
                StartCoroutine(Reload());   
                holdRtimer = 0;     
                holdingR = false;   
            }

            if(Input.GetKeyUp(KeyCode.R)){  

                if(holdRtimer <= 0.3f){ StartCoroutine(Chamber(ManualChamberTime));  }
                holdRtimer = 0f;
                holdingR = false;
            }
        }

        if(Input.GetKeyDown(KeyCode.X)){    ChangeFireMode();   }

    }


    // Chambers the gun / Takes bullet from mag and puts it into the chamber
    IEnumerator Chamber(float timeToChamber){
        
        if(isChambering){   Debug.Log("ALREADY CHAMBERING");    yield break; }
        Debug.Log("CHAMBER");

        if(ChamberSound){
            audioManager.PlayAudio(ChamberSound, this.transform.position);
        }
        float goneTime = 0;

        isChambering = true;
        while(goneTime <= (timeToChamber * 0.5f)){
            
            Rack.transform.localPosition = Vector3.forward * UnchamberedRackDistance * (goneTime / (timeToChamber * 0.5f));
            
            goneTime += Time.deltaTime;
            yield return null;
        }
        Rack.transform.localPosition = Vector3.forward * UnchamberedRackDistance;

        goneTime = 0;

        if(timeToChamber == ManualChamberTime){
            Recoil(0f, 0f, 0.7f, 0f);
        }
        
        if(CasingsInChamber >= 1 || BulletsChambered >= 1){  ExpellCasing(); }
        CasingsInChamber = 0;
        BulletsChambered = 0;


        while(goneTime <= (timeToChamber * 0.5f)){
            
            Rack.transform.localPosition = Vector3.forward * (UnchamberedRackDistance * (1 - (goneTime / (timeToChamber * 0.5f))));
            
            goneTime += Time.deltaTime;
            yield return null;
        }
        Rack.transform.localPosition = Vector3.zero;
        isChambering = false;
        isReloading = false;

        if(CurrentMagazineInfo.CurrentBullets >= 1){

            // Take a bullet from mag and put it in the chamber
            BulletsChambered = 1;
            CurrentMagazineInfo.CurrentBullets -= 1;

            Bullet = CurrentMagazineInfo.Bullet;

        }
        else if (CurrentMagazineInfo.CurrentBullets < 1){

            // Do nothing really
            BulletsChambered = 0;
            CurrentMagazineInfo.CurrentBullets = 0;
            Debug.Log(" MAG EMPTY -> CANT CHAMBER");

        }


        yield return null;
    }


    // Reloads the gun / Drops old mag and puts new one in. Chambers only if mag and chamber are both empty
    IEnumerator Reload(){

        if(isReloading){    Debug.Log("ALREADY RELOADING"); yield break;    }
        Debug.Log("RELOAD");

        isReloading = true;

        bool shouldChamber = false;

        if(CurrentMagazine){    if(BulletsChambered == 0 && CurrentMagazineInfo.CurrentBullets == 0){   shouldChamber = true;   }   }
        else{   shouldChamber = true;   }


        if(CurrentMagazine){
            CurrentMagazine.GetComponent<Rigidbody>().useGravity = true;
            CurrentMagazine.GetComponent<Rigidbody>().isKinematic = false;
            CurrentMagazine.GetComponentInChildren<BoxCollider>().isTrigger = false;

            CurrentMagazine.AddComponent<DestroyAfterTime>();
                
            CurrentMagazine.transform.SetParent(null);
            CurrentMagazine = null;
            CurrentMagazineInfo = null; 
        }


        yield return new WaitForSeconds(ReloadTime - 0.3f);

        Recoil(-0.3f, 0.05f, 0.01f, 0.6f);

        audioManager.PlayAudio(MagInsertSound, this.transform.position);

        CurrentMagazine = Instantiate(MagazinePrefab, MagazinePosition.position, Quaternion.identity, MagazinePosition);
        CurrentMagazine.transform.localPosition = Vector3.zero;
        CurrentMagazine.transform.localEulerAngles = Vector3.zero;
        CurrentMagazine.GetComponent<Rigidbody>().useGravity = false;
        CurrentMagazine.GetComponent<Rigidbody>().isKinematic = true;
        CurrentMagazine.GetComponentInChildren<BoxCollider>().isTrigger = true;
        CurrentMagazineInfo = CurrentMagazine.GetComponent<Magazine>();

        yield return new WaitForSeconds(0.3f);


        if(shouldChamber){  StartCoroutine(Chamber(ManualChamberTime));  }
        else{   isReloading = false;    }

        yield return null;
    }


    // Pull the trigger and do whatever the firemode says
    IEnumerator Shoot(){
        
        Recoil(0.3f, 0.05f, 0.01f, 0.01f);

        if(BulletsChambered == 0){  Debug.Log(" NO BULLET CHAMBERED");  yield break;    }
        
        Debug.Log("FIRING");


        if(fireMode == FireMode.Semi){
            FireBullet();
        }
        else if(fireMode == FireMode.Burst){
                FireBullet();
                yield return new WaitForSeconds(ChamberTime + 0.01f);                
                yield return new WaitForEndOfFrame();
                FireBullet();
                yield return new WaitForSeconds(ChamberTime + 0.01f);                
                yield return new WaitForEndOfFrame();
                FireBullet();
            
        }
        else if(fireMode == FireMode.Full){
            while(Input.GetKey(KeyCode.Mouse0)){
                FireBullet();
                yield return new WaitForSeconds(ChamberTime + 0.01f);
                yield return new WaitForEndOfFrame();
            }
        }


        yield return null;
    }


    // Fire a bullet and remove the bullet from the chamber but leave the casing
    void FireBullet(){
    
        if(BulletsChambered == 1){

            GameObject tempBullet = Instantiate(Bullet, Barrel.position, Quaternion.Euler(Barrel.eulerAngles.x - 90f, Barrel.eulerAngles.y, 0));
            tempBullet.GetComponent<Rigidbody>().AddForce(Barrel.forward * FiringForce, ForceMode.Impulse);
            tempBullet.GetComponent<Bullet>().Damage = BulletDamage;;

            audioManager.PlayAudio(FiringSound, this.transform.position);
            Recoil(VerticalRecoil, HorizontalRecoil, Kickback, Kickup);
            
            BulletsChambered = 0;
            CasingsInChamber = 1;

            if(AutomaticChamber){
                StartCoroutine(Chamber(ChamberTime));
            }

        }

    }


    // Expell the casing
    void ExpellCasing(){
        GameObject tempCasing = Instantiate(Bullet.GetComponent<Bullet>().Casing, CasingExitPos.position, Quaternion.Euler(Barrel.eulerAngles.x - 90f, Barrel.eulerAngles.y, 0));
        //tempCasing.transform.rotation = Barrel.transform.rotation;
        audioManager.PlayAudio(CasingExitSound, this.transform.position);
        tempCasing.GetComponent<Rigidbody>().AddForce(CasingExitPos.transform.up * (2 * 0.05f), ForceMode.Impulse);
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-1,1),Random.Range(-1,1), Random.Range(-1,1)).normalized * 0.01f, ForceMode.Impulse);

        CasingsInChamber = 0;   
    }


    // Change the mode you're firing in (semi, burst, full)
    void ChangeFireMode(){

        if(selectedFireModeIndex < firemodes.Count-1){
            selectedFireModeIndex++;
        }
        else{
            selectedFireModeIndex = 0;
        }

        fireMode = firemodes[selectedFireModeIndex];

    }


    // Move gun holder to aiming pos or hipfire pos
    void Aim(){
        if(IsAiming){
            WantedGunPos = Vector3.zero - AimPos.localPosition;
            cameraLook.IsZoomed = true;
        }
        else if (!IsAiming){
            cameraLook.IsZoomed = false;
            WantedGunPos = HipfirePos;
        }
        
        Holder.transform.localPosition = Vector3.MoveTowards(Holder.transform.localPosition, WantedGunPos, AimDownSpeed * Time.deltaTime);
        // this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, WantedGunPos, AimDownSpeed * Time.deltaTime);

    }


    // Move the gun towards its normal pos (makes it so recoil is temporary)
    void MoveGun(){
        this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, Vector3.zero, RecoverSpeed * Time.deltaTime * 0.025f);
    }


    // Same as MoveGun but with rotation
    void RotateGun(){
        this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, Quaternion.Euler(0, 0, 0), RecoverSpeed * Time.deltaTime);
    }


    // Sway the gun when you look around
    void LookSway(){

        float aimSwayModifierX = cameraLook.modifier;
        float aimSwayModifierY = cameraLook.modifier;


        if(IsAiming){
            this.transform.localEulerAngles += new Vector3(-Input.GetAxisRaw("Mouse Y") * AimSwayAmount * aimSwayModifierY * Time.deltaTime, Input.GetAxisRaw("Mouse X") * aimSwayModifierX * AimSwayAmount * Time.deltaTime, 0);
        }
        else{
            this.transform.localEulerAngles += new Vector3(-Input.GetAxisRaw("Mouse Y") * HipSwayAmount * aimSwayModifierY * Time.deltaTime * 2, Input.GetAxisRaw("Mouse X") * aimSwayModifierX * HipSwayAmount * Time.deltaTime, 0);
        }
    }





    // Add recoil to the gun 
    void Recoil(float verticalRecoil, float horizontalRecoil, float kickback, float kickup){

        if(IsAiming){
            if(Vector3.Distance(this.transform.localPosition, Vector3.zero) < .03f){
                this.transform.position += -this.transform.forward * kickback * 0.01f;
                this.transform.position += this.transform.up * kickup * 0.01f;
            }
            this.transform.localEulerAngles += new Vector3(-verticalRecoil, Random.Range(-horizontalRecoil, horizontalRecoil), 0);

        }
        else{
            if(Vector3.Distance(this.transform.localPosition, Vector3.zero) < .1f){
                this.transform.position += -this.transform.forward * kickback * 0.01f;
                this.transform.position += this.transform.up * kickup * 0.01f;
            }
            this.transform.localEulerAngles += new Vector3(-verticalRecoil * HipfireRecoilMultiplier, Random.Range(-horizontalRecoil, horizontalRecoil) * HipfireRecoilMultiplier * 2, 0);    

        }

    }



}
