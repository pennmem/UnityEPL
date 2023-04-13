using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using Unity.Collections.LowLevel.Unsafe;
using System.Diagnostics;
using UnityEngine.UI;
using Unity.Collections;
using System.Text.RegularExpressions;
using System.Text;

using UnityEPL;

public class Main : MonoBehaviour
{
    const long delay = 10000000000;
    InterfaceManager manager;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Debug.Log("ThreadID - Start: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        //UnityEngine.Debug.Log(ThreadPool.SetMinThreads(1, 1));
        //UnityEngine.Debug.Log(ThreadPool.SetMaxThreads(1, 1));

        manager = FindObjectOfType<InterfaceManager>();

        //UnityEngine.Debug.Log(UnsafeUtility.IsBlittable(typeof(StackString)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


