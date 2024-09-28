using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class Gun : MonoBehaviour
{
    enum FireMode { Semi, Burst, Full };

    public Rigidbody rb;

    [Header("Firing")]
    FireMode fireMode;
    [SerializeField] private List<FireMode> firemodes = new List<FireMode>() { FireMode.Semi };
    [SerializeField] private float FiringForce = 35f;
    [SerializeField] private bool AutomaticChamber = true; 
    [SerializeField] private float ChamberTime = 0.05f; 
    [SerializeField] private float BulletDamage = 1;
    
    [Header("Handling")]
    [SerializeField] private float VerticalRecoil = 4f;
    [SerializeField] private float HorizontalRecoil = 1f;
    [SerializeField] private float HipfireRecoilMultiplier = 2f;
    [SerializeField] private float Kickback = 0.5f;
    [SerializeField] private float Kickup = 0.5f;
    [Space]
    [SerializeField] private float ManualChamberTime = 1f; 
    [SerializeField] private GameObject Rack;
    [SerializeField] private float UnchamberedRackDistance;
    [Space]
    [SerializeField] private float RecoverSpeed = 12;
    [SerializeField] private float ReloadTime = 2f;
    [Space]
    [SerializeField] private float HipSwayAmount = 15f;
    [SerializeField] private float AimSwayAmount = 10f;

    [Header("Mag")]
    [SerializeField] private GameObject MagazinePrefab; 
    [SerializeField] private Transform MagazinePosition;
    [Space]
    [SerializeField] private GameObject CurrentMagazine;
    [SerializeField] private Magazine CurrentMagazineInfo;


    [Header("Bullet")]
    [SerializeField] private int BulletsChambered;
    [SerializeField] private Transform Barrel;
    [SerializeField] private GameObject Bullet;
    [SerializeField] private Transform CasingExitPos;

    [Header("Aiming")]
    [SerializeField] private Transform AimPos;
    [SerializeField] public Vector3 HipfirePos;
    [SerializeField] private float AimDownSpeed = 8f;
    [SerializeField] public float AimFOV = 75;
    public bool IsAiming;

    [Header("Sounds")]
    [SerializeField] private AudioClip FiringSound;
    [SerializeField] private AudioClip TriggerPullSound;
    [SerializeField] private AudioClip MagInsertSound;
    [SerializeField] private AudioClip ChamberSound;
    [SerializeField] private AudioClip CasingExitSound;




    [HideInInspector] public Transform Holder;
    
    bool holdingR;

    float CasingsInChamber;
    float holdRtimer;
    int selectedFireModeIndex;

    private bool isChambering;
    private bool isReloading;

    Camera cam;
    Camerainteract camerainteract;
    CameraLook cameraLook;
    AudioManager audioManager;

    Vector3 WantedGunPos;


    // Just for debug purposes
    [HideInInspector] public bool isPickedUp;
    [HideInInspector] public int MagazineBulletCount;
    

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

        if(transform.parent.name == "Holder"){  isPickedUp = true; Holder = transform.parent;  } else {    isPickedUp = false; Holder = null; }


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
            if(holdRtimer > 0.4f){  
                StartCoroutine(Reload());   
                holdRtimer = 0;     
                holdingR = false;   
            }

            if(Input.GetKeyUp(KeyCode.R)){  

                if(holdRtimer <= 0.4f){ StartCoroutine(Chamber(ManualChamberTime));  }
                holdRtimer = 0f;
                holdingR = false;
            }
        }

        if(Input.GetKeyDown(KeyCode.X)){    ChangeFireMode();   }


        MagazineBulletCount = CurrentMagazineInfo.CurrentBullets;
    }



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
            MagazineBulletCount = 0;
        }


        yield return new WaitForSeconds(ReloadTime - 0.3f);

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



    void ExpellCasing(){
        GameObject tempCasing = Instantiate(Bullet.GetComponent<Bullet>().Casing, CasingExitPos.position, Quaternion.Euler(Barrel.eulerAngles.x - 90f, Barrel.eulerAngles.y, 0));
        //tempCasing.transform.rotation = Barrel.transform.rotation;
        audioManager.PlayAudio(CasingExitSound, this.transform.position);
        tempCasing.GetComponent<Rigidbody>().AddForce(CasingExitPos.transform.up * 2, ForceMode.Impulse);
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-1,1),Random.Range(-1,1), Random.Range(-1,1)).normalized * 0.01f, ForceMode.Impulse);
    
        CasingsInChamber = 0;
    }



    void ChangeFireMode(){

        if(selectedFireModeIndex < firemodes.Count-1){
            selectedFireModeIndex++;
        }
        else{
            selectedFireModeIndex = 0;
        }

        fireMode = firemodes[selectedFireModeIndex];

    }



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



    void MoveGun(){
        this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, Vector3.zero, RecoverSpeed * Time.deltaTime * 0.025f);
    }



    void RotateGun(){
        this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, Quaternion.Euler(0, 0, 0), RecoverSpeed * Time.deltaTime);
    }



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
