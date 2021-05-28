using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlanetButtonRadialMenuController : MonoBehaviour
{
    List<Button> childButtons = new List<Button>();
    Vector2[] buttonGoalPos;
    bool open = false;
    int buttonDistance = 100;
    float speed = 2f;

    void Start()
    {
        //get all button components
        childButtons = this.GetComponentsInChildren<Button>(true).
                            Where(x => x.gameObject.transform.parent != transform.parent).ToList();

        buttonGoalPos = new Vector2[childButtons.Count];

        //assign menu buttons onClick function
        this.GetComponent<Button>().onClick.AddListener(() => { OpenMenu(); });
        //centre pivot point
        this.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        foreach (Button b in childButtons)
        {
            b.gameObject.transform.position = this.transform.position;
            b.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            b.gameObject.SetActive(false);
        }
    }

    public void OpenMenu()
    {
        open = !open;
        float angle = 90 / (childButtons.Count - 1) * Mathf.Deg2Rad;
        for (int i = 0; i < childButtons.Count; i++)
        {
            if (open)
            {
                float xpos = Mathf.Cos(angle * i) * buttonDistance;
                float ypos = Mathf.Sin(angle * i) * buttonDistance;

                buttonGoalPos[i] = new Vector2(this.transform.position.x - xpos,
                                               this.transform.position.y + ypos);
            }
            else
            {
                buttonGoalPos[i] = this.transform.position;
            }
        }

        StartCoroutine(MoveButtons());
    }

    private IEnumerator MoveButtons()
    {
        foreach (Button b in childButtons)
        {
            b.gameObject.SetActive(true);
        }
        int loops = 0;
        while (loops <= buttonDistance / speed)
        {
            yield return new WaitForSeconds(0.01f);
            for (int i = 0; i < childButtons.Count; i++)
            {
                Color c = childButtons[i].gameObject.GetComponent<Image>().color;
                if (open)
                    c.a = Mathf.Lerp(c.a, 1, speed * Time.deltaTime);
                else
                    c.a = Mathf.Lerp(c.a, 0, speed * Time.deltaTime);
                childButtons[i].gameObject.GetComponent<Image>().color = c;
                childButtons[i].gameObject.transform.position = Vector2.Lerp(
                            childButtons[i].gameObject.transform.position,
                            buttonGoalPos[i], speed * Time.deltaTime);
            }
            loops++;
        }
        if (!open)
        {
            foreach (Button b in childButtons)
            {
                b.gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
