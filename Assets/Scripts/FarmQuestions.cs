using System.Collections.Generic;
using UnityEngine;

// Educational examples: disease identification and treatment vary by crop and location.
public sealed class FarmQuestion {
    public readonly string prompt; public readonly string[] options;
    public FarmQuestion(string p,string correct,string b,string c) { prompt=p; options=new[]{correct,b,c}; }
}
public static class FarmQuestions {
    static readonly FarmQuestion[] bank = {
        new FarmQuestion("Tomato leaves have small brown spots. What should the farmer check first?", "Inspect plants and seek a diagnosis", "Assume every spot has the same cause", "Ignore all changes in the crop"),
        new FarmQuestion("Tomato leaves have target-like rings and yellow tissue. What disease is possible?", "Early blight", "Powdery mildew", "Root knot nematode"),
        new FarmQuestion("A white powdery coating appears on leaves. What disease is possible?", "Powdery mildew", "Rust", "Bacterial wilt"),
        new FarmQuestion("Orange powdery pustules appear on leaves. What disease is possible?", "Rust", "Powdery mildew", "Root rot"),
        new FarmQuestion("Tomato leaves have many small spots with dark edges. What disease is possible?", "Septoria leaf spot", "Powdery mildew", "Root rot"),
        new FarmQuestion("Tomato leaves show spots after soil splashes during rain. What may reduce splash?", "Use suitable mulch", "Splash more soil onto leaves", "Remove all mulch and ground cover"),
        new FarmQuestion("Leaf spot is spreading in a tomato crop. Which watering method keeps leaves drier?", "Drip irrigation near the roots", "Spray foliage throughout the night", "Keep leaves wet continuously"),
        new FarmQuestion("Plants with leaf disease are crowded. What can help leaves dry?", "Improve spacing and airflow", "Pack plants closer together", "Cover wet foliage overnight"),
        new FarmQuestion("Disease symptoms appear in a field. Why inspect crops regularly?", "To notice problems earlier", "To guarantee disease disappears", "To avoid ever checking again"),
        new FarmQuestion("A tomato crop has bacterial spot. Which plant parts can be affected?", "Leaves, stems and fruit", "Only roots", "Only seeds, never leaves"),
        new FarmQuestion("A farmer suspects leaf disease. What helps prevent spread on tools?", "Clean and sanitize tools", "Use dirty tools on every plant", "Store tools covered in diseased soil"),
        new FarmQuestion("A crop is affected by a suspected disease. Before spraying, what should be done?", "Confirm diagnosis and follow local label", "Apply any chemical without checking", "Double any pesticide dose"),
        new FarmQuestion("Older tomato leaves have dark spots with rings. Which disease is a possible cause?", "Early blight", "Powdery mildew", "Damping-off"),
        new FarmQuestion("White or gray powdery patches cover leaves. What kind of disease may be present?", "Powdery mildew", "Rust with orange pustules", "A broken irrigation pipe"),
        new FarmQuestion("A leaf has orange dust that rubs off. What should the farmer consider?", "Possible rust infection", "Certain drought diagnosis", "The plant cannot have a disease"),
        new FarmQuestion("Wet tomato foliage favors the spread of several leaf spots. What can help?", "Water at the base of the plant", "Keep foliage wet all night", "Water only onto the leaves"),
        new FarmQuestion("A field had a crop disease last season. Which planning practice may help?", "Rotate crop families when suitable", "Always repeat the same susceptible crop", "Ignore the field disease history"),
        new FarmQuestion("A farmer buys tomato transplants. Which choice helps prevent disease introduction?", "Choose healthy disease-free plants", "Choose seedlings with visible rot", "Choose seedlings covered in mold"),
        new FarmQuestion("A farmer cannot identify unusual spots on a crop. What is a useful next step?", "Ask a local plant clinic or extension", "Assume the cause from color alone", "Apply random chemicals immediately"),
        new FarmQuestion("Powdery mildew is more severe in shaded crowded growth. What may help?", "Improve light and air movement", "Crowd plants more closely", "Block all airflow around plants"),
        new FarmQuestion("Disease is present in one plot. Which work order can help limit spread?", "Work in healthy young plants first", "Carry infected debris through every plot", "Skip cleaning boots after diseased plots"),
        new FarmQuestion("Leaf spots occur on lower tomato leaves first. How may some pathogens reach them?", "Soil can splash onto lower leaves", "They appear only from broken branches", "They are always caused by insects"),
        new FarmQuestion("Tomato leaves have spots. Can one symptom alone always confirm the cause?", "No, different diseases may look similar", "Yes, all leaf spots are identical", "Yes, color alone proves the cause"),
        new FarmQuestion("A farmer sees symptoms of possible crop disease. What information helps diagnosis?", "Crop type, symptoms and field history", "Only the color of the tractor", "Only the day of the week")
    };
    static readonly List<int> unused = new List<int>();
    public static void ResetRun() { unused.Clear(); for(int i=0;i<bank.Length;i++) unused.Add(i); Shuffle(unused); }
    public static FarmQuestion Next() { if(unused.Count==0) ResetRun(); int i=unused[unused.Count-1]; unused.RemoveAt(unused.Count-1); return bank[i]; }
    public static void Shuffle<T>(IList<T> list) { for(int i=list.Count-1;i>0;i--){int j=Random.Range(0,i+1);T t=list[i];list[i]=list[j];list[j]=t;} }
}
