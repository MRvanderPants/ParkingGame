using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPM {
    
    public static float BpmToTime(int bpm) {
        return 60f / bpm;
    }

    public static int GetBarLength(TimeSignatures timeSignature) {
        switch (timeSignature) {
            case TimeSignatures.Four_Four:
                return 4;

            case TimeSignatures.Three_Four:
                return 3;

            default:
                return -1;
        }
    }
}
