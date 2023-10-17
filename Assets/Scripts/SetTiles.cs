using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SetTiles : MonoBehaviour
{
    public int rows;
    public int cols;

    List<TileParameters> tileList;
    List<TileParameters> lastTileList;

    private void Start()
    {
        tileList = new List<TileParameters>();
        lastTileList = new List<TileParameters>();
    }

    public void SetTileScore(int range)
    {
        //Get all tileparameters
        TileParameters[] childParameters = GetComponentsInChildren<TileParameters>();

        //Set all scores to 0
        foreach(TileParameters roads in childParameters)
        {
            if(roads.TileType == TileParameters.tileType.ROAD)
            {
                Debug.Log("Resetting " + roads.name + " to 0");
                roads.tileScore = 0;
                roads.HospitalPoints = false;
                roads.StorePoints = false;
                roads.ParkPoints = false;
            }
        }

        foreach (TileParameters param in childParameters)
        {
            //Check if building type is amenity
            if (param.TileType == TileParameters.tileType.NOTHING ||
                param.TileType == TileParameters.tileType.HOUSE ||
                param.TileType == TileParameters.tileType.ROAD)
            {
                continue;
            }
            //Get The neighbouring tiles of the amenity tile
            param.GetComponent<NeighbourTileDetection>().GetNeighbourRoads(range);

            //After we've gotten the tiles to check we go over them and delete dupes
            tileList = tileList.Distinct().ToList();

            //We add the score and set them to checked
            foreach (TileParameters tile in tileList)
            {
                if (param.TileType == TileParameters.tileType.HOSPITAL)
                {
                    if (tile.HospitalPoints == false)
                    {
                        tile.tileScore++;
                        tile.HospitalPoints = true;
                    }
                }
                else if (param.TileType == TileParameters.tileType.PARK)
                {
                    if (tile.ParkPoints == false)
                    {
                        tile.tileScore++;
                        tile.ParkPoints = true;
                    }
                }
                else if (param.TileType == TileParameters.tileType.STORE)
                {
                    if (tile.StorePoints == false)
                    {
                        tile.tileScore++;
                        tile.StorePoints = true;
                    }
                }
                tile.tileChecked = true;
            }

            //Copy the last checked list of tiles to a new list
            lastTileList = new List<TileParameters>(tileList);

            //Empty the tile list
            tileList.Clear();

            Debug.Log(lastTileList + " tile data");

            //Now keep doing this for the rest of the range
            for(int i = range -1; i > 0; i--)
            {
                foreach(TileParameters roadTile in lastTileList)
                {
                    roadTile.GetComponent<NeighbourTileDetection>().GetNeighbourRoads(i);
                }
                foreach(TileParameters tile in tileList.Distinct())
                {
                    if (param.TileType == TileParameters.tileType.HOSPITAL)
                    {
                       if(tile.HospitalPoints == false)
                        {
                            tile.tileScore++;
                            tile.HospitalPoints = true;
                        }
                    }
                    else if (param.TileType == TileParameters.tileType.STORE)
                    {
                        if (tile.StorePoints == false)
                        {
                            tile.tileScore++;
                            tile.StorePoints = true;
                        }
                    }
                    else if (param.TileType == TileParameters.tileType.PARK)
                    {
                        if (tile.ParkPoints == false)
                        {
                            tile.tileScore++;
                            tile.ParkPoints = true;
                        }
                    }
                    tile.tileChecked = true;
                }
                //Empty checked tiles
                lastTileList.Clear();
                //Set new checked tiles
                lastTileList = new List<TileParameters>(tileList);
                //Clear list for next loop
                tileList.Clear();
            }

            //Set all the tileChecks back to true for next loop
            foreach(TileParameters enableTiles in childParameters)
            {
                enableTiles.tileChecked = false;
            }
        }

        //At the end set all the scores properly and reset the aminity checks
        foreach (TileParameters TileScores in childParameters)
        {
            TileScores.GetComponentInChildren<IconManager>().ChangeScore(TileScores.tileScore);
        }
        //Set the Overal score
        GetComponent<HousePoints>().CountHousePoints();
    }

    //Add a tile to the checklist
    public void AddToTileList(TileParameters newTile)
    {
        tileList.Add(newTile);
    }
}
