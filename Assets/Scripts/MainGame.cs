using System;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    [Header("References")]
    public GameObject Player;
    
    public static MainGame Instance;

    private void Awake()
    {
        Instance = this;
    }
}
