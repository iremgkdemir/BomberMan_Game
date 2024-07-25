using Unity.VisualScripting;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    /// <summary>
    /// Trinty 1.0
    /// Level haz�rlama i�lemleri ARTIK KULLANILMIYOR GLOBAL ��ER�S�NDE TANIMLI
    /// </summary>

    [SerializeField] public static GameObject kayaPrefab; // Prefab for rocks
    public static GameObject duvarPrefab; // Prefab for boxes
    public static GameObject kutuPrefab; // Prefab for boxes
    public static GameObject canavarPrefab; // Prefab for monsters
    public static GameObject oyuncuPrefab; // Prefab for player
    public static Terrain zemin; 

    void Start()
    {
        
    }

    public static void PrepareNewLevel()
    {
        //if (global.map != null) Destroy(global.map);
        // Create a map array with the given dimensions
        global.map = new global.Nesne[global.en, global.boy];
    }
    public static void GenerateLevel()
    {

        // Terrain B�y�kl��� ayarlan�yor geni�lik katsay�s�na g�re
        zemin.terrainData.size = new Vector3(global.en*global.genislikKatsayisi, 0, global.boy * global.genislikKatsayisi);
        Debug.Log("Harita olu�turuldu :");
        // Place objects on the map
        //PlaceObjectsOnMap(kayaPrefab, 60);

        // Kayalar haritaya yerle�tiriliyor
        for (int i = 1; i< global.en-1; i+=2)
            for (int j = 1; j< global.boy-1; j+=2)
            {
                global.map[i, j] = global.Nesne.Kaya;
                PlaceObject(kayaPrefab, i, j);
            }

        
        PlaceObjectsOnMap(kutuPrefab, 45);          // Kutular olu�turuluyor
        PlaceObjectsOnMap(canavarPrefab, 20);       // Canavarlar olu�turuluyor

        // Oyuncu karakteri yerle�tiriliyor
        PlacePlayerOnMap(oyuncuPrefab);
    }

    static void PlaceObjectsOnMap(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Debug.Log("Nesne Olusturuluyor :" + i.ToString());
            PlaceObject(prefab);

        }
    }

    // Oyuncu olu�turuluyor ve global.Player1 e referans� ba�lan�yor.
    static void PlacePlayerOnMap(GameObject prefab)
    {
        // Bir nesne olu�turur
        global.map[0, 0] = global.Nesne.Player;
        global.Player1 = PlaceObject(prefab, 0, 0);
    }

    // Olu�turulan nesnenin referans�n� d�ndererek bir nesne olu�turur
    static GameObject PlaceObject(GameObject prefab)
    {
        // Rastgele bir konum belirle
        GameObject gameobj = null;
        int x, z;
        do    // Dizideki konum bo� olana kadar yeni say� �ret
        {
            x = Random.Range(0, global.en - 1);
            z = Random.Range(0, global.boy - 1);
        } while (global.map[x, z] != global.Nesne.Bos);

        // Prefab � yerle�tir
        Vector3 position = new Vector3((x + 0.5f) * global.genislikKatsayisi, 0.5f, (z + 0.5f) * global.genislikKatsayisi);
        Debug.Log("Prefab Olusturuluyor :");
        gameobj = Instantiate(prefab, position, Quaternion.identity);
        Debug.Log("Prefab Olusturuldu :");
        global.map[x, z] = global.Nesne.Kutu; // Mark the position as occupied

        return gameobj;
    }

    // Olu�turulan nesnenin referans�n� d�ndererek bir nesne olu�turur
    static GameObject PlaceObject(global.Nesne nesne)
    {
        // Rastgele bir konum belirle
        GameObject gameobj = null;
        GameObject prefab=null;
        int x, z;
        do    // Dizideki konum bo� olana kadar yeni say� �ret
        {
            x = Random.Range(0, global.en - 1);
            z = Random.Range(0, global.boy - 1);
        } while (global.map[x, z] != global.Nesne.Bos);

        // Prefab � yerle�tir
        Vector3 position = new Vector3((x + 0.5f) * global.genislikKatsayisi, 0.5f, (z + 0.5f) * global.genislikKatsayisi);
        switch (nesne)
        {
            case global.Nesne.Kutu: prefab = kutuPrefab; break;
            case global.Nesne.Player: prefab = oyuncuPrefab; break;
            case global.Nesne.Canavar: prefab = canavarPrefab; break;
            case global.Nesne.Kaya: prefab = kayaPrefab; break;
            case global.Nesne.Duvar: prefab = duvarPrefab; break;
            //case global.Nesne.Kutu: prefab = kutuPrefab; break;
            default: Debug.Log("Nesne Bulunamad�"); break;
        }
        gameobj = Instantiate(prefab, position, Quaternion.identity);
        Debug.Log("Prefab Olusturuldu :");
        global.map[x, z] = nesne; // Mark the position as occupied

        return gameobj;
    }

    // x y konumuna Prefab olu�turur
    static GameObject PlaceObject(GameObject prefab, int x, int y)
    {
        GameObject gameobj = null;
        // Check if the position is empty
        if (global.map[x, y] != global.Nesne.Bos)
        {
            Vector3 position = new Vector3((x + 0.5f) * global.genislikKatsayisi, 0.5f, (y + 0.5f) * global.genislikKatsayisi);
            gameobj = Instantiate(prefab, position, Quaternion.identity);
        }
        return gameobj;
    }
}
