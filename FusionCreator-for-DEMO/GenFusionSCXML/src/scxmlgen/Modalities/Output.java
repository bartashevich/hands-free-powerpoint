package scxmlgen.Modalities;

import scxmlgen.interfaces.IOutput;



public enum Output implements IOutput{
    
    /*SQUARE_RED("[shape][SQUARE][color][RED]"),
    SQUARE_BLUE("[shape][SQUARE][color][BLUE]"),
    SQUARE_YELLOW("[shape][SQUARE][color][YELLOW]"),
    TRIANGLE_RED("[shape][TRIANGLE][color][RED]"),
    TRIANGLE_BLUE("[shape][TRIANGLE][color][BLUE]"),
    TRIANGLE_YELLOW("[shape][TRIANGLE][color][YELLOW]"),
    CIRCLE_RED("[shape][CIRCLE][color][RED]"),
    CIRCLE_BLUE("[shape][CIRCLE][color][BLUE]"),
    CIRCLE_YELLOW("[shape][CIRCLE][color][YELLOW]"),
    CIRCLE("[shape][CIRCLE]"),*/
    
    // STATUS
    KINECT_ACTIVE("[status][KINECT_ACTIVE]"),
    KINECT_INACTIVE("[status][KINECT_INACTIVE]"),
    ASSISTANT_ACTIVE("[status][ASSISTANT_ACTIVE]"),
    ASSISTANT_INACTIVE("[status][ASSISTANT_INACTIVE]"),
    MOUSE_ACTIVE("[status][MOUSE_ACTIVE]"),
    MOUSE_ACTIVATING("[status][MOUSE_ACTIVATING]"),
    MOUSE_INACTIVE("[status][MOUSE_INACTIVE]"),
    VOLUME_ACTIVE("[status][VOLUME_ACTIVE]"),
    VOLUME_ACTIVATING("[status][VOLUME_ACTIVATING]"),
    VOLUME_INACTIVE("[status][VOLUME_INACTIVE]"),
        
    // SLIDE CONTROL NEXT/PREVIOUS
    NEXT_SLIDE("[action][NEXT_SLIDE]"),
    PREV_SLIDE("[action][PREV_SLIDE]"),
    
    // COMBO OUTPUT
    COMBO_LEFT("[combo][LEFT]"),
    COMBO_RIGHT("[combo][RIGHT]"),
    COMBO_CHANGE("[combo][CHANGE]"),
    
    SUSPEND("[action][SUSPEND]"),
    CALCULATOR("[action][CALCULATOR]"),
    READ_SLIDE("[action][READ_SLIDE]"),
    READ_NEXT("[action][READ_NEXT]"),
    OPEN_HELP("[action][OPEN_HELP]"),
    CLOSE_HELP("[action][CLOSE_HELP]"),
    
    
    ;
    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}
