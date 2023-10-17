using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SaveLoadState : MonoBehaviour
{
    List<TileParameters.tileType> childParameters;
    List<TileParameters.tileType> currentSaveState;


    private void Start()
    {
        childParameters = new List<TileParameters.tileType>();
        currentSaveState = new List<TileParameters.tileType>();
    }

    public void SaveState()
    {
        currentSaveState.Clear();
        childParameters.Clear();
        foreach (TileParameters tile in GetComponentsInChildren<TileParameters>())
        {
            childParameters.Add(tile.TileType);
        }

        List<TileParameters.tileType> stateList = new List<TileParameters.tileType>();
        foreach (TileParameters.tileType child in childParameters)
        {
            stateList.Add(child);
        }
        currentSaveState = new List<TileParameters.tileType>(stateList);
        stateList.Clear();
    }

    public void LoadState()
    {
        int i = 0;
        foreach(TileParameters tile in GetComponentsInChildren<TileParameters>())
        {
            tile.TileType = currentSaveState[i];
            i++;
        }
    }
}
