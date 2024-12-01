using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    // This Class was created based on the blog post here: https://discussions.unity.com/t/paint-terrain-texture-on-runtime/236566/2 All props go to @qtoompuu for implementing the terrain transformation and explaining it in a usable manor, some minor changes were made 
    // to accomodate for this Demos usecase, so it works with colliders instead of raycasts and input buttons
    public class CustomTerrainTerraform : MonoBehaviour
    {
        //place these where you would normally declare variables
        Terrain targetTerrain; //The terrain obj you want to edit
        int terrainHeightMapWidth; //Used to calculate click position
        int terrainHeightMapHeight;
        float[,] heights; //a variable to store the new heights
        TerrainData targetTerrainData; // stores the terrains terrain data
        public enum EffectType
        {
            raise,
            lower
        };

        public Texture2D[] brushIMG; // This will allow you to switch brushes
        float[,] brush; // this stores the brush.png pixel data
        public int brushSelection; // current selected brush
        public int areaOfEffectSize = 100; // size of the brush
        [Range(0.01f,1f)] // you can remove this if you want
        public float strength; // brush strength
        float[,,] splat; // A splat map is what unity uses to overlay all of your paints on to the terrain
        
        public Terrain GetTerrainAtObject(GameObject gameObject)
        {
            if (gameObject.GetComponent<Terrain>())
            {
                //This will return the Terrain component of an object (if present)
                return gameObject.GetComponent<Terrain>();
            }

            return default(Terrain);
        }
        
        public float[,] GenerateBrush(Texture2D texture, int size)
        {
            float[,] heightMap = new float[size,size];//creates a 2d array which will store our brush
            Texture2D scaledBrush = ResizeBrush(texture,size,size); // this calls a function which we will write next, and resizes the brush image
            //This will iterate over the entire re-scaled image and convert the pixel color into a value between 0 and 1
            for (int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    Color pixelValue = scaledBrush.GetPixel(x, y);
                    heightMap[x, y] = pixelValue.grayscale / 255;
                }
            }
    
            return heightMap;
        }
        
        public static Texture2D ResizeBrush(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            _gpu_scale(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }
        
        static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(width, height, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
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
            targetTerrainData = GetCurrentTerrainData();
            terrainHeightMapWidth = GetCurrentTerrainWidth();
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

                targetTerrainData.SetHeights(x - AOExMod, z - AOEzMod, heights);
            }
        }

        void Awake()
        {
            brush = GenerateBrush(brushIMG[brushSelection], areaOfEffectSize); // This will take the brush image from our array and will resize it to the area of effect
            targetTerrain = FindObjectOfType<Terrain>(); // this will find terrain in your scene, alternatively, if you know you will only have one terrain, you can make it a public variable and assign it that way
        }

        // Update is called once per frame
        void Update()
        {
            //if (Input.GetMouseButtonDown(0))
            //{
            //    Transform cam = Camera.main.transform;
            //    Ray ray = new Ray(cam.position, cam.forward);
            //    RaycastHit hit;
            //    if(Physics.Raycast (ray, hit, 500))
            //    {
            //        targetTerrain = GetTerrainAtObject(hit.transform.gameObject);
            //        SetEditValues(targetTerrain);
//
            //        GetTerrainCoordinates(hit, out int terX, out int terZ);
            //        ModifyTerrain(terX, terZ, EffectType.raise);
            //    }
            //}
        }

    }
}

