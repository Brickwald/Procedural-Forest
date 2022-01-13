using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProdTree : MonoBehaviour
{
  public int maxSize = 10;
  public float waterReqPerSize = 150.0f;
  public int rootLengthPerSize = 10;
  public float maxDrinking = 1.0f;
  public float maxSavedWaterPerSize = 0.0f;
  public int speciesIndex = 0;

  public float baseScale = 2.0f;

  //Index of ground vertex this tree is placed on
  int positionOnGround;
  int currentSize = 1;

  float currentWaterAmount = 0.0f;

  public void setPositionOnGround(int p){positionOnGround = p;}
  public int getPositionOnGround(){return positionOnGround;}

  public void setCurrentWater(float w){currentWaterAmount = w;}
  public float getCurrentWater(){return currentWaterAmount;}

  public int getRootLength(){return rootLengthPerSize*currentSize;}

  public float getRequiredWater(){return waterReqPerSize * (float)currentSize;}
  public float getMaxSavedWater(){return maxSavedWaterPerSize * (float)currentSize;}
  
  void Start(){
      Transform trunk = transform.Find("Trunk");
      float scale = (float)currentSize / (float)maxSize;
      trunk.localScale = new Vector3(scale, scale, scale) * baseScale;
  }
  public void GrowTree()
  {
    if(currentSize < maxSize){
      currentSize++;
      Transform trunk = transform.Find("Trunk");
      float scale = (float)currentSize / ((float)maxSize/2.0f);
      trunk.localScale = new Vector3(scale, scale, scale) * baseScale;
    }
  }

  //Old version
  /*
  public void GrowTree()
  {
    if(currentSize < maxSize){
      Transform new_trunk = Instantiate(transform.Find("Trunk"));
      new_trunk.transform.position = transform.position + Vector3.up * currentSize;
      new_trunk.parent = gameObject.transform;
      currentSize++;
    }
  }
  */
}
