using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class AsyncSceneLoader
{
    public static float progressf = 0f;

    public static IEnumerator LoadSceneCoroutine(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == sceneName && loadedScene.isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneName}' is already loaded!");
                yield break; // 중복 로드 방지
            }
        }
        
        progressf = 0f;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);

        // (선택) 씬이 다 로드될 때까지 멈추지 않게 설정
        operation.allowSceneActivation = false;

        // 로딩 진행률 체크
        while (!operation.isDone)
        {
            Debug.Log("Loading progress: " + (operation.progress * 100f) + "%");
            progressf = operation.progress * 100f;
            // 거의 다 됐으면(90%) 수동으로 전환 허용
            if (operation.progress >= 0.9f)
            {
                // 여기서 로딩 애니메이션 완료 조건 등이 만족되면 전환
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        //SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}
