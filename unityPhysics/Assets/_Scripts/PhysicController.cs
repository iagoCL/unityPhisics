using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhysicController : MonoBehaviour
{
    [SerializeField] private PhysicsManager physicsManager;
    private Text buttonText;
    // Start is called before the first frame update
    void Start()
    {
        this.buttonText = this.transform.GetComponentInChildren<Text>();
        updateText(this.physicsManager.getPaused());
        Button button = this.GetComponent<Button>();
        button.onClick.AddListener(() => { togglePaused(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            togglePaused();
        }
    }

    private void togglePaused(){
        updateText(this.physicsManager.togglePaused());
    }

    private void updateText(bool isPaused)
    {
        if (isPaused)
        {
            this.buttonText.text = "Start";
        }
        else
        {
            this.buttonText.text = "Pause";
        }
    }
}
