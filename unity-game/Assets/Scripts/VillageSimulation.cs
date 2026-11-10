using System;

// Deterministic educational simulation. All quantities are game units.
[Serializable]
public sealed class VillageSimulation
{
    public enum Weather { Drought, Rain, Storm }
    public enum Irrigation { None, Drip, Flood, GroundwaterPump }
    public enum Supply { Ration, Pond, Well }
    public enum Action { StoreRain, ReleaseRain, Recharge, Drip, Flood, PumpFarm, PlantMillet, Harvest, SupplyPond, SupplyWell, Ration, RepairLeak, CleanPollution, IgnorePollution, PlantTrees }
    public float time, pond = 8, aquifer = 43, health = 75, growth = 10, moisture = 12, crop = 8, quality = 100;
    public int coins = 45, score, harvests;
    public bool gate, recharge, millet, leakFixed, pollution, pollutionHandled, trees, capturedRain, finished, won;
    public Irrigation irrigation;
    public Supply supply = Supply.Ration;
    public string lastMessage = "The village is dry. Clouds are coming. Find the pond and prepare to capture rain.";
    float captured, thirst;
    public const float Duration = 225;
    public Weather CurrentWeather { get { return time >= 125 && time < 165 ? Weather.Storm : time >= 12 && time < 58 ? Weather.Rain : Weather.Drought; } }
    public int Day { get { return time < 58 ? 1 : time < 125 ? 2 : time < 165 ? 3 : 4; } }
    public float RainIn { get { return CurrentWeather == Weather.Storm ? 1.65f : CurrentWeather == Weather.Rain ? 1.25f : 0; } }
    public bool HasFarm { get { return irrigation != Irrigation.None; } }
    public bool Ready { get { return crop >= 99.9f; } }
    public string Objective {
        get {
            if (!capturedRain) return "Prepare the POND inlet, then catch the rain.";
            if (!HasFarm) return "Visit the FARM and set up irrigation.";
            if (!millet) return "Visit the SEED SHED to try a water-saving crop.";
            if (!leakFixed) return "Repair the leaking pipe at the VILLAGE WELL.";
            if (pollution && !pollutionHandled) return "Protect the POND: arrange a safe cleanup.";
            if (Ready) return "Your crops are ready. Return to the FARM to harvest.";
            if (harvests == 0) return "Keep crops watered until the first harvest.";
            if (!recharge) return "Build a recharge bed beside the POND before the next storm.";
            return "Keep water and people healthy through the final dry spell.";
        }
    }
    public bool Act(Action action) {
        if (finished) return false;
        switch(action) {
            case Action.StoreRain:
                if(gate) return Say("The inlet is already open. Rain is being collected.",false);
                gate=true; return Say("Inlet opened! Rain will fill the pond. Watch the blue water rise.");
            case Action.ReleaseRain:
                gate=false; return Say("Inlet closed. Incoming rain will run past the pond; stored water remains available.");
            case Action.Recharge:
                if(recharge) return Say("Your recharge bed is already working.",false);
                if(coins<18) return Say("You need 18 coins. Harvest crops to earn more.",false);
                coins-=18; recharge=true; score+=70; return Say("Recharge bed built. Clean rain now helps replenish the aquifer.");
            case Action.Drip:
                if(irrigation==Irrigation.Drip) return Say("Drip irrigation is already running.",false);
                if(pond<4) return Say("The pond needs at least 4 water units. Catch rain first, or use the pump carefully.",false);
                if(coins<12) return Say("Drip lines cost 12 coins. Harvest a crop first.",false);
                coins-=12; irrigation=Irrigation.Drip; score+=35; return Say("Drip lines installed. Water goes to roots with little waste.");
            case Action.Flood:
                if(pond<4) return Say("There is not enough pond water to flood the field.",false);
                irrigation=Irrigation.Flood; return Say("Flood irrigation started. Crops receive water, but the pond drains quickly.");
            case Action.PumpFarm:
                if(aquifer<5) return Say("The aquifer is too low to pump. Try rain storage and recharge.",false);
                irrigation=Irrigation.GroundwaterPump; return Say("The farm pump starts. It grows food now, but draws down shared groundwater.");
            case Action.PlantMillet:
                if(millet) return Say("Millet is already planted.",false);
                millet=true; score+=60; return Say("Millet planted. This crop needs less water during the dry spell.");
            case Action.Harvest:
                if(!Ready) return Say("The crop is still growing. Keep the soil moist and protect water quality.",false);
                crop=8; harvests++; coins+=28; growth=Clamp(growth+17); health=Clamp(health+9); score+=150;
                return Say("Harvest delivered! +28 coins. The village gains food, healthy homes and new families.");
            case Action.SupplyPond:
                if(pond<3) return Say("The pond is too low. The village needs another safe supply.",false);
                supply=Supply.Pond; return Say("Homes now share stored rainwater. This reduces pressure on groundwater.");
            case Action.SupplyWell:
                supply=Supply.Well; return Say("Homes are using the well. Watch the aquifer and avoid overpumping.");
            case Action.Ration:
                supply=Supply.Ration; return Say("The village shares a smaller emergency well supply. Growth slows, but water lasts longer.");
            case Action.RepairLeak:
                if(leakFixed) return Say("The pipe is already repaired.",false);
                if(coins<8) return Say("Repairing the pipe needs 8 coins.",false);
                coins-=8; leakFixed=true; score+=70; return Say("Leak fixed! Every household now needs less water.");
            case Action.CleanPollution:
                if(!pollution || pollutionHandled) return Say("No pollution incident needs attention right now.",false);
                pollutionHandled=true; quality=Clamp(quality+35); score+=100; return Say("Trained adults contain the leak and organise cleanup. The water source is protected.");
            case Action.IgnorePollution:
                return Say("The spill is still there. Water quality will keep falling until adults contain it.");
            case Action.PlantTrees:
                if(trees) return Say("The new trees are already planted.",false);
                if(coins<10 || pond<6) return Say("Saplings need 10 coins and 6 pond-water units.",false);
                coins-=10; pond-=6; trees=true; growth=Clamp(growth+6); score+=60; return Say("Saplings planted! A green belt grows around the village.");
        }
        return false;
    }
    bool Say(string value,bool success=true) { lastMessage=value; return success; }
    public void Tick(float dt) {
        if(finished || dt<=0) return;
        // Bounded integration also makes tests independent of frame rate.
        while(dt>0 && !finished) { float step=Math.Min(dt,.1f); Step(step); dt-=step; }
    }
    void Step(float dt) {
        time+=dt;
        float rain=RainIn;
        if(time>=90 && !pollution) { pollution=true; lastMessage="A storm drain has carried a leaking container near the pond. Find the safety station!"; }
        if(pollution && !pollutionHandled) quality-=.40f*dt;
        else if(pollutionHandled) quality+=.12f*dt;
        if(gate && rain>0) { float added=Math.Min(100-pond,rain*dt); pond+=added; captured+=added; if(!capturedRain && captured>=15) { capturedRain=true; score+=120; lastMessage="Rain rescued! +120 points. The pond is filling. Use the stored water to help the village."; } }
        if(recharge && rain>0 && quality>=55) aquifer+=rain*.36f*dt;
        else aquifer+=rain*.035f*dt;
        pond-=.035f*dt;
        moisture+=rain*1.1f*dt-(millet?.30f:.66f)*dt;
        float draw=0;
        if(irrigation==Irrigation.Drip && pond>0) { draw=Math.Min(pond,.17f*dt); pond-=draw; moisture+=draw*4.6f; }
        if(irrigation==Irrigation.Flood && pond>0) { draw=Math.Min(pond,.64f*dt); pond-=draw; moisture+=draw*2.1f; }
        if(irrigation==Irrigation.GroundwaterPump && aquifer>0) { draw=Math.Min(aquifer,.39f*dt); aquifer-=draw; moisture+=draw*2.8f; }
        float demand=(leakFixed?.085f:.15f)*(1+growth/180f)*dt;
        bool supplied;
        if(supply==Supply.Pond) { supplied=pond>=demand && quality>45; pond-=Math.Min(pond,demand); }
        else { float wellDemand=supply==Supply.Ration?demand*.58f:demand; supplied=aquifer>=wellDemand; aquifer-=Math.Min(aquifer,wellDemand); }
        if(supplied) { thirst=Math.Max(0,thirst-dt); health+=.035f*dt; if(supply!=Supply.Ration) growth+=.08f*dt; }
        else { thirst+=dt; health-=.8f*dt; growth-=.15f*dt; }
        if(quality<40) health-=.24f*dt;
        if(moisture>22 && quality>40) crop+=(millet?.85f:.63f)*dt;
        else crop-=.20f*dt;
        pond=Clamp(pond); aquifer=Clamp(aquifer); moisture=Clamp(moisture); crop=Clamp(crop); quality=Clamp(quality); health=Clamp(health); growth=Clamp(growth);
        if(health<=0 || thirst>45) { finished=true; won=false; lastMessage="The village ran out of safe water. Replay and prepare storage, irrigation and recharge earlier."; }
        else if(time>=Duration) { finished=true; won=health>=45 && aquifer>=18 && growth>=35 && harvests>=1 && quality>=50;
            if(won) score+=(int)(health+aquifer+growth)*2;
            lastMessage=won?"You carried the village through two rains and a dry season. Your choices built a water-smart community!":"The village survived, but needs a stronger water plan. Harvest food, protect quality and leave groundwater for the next season.";
        }
    }
    static float Clamp(float value) { return Math.Max(0,Math.Min(100,value)); }
}
