using System;
using UnityEngine;
using UnityEditor;

public static class AquaChecks
{
    static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
    [MenuItem("AquaGuardian/Run simulation checks")]
    public static void Run()
    {
        foreach (AquaSimulation.Scenario scenario in Enum.GetValues(typeof(AquaSimulation.Scenario)))
        {
            var sim = new AquaSimulation(scenario);
            Assert(sim.water >= 0 && sim.water <= 100, "Invalid initial water");
            string message;
            while (!sim.Finished)
                Assert(sim.Apply(AquaSimulation.Action.Extract, out message), "Action unexpectedly rejected");
            Assert(sim.turn <= AquaSimulation.Turns && (sim.turn == AquaSimulation.Turns || sim.water == 0), "Incorrect end condition");
            int score = sim.score;
            Assert(!sim.Apply(AquaSimulation.Action.Extract, out message) && sim.score == score, "Finished game changed");
        }
        var poor = new AquaSimulation(AquaSimulation.Scenario.Normal); poor.money = 0;
        string ignored; Assert(!poor.Apply(AquaSimulation.Action.Recharge, out ignored) && poor.turn == 0, "Unaffordable action consumed a turn");
        var polluted = new AquaSimulation(AquaSimulation.Scenario.Contamination);
        Assert(polluted.Apply(AquaSimulation.Action.Treat, out ignored) && polluted.pollution == 35, "Treatment did not reduce pollution");
        int saved = polluted.saved;
        Assert(polluted.Apply(AquaSimulation.Action.Diversify, out ignored) && polluted.saved > saved, "Crop diversification did not save water");
        Debug.Log("AquaGuardian simulation checks passed");
    }
}
