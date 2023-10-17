using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HousePoints : MonoBehaviour
{
    public int CurrentScore = 0;

    public TMP_Text ScoreText; 

    public void CountHousePoints()
    {
        CurrentScore = 0;
        //Get all tileparameters
        TileParameters[] childParameters = GetComponentsInChildren<TileParameters>();

        foreach (TileParameters house in childParameters)
        {
            if(house.TileType == TileParameters.tileType.HOUSE)
            {
                //Get nearby roads
                TileParameters[] neighbourRoads = house.GetComponent<NeighbourTileDetection>().GetTileOfType(TileParameters.tileType.ROAD);

                int highestPoints = 0;
                foreach(TileParameters roadNextToHouse in neighbourRoads)
                {
                    if(roadNextToHouse.tileScore > highestPoints)
                    {
                        highestPoints = roadNextToHouse.tileScore;
                    }
                }

                CurrentScore += highestPoints;
            }
        }

        ScoreText.text = "Score: " + CurrentScore;
    }
}
