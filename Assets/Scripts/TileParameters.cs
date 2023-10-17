using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileParameters : MonoBehaviour
{ 
    public enum tileType
    {
        NOTHING = 0,
        HOUSE = 1,
        ROAD = 2,
        STORE = 3,
        HOSPITAL = 4,
        PARK = 5
    };

    public tileType TileType = tileType.NOTHING;
    public int tileScore = 0;
    public int tileScoreMax = 10;
    public int tileRange = 3;
    public bool tileChecked = false;

    private tileType lastTileType;

    public bool HospitalPoints = false;
    public bool ParkPoints = false;
    public bool StorePoints = false;

    [Header("Tile Colors")]
    public Color HouseColor = Color.magenta;
    public Color RoadColor = Color.black;
    public Color StoreColor = Color.cyan;
    public Color HospitalColor = Color.red;
    public Color ParkColor = Color.green;
    public Color NothingColor = Color.gray;

    Ray ray;
    RaycastHit hit;

    private void Start()
    {
        GetComponentInChildren<IconManager>().ChangeScore(tileScore);
        SetTileColor();
    }

    private void Update()
    {
        OnClick();
        if (TileType != lastTileType)
        {
            //GetComponentInParent<SetTiles>().CheckTilesInRange();
            SetTileColor();
            GetComponentInChildren<IconManager>().ChangeIcon();
            

            lastTileType = TileType;
        }
    }

    public void SetTileColor()
    {
        if(TileType == tileType.NOTHING)
        {
            transform.GetComponent<MeshRenderer>().material.color = NothingColor;
        }

        if (TileType == tileType.ROAD)
        {
            transform.GetComponent<MeshRenderer>().material.color = RoadColor;
            return;
        }

        if (TileType == tileType.HOUSE)
        {
            transform.GetComponent<MeshRenderer>().material.color = HouseColor;
            return;
        }

        if (TileType == tileType.STORE)
        {
            transform.GetComponent<MeshRenderer>().material.color = StoreColor;
            return;
        }

        if (TileType == tileType.PARK)
        {
            transform.GetComponent<MeshRenderer>().material.color = ParkColor;
            return;
        }

        if (TileType == tileType.HOSPITAL)
        {
            transform.GetComponent<MeshRenderer>().material.color = HospitalColor;
            return;
        }
    }

    public void OverrideTileColour(Color color)
    {
        transform.GetComponent<MeshRenderer>().material.color = color;
    }

    void OnClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if(hit.collider.name == this.name)
                {
                    if(TileType == tileType.PARK)
                    {
                        TileType = tileType.NOTHING;
                    }
                    else
                    {
                        TileType++;
                    }
                    //After tile has been clicked we start setting the scores again
                    GetComponentInParent<SetTiles>().SetTileScore(tileRange);
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider.name == this.name)
                {
                    if(tileScore == tileScoreMax)
                    {
                        tileScore = 0;
                    }
                    else
                    {
                        tileScore++;
                    }
                    GetComponentInChildren<IconManager>().ChangeScore(tileScore);
                }
            }
        }
    }
}
