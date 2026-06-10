using UnityEngine;

public class MonsterCrossfade : MonoBehaviour
{
    public AudioSource voiceSource;    //ubiq hijacked communication channel
    public AudioSource staticSource;   //Monster static noise

    public float maxVoiceDistance = 8f; //Maximum communication distance

    public float maxMonsterDistance = 4f;  //Maximum monster effect distance
    public float maxYDifference = 2.5f;

    //Effects for monster creepiness
    [Range(0f, 1f)] public float maxDistortion = 0.6f;
    [Range(0f, 0.5f)] public float pitchWobbleAmt = 0.15f;

    private Transform localPlayer;
    private Transform monster;

    private AudioLowPassFilter lowPass;
    private AudioHighPassFilter highPass;
    private AudioDistortionFilter distortion;

    private float pitchTimer;
    private float targetPitch = 1f;

    void Start()
    {
        GameObject lp = GameObject.FindGameObjectWithTag("Player");
        if (lp != null) localPlayer = lp.transform;

        GameObject m = GameObject.FindGameObjectWithTag("Monster");
        if (m != null) monster = m.transform;

        if (staticSource != null)
        {
            staticSource.loop = true;
            staticSource.volume = 0f;
            if (!staticSource.isPlaying) staticSource.Play();
        }
    }

    public void AssignUbiqVoiceSource(AudioSource ubiqSource)
    {
        voiceSource = ubiqSource;
        if (voiceSource == null) return;

        lowPass = voiceSource.gameObject.GetComponent<AudioLowPassFilter>() ?? voiceSource.gameObject.AddComponent<AudioLowPassFilter>();
        highPass = voiceSource.gameObject.GetComponent<AudioHighPassFilter>() ?? voiceSource.gameObject.AddComponent<AudioHighPassFilter>();
        distortion = voiceSource.gameObject.GetComponent<AudioDistortionFilter>() ?? voiceSource.gameObject.AddComponent<AudioDistortionFilter>();

        highPass.cutoffFrequency = 1000f;
        lowPass.cutoffFrequency = 4000f;
    }

    void Update()
    {
        if (localPlayer == null) return;

        if (voiceSource != null) voiceSource.spatialBlend = 0f;

        //ignore vertical distance for players communications
        Vector3 posGiocatoreReteXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posMioGiocatoreXZ = new Vector3(localPlayer.position.x, 0f, localPlayer.position.z);

        float distanceTraGiocatori = Vector3.Distance(posGiocatoreReteXZ, posMioGiocatoreXZ);

        float voiceRatio = 1f - Mathf.Clamp01(distanceTraGiocatori / maxVoiceDistance);
        float volumeVoiceBase = voiceRatio * voiceRatio;

        //Calculate monster infleunce
        float monsterInfluence = 0f;
        if (monster != null)
        {
            float yDifference = Mathf.Abs(localPlayer.position.y - monster.position.y);
            if (yDifference <= maxYDifference)
            {
                float distFromMonster = Vector3.Distance(localPlayer.position, monster.position);
                monsterInfluence = 1f - Mathf.Clamp01(distFromMonster / maxMonsterDistance);
            }
        }

        //Horror effectes
        if (voiceSource != null && distortion != null && lowPass != null)
        {
            //Reduce voice volume 
            voiceSource.volume = volumeVoiceBase * (1f - monsterInfluence);
            distortion.distortionLevel = monsterInfluence * maxDistortion;
            //Remove frequencies
            lowPass.cutoffFrequency = Mathf.Lerp(4000f, 1500f, monsterInfluence);
            highPass.cutoffFrequency = Mathf.Lerp(1000f, 1200f, monsterInfluence);
             //Wobble effect
            if (monsterInfluence > 0.1f)
            {
                pitchTimer -= Time.deltaTime;
                if (pitchTimer <= 0f)
                {
                    float wobble = pitchWobbleAmt * monsterInfluence;
                    targetPitch = 1f + Random.Range(-wobble, wobble);
                    pitchTimer = Random.Range(0.03f, 0.1f);
                }
                voiceSource.pitch = Mathf.Lerp(voiceSource.pitch, targetPitch, Time.deltaTime * 15f);
            }
            else
            {
                voiceSource.pitch = 1f;
            }
        }
        //Increase monster noise with quadratic scale
        if (staticSource != null)
        {
            staticSource.volume = monsterInfluence * monsterInfluence;
        }
    }
}