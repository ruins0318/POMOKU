using UnityEngine;

namespace Pomoku.Core
{
    public static class PomokuRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateLocalGamePreview()
        {
            if (GameObject.Find("PomokuRuntimeBootstrap") != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject("PomokuRuntimeBootstrap");
            LocalGameManager localGameManager = bootstrapObject.AddComponent<LocalGameManager>();
            localGameManager.StartLocalGame();
        }
    }
}
