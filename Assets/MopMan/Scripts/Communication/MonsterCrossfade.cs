using UnityEngine;

public class MonsterCrossfade : MonoBehaviour
{
    public AudioSource voiceSource;    //Hope that Ubiq auto assigns it as by AudioHook
    public AudioSource staticSource;  

    public float maxVoiceDistance = 30f; //Maximum voice distance

    public float maxMonsterDistance = 15f;
    public float maxYDifference = 2.5f;

    private Transform localPlayer;
    private Transform monster;

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

    void Update()
    {
        if (localPlayer == null) return;

        if (voiceSource != null) voiceSource.spatialBlend = 0f;
        voiceSource.spatialBlend = 0f;
        if (staticSource != null) staticSource.spatialBlend = 0f;

        float distanceTraGiocatori = Vector3.Distance(transform.position, localPlayer.position);
        float volumeVoiceBase = 1f - Mathf.Clamp01(distanceTraGiocatori / maxVoiceDistance);

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

        voiceSource.volume = volumeVoiceBase * (1f - monsterInfluence);

        if (staticSource != null) staticSource.volume = monsterInfluence;
    }

    public void AssignUbiqVoiceSource(AudioSource ubiqSource)
    {
        voiceSource = ubiqSource;
    }
}