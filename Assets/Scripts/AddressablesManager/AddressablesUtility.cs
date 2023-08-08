namespace UnityEngine.AddressableAssets
{
    public static class AddressablesUtility
    {
        /// <summary>
        /// 解析资源路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="mainRes"></param>
        /// <param name="subRes"></param>
        public static void ParseAssetPath(string assetPath, out string mainRes, out string subRes)
        {
            int idxSpliter = assetPath.LastIndexOf(':');
            if (0 <= idxSpliter && idxSpliter < assetPath.Length)
            {
                mainRes = assetPath.Substring(0, idxSpliter);
                subRes = assetPath.Substring(idxSpliter + 1);
            }
            else
            {
                subRes = "";
                mainRes = assetPath;
            }
        }
    }
}