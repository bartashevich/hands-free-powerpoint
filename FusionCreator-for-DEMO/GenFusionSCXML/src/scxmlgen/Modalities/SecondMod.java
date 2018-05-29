package scxmlgen.Modalities;

import scxmlgen.interfaces.IModality;

/**
 *
 * @author nunof
 */
public enum SecondMod implements IModality{

    RED("[color][RED]",1500),
    BLUE("[color][BLUE]",1500),
    YELLOW("[color][YELLOW]",1500),
    
    // STATUS
    KINECT_ACTIVE("[status][KINECT_ACTIVE]",1500),
    KINECT_INACTIVE("[status][KNNECT_INACTIVE]",1500),
    MOUSE_ACTIVE("[status][MOUSE_ACTIVE]",1500),
    MOUSE_ACTIVATING("[status][MOUSE_ACTIVATING]",1500),
    MOUSE_INACTIVE("[status][MOUSE_INACTIVE]",1500),
    
    // SLIDE CONTROL
    RIGHT("[action][RIGHT]",1500),
    LEFT("[action][LEFT]",1500),
    NEXT_SLIDE("[action][NEXT_SLIDE]",1500),
    PREV_SLIDE("[action][PREV_SLIDE]",1500),
    CLAP("[action][CLAP]",1500),
    COMBO_LEFT("[combo][LEFT]",0),
    COMBO_RIGHT("[combo][RIGHT]",0),
    
    ;
    
    private String event;
    private int timeout;


    SecondMod(String m, int time) {
        event=m;
        timeout=time;
    }

    @Override
    public int getTimeOut() {
        return timeout;
    }

    @Override
    public String getEventName() {
        //return getModalityName()+"."+event;
        return event;
    }

    @Override
    public String getEvName() {
        return getModalityName().toLowerCase()+event.toLowerCase();
    }
    
}
