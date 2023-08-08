using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;


namespace UnityEngine.AddressableAssets
{
    using ResourceManagement.ResourceProviders;
    using ResourceManagement.ResourceLocations;

    public static class AddressablesManager
    {
        /// <summary>
        /// 异常处理类型
        /// </summary>
        public enum ExceptionHandleType
        {
            None,
            Log,
            Throw,
            Suppress
        }

        /// <summary>
        /// 加载资源的操作句柄
        /// </summary>
        private static readonly Dictionary<string, AsyncOperationHandle> _assets;

        /// <summary>
        /// 加载场景的操作句柄
        /// </summary>
        private static readonly Dictionary<string, AsyncOperationHandle> _scenes;

        /// <summary>
        /// 实例化操作句柄
        /// </summary>
        private static readonly Dictionary<string, List<AsyncOperationHandle>> _instances;

        /// <summary>
        /// 实例化到对象池中的操作句柄
        /// </summary>
        private static readonly Dictionary<string, List<AsyncOperationHandle>> _poolInstances;

        /// <summary>
        /// 实例化操作句柄列表回收池
        /// </summary>
        private static readonly Queue<List<AsyncOperationHandle>> _instanceListPool;

        /// <summary>
        /// 设置异常处理类型
        /// </summary>
        public static ExceptionHandleType ExceptionHandle { get; set; }

        public static bool SuppressWarningLogs { get; set; }

        public static bool SuppressErrorLogs { get; set; }

        private static GameObject _poolRoot;
        
        private static bool _isInitialized;
        
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 资源定位器
        /// </summary>
        private static IResourceLocator _resourceLocator;

        static AddressablesManager()
        {
            ExceptionHandle = ExceptionHandleType.None;

            _assets = new Dictionary<string, AsyncOperationHandle>();
            _scenes = new Dictionary<string, AsyncOperationHandle>();
            _instances = new Dictionary<string, List<AsyncOperationHandle>>();
            _poolInstances = new Dictionary<string, List<AsyncOperationHandle>>();
            _instanceListPool = new Queue<List<AsyncOperationHandle>>();
        }

        /// <summary>
        /// 回调版异步初始化
        /// </summary>
        public static void Initialize(Action<bool> onCompleted)
        {
            _poolRoot = GameObject.Find("AddressablesManager");
            if (_poolRoot != null) return;
            _poolRoot = new GameObject("AddressablesManager");
            Object.DontDestroyOnLoad(_poolRoot);
            Addressables.InitializeAsync().Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("AddressablesManager InitializeAsync Succeeded");
                    _resourceLocator = handle.Result;
                    onCompleted?.Invoke(true);
                }
                else
                {
                    Debug.LogError("AddressablesManager InitializeAsync Failed");
                    onCompleted?.Invoke(false);
                }

                _isInitialized = true;
            };
        }

        /// <summary>
        /// UniTask版异步初始化
        /// </summary>
        public static async UniTask<bool> InitializeAsync()
        {
            _poolRoot = GameObject.Find("AddressablesManager");
            if (_poolRoot != null) return true;
            var initializeOperation = Addressables.InitializeAsync();
            await initializeOperation;
            if (initializeOperation.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("AddressablesManager InitializeAsync Succeeded");
                _poolRoot = new GameObject("AddressablesManager");
                Object.DontDestroyOnLoad(_poolRoot);
                _resourceLocator = initializeOperation.Result;
                return true;
            }

            Debug.LogError("AddressablesManager InitializeAsync Failed");
            return false;
        }

        private static void Clear()
        {
            _assets.Clear();
            _scenes.Clear();
        }

        /// <summary>
        /// 检测资源key值是否合法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static bool GuardKey(string key, out string result)
        {
            result = key ?? string.Empty;

            return !string.IsNullOrEmpty(result);
        }

        #region 回调加载方法

        /// <summary>
        /// 回调的方式加载资源
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onSucceeded"></param>
        /// <param name="onFailed"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public static void LoadAsset<T>(string key,
            Action<string, T> onSucceeded,
            Action<string> onFailed = null)
        {
            if (!GuardKey(key, out key))
            {
                onFailed?.Invoke(key);
                return;
            }

            var isExist = LoadLocations<T>(key, out var locations, out var fixedKey);
            if (!isExist)
            {
                onFailed?.Invoke(fixedKey);
                return;
            }

            try
            {
                var operation = GetLoadAssetOperationHandle<T>(fixedKey);
                if (!operation.IsValid())
                {
                    onFailed?.Invoke(fixedKey);
                    return;
                }

                operation.Completed += handle => OnLoadAssetCompleted(handle, fixedKey, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (_assets.ContainsKey(fixedKey)) _assets.Remove(fixedKey);
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                onFailed?.Invoke(fixedKey);
            }
        }

        /// <summary>
        /// 回调的方式实例化预设
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="inWorldSpace"></param>
        /// <param name="trackHandle"></param>
        /// <param name="onSucceeded"></param>
        /// <param name="onFailed"></param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException"></exception>
        /// <exception cref="Exception"></exception>
        public static void Instantiate(string key,
            Transform parent = null,
            bool inWorldSpace = false,
            bool trackHandle = true, Action<string, GameObject> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            var isExist = LoadLocations<GameObject>(key, out var locations, out var fixedKey);
            if (!isExist)
            {
                onFailed?.Invoke(fixedKey);
                return;
            }

            try
            {
                var operation = GetInstantiateOperationHandle(fixedKey, parent, inWorldSpace, trackHandle);
                operation.Completed += handle => OnInstantiateCompleted(handle, fixedKey, onSucceeded, onFailed);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
                onFailed?.Invoke(fixedKey);
            }
        }

        /// <summary>
        /// 回调的方式加载资源位置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="locations"></param>
        /// <param name="fixedKey"></param>
        private static bool LoadLocations<T>(string key, out IList<IResourceLocation> locations, out string fixedKey)
        {
            if (_resourceLocator == null)
            {
                Debug.LogError("AddressablesManager is not initialized");
                locations = null;
                fixedKey = key;
                return false;
            }

            //这里需要对Sprite[]类型做特殊处理
            var realType = typeof(T);
            realType = realType == typeof(Sprite[]) ? typeof(Sprite) : realType;

            //第一步直接处理可以
            if (_resourceLocator.Locate(key, realType, out locations))
            {
                fixedKey = key;
                return true;
            }

            //第二步删除后缀再判断一次
            if (!string.IsNullOrEmpty(Path.GetExtension(key)))
            {
                fixedKey = Path.ChangeExtension(key, null);
                if (_resourceLocator.Locate(fixedKey, realType, out locations))
                {
                    return true;
                }
            }

            locations = null;
            fixedKey = key;
            Debug.LogWarning(Exceptions.CannotFindAssetKey<T>(key));
            return false;
        }

        #endregion

        #region UniTask相关的方法

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException"></exception>
        /// <exception cref="Exception"></exception>
        public static async UniTask<T> LoadAssetAsync<T>(string key)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            var isExist = LoadLocations<T>(key, out var locations, out var fixedKey);
            if (!isExist) return default;

            try
            {
                var operation = GetLoadAssetOperationHandle<T>(fixedKey);
                if (!operation.IsValid()) return default;
                await operation;

                OnLoadAssetCompleted(operation, fixedKey);
                return operation.Result;
            }
            catch (Exception ex)
            {
                if (_assets.ContainsKey(fixedKey)) _assets.Remove(fixedKey);
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// 异步的方式实例化预设
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="inWorldSpace"></param>
        /// <param name="trackHandle"></param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException"></exception>
        /// <exception cref="Exception"></exception>
        public static async UniTask<GameObject> InstantiateAsync(string key,
            Transform parent = null,
            bool inWorldSpace = false,
            bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            var isExist = LoadLocations<GameObject>(key, out var locations, out var fixedKey);
            if (!isExist) return default;

            try
            {
                var operation = GetInstantiateOperationHandle(fixedKey, parent, inWorldSpace, trackHandle);
                await operation;

                OnInstantiateCompleted(operation, fixedKey);
                return operation.Result;
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);

                return default;
            }
        }

        /// <summary>
        /// 直接实例化预设到对象池中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="trackHandle"></param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException"></exception>
        /// <exception cref="Exception"></exception>
        public static async UniTask InstantiateToPoolAsync(string key, bool trackHandle = true)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            var isExist = LoadLocations<GameObject>(key, out var locations, out var fixedKey);
            if (!isExist) return;

            try
            {
                var operation = GetInstantiateToPoolOperationHandle(fixedKey, trackHandle);
                await operation;

                if (operation.Result != null) operation.Result.SetActive(false);
                OnInstantiateToPoolCompleted(operation, fixedKey);
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        /// <summary>
        /// 异步的方式加载场景
        /// </summary>
        /// <param name="key"></param>
        /// <param name="loadMode"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <param name="onProgress"></param>
        /// <exception cref="InvalidKeyException"></exception>
        /// <exception cref="Exception"></exception>
        public static async UniTask LoadSceneAsync(string key,
            Action<float> onProgress = null,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100
        )
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            var isExist = LoadLocations<SceneInstance>(key, out var locations, out var fixedKey);
            if (!isExist) return;

            try
            {
                var operation = GetLoadSceneOperationHandle(fixedKey, loadMode, activateOnLoad, priority);
                while (!operation.IsDone)
                {
                    onProgress?.Invoke(operation.PercentComplete);
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }

                OnLoadSceneCompleted(operation, fixedKey);
            }
            catch (Exception ex)
            {
                if (_scenes.ContainsKey(fixedKey)) _scenes.Remove(fixedKey);
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(ex);
            }
        }

        #endregion

        #region 完成回调

        /// <summary>
        /// 加载资源完成回调
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="key"></param>
        /// <param name="onSucceeded"></param>
        /// <param name="onFailed"></param>
        /// <typeparam name="T"></typeparam>
        private static void OnLoadAssetCompleted<T>(AsyncOperationHandle<T> handle,
            string key,
            Action<string, T> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            if (handle.Result == null)
            {
                onFailed?.Invoke(key);
                return;
            }

            onSucceeded?.Invoke(key, handle.Result);
        }

        private static void OnLoadSceneCompleted(AsyncOperationHandle<SceneInstance> handle,
            string key,
            Action<SceneInstance> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                onSucceeded?.Invoke(handle.Result);
            }
            else if (handle.Status == AsyncOperationStatus.Failed)
            {
                onFailed?.Invoke(key);
            }
        }

        /// <summary>
        /// 实例化预设完成回调
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="key"></param>
        /// <param name="onSucceeded"></param>
        /// <param name="onFailed"></param>
        private static void OnInstantiateCompleted(AsyncOperationHandle<GameObject> handle,
            string key,
            Action<string, GameObject> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!handle.Result)
            {
                onFailed?.Invoke(key);
                return;
            }

            onSucceeded?.Invoke(key, handle.Result);
        }

        /// <summary>
        /// 实例化预设到对象池中的完成回调
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="key"></param>
        /// <param name="onSucceeded"></param>
        /// <param name="onFailed"></param>
        private static void OnInstantiateToPoolCompleted(AsyncOperationHandle<GameObject> handle,
            string key,
            Action<string> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                onFailed?.Invoke(key);
                return;
            }

            if (!handle.Result)
            {
                onFailed?.Invoke(key);
                return;
            }

            onSucceeded?.Invoke(key);
        }

        #endregion

        /// <summary>
        /// 获取实例化操作句柄列表
        /// </summary>
        /// <returns></returns>
        private static List<AsyncOperationHandle> GetInstanceList()
        {
            if (_instanceListPool.Count > 0)
                return _instanceListPool.Dequeue();

            return new List<AsyncOperationHandle>();
        }

        /// <summary>
        /// 获取加载场景的操作句柄
        /// </summary>
        /// <param name="key"></param>
        /// <param name="loadMode"></param>
        /// <param name="activateOnLoad"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private static AsyncOperationHandle<SceneInstance> GetLoadSceneOperationHandle(string key,
            LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            var isExist = _scenes.TryGetValue(key, out AsyncOperationHandle operation);
            if (!isExist || !operation.IsValid())
            {
                operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                if (isExist)
                {
                    _scenes[key] = operation;
                }
                else
                {
                    _scenes.Add(key, operation);
                }
            }

            return operation.Convert<SceneInstance>();
        }

        /// <summary>
        /// 获取加载资源的操作句柄
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static AsyncOperationHandle<T> GetLoadAssetOperationHandle<T>(string key)
        {
            var isExist = _assets.TryGetValue(key, out AsyncOperationHandle operation);
            if (!isExist || !operation.IsValid())
            {
                operation = Addressables.LoadAssetAsync<T>(key);
                if (isExist)
                {
                    _assets[key] = operation;
                }
                else
                {
                    _assets.Add(key, operation);
                }
            }

            try
            {
                return operation.Convert<T>();
            }
            catch (Exception ex)
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw ex;
                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogError(Exceptions.SpecifiedCastIsNotValid(key,
                        operation.Result == null ? "NULL" : operation.Result.GetType().Name, typeof(T).Name));
                return default;
            }
        }

        /// <summary>
        /// 获取实例化操作句柄
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="instantiateInWorldSpace"></param>
        /// <param name="trackHandle"></param>
        /// <returns></returns>
        private static AsyncOperationHandle<GameObject> GetInstantiateOperationHandle(string key,
            Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            var operation = Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
            if (!_instances.ContainsKey(key))
                _instances.Add(key, GetInstanceList());

            _instances[key].Add(operation);

            return operation;
        }

        /// <summary>
        /// 获取实例化到对象池中的操作句柄
        /// </summary>
        /// <param name="key"></param>
        /// <param name="trackHandle"></param>
        /// <returns></returns>
        private static AsyncOperationHandle<GameObject> GetInstantiateToPoolOperationHandle(string key,
            bool trackHandle = true)
        {
            var operation = Addressables.InstantiateAsync(key, _poolRoot.transform, false, trackHandle);
            if (!_poolInstances.ContainsKey(key))
                _poolInstances.Add(key, GetInstanceList());

            _poolInstances[key].Add(operation);

            return operation;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidKeyException"></exception>
        public static T GetAsset<T>(string key)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return default;
            }

            LoadLocations<T>(key, out var locations, out var fixedKey);

            return GetAssetInternal<T>(fixedKey);
        }

        /// <summary>
        /// 获取资源内部方法
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T GetAssetInternal<T>(string key)
        {
            if (!_assets.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.CannotFindAssetByKey(key));

                return default;
            }

            if (_assets[key].Status != AsyncOperationStatus.Succeeded)
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotSucceeded(key));

                return default;
            }

            if (_assets[key].Result is T asset)
                return asset;

            if (!SuppressWarningLogs)
                Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

            return default;
        }

        /// <summary>
        /// 从对象池中获取获取实例化资源
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parent"></param>
        /// <param name="instantiateInWorldSpace"></param>
        /// <returns></returns>
        public static GameObject GetInstanceFromPool(string key, Transform parent = null,
            bool instantiateInWorldSpace = false)
        {
            if (!_poolInstances.TryGetValue(key, out var instances) || instances.Count == 0)
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoInstanceKeyInitialized(key));
                return default;
            }

            var instance = instances[0];
            instances.RemoveAt(0);
            if (!instance.IsValid() || instance.Result == null)
            {
                Addressables.ReleaseInstance(instance);
                return default;
            }

            if (!_instances.ContainsKey(key))
                _instances.Add(key, GetInstanceList());

            _instances[key].Add(instance);
            var instanceObject = instance.Result as GameObject;
            if (instanceObject == null) return default;
            if (parent != null) instanceObject.transform.SetParent(parent, instantiateInWorldSpace);
            instanceObject.SetActive(true);

            return instanceObject;
        }

        public static void ReleaseAsset(string key)
        {
            if (!GuardKey(key, out key))
            {
                if (ExceptionHandle == ExceptionHandleType.Throw)
                    throw new InvalidKeyException(key);

                if (ExceptionHandle == ExceptionHandleType.Log)
                    Debug.LogException(new InvalidKeyException(key));

                return;
            }

            if (!_assets.TryGetValue(key, out var asset))
                return;

            _assets.Remove(key);
            Addressables.Release(asset);
        }
    }
}