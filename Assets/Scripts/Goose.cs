using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goose : MonoBehaviour
{
    public FMOD.Studio.EventInstance instance;

    public void Init(FMOD.Studio.EventInstance fmodInstance)
    {
        instance = fmodInstance;
    }

    public void play()
    {
        instance.start();
    }

    public void stop()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(this.gameObject));
    }
}
