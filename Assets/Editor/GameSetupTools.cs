using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class GameSetupTools
{
    [MenuItem("Tools/1 Fix Sprite Imports")]
    public static void FixSpriteImports()
    {
        string[] files = {
            "bones.jpeg", "bones2.jpeg", "bone1_clean.png", "bone2_clean.png",
            "background.png", "spike.png", "bonus_bone.png", "heart.png"
        };
        int fixedCount = 0;
        foreach (string f in files)
        {
            string path = "Assets/Sprites/" + f;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            fixedCount++;
        }
        Debug.Log("Fixed sprite import settings for " + fixedCount + " textures.");
    }

    [MenuItem("Tools/2 Build Level")]
    public static void BuildLevel()
    {
        if (GameObject.Find("LevelPlatforms") != null)
        {
            EditorUtility.DisplayDialog("Level already built",
                "Изтрий LevelPlatforms, LevelObstacles, LevelBones, LevelSpecials, LevelBackground и Ground_Finish/Finish от Hierarchy преди да пуснеш пак.",
                "OK");
            return;
        }

        Sprite bone1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bone1_clean.png");
        Sprite bone2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bone2_clean.png");
        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/background.png");
        Sprite spikeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spike.png");
        Sprite bonusSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bonus_bone.png");
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/heart.png");

        if (bone1 == null || bone2 == null || bg == null || spikeSprite == null || bonusSprite == null || heartSprite == null)
        {
            EditorUtility.DisplayDialog("Missing sprites", "Пусни първо Tools > 1 Fix Sprite Imports.", "OK");
            return;
        }

        GameObject platformsRoot = new GameObject("LevelPlatforms");
        GameObject obstaclesRoot = new GameObject("LevelObstacles");
        GameObject bonesRoot = new GameObject("LevelBones");
        GameObject specialsRoot = new GameObject("LevelSpecials");
        GameObject bgRoot = new GameObject("LevelBackground");

        Material dirtMat = new Material(Shader.Find("Standard"));
        dirtMat.color = new Color(0.55f, 0.36f, 0.2f);
        AssetDatabase.CreateAsset(dirtMat, "Assets/DirtMat.mat");

        foreach (string n in new string[] { "Ground", "Ground (1)", "Ground (2)" })
        {
            GameObject g = GameObject.Find(n);
            if (g != null)
            {
                MeshRenderer mr = g.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = dirtMat;
            }
        }

        string[] coinNames = { "Coin", "Coin (1)", "Coin (2)" };
        for (int i = 0; i < coinNames.Length; i++)
        {
            GameObject c = GameObject.Find(coinNames[i]);
            if (c == null) continue;
            Object.DestroyImmediate(c.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(c.GetComponent<MeshFilter>());
            SpriteRenderer sr = c.AddComponent<SpriteRenderer>();
            sr.sprite = (i % 2 == 0) ? bone1 : bone2;
            sr.sortingOrder = 5;
            float s = (i % 2 == 0) ? 0.18f : 0.16f;
            c.transform.localScale = new Vector3(s, s, 1f);
            CircleCollider2D cc = c.GetComponent<CircleCollider2D>();
            if (cc == null) cc = c.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = (i % 2 == 0) ? 1.7f : 1.9f;
        }

        float tileW = 16f;
        int tileCount = 24;
        for (int i = 0; i < tileCount; i++)
        {
            GameObject t = new GameObject("BGTile" + i);
            t.transform.parent = bgRoot.transform;
            t.transform.position = new Vector3(-12f + i * tileW, 1f, 5f);
            t.transform.localScale = new Vector3(1.02f, 1.35f, 1f);
            SpriteRenderer sr = t.AddComponent<SpriteRenderer>();
            sr.sprite = bg;
            sr.sortingOrder = -100;
        }

        GameObject MakePlatform(string name, float x, float y, float width)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            go.AddComponent<BoxCollider2D>();
            go.GetComponent<MeshRenderer>().sharedMaterial = dirtMat;
            go.transform.parent = platformsRoot.transform;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(width, 1f, 1f);
            return go;
        }

        void MakeBone(float x, float y, int variant)
        {
            GameObject b = new GameObject("Bone");
            b.transform.parent = bonesRoot.transform;
            b.transform.position = new Vector3(x, y, 0f);
            float s = (variant % 2 == 0) ? 0.18f : 0.16f;
            b.transform.localScale = new Vector3(s, s, 1f);
            SpriteRenderer sr = b.AddComponent<SpriteRenderer>();
            sr.sprite = (variant % 2 == 0) ? bone1 : bone2;
            sr.sortingOrder = 5;
            CircleCollider2D cc = b.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = (variant % 2 == 0) ? 1.7f : 1.9f;
            b.AddComponent<Coin>();
        }

        void MakeSpike(float x, float topY)
        {
            GameObject s = new GameObject("Spike");
            s.transform.parent = obstaclesRoot.transform;
            float scale = 0.7f;
            s.transform.position = new Vector3(x, topY + (1.4f * scale) / 2f, 0f);
            s.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = spikeSprite;
            sr.sortingOrder = 6;
            BoxCollider2D bc = s.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            s.AddComponent<Obstacle>();
        }

        void MakeBonus(float x, float y)
        {
            GameObject b = new GameObject("BonusBone");
            b.transform.parent = specialsRoot.transform;
            b.transform.position = new Vector3(x, y, 0f);
            float s = 0.625f;
            b.transform.localScale = new Vector3(s, s, 1f);
            SpriteRenderer sr = b.AddComponent<SpriteRenderer>();
            sr.sprite = bonusSprite;
            sr.sortingOrder = 6;
            CircleCollider2D cc = b.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.8f;
            b.AddComponent<BonusPickup>();
        }

        void MakeLife(float x, float y)
        {
            GameObject h = new GameObject("LifeHeart");
            h.transform.parent = specialsRoot.transform;
            h.transform.position = new Vector3(x, y, 0f);
            float s = 0.6f;
            h.transform.localScale = new Vector3(s, s, 1f);
            SpriteRenderer sr = h.AddComponent<SpriteRenderer>();
            sr.sprite = heartSprite;
            sr.sortingOrder = 6;
            CircleCollider2D cc = h.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.6f;
            h.AddComponent<LifePickup>();
        }

        float currentX = 13.5f;
        float currentTopY = -0.9f;
        int segments = 22;

        for (int i = 0; i < segments; i++)
        {
            float width = new float[] { 4f, 3f, 5f, 3.5f }[i % 4];

            float heightDelta = (i % 3 == 0) ? 1f : (i % 3 == 1) ? -0.5f : 0.5f;
            if (i % 6 == 5) heightDelta += 1.5f;
            currentTopY = Mathf.Clamp(currentTopY + heightDelta, -3f, 4f);

            bool bigGap = (i % 5 == 4);
            float gap = bigGap ? 7.5f : ((i % 3 == 0) ? 4f : 2.5f);

            float platformCenterX = currentX + width / 2f;
            MakePlatform("Ground_" + i, platformCenterX, currentTopY - 0.5f, width);

            if (i % 4 == 2 && i > 2)
            {
                MakeSpike(platformCenterX, currentTopY);
            }

            float gapCenterX = currentX + width + gap / 2f;
            float pickupY = currentTopY + (bigGap ? 2.5f : 1.1f);

            if (bigGap)
            {
                if (i == 4) MakeBonus(gapCenterX, pickupY);
                else if (i == 9) MakeLife(gapCenterX, pickupY);
                else if (i == 14) MakeBonus(gapCenterX, pickupY);
                else if (i == 19) MakeLife(gapCenterX, pickupY);
                else MakeBone(gapCenterX, pickupY, i);
            }
            else if (i % 2 == 0)
            {
                MakeBone(gapCenterX, pickupY, i);
            }

            currentX += width + gap;
        }

        float finishPlatformWidth = 6f;
        float finishX = currentX + finishPlatformWidth / 2f;
        MakePlatform("Ground_Finish", finishX, currentTopY - 0.5f, finishPlatformWidth);

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.name = "Finish";
        Object.DestroyImmediate(pole.GetComponent<BoxCollider>());
        BoxCollider2D poleCol = pole.AddComponent<BoxCollider2D>();
        poleCol.isTrigger = true;
        pole.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.85f, 0.2f);
        pole.transform.position = new Vector3(finishX, currentTopY + 2f, 0f);
        pole.transform.localScale = new Vector3(0.3f, 4f, 0.3f);
        pole.AddComponent<Finish>();

        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj != null)
        {
            GameManager gm = gmObj.GetComponent<GameManager>();
            if (gm != null)
            {
                GameObject playerForGm = GameObject.Find("Player");
                if (playerForGm != null) gm.player = playerForGm.transform;
                gm.spawnPoint = new Vector3(0f, 0f, 0f);

                gm.bonePickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/smb_coin.wav");
                gm.bonusPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/fronbondi_skegs-sfx-mario-brothers-like-coin-grab-sound-effect-236423.mp3");
                gm.lifePickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/smb_breakblock.wav");
                gm.hurtClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/smb_bump.wav");
                gm.finishClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/smb_flagpole.wav");
                gm.musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/main game sound.mp3");

                EditorUtility.SetDirty(gm);
            }
        }

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.jumpClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/smb_jump-small.wav");
                pc.doubleJumpClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sounds/smb_jump-super.wav");
                EditorUtility.SetDirty(pc);
            }
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Level built: " + segments + " segments, finish near X=" + finishX);
    }

    [MenuItem("Tools/3 Add Gap Variety (Safety Floors + Hazards)")]
    public static void AddGapVariety()
    {
        GameObject platformsRoot = GameObject.Find("LevelPlatforms");
        GameObject obstaclesRoot = GameObject.Find("LevelObstacles");
        if (platformsRoot == null || obstaclesRoot == null)
        {
            EditorUtility.DisplayDialog("Level not built", "Пусни първо Tools > 2 Build Level.", "OK");
            return;
        }

        if (GameObject.Find("GapFloor_0") != null || GameObject.Find("WaterHazard") != null || GameObject.Find("SpikeHazard_0") != null)
        {
            EditorUtility.DisplayDialog("Already added",
                "Gap variety изглежда вече е добавена. Изтрий обектите GapFloor_*, WaterHazard, SpikeHazard_* от Hierarchy преди да пуснеш пак.",
                "OK");
            return;
        }

        Material dirtMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/DirtMat.mat");
        Sprite spikeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spike.png");

        Material waterMat = new Material(Shader.Find("Standard"));
        waterMat.color = new Color(0.15f, 0.45f, 0.95f, 1f);
        AssetDatabase.CreateAsset(waterMat, "Assets/WaterMat.mat");

        System.Collections.Generic.List<Transform> segs = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < 22; i++)
        {
            GameObject g = GameObject.Find("Ground_" + i);
            if (g == null) { Debug.LogWarning("Missing Ground_" + i); continue; }
            segs.Add(g.transform);
        }
        GameObject finishGround = GameObject.Find("Ground_Finish");
        if (finishGround != null) segs.Add(finishGround.transform);

        int lethalA = 4;  // gap right after Ground_4 -> water hazard
        int lethalB = 14; // gap right after Ground_14 -> spike hazard

        GameObject MakeFloor(string gname, float x, float y, float width)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = gname;
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            go.AddComponent<BoxCollider2D>();
            go.GetComponent<MeshRenderer>().sharedMaterial = dirtMat;
            go.transform.parent = platformsRoot.transform;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(width, 1f, 1f);
            return go;
        }

        void MakeWaterHazard(float x, float y, float width)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "WaterHazard";
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            go.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
            go.transform.parent = obstaclesRoot.transform;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(width, 1.2f, 1f);
            go.AddComponent<Obstacle>();
        }

        void MakeSpikeRow(float centerX, float width, float topY)
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(width / 1.4f));
            float step = width / count;
            float startX = centerX - width / 2f + step / 2f;
            for (int k = 0; k < count; k++)
            {
                GameObject s = new GameObject("SpikeHazard_" + k);
                s.transform.parent = obstaclesRoot.transform;
                float scale = 1.1f;
                s.transform.position = new Vector3(startX + k * step, topY + (1.4f * scale) / 2f, 0f);
                s.transform.localScale = new Vector3(scale, scale, 1f);
                SpriteRenderer sr = s.AddComponent<SpriteRenderer>();
                sr.sprite = spikeSprite;
                sr.sortingOrder = 6;
                BoxCollider2D bc = s.AddComponent<BoxCollider2D>();
                bc.isTrigger = true;
                s.AddComponent<Obstacle>();
            }
        }

        int floorCount = 0;
        for (int i = 0; i < segs.Count - 1; i++)
        {
            Transform a = segs[i];
            Transform b = segs[i + 1];
            float gapStart = a.position.x + a.localScale.x / 2f;
            float gapEnd = b.position.x - b.localScale.x / 2f;
            float gapWidth = gapEnd - gapStart;
            if (gapWidth <= 0.05f) continue;
            float gapCenterX = (gapStart + gapEnd) / 2f;

            if (i == lethalA)
            {
                MakeWaterHazard(gapCenterX, -5.2f, gapWidth - 0.2f);
            }
            else if (i == lethalB)
            {
                MakeSpikeRow(gapCenterX, gapWidth - 0.4f, -5.6f);
            }
            else
            {
                MakeFloor("GapFloor_" + i, gapCenterX, -4.3f, gapWidth - 0.1f);
                floorCount++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Gap variety added: " + floorCount + " safety floors, lethal gaps at index " + lethalA + " (water) and " + lethalB + " (spikes).");
    }
}
