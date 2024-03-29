﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsFromPlayerPrefs : MonoBehaviour
{
    public string prefKey;

    private Slider slide;
    private Toggle tog;
    private Dropdown drop;

    private bool initialized = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        slide = GetComponentInChildren<Slider>();
        tog = GetComponentInChildren<Toggle>();
        drop = GetComponentInChildren<Dropdown>();

        initialized = true;
    }

    public void SetValueFromPref()
    {
        //This is all a bit hackneyed, but if it's dumb and it works, it's not dumb, right?
        //Basically, these will only ever have ONE of slide/tog/drop
        //If it's a slider, we know we want a float
        //For a toggle it's string (cast from a bool) and for dropdown it's an int
        //So we can use that knowledge to know what type of PlayerPref we need

        if (!initialized) {
            InitializeComponents();
        }

        if (PlayerPrefs.HasKey(prefKey)) {
            if (slide != null) {
                slide.SetValueWithoutNotify(PlayerPrefs.GetFloat(prefKey));
            }
            if (tog != null) {
                tog.SetIsOnWithoutNotify(PlayerPrefs.GetString(prefKey) == "True");
            }
            if (drop != null) {
                int v = PlayerPrefs.GetInt(prefKey);
                v = Mathf.Clamp(v, 0, drop.options.Count - 1);
                drop.SetValueWithoutNotify(PlayerPrefs.GetInt(prefKey));
            }
        }
    }

}
