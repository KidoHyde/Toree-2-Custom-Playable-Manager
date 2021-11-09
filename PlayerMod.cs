using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using System.IO;
using System.Diagnostics;

//Yo, this is my personal mess, Huge thanks to M_Cue for helping me with the basics and answering my dumb question
//Big thanks to Marcus as well for helping answer all my game related questions on how things work, Toree devs are some of the best!

//Notes: Since I assume that the player meshes are just placed in the map some advanced itteration may be required, p check if Mesh = whatever the ingame variant is called

namespace PlayerMod
{
    static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        public static int Enbvalue = 0;

        public static DirectoryInfo files;
        public static FileInfo[] info;
        public static bool render;
        public static bool done = false;
        public static String PlayerBundleName;
        public static GameObject _CustomModel;

        public static TextAsset getcustominfo;
        public static bool CallOnce = false;
        public static bool FromMenu = false;
        //Player Data
        public static bool isHover;
        public static float PlayerSpeed;
        public static bool infJump;
        

        //Basic mod manager stuff here, nothing super noteworthy
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            render = true;


            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            //modEntry.OnUpdate = OnUpdate;

            mod = modEntry;
            Refresh(modEntry);

            return true;
        }
        
        //Gets all mod files, and refreshes them for the OnGUI call
        static void Refresh(UnityModManager.ModEntry modEntry)
        {
            files = new DirectoryInfo(Path.Combine(mod.Path, "PlayerFolder"));
            info = files.GetFiles();
            //foreach (FileInfo f in info)
            //{
            //    //mod.Logger.Log(f.ToString());
            //}
        }
        
        //Turns mod on/off, kinda useless tbh but It's a but much to remove lol
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (value)
            {
                Enbvalue = 0;

            }
            else
            {
                StopMod();
            }

            enabled = value;
            return true;
        }

        //M_Cue ref code start:
        //Sorry M_Cue, this number is too perfect not to use
        const int MaxInLine = 7;

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            //if is we have the ability to be rendered, aka not in game
            if (CanUse())
            {
                //Instatiate our default player icon if none is used
                var tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                tex.LoadImage(File.ReadAllBytes(Path.Combine(modEntry.Path, "Default.png")));
                tex.wrapMode = TextureWrapMode.Clamp;

                //Get our current player
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Current player:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(ReadString());
                GUILayout.EndHorizontal();

                //Setup our BG data
                var bg = GUI.skin.button.normal.background;
                var bg2 = GUI.skin.button.hover.background;
                var txc = GUI.skin.button.normal.textColor;
                var txhc = GUI.skin.button.hover.textColor;

                GUILayout.BeginHorizontal();
                int i = 0;
                //Load our assets
                foreach (var r in info)
                {
                    AssetBundle.UnloadAllAssetBundles(true);

                    var myLoadedAssetBundle = AssetBundle.LoadFromFile(r.ToString());
                    String rname = r.ToString().Replace(modEntry.Path + "PlayerFolder\\", "");

                    var icon = myLoadedAssetBundle.LoadAsset<Texture2D>(rname + "icon");

                    if (MaxInLine == i)
                    {
                        i = 0;
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                    else
                    {
                        i++;
                    }
                    GUI.skin.button.hover.background = (Texture2D)(icon ?? tex);
                    GUI.skin.button.normal.background = (Texture2D)(icon ?? tex);
                    GUI.skin.button.normal.textColor = ReadString() == r.ToString().Replace(modEntry.Path + "PlayerFolder\\", "") ? Color.green : Color.white;
                    Color color1 = new Color(Color.green.r - 0.3f, Color.green.g - 0.3f, Color.green.b - 0.3f);
                    Color color2 = new Color(Color.white.r - 0.3f, Color.white.g - 0.3f, Color.white.b - 0.3f);
                    GUI.skin.button.hover.textColor = ReadString() == r.ToString().Replace(modEntry.Path + "PlayerFolder\\", "") ? color1 : color2;

                    //AssetBundle.UnloadAllAssetBundles(true);
                    //Writes our player name to file
                    if (GUILayout.Button("\n\n\n\n\n\n" + rname.Replace("_", " "), GUILayout.Width(128), GUILayout.Height(128)))
                    {
                        WriteString(rname);
                        PlayerBundleName = rname;
                        //AssetBundle.UnloadAllAssetBundles(true);

                    }


                }


                GUI.skin.button.hover.background = bg;
                GUI.skin.button.normal.background = bg2;
                GUI.skin.button.normal.textColor = txc;
                GUI.skin.button.hover.textColor = txhc;
                GUILayout.EndHorizontal();

                //2 bottom buttons to open the folder and refresh player selection
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Refresh"))
                {
                    Refresh(modEntry);
                }
                if (GUILayout.Button("Open character folder"))
                {
                    Process.Start(Path.Combine(modEntry.Path, "PlayerFolder"));
                }
                GUILayout.EndHorizontal();
            }

        }
        
        static bool CanUse()
        {
            return render;
        }
        //M_Cue ref code end.


        //Writes to a file
        static void WriteString(String str)
        {

            string path = Path.Combine(mod.Path, "CurrentPlayer.txt");
            File.WriteAllText(path, String.Empty);
            //Write some text to the test.txt file

            StreamWriter writer = new StreamWriter(path, true);

            writer.WriteLine(str);

            writer.Close();

        }
        //Reads from a file
        public static String ReadString()
        {

            string path = Path.Combine(mod.Path, "CurrentPlayer.txt");

            string Name;
            //Read the text from directly from the test.txt file

            StreamReader reader = new StreamReader(path);

            //mod.Logger.Log(reader.ReadLine());

            Name = reader.ReadLine() ?? "Default";
            reader.Close();

            return Name;

        }

        //thanks again M_Cue
        //when we refrence this variable we get the CharacterSelectionScript in the scene, I make an assumption that there's only ever one, so this will only ever get that one
        static CharacterSelectionScript _GlobalPlyMgr = null;
        static CharacterSelectionScript GlobalPlyMgr
        {
            get
            {
                if (_GlobalPlyMgr is CharacterSelectionScript)
                    return _GlobalPlyMgr;


                //mod.Logger.Log("Called");
                done = false;
                //OnMenu = true;

                var plymgr = GameObject.FindObjectsOfType<CharacterSelectionScript>();
                foreach (var p in plymgr)
                {
                    _GlobalPlyMgr = p;
                    return _GlobalPlyMgr;
                }
                return null; // nothing found lol;
            }
        }
        
        //Same thing as above just for PlayerSystem instead
        static PlayerSystem _GlobalPly = null;
        static PlayerSystem GlobalPly
        {
            get
            {
                if (_GlobalPly is PlayerSystem)
                    return _GlobalPly;
                var ply = GameObject.FindObjectsOfType<PlayerSystem>();
                foreach (var p in ply)
                {
                    _GlobalPly = p;
                    return _GlobalPly;
                }
                return null; // nothing found lol;
            }
        }

        //Mod is no longer active
        static bool StopMod()
        {
            Enbvalue = 1;

            return true;
        }

        //Start menu on initial boot up, for some unknown reason the start menu is loaded once here and then NEVER again after that, weird but just gotta work around it.
        [HarmonyPatch(typeof(TitleSelectScript), "Start", new System.Type[] { })]
        static class TitleSelectScript_Start_Patch
        {
            internal static bool Prefix()
            {
                //mod.Logger.Log("Called Start of Menu");
                CallOnce = false;
                FromMenu = true;
                return true;
            }
        }


        //When we get to the level select menu disable the ability to change in options and to make it so we no longer do the weird start menu
        [HarmonyPatch(typeof(LevelSelectScript), "Start", new System.Type[] { })]
        static class LevelSelectScript_Start_Patch
        {
            internal static void Postfix()
            {
                if (PlayerBundleName == null)
                {
                    PlayerBundleName = ReadString();
                }
                CallOnce = false;
                FromMenu = false;
                render = false;
                //mod.Logger.Log("Just Called False!");
            }
        }


        //CharacterSelectionScript CHANGES START:
        [HarmonyPatch(typeof(CharacterSelectionScript), "ChangeInfoPanel", new System.Type[] { })]
        static class CharacterSelectionScript_ChangeInfoPanel_Patch
        {
            internal static bool Prefix()
            {
                if (Enbvalue == 0)
                {
                    if (GlobalPlyMgr == null)
                    {
                        _GlobalPlyMgr = null;
                        return false;
                    }
                    CharacterSelectionScript Gbl = GlobalPlyMgr;

                    var typ = typeof(CharacterSelectionScript);
                    //use reflection to get private variable
                    FieldInfo type = typ.GetField("currentChar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblcurrentChar = (int)type.GetValue(Gbl);

                    FieldInfo type2 = typ.GetField("weOwnMacbat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnMacbat = (int)type2.GetValue(Gbl);


                    FieldInfo type3 = typ.GetField("weOwnTasty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnTasty = (int)type3.GetValue(Gbl);


                    FieldInfo type4 = typ.GetField("weOwnGlitchy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnGlitchy = (int)type4.GetValue(Gbl);

                    if (!render)
                    {
                        //We can render our options again
                        render = true;
                        //mod.Logger.Log("Just Called True!");

                    }

                    //If we haven't done our check for the initial creation and movements hasn't been done then manually move em
                    //I did the math and I man, I hate math
                    //GblCurrentChar is what player we have selected/are hovering over,
                    //0: Toree, 1:Macbat, 2: Tasty, 3: Glitchy, 4: Custom
                    if(!done)
                    {
                        Quaternion outrot;
                        if (GblcurrentChar == 0)
                        {
                            //Toree
                            Gbl._character1animation.transform.position = new Vector3(0, Gbl._character1animation.transform.position.y, 3);
                            outrot = Quaternion.Euler(0, 180, 0);
                            Gbl._character1animation.transform.rotation = outrot;
                            //Macbat
                            Gbl.Macbat.transform.position = new Vector3(1.47414f, Gbl.Macbat.transform.position.y, 4.07102f);
                            Gbl.Macbat_Shadow.transform.position = new Vector3(1.47414f, Gbl.Macbat_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -90 - 72, 0);
                            Gbl.Macbat.transform.rotation = outrot;
                            Gbl.Macbat_Shadow.transform.rotation = outrot;
                            //Tasty
                            Gbl.Tasty.transform.position = new Vector3(0.911068f, Gbl.Tasty.transform.position.y, 5.80398f);
                            Gbl.Tasty_Shadow.transform.position = new Vector3(0.911068f, Gbl.Tasty_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -90 - (72 * 2), 0);
                            Gbl.Tasty.transform.rotation = outrot;
                            Gbl.Tasty_Shadow.transform.rotation = outrot;
                            //Glitchy
                            Gbl.Glitchy.transform.position = new Vector3(-0.911067f, Gbl.Glitchy.transform.position.y, 5.80398f);
                            Gbl.Glitchy_Shadow.transform.position = new Vector3(-0.911067f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -(72 * 3), 0);
                            Gbl.Glitchy.transform.rotation = outrot;
                            Gbl.Glitchy_Shadow.transform.rotation = outrot;
                            //Custom
                            var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                            Question.gameObject.SetActive(true);
                            Question.transform.position = new Vector3(-1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -(72 * 4), 0);
                            Question.transform.rotation = outrot;
                            Question.transform.parent = Gbl.CharacterWheel;
                        }
                        else if (GblcurrentChar == 1)
                        {
                            //Toree
                            Gbl._character1animation.transform.position = new Vector3(-1.47414f, Gbl._character1animation.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, 180+72, 0);
                            Gbl._character1animation.transform.rotation = outrot;
                            //Macbat
                            Gbl.Macbat.transform.position = new Vector3(0, Gbl.Macbat.transform.position.y, 3);
                            Gbl.Macbat_Shadow.transform.position = new Vector3(0, Gbl.Macbat_Shadow.transform.position.y, 3);
                            outrot = Quaternion.Euler(0, -90, 0);
                            Gbl.Macbat.transform.rotation = outrot;
                            Gbl.Macbat_Shadow.transform.rotation = outrot;
                            //Tasty
                            Gbl.Tasty.transform.position = new Vector3(1.47414f, Gbl.Tasty.transform.position.y, 4.07102f);
                            Gbl.Tasty_Shadow.transform.position = new Vector3(1.47414f, Gbl.Tasty_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -90 - 72, 0);
                            Gbl.Tasty.transform.rotation = outrot;
                            Gbl.Tasty_Shadow.transform.rotation = outrot;
                            //Glitchy
                            Gbl.Glitchy.transform.position = new Vector3(0.911068f, Gbl.Glitchy.transform.position.y, 5.80398f);
                            Gbl.Glitchy_Shadow.transform.position = new Vector3(0.911068f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -(72 * 2), 0);
                            Gbl.Glitchy_Shadow.transform.rotation = outrot;
                            //Custom
                            var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                            Question.gameObject.SetActive(true);
                            Question.transform.position = new Vector3(-0.911067f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -(72 * 3), 0);
                            Question.transform.rotation = outrot;
                            Question.transform.parent = Gbl.CharacterWheel;
                        }
                        else if (GblcurrentChar == 2)
                        {
                            //Toree
                            Gbl._character1animation.transform.position = new Vector3(-0.911067f, Gbl._character1animation.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, 180+(72*2), 0);
                            Gbl._character1animation.transform.rotation = outrot;
                            //Macbat
                            Gbl.Macbat.transform.position = new Vector3(-1.47414f, Gbl.Macbat.transform.position.y, 4.07102f);
                            Gbl.Macbat_Shadow.transform.position = new Vector3(-1.47414f, Gbl.Macbat_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -90-(72 * 4), 0);
                            Gbl.Macbat.transform.rotation = outrot;
                            Gbl.Macbat_Shadow.transform.rotation = outrot;
                            //Tasty
                            Gbl.Tasty.transform.position = new Vector3(0, Gbl.Tasty.transform.position.y, 3);
                            Gbl.Tasty_Shadow.transform.position = new Vector3(0, Gbl.Tasty_Shadow.transform.position.y, 3);
                            outrot = Quaternion.Euler(0, -90, 0);
                            Gbl.Tasty.transform.rotation = outrot;
                            Gbl.Tasty_Shadow.transform.rotation = outrot;
                            //Glitchy
                            Gbl.Glitchy.transform.position = new Vector3(1.47414f, Gbl.Glitchy.transform.position.y, 4.07102f);
                            Gbl.Glitchy_Shadow.transform.position = new Vector3(1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -72, 0);
                            Gbl.Glitchy.transform.rotation = outrot;
                            Gbl.Glitchy_Shadow.transform.rotation = outrot;
                            //Custom
                            var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                            Question.gameObject.SetActive(true);
                            Question.transform.position = new Vector3(0.911068f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -(72 * 2), 0);
                            Question.transform.rotation = outrot;
                            Question.transform.parent = Gbl.CharacterWheel;
                        }
                        else if (GblcurrentChar == 3)
                        {
                            //Toree
                            Gbl._character1animation.transform.position = new Vector3(0.911068f, Gbl._character1animation.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, 180 + (72 * 3), 0);
                            Gbl._character1animation.transform.rotation = outrot;
                            //Macbat
                            Gbl.Macbat.transform.position = new Vector3(-0.911067f, Gbl.Macbat.transform.position.y, 5.80398f);
                            Gbl.Macbat_Shadow.transform.position = new Vector3(-0.911067f, Gbl.Macbat_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -90 - (72 * 3), 0);
                            Gbl.Macbat.transform.rotation = outrot;
                            Gbl.Macbat_Shadow.transform.rotation = outrot;
                            //Tasty
                            Gbl.Tasty.transform.position = new Vector3(-1.47414f, Gbl.Tasty.transform.position.y, 4.07102f);
                            Gbl.Tasty_Shadow.transform.position = new Vector3(-1.47414f, Gbl.Tasty_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -90 - (72 * 4), 0);
                            Gbl.Tasty.transform.rotation = outrot;
                            Gbl.Tasty_Shadow.transform.rotation = outrot;
                            //Glitchy
                            Gbl.Glitchy.transform.position = new Vector3(0, Gbl.Glitchy.transform.position.y, 3);
                            Gbl.Glitchy_Shadow.transform.position = new Vector3(0, Gbl.Glitchy_Shadow.transform.position.y, 3);
                            outrot = Quaternion.Euler(0, 0, 0);
                            Gbl.Glitchy.transform.rotation = outrot;
                            Gbl.Glitchy_Shadow.transform.rotation = outrot;
                            //Custom
                            var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                            Question.gameObject.SetActive(true);
                            Question.transform.position = new Vector3(1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -72, 0);
                            Question.transform.rotation = outrot;
                            Question.transform.parent = Gbl.CharacterWheel;
                        }
                        else if (GblcurrentChar == 4)
                        {
                            //Toree
                            Gbl._character1animation.transform.position = new Vector3(1.47414f, Gbl._character1animation.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, 180 + (72 * 4), 0);
                            Gbl._character1animation.transform.rotation = outrot;
                            //Macbat
                            Gbl.Macbat.transform.position = new Vector3(0.911068f, Gbl.Macbat.transform.position.y, 5.80398f);
                            Gbl.Macbat_Shadow.transform.position = new Vector3(0.911068f, Gbl.Macbat_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -90 - (72 * 2), 0);
                            Gbl.Macbat.transform.rotation = outrot;
                            Gbl.Macbat_Shadow.transform.rotation = outrot;
                            //Tasty
                            Gbl.Tasty.transform.position = new Vector3(-0.911067f, Gbl.Tasty.transform.position.y, 5.80398f);
                            Gbl.Tasty_Shadow.transform.position = new Vector3(-0.911067f, Gbl.Tasty_Shadow.transform.position.y, 5.80398f);
                            outrot = Quaternion.Euler(0, -90 - (72 * 3), 0);
                            Gbl.Tasty.transform.rotation = outrot;
                            Gbl.Tasty_Shadow.transform.rotation = outrot;
                            //Glitchy
                            Gbl.Glitchy.transform.position = new Vector3(-1.47414f, Gbl.Glitchy.transform.position.y, 4.07102f);
                            Gbl.Glitchy_Shadow.transform.position = new Vector3(-1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                            outrot = Quaternion.Euler(0, -(72 * 4), 0);
                            Gbl.Glitchy.transform.rotation = outrot;
                            Gbl.Glitchy_Shadow.transform.rotation = outrot;
                            //Custom
                            var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                            Question.gameObject.SetActive(true);
                            Question.transform.position = new Vector3(0, Gbl.Glitchy_Shadow.transform.position.y, 3);
                            outrot = Quaternion.Euler(0, 0, 0);
                            Question.transform.rotation = outrot;
                            Question.transform.parent = Gbl.CharacterWheel;
                        }

                    }


                    if (GblcurrentChar == 0)
                    {
                        GlobalPlyMgr.InfoPanelName.text = "Toree";
                        GlobalPlyMgr.InfoPanelName_Shadow.text = "Toree";
                        GlobalPlyMgr.InfoPanelFeature.text = "- is a bird!";
                    }
                    else if (GblcurrentChar == 1)
                    {
                        if (GblOwnMacbat == 1)
                        {
                            GlobalPlyMgr.InfoPanelName.text = "Macbat";
                            GlobalPlyMgr.InfoPanelName_Shadow.text = "Macbat";
                            GlobalPlyMgr.InfoPanelFeature.text = "- endless jumps";
                            return false;
                        }
                        GlobalPlyMgr.InfoPanelName.text = "???";
                        GlobalPlyMgr.InfoPanelName_Shadow.text = "???";
                        GlobalPlyMgr.InfoPanelFeature.text = "- ???";
                    }
                    else if (GblcurrentChar == 2)
                    {
                        if (GblOwnTasty == 1)
                        {
                            GlobalPlyMgr.InfoPanelName.text = "Tasty";
                            GlobalPlyMgr.InfoPanelName_Shadow.text = "Tasty";
                            GlobalPlyMgr.InfoPanelFeature.text = "- higher speed";
                            return false;
                        }
                        GlobalPlyMgr.InfoPanelName.text = "???";
                        GlobalPlyMgr.InfoPanelName_Shadow.text = "???";
                        GlobalPlyMgr.InfoPanelFeature.text = "- ???";
                    }
                    else if (GblcurrentChar == 3)
                    {
                        if (GblOwnGlitchy == 1)
                        {
                            GlobalPlyMgr.InfoPanelName.text = "Glitchy";
                            GlobalPlyMgr.InfoPanelName_Shadow.text = "Glitchy";
                            GlobalPlyMgr.InfoPanelFeature.text = "- even higher speed \n - endless jumps \n - ridiculous...";
                            return false;
                        }
                        GlobalPlyMgr.InfoPanelName.text = "???";
                        GlobalPlyMgr.InfoPanelName_Shadow.text = "???";
                        GlobalPlyMgr.InfoPanelFeature.text = "- ???";
                    }
                    else if (GblcurrentChar == 4)
                    {
                        //Add our custom character info
                        GlobalPlyMgr.InfoPanelName.text = "Custom";
                        GlobalPlyMgr.InfoPanelName_Shadow.text = "Custom";
                        GlobalPlyMgr.InfoPanelFeature.text = "- Be sure to select this in the mod loader!";
                        return false;
                    }

                    return false;

                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(CharacterSelectionScript), "ChoseCharacter", new System.Type[] { })]
        static class CharacterSelectionScript_ChoseCharacter_Patch
        {
            internal static bool Prefix()
            {
                if (Enbvalue == 0)
                {
                    if (GlobalPlyMgr == null)
                    {
                        _GlobalPlyMgr = null;
                        return false;
                    }

                    CharacterSelectionScript Gbl = GlobalPlyMgr;
                    var typ = typeof(CharacterSelectionScript);

                    FieldInfo type = typ.GetField("currentChar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblcurrentChar = (int)type.GetValue(Gbl);

                    FieldInfo type2 = typ.GetField("weOwnMacbat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnMacbat = (int)type2.GetValue(Gbl);

                    FieldInfo type3 = typ.GetField("weOwnTasty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnTasty = (int)type3.GetValue(Gbl);

                    FieldInfo type4 = typ.GetField("weOwnGlitchy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnGlitchy = (int)type4.GetValue(Gbl);

                    FieldInfo type5 = typ.GetField("jumpToNext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    //Mostly rewritten from Toree code, I just added 4 and made the continue node
                    if (GblcurrentChar == 0)
                    {
                        GamePadScript.instance.SetInt("ChosenCharacter", 0);
                        type5.SetValue(Gbl, true);
                        GlobalPlyMgr.blackScreenAnimator.SetBool("isOpen", false);
                        return false;
                    }
                    else if (GblcurrentChar == 1)
                    {
                        if (GblOwnMacbat == 1)
                        {
                            GamePadScript.instance.SetInt("ChosenCharacter", 1);
                            type5.SetValue(Gbl, true);
                            GlobalPlyMgr.blackScreenAnimator.SetBool("isOpen", false);
                            return false;
                        }
                        GlobalPlyMgr.DontOwn.Play();
                        return false;
                    }
                    else if (GblcurrentChar == 2)
                    {
                        if (GblOwnTasty == 1)
                        {
                            GamePadScript.instance.SetInt("ChosenCharacter", 2);
                            type5.SetValue(Gbl, true);
                            GlobalPlyMgr.blackScreenAnimator.SetBool("isOpen", false);
                            return false;
                        }
                        GlobalPlyMgr.DontOwn.Play();
                        return false;
                    }
                    else if (GblcurrentChar == 3)
                    {
                        if (GblOwnGlitchy == 1)
                        {
                            GamePadScript.instance.SetInt("ChosenCharacter", 3);
                            type5.SetValue(Gbl, true);
                            GlobalPlyMgr.blackScreenAnimator.SetBool("isOpen", false);
                            return false;
                        }
                        GlobalPlyMgr.DontOwn.Play();
                        return false;
                    }
                    else if (GblcurrentChar == 4)
                    {
                        GamePadScript.instance.SetInt("ChosenCharacter", 4);
                        //Set this true when renderer is done
                        type5.SetValue(Gbl, true);
                        GlobalPlyMgr.blackScreenAnimator.SetBool("isOpen", false);
                        return false;
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(CharacterSelectionScript), "ManageCharacterWheel", new System.Type[] { })]
        static class CharacterSelectionScript_ManageCharacterWheel_Patch
        {
            internal static bool Prefix()
            {
                if (Enbvalue == 0)
                {
                    if (GlobalPlyMgr == null)
                    {
                        _GlobalPlyMgr = null;
                        return false;
                    }
                    CharacterSelectionScript Gbl = GlobalPlyMgr;
                    var typ = typeof(CharacterSelectionScript);

                    FieldInfo type = typ.GetField("currentChar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblcurrentChar = (int)type.GetValue(Gbl);

                    //handles the camera, when it's from the Main menu we don't need to deal withplayer 4 camera for some reason
                    //But no matter where we start from we set the camera location manually once then update based on that position
                    float y = 0f;
                    if (!FromMenu)
                    {
                        if (GblcurrentChar == 0)
                        {
                            y = 0f;
                        }
                        else if (GblcurrentChar == 1)
                        {
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, 72f, 0f);
                            }
                            y = 72f;
                        }
                        else if (GblcurrentChar == 2)
                        {
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, 144f, 0f); ;
                            }
                            y = 144f;
                        }
                        else if (GblcurrentChar == 3)
                        {
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, 216f, 0f);
                            }
                            y = 216f;
                        }
                        else if (GblcurrentChar == 4)
                        {
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, 288f, 0f);
                            }
                            y = 288f;
                        }
                    }
                    else
                    {
                        if (GblcurrentChar == 0)
                        {
                            y = 0f;
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, y, 0f);
                            }
                        }
                        else if (GblcurrentChar == 1)
                        {
                            y = 72f;
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, y, 0f);
                            }
                        }
                        else if (GblcurrentChar == 2)
                        {
                            y = 144f;
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, y, 0f);
                            }
                        }
                        else if (GblcurrentChar == 3)
                        {
                            y = 216f;
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, y, 0f);
                            }
                        }
                        else if (GblcurrentChar == 4)
                        {
                            y = 288f;
                            if (!CallOnce)
                            {
                                Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, y, 0f);
                                //mod.Logger.Log("Called Initial Camera Set");
                            }
                        }
                    }

                    Quaternion b = Quaternion.Euler(0f, y, 0f);
                    //mod.Logger.Log("Called b for cameralocation");
                    Gbl.CharacterWheel.localRotation = Quaternion.Lerp(Gbl.CharacterWheel.localRotation, b, Time.deltaTime * 5f);

                   
                    CallOnce = true;

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(CharacterSelectionScript), "Start", new System.Type[] { })]
        static class CharacterSelectionScript_Start_Patch
        {
            internal static void Postfix()
            {
                if (Enbvalue == 0)
                {

                    if (GlobalPlyMgr == null)
                    {
                        _GlobalPlyMgr = null;
                        return;
                    }

                    CharacterSelectionScript Gbl = GlobalPlyMgr;
                    var typ = typeof(CharacterSelectionScript);

                    FieldInfo type = typ.GetField("currentChar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblcurrentChar = (int)type.GetValue(Gbl);

                    FieldInfo type2 = typ.GetField("weOwnMacbat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnMacbat = (int)type2.GetValue(Gbl);

                    FieldInfo type3 = typ.GetField("weOwnTasty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnTasty = (int)type3.GetValue(Gbl);

                    FieldInfo type4 = typ.GetField("weOwnGlitchy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblOwnGlitchy = (int)type4.GetValue(Gbl);

                    //Same idea as above just only needs to happen once
                    float num = 0f;
                    if (GblcurrentChar == 0)
                    {
                        num = 0f;
                        Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, num, 0f);
                    }
                    else if (GblcurrentChar == 1)
                    {
                        num = 72f;
                        Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, num, 0f);
                    }
                    else if (GblcurrentChar == 2)
                    {
                        num = 144f;
                        Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, num, 0f);
                    }
                    else if (GblcurrentChar == 3)
                    {
                        num = 216f;
                        Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, num, 0f);
                    }
                    else if (GblcurrentChar == 4)
                    {
                        num = 288f;
                        Gbl.CharacterWheel.localRotation = Quaternion.Euler(0f, num, 0f);
                    }
                    //mod.Logger.Log("Called num for cameralocation");

                    //mod.Logger.Log(Gbl._character1animation.transform.rotation.eulerAngles.ToString());

                    Quaternion outrot;
                    if (GblcurrentChar == 0)
                    {
                        //Toree
                        Gbl._character1animation.transform.position = new Vector3(0, Gbl._character1animation.transform.position.y, 3);
                        outrot = Quaternion.Euler(0, 180, 0);
                        Gbl._character1animation.transform.rotation = outrot;
                        //Macbat
                        Gbl.Macbat.transform.position = new Vector3(1.47414f, Gbl.Macbat.transform.position.y, 4.07102f);
                        Gbl.Macbat_Shadow.transform.position = new Vector3(1.47414f, Gbl.Macbat_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -90 - 72, 0);
                        Gbl.Macbat.transform.rotation = outrot;
                        Gbl.Macbat_Shadow.transform.rotation = outrot;
                        //Tasty
                        Gbl.Tasty.transform.position = new Vector3(0.911068f, Gbl.Tasty.transform.position.y, 5.80398f);
                        Gbl.Tasty_Shadow.transform.position = new Vector3(0.911068f, Gbl.Tasty_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -90 - (72 * 2), 0);
                        Gbl.Tasty.transform.rotation = outrot;
                        Gbl.Tasty_Shadow.transform.rotation = outrot;
                        //Glitchy
                        Gbl.Glitchy.transform.position = new Vector3(-0.911067f, Gbl.Glitchy.transform.position.y, 5.80398f);
                        Gbl.Glitchy_Shadow.transform.position = new Vector3(-0.911067f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -(72 * 3), 0);
                        Gbl.Glitchy.transform.rotation = outrot;
                        Gbl.Glitchy_Shadow.transform.rotation = outrot;
                        //Custom
                        var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                        Question.gameObject.SetActive(true);
                        Question.transform.position = new Vector3(-1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -(72 * 4), 0);
                        Question.transform.rotation = outrot;
                        Question.transform.parent = Gbl.CharacterWheel;
                    }
                    else if (GblcurrentChar == 1)
                    {
                        //Toree
                        Gbl._character1animation.transform.position = new Vector3(-1.47414f, Gbl._character1animation.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, 180 + 72, 0);
                        Gbl._character1animation.transform.rotation = outrot;
                        //Macbat
                        Gbl.Macbat.transform.position = new Vector3(0, Gbl.Macbat.transform.position.y, 3);
                        Gbl.Macbat_Shadow.transform.position = new Vector3(0, Gbl.Macbat_Shadow.transform.position.y, 3);
                        outrot = Quaternion.Euler(0, -90, 0);
                        Gbl.Macbat.transform.rotation = outrot;
                        Gbl.Macbat_Shadow.transform.rotation = outrot;
                        //Tasty
                        Gbl.Tasty.transform.position = new Vector3(1.47414f, Gbl.Tasty.transform.position.y, 4.07102f);
                        Gbl.Tasty_Shadow.transform.position = new Vector3(1.47414f, Gbl.Tasty_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -90 - 72, 0);
                        Gbl.Tasty.transform.rotation = outrot;
                        Gbl.Tasty_Shadow.transform.rotation = outrot;
                        //Glitchy
                        Gbl.Glitchy.transform.position = new Vector3(0.911068f, Gbl.Glitchy.transform.position.y, 5.80398f);
                        Gbl.Glitchy_Shadow.transform.position = new Vector3(0.911068f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -(72 * 2), 0);
                        Gbl.Glitchy_Shadow.transform.rotation = outrot;
                        //Custom
                        var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                        Question.gameObject.SetActive(true);
                        Question.transform.position = new Vector3(-0.911067f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -(72 * 3), 0);
                        Question.transform.rotation = outrot;
                        Question.transform.parent = Gbl.CharacterWheel;
                    }
                    else if (GblcurrentChar == 2)
                    {
                        //Toree
                        Gbl._character1animation.transform.position = new Vector3(-0.911067f, Gbl._character1animation.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, 180 + (72 * 2), 0);
                        Gbl._character1animation.transform.rotation = outrot;
                        //Macbat
                        Gbl.Macbat.transform.position = new Vector3(-1.47414f, Gbl.Macbat.transform.position.y, 4.07102f);
                        Gbl.Macbat_Shadow.transform.position = new Vector3(-1.47414f, Gbl.Macbat_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -90 - (72 * 4), 0);
                        Gbl.Macbat.transform.rotation = outrot;
                        Gbl.Macbat_Shadow.transform.rotation = outrot;
                        //Tasty
                        Gbl.Tasty.transform.position = new Vector3(0, Gbl.Tasty.transform.position.y, 3);
                        Gbl.Tasty_Shadow.transform.position = new Vector3(0, Gbl.Tasty_Shadow.transform.position.y, 3);
                        outrot = Quaternion.Euler(0, -90, 0);
                        Gbl.Tasty.transform.rotation = outrot;
                        Gbl.Tasty_Shadow.transform.rotation = outrot;
                        //Glitchy
                        Gbl.Glitchy.transform.position = new Vector3(1.47414f, Gbl.Glitchy.transform.position.y, 4.07102f);
                        Gbl.Glitchy_Shadow.transform.position = new Vector3(1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -72, 0);
                        Gbl.Glitchy.transform.rotation = outrot;
                        Gbl.Glitchy_Shadow.transform.rotation = outrot;
                        //Custom
                        var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                        Question.gameObject.SetActive(true);
                        Question.transform.position = new Vector3(0.911068f, Gbl.Glitchy_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -(72 * 2), 0);
                        Question.transform.rotation = outrot;
                        Question.transform.parent = Gbl.CharacterWheel;
                    }
                    else if (GblcurrentChar == 3)
                    {
                        //Toree
                        Gbl._character1animation.transform.position = new Vector3(0.911068f, Gbl._character1animation.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, 180 + (72 * 3), 0);
                        Gbl._character1animation.transform.rotation = outrot;
                        //Macbat
                        Gbl.Macbat.transform.position = new Vector3(-0.911067f, Gbl.Macbat.transform.position.y, 5.80398f);
                        Gbl.Macbat_Shadow.transform.position = new Vector3(-0.911067f, Gbl.Macbat_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -90 - (72 * 3), 0);
                        Gbl.Macbat.transform.rotation = outrot;
                        Gbl.Macbat_Shadow.transform.rotation = outrot;
                        //Tasty
                        Gbl.Tasty.transform.position = new Vector3(-1.47414f, Gbl.Tasty.transform.position.y, 4.07102f);
                        Gbl.Tasty_Shadow.transform.position = new Vector3(-1.47414f, Gbl.Tasty_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -90 - (72 * 4), 0);
                        Gbl.Tasty.transform.rotation = outrot;
                        Gbl.Tasty_Shadow.transform.rotation = outrot;
                        //Glitchy
                        Gbl.Glitchy.transform.position = new Vector3(0, Gbl.Glitchy.transform.position.y, 3);
                        Gbl.Glitchy_Shadow.transform.position = new Vector3(0, Gbl.Glitchy_Shadow.transform.position.y, 3);
                        outrot = Quaternion.Euler(0, 0, 0);
                        Gbl.Glitchy.transform.rotation = outrot;
                        Gbl.Glitchy_Shadow.transform.rotation = outrot;
                        //Custom
                        var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                        Question.gameObject.SetActive(true);
                        Question.transform.position = new Vector3(1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -72, 0);
                        Question.transform.rotation = outrot;
                        Question.transform.parent = Gbl.CharacterWheel;
                    }
                    else if (GblcurrentChar == 4)
                    {
                        //Toree
                        Gbl._character1animation.transform.position = new Vector3(1.47414f, Gbl._character1animation.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, 180 + (72 * 4), 0);
                        Gbl._character1animation.transform.rotation = outrot;
                        //Macbat
                        Gbl.Macbat.transform.position = new Vector3(0.911068f, Gbl.Macbat.transform.position.y, 5.80398f);
                        Gbl.Macbat_Shadow.transform.position = new Vector3(0.911068f, Gbl.Macbat_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -90 - (72 * 2), 0);
                        Gbl.Macbat.transform.rotation = outrot;
                        Gbl.Macbat_Shadow.transform.rotation = outrot;
                        //Tasty
                        Gbl.Tasty.transform.position = new Vector3(-0.911067f, Gbl.Tasty.transform.position.y, 5.80398f);
                        Gbl.Tasty_Shadow.transform.position = new Vector3(-0.911067f, Gbl.Tasty_Shadow.transform.position.y, 5.80398f);
                        outrot = Quaternion.Euler(0, -90 - (72 * 3), 0);
                        Gbl.Tasty.transform.rotation = outrot;
                        Gbl.Tasty_Shadow.transform.rotation = outrot;
                        //Glitchy
                        Gbl.Glitchy.transform.position = new Vector3(-1.47414f, Gbl.Glitchy.transform.position.y, 4.07102f);
                        Gbl.Glitchy_Shadow.transform.position = new Vector3(-1.47414f, Gbl.Glitchy_Shadow.transform.position.y, 4.07102f);
                        outrot = Quaternion.Euler(0, -(72 * 4), 0);
                        Gbl.Glitchy.transform.rotation = outrot;
                        Gbl.Glitchy_Shadow.transform.rotation = outrot;
                        //Custom
                        var Question = GameObject.Instantiate(Gbl.Glitchy_Shadow, new Vector3(0, 0, 0), Quaternion.identity);
                        Question.gameObject.SetActive(true);
                        Question.transform.position = new Vector3(0, Gbl.Glitchy_Shadow.transform.position.y, 3);
                        outrot = Quaternion.Euler(0, 0, 0);
                        Question.transform.rotation = outrot;
                        Question.transform.parent = Gbl.CharacterWheel;
                    }
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(CharacterSelectionScript), "Update", new System.Type[] { })]
        static class CharacterSelectionScript_Update_Patch
        {
            internal static bool Prefix()
            {
                if (Enbvalue == 0)
                {
                    if (GlobalPlyMgr == null)
                    {
                        _GlobalPlyMgr = null;
                        return false;
                    }
                    CharacterSelectionScript Gbl = GlobalPlyMgr;
                    var typ = typeof(CharacterSelectionScript);

                    FieldInfo type = typ.GetField("currentChar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblcurrentChar = (int)type.GetValue(Gbl);

                    FieldInfo type2 = typ.GetField("jumpToLast", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bool GblJumpToLast = (bool)type2.GetValue(Gbl);

                    FieldInfo type3 = typ.GetField("jumpToNext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bool GblJumpToNext = (bool)type3.GetValue(Gbl);

                    FieldInfo type4 = typ.GetField("horizontallInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    float GblHoriz = (float)type4.GetValue(Gbl);

                    FieldInfo type5 = typ.GetField("inputDeadZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    float GblinputDeadZone = (float)type5.GetValue(Gbl);


                    if (!GblJumpToLast && !GblJumpToNext)
                    {
                        type4.SetValue(Gbl, GamePadScript.instance.xAxis());
                        GblHoriz = GamePadScript.instance.xAxis();
                        if (GblHoriz < GblinputDeadZone && GblHoriz > -GblinputDeadZone)
                        {
                            type4.SetValue(Gbl, 0f);
                        }
                        var _method = Gbl.GetType().GetMethod("ChangeCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                        _method.Invoke(Gbl, new object[] { });
                        _method = Gbl.GetType().GetMethod("ManageCharacterWheel", BindingFlags.NonPublic | BindingFlags.Instance);
                        _method.Invoke(Gbl, new object[] { });
                        if (!done)
                        {
                            //Quick preliminary fix to call the ChangeInfoPanel
                            _method = Gbl.GetType().GetMethod("ChangeInfoPanel", BindingFlags.NonPublic | BindingFlags.Instance);
                            _method.Invoke(Gbl, new object[] { });
                            //mod.Logger.Log("Just invoked update");
                            done = true;
                        }

                        if (GamePadScript.instance.jumpButton() || Input.GetButtonDown("KeyboardUse"))
                        {
                            _method = Gbl.GetType().GetMethod("ChoseCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                            _method.Invoke(Gbl, new object[] { });
                        }
                        if (GamePadScript.instance.cancelButton())
                        {
                            type2.SetValue(Gbl, true);
                            Gbl.blackScreenAnimator.SetBool("isOpen", false);
                        }
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }


        [HarmonyPatch(typeof(CharacterSelectionScript), "ChangeCharacter", new System.Type[] { })]
        static class CharacterSelectionScript_ChangeCharacter_Patch
        {
            internal static bool Prefix()
            {
                if (Enbvalue == 0)
                {
                    if (GlobalPlyMgr == null)
                    {
                        _GlobalPlyMgr = null;
                        return false;
                    }
                    CharacterSelectionScript Gbl = GlobalPlyMgr;
                    var typ = typeof(CharacterSelectionScript);

                    FieldInfo type = typ.GetField("currentChar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int GblcurrentChar = (int)type.GetValue(Gbl);

                    FieldInfo type2 = typ.GetField("horizontallInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    float GblHoriz = (float)type2.GetValue(Gbl);

                    FieldInfo type3 = typ.GetField("inputcooldown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    float Gblinputcooldown = (float)type3.GetValue(Gbl);

                    if (GblHoriz > 0.2f && Gblinputcooldown < 0f)
                    {
                        Gbl.selectSound.Play();

                        GblcurrentChar++;
                        type.SetValue(Gbl, GblcurrentChar);

                        type3.SetValue(Gbl, 0.25f);
                        type2.SetValue(Gbl, 0f);
                    }
                    else if (GblHoriz < -0.2f && Gblinputcooldown < 0f)
                    {
                        Gbl.selectSound.Play();

                        GblcurrentChar--;
                        type.SetValue(Gbl, GblcurrentChar);

                        type3.SetValue(Gbl, 0.25f);
                        type2.SetValue(Gbl, 0f);
                    }

                    //Changes the inputs to allow for it to go up to 4 instead of 3
                    //mod.Logger.Log(GblcurrentChar.ToString());
                    if (GblcurrentChar < 0)
                    {
                        GblcurrentChar = 4;
                        type.SetValue(Gbl, 4);
                    }
                    if (GblcurrentChar > 4)
                    {
                        GblcurrentChar = 0;
                        type.SetValue(Gbl, 0);
                    }

                    if (Gblinputcooldown == 0.25f)
                    {
                        var _method = Gbl.GetType().GetMethod("ChangeInfoPanel", BindingFlags.NonPublic | BindingFlags.Instance);
                        _method.Invoke(Gbl, new object[] { });
                    }
                    if (Gblinputcooldown > 0f)
                    {
                        Gblinputcooldown -= 1f * Time.deltaTime;

                        type3.SetValue(Gbl, Gblinputcooldown -= 1f * Time.deltaTime);
                    }

                    return false;

                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(CharacterSelectionScript), "FixedUpdate", new System.Type[] { })]
        static class CharacterSelectionScript_FixedUpdate_Patch
        {
            internal static bool Prefix()
            {
                if (Enbvalue == 0)
                {
                    if (GlobalPlyMgr == null)
                    {
                        _GlobalPlyMgr = null;
                        return false;
                    }
                    CharacterSelectionScript Gbl = GlobalPlyMgr;
                    var typ = typeof(CharacterSelectionScript);

                    FieldInfo type2 = typ.GetField("jumpToLast", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bool GblJumpToLast = (bool)type2.GetValue(Gbl);

                    FieldInfo type3 = typ.GetField("jumpToNext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bool GblJumpToNext = (bool)type3.GetValue(Gbl);

                    //I had some scene managment stuff here, too scared to get rid of it now since this all works lmao
                    if (GblJumpToNext && Gbl.blackScreen.color.a == 1f)
                    {
                        //mod.Logger.Log("Call A");
                        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
                        return false;
                    }
                    if (GblJumpToLast && Gbl.blackScreen.color.a == 1f)
                    {
                        //mod.Logger.Log("Call B");
                        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex - 1);
                    }

                    return false;

                }
                else
                {
                    return true;
                }
            }
        }
        //CharacterSelectionScript CHANGES END, IT'S ALL DOWNHILL FROM HERE

        //The meat of it all what actually handles the player
        [HarmonyPatch(typeof(PlayerSystem), "Start", new System.Type[] { })]
        static class PlayerSystem_Start_Patch
        {
            internal static void Postfix()
            {
                if (Enbvalue == 0)
                {
                    //preliminary unload to ensure we don't error from loading the icons
                    AssetBundle.UnloadAllAssetBundles(true);
                    var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(mod.Path + "/PlayerFolder", PlayerBundleName));

                    //Read custom data, start
                    getcustominfo = myLoadedAssetBundle.LoadAsset<TextAsset>(PlayerBundleName + "info") ?? null;
                    if (getcustominfo != null)
                    {
                        string[] linesFromfile = getcustominfo.text.Split('\n');
                        int counter = 0;
                        string sample = "";
                        foreach (string line in linesFromfile)
                        {
                            if (counter == 0) //Hover
                            {
                                sample = line.Replace("isHover = ", "");
                                sample = sample.Replace(";", "");
                                isHover = bool.Parse(sample);
                            }
                            if (counter == 1) //Speed
                            {
                                sample = line.Replace("PlayerSpeed = ", "");
                                sample = sample.Replace(";", "");
                                PlayerSpeed = float.Parse(sample);
                            }
                            if (counter == 2) //Inf Jump
                            {
                                sample = line.Replace("infJump = ", "");
                                sample = sample.Replace(";", "");
                                infJump = bool.Parse(sample);
                                //mod.Logger.Log(infJump.ToString());
                            }
                            counter++;
                        }
                    }
                    else
                    {
                        PlayerSpeed = 10;
                        isHover = false;
                        infJump = false;
                    }
                    //Read custom data, end

                    //Check for level and get costume name accordingly.
                    String LvlSuffix = "";
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Myuu"))
                    {
                        LvlSuffix = "Myuu";
                    }
                    else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Pool"))
                    {
                        LvlSuffix = "Pool";
                    }
                    else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Sky"))
                    {
                        LvlSuffix = "Sky";
                    }
                    else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Lava"))
                    {
                        LvlSuffix = "Lava";
                    }
                    else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Zwei"))
                    {
                        LvlSuffix = "Zwei";
                    }

                    _CustomModel = myLoadedAssetBundle.LoadAsset<GameObject>(PlayerBundleName + "char" + LvlSuffix) ?? myLoadedAssetBundle.LoadAsset<GameObject>(PlayerBundleName + "char");

                    //mod.Logger.Log(_CustomModel.ToString());

                    //foreach (string sNum in myLoadedAssetBundle.GetAllAssetNames())
                    //{
                    //    mod.Logger.Log(sNum);
                    //}


                    if (GlobalPly == null)
                    {
                        _GlobalPly = null;

                    }

                    PlayerSystem Gbl = GlobalPly;


                    //Animation anim = Gbl._playeranimation.GetComponent<Animation>();
                    //foreach (AnimationState state in anim)
                    //{
                    //    mod.Logger.Log(state.name);
                    //}

                    //Animation plyanim = _CustomModel.GetComponent<Animation>();
                    //foreach (AnimationState state in plyanim)
                    //{
                    //    mod.Logger.Log(state.name);
                    //}



                    var typ = typeof(PlayerSystem);


                    FieldInfo type3 = typ.GetField("bigspeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    //get our custom stuff and emulate base game functions
                    //Setup is as follows, Create our model
                    //Set it's location to Toree's
                    //Hide toree and his components
                    Gbl.characterID = GamePadScript.instance.GetInt("ChosenCharacter");
                    if (Gbl.characterID == 4)
                    {
                        _CustomModel = GameObject.Instantiate(_CustomModel, new Vector3(0, 0, 0), Quaternion.identity);
                        _CustomModel.transform.position = Gbl._playeranimation.gameObject.transform.position;

                        
                        _CustomModel.transform.parent = Gbl._playermodel.transform;

                        Animation CustPlyanim = _CustomModel.GetComponentInChildren<Animation>() ?? null;
                        if (CustPlyanim != null)
                        {
                            CustPlyanim["Idle"].speed = Gbl.speed / 32f;
                            CustPlyanim["Run"].speed = Gbl.speed / 12f;
                            CustPlyanim["JumpNEW"].speed = Gbl.speed / 12f;
                            CustPlyanim["FallNEW"].speed = Gbl.speed / 18f;
                            CustPlyanim.Play("Idle");
                        }

                        //UnityEngine.Object.Destroy(Gbl.FootAura);
                        var X = UnityEngine.Object.FindObjectsOfType<ShadowScript>(); ;
                        foreach (var Y in X)
                        {
                            //mod.Logger.Log(Y.name);
                            Y.ShadowObject.gameObject.SetActive(false);
                        }


                        Gbl._playeranimation.gameObject.SetActive(false);

                        
                        type3.SetValue(Gbl, PlayerSpeed);

                    }


                    return;

                }
                else
                {
                    return;
                }
            }
        }


        //Handles Player death anim
        [HarmonyPatch(typeof(PlayerSystem), "DeathAnimation", new System.Type[] { })]
        static class PlayerSystem_DeathAnimation_Patch
        {
            internal static void Postfix()
            {
                if (Enbvalue == 0)
                {

                    if (GlobalPly == null)
                    {
                        _GlobalPly = null;

                    }

                    Animation CustPlyanim = _CustomModel.GetComponentInChildren<Animation>() ?? null;

                    if (CustPlyanim != null)
                    {
                        CustPlyanim["PlayerDeath"].speed = 0.3f;
                        CustPlyanim.CrossFade("PlayerDeath", 0.15f);//needs test initially .15f
                    }

                    return;

                }
                else
                {
                    return;
                }
            }
        }


        [HarmonyPatch(typeof(PlayerSystem), "AnimationManager", new System.Type[] { })]
        static class PlayerSystem_AnimationManager_Patch
        {
            internal static void Prefix()
            {
                if (Enbvalue == 0)
                {

                    if (GlobalPly == null)
                    {
                        _GlobalPly = null;

                    }

                    PlayerSystem Gbl = GlobalPly;
                    Animation CustPlyanim = _CustomModel.GetComponentInChildren<Animation>() ?? null;
                    var typ = typeof(PlayerSystem);

                    FieldInfo type2 = typ.GetField("horizontal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    float Gblhorizontal = (float)type2.GetValue(Gbl);

                    FieldInfo type3 = typ.GetField("vertical", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    float Gblvertical = (float)type3.GetValue(Gbl);

                    FieldInfo type4 = typ.GetField("moveDirection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Vector3 GblmoveDirection = (Vector3)type4.GetValue(Gbl);


                    FieldInfo type6 = typ.GetField("controller", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    CharacterController Gblcontroller = (CharacterController)type6.GetValue(Gbl);


                    FieldInfo type8 = typ.GetField("lastFrameGrounded_Animation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bool GbllastFrameGrounded_Animation = (bool)type8.GetValue(Gbl);


                    //Some copy of ingame stuff
                    float num = Mathf.Abs(Gblhorizontal);
                    float num2 = Mathf.Abs(Gblvertical);
                    if (num > num2)
                    {
                    }
                    if (GameManager.singleton.PlayOnMobile)
                    {
                    }
                    float num3 = Mathf.Abs(GblmoveDirection.magnitude / 10f);
                    float num4 = Gbl.speed;
                    if (Gbl.inWater && Gbl.LastPos != Gbl.transform.position)
                    {
                        Gbl.WaterEffect.enableEmission = true;
                        Gbl._waterWalkSound.mute = false;
                    }
                    else
                    {
                        Gbl.WaterEffect.enableEmission = false;
                        Gbl._waterWalkSound.mute = true;
                    }
                    //Handles if the player hovers or is a regular walk
                    int num5;
                    if (Gblcontroller.isGrounded)
                    {
                        if (Gbl.LastPos == Gbl.transform.position)
                        {
                            num5 = 1;
                            Gbl.FootStepSmoke.enableEmission = false;
                            Gbl.FootStepSmokeExtra.enableEmission = false;
                            Gbl.FootAura.SetActive(false);
                        }
                        else
                        {
                            num5 = 2;
                            Gbl.FootStepSmoke.enableEmission = true;
                            if (GamePadScript.instance.runButtonPressed() || GamePadScript.instance.sprintButtonPressed())
                            {
                                Gbl.FootStepSmokeExtra.enableEmission = true;
                                if (Gbl.characterID < 3)
                                {
                                    Gbl.FootAura.SetActive(true);
                                }
                                else if (Gbl.characterID == 4 && !isHover)
                                {
                                    Gbl.FootAura.SetActive(true);
                                }
                            }
                            else
                            {
                                Gbl.FootStepSmokeExtra.enableEmission = false;
                                if (Gbl.characterID < 3)
                                {
                                    Gbl.FootAura.SetActive(false);
                                }
                                else if (Gbl.characterID == 4 && !isHover)
                                {
                                    Gbl.FootAura.SetActive(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        Gbl.FootStepSmoke.enableEmission = false;
                        Gbl.FootStepSmokeExtra.enableEmission = false;
                        Gbl.FootAura.SetActive(false);
                        if (Gbl.LastPos.y < Gbl.gameObject.transform.position.y)
                        {
                            num5 = 3;
                        }
                        else
                        {
                            num5 = 4;
                        }
                    }

                    //Start handling some of our custom animations
                    if (num5 != 2)
                    {
                        Gbl.FootSounds.SetActive(false);
                    }
                    if (num5 == 1)
                    {
                        if (CustPlyanim != null)
                        {
                            CustPlyanim.CrossFade("Idle", 0.1f);
                            if (!GbllastFrameGrounded_Animation)
                            {
                                CustPlyanim.Play("Idle");
                            }
                        }
                    }
                    else if (num5 == 2)
                    {
                        if (GblmoveDirection == Vector3.zero)
                        {
                            Gbl.FootStepSmoke.enableEmission = false;
                            Gbl.FootSounds.SetActive(false);
                        }
                        else
                        {
                            if (CustPlyanim != null)
                            {
                                CustPlyanim["Run"].speed = num4 * num3 / 10f;
                                CustPlyanim.CrossFade("Run", 0.15f);
                                if (Gbl.characterID < 3)
                                {
                                    Gbl.FootSounds.SetActive(true);
                                }
                                if (Gbl.characterID == 4 && !isHover)
                                {
                                    Gbl.FootSounds.SetActive(true);
                                }
                                if (!GbllastFrameGrounded_Animation)
                                {
                                    CustPlyanim.Play("Run");
                                }
                            }
                        }
                    }
                    else if (num5 == 3)
                    {
                        if (CustPlyanim != null)
                        {
                            CustPlyanim["JumpNEW"].speed = num4 / 20f;
                            CustPlyanim.CrossFade("JumpNEW", 0.25f);
                        }
                    }
                    else if (num5 == 4)
                    {
                        if (CustPlyanim != null)
                        {
                            CustPlyanim["FallNEW"].speed = num4 / 20f;
                            CustPlyanim.CrossFade("FallNEW", 0.25f);
                        }
                    }
                    else if (num5 == 5)
                    {
                        if (Gbl.characterID != 4)
                        {
                            CustPlyanim["Slide"].speed = num4 / 20f;
                            CustPlyanim.CrossFade("Slide", 0.05f);
                        }
                    }
                    else if (num5 == 6)
                    {
                        if (Gbl.characterID != 4)
                        {
                            CustPlyanim["Attack"].speed = num4 / 11f;
                            CustPlyanim.CrossFade("Attack", 0.25f);
                        }
                    }
                    else if (num5 == 7)
                    {
                        if (Gbl.characterID != 4)
                        {
                            CustPlyanim["Fly"].speed = num4 / 20f;
                            CustPlyanim.CrossFade("Fly", 0.1f);
                        }
                    }
                    type8.SetValue(Gbl, Gblcontroller.isGrounded);

                    return;

                }
                else
                {
                    return;
                }
            }
        }

        //Setup infinite jumping capabilities
        [HarmonyPatch(typeof(PlayerSystem), "WalkAround", new System.Type[] { })]
        static class PlayerSystem_WalkAround_Patch
        {
            internal static void Postfix()
            {
                if (Enbvalue == 0)
                {

                    if (GlobalPly == null)
                    {
                        _GlobalPly = null;

                    }

                    PlayerSystem Gbl = GlobalPly;
                    var typ = typeof(PlayerSystem);

                    FieldInfo type2 = typ.GetField("doublejump", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    FieldInfo type3 = typ.GetField("donejumps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    FieldInfo type4 = typ.GetField("noGravityControl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    //mod.Logger.Log(infJump.ToString());
                    if (infJump)
                    {
                        type2.SetValue(Gbl, true);
                        type3.SetValue(Gbl, 0);
                        type4.SetValue(Gbl, true);
                    }

                }

            }
        }

        //Handles the custom end of level animation
        [HarmonyPatch(typeof(PlayerSystem), "EndOfLevel", new System.Type[] { })]
        static class PlayerSystem_EndOfLevel_Patch
        {
            internal static void Prefix()
            {
                if (Enbvalue == 0)
                {
                    PlayerSystem Gbl = GlobalPly;
                    Animation CustPlyanim = _CustomModel.GetComponentInChildren<Animation>() ?? null;

                    Gbl.myScaleManager.newScale = new Vector3(1.5f, 0.25f, 1.5f);
                    Gbl.StarEffect.Play();
                    Gbl._squishSound.Play();
                    Vector3 target = Gbl.Camera.position - Gbl.transform.position;
                    target.y = 0f;
                    Vector3 forward = Vector3.RotateTowards(Gbl._playermodel.forward, target, 90000f, 100f);
                    Gbl._playermodel.rotation = Quaternion.LookRotation(forward);
                    if (Gbl.characterID == 0)
                    {
                        Gbl._myVoiceManager.GetALine(2);
                    }
                    if (Gbl.characterID == 4 && CustPlyanim != null)
                    {
                        CustPlyanim["JumpNEW"].speed = 0.3f;
                        CustPlyanim.CrossFade("JumpNEW", 0.25f);
                    }
                    Gbl._playeranimation["JumpNEW"].speed = 0.3f;
                    Gbl._playeranimation.CrossFade("JumpNEW", 0.25f);
                    Gbl.FootStepSmoke.enableEmission = false;
                    Gbl.FootStepSmokeExtra.enableEmission = false;
                    Gbl.FootAura.SetActive(false);
                    Gbl.FootSounds.SetActive(false);
                }
            }
        }




    }
}
