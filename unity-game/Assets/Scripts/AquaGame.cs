using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

// Self-contained educational model. Values are gameplay units, not hydrological measurements.
public sealed class AquaSimulation
{
    public enum Scenario { Normal, Drought, HeavyRain, Contamination }
    public enum Action { Harvest, Recharge, Irrigation, Extract, Diversify, Treat }
    public const int Turns = 6;
    public Scenario scenario;
    public int water, money, score, turn, saved, initialWater, pollution;
    public bool Finished => turn >= Turns || water <= 0;
    public AquaSimulation(Scenario selected)
    {
        scenario = selected;
        water = selected == Scenario.Drought ? 45 : selected == Scenario.HeavyRain ? 75 : 60;
        initialWater = water; money = 100; pollution = selected == Scenario.Contamination ? 60 : 10;
    }
    public bool Apply(Action action, out string message)
    {
        message = "";
        if (Finished) { message = "The challenge is complete."; return false; }
        int cost = action == Action.Harvest ? 20 : action == Action.Recharge ? 30 : action == Action.Irrigation ? 25 : action == Action.Diversify ? 20 : action == Action.Treat ? 25 : 0;
        if (money < cost) { message = "Not enough money for this action."; return false; }
        money -= cost;
        int rain = scenario == Scenario.Drought ? 1 : scenario == Scenario.HeavyRain ? 7 : 4;
        int demand = 8;
        switch (action)
        {
            case Action.Harvest: water += 8; saved += 8; score += 80; message = "Rainwater captured and stored."; break;
            case Action.Recharge: water += 14; saved += 14; score += 100; message = "Recharge pit replenished the aquifer."; break;
            case Action.Irrigation: demand = 3; saved += 5; score += 90; message = "Efficient irrigation reduced demand."; break;
            case Action.Extract: water -= 10; money += 20; score = Math.Max(0, score - 25); message = "Extra pumping earned coins but depleted water."; break;
            case Action.Diversify: demand = 4; saved += 4; score += 85; message = "Growing less thirsty crops saves water!"; break;
            case Action.Treat: pollution = Math.Max(0, pollution - 25); score += 75; message = "Cleaning pollution protects the aquifer!"; break;
        }
        water = Math.Max(0, Math.Min(100, water + rain - demand));
        turn++;
        if (Finished) score += water * 5;
        return true;
    }
    public string Rating => pollution >= 40 ? "At risk" : water >= 70 ? "Excellent" : water >= 45 ? "Good" : water >= 25 ? "At risk" : "Critical";
}

public class AquaGame : MonoBehaviour
{
    enum Screen { Menu, Story, Playing, Result, Learning }
    Screen screen = Screen.Menu;
    AquaSimulation game;
    string message = "Choose a scenario to begin.";
    string learning = "";
    GUIStyle title, text, button, box;
    bool initialized;
    Vector2 scroll;
    AquaSimulation.Scenario selectedScenario;
    CharacterController player;
    [Serializable] public class ResultData { public string scenario; public int score, groundwaterBefore, groundwaterAfter, waterSaved; public string sustainabilityRating; }
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void AquaGuardianResult(string json);
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "Village" || FindFirstObjectByType<AquaGame>() != null) return;
        new GameObject("AquaGuardianBridge").AddComponent<AquaGame>();
    }
    void Start()
    {
        // The original prototype HUD and interactions remain in the scene for editing.
        var canvas = GameObject.Find("Canvas"); if (canvas) canvas.SetActive(false);
        foreach (var old in FindObjectsByType<WaterSourceInteraction>(FindObjectsSortMode.None)) old.enabled = false;
        foreach (var elder in FindObjectsByType<ElderDialogue>(FindObjectsSortMode.None)) elder.enabled = false;
        var oldSystem = FindFirstObjectByType<GroundwaterSystem>(); if (oldSystem) oldSystem.enabled = false;
        var playerObject = GameObject.Find("Player"); if (playerObject) player = playerObject.GetComponent<CharacterController>();
        DecorateVillage();
    }
    static void DecorateVillage()
    {
        Colorize("VillageGround", new Color(.35f, .57f, .27f));
        Colorize("House_Body", new Color(.85f, .69f, .46f));
        Colorize("House_Roof", new Color(.55f, .24f, .16f));
        Colorize("WaterWell", new Color(.48f, .52f, .55f));
        Colorize("VillagePond", new Color(.12f, .55f, .73f));
        Colorize("WaterTank", new Color(.25f, .49f, .68f));
        Colorize("Player", new Color(.18f, .43f, .76f));
        Colorize("VillageElder", new Color(.85f, .57f, .27f));
        // Primitive trees and fields keep the environment editable without external assets.
        for (int i = 0; i < 8; i++)
        {
            float x = -14 + (i % 4) * 8, z = 12 + (i / 4) * 5;
            Primitive("Tree trunk", PrimitiveType.Cylinder, new Vector3(x, 1, z), new Vector3(.5f, 1, .5f), new Color(.38f, .22f, .12f));
            Primitive("Tree canopy", PrimitiveType.Sphere, new Vector3(x, 3, z), new Vector3(3, 3, 3), new Color(.13f, .42f, .17f));
        }
        for (int i = 0; i < 4; i++) Primitive("Farm row", PrimitiveType.Cube, new Vector3(-12 + i * 2, .08f, -10), new Vector3(1.2f, .12f, 7), new Color(.39f, .25f, .13f));
    }
    static void Colorize(string objectName, Color color)
    {
        var go = GameObject.Find(objectName); if (!go) return;
        var renderer = go.GetComponent<Renderer>(); if (renderer) renderer.material.color = color;
    }
    static void Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.position = position; go.transform.localScale = scale;
        var renderer = go.GetComponent<Renderer>(); renderer.material.color = color;
    }
    void Styles()
    {
        if (initialized) return;
        title = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        text = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, normal = { textColor = Color.white } };
        button = new GUIStyle(GUI.skin.button) { fontSize = 16, wordWrap = true, fixedHeight = 42 };
        box = new GUIStyle(GUI.skin.box);
        var backdrop = new Texture2D(1, 1); backdrop.SetPixel(0, 0, new Color(.035f, .10f, .16f, .95f)); backdrop.Apply();
        box.normal.background = backdrop; initialized = true;
    }
    void Begin(AquaSimulation.Scenario s) { selectedScenario = s; screen = Screen.Story; scroll = Vector2.zero; }
    void StartChallenge() { game = new AquaSimulation(selectedScenario); message = "Help the village make six wise choices!"; screen = Screen.Playing; scroll = Vector2.zero; }
    void Move(Vector3 direction) { if (player) player.Move(direction * 4f * Time.deltaTime); }
    void Choose(AquaSimulation.Action a)
    {
        if (!game.Apply(a, out message)) return;
        if (game.Finished) { screen = Screen.Result; Publish(); }
    }
    void Publish()
    {
        var result = new ResultData { scenario = game.scenario.ToString(), score = game.score, groundwaterBefore = game.initialWater, groundwaterAfter = game.water, waterSaved = game.saved, sustainabilityRating = game.Rating };
        string json = JsonUtility.ToJson(result);
        Debug.Log("AquaGuardian result: " + json);
#if UNITY_WEBGL && !UNITY_EDITOR
        AquaGuardianResult(json);
#endif
    }
    void OnGUI()
    {
        Styles();
        float scale = Mathf.Clamp(Mathf.Min(ScreenWidth() / 1100f, ScreenHeight() / 700f), 0.65f, 1.4f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));
        float w = Mathf.Min(490, ScreenWidth() / scale - 32), h = ScreenHeight() / scale - 32;
        GUILayout.BeginArea(new Rect(16, 16, w, h), box);
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.Label("AQUAGUARDIAN", title);
        if (screen == Screen.Menu)
        {
            GUILayout.Label("Protect the village's groundwater. Choose a rainfall scenario, then balance water, money and conservation over six turns.", text);
            if (GUILayout.Button("Normal rainfall", button)) Begin(AquaSimulation.Scenario.Normal);
            if (GUILayout.Button("Drought", button)) Begin(AquaSimulation.Scenario.Drought);
            if (GUILayout.Button("Heavy rainfall", button)) Begin(AquaSimulation.Scenario.HeavyRain);
            if (GUILayout.Button("Pollution alert", button)) Begin(AquaSimulation.Scenario.Contamination);
            if (GUILayout.Button("Explore Q-learning", button)) { Train(); screen = Screen.Learning; }
            GUILayout.Label("Move with WASD or arrow keys. Values are educational game units.", text);
        }
        else if (screen == Screen.Story)
        {
            GUILayout.Label("A MESSAGE FROM THE VILLAGE ELDER", text);
            GUILayout.Label("Welcome, young water guardian! Our village needs your help. Wells and farms depend on groundwater hidden beneath our feet. Your choices can protect it for everyone.", text);
            GUILayout.Label("In " + selectedScenario + ", collect rain, recharge the ground, save water on farms and keep it clean. Every choice has a cost and a consequence!", text);
            if (GUILayout.Button("Let's help the village!", button)) StartChallenge();
            if (GUILayout.Button("Choose another mission", button)) screen = Screen.Menu;
        }
        else if (screen == Screen.Playing)
        {
            GUILayout.Label(game.scenario + "   |   Turn " + (game.turn + 1) + " / " + AquaSimulation.Turns, text);
            GUILayout.Label("Groundwater: " + game.water + "%    Money: " + game.money + "    Score: " + game.score, text);
            GUILayout.Label("Pollution: " + game.pollution + "%", text);
            GUILayout.Label(message, text);
            if (GUILayout.Button("Harvest rainwater  •  cost 20", button)) Choose(AquaSimulation.Action.Harvest);
            if (GUILayout.Button("Build recharge pit  •  cost 30", button)) Choose(AquaSimulation.Action.Recharge);
            if (GUILayout.Button("Efficient irrigation  •  cost 25", button)) Choose(AquaSimulation.Action.Irrigation);
            if (GUILayout.Button("Grow less thirsty crops  •  cost 20", button)) Choose(AquaSimulation.Action.Diversify);
            if (GUILayout.Button("Clean up pollution  •  cost 25", button)) Choose(AquaSimulation.Action.Treat);
            if (GUILayout.Button("Extra pumping  •  earn 20", button)) Choose(AquaSimulation.Action.Extract);
            GUILayout.Label("Explore: WASD / arrows or hold the buttons", text);
            GUILayout.BeginHorizontal();
            if (GUILayout.RepeatButton("←", button)) Move(Vector3.left);
            if (GUILayout.RepeatButton("↑", button)) Move(Vector3.forward);
            if (GUILayout.RepeatButton("↓", button)) Move(Vector3.back);
            if (GUILayout.RepeatButton("→", button)) Move(Vector3.right);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Return to menu", button)) screen = Screen.Menu;
        }
        else if (screen == Screen.Result)
        {
            GUILayout.Label("CHALLENGE COMPLETE", title);
            GUILayout.Label("Rating: " + game.Rating + "\nScore: " + game.score + "\nGroundwater: " + game.initialWater + "% → " + game.water + "%\nWater conserved: " + game.saved + " game units", text);
            GUILayout.Label("In WebGL, the host page can receive this result. Saving requires the host application to handle it.", text);
            if (GUILayout.Button("Play again", button)) screen = Screen.Menu;
        }
        else
        {
            GUILayout.Label("Q-LEARNING EXPERIMENT", title);
            GUILayout.Label(learning, text);
            if (GUILayout.Button("Train again", button)) Train();
            if (GUILayout.Button("Back", button)) screen = Screen.Menu;
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    static int ScreenWidth() => UnityEngine.Screen.width;
    static int ScreenHeight() => UnityEngine.Screen.height;
    void Train()
    {
        // Tabular Q-learning over discretized water and remaining budget. The model is
        // trained on the same six-turn simulation as the human player.
        var random = new System.Random(42); var q = new float[3, 4, 4];
        const float alpha = .15f, gamma = .9f;
        for (int episode = 0; episode < 3000; episode++)
        {
            var sim = new AquaSimulation((AquaSimulation.Scenario)(episode % 3));
            while (!sim.Finished)
            {
                int waterState = Math.Min(2, sim.water / 34), budget = Math.Min(3, sim.money / 25);
                int action = random.NextDouble() < .2 ? random.Next(4) : Best(q, waterState, budget);
                int before = sim.water; string ignored;
                if (!sim.Apply((AquaSimulation.Action)action, out ignored)) { q[waterState, budget, action] -= 1; continue; }
                float reward = (sim.water - before) + (sim.water >= 45 ? 3 : -5);
                int nextW = Math.Min(2, sim.water / 34), nextB = Math.Min(3, sim.money / 25);
                float future = sim.Finished ? 0 : q[nextW, nextB, Best(q, nextW, nextB)];
                q[waterState, budget, action] += alpha * (reward + gamma * future - q[waterState, budget, action]);
            }
        }
        string[] names = { "harvest rainwater", "build recharge pit", "use efficient irrigation", "extra pumping" };
        learning = "Trained 3,000 simulated episodes using Q(s,a) ← Q(s,a) + α[r + γ max Q(s',a') − Q(s,a)].\n\nSuggested first action with full budget:\nLow water: " + names[Best(q, 0, 3)] + "\nMedium water: " + names[Best(q, 1, 3)] + "\nHigh water: " + names[Best(q, 2, 3)] + "\n\nThis is a small tabular experiment, not PPO or a scientific groundwater model.";
    }
    static int Best(float[,,] q, int w, int b) { int best = 0; for (int i = 1; i < 4; i++) if (q[w,b,i] > q[w,b,best]) best = i; return best; }
}
