/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package genfusionscxml;

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
import scxmlgen.Modalities.Output;
import scxmlgen.Modalities.Speech;
import scxmlgen.Modalities.SecondMod;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws IOException {

    FusionGenerator fg = new FusionGenerator();
  
    
    /*fg.Sequence(Speech.SQUARE, SecondMod.RED, Output.SQUARE_RED);
    fg.Sequence(Speech.SQUARE, SecondMod.BLUE, Output.SQUARE_BLUE);
    fg.Sequence(Speech.SQUARE, SecondMod.YELLOW, Output.SQUARE_YELLOW);
    fg.Complementary(Speech.TRIANGLE, SecondMod.RED, Output.TRIANGLE_RED);
    fg.Complementary(Speech.TRIANGLE, SecondMod.BLUE, Output.TRIANGLE_BLUE);
    fg.Complementary(Speech.TRIANGLE, SecondMod.YELLOW, Output.TRIANGLE_YELLOW);
    fg.Complementary(Speech.CIRCLE, SecondMod.RED, Output.CIRCLE_RED);
    fg.Complementary(Speech.CIRCLE, SecondMod.BLUE, Output.CIRCLE_BLUE);
    fg.Complementary(Speech.CIRCLE, SecondMod.YELLOW, Output.CIRCLE_YELLOW);*/
    
    // KINNECT STATUS
    fg.Single(SecondMod.KINECT_ACTIVE, Output.KINECT_ACTIVE);
    fg.Single(SecondMod.KINECT_INACTIVE, Output.KINECT_INACTIVE);
    
    // MOUSE STATUS
    fg.Single(SecondMod.MOUSE_ACTIVE, Output.MOUSE_ACTIVE);
    fg.Single(SecondMod.MOUSE_ACTIVATING, Output.MOUSE_ACTIVATING);
    fg.Single(SecondMod.MOUSE_INACTIVE, Output.MOUSE_INACTIVE);
    
    // ASSISTANT STATUS
    fg.Single(Speech.ASSISTANT_ACTIVE, Output.ASSISTANT_ACTIVE);
    fg.Single(Speech.ASSISTANT_INACTIVE, Output.ASSISTANT_INACTIVE);
    
    // SLIDE CONTROL (CLAP)
    fg.Single(SecondMod.CLAP, Output.CLAP);
    
    // COMPLEMENTARY SLIDE CONTROL
    fg.Complementary(Speech.CHANGE, SecondMod.LEFT, Output.PREV_SLIDE);
    fg.Complementary(Speech.CHANGE, SecondMod.RIGHT, Output.NEXT_SLIDE);
    
    // REDUNDANT SLIDE CONTROL
    fg.Redundancy(Speech.PREV_SLIDE, SecondMod.PREV_SLIDE, Output.PREV_SLIDE);
    fg.Redundancy(Speech.NEXT_SLIDE, SecondMod.NEXT_SLIDE, Output.NEXT_SLIDE);
    
    // COMBO OUTPUT
    fg.Single(SecondMod.COMBO_LEFT, Output.COMBO_LEFT);
    fg.Single(SecondMod.COMBO_RIGHT, Output.COMBO_RIGHT);
    fg.Single(Speech.COMBO_CHANGE, Output.COMBO_CHANGE);
    
    fg.Build("fusion.scxml");
        
        
    }
    
}
