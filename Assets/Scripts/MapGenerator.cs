using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using Unity.VisualScripting;
using System.Drawing;
using System.Net.NetworkInformation;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;

    public float tileSize;

    List<Coordinate> tileCoordinates;
    Queue<Coordinate> shuffledTileCoordinates;
    Queue<Coordinate> shuffledOpenTileCoordinates;
    Transform[,] tileLocations;

    Map currentMap;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileLocations = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        currentMap.prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, .05f, currentMap.mapSize.y * tileSize);

        Transform mapHolder = InitializeMapData();
        GenerateMapTiles(mapHolder);
        bool[,] obstacleMap = GenerateObstacleTiles(mapHolder);
        GenerateMapBoundaries(mapHolder, obstacleMap);
    }

    Transform InitializeMapData()
    {
        tileCoordinates = new List<Coordinate>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                tileCoordinates.Add(new Coordinate(x, y));
            }
        }
        shuffledTileCoordinates = new Queue<Coordinate>(Utility.ShuffleArray(tileCoordinates.ToArray(), currentMap.seed));

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        return mapHolder;
    }

    void GenerateMapTiles(Transform mapHolder)
    {
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                tileLocations[x,y] = newTile;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
            }
        }
    }

    bool[,] GenerateObstacleTiles(Transform mapHolder)
    {
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coordinate> openTileCoordinates = new List<Coordinate>(tileCoordinates);

        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.mapCentre && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)currentMap.prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleHeight / 2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = UnityEngine.Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                openTileCoordinates.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoordinates = new Queue<Coordinate>(Utility.ShuffleArray(openTileCoordinates.ToArray(), currentMap.seed));

        return obstacleMap;
    }

    void GenerateMapBoundaries(Transform mapHolder, bool[,] obstacleMap)
    {
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }

    /* Floodfill, start from mapCenter since there won't be an obstacle there. Search all tiles in an expanding outwards radius (BFS). 
     * Store tiles searched in a queue and keep dequeing until no tiles are left, marking visited tiles.
     * After the traversal, the accessibleTileCount is compared to the target accessible tile count, calculated based on the map size (mapSize)
     * and current obstacle count.
     * If the two counts match, the method returns true, indicating that the map is fully accessible. Returns false otherwise.
     */
    bool IsMapFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        int width = obstacleMap.GetLength(0);
        int height = obstacleMap.GetLength(1);
        bool[,] visitedTiles = new bool[width, height];
        Queue<Coordinate> queue = new Queue<Coordinate>();

        queue.Enqueue(currentMap.mapCentre);
        visitedTiles[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coordinate tile = queue.Dequeue();

            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, -1, 0, 1 };

            for (int i = 0; i < 4; i++)
            {
                int neighbourXCoord = tile.x + dx[i];
                int neighbourYCoord = tile.y + dy[i];

                if (neighbourXCoord >= 0 && neighbourXCoord < width && neighbourYCoord >= 0 && neighbourYCoord < height)
                {
                    if (!visitedTiles[neighbourXCoord, neighbourYCoord] && !obstacleMap[neighbourXCoord, neighbourYCoord])
                    {
                        visitedTiles[neighbourXCoord, neighbourYCoord] = true;
                        queue.Enqueue(new Coordinate(neighbourXCoord, neighbourYCoord));
                        accessibleTileCount++;
                    }
                }
            }
        }

        int targetAccessibleTileCount = (width * height) - currentObstacleCount;
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp(x, 0, tileLocations.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileLocations.GetLength(1) - 1);
        return tileLocations[x, y];
    }

    public Coordinate GetRandomCoord()
    {
        Coordinate randomCoord = shuffledTileCoordinates.Dequeue();
        shuffledTileCoordinates.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coordinate randomCoord = shuffledOpenTileCoordinates.Dequeue();
        shuffledOpenTileCoordinates.Enqueue(randomCoord);
        return tileLocations[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return !(c1 == c2);
        }
    }

    [System.Serializable]
    public class Map
    {

        public Coordinate mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public UnityEngine.Color foregroundColour;
        public UnityEngine.Color backgroundColour;
        public System.Random prng;

        public Coordinate mapCentre
        {
            get
            {
                return new Coordinate(mapSize.x / 2, mapSize.y / 2);
            }
        }

    }
}
