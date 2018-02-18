using UnityEngine;

public class VfxHandler : MonoBehaviour {

    enum RunMode
    {
        playOnce,
        playLoop,
    }
    [SerializeField] RunMode runMode = RunMode.playOnce;
    [SerializeField] int numOfLoop = 1;
    [SerializeField] float timeGapBetweenLoop;
    int   currentLoop;
    float currentTime;
    ParticleSystem   vfx_partical;

	// Use this for initialization
	void Start () {

        vfx_partical = GetComponent<ParticleSystem>();

        if (runMode == RunMode.playOnce)
            vfx_partical.Play();
        else if (numOfLoop > 0)
        {
            vfx_partical.Play();
            currentLoop = 1;
            currentTime = 0;
        }
    }

    // Update is called once per frame
    void Update () {

        switch (runMode)
        {
            case RunMode.playOnce:
                if (!vfx_partical.IsAlive())
                {
                    Destroy(this.gameObject);
                }
                break;
            case RunMode.playLoop:
                {
                    if (currentLoop < numOfLoop)
                    {
                        currentTime += Time.deltaTime;
                        if (currentTime > timeGapBetweenLoop)
                        {
                            currentTime = 0;
                            currentLoop++;
                            vfx_partical.Play();
                        }
                    }
                    else if (!vfx_partical.IsAlive())
                    {
                        Destroy(this.gameObject);
                    }

                }
                break;
        }
	}
}
