using System;
using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    public class CustomTerrainTerraform : MonoBehaviour
    {
        [SerializeField] 
        [Tooltip("The width of the brush.")]
        public int brushWidth;
        
        [SerializeField] 
        [Tooltip("The height of the brush.")]
        public int brushHeight;
        
        [SerializeField] 
        [Tooltip("The strength of the brush.")]
        public float strength = 0.05f;
        
        private Terrain _targetTerrain;
        private TerrainData _targetTerrainData;
        private enum TerrainModificationAction
        {
            Raise,
            Lower
        }
        
        // The Brain of the entire operation, here whenever a raycast starting from the blade of the shovel, going forward
        // hits terrain it starts this function. In case the Ray hits an area way too far away or not the terrain nothing should happen
        // In case it hits the terrain, depending on the distance of the raycast, it either lowers or raises the terrain.
        // The Raising is only happening in case the shovel is properly opened, to make sure, the behavior is right.
        private void Update()
        {
            Transform shovelPos = this.transform;
            if (Physics.Raycast(shovelPos.position, -shovelPos.forward, out RaycastHit hitInfo))
            {
                // Only Handle Terrain hits
                if (   hitInfo.collider.gameObject.name != "Terrain"
                    || hitInfo.distance > 5)
                {
                    return;
                }

                TerrainModificationAction action = TerrainModificationAction.Lower;

                // Lower if Bucket practically hits the terrain, raise otherwise
                if (hitInfo.distance > 0.5f)
                {
                    // Additional Condition, World-Rotation (not local) must be so the open bucket points to the ground
                    if (   shovelPos.rotation.eulerAngles.x < 290
                        && shovelPos.rotation.eulerAngles.x > 260)
                    {
                        action = TerrainModificationAction.Raise;
                    }
                    else
                    {
                        return;
                    }
                }
                
                // It should find the Terrain with certainty, but just to make sure a try condition
                if (hitInfo.transform.TryGetComponent(out Terrain terrain))
                {
                    _targetTerrain = terrain;

                    _targetTerrainData = _targetTerrain.terrainData;
                }
                else
                {
                    return;
                }
                
                switch (action)
                {
                    case TerrainModificationAction.Raise:
                    {
                       RaiseTerrain(hitInfo.point);

                        break;
                    }

                    case TerrainModificationAction.Lower:
                    {
                        LowerTerrain(hitInfo.point);

                        break;
                    }
                    default:
                    {
                        // Should not happen, but just to be sure, log 
                        Debug.LogError("Invalid terrain modification action.", this);

                        break;
                    }
                }
            }
        }

        // Converts the World Position of the Ray-Hit to the current terrain-position, so it can be used to get data from the terraindata
        private Vector3 WorldToTerrainPosition(Vector3 worldPosition)
        {
            Vector3 terrainPosition = worldPosition - _targetTerrain.GetPosition();

            Vector3 terrainSize = _targetTerrainData.size;

            int heightmapResolution = _targetTerrainData.heightmapResolution;

            terrainPosition = new Vector3(terrainPosition.x / terrainSize.x, 
                                          terrainPosition.y / terrainSize.y,
                                          terrainPosition.z / terrainSize.z);

            return new Vector3(terrainPosition.x * heightmapResolution, 
                               0.0f, 
                               terrainPosition.z * heightmapResolution);
        }

        // The Brush-Position Clamping is done so in case the bucket is moved minimally, the terraforming does not change all too much
        private (int, int) ClampBrushPosition(Vector3 brushWorldPosition)
        {
            Vector3 terrainPosition = WorldToTerrainPosition(brushWorldPosition);

            int heightmapResolution = _targetTerrainData.heightmapResolution;

            int clampedBrushX = (int)Math.Min(
                                              Math.Max(
                                                       (terrainPosition.x - brushWidth * 0.5f), 
                                                       0), 
                                              heightmapResolution);
            int clampedBrushY = (int)Math.Min(
                                              Math.Max(
                                                        (terrainPosition.z - brushHeight * 0.5f), 
                                                        0), 
                                              heightmapResolution);

            return (clampedBrushX, clampedBrushY);
        }

        // The idea behind clamping the brush size is to provide more consistency in the modification area of the terraforming
        private (int, int) ClampBrushSize(int brushX, int brushY)
        {
            int heightmapResolution = _targetTerrainData.heightmapResolution;

            int clampedBrushWidth = Math.Min(brushWidth, heightmapResolution - brushX);
            int clampedBrushHeight = Math.Min(brushHeight, heightmapResolution - brushY);

            return (clampedBrushWidth, clampedBrushHeight);
        }

        // Increases the Heightmap Values in the area the Terraforming-Brush is applied at
        private void RaiseTerrain(Vector3 brushWorldPosition)
        {
            (int clampedBrushX, int clampedBrushY) = ClampBrushPosition(brushWorldPosition);

            (int clampedBrushWidth, int clampedBrushHeight) = ClampBrushSize(clampedBrushX, clampedBrushY);

            // Gets the current heightmap for the brush area
            float[,] heights = _targetTerrainData.GetHeights(clampedBrushX, 
                                                             clampedBrushY, 
                                                             clampedBrushWidth, 
                                                             clampedBrushHeight);

            // Calculates the strength, so the increase is not bound to framerate
            float increment = strength * Time.deltaTime;

            for (int y = 0; y < clampedBrushHeight; y++)
            {
                for (int x = 0; x < clampedBrushWidth; x++)
                {
                    heights[y, x] += increment;
                }
            }

            _targetTerrainData.SetHeights(clampedBrushX, clampedBrushY, heights);
            UpdateTerrainTexture(clampedBrushX, clampedBrushY, clampedBrushWidth, clampedBrushHeight, 2);
        }

        // Decreases the Heightmap Values in the area the Terraforming-Brush is applied at
        private void LowerTerrain(Vector3 brushWorldPosition)
        {
            (int clampedBrushX, int clampedBrushY) = ClampBrushPosition(brushWorldPosition);

            (int clampedBrushWidth, int clampedBrushHeight) = ClampBrushSize(clampedBrushX, clampedBrushY);

            float[,] heights =
                _targetTerrainData.GetHeights(clampedBrushX, clampedBrushY, clampedBrushWidth, clampedBrushHeight);

            float decrement = strength * Time.deltaTime;

            for (int y = 0; y < clampedBrushHeight; y++)
            {
                for (int x = 0; x < clampedBrushWidth; x++)
                {
                    heights[y, x] -= decrement;
                }
            }

            _targetTerrainData.SetHeights(clampedBrushX, clampedBrushY, heights);
            UpdateTerrainTexture(clampedBrushX, clampedBrushY, clampedBrushWidth, clampedBrushHeight, 2);
        }
        
        
        private int updateTexLastX = -1;
        private int updateTexLastY = -1;
        
        // This Function updates the Terrain Texture on the Terraforming-Position
        // so it is noticable when the terrain has been deformed at the position;
        
        // For this it grabs the alphamaps of the area where the terraforming happened
        // and sets the opacity of the dirt-texture to 1 and the alpha of the others to 0
        private void UpdateTerrainTexture(int posX, int posY, int width, int height, int textureIndex)
        {
            if (posX == updateTexLastX
                && posY == updateTexLastY)
            {
                return;
            }
            else {
                updateTexLastX = posX;
                updateTexLastY = posY;
            }
            // Get the number of textures on the terrain
            int texturesCount = _targetTerrainData.alphamapLayers;
  
            // Create a 3D array to hold the new alpha values for each texture on the terrain
            float[,,] textureAlphas = _targetTerrainData.GetAlphamaps(posX, posY, width, height);
  
            // Loop through each pixel in the brush area and set the alpha value for the specified texture index to 1, and 0 for all others
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    for (var i = 0; i < texturesCount; i++)
                    {
                        // If the current texture index matches the specified texture index, set its alpha value to 1
                        // Otherwise, set its alpha value to 0
                        textureAlphas[y, x, i] = (i == textureIndex) ? 1.0f : 0.0f;
                    }
                }
            }
  
            // Set the alpha map at the specified position to the updated texture alphas
            _targetTerrainData.SetAlphamaps(posX, posY, textureAlphas);
        }
    }
}