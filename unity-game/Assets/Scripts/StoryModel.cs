using System;
using System.Collections.Generic;

// Values represent simplified game units, not measurements or scientific forecasts.
[Serializable]
public class StoryChoice {
    public string text, outcome; public int water, trust, coins;
    public StoryChoice(string text, string outcome, int water, int trust, int coins) {
        this.text=text; this.outcome=outcome; this.water=water; this.trust=trust; this.coins=coins;
    }
}
[Serializable]
public class StoryChapter {
    public string location, title, speaker, dialogue, lesson;
    public StoryChoice[] choices;
    public StoryChapter(string location, string title, string speaker, string dialogue, string lesson, params StoryChoice[] choices) {
        this.location=location; this.title=title; this.speaker=speaker; this.dialogue=dialogue; this.lesson=lesson; this.choices=choices;
    }
}
[Serializable]
public class StoryState {
    public int chapter, water=50, trust=50, coins=30, score; public bool finished; public List<int> choices = new List<int>();
    public void Choose(int index) {
        if(finished || chapter<0 || chapter>=StoryContent.Chapters.Length) throw new InvalidOperationException("No active chapter");
        StoryChoice[] options=StoryContent.Chapters[chapter].choices;
        if(index<0 || index>=options.Length) throw new ArgumentOutOfRangeException("index");
        var c=options[index]; water=Clamp(water+c.water); trust=Clamp(trust+c.trust); coins=Clamp(coins+c.coins);
        score+=Math.Max(0,50+c.water*3+c.trust*2+c.coins); choices.Add(index);
    }
    public void Next() { chapter++; if(chapter>=StoryContent.Chapters.Length) finished=true; }
    public static int Clamp(int n) { return Math.Max(0,Math.Min(100,n)); }
    public string Ending {
        get {
            if(water>=65 && trust>=60) return "A village that planned together";
            if(water>=40 && trust>=35) return "A village learning to adapt";
            return "A difficult season, a new beginning";
        }
    }
}
public static class StoryContent {
    public static readonly StoryChapter[] Chapters={
        new StoryChapter("THE WELL", "The well is falling", "DADI MEERA", "The village well is lower than I have ever seen. Families and farms both need water. Aarav, the council is listening. What should we do first?", "During drought, managing demand gives groundwater time to recover.",
            new StoryChoice("Ask families to save water together", "Neighbours repair leaks and agree on fair water use. The well recovers a little and people trust your plan.", 10,12,4),
            new StoryChoice("Pump more water for everyone", "The buckets fill today, but extra pumping pulls the well down faster. The village will need another plan.", -14,-3,6),
            new StoryChoice("Close the well without a discussion", "Pumping slows, but families feel excluded and must walk farther for water.", 6,-13,0)),
        new StoryChapter("THE FARM", "A farmer's choice", "FARMER KAVYA", "My crops are thirsty, but they feed our families. I can change how I farm. Which plan should I try?", "Drip irrigation delivers water near roots; millet can need less water than water-intensive crops.",
            new StoryChoice("Drip irrigation and millet", "Kavya installs drip lines and grows millet on part of the farm. Water reaches the roots with less waste.", 15,10,-8),
            new StoryChoice("Flood the whole field", "The field is watered quickly, but much of the water evaporates or runs away.", -12,-3,5),
            new StoryChoice("Stop farming immediately", "Water use falls, yet families worry about food and the farmer's livelihood.", 7,-12,-4)),
        new StoryChapter("THE RAIN", "The unexpected storm", "ENGINEER RAVI", "Dark clouds gather above the village. Heavy rain could rush away in minutes. Where should we invest our effort?", "Recharge structures can help suitable clean rainwater soak into the ground; they need proper design and maintenance.",
            new StoryChoice("Build and maintain a recharge pit", "The village clears a safe inlet and builds a recharge pit. Some clean rainwater can soak into the ground.", 16,11,-9),
            new StoryChoice("Pave the open ground", "The new paving is convenient, but less rain can soak into the soil and more runs away.", -9,-2,4),
            new StoryChoice("Let the storm drain away", "The path stays clear, but the village misses an opportunity to collect rainwater.", -3,-2,3)),
        new StoryChapter("THE POND", "A warning in the water", "STUDENT TARA", "I found rubbish and a leaking container near the pond. My friends want to help, but we should keep everyone safe. What do you suggest?", "Preventing pollution protects water sources; unknown chemicals should be handled by trained adults.",
            new StoryChoice("Tell adults, isolate it and clean safely", "Adults report the leak, keep children away, and organise a safe cleanup. The village protects its water source.", 11,14,-4),
            new StoryChoice("Push the container into the pond", "The danger moves out of sight, but pollution can spread into the water and soil.", -17,-12,2),
            new StoryChoice("Ignore it until next season", "The leak remains near the pond. Neighbours worry about what might enter their water.", -9,-8,0)),
        new StoryChapter("THE COUNCIL", "The village decides", "DADI MEERA", "The season is ending. Our choices affected the well, the farm and our neighbours. What promise should guide the next season?", "Groundwater protection works best when a community monitors its sources and shares responsibility.",
            new StoryChoice("Monitor the well and share a water plan", "The council agrees to track the well and meet with farmers and families before the next dry season.", 8,12,-3),
            new StoryChoice("Let each household pump without limits", "People have more freedom today, but the shared groundwater has no protection from overuse.", -10,-8,4),
            new StoryChoice("Spend everything on a festival", "The celebration is joyful, but little is left to maintain the village water plan.", -3,2,-12))
    };
}
