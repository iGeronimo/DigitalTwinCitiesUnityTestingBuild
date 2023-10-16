using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SetTiles : MonoBehaviour
{
    Transform[] tiles;
    public int rows;
    public int cols;
    public int tileRange;

    private void Start()
    {
        CheckTilesInRange();
    }

    public void CheckTilesInRange()
    {
        tiles = GetComponentsInChildren<Transform>();

        //Reset tile score to keep points up to date whilst playing
        foreach (Transform tile in tiles)
        {
            if (int.TryParse(tile.name, out int tileNumber))
            {
                TileParameters resetTileData = tile.GetComponent<TileParameters>();
                resetTileData.tileScore = 0;
            }
        }

        //Check where in the row the cell is
        foreach (Transform tile in tiles)
        {
            if (int.TryParse(tile.name, out int tileNumber))
            {
                TileParameters tileData = tile.GetComponent<TileParameters>();
                if (tileData.TileType == TileParameters.tileType.ROAD)
                {
                    continue;
                }
                if (tileData.TileType == TileParameters.tileType.HOUSE)
                {
                    continue;
                }

                foreach (Transform tileToCheck in tiles)
                {
                    if (int.TryParse(tileToCheck.name, out int tileToCheckNumber))
                    {
                        TileParameters tileToCheckData = tileToCheck.GetComponent<TileParameters>();
                        if (tileToCheckData.TileType == TileParameters.tileType.ROAD)
                        {
                            //Check if in height range
                            if (Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) > tileRange)
                            {
                                continue;
                            }
                            //Check if in width range
                            if (Mathf.Abs(GetColNumber(tileNumber) - GetColNumber(tileToCheckNumber)) > tileRange)
                            {
                                continue;
                            }
                            //Check if we are in uneven row distance since uneven has special rules
                            if (Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) % 2 != 0)
                            {
                                //Check if starting row is even
                                if (GetRowNumber(tileNumber) % 2 == 0)
                                {
                                    //Check if we are bigger than target, otherwise get extra tile range for fork in pathing
                                    if (GetColNumber(tileNumber) > GetColNumber(tileToCheckNumber))
                                    {
                                        //Check if with this extra range we reach
                                        if (Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) + Mathf.Abs(GetColNumber(tileNumber) - GetColNumber(tileToCheckNumber)) > tileRange + 1 + ExtraRange(tileNumber, tileToCheckNumber))
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) + Mathf.Abs(GetColNumber(tileNumber) - GetColNumber(tileToCheckNumber)) > tileRange + ExtraRange(tileNumber, tileToCheckNumber))
                                        {
                                            continue;
                                        }
                                    }
                                }
                                //Else starting row is uneven
                                else
                                {
                                    if (GetColNumber(tileNumber) > GetColNumber(tileToCheckNumber))
                                    {
                                        if (Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) + Mathf.Abs(GetColNumber(tileNumber) - GetColNumber(tileToCheckNumber)) > tileRange + ExtraRange(tileNumber, tileToCheckNumber))
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) + Mathf.Abs(GetColNumber(tileNumber) - GetColNumber(tileToCheckNumber)) > tileRange + ExtraRange(tileNumber, tileToCheckNumber) + 1)
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                            //Check for when we are an even amount of rows away
                            else
                            {
                                if (Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) + Mathf.Abs(GetColNumber(tileNumber) - GetColNumber(tileToCheckNumber)) > tileRange + ExtraRange(tileNumber, tileToCheckNumber))
                                {
                                    continue;
                                }
                            }
                            //If we got through all possible checks we give +1 points to tile
                            tileToCheckData.tileScore++;
                        }
                    }
                }
            }
        }
        //Change the color of the tiles
        foreach (Transform tile in tiles)
        {
            if (int.TryParse(tile.name, out int tileNumber))
            {
                tile.GetComponent<TileParameters>().SetTileColor();
            }
        }
        //Set score for the houses based on the highest score they touch
        foreach(Transform tile in tiles)
        {
            if (int.TryParse(tile.name, out int tileNumber))
            {
                TileParameters tileData = tile.GetComponent<TileParameters>();
                {
                    if(tileData.TileType == TileParameters.tileType.HOUSE)
                    {
                        foreach (Transform tileToCheck in tiles)
                        {
                           
                        }
                    }
                }
            }
        }
    }

    private int GetRowNumber(int tileNumber)
    {
        return Mathf.FloorToInt((tileNumber + cols) / cols);
    }

    private int GetColNumber(int tileNumber)
    {
        return tileNumber % cols;
    }

    private int ExtraRange(int tileNumber, int tileToCheckNumber)
    {
        return Mathf.FloorToInt(Mathf.Abs(GetRowNumber(tileNumber) - GetRowNumber(tileToCheckNumber)) / 2.0f);
    }
}
