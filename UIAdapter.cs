//
//  UIAdapter.s
//
//  Created by zhanghaijun on 2018/12/05.
//  Copyright © 2018 zhanghaijun. All rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class UIAdapter : MonoBehaviour
{
    #region Test
    public Button btn;
    public Canvas canvas;
    public Text text;
    #endregion


#if UNITY_EDITOR
    [SerializeField]
    public bool EnablePCSimulator = false;
#endif
    [SerializeField]
    public float IphoneXBottomOffSet = 44f;
    [SerializeField]
    public float IphoneXLeftOffSet = 88f;
    [SerializeField]
    public bool EnableIphoneXBottomOffset = true;
    [SerializeField]
    public List<GameObject> Exceptionals;


    public RectTransform panel;
    Rect lastSafeArea = new Rect(0, 0, 0, 0);
    Rect CaculateRect = new Rect(0, 0, 0, 0);

    Rect lastSafeExceptionalArea = new Rect(0, 0, 0, 0);
    Rect CaculateExceptionalRect = new Rect(0, 0, 0, 0);

    Vector2 ReferenceResolution = new Vector2(1280, 720);

    Resolution resolutionInit;
    Resolution resolutionCurrent;
    Vector2 screenSize;
    Rect safeAreaInit;
    Vector3 positionVec = Vector3.zero;
    private List<Vector3> ExceptionalPositionInit = null;

    float scaleFactor = 0f;

    private int scaleWidth = 0;
    private int scaleHeight = 0;

    private void SetResolution()
    {
        int width = resolutionInit.width;
        int height = resolutionInit.height;
        int designWidth = (int)ReferenceResolution.x;
        int designHeight = (int)ReferenceResolution.y;
        float s1 = (float)designWidth / (float)designHeight;
        float s2 = (float)width / (float)height;
        if (s1 < s2)
        {
            designWidth = (int)Mathf.FloorToInt(designHeight * s2);
        }
        else if (s1 > s2)
        {
            designHeight = (int)Mathf.FloorToInt(designWidth / s2);
        }
        float contentScale = (float)designWidth / (float)width;
        if (contentScale < 1.0f)
        {
            scaleWidth = designWidth;
            scaleHeight = designHeight;
        }

        Screen.SetResolution(scaleWidth, scaleHeight, true);

        StartCoroutine(ChangeCanvasScaleFactor());
    }
    private IEnumerator ChangeCanvasScaleFactor()
    {
        // Screen.SetResolution(scaleWidth, scaleHeight, true); 在下一帧生效
        yield return null;
        //如果UI的缩放因子需要跟随分辨率的变化调整 
        Debug.Log("current setting: " + Screen.currentResolution.width + "," + Screen.currentResolution.height);
        yield return null;
        //if (resolutionCurrent.width != Screen.currentResolution.width || resolutionCurrent.height != Screen.currentResolution.height)
        //{
        //    screenSize = new Vector2(Screen.width, Screen.height);

        //    resolutionCurrent = Screen.currentResolution;
        //    scaleFactor = Mathf.Min(screenSize.x / ReferenceResolution.x, screenSize.y / ReferenceResolution.y);
        //    canvas.scaleFactor = scaleFactor;
        //    Debug.Log("setting: " + resolutionCurrent.width + "," + resolutionCurrent.height);
        //}
    }

    private void Awake()
    {
        if (Exceptionals != null && Exceptionals.Count != 0)
        {
            ExceptionalPositionInit = new List<Vector3>();
            for (int i = 0; i < Exceptionals.Count; ++i)
            {
                ExceptionalPositionInit.Add(Exceptionals[i].transform.localPosition);
            }
        }
#if UNITY_EDITOR
        resolutionInit = new Resolution();
        resolutionInit.width = Screen.width;
        resolutionInit.height = Screen.height;
#else
        resolutionInit = Screen.currentResolution;
#endif



        SetResolution();
        //  resolutionCurrent = Screen.currentResolution;

        //screenSize = new Vector2(Screen.width, Screen.height);
        //scaleFactor = Mathf.Min(screenSize.x / ReferenceResolution.x, screenSize.y / ReferenceResolution.y);
        //canvas.scaleFactor = scaleFactor;


        btn.onClick.AddListener(() =>
        {
            ReferenceResolution = new Vector2(1920, 1080);
            SetResolution();


        });
    }


    void ApplySafeArea(Rect area)
    {
        var anchorMin = area.position;
        var anchorMax = area.position + area.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;

        lastSafeArea = area;
    }
    void ApplyExceptionalSafeArea(float heightOffset)
    {
        Debug.Log("heightOffset: " + heightOffset);
        if (Exceptionals != null && Exceptionals.Count != 0)
        {
            for (int i = 0; i < Exceptionals.Count; ++i)
            {
                positionVec.Set(ExceptionalPositionInit[i].x, ExceptionalPositionInit[i].y + heightOffset, ExceptionalPositionInit[i].z);
                Exceptionals[i].transform.localPosition = positionVec;
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        // text.text = Screen.currentResolution.width + "," + Screen.currentResolution.height;
        //return;
        //  var iOSGen = UnityEngine.iOS.Device.generation;
        Rect safeArea = Screen.safeArea;
#if UNITY_EDITOR
        if (EnablePCSimulator)
        {
            CaculateRect.Set(safeArea.x + IphoneXLeftOffSet, safeArea.y + IphoneXBottomOffSet, safeArea.width - 2 * IphoneXLeftOffSet, safeArea.height - IphoneXBottomOffSet);
            if (EnableIphoneXBottomOffset)
            {
                CaculateRect.Set(CaculateRect.x, CaculateRect.y - IphoneXBottomOffSet, CaculateRect.width, CaculateRect.height + IphoneXBottomOffSet);
            }
        }
        else
#endif
        {
#if UNITY_EDITOR
            CaculateRect.Set(safeArea.x, safeArea.y, safeArea.width, safeArea.height);
#endif
#if UNITY_IOS
            if (EnableIphoneXBottomOffset)
            {
                if ((int)safeArea.height != (int)resolutionInit.height)
                {
                    IphoneXBottomOffSet = Mathf.Abs(resolutionInit.height - safeArea.height);
                    CaculateRect.Set(safeArea.x, safeArea.y - IphoneXBottomOffSet, safeArea.width, safeArea.height + IphoneXBottomOffSet);

                }
            }
            else
            {
                CaculateRect.Set(safeArea.x, safeArea.y, safeArea.width, safeArea.height);
            }
#endif
        }

        if (CaculateRect != lastSafeArea)
        {
            ApplySafeArea(CaculateRect);
            if (EnableIphoneXBottomOffset && (int)safeArea.height != (int)resolutionInit.height)
                if (Exceptionals != null && Exceptionals.Count != 0)
                    ApplyExceptionalSafeArea(IphoneXBottomOffSet);
        }
    }
}
