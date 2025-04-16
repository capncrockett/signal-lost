@echo off
echo Backing up original RadioTuner.tscn...
copy scenes\radio\RadioTuner.tscn scenes\radio\RadioTuner.tscn.bak2
echo Replacing with fixed RadioTuner.tscn...
copy scenes\radio\RadioTuner.tscn.fixed scenes\radio\RadioTuner.tscn
echo Done! The radio UI has been updated with proper non-overlapping elements.
echo You can now run the game to see the changes.
