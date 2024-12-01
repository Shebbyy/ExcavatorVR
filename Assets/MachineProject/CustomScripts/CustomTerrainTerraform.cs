using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    // This Class was created based on the blog post here: https://discussions.unity.com/t/paint-terrain-texture-on-runtime/236566/2 All props go to @qtoompuu for implementing the terrain transformation and explaining it in a usable manor, some minor changes were made 
    // to accomodate for this Demos usecase, so it works with colliders instead of raycasts and input buttons;
    // The Major changes are mostly in the Update Function, some smaller changes in Awake and some unneeded methods were removed
    public class CustomTerrainTerraform : MonoBehaviour
    {
        public enum EffectType
        {
            raise,
            lower
        };
        public int areaOfEffectSize = 100; // size of the brush
        public float strength; // brush strength
        
        private Terrain targetTerrain; //The terrain obj you want to edit
        private int terrainHeightMapWidth; //Used to calculate click position
        private int terrainHeightMapHeight;
        private float[,] heights; //a variable to store the new heights
        private TerrainData targetTerrainData; // stores the terrains terrain data
        private float[,] brush; // this stores the brush.png pixel data
        private BoxCollider shovelCollider;
        private float[,,] splat; // A splat map is what unity uses to overlay all of your paints on to the terrain
        
        public float[,] GenerateBrush(int size)
        {
            float[,] heightMap = new float[size,size];//creates a 2d array which will store our brush
            Texture2D scaledBrush = ResizeBrush(size,size); // this calls a function which we will write next, and resizes the brush image
            //This will iterate over the entire re-scaled image and convert the pixel color into a value between 0 and 1
            for (int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    Debug.Log("x, y: " + x + ";" + y);
                    Color pixelValue = scaledBrush.GetPixel(x, y);
                    heightMap[x, y] = pixelValue.grayscale / 255;
                }
            }
    
            return heightMap;
        }
        
        public Terrain GetTerrainAtObject(GameObject gameObject)
        {
            if (gameObject.GetComponent<Terrain>())
            {
                //This will return the Terrain component of an object (if present)
                return gameObject.GetComponent<Terrain>();
            }

            return default(Terrain);
        }
        
        public static Texture2D ResizeBrush(int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }
        
        public TerrainData GetCurrentTerrainData()
        {
            if (targetTerrain)
            {
                return targetTerrain.terrainData;
            }

            return default(TerrainData);
        }
        
        public void SetEditValues(Terrain terrain)
        {
            targetTerrainData      = GetCurrentTerrainData();
            heights                = GetCurrentTerrainHeightMap();
            terrainHeightMapWidth  = GetCurrentTerrainWidth();
            terrainHeightMapHeight = GetCurrentTerrainHeight();
        }
        
        private void GetTerrainCoordinates(RaycastHit hit, out int x,out int z)
        {
            int offset = areaOfEffectSize / 2; //This offsets the hit position to account for the size of the brush which gets drawn from the corner out
            //World Position Offset Coords, these can differ from the terrain coords if the terrain object is not at (0,0,0)
            Vector3 tempTerrainCoodinates = hit.point - hit.transform.position;
            //This takes the world coords and makes them relative to the terrain
            Vector3 terrainCoordinates = new Vector3(
                tempTerrainCoodinates.x / GetTerrainSize().x,
                tempTerrainCoodinates.y / GetTerrainSize().y,
                tempTerrainCoodinates.z / GetTerrainSize().z);

            // This will take the coords relative to the terrain and make them relative to the height map(which often has different dimensions)
            Vector3 locationInTerrain = new Vector3
            (
                terrainCoordinates.x * terrainHeightMapWidth,
                0,
                terrainCoordinates.z * terrainHeightMapHeight
            );

            //Finally, this will spit out the X Y values for use in other parts of the code
            x = (int)locationInTerrain.x - offset;
            z = (int)locationInTerrain.z - offset;
        }
        
        public Vector3 GetTerrainSize()
        {
            if (targetTerrain)
            {
                return targetTerrain.terrainData.size;
            }

            return Vector3.zero;
        }
        
        public int GetCurrentTerrainWidth()
        {
            if (targetTerrain)
            {
                return targetTerrain.terrainData.heightmapResolution;
            }

            return 0;
        }

        public int GetCurrentTerrainHeight()
        {
            if (targetTerrain)
            {
                return targetTerrain.terrainData.heightmapResolution;
            }

            return 0;
            //test2.GetComponent<MeshRenderer>().material.mainTexture = texture;
        }

        void ModifyTerrain(int x, int z, EffectType effectType)
        {
            //These AreaOfEffectModifier variables below will help us if we are modifying terrain that goes over the edge, you will see in a bit that I use Xmod for the the z(or Y) values, which was because I did not realize at first that the terrain X and world X is not the same so I had to flip them around and was too lazy to correct the names, so don't get thrown off by that.
            int AOExMod = 0;
            int AOEzMod = 0;

            int AOExMod1 = 0;
            int AOEzMod1 = 0;

            if (x < 0) // if the brush goes off the negative end of the x axis we set the mod == to it to offset the edited area
            {
                AOExMod = x;
            }
            else if (x + areaOfEffectSize >
                     terrainHeightMapWidth) // if the brush goes off the posative end of the x axis we set the mod == to this
            {
                AOExMod1 = x + areaOfEffectSize - terrainHeightMapWidth;
            }

            if (z < 0) //same as with x
            {
                AOEzMod = z;
            }
            else if (z + areaOfEffectSize > terrainHeightMapHeight)
            {
                AOEzMod1 = z + areaOfEffectSize - terrainHeightMapHeight;
            }
            
            ///Raise Terrain
            if (effectType == EffectType.raise)
            {
                for (int xx = 0; xx < areaOfEffectSize + AOEzMod - AOEzMod1; xx++)
                {
                    for (int yy = 0; yy < areaOfEffectSize + AOExMod - AOExMod1; yy++)
                    {
                        
                        heights[xx, yy] +=
                            brush[xx - AOEzMod, yy - AOExMod] *
                            strength; //for each point we raise the value  by the value of brush at the coords * the strength modifier
                    }
                }

                targetTerrainData.SetHeights(x - AOExMod, z - AOEzMod,
                    heights); // This bit of code will save the change to the Terrain data file, this means that the changes will persist out of play mode into the edit mode
            }
            /// Lower Terrain, just the reverse of raise terrain
            else if (effectType == EffectType.lower)
            {
                for (int xx = 0; xx < areaOfEffectSize + AOEzMod; xx++)
                {
                    for (int yy = 0; yy < areaOfEffectSize + AOExMod; yy++)
                    {
                        heights[xx, yy] -= brush[xx - AOEzMod, yy - AOExMod] * strength;
                    }
                }
                
                Debug.Log("x:" + x + "; z:" + z);
                Debug.Log("xMod:" + AOExMod + "; zMod:" + AOEzMod);
                Debug.Log("xBase:" + (x - AOExMod) + "; yBase:" + (z - AOEzMod));
                targetTerrainData.SetHeights(x - AOExMod, z - AOEzMod, heights);
            }
        }
        
        public float[,] GetCurrentTerrainHeightMap()
        {
            if (targetTerrain)
            {
                // the first 2 0's indicate the coords where we start, the next values indicate how far we extend the area, so what we are saying here is I want the heights starting at the Origin and extending the entire width and height of the terrain
                return targetTerrain.terrainData.GetHeights(0, 0, 
                    targetTerrain.terrainData.heightmapResolution, 
                    targetTerrain.terrainData.heightmapResolution);
            }

            return default(float[,]);
        }

        void Awake()
        {
            brush = GenerateBrush(areaOfEffectSize); // This will take the brush image from our array and will resize it to the area of effect
            targetTerrain = FindObjectOfType<Terrain>(); // this will find terrain in your scene, alternatively, if you know you will only have one terrain, you can make it a public variable and assign it that way
            shovelCollider = GetComponent<BoxCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            Transform shovelPos = shovelCollider.transform;
            RaycastHit hit;
            if(Physics.Raycast(shovelPos.position, -shovelPos.forward, out hit))
            {
                // Only Handle Terrain hits
                if (   hit.collider.gameObject.name != "Terrain"
                    || hit.distance > 5) {
                    return;
                }
                //Debug.Log("Hit Something: " + hit.collider.gameObject.name + ", Distance: " + hit.distance);
                EffectType action = EffectType.lower;
                if (hit.distance > 2f)
                {
                    action = EffectType.raise;
                }
                
                targetTerrain = GetTerrainAtObject(hit.transform.gameObject);
                SetEditValues(targetTerrain);
                
                GetTerrainCoordinates(hit, out int terX, out int terZ);
                ModifyTerrain(terX, terZ, action);
            }
        }
    }
}

