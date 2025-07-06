using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class BodenBuilder : MonoBehaviour
{
    [System.Serializable]
    public class BodenDaten
    {
        public int width;
        public int height;
        public List<int> tiles;
    }

    [Header("Prefabs")]
    public GameObject bodenArt1Prefab; // ID 1
    public GameObject bodenArt2Prefab; // ID 2
    public GameObject wandPrefab;      // ID 3
    public GameObject playerPrefab;    // Spieler

    [Header("Einstellungen")]
    public string jsonDateiname = "boden"; // Datei in Resources (ohne ".json")
    public float tileSpacing = 5.0f;

    public NavMeshSurface navMeshSurface; // Bewegbare Fläche berechnen

    private GameObject ersteTile1Objekt = null;

    void Start()
    {
        TextAsset json = Resources.Load<TextAsset>(jsonDateiname);
        if (json == null)
        {
            Debug.LogError("JSON-Datei nicht gefunden: " + jsonDateiname);
            return;
        }

        BodenDaten daten = JsonUtility.FromJson<BodenDaten>(json.text);
        if (daten.tiles == null || daten.tiles.Count != daten.width * daten.height)
        {
            Debug.LogError("Tile-Daten ungültig oder unvollständig.");
            return;
        }

        for (int y = 0; y < daten.height; y++)
        {
            for (int x = 0; x < daten.width; x++)
            {
                int index = y * daten.width + x;
                int id = daten.tiles[index];

                Vector3 pos = new Vector3(x * tileSpacing, 0f, -y * tileSpacing);
                GameObject instantiated = null;

                switch (id)
                {
                    case 1:
                        instantiated = Instantiate(bodenArt1Prefab, pos, Quaternion.identity, this.transform);
                        if (ersteTile1Objekt == null)
                            ersteTile1Objekt = instantiated;
                        break;

                    case 2:
                        instantiated = Instantiate(bodenArt2Prefab, pos, Quaternion.identity, this.transform);
                        break;

                    case 3:
                        Vector3 wandPos = pos + Vector3.up * 2.5f;
                        instantiated = Instantiate(wandPrefab, wandPos, Quaternion.identity, this.transform);
                        break;
                }
            }
        }

        if (playerPrefab != null)
        {
            if (ersteTile1Objekt != null)
            {
                Vector3 spawnPos = ersteTile1Objekt.transform.position + new Vector3(0f, 1f, 0f);
                Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Kein Boden mit ID 1 gefunden – Spieler konnte nicht platziert werden.");
            }
        }

        // <- WICHTIG: NavMesh nach dem Aufbau generieren
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("NavMeshSurface ist nicht zugewiesen!");
        }
    }
}
