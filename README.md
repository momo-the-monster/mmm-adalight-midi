# Adalight Unity

This project demonstrates how to control LEDs within Unity, by sending serial commands to an Arduino attached via USB.

It requires an Adalight setup, such as [this "DreamBox" setup](https://www.amazon.com/dp/B07PXS89CD). For best performance, the DreamBox controller should be flashed with the [Adalight-FastLED](https://github.com/dmadison/Adalight-FastLED) firmware by using the Arduino software.

# Setup

1.  Plug the Controller into your computer using the A-to-B USB Cable
2.  Open your Device Manager and look under Ports (COM & LPT). You should find USB-SERIAL CH340 (COMX)
    a.  If you don't see it listed, install [CH341SER](https://drive.google.com/open?id=1N-tTcoHp5GT9rvuRcb-8hhb33fN_MdQs) and unplug/replug.
    b.  Take note of the COM port listed here
3.  Plug the power adapter into 110V Power
4.  The LED strip has two barrel connectors. Find the one marked 'Data' and plug it into the Controller.
5.  Plug the other barrel into the power source.

Connect the DreamBox to your computer over USB, upload the Adalight-FastLED firmware if needed.

# Run
1. Open the Unity Project
2. Open the Example.unity scene in Assets/_Project/
3. Open the inspector for the *LightControl* GameObject
4. Change the Port string to match the port you found in Setup step 2b, including the 'COM' part like 'COM8'
5. Run the scene
6. Press each button to see it in action! The colored buttons should light up the strip in full, and the chase will clear the strip as it sends a single color through each light in a 'chase'.

You can use the LEDSerialController class directly in your own project, compose it inside another class, or use it as an example to write your own controller. 

Have fun!