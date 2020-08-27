using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonPigAI : MonoBehaviour
{
    private Animator anim;
    
    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("go", true);
    }
}
