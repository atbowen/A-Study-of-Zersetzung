using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScrambling : MonoBehaviour
{
    public Transform screen;
    public List<GameObject> sparks;
    public List<Light> lights;
    public List<Texture> randomTextures;

    public Vector3 sparkOrigin, sparkCenterLocationRelToObject, sparkBoundariesRelToSparkCenter;    // sparkOrigin is used as a reference point for determining the direction of the sparks
                                                                                                    // think of sparkOrigin as origin point of the sparks on/in the object, and sparkCenter is where they appear 

    public List<AudioClip> sparkSounds;

    [SerializeField]
    private bool isScrambled, isPermanentlyScrambled, shootsSparks, flickers, showRandomImages;

    [SerializeField]
    private float defaultScrambleTime, 
                    amountOfSparksAtOnceMin, amountOfSparksAtOnceMax, timeBetweenSparksMin, timeBetweenSparksMax, 
                    timeBetweenFlickersMin, timeBetweenFlickersMax,
                    timeBetweenRandomImagesMin, timeBetweenRandomImagesMax;

    private IDInteractable id;
    private AudioSource audioFX;
    private Texture originalTexture;

    private float scrambleTime, scrambleRefTime, currentNumOfSparks, sparkTime, sparkRefTime, flickerTime, flickerRefTime, randomImageTime, randomImageRefTime;
    private List<GameObject> currentSparks = new List<GameObject>();
    private List<Vector3> currentSparkLocations = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        id = this.transform.GetComponent<IDInteractable>();
        audioFX = this.transform.GetComponent<AudioSource>();
        
        if (screen) {
            originalTexture = screen.GetComponent<MeshRenderer>().material.mainTexture;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isScrambled && (Time.time - scrambleRefTime < scrambleTime)) {

            if (flickers && id.isOn && Time.time - flickerRefTime > flickerTime) {
                Debug.Log("flickered!");
                Flicker();
            }
            if (shootsSparks && id.isOn && Time.time - sparkRefTime > sparkTime) {
                Spark();
            }
            if (showRandomImages && Time.time - randomImageRefTime > randomImageTime) {
                ShowRandomImage();
            }
        }
        else {
            isScrambled = false;
            id.isScrambled = false;
            if (id.isOn) {
                foreach (Light lite in lights) { lite.enabled = true; }
            }
            if (originalTexture) {
                screen.GetComponent<MeshRenderer>().material.mainTexture = originalTexture;
            }
            for (int i = 0; i < currentSparks.Count; i++) {
                GameObject temp = currentSparks[i];
                currentSparks.Remove(currentSparks[i]);
                Destroy(temp);
            }
        }
    }

    public void ScrambleObject() {
        scrambleTime = defaultScrambleTime;
        scrambleRefTime = Time.time;
        isScrambled = true;
        id.isScrambled = true;

        if (shootsSparks) {
            Spark();
        }       

        if (flickers) {
            Flicker();
        }

        if (showRandomImages && screen) {
            ShowRandomImage();
        }
    }

    public void ScrambleObject(float duration) {
        scrambleTime = duration;
        scrambleRefTime = Time.time;
        isScrambled = true;
        id.isScrambled = true;
    }

    private void Flicker() {
        foreach (Light lite in lights) { lite.enabled = !lite.enabled; }
        flickerTime = Random.Range(timeBetweenFlickersMin, timeBetweenFlickersMax);
        flickerRefTime = Time.time;
    }

    private void Spark() {
        sparkTime = Random.Range(timeBetweenSparksMin, timeBetweenSparksMax);
        currentNumOfSparks = Mathf.Floor(Random.Range(amountOfSparksAtOnceMin, amountOfSparksAtOnceMax));
        sparkRefTime = Time.time;

        for (int i = 0; i < currentSparks.Count; i++) {
            GameObject temp = currentSparks[i];
            currentSparks.Remove(currentSparks[i]);
            Destroy(temp);
        }

        for (int i = 0; i < currentNumOfSparks; i++) {
            currentSparkLocations.Add(new Vector3(Random.Range(-sparkBoundariesRelToSparkCenter.x, sparkBoundariesRelToSparkCenter.x),
                                                    Random.Range(-sparkBoundariesRelToSparkCenter.y, sparkBoundariesRelToSparkCenter.y),
                                                    Random.Range(-sparkBoundariesRelToSparkCenter.z, sparkBoundariesRelToSparkCenter.z)));
        }

        for (int i = 0; i < currentNumOfSparks; i++) {
            GameObject newObject = Instantiate(sparks[Random.Range(0, sparks.Count)], this.transform.position + sparkOrigin + sparkCenterLocationRelToObject + currentSparkLocations[i], 
                                                                Quaternion.LookRotation(sparkCenterLocationRelToObject - currentSparkLocations[i]));
            currentSparks.Add(newObject);
            newObject.transform.GetComponent<Animator>().SetTrigger("Shoot");
        }

        audioFX.PlayOneShot(sparkSounds[Random.Range(0, sparkSounds.Count)]);
    }

    private void ShowRandomImage() {
        randomImageTime = Random.Range(timeBetweenRandomImagesMin, timeBetweenRandomImagesMax);
        screen.GetComponent<MeshRenderer>().material.mainTexture = randomTextures[Random.Range(0, randomTextures.Count)];
        randomImageRefTime = Time.time;
    }
}
