using System.Collections;
using System.Collections.Generic;
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

    public tileType TileType = tileType.ROAD;
    public int tileScore = 0;
    public int tileScoreMax = 10;

    private tileType lastTileType;

    [Header("Tile Colors")]
    public Color HouseColor = Color.magenta;
    public Color RoadColor = Color.white;
    public Color StoreColor = Color.cyan;
    public Color HospitalColor = Color.red;
    public Color ParkColor = Color.green;
    public Color NothingColor = Color.gray;

    Ray ray;
    RaycastHit hit;

    private void Start()
    {
        GetComponentInChildren<IconManager>().ChangeScore(tileScore);
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
            GetComponent<Transform>().GetComponent<MeshRenderer>().material.color = NothingColor;
        }

        if (TileType == tileType.ROAD)
        {
            GetComponent<Transform>().GetComponent<MeshRenderer>().material.color = RoadColor;
            return;
        }

        if (TileType == tileType.HOUSE)
        {
            GetComponent<Transform>().GetComponent<MeshRenderer>().material.color = HouseColor;
            return;
        }

        if (TileType == tileType.STORE)
        {
            GetComponent<Transform>().GetComponent<MeshRenderer>().material.color = StoreColor;
            return;
        }

        if (TileType == tileType.PARK)
        {
            GetComponent<Transform>().GetComponent<MeshRenderer>().material.color = ParkColor;
            return;
        }

        if (TileType == tileType.HOSPITAL)
        {
            GetComponent<Transform>().GetComponent<MeshRenderer>().material.color = HospitalColor;
            return;
        }
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
