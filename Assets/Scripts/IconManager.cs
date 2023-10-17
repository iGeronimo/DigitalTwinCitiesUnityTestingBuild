using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IconManager : MonoBehaviour
{

    public GameObject HouseSprite;
    public GameObject ParkSprite;
    public GameObject StoreSprite;
    public GameObject RoadSprite;
    public GameObject HospitalSprite;

    public TMP_Text ScoreText;

    public void ChangeIcon()
    {
        DisableIcons();
        switch (GetComponentInParent<TileParameters>().TileType)
        {
            case TileParameters.tileType.HOUSE:
                HouseSprite.SetActive(true);
                break;
            case TileParameters.tileType.PARK:
                ParkSprite.SetActive(true);
                break;
            case TileParameters.tileType.STORE:
                StoreSprite.SetActive(true);
                break;
            case TileParameters.tileType.ROAD:
                RoadSprite.SetActive(true);
                break;
            case TileParameters.tileType.HOSPITAL:
                HospitalSprite.SetActive(true);
                break;
            case TileParameters.tileType.NOTHING:
                break;
        }
    }

    public void ChangeScore(int score)
    {
        ScoreText.text = score.ToString();
    }

    private void DisableIcons()
    {
        HouseSprite.SetActive(false);
        ParkSprite.SetActive(false);
        StoreSprite.SetActive(false);
        RoadSprite.SetActive(false);
        HospitalSprite.SetActive(false);
    }
}
