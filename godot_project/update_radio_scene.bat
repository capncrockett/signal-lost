@echo off
echo Backing up original RadioTuner.tscn...
copy scenes\radio\RadioTuner.tscn scenes\radio\RadioTuner.tscn.bak
echo Replacing with new RadioTuner.tscn...
copy scenes\radio\RadioTuner.tscn.new scenes\radio\RadioTuner.tscn
echo Done! The radio UI has been updated.
echo You can now run the game to see the changes.
