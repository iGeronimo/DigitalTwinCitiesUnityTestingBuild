using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighbourTileDetection : MonoBehaviour
{

    public float overlapCheckRange = 1.1f;
    Transform currentPosition;
    TileParameters tileParameters;

    // Start is called before the first frame update
    void Start()
    {
        currentPosition = GetComponentInChildren<IconManager>().transform;
        tileParameters = GetComponent<TileParameters>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            GetNeighbourRoads();
        }
    }

    public void GetNeighbourRoads(int range = 1)
    {
        SetTiles setTiles = GetComponentInParent<SetTiles>();
        //Check if there is range left to check further
        if (range > 0)
        {
            //Get all surrounding tiles
            Collider[] hitTiles = Physics.OverlapSphere(currentPosition.position, overlapCheckRange);
            foreach (Collider hitTile in hitTiles)
            {
                //Tileparameter script check
                if (hitTile.GetComponent<TileParameters>() == null)
                {
                    Debug.Log("Tile does not have TileParameters Script");
                    continue;
                }

                TileParameters hitTileParameters = hitTile.GetComponent<TileParameters>();

                //Check if tile is a road
                if (hitTileParameters.TileType != TileParameters.tileType.ROAD)
                {
                    Debug.Log("Tile is not a road piece");
                    continue;
                }

                //Check if road was already checked
                if(hitTileParameters.tileChecked == true)
                {
                    Debug.Log("Road piece has already been checked");
                    continue;
                }

                //Add tile to tiles to check list
                setTiles.AddToTileList(hitTileParameters);
            }
        }
    }

    public TileParameters[] GetTileOfType(TileParameters.tileType TypeOfTile)
    {
        List<TileParameters> tileList = new List<TileParameters>();

        //Get all surrounding tiles
        Collider[] hitTiles = Physics.OverlapSphere(currentPosition.position, overlapCheckRange);
        foreach (Collider hitTile in hitTiles)
        {
            //Tileparameter script check
            if (hitTile.GetComponent<TileParameters>() == null)
            {
                Debug.Log("Tile does not have TileParameters Script");
                continue;
            }

            TileParameters hitTileParameters = hitTile.GetComponent<TileParameters>();

            //Check if tile is a road
            if (hitTileParameters.TileType != TypeOfTile)
            {
                Debug.Log("Tile is not of correct type");
                continue;
            }
            tileList.Add(hitTileParameters);
        }

        return tileList.ToArray();
    }
}
