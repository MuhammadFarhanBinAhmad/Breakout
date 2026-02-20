using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    List<EventInstance> _eventInstance = new List<EventInstance>();
    List<StudioEventEmitter> _studioEventEmitter = new List<StudioEventEmitter>();

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            print("more than one audio manager exist in scene");

        Instance = this;
    }

    public void PlayOneShot(EventReference sound, Vector3 worldpos)
    {
        RuntimeManager.PlayOneShot(sound, worldpos);
    }
    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        _eventInstance.Add(eventInstance);
        return eventInstance;
    }


    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameobject)
    {
        StudioEventEmitter emitter = emitterGameobject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        _studioEventEmitter.Add(emitter);
        return emitter;

    }
    void CleanUp()
    {
        //stop all created instance
        foreach (EventInstance eventInstance in _eventInstance)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        //stop all created instance
        foreach (StudioEventEmitter emitter in _studioEventEmitter)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
