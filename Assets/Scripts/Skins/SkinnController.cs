using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SkinnController : MonoBehaviour
{
    public static event Action<bool> OnChangeActive;

    public static int currentSkin { get; private set; }
    [SerializeField] private List<Skin> skinns;
    [SerializeField] private Transform skinnRotator;
    [SerializeField] private float offset;
    [SerializeField] private float radious;
    [SerializeField] private float startY;
    [SerializeField] private float rotateStep = 1;
    private float currentOffset;
    private float desiredOffset;
    private List<Transform> skinnsT;
    private bool moving;
    List<SkillHolderAdapted> savedSkinns;
    private void Start()
    {
        LoadMenuSkinns();
        SetSkinnsAvailable();
        SetSkinnsLock();
        currentSkin = 0;
        skinnsT = new List<Transform>();
        skinns.ForEach(s => skinnsT.Add(s.transform));
        startY = transform.position.y;
        currentOffset = offset;
        desiredOffset = offset;
        for (int i = 0; i < skinnsT.Count; i++)
        {
            float x = skinnRotator.position.x + radious * Mathf.Cos(i + offset);
            float y = skinnRotator.position.y + radious * Mathf.Sin(i + offset);
            skinnsT[i].position = new Vector3(x, startY, y);
        }
    }

    private void OnEnable()
    {
        LeftArrow.OnLeftArrowTap += PreviousSkin;
        RightArrow.OnRightArrowTapped += NextSkin;
    }
    private void OnDisable()
    {
        LeftArrow.OnLeftArrowTap -= PreviousSkin;
        RightArrow.OnRightArrowTapped -= NextSkin;
    }

    private void LoadMenuSkinns()
    {
        savedSkinns = SkinDataHolder.GetPlayerSkinData().players;
        if(skinns.Count != savedSkinns.Count)
        {
            print($"skinns {skinns.Count} savedSkinns {savedSkinns.Count}");
            Debug.LogError("Skins count in skinns data not equal to skinns couns in skinn controller");
            return;
        }
    }
    private void SetSkinnsAvailable()
    {
        int gamesCount = PlayerDataHolder.GetGamesCount();
        for (int i = 0; i < skinns.Count; i++)
        {
            int gamesNeeded = i * 5;
            savedSkinns[i].aviable = PlayerDataHolder.GetGamesCount() >= gamesNeeded;
        }
    }

    private void SetSkinnsLock()
    {
        for (int i = 0; i < skinns.Count; i++)
        {
            skinns[i].SetLock(!savedSkinns[i].aviable);
        }
    }

    private void NextSkin()
    {
        if (moving || currentSkin == skinns.Count-1) return;
        currentSkin = Mathf.Clamp(currentSkin + 1, 0, skinns.Count-1);
        desiredOffset = offset - currentSkin;
        moving = true;
        StartCoroutine(MoveToNext());
        OnChangeActive?.Invoke(CurrentSkinIsActive());
    }

    private void PreviousSkin()
    {
        if (moving || currentSkin == 0 ) return;
        currentSkin = Mathf.Clamp(currentSkin - 1, 0, currentSkin);
        desiredOffset = currentOffset + 1;
        moving = true;
        StartCoroutine(MoveToPrevious());
        OnChangeActive?.Invoke(CurrentSkinIsActive());
    }

    private IEnumerator MoveToNext()
    {
        while(currentOffset > desiredOffset)
        {
            currentOffset -= Time.deltaTime;
            SetPosition();
            yield return null;
        }
        currentOffset = desiredOffset;
        SetPosition();
        moving = false;
    }

    private bool CurrentSkinIsActive() => savedSkinns[currentSkin].aviable;

    private IEnumerator MoveToPrevious()
    {
        while (currentOffset < desiredOffset)
        {
            currentOffset += Time.deltaTime;
            SetPosition();
            yield return null;
        }
        currentOffset = desiredOffset;
        SetPosition();
        moving = false;
    }

    private void SetPosition()
    {
        for (int i = 0; i < skinnsT.Count; i++)
        {
            float x = skinnRotator.position.x + radious * Mathf.Cos(i + currentOffset);
            float y = skinnRotator.position.y + radious * Mathf.Sin(i + currentOffset);
            skinnsT[i].position = new Vector3(x, startY, y);
        }
    }
}
