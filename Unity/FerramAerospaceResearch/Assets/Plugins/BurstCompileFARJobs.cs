using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FerramAerospaceResearch;
using Unity.Collections;
using Unity.Jobs;

// a very minimal MonoBehaviour that references FerramAerospaceResearch so that Burst picks up its jobs
public class BurstCompileFARJobs : MonoBehaviour
{
    void Start()
    {
        Debug.Log(FerramAerospaceResearch.Version.LongString);
    }

}
