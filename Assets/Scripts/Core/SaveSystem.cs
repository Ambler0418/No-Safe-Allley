using UnityEngine;
using System.IO;

public static class SaveSystem
{
    // 저장 파일 이름
    private static string saveFileName = "savegame.json";

    // 저장 경로 (PersistentDataPath는 앱 업데이트가 되어도 유지되는 경로입니다)
    private static string GetPath()
    {
#if UNITY_EDITOR
        // 유니티 에디터에서는 프로젝트 루트 폴더 (Assets 폴더의 상위)에 저장합니다.
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, saveFileName);
#else
        // 빌드된 게임에서는 OS의 표준 저장 경로를 사용합니다.
        return Path.Combine(Application.persistentDataPath, saveFileName);
#endif
    }

    public static void SaveGame(GameSaveData data)
    {
        // 1. 데이터를 JSON 문자열로 변환
        string json = JsonUtility.ToJson(data, true); // true는 가독성 좋게 줄바꿈 포함

        // 2. 파일에 쓰기
        File.WriteAllText(GetPath(), json);
        
        Debug.Log($"게임 저장 완료: {GetPath()}");
    }

    public static GameSaveData LoadGame()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            // 1. 파일 내용 읽기
            string json = File.ReadAllText(path);

            // 2. JSON을 객체로 변환
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            
            Debug.Log("게임 로드 완료.");
            return data;
        }
        else
        {
            Debug.Log("저장된 게임 파일이 없습니다. 새로운 데이터를 생성합니다.");
            return null; // 데이터 없음
        }
    }

    public static void DeleteSaveFile()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("저장 파일 삭제 완료.");
        }
    }
}
