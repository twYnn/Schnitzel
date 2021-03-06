﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingManager : MonoBehaviour
{

    private int current_spicy = 0;
    private int current_salty = 0;
    private int current_sweet = 0;

    private int goal_spicy = 0;
    private int goal_salty = 0;
    private int goal_sweet = 0;

    private int goal_delta = 0;

    public Text salty_text;
    public Text spicy_text;
    public Text sweet_text;

    public GameObject ingredients_list;

    public GameObject taste_effect;

    public StepData data = null;

    public float transitionTime = 1.0f;

    public AudioClip ploufSound;



    // Start is called before the first frame update
    void Start()
    {
        if (data == null)
            data = Gamemachine.instance.GetData();
        goal_delta = data.delta;
        goal_salty = data.salty;
        goal_spicy = data.spicy;
        goal_sweet = data.sweet;

        if(data != null)
        {

            Reset();
        }
            
        taste_effect.GetComponent<CanvasGroup>().alpha = 0;
        RectTransform rt = (RectTransform)taste_effect.transform.Find("Synesthesia");
        rt.sizeDelta = new Vector2(0, 0);
        taste_effect.SetActive(false);

        taste_effect.GetComponent<Button>().onClick.AddListener(delegate { if(taste_effect.GetComponent<CanvasGroup>().alpha > 0.95f) StartCoroutine("FadeOutTaste"); });

        DisplayTaste(goal_spicy, goal_sweet, goal_salty);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            Serve();
    }

    public void AddIngredient(int spicy, int sweet, int salty)
    {
        AudioSource audioSource = FindObjectOfType<AudioSource>();
        if (audioSource) audioSource.PlayOneShot(ploufSound);

        current_spicy += spicy;
        current_salty += salty;
        current_sweet += sweet;


        salty_text.text = current_salty.ToString();
        sweet_text.text = current_sweet.ToString();
        spicy_text.text = current_spicy.ToString();

    }

    public bool CheckForVictory()
    {
        bool nothingCooked = ((current_salty + current_spicy + current_sweet) == 0);
        bool wantedNothing = ((goal_salty + goal_spicy + goal_sweet) == 0);
        if (nothingCooked && wantedNothing) return true;
        else if (nothingCooked) return false;

        int delta_sum =  Mathf.Abs(goal_salty - current_salty) + Mathf.Abs(goal_spicy - current_spicy) + Mathf.Abs(goal_sweet - current_sweet);

        Debug.Log("delta sum = " + delta_sum + " | goal = " + goal_delta + " | success = " + (delta_sum < goal_delta));
        return delta_sum < goal_delta;
    }

    public void Serve()
    {
        Gamemachine.instance.NextScene(CheckForVictory());
    }

    public void Reset()
    {
        Destroy(ingredients_list);
        ingredients_list = Instantiate(data.ingredientsPrefab);
        ingredients_list.SetActive(true);

        current_spicy = 0;
        current_salty = 0;
        current_sweet = 0;

        salty_text.text = current_salty.ToString();
        sweet_text.text = current_sweet.ToString();
        spicy_text.text = current_spicy.ToString();
    }

    IEnumerator FadeInTaste()
    {
        for (float f = 0; f < 1.0f; f += 0.01f)
        {
            RectTransform rt = (RectTransform)taste_effect.transform.Find("Synesthesia");
            rt.sizeDelta = new Vector2(f*1000, f*1000);
            taste_effect.GetComponent<CanvasGroup>().alpha = f;

            yield return new WaitForSecondsRealtime(transitionTime / 100.0f);
        }
        yield return new WaitForSecondsRealtime(transitionTime / 100.0f);
    }

    IEnumerator FadeOutTaste()
    {
        for (float f = 1.0f; f > 0.0f; f -= 0.01f)
        {
            RectTransform rt = (RectTransform)taste_effect.transform.Find("Synesthesia");
            rt.sizeDelta = new Vector2(f * 1000, f * 1000);
            taste_effect.GetComponent<CanvasGroup>().alpha = f;

            yield return new WaitForSecondsRealtime (transitionTime / 100.0f);
        }

        taste_effect.SetActive(false);
        yield return new WaitForSecondsRealtime(transitionTime / 100.0f);
    }

    public void DisplayTaste()
    {
        DisplayTaste(current_spicy, current_sweet, current_salty);
    }

    public void DisplayTaste(int spicy, int sweet, int salty)
    {
        taste_effect.SetActive(true);
        StartCoroutine("FadeInTaste");
        
        Image syn = taste_effect.transform.Find("Synesthesia").GetComponent<Image>();
        syn.material.SetFloat("_Sides", Mathf.Lerp(3,15, sweet/10.0f));
        syn.material.SetFloat("_Frequency", Mathf.Lerp(3,30, salty/10.0f));
        syn.material.SetColor("_Color", new Color(1, 1- spicy/10.0f, 1 - spicy/10.0f)); //TODO: decide on the color
    }
}
