using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    //player
    private RectTransform hp_cover, mana_cover;
    private TextMeshProUGUI hp_text;
    private Image active_img;
    private List<Image> passive_img = new();

    //teammate
    private RectTransform tmViewHpCover, tmViewManaCover;
    private TextMeshProUGUI tmViewHpTxt;

    private void Awake()
    {
        TryAttachStatBar();
    }
    private void TryAttachStatBar()
    {
        var statPanel = GameObject.FindGameObjectWithTag("Hpbar");
        hp_cover = null;
        mana_cover = null;
        hp_text = null;
        active_img = null;
        passive_img = new();
        if (statPanel != null)
        {
            hp_cover = statPanel.transform.Find("hp_cover").GetComponent<RectTransform>();
            mana_cover = statPanel.transform.Find("mana_cover").GetComponent<RectTransform>();
            hp_text = statPanel.transform.Find("hp_text").GetComponent<TextMeshProUGUI>();

            active_img = statPanel.transform.Find("active/sprite").GetComponent<Image>();
            for (int i = 0; i < 3; i++)
            {
                passive_img.Add(statPanel.transform.Find($"passive_{i}/sprite").GetComponent<Image>());
            }
        }
    }
    public void AssignTeammateView(RectTransform rt, Sprite sprite)
    {
        tmViewHpCover = rt.transform.Find("hp_cover").GetComponent<RectTransform>();
        tmViewManaCover = rt.transform.Find("mana_cover").GetComponent<RectTransform>();
        tmViewHpTxt = rt.transform.Find("hp_text").GetComponent<TextMeshProUGUI>();
        rt.transform.Find("icon").GetComponent<Image>().sprite = sprite;
    }

    public void UpdateHp(float current, float max)
    {
        //Debug.LogWarning(current);
        if (hp_cover == null)
            return;
        hp_cover.localScale = new Vector3(current/max, 1, 1);
        hp_text.text = $"{current}/{max}";
    }
    public void UpdateMana(float current, float max)
    {
        if (hp_cover == null)
            return;
        mana_cover.localScale = new Vector3(current / max, 1, 1);
    }
    public void UpdateActive(SkillCardBase skill)
    {
        active_img.enabled = true;
        active_img.sprite = skill.Icon;
        IconInfo.Assign(active_img.transform.parent.GetComponent<ButtonUI>(), skill);
    }
    public void UpdatePassive(SkillCardBase skill, int index)
    {
        passive_img[index].enabled = true;
        passive_img[index].sprite = skill.Icon;
        IconInfo.Assign(passive_img[index].transform.parent.GetComponent<ButtonUI>(), skill);
    }
    public void UpdateTeammateHp(float current, float max)
    {
        if (tmViewHpCover == null) return;
        tmViewHpCover.localScale = new Vector2(current / max, 1);
        tmViewHpTxt.text = $"{current} / {max}";
    }
    public void UpdateTeammateMana(float current, float max)
    {
        if (tmViewHpCover == null) return;
        tmViewManaCover.localScale = new Vector2(current / max, 1);
    }
}
