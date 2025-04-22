#!/bin/bash
# Mac wrapper for the unified game runner script

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Call the common script with all arguments
"$DIR/../common/run/game.sh" "$@"
