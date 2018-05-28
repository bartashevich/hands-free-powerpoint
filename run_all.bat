cd gestureModality\bin\x86\Debug
start DiscreteGestureBasics-WPF.exe
cd ..\..\..\..
cd AppGui\AppGui\bin\Debug\
start AppGui.exe
cd ..\..\..\..
cd IM
java -jar mmiframeworkV2.jar

Taskkill /IM AppGui.exe
Taskkill /IM DiscreteGestureBasics-WPF.exe