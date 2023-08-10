using Assets.scripts;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AssestLoader : MonoBehaviour
{
    private int assestCounts = 10;
    private int assestCounter = 0;
    private int cubeCounts = 7;
    private void LoadAlls()
    {
        AddressablesManager.Initialize(isInited =>
        {
            AddressablesManager.LoadAsset<GameObject>("prefabs/enemy", (key, prefab) =>
            {
                AddToGameManager("enemy", prefab);
            });

            AddressablesManager.LoadAsset<GameObject>("prefabs/role", (key, prefab) =>
            {
                AddToGameManager("role", prefab);
            });

            for (int i = 1; i <= cubeCounts; i++)
            {
                AddressablesManager.LoadAsset<GameObject>("prefabs/item"+i, (key, prefab) =>
                {
                    AddToGameManager(key, prefab);
                });
            }
            LoadMarerials();
        });
    }
    private void LoadMarerials()
    {
        AddressablesManager.LoadAsset<Material>("Assets/mats/Transparent.mat", (key, prefab) =>
        {
            GameManager._instance.transparentMaterial = prefab;
            assestCounter++;
        });

    }
    private void AddToGameManager(string key,GameObject prefab)
    {
        string[] infos = key.Split("/");
        key = infos[infos.Length - 1];

        GameManager._instance.prefabAssests.Add(key, prefab);
        assestCounter++;
        Debug.Log( prefab.name + ":完成预加载");
    }
    private void ReleaseAsset(string key)
    {
        GameManager._instance.prefabAssests.Remove(key);
        AddressablesManager.ReleaseAsset(key);//用不到之后再去释放
    }

    private void Awake()
    {
        LoadAlls();
    }
    private void FixedUpdate()
    {
        if (assestCounter == assestCounts)
        {
            Destroy(gameObject);
            GameManager._instance.InitialGameMangerComponent();
        }
    }
}
