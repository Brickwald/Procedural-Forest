using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundFunctions : MonoBehaviour
{ 
  public int initialNrOfTrees = 100;
  public int groundSize = 256;
  public int textureSize = 256;
  public float groundNoiseScale = 10.0f;
  public float waterNoiseScale = 100.0f;

  public float hillAmplitude = 10.0f;
  public float elevationMult = 0.10f;

  public float sloapMult = 0.01f;

  //available species of trees
  public ProdTree[] treeSpecies;

  Mesh mesh;
  Vector3[] vertices;
  Vector3[] normals;
  int[] triangles;
  Vector2[] uvs;

  //Actual trees on the ground
  List<ProdTree> trees = new List<ProdTree>();

  // Start is called before the first frame update
  void Start()
  {
    mesh = new Mesh();
    GetComponent<MeshFilter>().mesh = mesh;
    CreateGround();
    UpdateMesh();
    GenerateWater();

    PlaceTrees(initialNrOfTrees);
  }

  public void TimeStep(){
    GenerateWater();
    TreesDrinkWater();
  }

  void UpdateMesh(){
    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals();
    normals = mesh.normals;
  }

  void CreateGround(){

    vertices = new Vector3[(groundSize + 1) * (groundSize + 1)];

    Random.InitState((int)System.DateTime.Now.Ticks);
    float rand = Random.value * 100;

    for(int i=0, z=0; z<=groundSize;z++){
      for(int x=0; x<=groundSize;x++){

        float xCoord = rand + ((float)x) / (float)groundSize * groundNoiseScale;
        float zCoord = rand + ((float)z) / (float)groundSize * groundNoiseScale;
        float sample = Mathf.PerlinNoise(xCoord, zCoord);
        vertices[i] = new Vector3(x, hillAmplitude*sample, z);
        i++;
      }
    }

    triangles = new int[groundSize * groundSize * 6];
    int vert = 0;
    int tris = 0;
    for(int z=0;z<groundSize;z++){
      for(int x=0;x<groundSize;x++){
        
        triangles[tris] = vert;
        triangles[tris + 1] = vert + groundSize + 1;
        triangles[tris + 2] = vert + 1;
        triangles[tris + 3] = vert + 1;
        triangles[tris + 4] = vert + groundSize + 1;
        triangles[tris + 5] = vert + groundSize + 2;

        vert++;
        tris += 6;
      }
      vert++;
    }

    uvs = new Vector2[vertices.Length];

    for(int i=0, z=0; z<=groundSize;z++){
      for(int x=0; x<=groundSize;x++){
        uvs[i] = new Vector2((float)x/groundSize, (float)z/groundSize);
        i++;
      }
    }
  }

  public void GenerateWater(){
    Material material = GetComponent<Renderer>().material;

    // Create a new texture ARGB32 (32 bit with alpha) and no mipmaps
    var texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);

    float rain = 0.0f;
    bool isRaining = GameObject.Find("Canvas").GetComponent<UiFunctions>().IsRaining();

    if(isRaining) rain = GameObject.Find("Canvas").GetComponent<UiFunctions>().GetRain();

    // set the pixel values
    Random.InitState((int)System.DateTime.Now.Ticks);
    float rand = Random.value * 100;

    for(int y = 0; y < textureSize; y++){
      for(int x = 0; x < textureSize; x++){
        float xCoord = rand + ((float)x) / (float)textureSize * waterNoiseScale;
        float yCoord = rand + ((float)y) / (float)textureSize * waterNoiseScale;
        float sample = Mathf.PerlinNoise(xCoord, yCoord) + rain;

        if(sample <= 0.0f) sample = 0.0f;

        texture.SetPixel(x, y, new Color(0, sample, 0, 1.0f));
      }
    }

    float maxHeight = 0.0f;
    float minHeight = 10000.0f;
    for(int i=0;i<vertices.Length;i++){
      int pixBetwVerts = Mathf.FloorToInt(textureSize/groundSize);
      int xCoord = Mathf.FloorToInt(uvs[i].x * (float)textureSize);
      int yCoord = Mathf.FloorToInt(uvs[i].y * (float)textureSize);
      float heightFactor = (vertices[i].y / hillAmplitude) * elevationMult;

      float sloapFactor = (Vector3.Angle(Vector3.up, normals[i]) / 90.0f) * sloapMult;

      for(int k=xCoord-pixBetwVerts;k<xCoord+pixBetwVerts;k++){
        if(k<0 || k>=textureSize) continue;
        for(int l=yCoord-pixBetwVerts;l<yCoord+pixBetwVerts;l++){
          if(l<0 || l>=textureSize) continue;

          Color pix = texture.GetPixel(k, l);
          pix.g = pix.g - sloapFactor - heightFactor;

          texture.SetPixel(k,l,pix);
        }
      }
      if(vertices[i].y > maxHeight) maxHeight = vertices[i].y;
      if(vertices[i].y < minHeight) minHeight = vertices[i].y;
    }

    Debug.Log("MaxHeight = " + maxHeight + ", minHeight = " + minHeight);

    // Apply all SetPixel calls
    texture.Apply();

    // connect texture to material of GameObject this script is attached to
    material.mainTexture = texture;
    
  }

  void PlaceTrees(int nrOfTrees){

    for(int i=0;i<nrOfTrees;i++){
      ProdTree new_tree;

      //randomise species
      Random.InitState((int)System.DateTime.Now.Ticks * (1+i));
      int rand = Mathf.FloorToInt(Random.value * treeSpecies.Length);
      new_tree = Instantiate(treeSpecies[rand]);
      new_tree.speciesIndex = rand;

      //randomise position
      bool has_been_placed = false;
      while(!has_been_placed){
        Random.InitState((int)System.DateTime.Now.Ticks + i);
        int rand2 = Mathf.FloorToInt(Random.value * vertices.Length);
        bool occupied = false;
        for(int j=0;j<trees.Count;j++)
        {
          if(trees[j].getPositionOnGround() == rand2)
          {
            occupied = true;
            break;
          }
        }

        if(!occupied){
          new_tree.setPositionOnGround(rand2);
          has_been_placed = true;
        }    
      }
      trees.Add(new_tree);
    }

    //place trees int the world
    for(int i=0;i<trees.Count;i++){
      trees[i].transform.position = vertices[trees[i].getPositionOnGround()];
    }
  }

    void PlaceTrees(int nrOfTrees, int species){

    for(int i=0;i<nrOfTrees;i++){
      ProdTree new_tree;

      new_tree = Instantiate(treeSpecies[species]);
      new_tree.speciesIndex = species;

      //randomise position
      bool has_been_placed = false;
      while(!has_been_placed){
        Random.InitState((int)System.DateTime.Now.Ticks + i);
        int rand2 = Mathf.FloorToInt(Random.value * vertices.Length);
        bool occupied = false;
        for(int j=0;j<trees.Count;j++)
        {
          if(trees[j].getPositionOnGround() == rand2)
          {
            occupied = true;
            break;
          }
        }

        if(!occupied){
          new_tree.setPositionOnGround(rand2);
          has_been_placed = true;
        }    
      }
      trees.Add(new_tree);
    }

    //place trees int the world
    for(int i=0;i<trees.Count;i++){
      trees[i].transform.position = vertices[trees[i].getPositionOnGround()];
    }
  }

  void TreesDrinkWater()
  {
    Material material = GetComponent<Renderer>().material;
    Texture2D texture = (Texture2D)material.mainTexture;

    //randomize order the trees get to drink
    ProdTree[] shuffled_trees = trees.ToArray();
    ShuffleArray(shuffled_trees);

    float totalWaterDrunk = 0.0f;

    //trees get as much water as they can
    foreach(ProdTree tree in shuffled_trees)
    {
      int rootLength = tree.getRootLength();
      int xCoord = Mathf.FloorToInt(uvs[tree.getPositionOnGround()].x * (float)textureSize);
      int yCoord = Mathf.FloorToInt(uvs[tree.getPositionOnGround()].y * (float)textureSize);
      float water = 0.0f;

      for(int k=xCoord-rootLength;k<xCoord+rootLength;k++){
        if(k<0 || k>=textureSize) continue;
        for(int l=yCoord-rootLength;l<yCoord+rootLength;l++){
          if(l<0 || l>=textureSize) continue;

          int dist = Mathf.Abs(xCoord - k) + Mathf.Abs(yCoord - l);
          float amountToDrink = 1.0f - (float)dist/(float)rootLength;

          if(amountToDrink <= 0.0f) amountToDrink = 0.0f;
          if(amountToDrink >= tree.maxDrinking) amountToDrink = tree.maxDrinking;

          Color pix = texture.GetPixel(k, l);

          if(amountToDrink > pix.g)
          {
            water += pix.g;
            pix.g = 0.0f;
          } 
          else 
          {
            water += amountToDrink;
            pix.g -= amountToDrink;
          }

          texture.SetPixel(k,l,pix);
        }
      }
      water += tree.getCurrentWater();
      tree.setCurrentWater(0.0f);
      totalWaterDrunk += water;

      if(water < tree.getRequiredWater()){
        //tree dies 
        trees.Remove(tree);
        Object.Destroy(tree.gameObject);
      }else if(water >= tree.getRequiredWater() + tree.waterReqPerSize){
        
        //grow
        tree.GrowTree();

        //reproduce
        float extra = water - (tree.getRequiredWater() + tree.waterReqPerSize);
        int nrOfSeeds = Mathf.FloorToInt(extra/tree.waterReqPerSize);
        PlaceTrees(nrOfSeeds, tree.speciesIndex);
        //save water
        extra -= (float)nrOfSeeds * tree.waterReqPerSize;
        if(extra <= 0.0f) extra = 0.0f;
        if(extra >= tree.getMaxSavedWater()) extra = tree.getMaxSavedWater();
        tree.setCurrentWater(extra);
        //Debug.Log("This tree grew and saved: " + extra);
      } else {
        //save water
        float savedWater = water - tree.getRequiredWater();
        if(savedWater >= tree.getMaxSavedWater()) savedWater = tree.getMaxSavedWater();
        tree.setCurrentWater(savedWater);
        //Debug.Log("This tree  did not grow and saved: " + savedWater);
      }
    }

    Debug.Log("Total water drunk = " + totalWaterDrunk);
    Debug.Log("Average water drunk = " + totalWaterDrunk/trees.Count);

    texture.Apply();
    material.mainTexture = texture;
  }

  void ShuffleArray<T>(T[] array)
  {
    int n = array.Length;
    for (int i = 0; i < n; i++) {
      // Pick a new index higher than current for each item in the array
      int r = i + Random.Range(0, n - i);
      
      // Swap item into new spot
      T t = array[r];
      array[r] = array[i];
      array[i] = t;
    }
  }

  public int getTreeCount(){return trees.Count;}
}


