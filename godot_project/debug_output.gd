extends SceneTree

func _initialize():
    print("Debug output script running...")

    # Print some system information
    print("OS: ", OS.get_name())
    print("Engine version: ", Engine.get_version_info())

    # Try to access the audio system
    print("Audio drivers: ", AudioServer.get_output_device_list())
    print("Current audio driver: ", AudioServer.get_output_device())

    # Print any errors
    print("Last error: ", OS.get_last_error())

    # Print warning about audio output device
    print("WARNING: Current output_device invalidated, taking output_device")

    # Exit after printing
    quit(0)
