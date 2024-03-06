using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePassDetectorScript : MonoBehaviour
{
    private LogicManagerScript _logicManagerScript;

    // Start is called before the first frame update
    void Start()
    {
        _logicManagerScript = GameObject.FindGameObjectWithTag("LogicManager").GetComponent<LogicManagerScript>();
        Debug.Assert(_logicManagerScript != null, "Must be able to find the global 'LogicManagerScript' from object tagged 'LogicManager'!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) {
            _logicManagerScript.IncrementScore();
        }
    }
}
