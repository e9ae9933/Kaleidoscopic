using HarmonyLib;
using nel.title;
using XX;

namespace Kaleidoscopic;

public class GeneralPatches {
    
    [HarmonyPatch(typeof(SceneTitleTemp), "fineTexts")]
    [HarmonyPostfix]
    public static void Postfix(TextRenderer ___TxCp) {
        ___TxCp.text_content += "\nwith Kaleidoscopic present, e9ae9933, aliceincradle.org";
    }
}