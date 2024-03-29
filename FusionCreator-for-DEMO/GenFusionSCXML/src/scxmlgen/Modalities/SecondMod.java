package scxmlgen.Modalities;

import scxmlgen.interfaces.IModality;

/**
 *
 * @author nunof
 */
public enum SecondMod implements IModality{

    /*RED("[color][RED]",1500),
    BLUE("[color][BLUE]",1500),
    YELLOW("[color][YELLOW]",1500),*/
    
    // STATUS
    KINECT_ACTIVE("[status][KINECT_ACTIVE]",0),
    KINECT_INACTIVE("[status][KNNECT_INACTIVE]",0),
    MOUSE_ACTIVE("[status][MOUSE_ACTIVE]",0),
    MOUSE_ACTIVATING("[status][MOUSE_ACTIVATING]",0),
    MOUSE_INACTIVE("[status][MOUSE_INACTIVE]",0),
    VOLUME_ACTIVE("[status][VOLUME_ACTIVE]",0),
    VOLUME_ACTIVATING("[status][VOLUME_ACTIVATING]",0),
    VOLUME_INACTIVE("[status][VOLUME_INACTIVE]",0),
    
    // SLIDE CONTROL
    RIGHT("[action][RIGHT]",1500),
    LEFT("[action][LEFT]",1500),
    NEXT_SLIDE("[action][NEXT_SLIDE]",1000),
    PREV_SLIDE("[action][PREV_SLIDE]",1000),
    OPEN_HELP("[action][OPEN_HELP]",1000),
    CLOSE_HELP("[action][CLOSE_HELP]",1000),
    PEN("[action][PEN]",0),
    
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
